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
                record.Level,
                record.Effects,
                record.Range,
                record.Cooldown,
                record.ProjectileSpeed,
                record.FallbackColor,
                projectilePrefab,
                muzzlePrefab,
                impactPrefab).WithConfigId(record.ConfigId);
        }

        public static IDRPG3DPrototypeSkillRuntime BuildRuntime(
            IDRPG3DPrototypeSkillConfigRecord record,
            GameObject projectilePrefab = null,
            GameObject muzzlePrefab = null,
            GameObject impactPrefab = null)
        {
            return new IDRPG3DPrototypeSkillRuntime(
                Build(record, projectilePrefab, muzzlePrefab, impactPrefab),
                record.ResourceType,
                record.ResourceCost,
                record.ResourceGain,
                record.CastMode,
                record.TargetRule,
                record.ThreatMultiplier);
        }
    }
}
