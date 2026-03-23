using Godot;
using System;

public partial class NetworkIcon : TextureRect
{
	private NetworkIconType _currentType = (NetworkIconType)(-1);

	[Export] public Texture2D texClient;
	[Export] public Texture2D texServer;
	[Export] public Texture2D texUnknown;

	public override void _Ready()
	{
		texClient = GD.Load<Texture2D>("res://sprites/icons/sp-network-client.png");
		texServer = GD.Load<Texture2D>("res://sprites/icons/sp-network-server.png");
		texUnknown = GD.Load<Texture2D>("res://sprites/icons/sp-network-unknown.png");
		UpdateIcon();
	}

	private NetworkIconType DetermineType()
	{

		// Check if NotFound first (no peer or offline)
		var peer = Multiplayer.MultiplayerPeer;
		if (peer == null)
			return NetworkIconType.NotFound;

		if (peer is OfflineMultiplayerPeer)
			return NetworkIconType.NotFound;

		if (peer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected)
			return NetworkIconType.NotFound;

		// If connected, determine if server or client
		return Multiplayer.IsServer() ? NetworkIconType.Server : NetworkIconType.Client;
	}

	public void UpdateIcon()
	{
		Texture = DetermineType() switch
		{
			NetworkIconType.Client => texClient,
			NetworkIconType.Server => texServer,
			_ => texUnknown
		};
	}

	public enum NetworkIconType
	{
		NotFound,
		Client,
		ClientDisconnected,
		Server
	}
}
