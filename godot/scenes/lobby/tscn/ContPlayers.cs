using Godot;
using System;
using System.Collections.Generic;

public partial class ContPlayers : HFlowContainer
{
	private readonly List<playerData> playerList = new();

	public override void _Ready()
	{
		// MOCK DATA
		playerList.Add(new playerData());
		playerList.Add(new playerData(true, "yayeet", 1));
		UpdatePlayersAll();
	}

	public void UpdatePlayersAll()
	{
		var children = GetChildren();
		int slotIndex = 0;

		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is not CtrlPlayer slot)
				continue;

			bool hasPlayer = slotIndex < playerList.Count;
			slot.Visible = hasPlayer;

			if (hasPlayer)
			{
				var plData = playerList[slotIndex];
				slot.NameLabel.Text = plData.Name;
				slot.SetReady(plData.IsReady);
				slot.BorderPanel.Modulate = TeamColor(plData.TeamId);
			}

			slotIndex++;
		}
	}

	private struct playerData
	{
		public bool IsReady;
		public string Name;
		public int TeamId;
		
		public playerData(bool isReady, string name, int teamId) {
			IsReady = isReady;
			Name = name;
			TeamId = teamId;
		}
		
		public playerData() {
			IsReady = false;
			Name = "noname";
			TeamId = 0;
		}
	}
	
	private static Color TeamColor(int teamId) => teamId switch
		{
		0 => Colors.Gray,
		1 => Colors.Red,
		2 => Colors.Blue,
		3 => Colors.Green,
		4 => Colors.Yellow,
		_ => Colors.White
	};

}
