using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionHint : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Text nameText;
		[SerializeField] Text descriptionText;

		RectTransform rectTransform;

		void Start()
		{
			rectTransform = selfObject.GetComponent<RectTransform>();

			Hide();
		}

		public void Show(UnitData unitData, Vector2 position)
		{
			selfObject.SetActive(true);

			rectTransform.anchoredPosition = position;
			nameText.text = unitData.textId;

			string electricityText = "";

			var textLibrary = GameController.instance.textsLibrary;

			if (GameController.instance.MainStorage.isElectricityUsedInGame)
			{
				if (unitData.addsElectricity > 0)
					electricityText = textLibrary.GetUITextById("addsElectricity") + ": " + unitData.addsElectricity + "\n";

				if (unitData.usesElectricity > 0)
					electricityText = textLibrary.GetUITextById("usesElectricity") + ": " + unitData.usesElectricity + "\n";
			}

			descriptionText.text = textLibrary.GetUITextById("price") + ": " + unitData.price + "\n" + electricityText + textLibrary.GetUITextById("buildTime") + ": " + unitData.buildTime + "s";

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)descriptionText.transform); // needed for content size fitter ## необходимо для того, чтобы Content Size Fitter обновлял размер
		}

		public void ShowForAbility(Abilities.AbilityData abilityData, Vector2 position)
		{
			selfObject.SetActive(true);

			rectTransform.anchoredPosition = position;
			nameText.text = abilityData.abilityName;
			descriptionText.text = "";

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)descriptionText.transform);
		}

		public void Hide()
		{
			selfObject.SetActive(false);
		}
	}
}