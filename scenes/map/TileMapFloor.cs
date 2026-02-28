using Godot;
using Mapgen;

public partial class TileMapFloor : TileMapLayer
{
	private MapgenData data;
	[Export] public int SourceId;

	public override void _Ready()
	{
		InitiateMapgen();
	}

	public void InitiateMapgen()
	{
		Clear();
		
		var rng = new RandomNumberGenerator();
		ulong seed = rng.Randi();   // full 64-bit

		data = MapgenData.GenerateMap(seed, 200);

		foreach (var p in data.TileFloor)
		{
			SetCell(p, SourceId, new Vector2I(1, 0));
		}
	}
}
