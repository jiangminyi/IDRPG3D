using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    [System.Serializable]
    public struct IDRPG3DPrototypeSkillDefinition
    {
        [SerializeField] private string skillId;
        [SerializeField] private string displayName;
        [SerializeField] private int level;
        [SerializeField] private float damage;
        [SerializeField] private IDRPG3DPrototypeEffectDefinition primaryEffect;
        [SerializeField] private float range;
        [SerializeField] private float cooldown;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private Color fallbackColor;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject muzzlePrefab;
        [SerializeField] private GameObject impactPrefab;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public int Level => level;
        public float Damage => damage;
        public IDRPG3DPrototypeEffectDefinition PrimaryEffect => primaryEffect.IsValid
            ? primaryEffect
            : IDRPG3DPrototypeEffectDefinition.Damage(0, damage);
        public IReadOnlyList<IDRPG3DPrototypeEffectDefinition> Effects { get; private set; }
        public float Range => range;
        public float Cooldown => cooldown;
        public float ProjectileSpeed => projectileSpeed;
        public Color FallbackColor => fallbackColor;
        public GameObject ProjectilePrefab => projectilePrefab;
        public GameObject MuzzlePrefab => muzzlePrefab;
        public GameObject ImpactPrefab => impactPrefab;
        public bool UsesProjectile => projectilePrefab != null || projectileSpeed > 0f;
        public bool IsValid => Effects != null && Effects.Count > 0 && range > 0f && cooldown > 0f;

        public IDRPG3DPrototypeSkillDefinition(
            string skillId,
            string displayName,
            float damage,
            float range,
            float cooldown,
            float projectileSpeed,
            Color fallbackColor,
            GameObject projectilePrefab = null,
            GameObject muzzlePrefab = null,
            GameObject impactPrefab = null)
            : this(
                skillId,
                displayName,
                IDRPG3DPrototypeEffectDefinition.Damage(0, damage),
                range,
                cooldown,
                projectileSpeed,
                fallbackColor,
                projectilePrefab,
                muzzlePrefab,
                impactPrefab)
        {
        }

        public IDRPG3DPrototypeSkillDefinition(
            string skillId,
            string displayName,
            IDRPG3DPrototypeEffectDefinition primaryEffect,
            float range,
            float cooldown,
            float projectileSpeed,
            Color fallbackColor,
            GameObject projectilePrefab = null,
            GameObject muzzlePrefab = null,
            GameObject impactPrefab = null)
            : this(
                skillId,
                displayName,
                level: 1,
                new[] { primaryEffect },
                range,
                cooldown,
                projectileSpeed,
                fallbackColor,
                projectilePrefab,
                muzzlePrefab,
                impactPrefab)
        {
        }

        public IDRPG3DPrototypeSkillDefinition(
            string skillId,
            string displayName,
            int level,
            IReadOnlyList<IDRPG3DPrototypeEffectDefinition> effects,
            float range,
            float cooldown,
            float projectileSpeed,
            Color fallbackColor,
            GameObject projectilePrefab = null,
            GameObject muzzlePrefab = null,
            GameObject impactPrefab = null)
        {
            this.skillId = skillId;
            this.displayName = displayName;
            this.level = Mathf.Max(1, level);
            Effects = effects ?? System.Array.Empty<IDRPG3DPrototypeEffectDefinition>();
            this.primaryEffect = Effects.Count > 0 ? Effects[0] : default;
            this.damage = FindPrimaryDamage(Effects);
            this.range = Mathf.Max(0.1f, range);
            this.cooldown = Mathf.Max(0.1f, cooldown);
            this.projectileSpeed = Mathf.Max(0f, projectileSpeed);
            this.fallbackColor = fallbackColor;
            this.projectilePrefab = projectilePrefab;
            this.muzzlePrefab = muzzlePrefab;
            this.impactPrefab = impactPrefab;
        }

        private static float FindPrimaryDamage(IReadOnlyList<IDRPG3DPrototypeEffectDefinition> effects)
        {
            if (effects == null)
            {
                return 0f;
            }

            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i].EffectType == IDRPG3DPrototypeEffectType.Damage)
                {
                    return Mathf.Max(0f, effects[i].Value);
                }
            }

            return 0f;
        }

        public static IDRPG3DPrototypeSkillDefinition CreateFrostbolt(
            GameObject projectilePrefab = null,
            GameObject muzzlePrefab = null,
            GameObject impactPrefab = null)
        {
            return new IDRPG3DPrototypeSkillDefinition(
                "frostbolt",
                "Frostbolt",
                18f,
                8.5f,
                1.45f,
                12f,
                new Color(0.25f, 0.78f, 1f, 1f),
                projectilePrefab,
                muzzlePrefab,
                impactPrefab);
        }

        public static IDRPG3DPrototypeSkillDefinition CreateFireball(
            GameObject projectilePrefab = null,
            GameObject muzzlePrefab = null,
            GameObject impactPrefab = null)
        {
            return new IDRPG3DPrototypeSkillDefinition(
                "fireball",
                "Fireball",
                24f,
                8f,
                1.75f,
                10f,
                new Color(1f, 0.38f, 0.08f, 1f),
                projectilePrefab,
                muzzlePrefab,
                impactPrefab);
        }
    }
}
