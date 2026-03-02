using Godot;
using System;
using System.Collections.Generic;

namespace Shared {
	public class PlayerData
	{
		public bool Ready;
		public string Name;
		public int TeamId;
		public int Credits;
		public int Score;
		public int Kills;

		public PlayerData(bool ready, string name, int teamId, int credits, int score, int kills)
		{
			Ready = ready;
			Name = name;
			TeamId = teamId;
			Credits = credits;
			Score = score;
			Kills = kills;
		}

		public PlayerData(string name) : this(false, name, 0, 0, 0, 0) { }
	}

	public class MatchConfig
	{
		public ulong MapSeed;
		public int MapSize;
		public HashSet<GameMode> GameModes;
		public HashSet<GearSettings> StartGearSettings;
		public int RoundsToWin;
		public int TimeLimitMinutes;


		public MatchConfig(ulong mapSeed, int mapSize, HashSet<GameMode> gameModes, HashSet<GearSettings> startGearSettings, int roundsToWin, int timeLimitMinutes)
		{
			MapSeed = mapSeed;
			MapSize = mapSize;
			GameModes = gameModes;
			StartGearSettings = startGearSettings;
			RoundsToWin = roundsToWin;
			TimeLimitMinutes = timeLimitMinutes;
		}
		public MatchConfig() : this((ulong)GD.Randi(), 4, new HashSet<GameMode> { GameMode.Deathmatch }, new HashSet<GearSettings> { GearSettings.BuyPhase }, 3, 5) { }
		public void UpdateFrom(MatchConfig other)
		{
			MapSeed = other.MapSeed;
			MapSize = other.MapSize;
			GameModes = other.GameModes;
			StartGearSettings = other.StartGearSettings;
			RoundsToWin = other.RoundsToWin;
			TimeLimitMinutes = other.TimeLimitMinutes;
		}
		// ---------- SETTERS ----------
		public void SetMapSeed(ulong seed) => MapSeed = seed;
		public void SetMapSize(int size) => MapSize = size;
		public void SetGameModes(HashSet<GameMode> modes) => GameModes = modes;
		public void SetStartGearSettings(HashSet<GearSettings> settings) => StartGearSettings = settings;
		public void SetRoundsToWin(int rounds) => RoundsToWin = rounds;
		public void SetTimeLimitMinutes(int minutes) => TimeLimitMinutes = minutes;

		// ---------- GETTERS HASH ----------
		public bool HasGameMode(GameMode mode) => GameModes != null && GameModes.Contains(mode);
		public bool HasStartGearSetting(GearSettings setting) => StartGearSettings != null && StartGearSettings.Contains(setting);
		public List<GameMode> GetGameModes() => GameModes != null ? new List<GameMode>(GameModes) : null;
		public List<GearSettings> GetStartGearSettings() => StartGearSettings != null ? new List<GearSettings>(StartGearSettings) : null;

		public override string ToString()
		{
			return $"MapSeed: {MapSeed}, MapSize: {MapSize}, GameModes: [{string.Join(", ", GameModes)}], StartGearSettings: [{string.Join(", ", StartGearSettings)}], RoundsToWin: {RoundsToWin}, TimeLimitMinutes: {TimeLimitMinutes}";
		}
	}

	public enum GameMode
	{
		Deathmatch,
		CaptureTheFlag,
		KingOfTheHill,
		KillConfirmed,
		BattleRoyale,
		Domination,
	}

	public enum GearSettings
	{
		RandomPresetDuplicates,		// everyone gets the same random preset
		RandomPresetNoDuplicates,	// everyone gets a random preset
		ChoosePreset,				// everyone gets to choose a preset of several gear options
		BuyPhase,					// players can buy/sell gear in a buy phase between rounds
		BattleRoyale,				// player start with basic gear and can find better gear in the map
	}
}
