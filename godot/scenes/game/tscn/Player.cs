using Godot;
using System;
using Combat;

public partial class Player : CharacterBody2D
{
	[Export] public float DefaultSpeed = 250f;
	[Export] public int TeamId = 0;
	[Export] CombatNode CombatNode;
	private InputService _input;
	public bool IsShooting = false;

	public override void _Ready()
	{
		_input = GetNode<InputService>("/root/InputService");
		if (CombatNode == null)
			throw new Exception("Player requires a reference to its CombatNode for damage handling. Please set the CombatNode property in the inspector.");
		CombatNode.Container.TeamId = TeamId;
	}

	public override void _PhysicsProcess(double delta)
	{
		var _currentInput = _input.CurrentInputState;	
		
		// --- Speed multiplyers ---
		float speedMultiplyer = 1;
		speedMultiplyer *= _currentInput.IsAiming ? .5f : 1f;
		// -------------------------
		
		Vector2 movement = _currentInput.MovementVector;
		Velocity = movement * DefaultSpeed * speedMultiplyer;
		MoveAndSlide(); // TODO - NetworkService.IsServer ? MoveAndSlide() : MoveAndSlideWithSnap() for client-side prediction and server reconciliation
	}

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Space) && !IsShooting)
		{
			IsShooting = true;
			EntityFactory.Instance?.SpawnBullet(Velocity.Normalized().IsZeroApprox() ? Vector2.Right : Velocity.Normalized(), enableFriendlyFire: false, shooter: this, teamId: TeamId, position: GlobalPosition);
		} else if (!Input.IsKeyPressed(Key.Space) && IsShooting)
		{
			IsShooting = false;
		}
	}

}
