using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary>BuildingData is DEPRECTATED. Use UnitData instead.</summary>
	[CreateAssetMenu(fileName = "BuildingData", menuName = "RTS Starter Kit/(Deprecated) Building Data")]
	public class BuildingData : UnitData
	{
		void OnEnabled()
		{
			isBuilding = true;
			Debug.LogWarning("BuildingData is DEPRECTATED. Use UnitData instead.");
		}
	}
}