using System;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public enum IDRPG3DCombatEventType
    {
        None,
        CastStart,
        ProjectileSpawn,
        ProjectileImpact,
        ApplyEffect,
        ApplyBuff,
        CastEnd,
        CastInterrupted
    }

    public readonly struct IDRPG3DCombatEvent
    {
        public IDRPG3DCombatEvent(
            IDRPG3DCombatEventType eventType,
            int sequence,
            int actionId,
            int projectileId,
            int casterUnitId,
            int sourceUnitId,
            int targetUnitId,
            int skillId,
            string skillKey,
            int effectId,
            int buffId,
            int stack,
            float value,
            float targetHealth,
            float remainingTime,
            float eventTime,
            Vector3 position)
        {
            EventType = eventType;
            Sequence = sequence;
            ActionId = actionId;
            ProjectileId = projectileId;
            CasterUnitId = casterUnitId;
            SourceUnitId = sourceUnitId;
            TargetUnitId = targetUnitId;
            SkillId = skillId;
            SkillKey = skillKey;
            EffectId = effectId;
            BuffId = buffId;
            Stack = stack;
            Value = value;
            TargetHealth = targetHealth;
            RemainingTime = remainingTime;
            EventTime = eventTime;
            Position = position;
        }

        public IDRPG3DCombatEventType EventType { get; }
        public int Sequence { get; }
        public int ActionId { get; }
        public int ProjectileId { get; }
        public int CasterUnitId { get; }
        public int SourceUnitId { get; }
        public int TargetUnitId { get; }
        public int SkillId { get; }
        public string SkillKey { get; }
        public int EffectId { get; }
        public int BuffId { get; }
        public int Stack { get; }
        public float Value { get; }
        public float TargetHealth { get; }
        public float RemainingTime { get; }
        public float EventTime { get; }
        public Vector3 Position { get; }
    }

    public readonly struct IDRPG3DCombatAction
    {
        public IDRPG3DCombatAction(
            int actionId,
            int sequence,
            int casterUnitId,
            int targetUnitId,
            int skillId,
            string skillKey,
            float startTime)
        {
            ActionId = actionId;
            Sequence = sequence;
            CasterUnitId = casterUnitId;
            TargetUnitId = targetUnitId;
            SkillId = skillId;
            SkillKey = skillKey;
            StartTime = startTime;
        }

        public int ActionId { get; }
        public int Sequence { get; }
        public int CasterUnitId { get; }
        public int TargetUnitId { get; }
        public int SkillId { get; }
        public string SkillKey { get; }
        public float StartTime { get; }
    }

    public static class IDRPG3DCombatEventStream
    {
        private static int nextSequence = 1;
        private static int nextActionId = 1;
        private static int nextProjectileId = 1;

        public static event Action<IDRPG3DCombatEvent> EventPublished;

        public static IDRPG3DCombatAction BeginCast(
            IDRPG3DCombatUnit caster,
            IDRPG3DCombatUnit target,
            IDRPG3DPrototypeSkillDefinition skill,
            Vector3 position)
        {
            var action = new IDRPG3DCombatAction(
                nextActionId++,
                nextSequence++,
                caster != null ? caster.UnitId : 0,
                target != null ? target.UnitId : 0,
                skill.ConfigId,
                skill.SkillId,
                Time.time);

            Publish(new IDRPG3DCombatEvent(
                IDRPG3DCombatEventType.CastStart,
                action.Sequence,
                action.ActionId,
                projectileId: 0,
                action.CasterUnitId,
                sourceUnitId: action.CasterUnitId,
                action.TargetUnitId,
                action.SkillId,
                action.SkillKey,
                effectId: 0,
                buffId: 0,
                stack: 0,
                value: 0f,
                targetHealth: target != null ? target.Health : 0f,
                remainingTime: 0f,
                Time.time,
                position));

            return action;
        }

        public static int PublishProjectileSpawn(IDRPG3DCombatAction action, Vector3 position)
        {
            var projectileId = nextProjectileId++;
            Publish(new IDRPG3DCombatEvent(
                IDRPG3DCombatEventType.ProjectileSpawn,
                action.Sequence,
                action.ActionId,
                projectileId,
                action.CasterUnitId,
                action.CasterUnitId,
                action.TargetUnitId,
                action.SkillId,
                action.SkillKey,
                effectId: 0,
                buffId: 0,
                stack: 0,
                value: 0f,
                targetHealth: 0f,
                remainingTime: 0f,
                Time.time,
                position));
            return projectileId;
        }

        public static void PublishProjectileImpact(IDRPG3DCombatAction action, int projectileId, IDRPG3DCombatUnit target, Vector3 position)
        {
            Publish(new IDRPG3DCombatEvent(
                IDRPG3DCombatEventType.ProjectileImpact,
                action.Sequence,
                action.ActionId,
                projectileId,
                action.CasterUnitId,
                action.CasterUnitId,
                target != null ? target.UnitId : action.TargetUnitId,
                action.SkillId,
                action.SkillKey,
                effectId: 0,
                buffId: 0,
                stack: 0,
                value: 0f,
                targetHealth: target != null ? target.Health : 0f,
                remainingTime: 0f,
                Time.time,
                position));
        }

        public static void PublishEffect(
            IDRPG3DCombatAction action,
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit target,
            IDRPG3DPrototypeEffectResult result)
        {
            if (!result.Applied)
            {
                return;
            }

            Publish(new IDRPG3DCombatEvent(
                IDRPG3DCombatEventType.ApplyEffect,
                action.Sequence,
                action.ActionId,
                projectileId: 0,
                action.CasterUnitId,
                source != null ? source.UnitId : action.CasterUnitId,
                target != null ? target.UnitId : action.TargetUnitId,
                action.SkillId,
                action.SkillKey,
                result.EffectId,
                result.BuffId,
                stack: 0,
                result.Value,
                target != null ? target.Health : 0f,
                remainingTime: 0f,
                Time.time,
                target != null ? target.transform.position : Vector3.zero));

            if (result.BuffId > 0)
            {
                Publish(new IDRPG3DCombatEvent(
                    IDRPG3DCombatEventType.ApplyBuff,
                    action.Sequence,
                    action.ActionId,
                    projectileId: 0,
                    action.CasterUnitId,
                    source != null ? source.UnitId : action.CasterUnitId,
                    target != null ? target.UnitId : action.TargetUnitId,
                    action.SkillId,
                    action.SkillKey,
                    result.EffectId,
                    result.BuffId,
                    result.BuffStack,
                    value: 0f,
                    targetHealth: target != null ? target.Health : 0f,
                    result.BuffRemainingTime,
                    Time.time,
                    target != null ? target.transform.position : Vector3.zero));
            }
        }

        public static void EndCast(IDRPG3DCombatAction action, IDRPG3DCombatUnit target, Vector3 position)
        {
            Publish(new IDRPG3DCombatEvent(
                IDRPG3DCombatEventType.CastEnd,
                action.Sequence,
                action.ActionId,
                projectileId: 0,
                action.CasterUnitId,
                action.CasterUnitId,
                target != null ? target.UnitId : action.TargetUnitId,
                action.SkillId,
                action.SkillKey,
                effectId: 0,
                buffId: 0,
                stack: 0,
                value: 0f,
                targetHealth: target != null ? target.Health : 0f,
                remainingTime: 0f,
                Time.time,
                position));
        }

        public static void ResetForTest()
        {
            nextSequence = 1;
            nextActionId = 1;
            nextProjectileId = 1;
            EventPublished = null;
        }

        private static void Publish(IDRPG3DCombatEvent combatEvent)
        {
            EventPublished?.Invoke(combatEvent);
        }
    }
}
