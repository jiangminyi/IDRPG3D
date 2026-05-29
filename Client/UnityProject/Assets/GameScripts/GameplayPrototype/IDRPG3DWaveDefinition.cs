using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public readonly struct IDRPG3DWaveDefinition
    {
        public IDRPG3DWaveDefinition(
            int waveId,
            int stageId,
            int waveIndex,
            IDRPG3DWaveSpawnMode spawnMode,
            int enemyId,
            int enemyLevel,
            int count,
            float spawnDistanceAhead,
            float spawnRadius,
            string spawnAnchorId,
            bool isBoss,
            float nextWaveDelay)
            : this(
                waveId,
                stageId,
                waveIndex,
                spawnMode,
                enemyId,
                enemyLevel,
                new IDRPG3DWaveLevelRule("Fixed", enemyLevel, 0, 0, 0),
                count,
                spawnDistanceAhead,
                spawnRadius,
                spawnAnchorId,
                isBoss,
                nextWaveDelay,
                isBoss ? IDRPG3DWaveEngageMode.HoldPosition : IDRPG3DWaveEngageMode.RushTeam)
        {
        }

        public IDRPG3DWaveDefinition(
            int waveId,
            int stageId,
            int waveIndex,
            IDRPG3DWaveSpawnMode spawnMode,
            int enemyId,
            int enemyLevel,
            int count,
            float spawnDistanceAhead,
            float spawnRadius,
            string spawnAnchorId,
            bool isBoss,
            float nextWaveDelay,
            IDRPG3DWaveEngageMode spawnEngageMode)
            : this(
                waveId,
                stageId,
                waveIndex,
                spawnMode,
                enemyId,
                enemyLevel,
                new IDRPG3DWaveLevelRule("Fixed", enemyLevel, 0, 0, 0),
                count,
                spawnDistanceAhead,
                spawnRadius,
                spawnAnchorId,
                isBoss,
                nextWaveDelay,
                spawnEngageMode)
        {
        }

        public IDRPG3DWaveDefinition(
            int waveId,
            int stageId,
            int waveIndex,
            IDRPG3DWaveSpawnMode spawnMode,
            int enemyId,
            int enemyLevel,
            IDRPG3DWaveLevelRule levelRule,
            int count,
            float spawnDistanceAhead,
            float spawnRadius,
            string spawnAnchorId,
            bool isBoss,
            float nextWaveDelay,
            IDRPG3DWaveEngageMode spawnEngageMode,
            float hpMultiplier = 1f,
            float attackMultiplier = 1f)
        {
            WaveId = waveId;
            StageId = stageId;
            WaveIndex = waveIndex;
            SpawnMode = spawnMode;
            EnemyId = enemyId;
            EnemyLevel = Mathf.Max(1, enemyLevel);
            LevelRule = levelRule;
            Count = Mathf.Max(1, count);
            SpawnDistanceAhead = Mathf.Max(0f, spawnDistanceAhead);
            SpawnRadius = Mathf.Max(0f, spawnRadius);
            SpawnAnchorId = spawnAnchorId ?? string.Empty;
            IsBoss = isBoss;
            NextWaveDelay = Mathf.Max(0f, nextWaveDelay);
            SpawnEngageMode = spawnEngageMode;
            HpMultiplier = Mathf.Max(0.01f, hpMultiplier);
            AttackMultiplier = Mathf.Max(0.01f, attackMultiplier);
        }

        public int WaveId { get; }
        public int StageId { get; }
        public int WaveIndex { get; }
        public IDRPG3DWaveSpawnMode SpawnMode { get; }
        public int EnemyId { get; }
        public int EnemyLevel { get; }
        public IDRPG3DWaveLevelRule LevelRule { get; }
        public int Count { get; }
        public float SpawnDistanceAhead { get; }
        public float SpawnRadius { get; }
        public string SpawnAnchorId { get; }
        public bool IsBoss { get; }
        public float NextWaveDelay { get; }
        public IDRPG3DWaveEngageMode SpawnEngageMode { get; }
        public float HpMultiplier { get; }
        public float AttackMultiplier { get; }
    }
}
