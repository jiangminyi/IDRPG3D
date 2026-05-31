using System;
using System.Collections.Generic;
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

    public readonly struct IDRPG3DCombatFloatingTextConfig
    {
        public IDRPG3DCombatFloatingTextConfig(
            IDRPG3DCombatFloatingTextKind kind,
            string prefabPath,
            Color color,
            float scale,
            Vector3 worldOffset,
            bool followTarget)
        {
            Kind = kind;
            PrefabPath = prefabPath ?? string.Empty;
            Color = color;
            Scale = Mathf.Max(0.01f, scale);
            WorldOffset = worldOffset;
            FollowTarget = followTarget;
        }

        public IDRPG3DCombatFloatingTextKind Kind { get; }
        public string PrefabPath { get; }
        public Color Color { get; }
        public float Scale { get; }
        public Vector3 WorldOffset { get; }
        public bool FollowTarget { get; }
        public bool IsValid => Kind != IDRPG3DCombatFloatingTextKind.None && !string.IsNullOrWhiteSpace(PrefabPath);
    }

    public sealed class IDRPG3DCombatFloatingTextCatalog
    {
        private readonly Dictionary<IDRPG3DCombatFloatingTextKind, IDRPG3DCombatFloatingTextConfig> configs =
            new Dictionary<IDRPG3DCombatFloatingTextKind, IDRPG3DCombatFloatingTextConfig>();

        public IDRPG3DCombatFloatingTextCatalog(IEnumerable<IDRPG3DCombatFloatingTextConfig> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var value in values)
            {
                if (value.IsValid)
                {
                    configs[value.Kind] = value;
                }
            }
        }

        public bool TryGet(IDRPG3DCombatFloatingTextKind kind, out IDRPG3DCombatFloatingTextConfig config)
        {
            return configs.TryGetValue(kind, out config);
        }

        public static IDRPG3DCombatFloatingTextCatalog CreateDefault()
        {
            return new IDRPG3DCombatFloatingTextCatalog(new[]
            {
                new IDRPG3DCombatFloatingTextConfig(
                    IDRPG3DCombatFloatingTextKind.NormalDamage,
                    "Assets/AssetRaw/UI/DamageNumbers/DamageNumber_Normal.prefab",
                    new Color(1f, 0.48f, 0.05f, 1f),
                    1f,
                    new Vector3(0f, 1.35f, 0f),
                    followTarget: true),
                new IDRPG3DCombatFloatingTextConfig(
                    IDRPG3DCombatFloatingTextKind.CriticalDamage,
                    "Assets/AssetRaw/UI/DamageNumbers/DamageNumber_Critical.prefab",
                    new Color(1f, 0.08f, 0.05f, 1f),
                    1.35f,
                    new Vector3(0f, 1.5f, 0f),
                    followTarget: true),
                new IDRPG3DCombatFloatingTextConfig(
                    IDRPG3DCombatFloatingTextKind.Heal,
                    "Assets/AssetRaw/UI/DamageNumbers/DamageNumber_Heal.prefab",
                    new Color(0.2f, 1f, 0.35f, 1f),
                    1f,
                    new Vector3(0f, 1.45f, 0f),
                    followTarget: true)
            });
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    public sealed class IDRPG3DCombatFloatingTextPresenter : MonoBehaviour
    {
        private static readonly Type[] SpawnNumberSignature = { typeof(Vector3), typeof(float) };
        private static readonly Type[] SpawnTextSignature = { typeof(Vector3), typeof(string) };
        private static readonly Type[] ColorSignature = { typeof(Color) };
        private static readonly Type[] ScaleSignature = { typeof(float) };
        private static readonly Type[] FollowSignature = { typeof(Transform), typeof(bool) };
        private static readonly Type[] EmptySignature = Type.EmptyTypes;

        [SerializeField] private IDRPG3DCombatUnit unit;
        [SerializeField] private GameObject normalDamagePrefab;
        [SerializeField] private GameObject criticalDamagePrefab;
        [SerializeField] private GameObject healPrefab;

        private IDRPG3DCombatFloatingTextCatalog catalog = IDRPG3DCombatFloatingTextCatalog.CreateDefault();
        private readonly Dictionary<string, GameObject> prefabsByPath = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (unit == null)
            {
                unit = GetComponent<IDRPG3DCombatUnit>();
            }

            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public void Configure(
            IDRPG3DCombatUnit targetUnit,
            IDRPG3DCombatFloatingTextCatalog textCatalog,
            IReadOnlyDictionary<string, GameObject> popupPrefabs)
        {
            Unsubscribe();

            unit = targetUnit != null ? targetUnit : GetComponent<IDRPG3DCombatUnit>();
            catalog = textCatalog ?? IDRPG3DCombatFloatingTextCatalog.CreateDefault();
            prefabsByPath.Clear();
            if (popupPrefabs != null)
            {
                foreach (var pair in popupPrefabs)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value != null)
                    {
                        prefabsByPath[pair.Key] = pair.Value;
                    }
                }
            }

            normalDamagePrefab = ResolvePrefab(IDRPG3DCombatFloatingTextKind.NormalDamage);
            criticalDamagePrefab = ResolvePrefab(IDRPG3DCombatFloatingTextKind.CriticalDamage);
            healPrefab = ResolvePrefab(IDRPG3DCombatFloatingTextKind.Heal);
            Subscribe();
        }

        private void Subscribe()
        {
            if (unit != null)
            {
                unit.FloatingTextRequested += OnFloatingTextRequested;
            }
        }

        private void Unsubscribe()
        {
            if (unit != null)
            {
                unit.FloatingTextRequested -= OnFloatingTextRequested;
            }
        }

        private void OnFloatingTextRequested(IDRPG3DCombatFloatingTextEvent textEvent)
        {
            var kind = textEvent.Kind;
            if (kind == IDRPG3DCombatFloatingTextKind.None
                || textEvent.Target == null
                || textEvent.Value <= 0f
                || !catalog.TryGet(kind, out var config))
            {
                return;
            }

            var prefab = ResolvePrefab(config);
            if (prefab == null)
            {
                return;
            }

            var position = CalculateSpawnPosition(textEvent, config);
            var popup = SpawnPopup(prefab, position, textEvent.Value, kind == IDRPG3DCombatFloatingTextKind.Heal);
            if (popup == null)
            {
                return;
            }

            ApplyPopupRuntimeSettings(popup, config, textEvent.Target.transform);
        }

        public static Vector3 CalculateSpawnPositionForTest(
            IDRPG3DCombatFloatingTextEvent textEvent,
            IDRPG3DCombatFloatingTextConfig config)
        {
            return CalculateSpawnPosition(textEvent, config);
        }

        public static void ApplyPopupRuntimeSettingsForTest(
            Component popup,
            IDRPG3DCombatFloatingTextConfig config,
            Transform followedTarget)
        {
            ApplyPopupRuntimeSettings(popup, config, followedTarget);
        }

        private static Vector3 CalculateSpawnPosition(
            IDRPG3DCombatFloatingTextEvent textEvent,
            IDRPG3DCombatFloatingTextConfig config)
        {
            return textEvent.Target != null
                ? textEvent.Target.transform.position + config.WorldOffset
                : config.WorldOffset;
        }

        private static void ApplyPopupRuntimeSettings(
            Component popup,
            IDRPG3DCombatFloatingTextConfig config,
            Transform followedTarget)
        {
            InvokePopupMethod(popup, "SetColor", ColorSignature, config.Color);
            InvokePopupMethod(popup, "SetScale", ScaleSignature, config.Scale);
            if (config.FollowTarget && followedTarget != null)
            {
                InvokePopupMethod(popup, "SetFollowedTarget", FollowSignature, followedTarget, true);
            }

            InvokePopupMethod(popup, "UpdateText", EmptySignature);
        }

        private GameObject ResolvePrefab(IDRPG3DCombatFloatingTextKind kind)
        {
            return catalog != null && catalog.TryGet(kind, out var config)
                ? ResolvePrefab(config)
                : null;
        }

        private GameObject ResolvePrefab(IDRPG3DCombatFloatingTextConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config.PrefabPath)
                && prefabsByPath.TryGetValue(config.PrefabPath, out var prefab)
                && prefab != null)
            {
                return prefab;
            }

            if (config.Kind == IDRPG3DCombatFloatingTextKind.NormalDamage)
            {
                return normalDamagePrefab;
            }

            if (config.Kind == IDRPG3DCombatFloatingTextKind.CriticalDamage)
            {
                return criticalDamagePrefab;
            }

            return config.Kind == IDRPG3DCombatFloatingTextKind.Heal ? healPrefab : null;
        }

        private static Component SpawnPopup(GameObject prefab, Vector3 position, float value, bool showAsHeal)
        {
            var damageNumber = FindDamageNumberComponent(prefab);
            if (damageNumber == null)
            {
                return null;
            }

            var type = damageNumber.GetType();
            if (showAsHeal)
            {
                var textMethod = type.GetMethod("Spawn", SpawnTextSignature);
                if (textMethod != null)
                {
                    return textMethod.Invoke(damageNumber, new object[] { position, $"+{Mathf.RoundToInt(value)}" }) as Component;
                }
            }

            var method = type.GetMethod("Spawn", SpawnNumberSignature);
            return method?.Invoke(damageNumber, new object[] { position, value }) as Component;
        }

        private static Component FindDamageNumberComponent(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            var components = prefab.GetComponentsInChildren<Component>(true);
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null)
                {
                    continue;
                }

                var type = component.GetType();
                while (type != null)
                {
                    if (type.FullName == "DamageNumbersPro.DamageNumber")
                    {
                        return component;
                    }

                    type = type.BaseType;
                }
            }

            return null;
        }

        private static void InvokePopupMethod(Component popup, string methodName, Type[] signature, params object[] arguments)
        {
            if (popup == null)
            {
                return;
            }

            var method = popup.GetType().GetMethod(methodName, signature);
            method?.Invoke(popup, arguments);
        }
    }
}
