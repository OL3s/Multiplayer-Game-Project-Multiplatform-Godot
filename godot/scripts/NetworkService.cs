using Godot;
using Shared;
using System.Linq;
using System.Collections.Generic;

public partial class NetworkService : Node
{
	public Dictionary<int, PlayerData> Players = new();

	public override void _Ready()
	{
		string mode;

		if (IsServerMode())
		{
			StartServer();
			mode = "server";
		}
		else
		{
			StartClient();
			mode = "client";
		}

		GD.Print("NetworkService ready as " + mode);
	}

	public bool IsServerMode() =>
		OS.GetCmdlineArgs().Contains("--server");

	private void StartServer()
	{
		var peer = new WebSocketMultiplayerPeer();
		peer.CreateServer(7777);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Server started");
	}

	private void StartClient()
	{
		var peer = new WebSocketMultiplayerPeer();
		peer.CreateClient("ws://127.0.0.1:7777");
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Client started");
	}
}
