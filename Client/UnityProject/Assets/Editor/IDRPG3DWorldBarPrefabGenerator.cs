using System.IO;
using INab.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public static class IDRPG3DWorldBarPrefabGenerator
    {
        private const string MaterialFolder = "Assets/AssetRaw/Materials/WorldBars";
        private const string PrefabFolder = "Assets/AssetRaw/UI/WorldBars";

        private const string HealthTemplatePath =
            "Assets/INab Studio/2D Assets/Procedural Progress Bars/Demo Files/Demo Materials/Showcase/Showcase 38.mat";

        private const string ResourceTemplatePath =
            "Assets/INab Studio/2D Assets/Procedural Progress Bars/Demo Files/Demo Materials/Showcase/Showcase 39.mat";

        private const string HealthMaterialPath = MaterialFolder + "/WorldBar_Ragged_Health_Red.mat";
        private const string ManaMaterialPath = MaterialFolder + "/WorldBar_Ragged_Mana_Blue.mat";
        private const string RageMaterialPath = MaterialFolder + "/WorldBar_Ragged_Rage_Yellow.mat";

        private const string MonsterPrefabPath = PrefabFolder + "/WorldBar_Monster_HealthOnly.prefab";
        private const string WarriorPrefabPath = PrefabFolder + "/WorldBar_Warrior_HealthRage.prefab";
        private const string MagePrefabPath = PrefabFolder + "/WorldBar_Mage_HealthMana.prefab";

        [InitializeOnLoadMethod]
        private static void EnsureDefaultPrefabsExist()
        {
            EditorApplication.delayCall += () =>
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(MonsterPrefabPath) != null
                    && AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPrefabPath) != null
                    && AssetDatabase.LoadAssetAtPath<GameObject>(MagePrefabPath) != null)
                {
                    return;
                }

                GenerateDefaultWorldBarPrefabs();
            };
        }

        [DidReloadScripts]
        private static void EnsureDefaultPrefabsExistAfterReload()
        {
            EnsureDefaultPrefabsExist();
        }

        [MenuItem("IDRPG3D/Gameplay/Generate Default World Bar Prefabs")]
        public static void GenerateDefaultWorldBarPrefabs()
        {
            EnsureFolders();

            var healthMaterial = CreateBarMaterial(
                HealthTemplatePath,
                HealthMaterialPath,
                new Color(0.86f, 0.14f, 0.09f, 1f),
                new Color(0.06f, 0.01f, 0.01f, 0.75f));
            var manaMaterial = CreateBarMaterial(
                ResourceTemplatePath,
                ManaMaterialPath,
                new Color(0.12f, 0.44f, 1f, 1f),
                new Color(0.02f, 0.07f, 0.22f, 0.72f));
            var rageMaterial = CreateBarMaterial(
                ResourceTemplatePath,
                RageMaterialPath,
                new Color(1f, 0.73f, 0.08f, 1f),
                new Color(0.22f, 0.12f, 0.01f, 0.72f));

            CreateWorldBarPrefab(MonsterPrefabPath, "WorldBar_Monster_HealthOnly", healthMaterial, null);
            CreateWorldBarPrefab(WarriorPrefabPath, "WorldBar_Warrior_HealthRage", healthMaterial, rageMaterial);
            CreateWorldBarPrefab(MagePrefabPath, "WorldBar_Mage_HealthMana", healthMaterial, manaMaterial);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[IDRPG3D] Default world bar prefabs generated.");
        }

        private static Material CreateBarMaterial(string templatePath, string outputPath, Color fillColor, Color backgroundColor)
        {
            if (!File.Exists(outputPath))
            {
                if (!AssetDatabase.CopyAsset(templatePath, outputPath))
                {
                    Debug.LogError($"[IDRPG3D] Failed to copy progress bar material template: {templatePath}");
                    return null;
                }
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(outputPath);
            if (material == null)
            {
                Debug.LogError($"[IDRPG3D] Failed to load progress bar material: {outputPath}");
                return null;
            }

            SetColorIfExists(material, "_Fill_Color_No_Gradient", fillColor);
            SetColorIfExists(material, "_Fill_Gradient_Color_Left", fillColor * 0.72f);
            SetColorIfExists(material, "_Fill_Gradient_Color_Right", fillColor);
            SetColorIfExists(material, "_Fill_Background_color", backgroundColor);
            SetFloatIfExists(material, "_Fill_Amount", 1f);
            SetFloatIfExists(material, "_Main_Bar_Fill_Amount", 1f);
            SetFloatIfExists(material, "_Use_Gradient", 0f);
            SetFloatIfExists(material, "_General_Shape_Height", 0.014f);

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateWorldBarPrefab(
            string prefabPath,
            string prefabName,
            Material healthMaterial,
            Material resourceMaterial)
        {
            if (healthMaterial == null)
            {
                return;
            }

            var root = new GameObject(prefabName);
            root.transform.localScale = Vector3.one;

            CreateBarQuad("HealthBar", root.transform, healthMaterial, new Vector3(0f, 0.08f, 0f), new Vector3(2.2f, 0.22f, 1f));

            if (resourceMaterial != null)
            {
                CreateBarQuad("ResourceBar", root.transform, resourceMaterial, new Vector3(0f, -0.13f, 0f), new Vector3(2f, 0.16f, 1f));
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreateBarQuad(
            string name,
            Transform parent,
            Material material,
            Vector3 localPosition,
            Vector3 localScale)
        {
            var bar = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bar.name = name;
            bar.transform.SetParent(parent, false);
            bar.transform.localPosition = localPosition;
            bar.transform.localRotation = Quaternion.identity;
            bar.transform.localScale = localScale;

            var collider = bar.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = bar.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var progressBar = bar.AddComponent<ProceduralProgressBar>();
            progressBar.progressBarMaterial = material;
            progressBar.barRenderer = renderer;
            progressBar.instantiateMaterialOnStart = true;
            progressBar.UseInitialFillAmount = true;
            progressBar.InitialFillAmount = 1f;
            progressBar.FillAmount = 1f;
            progressBar.MainBarFillAmount = 1f;

            return bar;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/AssetRaw", "Materials");
            EnsureFolder("Assets/AssetRaw/Materials", "WorldBars");
            EnsureFolder("Assets/AssetRaw", "UI");
            EnsureFolder("Assets/AssetRaw/UI", "WorldBars");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void SetColorIfExists(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetFloatIfExists(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }
    }
}
