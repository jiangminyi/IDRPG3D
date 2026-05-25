using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class UIController : MonoBehaviour
	{
		public static UIController instance { get; protected set; }

		[SerializeField] Canvas mainCanvas;
		[SerializeField] Text moneyText;
		[SerializeField] GameObject winScreen;
		[SerializeField] GameObject loseScreen;

		public Minimap minimapComponent { get; protected set; }
		public ProductionHint productionHint { get; protected set; }
		public SelectProductionNumberPanel selectProductionNumberPanel { get; protected set; }
		public MinimapSignal minimapSignal { get; protected set; }
		public CarryingUnitList carryingUnitList { get; protected set; }

		SelectProductionTypePanel selectProductionPanel;
		
		public Canvas MainCanvas
		{
			get { return mainCanvas; }
		}

		private void Awake()
		{
			instance = this;
			minimapComponent = GetComponent<Minimap>();
			productionHint = GetComponent<ProductionHint>();
			selectProductionPanel = GetComponent<SelectProductionTypePanel>();
			selectProductionNumberPanel = GetComponent<SelectProductionNumberPanel>();
			minimapSignal = GetComponent<MinimapSignal>();
			carryingUnitList = GetComponent<CarryingUnitList>();
		}

		void Start()
		{
			Unit.unitDestroyedEvent += Healthbar.RemoveHealthbarOfUnit;
			Controls.Selection.unitSelected += Healthbar.SpawnHealthbarForUnit;
			Controls.Selection.selectionCleared += Healthbar.RemoveAllHealthbars;
			
			Unit.unitHoveredEvent += OnUnitHovered;
			Unit.unitUnhoveredEvent += OnUnitUnhovered;

			OnPlayerMoneyChanged(GameController.instance.playersController.playersIngame[Player.localPlayerId].money);
			
			Player.localPlayerMoneyChangedEvent += OnPlayerMoneyChanged;
		}

		void OnUnitHovered(Unit unit)
		{
			if (unit.isSelected || !unit.GetModule<FogOfWarModule>().isVisibleInFOW)
				return;
			
			Healthbar.SpawnHealthbarForUnit(unit);
		}
		
		void OnUnitUnhovered(Unit unit)
		{
			if (unit.isSelected)
				return;
			
			Healthbar.RemoveHealthbarOfUnit(unit);
		}
		
		public void OnGameInitialized()
		{
			selectProductionPanel.OnSelectButtonClick(GameController.instance.MainStorage.availableProductionCategories[0]);
		}

		public void ShowWinScreen() { winScreen.SetActive(true); }
		public void ShowDefeatScreen() { loseScreen.SetActive(true); }

		void OnPlayerMoneyChanged(int moneyValue) { moneyText.text = "$" + moneyValue; }

		void OnDestroy()
		{
			Unit.unitDestroyedEvent -= Healthbar.RemoveHealthbarOfUnit;
			Player.localPlayerMoneyChangedEvent -= OnPlayerMoneyChanged;

			Controls.Selection.unitSelected -= Healthbar.SpawnHealthbarForUnit;
			Controls.Selection.selectionCleared -= Healthbar.RemoveAllHealthbars;
			
			Unit.unitHoveredEvent -= Healthbar.SpawnHealthbarForUnit;
			Unit.unitUnhoveredEvent -= Healthbar.RemoveHealthbarOfUnit;
		}
	}
}