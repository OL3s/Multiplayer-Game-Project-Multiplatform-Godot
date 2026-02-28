using Godot;
using System;

namespace Shared {
	public struct PlayerData
	{
		public bool IsReady;
		public string Name;
		public int TeamId;

		public PlayerData(bool isReady, string name, int teamId)
		{
			IsReady = isReady;
			Name = name;
			TeamId = teamId;
		}
	}
}
