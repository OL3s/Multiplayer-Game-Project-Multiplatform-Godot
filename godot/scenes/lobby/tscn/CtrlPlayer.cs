using Godot;
using System;

public partial class CtrlPlayer : Control {
	
	public string PlayerName { get; set; } = "?";
	public bool IsReady { get; set; } = false;
	[Export] public TextureRect ReadyTexture;
	[Export] public TextureRect AvatarTexture;
	[Export] public Label NameLabel;
	[Export] public Panel BorderPanel;
	
	public override void _Ready() {
		
	}
	
	public void SetReady(bool isReady)
	{
		IsReady = isReady;
		ReadyTexture.Texture = IsReady
			? GD.Load<Texture2D>("res://sprites/icons/sp-ready-true.png")
			: GD.Load<Texture2D>("res://sprites/icons/sp-ready-false.png");
	}
}
