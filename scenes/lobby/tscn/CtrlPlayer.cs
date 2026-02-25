using Godot;
using System;

public partial class CtrlPlayer : Control
{
	public TextureRect ImgReady;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ImgReady = GetNode<TextureRect>("Img-Ready");
		ImgReady.Visible = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
