using System.Collections.Generic;
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
            : this(
                configId,
                skillKey,
                displayName,
                level: 1,
                range,
                cooldown,
                projectileSpeed,
                projectilePrefabPath,
                muzzlePrefabPath,
                impactPrefabPath,
                fallbackColor,
                buff.IsValid
                    ? new[]
                    {
                        IDRPG3DPrototypeEffectDefinition.DamageWithBuff(effectId, damage, buff)
                    }
                    : new[]
                    {
                        IDRPG3DPrototypeEffectDefinition.Damage(effectId, damage)
                    })
        {
        }

        public IDRPG3DPrototypeSkillConfigRecord(
            int configId,
            string skillKey,
            string displayName,
            int level,
            float range,
            float cooldown,
            float projectileSpeed,
            string projectilePrefabPath,
            string muzzlePrefabPath,
            string impactPrefabPath,
            Color fallbackColor,
            IReadOnlyList<IDRPG3DPrototypeEffectDefinition> effects)
        {
            ConfigId = configId;
            SkillKey = skillKey;
            DisplayName = displayName;
            Level = Mathf.Max(1, level);
            Range = range;
            Cooldown = cooldown;
            ProjectileSpeed = projectileSpeed;
            ProjectilePrefabPath = projectilePrefabPath;
            MuzzlePrefabPath = muzzlePrefabPath;
            ImpactPrefabPath = impactPrefabPath;
            FallbackColor = fallbackColor;
            Effects = effects ?? System.Array.Empty<IDRPG3DPrototypeEffectDefinition>();
            var primary = Effects.Count > 0 ? Effects[0] : default;
            EffectId = primary.EffectId;
            Damage = primary.EffectType == IDRPG3DPrototypeEffectType.Damage ? primary.Value : 0f;
            Buff = primary.Buff;
        }

        public int ConfigId { get; }
        public string SkillKey { get; }
        public string DisplayName { get; }
        public int Level { get; }
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
        public IReadOnlyList<IDRPG3DPrototypeEffectDefinition> Effects { get; }
    }
}
