using Godot;
using System;
using Combat;

public partial class CombatNode : Node
{
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
	}
	
}
