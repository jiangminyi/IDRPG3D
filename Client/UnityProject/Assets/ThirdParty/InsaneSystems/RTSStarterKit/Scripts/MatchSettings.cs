using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class MatchSettings
	{
		public static MatchSettings currentMatchSettings;

		public List<PlayerSettings> playersSettings { get; set; }
		public MapSettings selectedMap { get; protected set; }

		public MatchSettings() { Reset(); }

		public void Reset() { playersSettings = new List<PlayerSettings>(); }

		public void AddPlayerSettings(PlayerSettings playerSettings) { playersSettings.Add(playerSettings); }
		public void RemovePlayerSettingsById(byte id) { playersSettings.RemoveAt(id); }

		public void RemovePlayerSettings(PlayerSettings playerSettings) { playersSettings.Remove(playerSettings); }
		public void SelectMap(MapSettings selectedMap) { this.selectedMap = selectedMap; }
	}

	[System.Serializable]
	public class PlayerSettings
	{
		public string nickName = "Player";
		public byte team;
		public Color color = Color.white;
		public bool isAI;
		public FactionData selectedFaction;
		[Range(0, 100000)] public int startMoneyForSingleplayer = 10000;

		public PlayerEntry playerLobbyEntry;

		public PlayerSettings(byte team, Color color, bool isAI = false)
		{
			this.team = team;
			this.color = color;
			this.isAI = isAI;
		}
	}
}