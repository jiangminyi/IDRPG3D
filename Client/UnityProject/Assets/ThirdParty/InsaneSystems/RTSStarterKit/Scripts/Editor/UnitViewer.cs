using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public class UnitViewer : DataListEditor
	{
		static readonly List<UnitData> loadedUnitDatas = new List<UnitData>();

		[MenuItem("RTS Starter Kit/Unit Editor", priority = 1)]
		static void Init()
		{
			var window = (UnitViewer)EditorWindow.GetWindow(typeof(UnitViewer));
			window.maxSize = new Vector2(1024f, 600f);
			window.minSize = window.maxSize;
			window.titleContent = new GUIContent("RTS Unit Editor");
			window.Show();

			LoadUnitDatas();
		}

		static void LoadUnitDatas()
		{
			loadedUnitDatas.Clear();

			EditorExtensions.LoadAssetsToList(loadedUnitDatas, "t:unitdata");

			loadedDatasCount = loadedUnitDatas.Count;
		}

		protected override void DrawList()
		{
			for (int i = 0; i < loadedUnitDatas.Count; i++)
			{
				int cachedIndex = i;

				GUILayout.BeginHorizontal();

				if (selectedDataId == i)
					GUI.color = InsaneEditorStyles.selectedButtonColor;

				var buttonContent = new GUIContent(" " + loadedUnitDatas[i].textId); // space needed to step from icon
				if (loadedUnitDatas[i].icon)
					buttonContent.image = loadedUnitDatas[i].icon.texture;
				
				if (GUILayout.Button(buttonContent, GUILayout.MaxHeight(24)))
					selectedDataId = cachedIndex;

				GUI.color = Color.white;
				if (GUILayout.Button("X", GUILayout.MaxHeight(24), GUILayout.MaxWidth(24)))
					RemoveDataWindow.Init(AssetDatabase.GetAssetPath(loadedUnitDatas[i]), loadedUnitDatas[i], this);

				GUILayout.EndHorizontal();
			}
		}

		protected override void DrawCreateButton()
		{
			if (GUILayout.Button("Create new unit"))
				CreateNewUnit();
		}

		protected override string GetEditorTitle()
		{
			if (selectedDataId < loadedUnitDatas.Count)
				return loadedUnitDatas[selectedDataId].textId;

			return "Empty";
		}

		void CreateNewUnit(UnitData unitDataToClone = null)
		{
			if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				AssetDatabase.CreateFolder("Assets", "Resources");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
				AssetDatabase.CreateFolder("Assets/Resources", "Data");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/UnitsDatas"))
				AssetDatabase.CreateFolder("Assets/Resources/Data", "UnitsDatas");

			var unitData = ScriptableObject.CreateInstance(typeof(UnitData)) as UnitData;
			unitData.textId = "New Unit";

			if (unitDataToClone)
			{
				unitData = Instantiate(unitDataToClone);
				unitData.textId += "_Clone";
			}

			AssetDatabase.CreateAsset(unitData, "Assets/Resources/Data/UnitsDatas/Unit" + (loadedUnitDatas.Count + 1) + "_" + unitData.textId + ".asset");

			LoadUnitDatas();
			SelectUnit(unitData);
		}

		protected override void DrawEditor()
		{
			if (selectedDataId >= loadedUnitDatas.Count)
				return;

			dataEditorScrollPos = EditorGUILayout.BeginScrollView(dataEditorScrollPos);
			var unitDataEditor = Editor.CreateEditor(loadedUnitDatas[selectedDataId]);
			unitDataEditor.OnInspectorGUI();
			EditorGUILayout.EndScrollView();
		}

		protected override void DrawActions()
		{
			if (selectedDataId >= loadedUnitDatas.Count)
				return;

			bool isUnitDataSuitableForPrefab = loadedUnitDatas[selectedDataId].unitModel != null;

			if (!isUnitDataSuitableForPrefab)
				EditorGUILayout.HelpBox("Add unit model to Unit Data field to generate prefab.", MessageType.Info);
			else
				EditorGUILayout.HelpBox("Unit prefab will be generated and created on opened scene. Also it will be saved to assets.", MessageType.Info);

			GUI.enabled = isUnitDataSuitableForPrefab;

			if (GUILayout.Button("Generate unit prefab"))
				CreateUnitPrefab(loadedUnitDatas[selectedDataId]);

			GUI.enabled = true;

			if (GUILayout.Button("Clone unit"))
				CreateNewUnit(loadedUnitDatas[selectedDataId]);

			if (GUILayout.Button("Delete unit"))
				RemoveDataWindow.Init(AssetDatabase.GetAssetPath(loadedUnitDatas[selectedDataId]), loadedUnitDatas[selectedDataId], this);
		}

		void CreateUnitPrefab(UnitData unitData)
		{
			var unitGameObject = new GameObject("Unit" + unitData.textId);
			unitGameObject.layer = LayerMask.NameToLayer("Unit");

			var unitModule = unitGameObject.AddComponent<Unit>();
			unitModule.SetUnitData(unitData);

			var model = new GameObject("Model");
			model.transform.SetParent(unitGameObject.transform);
			model.transform.localPosition = Vector3.zero;

			var realModel = Instantiate(unitData.unitModel, model.transform);
			realModel.transform.localPosition = Vector3.zero;

			if (unitData.addsElectricity > 0 || unitData.usesElectricity > 0)
				unitGameObject.AddComponent<ElectricityModule>();

			if (!unitGameObject.GetComponent<BoxCollider>())
				unitGameObject.AddComponent<BoxCollider>();

			if (unitData.canBeDestroyed)
				unitGameObject.AddComponent<Damageable>();

			if (unitData.hasMoveModule)
				unitGameObject.AddComponent<Movable>();

			if (unitData.hasAttackModule)
			{
				var attackable = unitGameObject.AddComponent<Attackable>();
				var shootPoints = model.transform.GetAllChilds(true).FindAll(ch => ch.name.ToLower().StartsWith("shootpoint"));

				attackable.SetShootPoints(shootPoints);
			}

			if (unitData.hasTurret)
			{
				var turret = unitGameObject.AddComponent<Tower>();
				var turretTransform = model.transform.GetAllChilds(true).Find(ch => ch.name.ToLower().StartsWith("turretbone"));

				turret.SetTurretTransform(turretTransform);
			}

			if (unitData.isHarvester)
				unitGameObject.AddComponent<Harvester>();

			if (unitData.isRefinery)
				unitGameObject.AddComponent<Refinery>();

			if (unitData.isProduction)
			{
				for (var i = 0; i < unitData.productionCategories.Count; i++)
				{
					var production = unitGameObject.AddComponent<Production>();
					production.SetCategoryId(i);
					
					// todo find spawnpoint and waypoint bones and set it to prod
				}
			}

			if (unitData.isInfantry)
			{
				unitGameObject.AddComponent<Infantry>();
				var animator = unitGameObject.AddComponent<Animator>();

				if (unitData.animatorController)
					animator.runtimeAnimatorController = unitData.animatorController;
			}

			unitGameObject.AddComponent<FogOfWarModule>();

			if (unitData.canCarryUnitsCount > 0)
				unitGameObject.AddComponent<CarryModule>();

			var wheelsBones = model.transform.GetAllChilds(true).FindAll(ch => ch.name.ToLower().StartsWith("wheelbone"));

			if (wheelsBones.Count > 0)
			{
				var wheelsComponent = unitGameObject.AddComponent<Wheels>();
				wheelsComponent.SetupWheels(wheelsBones);
			}

			unitGameObject.AddComponent<AudioSource>();

			if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
				AssetDatabase.CreateFolder("Assets", "Prefabs");

			if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Units"))
				AssetDatabase.CreateFolder("Assets/Prefabs", "Units");

			// todo add support of 2019 etc - SaveAsAsset or what
			
			var prefab = PrefabUtility.CreatePrefab("Assets/Prefabs/Units/Unit" + unitData.textId + ".prefab", unitGameObject);
			PrefabUtility.ConnectGameObjectToPrefab(unitGameObject, prefab);
			//AssetDatabase.Refresh();

			unitData.selfPrefab = prefab; // PrefabUtility.GetCorrespondingObjectFromSource(unitGameObject) as GameObject;
			EditorUtility.SetDirty(unitData);
			
		}

		public void SelectUnit(UnitData unitData)
		{
			for (int i = 0; i < loadedUnitDatas.Count; i++)
				if (unitData == loadedUnitDatas[i])
					selectedDataId = i;
		}

		public override void ReloadDatas() { LoadUnitDatas(); }
	}
}