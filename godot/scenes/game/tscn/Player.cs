using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float DefaultSpeed = 250f;
	private InputService _input;

	public override void _Ready()
	{
		_input = GetNode<InputService>("/root/InputService");
	}

	public override void _PhysicsProcess(double delta)
	{
		var _currentInput = _input.CurrentInputState;
		
		// --- Speed multiplyers ---
		float speedMultiplyer = 1;
		speedMultiplyer *= (_currentInput.IsAiming ? .5f : 1f);
		// -------------------------
		
		Vector2 movement = _currentInput.MovementVector;
		Velocity = movement * DefaultSpeed * speedMultiplyer;
		MoveAndSlide(); // TODO - NetworkService.IsServer ? MoveAndSlide() : MoveAndSlideWithSnap() for client-side prediction and server reconciliation
	}

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Space))
		{
			EntityFactory.Instance?.SpawnBullet(Velocity.Normalized(), shooter: this, speed: 1500f, maxDistance: 2000f);
		}
	}

}
