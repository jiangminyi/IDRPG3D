using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class ElectricityModule : Module
	{
		int addsElectricity, neededElectricity;

		protected override void Awake()
		{
			base.Awake();

			addsElectricity = selfUnit.data.addsElectricity;
			neededElectricity = selfUnit.data.usesElectricity;

			Unit.unitSpawnedEvent += OnBuildingComplete;
		}

		void Start()
		{
			selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
		}

		public void OnBuildingComplete(Unit unit)
		{
			if (unit != selfUnit)
				return;

			Player.GetPlayerById(selfUnit.OwnerPlayerId).AddElectricity(addsElectricity);
			Player.GetPlayerById(selfUnit.OwnerPlayerId).AddUsedElectricity(neededElectricity);
		}

		public void OnDie(Unit unit)
		{
			if (unit != selfUnit)
				return;

			Player.GetPlayerById(selfUnit.OwnerPlayerId).RemoveElectricity(addsElectricity);
			Player.GetPlayerById(selfUnit.OwnerPlayerId).RemoveUsedElectricity(neededElectricity);
		}

		public void IncreaseAddingElectricity(int addToAdding)
		{
			addsElectricity += addToAdding;
			Player.GetPlayerById(selfUnit.OwnerPlayerId).AddElectricity(addToAdding);
		}

		private void OnDestroy()
		{
			Unit.unitSpawnedEvent -= OnBuildingComplete;
		}
	}
}