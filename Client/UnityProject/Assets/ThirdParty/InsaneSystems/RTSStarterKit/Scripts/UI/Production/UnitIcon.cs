using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class UnitIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField] Image fillImage;
		[SerializeField] Image iconImage;
		[SerializeField] Button button;
		[SerializeField] Text countText;

		UnitData unitDataTemplate;

		ProductionIconsPanel selfProductionIconsPanel;

		RectTransform rectTransform;

		void Start() { rectTransform = GetComponent<RectTransform>(); }
		void Update() { Redraw(); }

		public void Redraw()
		{
			var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;
			bool isBuilding = IsBuildingType();
			bool isInProductionQueue = selectedProduction.IsUnitOfTypeInQueue(unitDataTemplate);
			if (!selectedProduction)
				return;

			iconImage.sprite = unitDataTemplate.icon;

			if (selectedProduction.IsUnitOfTypeCurrentlyBuilding(unitDataTemplate))
				fillImage.fillAmount = 1f - selectedProduction.GetBuildProgressPercents();
			else if ((isBuilding && isInProductionQueue) || (!isBuilding && isInProductionQueue))
				fillImage.fillAmount = 1f;
			else
				fillImage.fillAmount = 0f;

			int unitsCount = selectedProduction.GetUnitsOfSpecificTypeInQueue(unitDataTemplate);
			countText.text = unitsCount > 0 ? unitsCount.ToString() : "";

			if (isBuilding && IsAnyBuildingInqueue(selectedProduction))
				SetActive(IsCurrentBuildingInqueue(selectedProduction));
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				OnClick();
			else if (eventData.button == PointerEventData.InputButton.Right)
				OnRightClick();
		}

		public void OnClick()
		{
			var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;

			if (!selectedProduction)
				return;

			if (IsBuildingType())
			{
				bool isBuildingReady = IsCurrentBuildingInqueue(selectedProduction) && selectedProduction.IsBuildingReady();

				if (IsAnyBuildingInqueue(selectedProduction))
				{
					if (isBuildingReady)
						GameController.instance.build.EnableBuildMode(unitDataTemplate.selfPrefab, () =>
						{
							selectedProduction.FinishBuilding();
							selfProductionIconsPanel.Redraw();
						});
					
					return;
				}
			}
			
			selectedProduction.AddUnitToQueue(unitDataTemplate);
		}

		void OnRightClick()
		{
			var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;

			if (!selectedProduction)
				return;

			selectedProduction.RemoveUnitFromQueue(unitDataTemplate, true);

			selfProductionIconsPanel.Redraw();
		}

		bool IsAnyBuildingInqueue(Production production) { return production.unitsQueue.Count > 0; }

		bool IsCurrentBuildingInqueue(Production production)
		{
			return production.unitsQueue.Count > 0 && production.unitsQueue[0] == unitDataTemplate;
		}

		bool IsBuildingType()
		{
			return SelectProductionTypePanel.selectedProductionCategory.isBuildings;
		}

		public void SetActive(bool value) { button.interactable = value; }
		
		public void SetupWithUnitData(ProductionIconsPanel selfPanel, UnitData unitData)
		{
			selfProductionIconsPanel = selfPanel;
			unitDataTemplate = unitData;
			Redraw();
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			UIController.instance.productionHint.Show(unitDataTemplate, rectTransform.position + new Vector3(0, rectTransform.sizeDelta.y / 2f + 10));
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			UIController.instance.productionHint.Hide();
		}
	}
}