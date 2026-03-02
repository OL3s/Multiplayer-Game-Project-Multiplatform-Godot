using Godot;
using System;
using Shared;

public partial class CtrlPlayer : Control {
	
	public PlayerData PlayerData { get; set; } = new("Fetching...");
	[Export] public TextureRect ReadyTexture;
	[Export] public TextureRect AvatarTexture;
	[Export] public Label NameLabel;
	[Export] public Panel BorderPanel;
	
	public override void _Ready() {
		
	}
	
	public void SetReady(bool ready)
	{
		PlayerData.Ready = ready;
		ReadyTexture.Texture = PlayerData.Ready
			? GD.Load<Texture2D>("res://sprites/icons/sp-ready-true.png")
			: GD.Load<Texture2D>("res://sprites/icons/sp-ready-false.png");
	}
}
