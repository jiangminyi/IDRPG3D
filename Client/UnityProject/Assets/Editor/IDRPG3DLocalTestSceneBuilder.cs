using System.IO;
using System.Linq;
using IDRPG3D.LocalTest;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IDRPG3D.EditorTools
{
    public static class IDRPG3DLocalTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/IDRPG3D_LocalTest.unity";
        private const string WorldRootName = "World_TestTerrain";
        private const string PolyartRoot = "Assets/ThirdParty/Blink/Tools/RPGBuilder/ThirdPartyAssets/Polyart";
        private const float RoadLength = 80f;
        private const float RoadWidth = 7f;

        [MenuItem("IDRPG3D/Local Test/Rebuild Scene", priority = 1)]
        public static void RebuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "IDRPG3D_LocalTest";

            var bootstrapObject = new GameObject("IDRPG3D_LocalTest_Bootstrap");
            bootstrapObject.AddComponent<IDRPG3DLocalTestBootstrap>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(bootstrapObject);

            Debug.Log($"IDRPG3D local test scene rebuilt: {ScenePath}");
        }

        [MenuItem("IDRPG3D/Local Test/Build Blink Road Terrain", priority = 2)]
        public static void BuildBlinkRoadTerrain()
        {
            OpenScene();
            ClearExistingWorldRoot();

            var root = new GameObject(WorldRootName);
            var grassMaterial = LoadMaterial($"{PolyartRoot}/Foliage/M_GrassMedium_01.mat", new Color(0.22f, 0.43f, 0.18f), "Grass_Ground_Mat");
            var roadMaterial = LoadMaterial($"{PolyartRoot}/Materials/M_RockFormation_04.mat", new Color(0.33f, 0.31f, 0.27f), "Stone_Road_Mat");

            CreateGround("Grass_Left", root.transform, new Vector3(0f, -0.03f, -8.25f), new Vector3(RoadLength, 0.12f, 9.5f), grassMaterial);
            CreateGround("Grass_Right", root.transform, new Vector3(0f, -0.03f, 8.25f), new Vector3(RoadLength, 0.12f, 9.5f), grassMaterial);
            CreateGround("Stone_Road", root.transform, Vector3.zero, new Vector3(RoadLength, 0.14f, RoadWidth), roadMaterial);

            AddRoadEdgeMarkers(root.transform);
            PopulateBlinkVegetation(root.transform);
            CreateSpawnMarkers(root.transform);
            SetupCameraAndLight();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            Debug.Log($"IDRPG3D Blink road terrain generated: {ScenePath}");
        }

        [MenuItem("IDRPG3D/Local Test/Open Scene", priority = 2)]
        public static void OpenScene()
        {
            if (!File.Exists(ScenePath))
            {
                RebuildScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("IDRPG3D/Local Test/Add Scene To Build Settings", priority = 3)]
        public static void AddSceneToBuildSettings()
        {
            if (!File.Exists(ScenePath))
            {
                RebuildScene();
            }

            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                if (scene.path == ScenePath)
                {
                    Debug.Log($"IDRPG3D local test scene already in build settings: {ScenePath}");
                    return;
                }
            }

            var next = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(next, 0);
            next[next.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = next;
            Debug.Log($"IDRPG3D local test scene added to build settings: {ScenePath}");
        }

        private static void ClearExistingWorldRoot()
        {
            var existing = GameObject.Find(WorldRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static void CreateGround(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = name;
            ground.transform.SetParent(parent);
            ground.transform.position = position;
            ground.transform.localScale = scale;

            var renderer = ground.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static Material LoadMaterial(string path, Color fallbackColor, string fallbackName)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            return material != null ? material : CreateMaterial(fallbackName, fallbackColor);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = name,
                color = color
            };
            return material;
        }

        private static void AddRoadEdgeMarkers(Transform parent)
        {
            var markerMaterial = CreateMaterial("Road_Edge_Marker_Mat", new Color(0.18f, 0.16f, 0.13f));
            for (var x = -36f; x <= 36f; x += 6f)
            {
                CreateEdgeMarker(parent, new Vector3(x, 0.08f, -RoadWidth * 0.5f), markerMaterial);
                CreateEdgeMarker(parent, new Vector3(x, 0.08f, RoadWidth * 0.5f), markerMaterial);
            }
        }

        private static void CreateEdgeMarker(Transform parent, Vector3 position, Material material)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Road_Edge_Stone";
            marker.transform.SetParent(parent);
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(1.4f, 0.18f, 0.22f);
            marker.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void PopulateBlinkVegetation(Transform parent)
        {
            var left = new GameObject("Left_Vegetation").transform;
            left.SetParent(parent);
            var right = new GameObject("Right_Vegetation").transform;
            right.SetParent(parent);

            var prefabs = new[]
            {
                $"{PolyartRoot}/Prefabs/Prefab_Bush_04.prefab",
                $"{PolyartRoot}/Prefabs/Prefab_Bush_05.prefab",
                $"{PolyartRoot}/Foliage/Prefab_Fern_01.prefab",
                $"{PolyartRoot}/Foliage/Prefab_Grass_Medium02.prefab",
                $"{PolyartRoot}/Foliage/Prefab_FlowerGroup_Yellow.prefab",
            }.Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(prefab => prefab != null).ToArray();
            var treePrefabs = new[]
            {
                $"{PolyartRoot}/Trees/Prefab_Conifer_01.prefab",
                $"{PolyartRoot}/Trees/Prefab_Conifer_07.prefab",
                $"{PolyartRoot}/Trees/Prefab_Conifer_08.prefab",
                $"{PolyartRoot}/Prefabs/Prefab_RPGTree_01.prefab",
                $"{PolyartRoot}/Prefabs/Prefab_RPGTree_02.prefab",
            }.Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(prefab => prefab != null).ToArray();

            if (prefabs.Length == 0)
            {
                Debug.LogWarning("No Blink Polyart vegetation prefabs found. Terrain ground was generated without decoration.");
                return;
            }

            var random = new System.Random(20260524);
            for (var x = -36f; x <= 36f; x += 4f)
            {
                CreateDecoration(prefabs, left, random, x, -RandomRange(random, 7.0f, 11.5f), 0.65f, 1.05f);
                CreateDecoration(prefabs, right, random, x + RandomRange(random, -1.2f, 1.2f), RandomRange(random, 6.5f, 10.5f), 0.75f, 1.2f);
                if (treePrefabs.Length > 0 && random.NextDouble() > 0.4d)
                {
                    CreateDecoration(treePrefabs, right, random, x + RandomRange(random, -1.5f, 1.5f), RandomRange(random, 10.5f, 14.5f), 0.8f, 1.35f);
                }
            }
        }

        private static void CreateDecoration(GameObject[] prefabs, Transform parent, System.Random random, float x, float z, float minScale, float maxScale)
        {
            var prefab = prefabs[random.Next(prefabs.Length)];
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            instance.name = prefab.name;
            instance.transform.SetParent(parent);
            instance.transform.position = new Vector3(x, 0.05f, z);
            instance.transform.rotation = Quaternion.Euler(0f, RandomRange(random, 0f, 360f), 0f);
            var scale = RandomRange(random, minScale, maxScale);
            instance.transform.localScale = Vector3.one * scale;
        }

        private static float RandomRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static void CreateSpawnMarkers(Transform parent)
        {
            var markerRoot = new GameObject("Spawn_Markers");
            markerRoot.transform.SetParent(parent);

            for (var i = 0; i < 5; i++)
            {
                var marker = new GameObject($"MonsterSpawn_{i + 1:00}");
                marker.transform.SetParent(markerRoot.transform);
                marker.transform.position = new Vector3(-24f + i * 12f, 0.35f, 0f);
            }
        }

        private static void SetupCameraAndLight()
        {
            var light = FindFirstLight();
            if (light == null)
            {
                var lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            light.intensity = 1.2f;

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.transform.position = new Vector3(0f, 17f, -24f);
            camera.transform.rotation = Quaternion.Euler(36f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 13.5f;
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
        }

        private static Light FindFirstLight()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<Light>();
#else
            return Object.FindObjectOfType<Light>();
#endif
        }
    }
}
