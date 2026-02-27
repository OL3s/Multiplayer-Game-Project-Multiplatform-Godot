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
		playerList.Add(new playerData());
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
		}

		slotIndex++;
	}
}

	private struct playerData
	{
		public bool IsReady;
		public string Name;
		
		public playerData(bool isReady, string name) {
			IsReady = isReady;
			Name = name;
		}
		
		public playerData() {
			IsReady = false;
			Name = "noname";
		}
	}
}
