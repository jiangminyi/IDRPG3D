using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public class ProductionCategoriesEditor : DataListEditor
	{
		static List<ProductionCategory> loadedCategories = new List<ProductionCategory>();

		[MenuItem("RTS Starter Kit/Production Categories Editor", priority = 2)]
		static void Init()
		{
			ProductionCategoriesEditor window = (ProductionCategoriesEditor)EditorWindow.GetWindow(typeof(ProductionCategoriesEditor));
			window.titleContent = new GUIContent("RTS Production Categories Editor");
			window.maxSize = new Vector2(1024f, 600f);
			window.minSize = window.maxSize;
			window.Show();

			if (loadedCategories.Count <= selectedDataId)
				selectedDataId = 0;

			LoadCategories();
		}

		public static void LoadCategories()
		{
			loadedCategories.Clear();

			EditorExtensions.LoadAssetsToList(loadedCategories, "t:productioncategory");

			loadedDatasCount = loadedCategories.Count;
		}

		protected override void DrawList()
		{
			for (int i = 0; i < loadedCategories.Count; i++)
			{
				int cachedIndex = i;

				GUILayout.BeginHorizontal();

				if (selectedDataId == i)
					GUI.color = InsaneEditorStyles.selectedButtonColor;

				var buttonContent = new GUIContent(loadedCategories[i].textId);
				if (loadedCategories[i].icon)
					buttonContent.image = loadedCategories[i].icon.texture;

				if (GUILayout.Button(buttonContent, GUILayout.MaxHeight(24)))
					selectedDataId = cachedIndex;

				GUI.color = Color.white;
				if (GUILayout.Button("X", GUILayout.MaxHeight(24), GUILayout.MaxWidth(24)))
					RemoveDataWindow.Init(AssetDatabase.GetAssetPath(loadedCategories[i]), loadedCategories[i], this);

				GUILayout.EndHorizontal();
			}
		}

		protected override void DrawCreateButton()
		{
			if (GUILayout.Button("Create new category"))
				CreateNewCategory();
		}

		protected override string GetEditorTitle()
		{
			if (selectedDataId < loadedCategories.Count)
				return loadedCategories[selectedDataId].textId;

			return "Empty";
		}

		void CreateNewCategory(ProductionCategory categoryToClone = null)
		{
			if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				AssetDatabase.CreateFolder("Assets", "Resources");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
				AssetDatabase.CreateFolder("Assets/Resources", "Data");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/ProductionCategories"))
				AssetDatabase.CreateFolder("Assets/Resources/Data", "ProductionCategories");

			var category = ScriptableObject.CreateInstance(typeof(ProductionCategory)) as ProductionCategory;
			category.textId = "New Category";

			if (categoryToClone)
			{
				category = Instantiate(categoryToClone);
				category.textId += "_Clone";
			}

			AssetDatabase.CreateAsset(category, "Assets/Resources/Data/ProductionCategories/Category" + (loadedCategories.Count + 1) + "_" + category.textId + ".asset");

			LoadCategories();
			SelectCategory(category);
		}

		protected override void DrawEditor()
		{
			if (selectedDataId >= loadedCategories.Count)
				return;

			dataEditorScrollPos = EditorGUILayout.BeginScrollView(dataEditorScrollPos);
			var productionCategoryEditor = Editor.CreateEditor(loadedCategories[selectedDataId]);
			productionCategoryEditor.OnInspectorGUI();
			EditorGUILayout.EndScrollView();
		}

		public void SelectCategory(ProductionCategory category)
		{
			for (int i = 0; i < loadedCategories.Count; i++)
				if (category == loadedCategories[i])
					selectedDataId = i;
		}

		protected override void DrawActions()
		{
			if (selectedDataId >= loadedCategories.Count)
				return;

			if (GUILayout.Button("Clone production category"))
				CreateNewCategory(loadedCategories[selectedDataId]);

			if (GUILayout.Button("Delete production category"))
				RemoveDataWindow.Init(AssetDatabase.GetAssetPath(loadedCategories[selectedDataId]), loadedCategories[selectedDataId], this);
		}

		public override void ReloadDatas() { LoadCategories(); }
	}
}