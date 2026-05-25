using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class DebugCode : MonoBehaviour
	{
		[SerializeField] UnitData unitDataForQueue;

		void Start()
		{
			var prodModule = GetComponent<Production>();

			if (prodModule)
			{
				prodModule.AddUnitToQueue(unitDataForQueue);
				prodModule.AddUnitToQueue(unitDataForQueue);
			}
		}
	}
}