using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    public sealed class IDRPG3DPrototypeSkillCaster : MonoBehaviour
    {
        [SerializeField] private IDRPG3DPrototypeSkillDefinition skill;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private Vector3 castOffset = new Vector3(0f, 1.2f, 0.25f);
        [SerializeField] private float fallbackProjectileScale = 0.22f;

        private IDRPG3DCombatUnit unit;

        public IDRPG3DPrototypeSkillDefinition Skill => skill;
        public bool HasSkill => skill.IsValid;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            unit = GetComponent<IDRPG3DCombatUnit>();
        }

        public void Configure(IDRPG3DPrototypeSkillDefinition skillDefinition, Transform parent)
        {
            skill = skillDefinition;
            projectileParent = parent;
            Initialize();
        }

        public bool TryCast(IDRPG3DCombatUnit target)
        {
            if (!HasSkill || target == null || !target.IsAlive)
            {
                return false;
            }

            if (unit == null)
            {
                Initialize();
            }

            var startPosition = transform.TransformPoint(castOffset);
            SpawnMuzzle(startPosition);

            if (!skill.UsesProjectile)
            {
                ApplyEffects(target);
                return true;
            }

            var projectileObject = CreateProjectileObject(startPosition);
            var projectile = projectileObject.GetComponent<IDRPG3DPrototypeProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<IDRPG3DPrototypeProjectile>();
            }

            projectile.Launch(unit, target, skill, startPosition);
            return true;
        }

        private void ApplyEffects(IDRPG3DCombatUnit target)
        {
            var effects = skill.Effects;
            if (effects != null && effects.Count > 0)
            {
                for (var i = 0; i < effects.Count; i++)
                {
                    IDRPG3DPrototypeEffectRunner.Apply(effects[i], unit, target);
                }

                return;
            }

            IDRPG3DPrototypeEffectRunner.Apply(skill.PrimaryEffect, unit, target);
        }

        private GameObject CreateProjectileObject(Vector3 startPosition)
        {
            GameObject projectileObject;
            if (skill.ProjectilePrefab != null)
            {
                projectileObject = Instantiate(skill.ProjectilePrefab, startPosition, Quaternion.identity, projectileParent);
            }
            else
            {
                projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projectileObject.name = $"Prototype_{skill.SkillId}_Projectile";
                projectileObject.transform.SetParent(projectileParent, true);
                projectileObject.transform.position = startPosition;
                projectileObject.transform.localScale = Vector3.one * fallbackProjectileScale;
                var collider = projectileObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }

                var renderer = projectileObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = skill.FallbackColor;
                }

                var light = projectileObject.AddComponent<Light>();
                light.color = skill.FallbackColor;
                light.range = 2f;
                light.intensity = 1.5f;
            }

            return projectileObject;
        }

        private void SpawnMuzzle(Vector3 startPosition)
        {
            if (skill.MuzzlePrefab == null)
            {
                return;
            }

            var muzzle = Instantiate(skill.MuzzlePrefab, startPosition, transform.rotation, projectileParent);
            Destroy(muzzle, 1.5f);
        }
    }
}
