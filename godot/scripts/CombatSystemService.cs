using System;
using System.Collections.Generic;
using System.Linq;

namespace Combat {
	public enum DamageType {
		Fire,
		Poison,
		Pierce,
		Crush,
		Explosive
	}

	/// <summary>
	/// Base container for per-damage-type integer values.
	/// DamageApply = raw damage values.
	/// DamageArmor = % reduction values (0..100), clamped.
	/// </summary>
	public abstract class DamageContainer : Dictionary<DamageType, int> {
		public DamageContainer() : base() { }

		public DamageContainer(IDictionary<DamageType, int> dict)
			: base(dict ?? throw new ArgumentNullException(nameof(dict))) { }

		public DamageContainer(DamageType type, int value) : base() {
			this[type] = value;
		}

		public DamageContainer(
			int? fire = null,
			int? poison = null,
			int? pierce = null,
			int? crush = null,
			int? explosive = null
		) : base() {
			if (fire.HasValue) this[DamageType.Fire] = fire.Value;
			if (poison.HasValue) this[DamageType.Poison] = poison.Value;
			if (pierce.HasValue) this[DamageType.Pierce] = pierce.Value;
			if (crush.HasValue) this[DamageType.Crush] = crush.Value;
			if (explosive.HasValue) this[DamageType.Explosive] = explosive.Value;
		}

		public int GetValue(DamageType type) => TryGetValue(type, out var v) ? v : 0;

		public int TotalValue() => Values.Sum();

		public override string ToString() {
			return string.Join(", ", Keys.OrderBy(k => k).Select(k => $"{k}: {this[k]}"));
		}

		/// <summary>
		/// True if no entries, or all values are 0.
		/// </summary>
		public bool IsEmpty() => Count == 0 || Values.All(v => v == 0);

		// ---------- SHARED HELPERS ----------
		// Intentionally no operator overloads on the abstract base.
		// Operators live on concrete types so their return types are correct.
		protected static Dictionary<DamageType, int> AddDict(DamageContainer left, DamageContainer right) {
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));

			var result = new Dictionary<DamageType, int>(left);
			foreach (var kvp in right) {
				result[kvp.Key] = (result.TryGetValue(kvp.Key, out var existing) ? existing : 0) + kvp.Value;
			}
			return result;
		}

		protected static Dictionary<DamageType, int> SubtractDict(DamageContainer left, DamageContainer right) {
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));

			var result = new Dictionary<DamageType, int>(left);
			foreach (var kvp in right) {
				result[kvp.Key] = (result.TryGetValue(kvp.Key, out var existing) ? existing : 0) - kvp.Value;
			}
			return result;
		}

		protected static Dictionary<DamageType, int> ScaleDict(DamageContainer damage, float multiplier) {
			if (damage == null) throw new ArgumentNullException(nameof(damage));

			var result = new Dictionary<DamageType, int>(damage.Count);
			foreach (var kvp in damage) {
				result[kvp.Key] = (int)(kvp.Value * multiplier);
			}
			return result;
		}

		protected static Dictionary<DamageType, int> DivideDict(DamageContainer damage, float divisor) {
			if (damage == null) throw new ArgumentNullException(nameof(damage));
			if (divisor == 0) throw new DivideByZeroException();
			return ScaleDict(damage, 1f / divisor);
		}
	}


	/// <summary>
	/// Raw damage numbers to apply. Before armor mitigation.
	/// </summary>
	public class DamageApply : DamageContainer {
		public int TeamId { get; set; } // optional team ID for friend logic

		private static Dictionary<DamageType, int> BuildDict(
			int? fire,
			int? poison,
			int? pierce,
			int? crush,
			int? explosive
		) {
			var dict = new Dictionary<DamageType, int>();
			if (fire.HasValue) dict[DamageType.Fire] = fire.Value;
			if (poison.HasValue) dict[DamageType.Poison] = poison.Value;
			if (pierce.HasValue) dict[DamageType.Pierce] = pierce.Value;
			if (crush.HasValue) dict[DamageType.Crush] = crush.Value;
			if (explosive.HasValue) dict[DamageType.Explosive] = explosive.Value;
			return dict;
		}

		// Canonical ctor: TeamId is assigned in exactly one place.
		public DamageApply(IDictionary<DamageType, int> dict, int teamId = 0) : base(dict) {
			TeamId = teamId;
		}

		public DamageApply(DamageType type, int value, int teamId = 0)
			: this(new Dictionary<DamageType, int> { [type] = value }, teamId) { }

		// Convenience for named args: `new DamageApply(pierce: 50)` and `new DamageApply(teamId: 2, pierce: 50)`.
		public DamageApply(
			int teamId = 0,
			int? fire = null,
			int? poison = null,
			int? pierce = null,
			int? crush = null,
			int? explosive = null
		) : this(BuildDict(fire, poison, pierce, crush, explosive), teamId) { }

		// ---------- OPERATORS (DamageApply) ----------
		public static DamageApply operator +(DamageApply left, DamageApply right)
			=> new DamageApply(AddDict(left, right), left.TeamId);

		public static DamageApply operator -(DamageApply left, DamageApply right)
			=> new DamageApply(SubtractDict(left, right), left.TeamId);

		public static DamageApply operator *(DamageApply damage, float multiplier)
			=> new DamageApply(ScaleDict(damage, multiplier), damage.TeamId);

		public static DamageApply operator *(float multiplier, DamageApply damage) => damage * multiplier;
		public static DamageApply operator *(DamageApply damage, int multiplier) => damage * (float)multiplier;
		public static DamageApply operator *(int multiplier, DamageApply damage) => damage * multiplier;

		public static DamageApply operator /(DamageApply damage, float divisor)
			=> new DamageApply(DivideDict(damage, divisor), damage.TeamId);

		public static DamageApply operator /(DamageApply damage, int divisor) => damage / (float)divisor;

		// ---------- METHODS ----------
		public (bool isDead, int damageTaken) ApplyTo(CombatContainer target) {
			if (target == null) throw new ArgumentNullException(nameof(target));
			return target.ApplyDamage(this);
		}
	}

	/// <summary>
	/// Armor values are % reduction per type: 0 = no reduction, 100 = immune.
	/// Values are clamped to [0,100].
	/// </summary>
	/// <example>
	/// 20 fire armor means "take 20% less fire damage". So 100 fire damage would be reduced to 80, while 50 fire damage would be reduced to 40.
	/// </example>
	public class DamageArmor : DamageContainer {
		// Clamp happens once here; every other constructor chains to this one.
		public DamageArmor(IDictionary<DamageType, int> dict, int baseValue) : base(dict) {
			SetBaseValue(baseValue);
			ClampAllValues();
		}

		public DamageArmor() : this(new Dictionary<DamageType, int>(), 0) { }

		public DamageArmor(DamageType type, int value)
			: this(new Dictionary<DamageType, int> { [type] = value }, 0) { }

		public DamageArmor(int baseValue, int? fire = null, int? poison = null, int? pierce = null, int? crush = null, int? explosive = null)
			: this(BuildDict(fire, poison, pierce, crush, explosive), baseValue) { }

		private static Dictionary<DamageType, int> BuildDict(
			int? fire, int? poison, int? pierce, int? crush, int? explosive
		) {
			var dict = new Dictionary<DamageType, int>();
			if (fire.HasValue) dict[DamageType.Fire] = fire.Value;
			if (poison.HasValue) dict[DamageType.Poison] = poison.Value;
			if (pierce.HasValue) dict[DamageType.Pierce] = pierce.Value;
			if (crush.HasValue) dict[DamageType.Crush] = crush.Value;
			if (explosive.HasValue) dict[DamageType.Explosive] = explosive.Value;
			return dict;
		}

		private void ClampAllValues() {
			foreach (var key in Keys.ToList()) {
				this[key] = Math.Clamp(this[key], 0, 100);
			}
		}

		private void SetBaseValue(int baseValue) {
			foreach (DamageType type in Enum.GetValues<DamageType>()) {
				TryAdd(type, baseValue); // only adds if missing
			}
		}

		// ---------- OPERATORS (DamageArmor) ----------
		public static DamageArmor operator +(DamageArmor left, DamageArmor right)
			=> new DamageArmor(AddDict(left, right), baseValue: 0);

		public static DamageArmor operator -(DamageArmor left, DamageArmor right)
			=> new DamageArmor(SubtractDict(left, right), baseValue: 0);

		public static DamageArmor operator *(DamageArmor armor, float multiplier)
			=> new DamageArmor(ScaleDict(armor, multiplier), baseValue: 0);

		public static DamageArmor operator *(float multiplier, DamageArmor armor) => armor * multiplier;

		public static DamageArmor operator /(DamageArmor armor, float divisor)
			=> new DamageArmor(DivideDict(armor, divisor), baseValue: 0);
	}

	public static class DamageCalculator {
		/// <summary>
		/// Returns total damage after per-type armor mitigation.
		/// final = raw * (100 - armor%) / 100
		/// </summary>
		public static int CalculateDamage(DamageApply baseDamage, DamageArmor targetArmor) {
			if (baseDamage == null) throw new ArgumentNullException(nameof(baseDamage));
			if (targetArmor == null) throw new ArgumentNullException(nameof(targetArmor));

			int totalDamage = 0;

			foreach (var damageType in Enum.GetValues<DamageType>()) {
				int raw = baseDamage.GetValue(damageType);
				int armorPercent = targetArmor.GetValue(damageType); // expected 0..100

				int final = raw * (100 - armorPercent) / 100; // integer math (truncates)
				totalDamage += final;
			}

			return totalDamage;
		}
	}

	/// <summary>
	/// Container for an entity's combat state: health + armor.
	/// </summary>
	public class CombatContainer {
		public int Health { get; set; }
		public DamageArmor Armor { get; set; }
		public float PenetrationCost { get; set; } // how much penetration is removed when hit
		public int TeamId { get; set; } // optional team ID for friend/foe logic
		public CombatContainer(int health, DamageArmor armor, int penetrationCost, int teamId) {
			Health = health;
			Armor = armor ?? new DamageArmor();
			PenetrationCost = penetrationCost;
			TeamId = teamId;
		}

		public CombatContainer() : this(100, new DamageArmor(), 100, 0) { }

		public (bool isDead, int damageTaken) ApplyDamage(DamageApply damage) {
			if (damage == null) throw new ArgumentNullException(nameof(damage));
			Armor ??= new DamageArmor();

			var finalDamage = DamageCalculator.CalculateDamage(damage, Armor);

			Health = Math.Max(0, Health - finalDamage);
			return (Health <= 0, finalDamage);
		}
	}
}
