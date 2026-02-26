using Godot;
using System;
using System.Collections.Generic;

public partial class ContPlayers : HFlowContainer
{
	private readonly List<StatPlayerLobby> playerList = new();

	public override void _Ready()
	{
		UpdateVisibility();
	}

	public void UpdateVisibility()
	{
		var children = GetChildren();

		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is not Control slot)
				continue;

			// Show slot only if we have player data for it
			slot.Visible = i < playerList.Count;
		}
	}

	public struct StatPlayerLobby
	{
		public bool IsReady;
		public string Name;
	}
}
