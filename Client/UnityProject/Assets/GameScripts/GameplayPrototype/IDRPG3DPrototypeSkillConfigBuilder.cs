using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public static class IDRPG3DPrototypeSkillConfigBuilder
    {
        public static IDRPG3DPrototypeSkillDefinition Build(
            IDRPG3DPrototypeSkillConfigRecord record,
            GameObject projectilePrefab = null,
            GameObject muzzlePrefab = null,
            GameObject impactPrefab = null)
        {
            var effect = record.Buff.IsValid
                ? IDRPG3DPrototypeEffectDefinition.DamageWithBuff(record.EffectId, record.Damage, record.Buff)
                : IDRPG3DPrototypeEffectDefinition.Damage(record.EffectId, record.Damage);

            return new IDRPG3DPrototypeSkillDefinition(
                record.SkillKey,
                record.DisplayName,
                effect,
                record.Range,
                record.Cooldown,
                record.ProjectileSpeed,
                record.FallbackColor,
                projectilePrefab,
                muzzlePrefab,
                impactPrefab);
        }
    }
}
