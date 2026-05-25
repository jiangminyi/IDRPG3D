using System;
using System.IO;
using GameConfig;
using IDRPG3D.GameplayPrototype;
using Luban;
using UnityEngine;
using SkillConfig = GameConfig.skill.Skill;
using EffectConfig = GameConfig.skill.Effect;
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

                var effect = loadedTables.TbEffect.GetOrDefault(skill.EffectId);
                if (effect == null)
                {
                    return false;
                }

                var projectile = loadedTables.TbProjectile.GetOrDefault(skill.ProjectileId);
                if (projectile == null)
                {
                    return false;
                }

                record = BuildRecord(skill, effect, projectile);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Failed to load skill config {skillId}: {exception.Message}");
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

        private static IDRPG3DPrototypeSkillConfigRecord BuildRecord(
            SkillConfig skill,
            EffectConfig effect,
            ProjectileConfig projectile)
        {
            return new IDRPG3DPrototypeSkillConfigRecord(
                skill.Id,
                skill.SkillKey,
                skill.Name,
                effect.Value,
                skill.Range,
                skill.Cooldown,
                projectile.Speed,
                projectile.ProjectilePrefabPath,
                projectile.MuzzlePrefabPath,
                projectile.ImpactPrefabPath,
                ParseColor(projectile.FallbackColor));
        }

        private static Color ParseColor(string colorText)
        {
            return ColorUtility.TryParseHtmlString(colorText, out var color) ? color : Color.white;
        }
    }
}
