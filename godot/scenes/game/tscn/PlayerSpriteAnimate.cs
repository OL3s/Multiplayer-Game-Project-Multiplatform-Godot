using Godot;
using System;
using AnimationService;

public partial class PlayerSpriteAnimate : Sprite2D
{
	private double _time;
	private Vector2 _baseLocalPos;

	private bool IsMoving => GetParent<Player>().Velocity.Length() > 0.1f;
	[Export] public float MoveSwaySpeed = 10f;
	[Export] public float MoveSwayAngle = 5f;
	[Export] public float MoveBounceSpeed = 10f;
	[Export] public float MoveBounceAmplitude = 2f;

	public override void _Ready()
	{
		GD.Print("PlayerSpriteAnimate ready.");
		_time = 0;
		_baseLocalPos = Position; // local offset in parent space
	}

	public override void _Process(double delta)
	{
		_time = IsMoving ? _time + delta : 0;

		if (IsMoving)
		{
			RotationDegrees = PositionModifiers.Sway(_time, MoveSwaySpeed, MoveSwayAngle);
			Position = _baseLocalPos - new Vector2(0, PositionModifiers.Bounce(_time, MoveBounceSpeed, MoveBounceAmplitude));
		}
		else
		{
			RotationDegrees = 0;
			Position = _baseLocalPos;
		}
	}
}
