using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public class MoveOrderTrigger : TriggerBase
	{
		[SerializeField] List<Unit> unitsToAddOrder = new List<Unit>();
		[SerializeField] Transform destinationWaypointTransform;

		protected override void ExecuteAction()
		{
			var order = new MovePositionOrder();
			order.movePosition = destinationWaypointTransform.position;

			Controls.Ordering.SendOrderToUnits(unitsToAddOrder, order, false);
		}
	}
}