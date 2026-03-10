using Godot;
using System;
using Combat;
using System.Collections;

public partial class CombatNode : Node2D
{
	[Export] public bool DebugDraw = false;
	[Export] public int Health = 100;
	[Export] public int TeamId = 0;
	[Export] public int PenetrationCost = 100;

	[ExportGroup("Armor")]
	[Export] public int ArmorBase = 0;
	[Export] public int ArmorFire = 0;
	[Export] public int ArmorPoison = 0;
	[Export] public int ArmorPierce = 0;
	[Export] public int ArmorCrush = 0;
	[Export] public int ArmorExplosive = 0;

	public CombatContainer Container { get; private set; }
	public override void _Ready()
	{
		var armor = new DamageArmor(
			baseValue: ArmorBase,
			fire: ArmorFire,
			poison: ArmorPoison,
			pierce: ArmorPierce,
			crush: ArmorCrush,
			explosive: ArmorExplosive
		);

		Container = new CombatContainer(
			health: Health,
			armor: armor,
			penetrationCost: PenetrationCost,
			teamId: TeamId
		);

		QueueRedraw(); // Redraw to show health bar if in debug mode
	}

	public override void _Draw()
	{
		base._Draw();
		// Draw health bar
		if (DebugDraw || OS.IsDebugBuild()) {
			float healthPercent = Container.Health / (float)Health;
			Color healthColor = healthPercent > 0.5f ? Colors.Green : (healthPercent > 0.25f ? Colors.Yellow : Colors.Red);
			DrawRect(new Rect2(-25, -40, 50 * healthPercent, 5), healthColor);
			var outlineColor = Colors.White;
			outlineColor.A = 0.5f;
			DrawRect(new Rect2(-25, -40, 50, 5), outlineColor, false);
		}
	}

	public (bool isDead, int damageTaken) ApplyDamage(DamageApply damage)
	{
		var (isDead, damageTaken) = Container.ApplyDamage(damage);
		if (isDead)
		{
			GetParent().QueueFree(); // For simplicity, just free the parent node (which should be the character body)
		}

		QueueRedraw(); // Redraw to update any visual representation of health/armor/etc.

		return (isDead, damageTaken);
	}
	
}
