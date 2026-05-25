using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public readonly struct IDRPG3DPrototypeSkillConfigRecord
    {
        public IDRPG3DPrototypeSkillConfigRecord(
            int configId,
            string skillKey,
            string displayName,
            int effectId,
            float damage,
            float range,
            float cooldown,
            float projectileSpeed,
            string projectilePrefabPath,
            string muzzlePrefabPath,
            string impactPrefabPath,
            Color fallbackColor,
            IDRPG3DPrototypeBuffDefinition buff = default)
        {
            ConfigId = configId;
            SkillKey = skillKey;
            DisplayName = displayName;
            EffectId = effectId;
            Damage = damage;
            Range = range;
            Cooldown = cooldown;
            ProjectileSpeed = projectileSpeed;
            ProjectilePrefabPath = projectilePrefabPath;
            MuzzlePrefabPath = muzzlePrefabPath;
            ImpactPrefabPath = impactPrefabPath;
            FallbackColor = fallbackColor;
            Buff = buff;
        }

        public int ConfigId { get; }
        public string SkillKey { get; }
        public string DisplayName { get; }
        public int EffectId { get; }
        public float Damage { get; }
        public float Range { get; }
        public float Cooldown { get; }
        public float ProjectileSpeed { get; }
        public string ProjectilePrefabPath { get; }
        public string MuzzlePrefabPath { get; }
        public string ImpactPrefabPath { get; }
        public Color FallbackColor { get; }
        public IDRPG3DPrototypeBuffDefinition Buff { get; }
    }
}
