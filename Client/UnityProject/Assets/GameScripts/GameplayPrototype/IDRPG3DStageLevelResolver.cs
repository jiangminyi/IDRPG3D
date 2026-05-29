using System;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public readonly struct IDRPG3DStageLevelRule
    {
        public IDRPG3DStageLevelRule(
            string levelMode,
            int minEnemyLevel,
            int maxEnemyLevel,
            int baseLevelOffset,
            float powerScale)
        {
            LevelMode = string.IsNullOrWhiteSpace(levelMode) ? "Fixed" : levelMode;
            MinEnemyLevel = Mathf.Max(1, minEnemyLevel);
            MaxEnemyLevel = Mathf.Max(MinEnemyLevel, maxEnemyLevel);
            BaseLevelOffset = baseLevelOffset;
            PowerScale = Mathf.Max(0.01f, powerScale);
        }

        public string LevelMode { get; }
        public int MinEnemyLevel { get; }
        public int MaxEnemyLevel { get; }
        public int BaseLevelOffset { get; }
        public float PowerScale { get; }
    }

    public readonly struct IDRPG3DWaveLevelRule
    {
        public IDRPG3DWaveLevelRule(
            string levelMode,
            int fixedEnemyLevel,
            int levelOffset,
            int minEnemyLevel,
            int maxEnemyLevel)
        {
            LevelMode = string.IsNullOrWhiteSpace(levelMode) ? "Inherit" : levelMode;
            FixedEnemyLevel = fixedEnemyLevel;
            LevelOffset = levelOffset;
            MinEnemyLevel = minEnemyLevel;
            MaxEnemyLevel = maxEnemyLevel;
        }

        public string LevelMode { get; }
        public int FixedEnemyLevel { get; }
        public int LevelOffset { get; }
        public int MinEnemyLevel { get; }
        public int MaxEnemyLevel { get; }
    }

    public static class IDRPG3DStageLevelResolver
    {
        public static int ResolveEnemyLevel(
            IDRPG3DStageLevelRule stage,
            IDRPG3DWaveLevelRule wave,
            int partyReferenceLevel,
            int fallbackLevel)
        {
            var minLevel = wave.MinEnemyLevel > 0 ? wave.MinEnemyLevel : stage.MinEnemyLevel;
            var maxLevel = wave.MaxEnemyLevel > 0 ? wave.MaxEnemyLevel : stage.MaxEnemyLevel;
            if (maxLevel < minLevel)
            {
                maxLevel = minLevel;
            }

            var levelMode = string.Equals(wave.LevelMode, "Inherit", StringComparison.OrdinalIgnoreCase)
                ? stage.LevelMode
                : wave.LevelMode;

            var targetLevel = string.Equals(levelMode, "Fixed", StringComparison.OrdinalIgnoreCase)
                ? (wave.FixedEnemyLevel > 0 ? wave.FixedEnemyLevel : fallbackLevel)
                : Mathf.Max(1, partyReferenceLevel) + stage.BaseLevelOffset + wave.LevelOffset;

            return Mathf.Clamp(Mathf.Max(1, targetLevel), minLevel, maxLevel);
        }
    }
}
