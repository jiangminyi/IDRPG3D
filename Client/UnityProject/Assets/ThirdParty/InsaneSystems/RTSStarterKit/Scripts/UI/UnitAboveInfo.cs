using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	/// <summary>
	/// This class describes component, which allows to draw more info above selected unit (like healthbar etc).
	/// Instantiates when healthbar being instantiated.
	/// </summary>
	public class UnitAboveInfo : MonoBehaviour
	{
		[SerializeField] Text selectionGroupText;
		[SerializeField] GameObject lockedIconObject;
		[SerializeField] List<CarryCell> carryCells = new List<CarryCell>();

		Unit selfUnit;
		float updateTimer;

		Healthbar selfHealthbar;

		void Awake()
		{
			selectionGroupText.enabled = false;
			selfHealthbar = GetComponent<Healthbar>();
			
			for (var i = 0; i < carryCells.Count; i++)
				carryCells[i].SetActive(false);
			
			lockedIconObject.SetActive(false);
		}

		void Update()
		{
			updateTimer -= Time.deltaTime;

			if (updateTimer <= 0)
			{
				if (selfHealthbar && (!selfUnit || selfUnit != selfHealthbar.damageable.selfUnit))
					SetupWithUnit(selfHealthbar.damageable.selfUnit);

				lockedIconObject.SetActive(selfUnit.isMovementLockedByHotkey);
				UpdateText();
				UpdateCarryCells();
				updateTimer = 0.2f;
			}
		}

		public void SetupWithUnit(Unit unit)
		{
			selfUnit = unit;

			UpdateText();
			UpdateCarryCells();
		}

		void UpdateText()
		{
			if (!selfUnit)
			{ 
				selectionGroupText.enabled = false;
				return;
			}

			if (selfUnit.unitSelectionGroup > -1)
			{
				selectionGroupText.enabled = true;
				selectionGroupText.text = selfUnit.unitSelectionGroup.ToString();
			}
			else
			{
				selectionGroupText.enabled = false;
			}
		}

		void UpdateCarryCells()
		{
			var carrierModule = selfUnit.GetModule<CarryModule>();
			
			for (var i = 0; i < carryCells.Count; i++)
			{
				carryCells[i].SetActive(selfUnit.data.canCarryUnitsCount > i);
				
				if (carrierModule && carrierModule.carryingUnits.Count > i)
					carryCells[i].UpdateState(carrierModule.carryingUnits[i]);
				else
					carryCells[i].UpdateState(null);
			}
		}
	}
}