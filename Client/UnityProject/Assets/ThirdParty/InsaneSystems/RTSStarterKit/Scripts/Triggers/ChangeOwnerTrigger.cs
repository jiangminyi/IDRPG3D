using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public class ChangeOwnerTrigger : TriggerBase
	{
		[SerializeField] List<Unit> unitsToChangeOwner = new List<Unit>();
		[SerializeField] [Range(0, 15)] int newPlayerOwner = 0;

		protected override void ExecuteAction()
		{
			for (int i = 0; i < unitsToChangeOwner.Count; i++)
				unitsToChangeOwner[i].SetOwner((byte)newPlayerOwner);
		}
	}
}