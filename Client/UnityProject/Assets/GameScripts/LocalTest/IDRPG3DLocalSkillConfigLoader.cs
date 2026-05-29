using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameConfig;
using IDRPG3D.GameplayPrototype;
using Luban;
using UnityEngine;
using SkillConfig = GameConfig.skill.Skill;
using SkillLevelConfig = GameConfig.skill.SkillLevel;
using EffectConfig = GameConfig.skill.Effect;
using BuffConfig = GameConfig.skill.Buff;
using ProjectileConfig = GameConfig.skill.Projectile;

namespace IDRPG3D.LocalTest
{
    public sealed class IDRPG3DLocalSkillConfigLoader
    {
        private const string ConfigBytesRoot = "AssetRaw/Configs/bytes";
        private const string BytesExtension = ".bytes";

        private Tables tables;

        public bool TryBuildSkill(int skillId, out IDRPG3DPrototypeSkillConfigRecord record)
        {
            record = default;

            try
            {
                var loadedTables = GetTables();
                var skill = loadedTables.TbSkill.GetOrDefault(skillId);
                if (skill == null)
                {
                    return false;
                }

                return TryBuildSkill(skillId, skill.DefaultLevel, out record);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Failed to load skill config {skillId}: {exception.Message}");
                return false;
            }
        }

        public bool TryBuildSkill(int skillId, int level, out IDRPG3DPrototypeSkillConfigRecord record)
        {
            record = default;

            try
            {
                var loadedTables = GetTables();
                var skill = loadedTables.TbSkill.GetOrDefault(skillId);
                if (skill == null)
                {
                    return false;
                }

                var skillLevel = FindSkillLevel(loadedTables, skillId, level);
                if (skillLevel == null)
                {
                    return false;
                }

                var projectile = skillLevel.ProjectileId > 0
                    ? loadedTables.TbProjectile.GetOrDefault(skillLevel.ProjectileId)
                    : null;
                if (skillLevel.ProjectileId > 0 && projectile == null)
                {
                    return false;
                }

                var effects = BuildEffects(loadedTables, skillId, skillLevel.Level);
                if (effects.Count == 0)
                {
                    return false;
                }

                record = BuildRecord(skill, skillLevel, projectile, effects);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Failed to load skill config {skillId} level {level}: {exception.Message}");
                return false;
            }
        }

        private Tables GetTables()
        {
            if (tables != null)
            {
                return tables;
            }

            tables = new Tables(LoadByteBuf);
            return tables;
        }

        private static ByteBuf LoadByteBuf(string file)
        {
            var path = Path.Combine(Application.dataPath, ConfigBytesRoot, file + BytesExtension);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Config bytes not found: {path}", path);
            }

            return new ByteBuf(File.ReadAllBytes(path));
        }

        private static SkillLevelConfig FindSkillLevel(Tables loadedTables, int skillId, int level)
        {
            var skillLevelId = skillId * 100 + level;
            var skillLevel = loadedTables.TbSkillLevel.GetOrDefault(skillLevelId);
            if (skillLevel != null)
            {
                return skillLevel;
            }

            var levels = loadedTables.TbSkillLevel.DataList;
            for (var i = 0; i < levels.Count; i++)
            {
                var candidate = levels[i];
                if (candidate.SkillId == skillId && candidate.Level == level)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static IDRPG3DPrototypeSkillConfigRecord BuildRecord(
            SkillConfig skill,
            SkillLevelConfig skillLevel,
            ProjectileConfig projectile,
            IReadOnlyList<IDRPG3DPrototypeEffectDefinition> effects)
        {
            return new IDRPG3DPrototypeSkillConfigRecord(
                skill.Id,
                skill.SkillKey,
                skill.Name,
                skillLevel.Level,
                skillLevel.Range,
                skillLevel.Cooldown,
                ResolveProjectileSpeed(skillLevel, projectile),
                projectile?.ProjectilePrefabPath ?? string.Empty,
                projectile?.MuzzlePrefabPath ?? string.Empty,
                projectile?.ImpactPrefabPath ?? string.Empty,
                ParseColor(!string.IsNullOrWhiteSpace(skillLevel.FallbackColor)
                    ? skillLevel.FallbackColor
                    : projectile?.FallbackColor),
                IDRPG3DCombatResource.ParseType(skill.ResourceType),
                skillLevel.ResourceCost,
                skillLevel.ResourceGain,
                IDRPG3DPrototypeSkillRuntime.ParseCastMode(skill.CastMode),
                IDRPG3DPrototypeSkillRuntime.ParseTargetRule(skill.TargetRule),
                skill.ThreatMultiplier,
                effects);
        }

        private static float ResolveProjectileSpeed(SkillLevelConfig skillLevel, ProjectileConfig projectile)
        {
            if (skillLevel.ProjectileSpeed > 0f)
            {
                return skillLevel.ProjectileSpeed;
            }

            return projectile?.Speed ?? 0f;
        }

        private static List<IDRPG3DPrototypeEffectDefinition> BuildEffects(Tables loadedTables, int skillId, int level)
        {
            var effectRelations = loadedTables.TbSkillEffect.DataList
                .Where(relation => relation.SkillId == skillId && relation.Level == level)
                .OrderBy(relation => relation.Order);
            var effects = new List<IDRPG3DPrototypeEffectDefinition>();

            foreach (var relation in effectRelations)
            {
                var effect = loadedTables.TbEffect.GetOrDefault(relation.EffectId);
                if (effect == null)
                {
                    continue;
                }

                effects.Add(BuildEffectDefinition(loadedTables, effect));
            }

            return effects;
        }

        private static IDRPG3DPrototypeEffectDefinition BuildEffectDefinition(Tables loadedTables, EffectConfig effect)
        {
            if (string.Equals(effect.EffectType, "Heal", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeEffectDefinition.Heal(effect.Id, effect.Value);
            }

            if (string.Equals(effect.EffectType, "AreaDamage", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeEffectDefinition.AreaDamage(effect.Id, effect.Value);
            }

            if (string.Equals(effect.EffectType, "AddThreat", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeEffectDefinition.AddThreat(effect.Id, effect.Value);
            }

            if (string.Equals(effect.EffectType, "GenerateResource", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeEffectDefinition.GenerateResource(effect.Id, effect.Value);
            }

            if (string.Equals(effect.EffectType, "Resurrect", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeEffectDefinition.Resurrect(effect.Id, effect.Value);
            }

            if (string.Equals(effect.EffectType, "AddBuff", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeEffectDefinition.AddBuff(effect.Id, BuildBuffDefinition(loadedTables, effect.BuffId));
            }

            if (effect.BuffId > 0)
            {
                return IDRPG3DPrototypeEffectDefinition.DamageWithBuff(
                    effect.Id,
                    effect.Value,
                    BuildBuffDefinition(loadedTables, effect.BuffId));
            }

            return IDRPG3DPrototypeEffectDefinition.Damage(effect.Id, effect.Value);
        }

        private static IDRPG3DPrototypeBuffDefinition BuildBuffDefinition(Tables loadedTables, int buffId)
        {
            if (buffId <= 0)
            {
                return default;
            }

            var buff = loadedTables.TbBuff.GetOrDefault(buffId);
            return BuildBuffDefinition(loadedTables, buff);
        }

        private static IDRPG3DPrototypeBuffDefinition BuildBuffDefinition(Tables loadedTables, BuffConfig buff)
        {
            if (buff == null || buff.Id <= 0)
            {
                return default;
            }

            if (string.Equals(buff.BuffType, "DamageOverTime", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeBuffDefinition.DamageOverTime(
                    buff.Id,
                    buff.BuffKey,
                    buff.Name,
                    buff.Duration,
                    buff.MaxStack,
                    buff.TickInterval,
                    buff.ModifierValue);
            }

            if (string.Equals(buff.BuffType, "Aura", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeBuffDefinition.Aura(
                    buff.Id,
                    buff.BuffKey,
                    buff.Name,
                    buff.Duration,
                    buff.TickInterval,
                    buff.AuraRadius,
                    BuildBuffDefinition(loadedTables, buff.AuraBuffId));
            }

            return IDRPG3DPrototypeBuffDefinition.StatModifier(
                buff.Id,
                buff.BuffKey,
                buff.Name,
                buff.Duration,
                buff.MaxStack,
                ParseStatType(buff.StatType),
                ParseModifierType(buff.ModifierType),
                buff.ModifierValue);
        }

        private static IDRPG3DPrototypeStatType ParseStatType(string value)
        {
            return Enum.TryParse(value, true, out IDRPG3DPrototypeStatType result)
                ? result
                : IDRPG3DPrototypeStatType.None;
        }

        private static IDRPG3DPrototypeModifierType ParseModifierType(string value)
        {
            if (string.Equals(value, "Mul", StringComparison.OrdinalIgnoreCase))
            {
                return IDRPG3DPrototypeModifierType.Multiply;
            }

            return Enum.TryParse(value, true, out IDRPG3DPrototypeModifierType result)
                ? result
                : IDRPG3DPrototypeModifierType.None;
        }

        private static Color ParseColor(string colorText)
        {
            return ColorUtility.TryParseHtmlString(colorText, out var color) ? color : Color.white;
        }
    }
}
