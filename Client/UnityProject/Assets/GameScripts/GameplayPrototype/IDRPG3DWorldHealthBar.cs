using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    public sealed class IDRPG3DWorldHealthBar : MonoBehaviour
    {
        [SerializeField] private Transform fillTransform;
        [SerializeField] private Transform barRoot;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.1f, 0f);
        [SerializeField] private float width = 2.4f;
        [SerializeField] private float height = 0.32f;
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.02f, 0.02f, 0.88f);
        [SerializeField] private Color fillColor = new Color(0.9f, 0.08f, 0.05f, 1f);

        private IDRPG3DCombatUnit unit;
        private Camera cachedCamera;

        private void Awake()
        {
            if (unit == null)
            {
                unit = GetComponent<IDRPG3DCombatUnit>();
            }
            EnsureVisuals();
            Subscribe();
            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void LateUpdate()
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

        public void Configure(
            IDRPG3DCombatUnit targetUnit,
            Transform targetFill,
            Transform targetRoot,
            float targetWidth,
            float targetHeight,
            float targetHeightOffset)
        {
            Unsubscribe();
            unit = targetUnit != null ? targetUnit : GetComponent<IDRPG3DCombatUnit>();
            fillTransform = targetFill != null ? targetFill : fillTransform;
            barRoot = targetRoot != null ? targetRoot : barRoot;
            width = Mathf.Max(0.1f, targetWidth);
            height = Mathf.Max(0.05f, targetHeight);
            worldOffset = new Vector3(0f, Mathf.Max(0f, targetHeightOffset), 0f);
            EnsureVisuals();
            Subscribe();
            Refresh();
        }

        public void Refresh()
        {
            if (unit == null || fillTransform == null)
            {
                return;
            }

            var ratio = unit.MaxHealth > 0f ? Mathf.Clamp01(unit.Health / unit.MaxHealth) : 0f;
            var scale = fillTransform.localScale;
            scale.x = ratio;
            fillTransform.localScale = scale;

            if (barRoot != null)
            {
                barRoot.gameObject.SetActive(unit.IsAlive);
            }
        }

        private void Subscribe()
        {
            if (unit == null)
            {
                return;
            }

            unit.HealthChanged += OnHealthChanged;
            unit.Died += OnDied;
        }

        private void Unsubscribe()
        {
            if (unit == null)
            {
                return;
            }

            unit.HealthChanged -= OnHealthChanged;
            unit.Died -= OnDied;
        }

        private void OnHealthChanged(IDRPG3DCombatUnit changedUnit, IDRPG3DCombatUnit source)
        {
            Refresh();
        }

        private void OnDied(IDRPG3DCombatUnit deadUnit)
        {
            Refresh();
        }

        private void EnsureVisuals()
        {
            if (barRoot == null)
            {
                barRoot = new GameObject("WorldHealthBar").transform;
                barRoot.SetParent(transform, false);
            }

            barRoot.localScale = Vector3.one;
            barRoot.position = transform.position + worldOffset;

            if (fillTransform == null)
            {
                var existingFill = barRoot.Find("HealthBar_Fill");
                if (existingFill != null)
                {
                    fillTransform = existingFill;
                }
            }

            if (fillTransform == null)
            {
                var background = GameObject.CreatePrimitive(PrimitiveType.Cube);
                background.name = "HealthBar_Background";
                background.transform.SetParent(barRoot, false);
                background.transform.localPosition = Vector3.zero;
                background.transform.localScale = new Vector3(width, height, 0.04f);
                ApplyMaterialColor(background, backgroundColor);
                DisableCollider(background);

                var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fill.name = "HealthBar_Fill";
                fill.transform.SetParent(barRoot, false);
                fill.transform.localPosition = new Vector3(-width * 0.5f, 0f, -0.03f);
                fill.transform.localScale = Vector3.one;
                ApplyMaterialColor(fill, fillColor);
                DisableCollider(fill);

                var fillMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fillMesh.name = "FillMesh";
                fillMesh.transform.SetParent(fill.transform, false);
                fillMesh.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                fillMesh.transform.localScale = new Vector3(width, height, 0.06f);
                ApplyMaterialColor(fillMesh, fillColor);
                DisableCollider(fillMesh);

                fillTransform = fill.transform;
            }
        }

        private static void ApplyMaterialColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }

        private static void DisableCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }
}
