using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Refinery : MonoBehaviour
	{
		[SerializeField] Transform carryOutResourcesPoint;
		
		[Tooltip("Harvester unit data which will be spawned on this refinery at start.")]
		[SerializeField] UnitData harversterUnitData;

		public Transform CarryOutResourcesPoint { get { return carryOutResourcesPoint; } }

		public Unit selfUnit { get; protected set; }

		void Start()
		{
			selfUnit = GetComponent<Unit>();
			SpawnHarvester();
		}

		public void AddResources(int amount)
		{
			GameController.instance.playersController.playersIngame[selfUnit.OwnerPlayerId].AddMoney(amount);
		}

		void SpawnHarvester()
		{
			var spawnedHarvester = SpawnController.SpawnUnit(harversterUnitData, selfUnit.OwnerPlayerId, carryOutResourcesPoint);
			spawnedHarvester.GetComponent<Harvester>().SetRefinery(this);
		}
	}
}