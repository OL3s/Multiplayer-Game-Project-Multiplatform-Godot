using Godot;
using System;

public partial class NetworkIcon : TextureRect
{
	private NetworkIconType _currentType = (NetworkIconType)(-1);

	private Texture2D _texClient;
	private Texture2D _texServer;
	private Texture2D _texUnknown;

	public override void _Ready()
	{
		_texClient = GD.Load<Texture2D>("res://sprites/icons/sp-network-client.png");
		_texServer = GD.Load<Texture2D>("res://sprites/icons/sp-network-server.png");
		_texUnknown = GD.Load<Texture2D>("res://sprites/icons/sp-network-unknown.png");
		UpdateIcon();
	}

	private NetworkIconType DetermineType()
	{
		var peer = Multiplayer.MultiplayerPeer;
		if (peer == null)
			return NetworkIconType.NotFound;

		if (peer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected)
			return NetworkIconType.NotFound;

		return Multiplayer.IsServer() ? NetworkIconType.Server : NetworkIconType.Client;
	}

	public void UpdateIcon()
	{
		Texture = DetermineType() switch
		{
			NetworkIconType.Client => _texClient,
			NetworkIconType.Server => _texServer,
			_ => _texUnknown
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
