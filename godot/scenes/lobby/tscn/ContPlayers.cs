using Godot;
using System;
using Shared;
using System.Collections.Generic;
using System.Linq;

public partial class ContPlayers : HFlowContainer
{

	public override void _Ready()
	{
		UpdatePlayersAll();
		var networkService = GetNode<NetworkService>("/root/NetworkService");
		networkService.PeersUpdated += UpdatePlayersAll; // subscribe to player list updates
	}

	public void UpdatePlayersAll()
	{
		var networkService = GetNode<NetworkService>("/root/NetworkService");
		var playerList = networkService.Peers.Values.ToList();
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
				slot.SetReady(plData.Ready);
				slot.BorderPanel.Modulate = TeamColor(plData.TeamId);
			}

			slotIndex++;
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
