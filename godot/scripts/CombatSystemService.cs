using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Combat {
	/// <summary>
	/// Categories of damage that can be carried and combined in a <see cref="DamageContainer"/>.
	/// </summary>
	public enum DamageType {
		Fire,
		Poison,
		Pierce,
		Crush,
		Slash,
		Explosive
	}

	/// <summary>
	/// A small value object that stores damage amounts per <see cref="DamageType"/>.
	/// </summary>
	/// <remarks>
	/// Values are stored as integers and are typically interpreted as "raw" damage.
	/// The container is mutable; methods like <see cref="AddDamage"/>, <see cref="ScaleDamage"/>,
	/// and <see cref="Combine"/> mutate the instance.
	/// </remarks>
	public class DamageContainer {
		private Dictionary<DamageType, int> damageValues;

		/// <summary>
		/// Creates a container backed by an existing dictionary.
		/// </summary>
		/// <remarks>
		/// The provided dictionary instance is used directly (not copied). Mutating the returned
		/// dictionary from <see cref="GetAllDamage"/> will mutate this container and vice-versa.
		/// </remarks>
		/// <param name="damageValues">Backing store for damage values.</param>
		public DamageContainer(Dictionary<DamageType, int> damageValues) {
			this.damageValues = damageValues;
		}

		/// <summary>
		/// Creates a container with a single damage type.
		/// </summary>
		/// <param name="type">Damage type to set.</param>
		/// <param name="value">Damage amount.</param>
		public DamageContainer(DamageType type, int value) {
			this.damageValues = new Dictionary<DamageType, int>();
			this.damageValues[type] = value;
		}

		/// <summary>
		/// Creates a container from optional values for each supported damage type.
		/// </summary>
		/// <remarks>
		/// Any null parameter is omitted from the container.
		/// </remarks>
		public DamageContainer(int? fire, int? poison, int? pierce, int? crush, int? slash, int? explosive) {
			this.damageValues = new Dictionary<DamageType, int>();
			if (fire.HasValue) this.damageValues[DamageType.Fire] = fire.Value;
			if (poison.HasValue) this.damageValues[DamageType.Poison] = poison.Value;
			if (pierce.HasValue) this.damageValues[DamageType.Pierce] = pierce.Value;
			if (crush.HasValue) this.damageValues[DamageType.Crush] = crush.Value;
			if (slash.HasValue) this.damageValues[DamageType.Slash] = slash.Value;
			if (explosive.HasValue) this.damageValues[DamageType.Explosive] = explosive.Value;
		}

		/// <summary>
		/// Returns the damage value for a given type, or 0 if not present.
		/// </summary>
		public int GetDamage(DamageType type) {
			return damageValues.ContainsKey(type) ? damageValues[type] : 0;
		}

		/// <summary>
		/// Returns the underlying damage dictionary.
		/// </summary>
		/// <remarks>
		/// This returns the internal dictionary by reference (not a copy).
		/// External mutations will affect this container.
		/// </remarks>
		public Dictionary<DamageType, int> GetAllDamage() {
			return damageValues;
		}

		/// <summary>
		/// Adds damage of the given type, accumulating with any existing value.
		/// </summary>
		public void AddDamage(DamageType type, int value) {
			if (damageValues.ContainsKey(type)) {
				damageValues[type] += value;
			} else {
				damageValues[type] = value;
			}
		}

		/// <summary>
		/// Multiplies all stored values by <paramref name="multiplier"/>.
		/// </summary>
		/// <remarks>
		/// Values are truncated toward zero when converted back to int.
		/// </remarks>
		public void ScaleDamage(float multiplier) {
			foreach (var key in damageValues.Keys.ToList()) {
				damageValues[key] = (int)(damageValues[key] * multiplier);
			}
		}

		/// <summary>
		/// Adds all damage values from <paramref name="other"/> into this container.
		/// </summary>
		public void Combine(DamageContainer other) {
			foreach (var kv in other.GetAllDamage()) {
				AddDamage(kv.Key, kv.Value);
			}
		}

		/// <summary>
		/// Returns the sum of all damage values.
		/// </summary>
		public int TotalDamage() {
			return damageValues.Values.Sum();
		}

		/// <summary>
		/// Removes all stored damage values.
		/// </summary>
		public void ClearDamage() {
			damageValues.Clear();
		}

		/// <summary>
		/// Returns true if the container has no entries.
		/// </summary>
		public bool IsEmpty() {
			return damageValues.Count == 0;
		}

		/// <summary>
		/// Returns true if the container stores a value for <paramref name="type"/>.
		/// </summary>
		public bool HasDamageType(DamageType type) {
			return damageValues.ContainsKey(type);
		}

		/// <summary>
		/// Returns a human-readable summary of the stored values.
		/// </summary>
		public override string ToString() {
			return string.Join(", ", damageValues.Select(kv => $"{kv.Key}: {kv.Value}"));
		}


	}
}
