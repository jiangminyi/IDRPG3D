using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	// obsolete?
	public class AbilitiesList : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Transform abilitiesPanel;

		readonly List<GameObject> drawnIcons = new List<GameObject>();

		Unit selectedUnit;

		void Start()
		{
			Controls.Selection.unitSelected += OnUnitSelected;
			Controls.Selection.selectionCleared += OnClearSelection;
		}

		public void OnClearSelection() { Hide(); }

		public void OnUnitSelected(Unit unit)
		{
			selectedUnit = unit;

			Redraw();
		}

		public void Redraw()
		{
			var abilitiesModule = selectedUnit.GetModule<AbilitiesModule>();

			if (!abilitiesModule || abilitiesModule.abilities.Count == 0)
				return;

			Show();
			ClearDrawn();

			var iconTemplate = GameController.instance.MainStorage.unitAbilityIcon;

			for (int i = 0; i < abilitiesModule.abilities.Count; i++)
			{
				GameObject iconObject = null;

				iconObject = Instantiate(iconTemplate, abilitiesPanel);

				var spawnedIconComponent = iconObject.GetComponent<AbilityIcon>();
				spawnedIconComponent.Setup(abilitiesModule.abilities[i]);

				drawnIcons.Add(iconObject);
			}
		}

		void ClearDrawn()
		{
			for (int i = 0; i < drawnIcons.Count; i++)
				if (drawnIcons[i] != null)
					Destroy(drawnIcons[i]);

			drawnIcons.Clear();
		}

		void Show() { selfObject.SetActive(true); }
		void Hide() { selfObject.SetActive(false); }

		void OnDestroy()
		{
			Controls.Selection.unitSelected -= OnUnitSelected;
			Controls.Selection.selectionCleared -= OnClearSelection;
		}
	}
}