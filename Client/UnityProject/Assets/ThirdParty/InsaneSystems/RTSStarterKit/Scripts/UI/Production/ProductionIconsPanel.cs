using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionIconsPanel : MonoBehaviour
	{
		[SerializeField] RectTransform iconsPanel;
	
		List<GameObject> spawnedIcons = new List<GameObject>();

		private void Awake()
		{
			SelectProductionNumberPanel.selectedProductionChanged += ProductionChangedAction;
		}

		void ProductionChangedAction(Production production) { Redraw(); }

		public void Redraw()
		{
			ClearDrawn();

			var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;

			if (selectedProduction == null)
				return;
		
			for (int i = 0; i < selectedProduction.AvailableUnits.Count; i++)
			{
				GameObject spawnedObject = Instantiate(GameController.instance.MainStorage.unitProductionIconTemplate, iconsPanel);
				var unitIconComponent = spawnedObject.GetComponent<UnitIcon>();

				unitIconComponent.SetupWithUnitData(this, selectedProduction.AvailableUnits[i]);
				
				if (SelectProductionTypePanel.selectedProductionCategory.isBuildings && selectedProduction.unitsQueue.Count > 0)
				{
					bool isCurrentBuildingInQueue = selectedProduction.unitsQueue[0] != selectedProduction.AvailableUnits[i];
					
					unitIconComponent.SetActive(isCurrentBuildingInQueue);
				}

				spawnedIcons.Add(spawnedObject);
			}
		}

		void ClearDrawn()
		{
			for (int i = 0; i < spawnedIcons.Count; i++)
				Destroy(spawnedIcons[i]);

			spawnedIcons.Clear();
		}

		void OnDestroy()
		{
			SelectProductionNumberPanel.selectedProductionChanged -= ProductionChangedAction;
		}
	}
}