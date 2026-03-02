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

		// ---------- OPERATORS ----------
		public static DamageContainer operator +(DamageContainer left, DamageContainer right) {
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));

			var result = new Dictionary<DamageType, int>(left);
			foreach (var kvp in right) {
				result[kvp.Key] = result.GetValueOrDefault(kvp.Key, 0) + kvp.Value;
			}
			return new DamageApply(result);
		}
		public static DamageContainer operator -(DamageContainer left, DamageContainer right) {
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));
			return left + (right * -1);
		}

		public static DamageContainer operator *(DamageContainer damage, float multiplier) {
			if (damage == null) throw new ArgumentNullException(nameof(damage));
			var result = new Dictionary<DamageType, int>();
			foreach (var kvp in damage) {
				result[kvp.Key] = (int)(kvp.Value * multiplier);
			}
			return new DamageApply(result);
		}

		public static DamageContainer operator *(float multiplier, DamageContainer damage) => damage * multiplier;
		public static DamageContainer operator *(DamageContainer damage, int multiplier) => damage * (float)multiplier;
		public static DamageContainer operator *(int multiplier, DamageContainer damage) => damage * multiplier;

		public static DamageContainer operator /(DamageContainer damage, float divisor) {
			if (damage == null) throw new ArgumentNullException(nameof(damage));
			if (divisor == 0) throw new DivideByZeroException();
			var result = new Dictionary<DamageType, int>();
			foreach (var kvp in damage) {
				result[kvp.Key] = (int)(kvp.Value / divisor);
			}
			return new DamageApply(result);
		}
		public static DamageContainer operator /(DamageContainer damage, int divisor) => damage / (float)divisor;

	}


    /// <summary>
    /// Raw damage numbers to apply.
    /// </summary>
    public class DamageApply : DamageContainer {
        public DamageApply() : base() { }
        public DamageApply(IDictionary<DamageType, int> dict) : base(dict) { }
        public DamageApply(DamageType type, int value) : base(type, value) { }
        public DamageApply(int? fire = null, int? poison = null, int? pierce = null, int? crush = null, int? explosive = null)
            : base(fire, poison, pierce, crush, explosive) { }

        public (bool isDead, int damageTaken) ApplyTo(CombatContainer target) {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return target.ApplyDamage(this);
        }
	}

    /// <summary>
    /// Armor values are % reduction per type: 0 = no reduction, 100 = immune.
    /// Values are clamped to [0,100].
    /// </summary>
    public class DamageArmor : DamageContainer {
        // Clamp happens once here; every other constructor chains to this one.
        public DamageArmor(IDictionary<DamageType, int> dict) : base(dict) {
            ClampAllValues();
        }

        public DamageArmor() : this(new Dictionary<DamageType, int>()) { }

        public DamageArmor(DamageType type, int value)
            : this(new Dictionary<DamageType, int> { [type] = value }) { }

        public DamageArmor(int? fire = null, int? poison = null, int? pierce = null, int? crush = null, int? explosive = null)
            : this(BuildDict(fire, poison, pierce, crush, explosive)) { }

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

    public class CombatContainer {
        public int Health { get; set; }
        public DamageArmor Armor { get; set; }

        public CombatContainer(int health, DamageArmor armor) {
            Health = health;
            Armor = armor ?? new DamageArmor();
        }

        public CombatContainer() : this(100, new DamageArmor()) { }

        public (bool isDead, int damageTaken) ApplyDamage(DamageApply damage) {
            if (damage == null) throw new ArgumentNullException(nameof(damage));
            Armor ??= new DamageArmor();

            var finalDamage = DamageCalculator.CalculateDamage(damage, Armor);

            Health = Math.Max(0, Health - finalDamage);
            return (Health <= 0, finalDamage);
        }
    }
}