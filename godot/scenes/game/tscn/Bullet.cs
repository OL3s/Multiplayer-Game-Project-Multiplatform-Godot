using Godot;
using System;

#nullable enable

public partial class Bullet : Node2D
{
	[Export] public float Speed = 1200f;
	[Export] public float MaxDistance = 1400f;
	[Export] public float LineWidth = 8.0f;
	[Export] public Color LineColor = new Color(1f, 1f, 1f, 1f);

	// Layer 1 + Layer 2
	// Godot layer index 1 = bit 0
	// Godot layer index 2 = bit 1
	[Export] public uint CollisionMask = (1u << 0) | (1u << 1);
	[Export] public bool CollideWithBodies = true;
	[Export] public bool CollideWithAreas = true;
	public Vector2 Direction = Vector2.Right;
	public CollisionObject2D? OwnerNode;

	private Vector2 _prevGlobal;
	private Vector2 _currGlobal;
	private Vector2 _startGlobal;
	private float _travelled = 0f;

	public override void _Ready()
	{
		Direction = Direction.Normalized();
		_prevGlobal = GlobalPosition;
		_currGlobal = GlobalPosition;
		_startGlobal = GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		_prevGlobal = GlobalPosition;
		throw new NotImplementedException("Bullet movement and collision detection not implemented yet.");
	}

	public override void _Draw()
	{
		Vector2 a = ToLocal(_prevGlobal);
		Vector2 b = ToLocal(_currGlobal);
		DrawLine(a, b, LineColor, LineWidth);
	}

	private void OnHit(CollisionObject2D hitObject, Vector2 hitPos)
	{
		GD.Print($"Bullet hit: {hitObject.Name} at {hitPos}");
	}

}
