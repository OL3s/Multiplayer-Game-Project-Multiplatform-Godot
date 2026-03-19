using Godot;
using System;
using Combat;
using System.Linq;

public partial class CombatNode : Node2D
{

	[Export] public CollisionObject2D ParentObject;
	[Export] public StaticSpriteAnimate StaticAnimateSprite;
	private PlayerSpriteAnimate _playerAnimateSprite;
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
		if (ParentObject == null)
			throw new Exception("CombatNode requires a reference to its parent object for death handling. Please set the ParentObject property in the inspector.");

		// check if player is parent for animation
		if (ParentObject is Player)
		{
			_playerAnimateSprite = ParentObject
				.GetChildren()
				.OfType<PlayerSpriteAnimate>()
				.FirstOrDefault();

			if (_playerAnimateSprite == null)
				GD.PrintErr("Parent player object does not have a PlayerSpriteAnimate child node. Player animations will not work.");
		}
		

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

	public ApplyDamageResult ApplyDamage(DamageApply damage, bool enableFriendlyFire)
	{

		// Apply damage to the container and check for death
		var result = Container.ApplyDamage(damage, enableFriendlyFire: enableFriendlyFire);
		if (result.IsDead)
			ParentObject?.QueueFree(); // For simplicity, just free the parent node (which should be the character body)

		QueueRedraw(); // Redraw to update any visual representation of health/armor/etc.

		// Animate player sprite if available
		if (_playerAnimateSprite != null)
		{
			var pas = _playerAnimateSprite;
			var strength = Mathf.Clamp(result.DamageTaken / 100f, 0, 0.8f); // Normalize damage to a 0-0.8 range for animation strength
			pas.Scale = new Vector2(pas.Scale.X * (1 - strength), pas.Scale.Y * (1 + strength)); // stretch vertically and compress horizontally
		}

		// Animate static sprite if available
		else if (StaticAnimateSprite != null)
		{
			var sas = StaticAnimateSprite;
			var strength = Mathf.Clamp(result.DamageTaken / 100f, 0, 0.8f); // Normalize damage to a 0-0.8 range for animation strength
			sas.Scale = new Vector2(sas.Scale.X * (1 - strength), sas.Scale.Y * (1 + strength)); // stretch vertically and compress horizontally
		}

		return result;
	}
}
