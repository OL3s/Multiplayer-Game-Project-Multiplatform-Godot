#nullable enable
using Godot;
using System;

/// <summary>
/// Factory for spawning entities in the game. Provides methods to spawn various types of entities, such as bullets, players, props, etc.
/// </summary>
/// <remarks>
/// This class can be extended to include more specific spawn methods for different entity types, allowing for centralized control over entity creation and initialization.
/// </remarks>
/// <example>
/// // Example usage:
/// // var bullet = EntityFactory.Instance.SpawnBullet(new Vector2(1, 0), speed: 1500f, maxDistance: 2000f);
/// </example>
public partial class EntityFactory : Node
{
	public static EntityFactory? Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		GD.Print("EntityFactory ready and instance set.");
	}

	public T Spawn<T>(string path, Node2D? position = null, Node? parent = null) where T : Node2D
	{
		var scene = GD.Load<PackedScene>(path);
		var obj = scene.Instantiate<T>();

		parent ??= GetTree().CurrentScene;
		parent.AddChild(obj);

		if (position != null)
			obj.GlobalPosition = position.GlobalPosition;

		GD.Print($"Spawned {typeof(T).Name} from {path} at position {obj.GlobalPosition}");
		return obj;
	}

	public Bullet SpawnBullet(Vector2 direction, float? speed = null, float? maxDistance = null)
	{
		var bullet = Spawn<Bullet>("res://scenes/game/tscn/Bullet.tscn");
		if (speed.HasValue)         bullet.Speed = speed.Value;
		if (maxDistance.HasValue)   bullet.MaxDistance = maxDistance.Value;
									bullet.Direction = direction;
		return bullet;
	}

	public Player SpawnPlayer(Vector2 position)
	{
		var player = Spawn<Player>("res://scenes/game/tscn/Player.tscn");
		player.GlobalPosition = position;
		return player;
	}
}
