using System;
using System.Reflection;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    public sealed class IDRPG3DWorldUnitBar : MonoBehaviour
    {
        private const string HealthBarName = "HealthBar";
        private const string ResourceBarName = "ResourceBar";
        private const string LevelName = "Level";
        private const string FillAmountProperty = "_Fill_Amount";
        private const string MainBarFillAmountProperty = "_Main_Bar_Fill_Amount";

        [SerializeField] private IDRPG3DCombatUnit unit;
        [SerializeField] private IDRPG3DCombatResource resource;
        [SerializeField] private GameObject barPrefab;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
        [SerializeField] private float visualScale = 1f;
        [SerializeField] private bool showResourceBar;
        [SerializeField, Range(0f, 1f)] private float resourceFill = 1f;

        private Transform barRoot;
        private Transform healthBar;
        private Transform resourceBar;
        private Transform levelLabel;
        private Camera cachedCamera;
        private float healthFill = 1f;

        public Transform BarRootForTest => barRoot;
        public Transform HealthBarForTest => healthBar;
        public Transform ResourceBarForTest => resourceBar;
        public float HealthFillForTest => healthFill;
        public float ResourceFillForTest => resourceFill;

        private void Awake()
        {
            if (unit == null)
            {
                unit = GetComponent<IDRPG3DCombatUnit>();
            }
            if (resource == null)
            {
                resource = GetComponent<IDRPG3DCombatResource>();
            }

            if (barPrefab != null)
            {
                RebuildVisuals();
            }

            Subscribe();
            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();

            if (barRoot != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(barRoot.gameObject);
                }
                else
                {
                    DestroyImmediate(barRoot.gameObject);
                }
            }
        }

        private void LateUpdate()
        {
            TickForTest();
        }

        public void Configure(
            IDRPG3DCombatUnit targetUnit,
            GameObject prefab,
            Vector3 offset,
            float scale,
            bool hasResourceBar,
            float initialResourceFill)
        {
            Unsubscribe();

            unit = targetUnit != null ? targetUnit : GetComponent<IDRPG3DCombatUnit>();
            resource = unit != null ? unit.GetComponent<IDRPG3DCombatResource>() : GetComponent<IDRPG3DCombatResource>();
            barPrefab = prefab;
            worldOffset = offset;
            visualScale = Mathf.Max(0.01f, scale);
            showResourceBar = hasResourceBar;
            resourceFill = Mathf.Clamp01(initialResourceFill);

            RebuildVisuals();
            Subscribe();
            Refresh();
        }

        public void SetResourceFill(float value)
        {
            resourceFill = Mathf.Clamp01(value);
            ApplyFill(resourceBar, resourceFill);
        }

        public void Refresh()
        {
            healthFill = unit != null && unit.MaxHealth > 0f ? Mathf.Clamp01(unit.Health / unit.MaxHealth) : 0f;
            ApplyFill(healthBar, healthFill);

            if (resourceBar != null)
            {
                resourceBar.gameObject.SetActive(showResourceBar);
                ApplyFill(resourceBar, resource != null ? resource.FillAmount : resourceFill);
            }

            if (levelLabel != null && unit != null)
            {
                SetText(levelLabel, $"Lv.{unit.Level}");
            }

            if (barRoot != null)
            {
                barRoot.gameObject.SetActive(unit == null || unit.IsAlive);
            }
        }

        public void TickForTest()
        {
            if (barRoot == null)
            {
                return;
            }

            barRoot.position = transform.position + worldOffset;
            cachedCamera = cachedCamera != null ? cachedCamera : Camera.main;
            if (cachedCamera != null)
            {
                barRoot.rotation = cachedCamera.transform.rotation;
            }
        }

        private void RebuildVisuals()
        {
            if (barRoot != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(barRoot.gameObject);
                }
                else
                {
                    DestroyImmediate(barRoot.gameObject);
                }
            }

            var rootObject = barPrefab != null ? Instantiate(barPrefab) : new GameObject("WorldBar_Runtime");
            rootObject.name = $"WorldBar_{name}";
            barRoot = rootObject.transform;
            barRoot.localScale = Vector3.one * visualScale;

            healthBar = FindChildByName(barRoot, HealthBarName);
            resourceBar = FindChildByName(barRoot, ResourceBarName);
            levelLabel = FindChildByName(barRoot, LevelName);

            if (resourceBar != null)
            {
                resourceBar.gameObject.SetActive(showResourceBar);
            }

            TickForTest();
        }

        private void Subscribe()
        {
            if (unit == null)
            {
                return;
            }

            unit.HealthChanged += OnHealthChanged;
            unit.Died += OnDied;
            unit.LevelChanged += OnLevelChanged;
            if (resource != null)
            {
                resource.ResourceChanged += OnResourceChanged;
            }
        }

        private void Unsubscribe()
        {
            if (unit != null)
            {
                unit.HealthChanged -= OnHealthChanged;
                unit.Died -= OnDied;
                unit.LevelChanged -= OnLevelChanged;
            }
            if (resource != null)
            {
                resource.ResourceChanged -= OnResourceChanged;
            }
        }

        private void OnHealthChanged(IDRPG3DCombatUnit changedUnit, IDRPG3DCombatUnit source)
        {
            Refresh();
        }

        private void OnDied(IDRPG3DCombatUnit deadUnit)
        {
            Refresh();
        }

        private void OnLevelChanged(IDRPG3DCombatUnit changedUnit)
        {
            Refresh();
        }

        private void OnResourceChanged(IDRPG3DCombatResource changedResource)
        {
            SetResourceFill(changedResource != null ? changedResource.FillAmount : resourceFill);
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            var children = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i] != null && string.Equals(children[i].name, childName, StringComparison.Ordinal))
                {
                    return children[i];
                }
            }

            return null;
        }

        private static void ApplyFill(Transform bar, float value)
        {
            if (bar == null)
            {
                return;
            }

            var clamped = Mathf.Clamp01(value);
            if (!TryApplyProceduralProgressBar(bar, clamped))
            {
                ApplyFillToRendererMaterial(bar, clamped);
            }
        }

        private static bool TryApplyProceduralProgressBar(Transform bar, float value)
        {
            var components = bar.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null || component.GetType().FullName != "INab.UI.ProceduralProgressBar")
                {
                    continue;
                }

                var type = component.GetType();
                if (!EnsureProceduralProgressBarReferences(type, component, bar))
                {
                    ApplyFillToRendererMaterial(bar, value);
                    return true;
                }

                var method = type.GetMethod("UpdateBarFillAmount", BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    try
                    {
                        method.Invoke(component, new object[] { value });
                    }
                    catch (TargetInvocationException exception) when (exception.InnerException is UnassignedReferenceException || exception.InnerException is NullReferenceException)
                    {
                        ApplyFillToRendererMaterial(bar, value);
                    }

                    return true;
                }

                SetFloatField(type, component, "FillAmount", value);
                SetFloatField(type, component, "MainBarFillAmount", value);
                ApplyFillToRendererMaterial(bar, value);
                return true;
            }

            return false;
        }

        private static bool EnsureProceduralProgressBarReferences(Type type, Component component, Transform bar)
        {
            var renderer = bar.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var rendererField = type.GetField("barRenderer", BindingFlags.Instance | BindingFlags.Public);
                if (rendererField != null && rendererField.GetValue(component) == null)
                {
                    rendererField.SetValue(component, renderer);
                }
            }

            var materialField = type.GetField("progressBarMaterial", BindingFlags.Instance | BindingFlags.Public);
            if (materialField == null)
            {
                return false;
            }

            if (materialField.GetValue(component) != null)
            {
                return true;
            }

            if (renderer == null)
            {
                return false;
            }

            var material = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
            if (material == null)
            {
                return false;
            }

            materialField.SetValue(component, material);
            return true;
        }

        private static void SetFloatField(Type type, Component component, string fieldName, float value)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(component, value);
            }
        }

        private static void ApplyFillToRendererMaterial(Transform bar, float value)
        {
            var renderer = bar.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var material = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
            if (material == null)
            {
                return;
            }

            if (material.HasProperty(FillAmountProperty))
            {
                material.SetFloat(FillAmountProperty, value);
            }

            if (material.HasProperty(MainBarFillAmountProperty))
            {
                material.SetFloat(MainBarFillAmountProperty, value);
            }
        }

        private static void SetText(Transform root, string value)
        {
            var components = root.GetComponentsInChildren<Component>(true);
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null)
                {
                    continue;
                }

                var property = component.GetType().GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
                if (property != null && property.CanWrite && property.PropertyType == typeof(string))
                {
                    property.SetValue(component, value);
                    return;
                }
            }
        }
    }
}
