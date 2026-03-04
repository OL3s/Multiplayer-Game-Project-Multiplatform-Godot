using Godot;
using System.Linq;
using System.Collections.Generic;
using Shared;

using GArray = Godot.Collections.Array;
using GDict  = Godot.Collections.Dictionary;

public partial class NetworkService : Node
{
	private const string InitialScenePath = "res://scenes/lobby/lobby.tscn";
	public bool IsServer;
	[Export] public int Port = 7777;
	[Export] public string DefaultHost = "127.0.0.1";
	[Export] public string DefaultPlayerName = "Player";

	// Session/peer state (scene-agnostic)
	public Dictionary<long, PlayerData> Peers = new();
	public MatchConfig CurrentMatchConfig = new();
	[Signal] public delegate void PeersUpdatedEventHandler();

	public override void _Ready()
	{
<<<<<<< Updated upstream
=======
		PrintHelp();
		GD.Print("NetworkService ready.");
		IsServer = IsServerMode();
>>>>>>> Stashed changes
		// keep debug hook
		PeersUpdated += () => GD.Print("Peers updated. Total peers: " + Peers.Count);
		CallDeferred(nameof(Boot));
	}

	private void Boot()
	{
		if (IsServer)
		{
			StartServer();
			GetTree().ChangeSceneToFile(InitialScenePath);
		}
		else
		{
			GD.Print("Client idle. Waiting for JoinGame().");
		}
	}

	// ---------- OS ARGUMENTS ------------
	public bool IsServerMode() => OS.GetCmdlineArgs().Contains("--server");
	
	private void ApplyCmdArgs()
	{
		var args = OS.GetCmdlineArgs();

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "--port" && i + 1 < args.Length)
			{
				if (int.TryParse(args[i + 1], out var p))
					Port = p;
			}
		}
	}
	
	private void PrintHelp()
	{
		GD.Print(
			@"Arguments:
			--server          Run as server
			--port <number>   Override port (default 7777)"
		);
	}

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

		// Default server-side data until the client sends its local data (name/etc.)
		Peers[id] = new PlayerData("Noname");
		EmitSignal(SignalName.PeersUpdated);

		GD.Print($"Peer connected: {id}");

		// Tell ONLY that peer what scene to load (initial join scene)
		RpcId(id, nameof(RpcLoadScene), InitialScenePath);

		// Broadcast current snapshot (client will update its entry right after load)
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
		if (IsServer) return;

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

		// avoid race: send next frame (so scene tree is ready)
		CallDeferred(nameof(SendClientReadyAndLocalData));
	}

	private void SendClientReadyAndLocalData()
	{
		// ENet server is peer id 1
		RpcId(1, nameof(RpcClientReady));

		// Send local player data (at least name) so server updates Peers[id]
		var local = LoadLocalPlayerData();
		RpcId(1, nameof(RpcClientLocalData), PlayerDataToDict(local));
	}

	private PlayerData LoadLocalPlayerData()
	{
		// TODO: load from user:// config / save file later
		// For now: only name is "local", rest defaults.
		var name = (DefaultPlayerName ?? "").Trim();
		if (name.Length == 0) name = "Noname";
		if (name.Length > 24) name = name[..24];

		return new PlayerData(name)
		{
			Ready = false,
			TeamId = 0,
			Credits = 0,
			Score = 0,
			Kills = 0,
		};
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

	// client -> server (local machine data like name, cosmetics, etc.)
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RpcClientLocalData(GDict payload)
	{
		if (!Multiplayer.IsServer()) return;

		var id = Multiplayer.GetRemoteSenderId();

		if (!Peers.ContainsKey(id))
			Peers[id] = new PlayerData("Noname");

		var incoming = DictToPlayerData(payload);

		// sanitize name
		incoming.Name = (incoming.Name ?? "").Trim();
		if (incoming.Name.Length == 0) incoming.Name = "Noname";
		if (incoming.Name.Length > 24) incoming.Name = incoming.Name[..24];

		// Update server state (classes = reference semantics; assignment is still fine)
		var pd = Peers[id];
		pd.Name = incoming.Name;
		pd.TeamId = incoming.TeamId;
		pd.Credits = incoming.Credits;
		pd.Score = incoming.Score;
		pd.Kills = incoming.Kills;
		// NOTE: you can decide if client is allowed to set Ready; usually server-controlled.
		// pd.Ready = incoming.Ready;

		EmitSignal(SignalName.PeersUpdated);

		// Broadcast updated snapshot so everyone sees the new name immediately
		Rpc(nameof(RpcPeersSnapshot), BuildPeersSnapshot());
	}

	// ---------- PEER STATE SYNC ----------
	private GArray BuildPeersSnapshot()
	{
		var arr = new GArray();

		foreach (var kv in Peers)
		{
			var id = kv.Key;
			var p = kv.Value;

			arr.Add(new GDict {
				{ "id", (int)id },
				{ "name", p.Name ?? "" },
				{ "ready", p.Ready },
				{ "teamId", p.TeamId },
				{ "credits", p.Credits },
				{ "score", p.Score },
				{ "kills", p.Kills },
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

			Peers[id] = new PlayerData(d.ContainsKey("name") ? (string)d["name"] : "")
			{
				Ready = d.ContainsKey("ready") && (bool)d["ready"],
				TeamId = d.ContainsKey("teamId") ? (int)d["teamId"] : 0,
				Credits = d.ContainsKey("credits") ? (int)d["credits"] : 0,
				Score = d.ContainsKey("score") ? (int)d["score"] : 0,
				Kills = d.ContainsKey("kills") ? (int)d["kills"] : 0,
			};
		}

		EmitSignal(SignalName.PeersUpdated);
	}

	// ---------- helpers ----------
	private static GDict PlayerDataToDict(PlayerData p)
	{
		return new GDict
		{
			{ "ready", p.Ready },
			{ "name", p.Name ?? "" },
			{ "teamId", p.TeamId },
			{ "credits", p.Credits },
			{ "score", p.Score },
			{ "kills", p.Kills },
		};
	}

	private static PlayerData DictToPlayerData(GDict d)
	{
		var name = d.ContainsKey("name") ? (string)d["name"] : "";

		return new PlayerData(name)
		{
			Ready = d.ContainsKey("ready") && (bool)d["ready"],
			TeamId = d.ContainsKey("teamId") ? (int)d["teamId"] : 0,
			Credits = d.ContainsKey("credits") ? (int)d["credits"] : 0,
			Score = d.ContainsKey("score") ? (int)d["score"] : 0,
			Kills = d.ContainsKey("kills") ? (int)d["kills"] : 0,
		};
	}
}
