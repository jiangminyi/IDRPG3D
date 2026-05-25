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
            return new IDRPG3DPrototypeSkillDefinition(
                record.SkillKey,
                record.DisplayName,
                record.Damage,
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
