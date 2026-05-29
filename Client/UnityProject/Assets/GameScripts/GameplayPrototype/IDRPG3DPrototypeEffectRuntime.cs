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
        AddBuff,
        AreaDamage,
        AddThreat,
        GenerateResource,
        Resurrect
    }

    public enum IDRPG3DPrototypeStatType
    {
        None,
        MoveSpeed,
        Armor,
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

    public enum IDRPG3DPrototypeBuffType
    {
        None,
        StatModifier,
        DamageOverTime,
        Aura
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
            float modifierValue,
            IDRPG3DPrototypeBuffType buffType = IDRPG3DPrototypeBuffType.StatModifier,
            float tickInterval = 0f,
            float tickValue = 0f,
            float auraRadius = 0f,
            int auraBuffId = 0,
            string auraBuffKey = null,
            string auraBuffDisplayName = null,
            float auraBuffDuration = 0f,
            IDRPG3DPrototypeStatType auraStatType = IDRPG3DPrototypeStatType.None,
            IDRPG3DPrototypeModifierType auraModifierType = IDRPG3DPrototypeModifierType.None,
            float auraModifierValue = 0f)
        {
            BuffId = buffId;
            BuffKey = buffKey;
            DisplayName = displayName;
            Duration = Mathf.Max(0f, duration);
            MaxStack = Mathf.Max(1, maxStack);
            StatType = statType;
            ModifierType = modifierType;
            ModifierValue = modifierValue;
            BuffType = buffType;
            TickInterval = Mathf.Max(0f, tickInterval);
            TickValue = Mathf.Max(0f, tickValue);
            AuraRadius = Mathf.Max(0f, auraRadius);
            AuraBuffId = auraBuffId;
            AuraBuffKey = auraBuffKey;
            AuraBuffDisplayName = auraBuffDisplayName;
            AuraBuffDuration = Mathf.Max(0f, auraBuffDuration);
            AuraStatType = auraStatType;
            AuraModifierType = auraModifierType;
            AuraModifierValue = auraModifierValue;
        }

        public int BuffId { get; }
        public string BuffKey { get; }
        public string DisplayName { get; }
        public float Duration { get; }
        public int MaxStack { get; }
        public IDRPG3DPrototypeStatType StatType { get; }
        public IDRPG3DPrototypeModifierType ModifierType { get; }
        public float ModifierValue { get; }
        public IDRPG3DPrototypeBuffType BuffType { get; }
        public float TickInterval { get; }
        public float TickValue { get; }
        public float AuraRadius { get; }
        public int AuraBuffId { get; }
        public string AuraBuffKey { get; }
        public string AuraBuffDisplayName { get; }
        public float AuraBuffDuration { get; }
        public IDRPG3DPrototypeStatType AuraStatType { get; }
        public IDRPG3DPrototypeModifierType AuraModifierType { get; }
        public float AuraModifierValue { get; }
        public bool IsValid => BuffId > 0 && Duration > 0f;
        public bool HasAuraBuff => AuraBuffId > 0 && AuraBuffDuration > 0f;
        public bool IsControl => BuffKey != null && BuffKey.IndexOf("slow", StringComparison.OrdinalIgnoreCase) >= 0;

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

        public static IDRPG3DPrototypeBuffDefinition DamageOverTime(
            int buffId,
            string buffKey,
            string displayName,
            float duration,
            int maxStack,
            float tickInterval,
            float tickValue)
        {
            return new IDRPG3DPrototypeBuffDefinition(
                buffId,
                buffKey,
                displayName,
                duration,
                maxStack,
                IDRPG3DPrototypeStatType.None,
                IDRPG3DPrototypeModifierType.None,
                0f,
                IDRPG3DPrototypeBuffType.DamageOverTime,
                tickInterval,
                tickValue);
        }

        public static IDRPG3DPrototypeBuffDefinition Aura(
            int buffId,
            string buffKey,
            string displayName,
            float duration,
            float tickInterval,
            float auraRadius,
            IDRPG3DPrototypeBuffDefinition auraBuff)
        {
            return new IDRPG3DPrototypeBuffDefinition(
                buffId,
                buffKey,
                displayName,
                duration,
                1,
                IDRPG3DPrototypeStatType.None,
                IDRPG3DPrototypeModifierType.None,
                0f,
                IDRPG3DPrototypeBuffType.Aura,
                tickInterval,
                0f,
                auraRadius,
                auraBuff.BuffId,
                auraBuff.BuffKey,
                auraBuff.DisplayName,
                auraBuff.Duration,
                auraBuff.StatType,
                auraBuff.ModifierType,
                auraBuff.ModifierValue);
        }

        public IDRPG3DPrototypeBuffDefinition CreateAuraBuff()
        {
            return StatModifier(
                AuraBuffId,
                AuraBuffKey,
                AuraBuffDisplayName,
                AuraBuffDuration,
                1,
                AuraStatType,
                AuraModifierType,
                AuraModifierValue);
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

        public static IDRPG3DPrototypeEffectDefinition Heal(int effectId, float value)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.Heal,
                value,
                default);
        }

        public static IDRPG3DPrototypeEffectDefinition AddBuff(
            int effectId,
            IDRPG3DPrototypeBuffDefinition buff)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.AddBuff,
                0f,
                buff);
        }

        public static IDRPG3DPrototypeEffectDefinition AreaDamage(int effectId, float value)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.AreaDamage,
                value,
                default);
        }

        public static IDRPG3DPrototypeEffectDefinition AddThreat(int effectId, float value)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.AddThreat,
                value,
                default);
        }

        public static IDRPG3DPrototypeEffectDefinition GenerateResource(int effectId, float value)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.GenerateResource,
                value,
                default);
        }

        public static IDRPG3DPrototypeEffectDefinition Resurrect(int effectId, float healthRatio)
        {
            return new IDRPG3DPrototypeEffectDefinition(
                effectId,
                IDRPG3DPrototypeEffectType.Resurrect,
                healthRatio,
                default);
        }
    }

    public readonly struct IDRPG3DPrototypeEffectResult
    {
        public IDRPG3DPrototypeEffectResult(
            bool applied,
            int effectId,
            int buffId,
            float value,
            int buffStack = 0,
            float buffRemainingTime = 0f)
        {
            Applied = applied;
            EffectId = effectId;
            BuffId = buffId;
            Value = value;
            BuffStack = buffStack;
            BuffRemainingTime = buffRemainingTime;
        }

        public bool Applied { get; }
        public int EffectId { get; }
        public int BuffId { get; }
        public float Value { get; }
        public int BuffStack { get; }
        public float BuffRemainingTime { get; }
    }

    public static class IDRPG3DPrototypeEffectRunner
    {
        public static IDRPG3DPrototypeEffectResult Apply(
            IDRPG3DPrototypeEffectDefinition effect,
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit target)
        {
            return Apply(effect, source, target, 1f);
        }

        public static IDRPG3DPrototypeEffectResult Apply(
            IDRPG3DPrototypeEffectDefinition effect,
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit target,
            float threatMultiplier)
        {
            if (!effect.IsValid || target == null)
            {
                return default;
            }

            if (effect.EffectType != IDRPG3DPrototypeEffectType.Resurrect && !target.IsAlive)
            {
                return default;
            }

            var appliedValue = 0f;
            if (effect.EffectType == IDRPG3DPrototypeEffectType.Damage
                || effect.EffectType == IDRPG3DPrototypeEffectType.AreaDamage)
            {
                target.TakeDamage(effect.Value, source);
                AddBonusThreat(target, source, effect.Value, threatMultiplier);
                appliedValue = effect.Value;
            }
            else if (effect.EffectType == IDRPG3DPrototypeEffectType.Heal)
            {
                appliedValue = target.Heal(effect.Value, source);
            }
            else if (effect.EffectType == IDRPG3DPrototypeEffectType.GenerateResource)
            {
                var resource = source != null ? source.GetComponent<IDRPG3DCombatResource>() : null;
                resource?.Gain(effect.Value);
                appliedValue = effect.Value;
            }
            else if (effect.EffectType == IDRPG3DPrototypeEffectType.Resurrect)
            {
                if (target.Revive(effect.Value))
                {
                    appliedValue = effect.Value;
                }
            }
            else if (effect.EffectType == IDRPG3DPrototypeEffectType.AddThreat)
            {
                if (source != null)
                {
                    target.ThreatTable.AddThreat(source, effect.Value);
                    appliedValue = effect.Value;
                }
            }

            var buffId = 0;
            if (effect.HasBuff)
            {
                if (target.IsControlImmune && effect.Buff.IsControl)
                {
                    return new IDRPG3DPrototypeEffectResult(false, effect.EffectId, 0, appliedValue);
                }

                var controller = target.GetComponent<IDRPG3DPrototypeBuffController>();
                if (controller == null)
                {
                    controller = target.gameObject.AddComponent<IDRPG3DPrototypeBuffController>();
                }

                controller.ApplyBuff(effect.Buff, source);
                buffId = effect.Buff.BuffId;
            }

            var buffController = buffId > 0 ? target.GetComponent<IDRPG3DPrototypeBuffController>() : null;
            return new IDRPG3DPrototypeEffectResult(
                true,
                effect.EffectId,
                buffId,
                appliedValue,
                buffController != null ? buffController.GetStack(buffId) : 0,
                buffController != null ? buffController.GetRemainingTime(buffId) : 0f);
        }

        private static void AddBonusThreat(
            IDRPG3DCombatUnit target,
            IDRPG3DCombatUnit source,
            float baseThreat,
            float threatMultiplier)
        {
            if (target == null || source == null || baseThreat <= 0f || threatMultiplier <= 1f)
            {
                return;
            }

            target.ThreatTable.AddThreat(source, baseThreat * (threatMultiplier - 1f));
        }
    }

    public sealed class IDRPG3DPrototypeBuffController : MonoBehaviour
    {
        private readonly Dictionary<int, ActiveBuff> activeBuffs = new Dictionary<int, ActiveBuff>();

        public int ActiveBuffCount => activeBuffs.Count;
        public float MoveSpeedMultiplier => CalculateMultiplier(IDRPG3DPrototypeStatType.MoveSpeed);
        public float AttackSpeedMultiplier => CalculateAttackSpeedMultiplier();
        public float CastSpeedMultiplier => CalculateMultiplier(IDRPG3DPrototypeStatType.CastSpeed);
        public float ArmorBonus => CalculateAdditive(IDRPG3DPrototypeStatType.Armor);

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

            activeBuffs.Add(definition.BuffId, new ActiveBuff(definition, source));
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
                pair.Value.Tick(deltaTime, this);
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

        public float GetRemainingTime(int buffId)
        {
            return activeBuffs.TryGetValue(buffId, out var active) ? active.RemainingTime : 0f;
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

        private float CalculateAttackSpeedMultiplier()
        {
            var multiplier = CalculateMultiplier(IDRPG3DPrototypeStatType.AttackSpeed);
            foreach (var pair in activeBuffs)
            {
                var active = pair.Value;
                if (active.Definition.StatType != IDRPG3DPrototypeStatType.MoveSpeed || !active.Definition.IsControl)
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

        private float CalculateAdditive(IDRPG3DPrototypeStatType statType)
        {
            var value = 0f;
            foreach (var pair in activeBuffs)
            {
                var active = pair.Value;
                if (active.Definition.StatType != statType)
                {
                    continue;
                }

                if (active.Definition.ModifierType == IDRPG3DPrototypeModifierType.Add)
                {
                    value += active.Definition.ModifierValue * active.Stack;
                }
            }

            return value;
        }

        private static IDRPG3DPrototypeBuffController GetOrAdd(IDRPG3DCombatUnit unit)
        {
            var controller = unit.GetComponent<IDRPG3DPrototypeBuffController>();
            return controller != null ? controller : unit.gameObject.AddComponent<IDRPG3DPrototypeBuffController>();
        }

        private sealed class ActiveBuff
        {
            public ActiveBuff(IDRPG3DPrototypeBuffDefinition definition, IDRPG3DCombatUnit source)
            {
                Definition = definition;
                Stack = 1;
                RemainingTime = definition.Duration;
                this.source = source;
            }

            public IDRPG3DPrototypeBuffDefinition Definition { get; private set; }
            public int Stack { get; private set; }
            public float RemainingTime { get; private set; }
            private IDRPG3DCombatUnit source;
            private float tickAccumulator;

            public void Refresh(IDRPG3DPrototypeBuffDefinition definition)
            {
                Definition = definition;
                Stack = Mathf.Min(definition.MaxStack, Stack + 1);
                RemainingTime = definition.Duration;
            }

            public void Tick(float deltaTime, IDRPG3DPrototypeBuffController owner)
            {
                RemainingTime -= deltaTime;
                if (Definition.TickInterval <= 0f)
                {
                    return;
                }

                tickAccumulator += deltaTime;
                while (tickAccumulator >= Definition.TickInterval)
                {
                    tickAccumulator -= Definition.TickInterval;
                    ApplyPeriodicTick(owner);
                }
            }

            private void ApplyPeriodicTick(IDRPG3DPrototypeBuffController owner)
            {
                if (Definition.BuffType == IDRPG3DPrototypeBuffType.DamageOverTime)
                {
                    var target = owner.GetComponent<IDRPG3DCombatUnit>();
                    target?.TakeDamage(Definition.TickValue * Stack, source);
                }
                else if (Definition.BuffType == IDRPG3DPrototypeBuffType.Aura)
                {
                    var auraSource = owner.GetComponent<IDRPG3DCombatUnit>();
                    if (auraSource == null || !auraSource.IsAlive || !Definition.HasAuraBuff)
                    {
                        return;
                    }

                    var radius = Definition.AuraRadius;
#if UNITY_2023_1_OR_NEWER
                    var units = UnityEngine.Object.FindObjectsByType<IDRPG3DCombatUnit>(FindObjectsSortMode.None);
#else
                    var units = UnityEngine.Object.FindObjectsOfType<IDRPG3DCombatUnit>();
#endif
                    for (var i = 0; i < units.Length; i++)
                    {
                        var unit = units[i];
                        if (unit == null
                            || unit == auraSource
                            || !unit.IsAlive
                            || unit.Faction != auraSource.Faction)
                        {
                            continue;
                        }

                        if ((unit.transform.position - auraSource.transform.position).sqrMagnitude > radius * radius)
                        {
                            continue;
                        }

                        GetOrAdd(unit).ApplyBuff(Definition.CreateAuraBuff(), auraSource);
                    }
                }
            }

            public void SetSource(IDRPG3DCombatUnit value)
            {
                source = value;
            }
        }
    }
}
