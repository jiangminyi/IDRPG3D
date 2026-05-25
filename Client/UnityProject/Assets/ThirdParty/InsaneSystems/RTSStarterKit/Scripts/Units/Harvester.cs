using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Harvester : Module
	{
		const float randomFieldDistance = 2f;
		const float sqrRandomFieldDistance = 24f;

		public enum HarvestState
		{
			MoveToField,
			Harvest,
			MoveToRefinery,
			CarryOutResources,
			Idle
		}

		public event HarvesterResourcesChanged harvesterResourcesChangedEvent;

		[SerializeField] int maxResources = 600;
		[SerializeField] float harvestTime = 5f;
		[SerializeField] float carryOutTime = 3f;
		
		public int MaxResources { get { return maxResources; } }
		public int harvestedResources { get; protected set; }

		HarvestState harvestState;

		Refinery nearestRefinery;
		ResourcesField resourcesField;
		float recheckTimer = 1f;

		float harvestTimeLeft;
		float carryOutTimeLeft;

		int addedToRefineryResources;

		public delegate void HarvesterResourcesChanged(float newValue, float maxValue);

		void Start()
		{
			selfUnit.unitReceivedOrderEvent += OnUnitReceivedOrder;

			//if (Player.localPlayerId == selfUnit.OwnerPlayerId)
			//	UI.HarvesterBar.SpawnForHarvester(this);

			if (harvesterResourcesChangedEvent != null)
				harvesterResourcesChangedEvent.Invoke(0, maxResources);
		}

		void Update()
		{
			if (!nearestRefinery)
			{
				SearchNearestRefinery();

				return;
			}

			if (!resourcesField)
			{
				SearchNearestResourcesField();

				return;
			}

			switch (harvestState)
			{
				case HarvestState.MoveToField:
					if ((transform.position - resourcesField.transform.position).sqrMagnitude < sqrRandomFieldDistance)
						SetHarvestState(HarvestState.Harvest);
					break;

				case HarvestState.Harvest:
					harvestTimeLeft -= Time.deltaTime;
					harvestedResources = (int)Mathf.Lerp(0, maxResources, 1f - harvestTimeLeft / harvestTime);

					if (harvesterResourcesChangedEvent != null)
						harvesterResourcesChangedEvent.Invoke(harvestedResources, maxResources);

					if (harvestTimeLeft <= 0)
					{
						harvestTimeLeft = 0f;
						harvestedResources = maxResources;

						SetHarvestState(HarvestState.MoveToRefinery);
					}
					break;

				case HarvestState.MoveToRefinery:
					if ((transform.position - nearestRefinery.CarryOutResourcesPoint.position).sqrMagnitude < 8f)
						SetHarvestState(HarvestState.CarryOutResources);
					break;

				case HarvestState.CarryOutResources:
					carryOutTimeLeft -= Time.deltaTime;

					if (carryOutTimeLeft <= 0)
					{
						carryOutTimeLeft = 0f;
				
						nearestRefinery.AddResources(harvestedResources);
						harvestedResources = 0;

						if (harvesterResourcesChangedEvent != null)
							harvesterResourcesChangedEvent.Invoke(harvestedResources, maxResources);

						SetHarvestState(HarvestState.MoveToField);
					}
					break;
			}
		}

		void SearchNearestRefinery()
		{
			if (recheckTimer > 0)
			{
				recheckTimer -= Time.deltaTime;
			}
			else
			{
				var allRefineries = new List<Refinery>(FindObjectsOfType<Refinery>());
				allRefineries = allRefineries.FindAll(refinery => refinery.selfUnit.OwnerPlayerId == selfUnit.OwnerPlayerId);

				float distance = float.MaxValue - 1f;

				for (int i = 0; i < allRefineries.Count; i++)
				{
					float curDistance = (transform.position - allRefineries[i].transform.position).sqrMagnitude;

					if (curDistance < distance)
					{
						nearestRefinery = allRefineries[i];
						distance = curDistance;
					}
				}

				recheckTimer = 1f;
			}
		}

		void SearchNearestResourcesField()
		{
			if (recheckTimer > 0)
			{
				recheckTimer -= Time.deltaTime;
			}
			else
			{
				var allFields = FindObjectsOfType<ResourcesField>();
				float distance = float.MaxValue - 1f;

				for (int i = 0; i < allFields.Length; i++)
				{
					float curDistance = (transform.position - allFields[i].transform.position).sqrMagnitude;

					if (curDistance < distance)
					{
						resourcesField = allFields[i];
						distance = curDistance;
					}
				}
			
				if (resourcesField)
					SetHarvestState(HarvestState.MoveToField);

				recheckTimer = 1f;
			}
		}

		public void SetHarvestState(HarvestState newState)
		{
			harvestState = newState;

			switch (harvestState)
			{
				case HarvestState.MoveToField:
					var order = new MovePositionOrder();
					order.executor = selfUnit;
					order.movePosition = resourcesField.transform.position + new Vector3(Random.Range(-randomFieldDistance, randomFieldDistance), 0, Random.Range(-randomFieldDistance, randomFieldDistance)); // todo change to proportion of resource field size
					selfUnit.AddOrder(order, false, isReceivedEventNeeded: false);
					break;

				case HarvestState.Harvest:
					harvestTimeLeft = harvestTime;
					break;

				case HarvestState.MoveToRefinery:
					var orderBack = new MovePositionOrder();
					orderBack.movePosition = nearestRefinery.CarryOutResourcesPoint.position;
					selfUnit.AddOrder(orderBack, false, isReceivedEventNeeded: false);
					break;

				case HarvestState.CarryOutResources:
					carryOutTimeLeft = carryOutTime;
					addedToRefineryResources = 0;
					break;
			}
		}

		public void OnUnitReceivedOrder(Unit unit, Order order)
		{
			if (order is MovePositionOrder)
			{
				var position = (order as MovePositionOrder).movePosition;

				Collider[] nearestObjects = Physics.OverlapSphere(position, 7f);

				for (int i = 0; i < nearestObjects.Length; i++)
				{
					var field = nearestObjects[i].GetComponent<ResourcesField>();

					if (field)
					{
						resourcesField = field;
						SetHarvestState(HarvestState.MoveToField);

						return;
					}
				}
			}
			else if (order is FollowOrder)
			{
				var target = (order as FollowOrder).followTarget;

				var refinery = target.GetComponent<Refinery>();

				if (refinery)
				{
					SetRefinery(refinery);

					if (harvestedResources > 0)
						SetHarvestState(HarvestState.MoveToRefinery);

					return;
				}
			}

			SetHarvestState(HarvestState.Idle);
		}

		public void SetRefinery(Refinery refinery) { nearestRefinery = refinery; }
	}
}