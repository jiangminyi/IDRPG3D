using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public enum IDRPG3DPrototypeEffectType
    {
        None,
        Damage,
        Heal,
        AddBuff
    }

    public enum IDRPG3DPrototypeStatType
    {
        None,
        MoveSpeed,
        Attack,
        AttackSpeed,
        CastSpeed
    }

    public enum IDRPG3DPrototypeModifierType
    {
        None,
        Add,
        Percent,
        Multiply
    }

    [Serializable]
    public readonly struct IDRPG3DPrototypeBuffDefinition
    {
        public IDRPG3DPrototypeBuffDefinition(
            int buffId,
            string buffKey,
            string displayName,
            float duration,
            int maxStack,
            IDRPG3DPrototypeStatType statType,
            IDRPG3DPrototypeModifierType modifierType,
            float modifierValue)
        {
            BuffId = buffId;
            BuffKey = buffKey;
            DisplayName = displayName;
            Duration = Mathf.Max(0f, duration);
            MaxStack = Mathf.Max(1, maxStack);
            StatType = statType;
            ModifierType = modifierType;
            ModifierValue = modifierValue;
        }

        public int BuffId { get; }
        public string BuffKey { get; }
        public string DisplayName { get; }
        public float Duration { get; }
        public int MaxStack { get; }
        public IDRPG3DPrototypeStatType StatType { get; }
        public IDRPG3DPrototypeModifierType ModifierType { get; }
        public float ModifierValue { get; }
        public bool IsValid => BuffId > 0 && Duration > 0f;

        public static IDRPG3DPrototypeBuffDefinition StatModifier(
            int buffId,
            string buffKey,
            string displayName,
            float duration,
            int maxStack,
            IDRPG3DPrototypeStatType statType,
            IDRPG3DPrototypeModifierType modifierType,
            float modifierValue)
        {
            return new IDRPG3DPrototypeBuffDefinition(
                buffId,
                buffKey,
                displayName,
                duration,
                maxStack,
                statType,
                modifierType,
                modifierValue);
        }
    }

    [Serializable]
    public readonly struct IDRPG3DPrototypeEffectDefinition
    {
        public IDRPG3DPrototypeEffectDefinition(
            int effectId,
            IDRPG3DPrototypeEffectType effectType,
            float value,
            IDRPG3DPrototypeBuffDefinition buff)
        {
            EffectId = effectId;
            EffectType = effectType;
            Value = Mathf.Max(0f, value);
            Buff = buff;
        }

        public int EffectId { get; }
        public IDRPG3DPrototypeEffectType EffectType { get; }
        public float Value { get; }
        public IDRPG3DPrototypeBuffDefinition Buff { get; }
        public bool HasBuff => Buff.IsValid;
        public bool IsValid => EffectType != IDRPG3DPrototypeEffectType.None && (Value > 0f || HasBuff);

        public static IDRPG3DPrototypeEffectDefinition Damage(int effectId, float value)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.Damage,
                value,
                default);
        }

        public static IDRPG3DPrototypeEffectDefinition DamageWithBuff(
            int effectId,
            float value,
            IDRPG3DPrototypeBuffDefinition buff)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.Damage,
                value,
                buff);
        }
    }

    public readonly struct IDRPG3DPrototypeEffectResult
    {
        public IDRPG3DPrototypeEffectResult(
            bool applied,
            int effectId,
            int buffId,
            float value)
        {
            Applied = applied;
            EffectId = effectId;
            BuffId = buffId;
            Value = value;
        }

        public bool Applied { get; }
        public int EffectId { get; }
        public int BuffId { get; }
        public float Value { get; }
    }

    public static class IDRPG3DPrototypeEffectRunner
    {
        public static IDRPG3DPrototypeEffectResult Apply(
            IDRPG3DPrototypeEffectDefinition effect,
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit target)
        {
            if (!effect.IsValid || target == null || !target.IsAlive)
            {
                return default;
            }

            var appliedValue = 0f;
            if (effect.EffectType == IDRPG3DPrototypeEffectType.Damage)
            {
                target.TakeDamage(effect.Value, source);
                appliedValue = effect.Value;
            }

            var buffId = 0;
            if (effect.HasBuff)
            {
                var controller = target.GetComponent<IDRPG3DPrototypeBuffController>();
                if (controller == null)
                {
                    controller = target.gameObject.AddComponent<IDRPG3DPrototypeBuffController>();
                }

                controller.ApplyBuff(effect.Buff, source);
                buffId = effect.Buff.BuffId;
            }

            return new IDRPG3DPrototypeEffectResult(true, effect.EffectId, buffId, appliedValue);
        }
    }

    public sealed class IDRPG3DPrototypeBuffController : MonoBehaviour
    {
        private readonly Dictionary<int, ActiveBuff> activeBuffs = new Dictionary<int, ActiveBuff>();

        public int ActiveBuffCount => activeBuffs.Count;
        public float MoveSpeedMultiplier => CalculateMultiplier(IDRPG3DPrototypeStatType.MoveSpeed);

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void ApplyBuff(IDRPG3DPrototypeBuffDefinition definition, IDRPG3DCombatUnit source)
        {
            if (!definition.IsValid)
            {
                return;
            }

            if (activeBuffs.TryGetValue(definition.BuffId, out var active))
            {
                active.Refresh(definition);
                return;
            }

            activeBuffs.Add(definition.BuffId, new ActiveBuff(definition));
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || activeBuffs.Count == 0)
            {
                return;
            }

            List<int> expired = null;
            foreach (var pair in activeBuffs)
            {
                pair.Value.Tick(deltaTime);
                if (pair.Value.RemainingTime <= 0f)
                {
                    expired ??= new List<int>();
                    expired.Add(pair.Key);
                }
            }

            if (expired == null)
            {
                return;
            }

            for (var i = 0; i < expired.Count; i++)
            {
                activeBuffs.Remove(expired[i]);
            }
        }

        public int GetStack(int buffId)
        {
            return activeBuffs.TryGetValue(buffId, out var active) ? active.Stack : 0;
        }

        private float CalculateMultiplier(IDRPG3DPrototypeStatType statType)
        {
            var multiplier = 1f;
            foreach (var pair in activeBuffs)
            {
                var active = pair.Value;
                if (active.Definition.StatType != statType)
                {
                    continue;
                }

                if (active.Definition.ModifierType == IDRPG3DPrototypeModifierType.Percent)
                {
                    multiplier += active.Definition.ModifierValue * active.Stack;
                }
                else if (active.Definition.ModifierType == IDRPG3DPrototypeModifierType.Multiply)
                {
                    multiplier *= Mathf.Pow(active.Definition.ModifierValue, active.Stack);
                }
            }

            return Mathf.Max(0.05f, multiplier);
        }

        private sealed class ActiveBuff
        {
            public ActiveBuff(IDRPG3DPrototypeBuffDefinition definition)
            {
                Definition = definition;
                Stack = 1;
                RemainingTime = definition.Duration;
            }

            public IDRPG3DPrototypeBuffDefinition Definition { get; private set; }
            public int Stack { get; private set; }
            public float RemainingTime { get; private set; }

            public void Refresh(IDRPG3DPrototypeBuffDefinition definition)
            {
                Definition = definition;
                Stack = Mathf.Min(definition.MaxStack, Stack + 1);
                RemainingTime = definition.Duration;
            }

            public void Tick(float deltaTime)
            {
                RemainingTime -= deltaTime;
            }
        }
    }
}
