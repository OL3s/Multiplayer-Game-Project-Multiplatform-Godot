using Godot;
using System.Linq;
using System.Collections.Generic;
using Shared;

using GArray = Godot.Collections.Array;
using GDict  = Godot.Collections.Dictionary;

public partial class NetworkService : Node
{
	private const string InitialScenePath = "res://scenes/lobby/lobby.tscn";

	[Export] public int Port = 7777;
	[Export] public string DefaultHost = "127.0.0.1";

	// Session/peer state (scene-agnostic)
	public Dictionary<long, PlayerData> Peers = new();

	[Signal] public delegate void PeersUpdatedEventHandler();

	public override void _Ready()
	{
		// keep debug hook
		PeersUpdated += () => GD.Print("Peers updated. Total peers: " + Peers.Count);

		CallDeferred(nameof(Boot));
	}

	private void Boot()
	{
		if (IsServerMode())
		{
			StartServer();
			GetTree().ChangeSceneToFile(InitialScenePath);
		}
		else
		{
			GD.Print("Client idle. Waiting for JoinGame().");
		}
	}

	public bool IsServerMode() => OS.GetCmdlineArgs().Contains("--server");

	// ---------- SERVER START ----------
	private void StartServer()
	{
		var peer = new ENetMultiplayerPeer();
		var err = peer.CreateServer(Port, maxClients: 8);
		if (err != Error.Ok) { GD.PrintErr($"Server start failed: {err}"); return; }

		Multiplayer.MultiplayerPeer = peer;

		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;

		GD.Print($"Server started on {Port}");
	}

	private void OnPeerConnected(long id)
	{
		if (!Multiplayer.IsServer()) return;

		Peers[id] = new PlayerData(); // default data
		EmitSignal(SignalName.PeersUpdated);

		GD.Print($"Peer connected: {id}");

		// Tell ONLY that peer what scene to load (initial join scene)
		RpcId(id, nameof(RpcLoadScene), InitialScenePath);

		// Broadcast peer snapshot to already-loaded clients
		Rpc(nameof(RpcPeersSnapshot), BuildPeersSnapshot());
	}

	private void OnPeerDisconnected(long id)
	{
		if (!Multiplayer.IsServer()) return;

		Peers.Remove(id);
		EmitSignal(SignalName.PeersUpdated);

		GD.Print($"Peer disconnected: {id}");

		// Broadcast updated peer snapshot
		Rpc(nameof(RpcPeersSnapshot), BuildPeersSnapshot());
	}

	// ---------- CLIENT JOIN ----------
	public void JoinGame(string host = "")
	{
		if (IsServerMode()) return;

		var h = string.IsNullOrWhiteSpace(host) ? DefaultHost : host;

		var peer = new ENetMultiplayerPeer();
		var err = peer.CreateClient(h, Port);
		if (err != Error.Ok) { GD.PrintErr($"Client connect failed: {err}"); return; }

		Multiplayer.MultiplayerPeer = peer;

		Multiplayer.ConnectedToServer += () => GD.Print("Connected. Waiting for server scene...");
		Multiplayer.ConnectionFailed += () => GD.PrintErr("Connection failed");
		Multiplayer.ServerDisconnected += () => GD.Print("Server disconnected");

		GD.Print($"Client connecting to {h}:{Port}");
	}

	// ---------- SCENE SYNC ----------
	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void RpcLoadScene(string scenePath)
	{
		GetTree().ChangeSceneToFile(scenePath);

		// avoid race: send ready next frame
		CallDeferred(nameof(SendClientReady));
	}

	private void SendClientReady()
	{
		// ENet server is peer id 1
		RpcId(1, nameof(RpcClientReady));
	}

	// client -> server
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcClientReady()
	{
		if (!Multiplayer.IsServer()) return;

		var id = Multiplayer.GetRemoteSenderId();
		GD.Print($"Client ready: {id}");

		// Send snapshot ONLY to that client
		RpcId(id, nameof(RpcPeersSnapshot), BuildPeersSnapshot());
	}

	// ---------- PEER STATE SYNC ----------
	private GArray BuildPeersSnapshot()
	{
		var arr = new GArray();

		foreach (var kv in Peers)
		{
			arr.Add(new GDict {
				{ "id", (int)kv.Key },
				{ "name", kv.Value.Name },
				{ "ready", kv.Value.Ready },
			});
		}

		return arr;
	}

	// server -> clients
	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void RpcPeersSnapshot(GArray snapshot)
	{
		Peers.Clear();

		foreach (GDict d in snapshot)
		{
			long id = (int)d["id"];

			Peers[id] = new PlayerData
			{
				Name = d.ContainsKey("name") ? (string)d["name"] : "",
				Ready = d.ContainsKey("ready") && (bool)d["ready"],
			};
		}

		EmitSignal(SignalName.PeersUpdated);
	}

	// ---------- TYPES ----------
	public struct MatchSettings
	{
		public ulong MapSeed;
		public int MapSize;

		public MatchSettings(ulong mapSeed, int mapSize)
		{
			MapSeed = mapSeed;
			MapSize = mapSize;
		}
		public MatchSettings() : this(((ulong)GD.Randi() << 32) | GD.Randi(), 4) { }
		public override string ToString() => $"Seed: {MapSeed}, Size: {MapSize}";
	}

}
