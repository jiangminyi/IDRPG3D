using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public class ProductionAddUnitTrigger : TriggerBase
	{
		[Tooltip("List of productions, which will build units.")]
		[SerializeField] List<Production> productionsToPlaceOrder = new List<Production>();
		[Tooltip("Units, which will be added to production building queue.")]
		[SerializeField] List<UnitData> unitDatasToAddInOrder = new List<UnitData>();

		protected override void ExecuteAction()
		{
			for (int i = 0; i < productionsToPlaceOrder.Count; i++)
				for (int k = 0; k < unitDatasToAddInOrder.Count; k++)
					productionsToPlaceOrder[i].AddUnitToQueue(unitDatasToAddInOrder[k]);
		}
	}
}