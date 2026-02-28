using Godot;
using System;

public partial class ButtonLogin : Button
{
	[Export] public string TargetScene = "res://scenes/menu/menu.tscn";

	public override void _Ready()
	{
		Pressed += OnPressed;
	}

	private void OnPressed()
	{
		GetTree().ChangeSceneToFile(TargetScene);
	}
}
