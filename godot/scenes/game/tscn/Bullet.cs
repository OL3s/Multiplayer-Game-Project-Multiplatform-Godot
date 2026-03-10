using Combat;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

public partial class Bullet : Node2D
{
	[Export] public float Speed = 1200f;
	[Export] public float MaxDistance = 1400f;
	private float _distanceTravelled = 0f;
	[Export] public float DamageFalloffStart = 800f;
	[Export] public float LineWidth = 8.0f;
	[Export] public Color LineColor = new Color(1, 1, 0, 1); // Yellow color

	// Layer 1 + Layer 2
	// Godot layer index 1 = bit 0
	// Godot layer index 2 = bit 1
	[Export] public uint CollisionMask = (1u << 0) | (1u << 1);
	[Export] public bool CollideWithBodies = true;
	[Export] public bool CollideWithAreas = true;
	public DamageApply Damage = new DamageApply();
	[Export] public float Penetration = 100f; // how much penetration this bullet has left
	public Vector2 Direction = Vector2.Right;
	public CollisionObject2D? OwnerNode;
	private HashSet<CollisionObject2D> _alreadyHit = new HashSet<CollisionObject2D>();
	private Vector2 _lineStartLocal = Vector2.Zero;


	public override void _Ready()
	{
		Direction = Direction.Normalized();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Initiate positions
		Vector2 currentPos = GlobalPosition;
		Vector2 displacement = Direction * Speed * (float)delta;
		Vector2 nextPos = currentPos + displacement;
		_distanceTravelled += displacement.Length();

		// Check for collisions along the path from currentPos to nextPos
		var space = GetWorld2D().DirectSpaceState;
		var query = new PhysicsRayQueryParameters2D {
			From = currentPos,
			To = nextPos,
			CollisionMask = CollisionMask,
			CollideWithBodies = CollideWithBodies,
			CollideWithAreas = CollideWithAreas
		};
		var results = space.IntersectRay(query);
		
		// Process collision (Godot 4: IntersectRay returns a Dictionary for the first hit)
		if (results.Count > 0
			&& results.TryGetValue("collider", out var colliderVar)
			&& colliderVar.VariantType != Variant.Type.Nil) {
			var colliderObj = colliderVar.AsGodotObject();
			if (colliderObj is CollisionObject2D hitObject) {
				OnHit(hitObject);
			}
		}

		// Check if bullet should be destroyed
		if (IsPenetrationDepleted() || IsDistanceDepleted()) {
			QueueFree();
			return;
		}

		// draw event + update pos
		GlobalPosition = nextPos;
		_lineStartLocal = ToLocal(currentPos);
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawLine(_lineStartLocal, Vector2.Zero, LineColor, LineWidth);
	}

	private void OnHit(CollisionObject2D hitObject)
	{
		// Fetch the CombatNode from the hit object, if it exists
		var hitNode = hitObject.GetChildren().OfType<CombatNode>().FirstOrDefault(); // Check if the hit object has a CombatNode

		// === Exceptions and early exits ===
		// Not a combat node, ignore
		if (hitNode == null) { 
			GD.Print("Hit object does not have a CombatNode, ignore"); 
			return; 
		} 

		// Hit self, ignore
		if (hitObject == OwnerNode) {
			GD.Print("Hit self, ignore");
			return;
		}

		// Already hit this object, ignore
		if (_alreadyHit.Contains(hitObject)) 
			return;
		// ====================================

		// Calculate damage with falloff and penetration
		var container = hitNode.Container;

		float falloffMultiplier = CalculateDamageFalloff();
		float penetrationMultiplier = CalculatePenetrationPercent();
		Penetration = Math.Max(0, Penetration - container.PenetrationCost); // reduce penetration by target's penetration cost

		DamageApply scaledDamage = Damage * falloffMultiplier * penetrationMultiplier;
		(bool isDead, int damageTaken) = container.ApplyDamage(scaledDamage);
		_alreadyHit.Add(hitObject); // mark this object as hit to prevent multiple hits in one shot
		GD.Print($"Bullet hit {hitObject.Name} with damage: {damageTaken} (falloff: {falloffMultiplier:F2}, penetration: {penetrationMultiplier:F2})");

		if (isDead) {
			hitObject.QueueFree(); // or some death handling logic
			GD.Print($"Hit object {hitObject.Name} died from damage: {damageTaken}");
		} 
	}

	private float CalculateDamageFalloff() {
		if (_distanceTravelled <= DamageFalloffStart) 
			return 1f; // no falloff

		float excessDistance = _distanceTravelled - DamageFalloffStart;
		float falloffRange = MaxDistance - DamageFalloffStart;

		if (falloffRange <= 0) 
			return 0f; // avoid division by zero, treat as instant drop to 0

		float falloffPercent = excessDistance / falloffRange;
		return Math.Max(0f, 1f - falloffPercent); // linear falloff, clamped to 0
	}

	private float CalculatePenetrationPercent() {
		// Penetration is treated as a 0..100 value.
		// Convert to damage multiplier (0..1).
		return Math.Clamp(Penetration / 100f, 0f, 1f);
	}

	private bool IsPenetrationDepleted() {
		return Penetration <= 0;
	}

	private bool IsDistanceDepleted() {
		return _distanceTravelled >= MaxDistance;
	}

}
