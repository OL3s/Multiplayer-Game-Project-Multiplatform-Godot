using Godot;
using System;
using AnimationService;

public partial class PlayerSpriteAnimate : Sprite2D
{
	private double _time;
	private Vector2 _baseLocalPos;
	private NetworkService _networkService;
	private bool IsMoving => GetParent<Player>().Velocity.Length() > 0.1f;
	public SpriteTarget SpriteTarget = new SpriteTarget(0, 0, 0, 1, 1);
	[Export] public float MoveSwaySpeed = 10f;
	[Export] public float MoveSwayAngle = 7f;
	[Export] public float MoveBounceSpeed = 10f;
	[Export] public float MoveBounceAmplitude = 2f;

	public override void _Ready()
	{
		GD.Print("PlayerSpriteAnimate ready.");
		_time = 0;
		_baseLocalPos = Position; // local offset in parent space
		_networkService = GetNodeOrNull<NetworkService>("/root/NetworkService");
		if (_networkService != null && _networkService.IsServer)
		{
			SetProcess(false);
		}
	}

	public override void _Process(double delta)
	{
		if (_networkService != null && _networkService.IsServer) return;
		// --- Moving Animations ---
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
		
		// --- Set lerp towards target ---
		float followSpeed = 12f; // bigger = snappier
		float t = 1f - Mathf.Exp(-followSpeed * (float)delta);

		RotationDegrees = Mathf.LerpAngle(RotationDegrees, SpriteTarget.Rotation, t);
		Position = Position.Lerp(new Vector2(SpriteTarget.X, SpriteTarget.Y), t);
		Scale = Scale.Lerp(new Vector2(SpriteTarget.ScaleX, SpriteTarget.ScaleY), t);

		// --- Set target ---
		if (GetParent<Player>().Velocity.X > 0.1f)
			SpriteTarget.ScaleX = 1;
		else if (GetParent<Player>().Velocity.X < -0.1f)
			SpriteTarget.ScaleX = -1;
	}
}
