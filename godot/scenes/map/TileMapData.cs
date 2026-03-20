using Godot;
using MapGeneration;
using Combat;
using System;
using System.Collections.Generic;

public partial class TileMapData : TileMapLayer
{
	private MapGeneratorData data;
	private HashSet<Vector2I> wallTiles => MapGeneratorData.GetWallTiles(data.TileFloor);
	[Export] public int Seed { get; set; }
	[Export] public int Padding = 2;
	[Export] public int Length = 300;
	[Export] public int SourceId;
	[Export] public Vector2I AtlasDestroyedTile = new Vector2I(0, 0);
	public Dictionary<Vector2I, CombatContainer> TileData = new Dictionary<Vector2I, CombatContainer>();
	public override void _Ready()
	{
		InitiateMapgen();
	}

	public void InitiateMapgen()
	{
		Clear();

		data = MapGeneratorData.GenerateMap((Seed == 0) ? new Random().Next() : Seed, Length, Padding);
		MapGenerationToTileData(data);
	}

	public ApplyDamageResult ApplyDamageOnTile(Vector2I tileIndex, DamageApply damage)
	{

		// fetch key path
		bool hasData = TileData.TryGetValue(tileIndex, out CombatContainer container);

		// add new container if not exists on hit
		// TODO: add values to a config file, with different values for different biomes etc.
		if (!hasData) TileData[tileIndex] = container = new CombatContainer(
			health: 1000,
			armor: new DamageArmor(
				baseValue: 100,
				pierce: 10,
				crush: 0,
				explosive: 0
			),
			penetrationCost: 500,
			teamId: 0
		);

		var result = container.ApplyDamage(damage, enableFriendlyFire: true);
		if (result.IsDead) DestroyTile(tileIndex);

		return result;
	}

	public ApplyDamageResult ApplyDamageOnPosition(Vector2 position, DamageApply damage)
	{
		Vector2I tileIndex = LocalToMap(position);
		return ApplyDamageOnTile(tileIndex, damage);
	}

	private void DestroyTile(Vector2I tileIndex)
	{
		SetCell(tileIndex, SourceId, AtlasDestroyedTile);
	}

	private void MapGenerationToTileData(MapGeneratorData data, int borderPadding = 5)
	{
		// TODO: convert MapGeneratorData to TileData.
		foreach (var tile in data.TileFloor)
		{
			SetCell(tile + new Vector2I(borderPadding, borderPadding), SourceId, new Vector2I(0, 0));
		}

		foreach (var wall in wallTiles)
		{
			SetCell(wall + new Vector2I(borderPadding, borderPadding), SourceId, new Vector2I(6, 2));
		}

	}

}
