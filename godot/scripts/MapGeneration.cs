using Godot;
using System.Collections.Generic;
using System;

namespace MapGeneration
{
	/// <summary>
	/// Generates a random map as a set of floor tiles and a path connecting them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The map is generated in a coordinate system where the starting tile is at (0,0)
	/// and can extend in all directions. After generation, the map is shifted to positive
	/// coordinates.
	/// </para>
	/// <para>
	/// Calling a new MapgenData() creates an empty map. To generate a new random map,
	/// call MapgenData.GenerateMap(seed).
	/// </para>
	/// </remarks>
	public class MapGeneratorData
	{
		public HashSet<Vector2I> TileFloor;
		public List<Vector2I> Path;

		public MapGeneratorData()
		{
			TileFloor = new HashSet<Vector2I>();
			Path = new List<Vector2I>();
		}

		/// <summary>
		///  Generates a random map using a "drunkard's walk" algorithm. The path starts at (0,0) 
		/// and takes random steps in the four cardinal directions until the desired length is reached. The path can cross
		/// itself, creating loops and branches.
		/// </summary>
		/// <param name="seed">The seed for the random number generator.</param>
		/// <param name="length">The desired length of the path.</param>
		/// <param name="padding">The padding around the path.</param>
		/// <returns>A MapGeneratorData object containing the generated map.</returns>
		public static MapGeneratorData GenerateMap(int seed, int length = 50, int padding = 1)
		{
			//Input exceptions
			if (length <= 0 || padding < 0)
				throw new ArgumentException("Length must be positive and padding cannot be negative.");

			MapGeneratorData data = new();
			Random random = new(seed);
			Vector2I pos = Vector2I.Zero;

			AddTile(data, pos);
			ApplySnake(ref pos, length, data, random);
			ApplyPadding(data, padding);
			
			data.MoveToPositive();

			GD.Print("Map Generated : " + data.GetSize());
			return data;
		}

		private static Vector2I ToVector2I(Vector2 v) =>
			new(Mathf.RoundToInt(v.X), Mathf.RoundToInt(v.Y));

		private static void AddTile(MapGeneratorData data, Vector2I pos)
		{
			data.TileFloor.Add(pos);
			data.Path.Add(pos);
		}

		private static void ApplySnake(ref Vector2I pos, int length, MapGeneratorData data, Random random)
		{
			Vector2 baseDir = new(1f, 0f);

			while (length > 0)
			{
				int step = random.Next(0, 4);
				float angle = step * Mathf.Pi / 2f;
				Vector2I dir = ToVector2I(baseDir.Rotated(angle));

				Vector2I next = pos + dir;

				while (data.TileFloor.Contains(next))
					next += dir;

				pos = next;
				AddTile(data, pos);
				length--;
			}
		}

		private static void ApplyPadding(MapGeneratorData data, int padding)
		{
			var padded = new HashSet<Vector2I>();

			foreach (var tile in data.TileFloor)
			{
				for (int dx = -padding; dx <= padding; dx++)
				{
					for (int dy = -padding; dy <= padding; dy++)
					{
						padded.Add(tile + new Vector2I(dx, dy));
					}
				}
			}

			data.TileFloor = padded;
		}

		public Vector2I GetTopLeft()
		{
			if (TileFloor.Count == 0)
				throw new System.InvalidOperationException("TileFloor is empty");

			bool first = true;
			int minX = 0, minY = 0;

			foreach (var v in TileFloor)
			{
				if (first) { minX = v.X; minY = v.Y; first = false; continue; }
				if (v.X < minX) minX = v.X;
				if (v.Y < minY) minY = v.Y;
			}

			return new Vector2I(minX, minY);
		}

		public Vector2I GetBottomRight()
		{
			if (TileFloor.Count == 0)
				throw new System.InvalidOperationException("TileFloor is empty");

			bool first = true;
			int maxX = 0, maxY = 0;

			foreach (var v in TileFloor)
			{
				if (first) { maxX = v.X; maxY = v.Y; first = false; continue; }
				if (v.X > maxX) maxX = v.X;
				if (v.Y > maxY) maxY = v.Y;
			}

			return new Vector2I(maxX, maxY);
		}

		public static HashSet<Vector2I> GetWallTiles(HashSet<Vector2I> tileFloor)
		{
			var wallTiles = new HashSet<Vector2I>();

			foreach (var floor in tileFloor)
			{
				foreach (var dir in AllDirs)
				{
					var neighbor = floor + dir;
					if (!tileFloor.Contains(neighbor))
						wallTiles.Add(neighbor);
				}
			}

			return wallTiles;
		}

		private static readonly Vector2I[] AllDirs =
		{
			Vector2I.Up,
			Vector2I.Right,
			Vector2I.Down,
			Vector2I.Left,
			new Vector2I(-1, -1),
			new Vector2I(1, -1),
			new Vector2I(1, 1),
			new Vector2I(-1, 1),
		};

		/// <summary>Shifts all tiles and path so the map is in positive coordinates.</summary>
		/// <returns>Offset applied to all positions. Zero if already positive.</returns>
		public Vector2I MoveToPositive()
		{
			if (TileFloor.Count == 0)
				return Vector2I.Zero;

			Vector2I topLeft = GetTopLeft();

			int dx = topLeft.X < 0 ? -topLeft.X : 0;
			int dy = topLeft.Y < 0 ? -topLeft.Y : 0;

			Vector2I offset = new(dx, dy);
			if (offset == Vector2I.Zero)
				return Vector2I.Zero;

			var shifted = new HashSet<Vector2I>(TileFloor.Count);
			foreach (var p in TileFloor)
				shifted.Add(p + offset);
			TileFloor = shifted;

			for (int i = 0; i < Path.Count; i++)
				Path[i] = Path[i] + offset;

			return offset;
		}

		public Vector2I GetSize() =>
			GetBottomRight() - GetTopLeft() + Vector2I.One;
	}


}