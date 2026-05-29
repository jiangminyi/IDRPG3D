using System;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public enum IDRPG3DPrototypeSkillCastMode
    {
        Melee,
        Projectile,
        Instant,
        Charge,
        Area
    }

    public enum IDRPG3DPrototypeSkillTargetRule
    {
        Enemy,
        AreaEnemy,
        AllyLowestHp,
        DeadAlly,
        Self
    }

    public readonly struct IDRPG3DPrototypeSkillRuntime
    {
        public IDRPG3DPrototypeSkillRuntime(
            IDRPG3DPrototypeSkillDefinition definition,
            IDRPG3DCombatResourceType resourceType,
            float resourceCost,
            float resourceGain,
            IDRPG3DPrototypeSkillCastMode castMode,
            IDRPG3DPrototypeSkillTargetRule targetRule,
            float threatMultiplier)
        {
            Definition = definition;
            ResourceType = resourceType;
            ResourceCost = Mathf.Max(0f, resourceCost);
            ResourceGain = Mathf.Max(0f, resourceGain);
            CastMode = castMode;
            TargetRule = targetRule;
            ThreatMultiplier = Mathf.Max(0f, threatMultiplier);
        }

        public IDRPG3DPrototypeSkillDefinition Definition { get; }
        public IDRPG3DCombatResourceType ResourceType { get; }
        public float ResourceCost { get; }
        public float ResourceGain { get; }
        public IDRPG3DPrototypeSkillCastMode CastMode { get; }
        public IDRPG3DPrototypeSkillTargetRule TargetRule { get; }
        public float ThreatMultiplier { get; }
        public bool IsValid => Definition.IsValid;
        public bool IsBasicAttack
        {
            get
            {
                var skillKey = Definition.SkillId ?? string.Empty;
                return skillKey.IndexOf("basic", StringComparison.OrdinalIgnoreCase) >= 0
                    || skillKey.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0 && ResourceCost <= 0f;
            }
        }

        public static IDRPG3DPrototypeSkillCastMode ParseCastMode(string value)
        {
            return Enum.TryParse(value, true, out IDRPG3DPrototypeSkillCastMode result)
                ? result
                : IDRPG3DPrototypeSkillCastMode.Projectile;
        }

        public static IDRPG3DPrototypeSkillTargetRule ParseTargetRule(string value)
        {
            return Enum.TryParse(value, true, out IDRPG3DPrototypeSkillTargetRule result)
                ? result
                : IDRPG3DPrototypeSkillTargetRule.Enemy;
        }
    }
}
