using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 250f;
	private InputService _input;

	public override void _Ready()
	{
		GD.Print("Player ready.");
		_input = GetNode<InputService>("/root/InputService");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 movement = _input.CurrentInputState.MovementVector;
		Velocity = movement * Speed;
		MoveAndSlide(); // TODO - NetworkService.IsServer ? MoveAndSlide() : MoveAndSlideWithSnap() for client-side prediction and server reconciliation
	}

}
