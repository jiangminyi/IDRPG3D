using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionTypeButton : MonoBehaviour
	{
		[SerializeField] ProductionCategory productionCategory;
		
		SelectProductionTypePanel controllerPanel;

		Image selfImage;
		Button selfButton;
		Color defaultColor;

		public ProductionCategory GetProductionCategory
		{
			get { return productionCategory; }
		}

		void Awake()
		{
			selfImage = GetComponent<Image>();
			selfButton = GetComponent<Button>();
			defaultColor = selfImage.color;
		}

		void Start() { Redraw(); }

		public void SetActive() { selfImage.color = Color.green; }
		public void SetUnactive() { selfImage.color = defaultColor; }
		public void OnClick() { controllerPanel.OnSelectButtonClick(productionCategory); }

		public void Redraw()
		{
			selfButton.interactable = Player.GetLocalPlayer().IsHaveProductionOfCategory(productionCategory);
		}

		public void SetupWithController(SelectProductionTypePanel typePanel) { controllerPanel = typePanel; }

		public void SetupWithProductionCategory(ProductionCategory category)
		{
			productionCategory = category;

			selfImage.sprite = category.icon;

			Redraw();
		}
	}
}