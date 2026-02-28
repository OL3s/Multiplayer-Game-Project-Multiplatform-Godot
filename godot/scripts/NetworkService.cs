using Godot;
using Shared;
using System.Linq;
using System.Collections.Generic;

public partial class NetworkService : Node
{
	public Dictionary<int, PlayerData> Players = new();

	public override void _Ready()
	{
		GD.Print(OS.GetCmdlineArgs());
		CallDeferred(nameof(Boot)); // run after tree is done building
	}

	private void Boot()
	{
		if (IsServerMode()) StartServer();
		else 				StartClient();
		GD.Print("NetworkService ready as " + (IsServerMode() ? "server" : "client"));
	}

	public bool IsServerMode() =>
		OS.GetCmdlineArgs().Contains("--server");

	private void StartServer()
	{
		var peer = new WebSocketMultiplayerPeer();
		peer.CreateServer(7777);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Server started");

		CallDeferred(nameof(GoLobby)); // safe scene switch
	}

	private void GoLobby()
	{
		GetTree().ChangeSceneToFile("res://scenes/lobby/lobby.tscn");
	}

	private void StartClient()
	{
		var peer = new WebSocketMultiplayerPeer();
		peer.CreateClient("ws://127.0.0.1:7777");
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Client started");
	}
}
