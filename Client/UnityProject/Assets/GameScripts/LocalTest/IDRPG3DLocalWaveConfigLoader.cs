using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameConfig;
using IDRPG3D.GameplayPrototype;
using Luban;
using UnityEngine;
using EnemyConfig = GameConfig.stage.Enemy;
using EnemyLevelConfig = GameConfig.stage.EnemyLevel;
using StageConfig = GameConfig.stage.Stage;
using WaveConfig = GameConfig.stage.Wave;

namespace IDRPG3D.LocalTest
{
    public sealed class IDRPG3DLocalWaveConfigLoader
    {
        private const string ConfigBytesRoot = "AssetRaw/Configs/bytes";
        private const string BytesExtension = ".bytes";

        private Tables tables;

        public bool TryBuildStage(int stageId, out IDRPG3DLocalStageWaveConfig config)
        {
            config = default;

            try
            {
                var loadedTables = GetTables();
                var stage = loadedTables.TbStage.GetOrDefault(stageId);
                if (stage == null)
                {
                    return false;
                }

                var waves = loadedTables.TbWave.DataList
                    .Where(wave => wave.StageId == stageId)
                    .OrderBy(wave => wave.WaveIndex)
                    .Select(BuildWaveDefinition)
                    .ToList();
                if (waves.Count == 0)
                {
                    return false;
                }

                config = new IDRPG3DLocalStageWaveConfig(
                    stage.Id,
                    stage.StageKey,
                    stage.RouteKey,
                    new IDRPG3DStageLevelRule(
                        stage.LevelMode,
                        stage.MinEnemyLevel,
                        stage.MaxEnemyLevel,
                        stage.BaseLevelOffset,
                        stage.PowerScale),
                    string.Equals(stage.LoopMode, "Repeat", StringComparison.OrdinalIgnoreCase),
                    waves);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Failed to load wave config {stageId}: {exception.Message}");
                return false;
            }
        }

        public bool TryBuildEnemyStats(int enemyId, int level, out IDRPG3DLocalEnemyConfig config)
        {
            config = default;

            try
            {
                var loadedTables = GetTables();
                var enemy = loadedTables.TbEnemy.GetOrDefault(enemyId);
                if (enemy == null)
                {
                    return false;
                }

                var enemyLevel = FindEnemyLevel(loadedTables, enemyId, level);
                config = BuildEnemyConfig(enemy, enemyLevel, level);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Failed to load enemy config {enemyId} Lv{level}: {exception.Message}");
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

        private static IDRPG3DWaveDefinition BuildWaveDefinition(WaveConfig wave)
        {
            return new IDRPG3DWaveDefinition(
                wave.Id,
                wave.StageId,
                wave.WaveIndex,
                ParseSpawnMode(wave.SpawnMode),
                wave.EnemyId,
                wave.EnemyLevel,
                new IDRPG3DWaveLevelRule(
                    wave.LevelMode,
                    wave.FixedEnemyLevel,
                    wave.LevelOffset,
                    wave.MinEnemyLevel,
                    wave.MaxEnemyLevel),
                wave.Count,
                wave.SpawnDistanceAhead,
                wave.SpawnRadius,
                wave.SpawnAnchorId,
                wave.IsBoss != 0,
                wave.NextWaveDelay,
                ParseEngageMode(wave.SpawnEngageMode, wave.IsBoss != 0),
                wave.HpMultiplier,
                wave.AttackMultiplier);
        }

        private static EnemyLevelConfig FindEnemyLevel(Tables loadedTables, int enemyId, int level)
        {
            var expectedId = enemyId * 100 + level;
            var enemyLevel = loadedTables.TbEnemyLevel.GetOrDefault(expectedId);
            if (enemyLevel != null)
            {
                return enemyLevel;
            }

            var levels = loadedTables.TbEnemyLevel.DataList;
            for (var i = 0; i < levels.Count; i++)
            {
                var candidate = levels[i];
                if (candidate.EnemyId == enemyId && candidate.Level == level)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static IDRPG3DLocalEnemyConfig BuildEnemyConfig(
            EnemyConfig enemy,
            EnemyLevelConfig enemyLevel,
            int requestedLevel)
        {
            var hpMultiplier = enemyLevel != null ? enemyLevel.HpMultiplier : 1f;
            var attackMultiplier = enemyLevel != null ? enemyLevel.AttackMultiplier : 1f;
            var defenseMultiplier = enemyLevel != null ? enemyLevel.DefenseMultiplier : 1f;
            return new IDRPG3DLocalEnemyConfig(
                enemy.Id,
                requestedLevel,
                enemy.EnemyKey,
                enemy.Name,
                enemy.TemplateKey,
                enemy.BaseHp * hpMultiplier,
                enemy.BaseAttack * attackMultiplier,
                enemy.BaseDefense * defenseMultiplier,
                Mathf.RoundToInt(enemy.BaseExp * (enemyLevel != null ? enemyLevel.ExpMultiplier : 1f)),
                enemy.MoveSpeed,
                enemy.AttackRange,
                enemy.AttackInterval,
                enemy.AggroRadius,
                enemy.MovePriority,
                enemy.VisualScale);
        }

        private static IDRPG3DWaveSpawnMode ParseSpawnMode(string value)
        {
            return Enum.TryParse(value, true, out IDRPG3DWaveSpawnMode mode)
                ? mode
                : IDRPG3DWaveSpawnMode.SplineAhead;
        }

        private static IDRPG3DWaveEngageMode ParseEngageMode(string value, bool isBoss)
        {
            if (Enum.TryParse(value, true, out IDRPG3DWaveEngageMode mode))
            {
                return mode;
            }

            return isBoss ? IDRPG3DWaveEngageMode.HoldPosition : IDRPG3DWaveEngageMode.RushTeam;
        }
    }

    public readonly struct IDRPG3DLocalStageWaveConfig
    {
        public IDRPG3DLocalStageWaveConfig(
            int stageId,
            string stageKey,
            string routeKey,
            IDRPG3DStageLevelRule levelRule,
            bool loopStage,
            IReadOnlyList<IDRPG3DWaveDefinition> waves)
        {
            StageId = stageId;
            StageKey = stageKey ?? string.Empty;
            RouteKey = routeKey ?? string.Empty;
            LevelRule = levelRule;
            LoopStage = loopStage;
            Waves = waves ?? Array.Empty<IDRPG3DWaveDefinition>();
        }

        public int StageId { get; }
        public string StageKey { get; }
        public string RouteKey { get; }
        public IDRPG3DStageLevelRule LevelRule { get; }
        public bool LoopStage { get; }
        public IReadOnlyList<IDRPG3DWaveDefinition> Waves { get; }
    }

    public readonly struct IDRPG3DLocalEnemyConfig
    {
        public IDRPG3DLocalEnemyConfig(
            int enemyId,
            int level,
            string enemyKey,
            string displayName,
            string templateKey,
            float health,
            float attack,
            float defense,
            int experienceReward,
            float moveSpeed,
            float attackRange,
            float attackInterval,
            float aggroRadius,
            int movePriority,
            float visualScale)
        {
            EnemyId = enemyId;
            Level = Mathf.Max(1, level);
            EnemyKey = enemyKey ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            TemplateKey = templateKey ?? string.Empty;
            Health = Mathf.Max(1f, health);
            Attack = Mathf.Max(0f, attack);
            Defense = Mathf.Max(0f, defense);
            ExperienceReward = Mathf.Max(0, experienceReward);
            MoveSpeed = Mathf.Max(0.1f, moveSpeed);
            AttackRange = Mathf.Max(0.1f, attackRange);
            AttackInterval = Mathf.Max(0.1f, attackInterval);
            AggroRadius = Mathf.Max(0.1f, aggroRadius);
            MovePriority = movePriority;
            VisualScale = Mathf.Max(0.01f, visualScale);
        }

        public int EnemyId { get; }
        public int Level { get; }
        public string EnemyKey { get; }
        public string DisplayName { get; }
        public string TemplateKey { get; }
        public float Health { get; }
        public float Attack { get; }
        public float Defense { get; }
        public int ExperienceReward { get; }
        public float MoveSpeed { get; }
        public float AttackRange { get; }
        public float AttackInterval { get; }
        public float AggroRadius { get; }
        public int MovePriority { get; }
        public float VisualScale { get; }
    }
}
