using System;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public enum IDRPG3DCombatResourceType
    {
        None,
        Rage,
        Mana
    }

    public sealed class IDRPG3DCombatResource : MonoBehaviour
    {
        [SerializeField] private IDRPG3DCombatResourceType resourceType;
        [SerializeField] private float maxValue;
        [SerializeField] private float currentValue;
        [SerializeField] private float regenPerSecond;

        public IDRPG3DCombatResourceType ResourceType => resourceType;
        public float MaxValue => maxValue;
        public float CurrentValue => currentValue;
        public float FillAmount => maxValue > 0f ? Mathf.Clamp01(currentValue / maxValue) : 0f;
        public bool HasResource => resourceType != IDRPG3DCombatResourceType.None && maxValue > 0f;

        public event Action<IDRPG3DCombatResource> ResourceChanged;

        private void Update()
        {
            TickForTest(Time.deltaTime);
        }

        public void Configure(
            IDRPG3DCombatResourceType type,
            float maxValue,
            float initialValue,
            float regenPerSecond)
        {
            resourceType = type;
            this.maxValue = Mathf.Max(0f, maxValue);
            this.currentValue = Mathf.Clamp(initialValue, 0f, this.maxValue);
            this.regenPerSecond = Mathf.Max(0f, regenPerSecond);
            ResourceChanged?.Invoke(this);
        }

        public bool HasEnough(float amount)
        {
            return amount <= 0f || !HasResource || currentValue + 0.0001f >= amount;
        }

        public bool TrySpend(float amount)
        {
            if (amount <= 0f || !HasResource)
            {
                return true;
            }

            if (!HasEnough(amount))
            {
                return false;
            }

            currentValue = Mathf.Max(0f, currentValue - amount);
            ResourceChanged?.Invoke(this);
            return true;
        }

        public void Gain(float amount)
        {
            if (amount <= 0f || !HasResource)
            {
                return;
            }

            var nextValue = Mathf.Min(maxValue, currentValue + amount);
            if (Mathf.Approximately(nextValue, currentValue))
            {
                return;
            }

            currentValue = nextValue;
            ResourceChanged?.Invoke(this);
        }

        public void SetMaxValue(float value, bool keepFillAmount)
        {
            var oldFill = FillAmount;
            maxValue = Mathf.Max(0f, value);
            currentValue = keepFillAmount ? maxValue * oldFill : Mathf.Min(currentValue, maxValue);
            ResourceChanged?.Invoke(this);
        }

        public void TickForTest(float deltaTime)
        {
            if (deltaTime <= 0f || regenPerSecond <= 0f || !HasResource || currentValue >= maxValue)
            {
                return;
            }

            Gain(regenPerSecond * deltaTime);
        }

        public static IDRPG3DCombatResourceType ParseType(string value)
        {
            return Enum.TryParse(value, true, out IDRPG3DCombatResourceType result)
                ? result
                : IDRPG3DCombatResourceType.None;
        }
    }
}
