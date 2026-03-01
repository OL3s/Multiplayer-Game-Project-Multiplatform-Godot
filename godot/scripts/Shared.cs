using Godot;
using System;

namespace Shared {
	public struct PlayerData
	{
		public bool Ready;
		public string Name;
		public int TeamId;

		public PlayerData(bool ready, string name, int teamId)
		{
			Ready = ready;
			Name = name;
			TeamId = teamId;
		}

		public PlayerData() : this(false, "Noname", 0) { }
	}
}
