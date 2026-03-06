using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 250f;
	private InputService _input;

	public override void _Ready()
	{
		_input = GetNode<InputService>("/root/InputService");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 movement = _input.CurrentInputState.MovementVector;
		Velocity = movement * Speed;
		MoveAndSlide(); // TODO - NetworkService.IsServer ? MoveAndSlide() : MoveAndSlideWithSnap() for client-side prediction and server reconciliation
	}

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Space))
		{
			EntityFactory.Instance?.SpawnBullet(new Vector2(1, 0), parent: this, speed: 1500f, maxDistance: 2000f);
		}
	}

}
