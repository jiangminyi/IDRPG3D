using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "Storage", menuName = "RTS Starter Kit/Storage")]
	public class Storage : ScriptableObject
	{
		[Header("Game Objects")]
		[Tooltip("This is building object, which will be spawned for every player at game start")]
		public GameObject defaultCommandCenterObject;
		public GameObject selectionIndicatorTemplate;
		public GameObject moveOrderEffect;
		public GameObject attackOrderEffect;

		[Header("UI Templates")]
		public GameObject unitMinimapIconTemplate;
		public GameObject healthbarTemplate;
		public GameObject productionButtonTemplate;
		public GameObject productionNumberButtonTemplate;
		public GameObject unitProductionIconTemplate;
		public GameObject unitMultiselectionIconTemplate;
		public GameObject harvesterBarTemplate;
		public GameObject minimapSignalTemplate;
		public GameObject unitCarryingIcon;
		public GameObject unitAbilityIcon;

		[Header("Cursors")]
		public Texture2D defaultCursour;
		public Texture2D attackCursour;
		public Texture2D gatherResourcesCursour;
		public Texture2D giveResourcesCursour;
		public Texture2D restrictCursour;
		public Texture2D mapOrderCursor;

		[Header("Menu UI Templates")]
		public GameObject playerEntry;

		[Header("Default Game Settings")]
		[Tooltip("Here you should add all created Map Settings objects, otherwise map don't appear in maps list ingame.")]
		public List<MapSettings> availableMaps;
		public List<ProductionCategory> availableProductionCategories;
		public List<FactionData> availableFactions;
		[Range(0, 100000)] public int startPlayerMoney = 10000;
		[Tooltip("This field contains maximum building distance. Player will be able to create buildings only in this radius from start point.")]
		[Range(10, 1000)] public int maxBuildDistance = 40;
		public bool allowBuildingsRotation = true;
		public bool useGridForBuildingMode = true;
		public bool allowCameraRotation = true;
		public bool allowCameraZoom = true;
		[Tooltip("This parameter means, will be visible black borders outside the map bounds or not.")]
		public bool showMapBorders = true;
		[Tooltip("If you don't need automatic NavMeshObstacle component addition to your buildings, turn this off.")]
		public bool addNavMeshObstacleToBuildings = true;
		[Tooltip("Used units formation type. Default formation keeps units positions same as it was before order, Square Predict is better for square formations.")]
		public UnitsFormation unitsFormation = UnitsFormation.Default;

		[Header("Misc")]
		public Material playerColorMaterialTemplate;

		[Tooltip("Max count of units icons in multiselection interface panel. 0 for no limit. Note that high limit values or 0 can cause some lags on huge units count selection.")]
		[Range(0, 80)] public int unitIconsLimitInMultiselectionUI = 20;

		[Tooltip("List of all available for player colors. You can add new colors or remove existing.")]
		public List<Color> availablePlayerColors;
		public SoundLibrary soundLibrary;
		public TextsLibrary textsLibrary;
		[Sound] public AudioClip testSound;

		[Header("Layers")]
		public LayerMask unitLayerMask;
		[Tooltip("List of layers which will be obstacle for shooting units when aiming target.")]
		public LayerMask obstaclesToUnitShoots;
		[Tooltip("List of layers which will be obstacle for shooting units when aiming target.")]
		public LayerMask obstaclesToUnitShootsWithoutUnitLayer;

		[Header("Gameplay - Electricity")]
		[Tooltip("Check this, if your game uses electricity 'model' of gameplay. It means that some buildings uses electricity to work, and there exists some powerplants which gives electricity.")]
		public bool isElectricityUsedInGame;
		[Tooltip("Speed decrease value when electricity limit is reached. If you set 1, there will be original production speed (100%), if you set 0.5, it will be 50%. So set 0 to pause production until electricity will be restored.")]
		[Range(0f, 1f)] public float speedCoefForProductionsWithoutElectricity = 1f;

		[Header("Gameplay - Fog of War")]
		[Tooltip("Is For of War used in the game? If yes, check this toggle. Note that fog of war can be expensive for performance in games with big units count.")]
		public bool isFogOfWarOn = true;
		[Tooltip("Delay between updates of fog of war visual part. Smaller values can cause bad performance, but better quality.")]
		[Range(0f, 0.5f)] public float fowUpdateDelay = 0.05f;
		
		public MapSettings GetMapBySceneName(string name)
		{
			for (int i = 0; i < availableMaps.Count; i++)
				if (availableMaps[i].mapSceneName == name)
					return availableMaps[i];

			throw new System.Exception("No map with name " + name + " found!");
		}
	}

	public enum UnitsFormation
	{
		Default,
		SquarePredict
	}
}