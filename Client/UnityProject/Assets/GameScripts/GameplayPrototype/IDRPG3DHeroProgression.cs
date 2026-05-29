using System;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DHeroProgression : MonoBehaviour
    {
        [SerializeField] private int heroId;
        [SerializeField] private int level = 1;
        [SerializeField] private int currentExperience;
        [SerializeField] private int maxLevel = 1;

        private IDRPG3DCombatUnit unit;
        private int[] requiredExperienceByLevel = Array.Empty<int>();

        public int HeroId => heroId;
        public int Level => level;
        public int CurrentExperience => currentExperience;
        public int MaxLevel => maxLevel;
        public int NextLevelRequiredExperience => level < maxLevel && level < requiredExperienceByLevel.Length
            ? requiredExperienceByLevel[level]
            : 0;

        public event Action<IDRPG3DHeroProgression> LevelChanged;
        public event Action<IDRPG3DHeroProgression> ExperienceChanged;

        public void Configure(
            IDRPG3DCombatUnit targetUnit,
            int heroId,
            int startLevel,
            int maxLevel,
            int[] requiredExperienceByLevel)
        {
            unit = targetUnit != null ? targetUnit : GetComponent<IDRPG3DCombatUnit>();
            this.heroId = heroId;
            this.maxLevel = Mathf.Max(1, maxLevel);
            this.requiredExperienceByLevel = requiredExperienceByLevel ?? Array.Empty<int>();
            level = Mathf.Clamp(startLevel, 1, this.maxLevel);
            currentExperience = 0;
            unit?.SetLevel(level);
            LevelChanged?.Invoke(this);
            ExperienceChanged?.Invoke(this);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || level >= maxLevel)
            {
                return;
            }

            currentExperience += amount;
            var leveled = false;
            while (level < maxLevel)
            {
                var required = NextLevelRequiredExperience;
                if (required <= 0 || currentExperience < required)
                {
                    break;
                }

                currentExperience -= required;
                level++;
                unit?.SetLevel(level);
                leveled = true;
            }

            ExperienceChanged?.Invoke(this);
            if (leveled)
            {
                LevelChanged?.Invoke(this);
            }
        }
    }
}
