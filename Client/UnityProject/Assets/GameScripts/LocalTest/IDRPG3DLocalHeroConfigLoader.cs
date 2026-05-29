using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameConfig;
using IDRPG3D.GameplayPrototype;
using Luban;
using UnityEngine;
using HeroConfig = GameConfig.stage.Hero;
using HeroLevelConfig = GameConfig.stage.HeroLevel;

namespace IDRPG3D.LocalTest
{
    public sealed class IDRPG3DLocalHeroConfigLoader
    {
        private const string ConfigBytesRoot = "AssetRaw/Configs/bytes";
        private const string BytesExtension = ".bytes";

        private Tables tables;

        public bool TryBuildHero(int heroId, int level, out IDRPG3DLocalHeroConfig config)
        {
            config = default;

            try
            {
                var loadedTables = GetTables();
                var hero = loadedTables.TbHero.GetOrDefault(heroId);
                if (hero == null)
                {
                    return false;
                }

                var levels = loadedTables.TbHeroLevel.DataList
                    .Where(candidate => candidate.HeroId == heroId)
                    .OrderBy(candidate => candidate.Level)
                    .ToList();
                var currentLevel = FindHeroLevel(levels, level);
                if (currentLevel == null)
                {
                    return false;
                }

                config = BuildConfig(hero, currentLevel, levels);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Failed to load hero config {heroId} Lv{level}: {exception.Message}");
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

        private static HeroLevelConfig FindHeroLevel(IReadOnlyList<HeroLevelConfig> levels, int level)
        {
            for (var i = 0; i < levels.Count; i++)
            {
                if (levels[i].Level == level)
                {
                    return levels[i];
                }
            }

            return levels.Count > 0 ? levels[0] : null;
        }

        private static IDRPG3DLocalHeroConfig BuildConfig(
            HeroConfig hero,
            HeroLevelConfig currentLevel,
            IReadOnlyList<HeroLevelConfig> levels)
        {
            var requirements = new int[Mathf.Max(1, levels.Count)];
            for (var i = 0; i < levels.Count; i++)
            {
                var index = Mathf.Clamp(levels[i].Level - 1, 0, requirements.Length - 1);
                requirements[index] = levels[i].RequiredExp;
            }

            return new IDRPG3DLocalHeroConfig(
                hero.Id,
                hero.HeroKey,
                hero.Name,
                hero.ClassType,
                currentLevel.Level,
                levels.Count,
                hero.BaseHp * currentLevel.HpMultiplier,
                hero.BaseAttack * currentLevel.AttackMultiplier,
                hero.BaseDefense * currentLevel.DefenseMultiplier,
                hero.MoveSpeed,
                hero.AttackRange,
                hero.AttackInterval,
                hero.AggroRadius,
                hero.HealthRegen,
                IDRPG3DCombatResource.ParseType(hero.ResourceType),
                hero.MaxResource + currentLevel.MaxResourceBonus,
                hero.InitialResource,
                hero.ResourceRegen,
                hero.MovePriority,
                ParseSkillIds(hero.DefaultSkillIds),
                hero.BarPrefabType,
                requirements);
        }

        private static int[] ParseSkillIds(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<int>();
            }

            return value
                .Split(',')
                .Select(part => int.TryParse(part.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToArray();
        }
    }

    public readonly struct IDRPG3DLocalHeroConfig
    {
        public IDRPG3DLocalHeroConfig(
            int heroId,
            string heroKey,
            string displayName,
            string classType,
            int level,
            int maxLevel,
            float health,
            float attack,
            float defense,
            float moveSpeed,
            float attackRange,
            float attackInterval,
            float aggroRadius,
            float healthRegen,
            IDRPG3DCombatResourceType resourceType,
            float maxResource,
            float initialResource,
            float resourceRegen,
            int movePriority,
            int[] defaultSkillIds,
            string barPrefabType,
            int[] requiredExperienceByLevel)
        {
            HeroId = heroId;
            HeroKey = heroKey ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            ClassType = classType ?? string.Empty;
            Level = Mathf.Max(1, level);
            MaxLevel = Mathf.Max(Level, maxLevel);
            Health = Mathf.Max(1f, health);
            Attack = Mathf.Max(0f, attack);
            Defense = Mathf.Max(0f, defense);
            MoveSpeed = Mathf.Max(0.1f, moveSpeed);
            AttackRange = Mathf.Max(0.1f, attackRange);
            AttackInterval = Mathf.Max(0.1f, attackInterval);
            AggroRadius = Mathf.Max(0.1f, aggroRadius);
            HealthRegen = Mathf.Max(0f, healthRegen);
            ResourceType = resourceType;
            MaxResource = Mathf.Max(0f, maxResource);
            InitialResource = Mathf.Max(0f, initialResource);
            ResourceRegen = Mathf.Max(0f, resourceRegen);
            MovePriority = movePriority;
            DefaultSkillIds = defaultSkillIds ?? Array.Empty<int>();
            BarPrefabType = barPrefabType ?? string.Empty;
            RequiredExperienceByLevel = requiredExperienceByLevel ?? Array.Empty<int>();
        }

        public int HeroId { get; }
        public string HeroKey { get; }
        public string DisplayName { get; }
        public string ClassType { get; }
        public int Level { get; }
        public int MaxLevel { get; }
        public float Health { get; }
        public float Attack { get; }
        public float Defense { get; }
        public float MoveSpeed { get; }
        public float AttackRange { get; }
        public float AttackInterval { get; }
        public float AggroRadius { get; }
        public float HealthRegen { get; }
        public IDRPG3DCombatResourceType ResourceType { get; }
        public float MaxResource { get; }
        public float InitialResource { get; }
        public float ResourceRegen { get; }
        public int MovePriority { get; }
        public int[] DefaultSkillIds { get; }
        public string BarPrefabType { get; }
        public int[] RequiredExperienceByLevel { get; }
    }
}
