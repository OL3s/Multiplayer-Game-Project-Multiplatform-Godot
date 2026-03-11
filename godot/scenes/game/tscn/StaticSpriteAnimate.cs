using Godot;
using AnimationService;

public partial class StaticSpriteAnimate : Sprite2D
{
	public SpriteTarget SpriteTarget;

	[Export] public float FollowSpeed = 8f;

	public override void _Ready()
	{
		SpriteTarget = new SpriteTarget(
			Position.X,
			Position.Y,
			RotationDegrees,
			Scale.X,
			Scale.Y
		);
	}

	public override void _Process(double delta)
	{
		float t = 1f - Mathf.Exp(-FollowSpeed * (float)delta);

		RotationDegrees = Mathf.LerpAngle(RotationDegrees, SpriteTarget.Rotation, t);
		Position = Position.Lerp(new Vector2(SpriteTarget.X, SpriteTarget.Y), t);
		Scale = Scale.Lerp(new Vector2(SpriteTarget.ScaleX, SpriteTarget.ScaleY), t);
	}
}
