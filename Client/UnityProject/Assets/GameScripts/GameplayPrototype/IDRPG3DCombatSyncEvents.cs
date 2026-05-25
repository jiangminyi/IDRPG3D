namespace IDRPG3D.GameplayPrototype
{
    public static class IDRPG3DCombatSyncEvents
    {
        public static IDRPG3DCastSkillSyncEvent CastSkill(
            int sequence,
            int casterUnitId,
            int targetUnitId,
            int skillId,
            string skillKey)
        {
            return new IDRPG3DCastSkillSyncEvent(sequence, casterUnitId, targetUnitId, skillId, skillKey);
        }

        public static IDRPG3DSpawnProjectileSyncEvent SpawnProjectile(
            int sequence,
            int projectileId,
            int casterUnitId,
            int targetUnitId,
            int skillId)
        {
            return new IDRPG3DSpawnProjectileSyncEvent(sequence, projectileId, casterUnitId, targetUnitId, skillId);
        }

        public static IDRPG3DApplyEffectSyncEvent ApplyEffect(
            int sequence,
            int effectId,
            int sourceUnitId,
            int targetUnitId,
            float value,
            int buffId)
        {
            return new IDRPG3DApplyEffectSyncEvent(sequence, effectId, sourceUnitId, targetUnitId, value, buffId);
        }

        public static IDRPG3DApplyBuffSyncEvent ApplyBuff(
            int sequence,
            int buffId,
            int sourceUnitId,
            int targetUnitId,
            int stack,
            float remainingTime)
        {
            return new IDRPG3DApplyBuffSyncEvent(sequence, buffId, sourceUnitId, targetUnitId, stack, remainingTime);
        }
    }

    public readonly struct IDRPG3DCastSkillSyncEvent
    {
        public IDRPG3DCastSkillSyncEvent(
            int sequence,
            int casterUnitId,
            int targetUnitId,
            int skillId,
            string skillKey)
        {
            Sequence = sequence;
            CasterUnitId = casterUnitId;
            TargetUnitId = targetUnitId;
            SkillId = skillId;
            SkillKey = skillKey;
        }

        public int Sequence { get; }
        public int CasterUnitId { get; }
        public int TargetUnitId { get; }
        public int SkillId { get; }
        public string SkillKey { get; }
    }

    public readonly struct IDRPG3DSpawnProjectileSyncEvent
    {
        public IDRPG3DSpawnProjectileSyncEvent(
            int sequence,
            int projectileId,
            int casterUnitId,
            int targetUnitId,
            int skillId)
        {
            Sequence = sequence;
            ProjectileId = projectileId;
            CasterUnitId = casterUnitId;
            TargetUnitId = targetUnitId;
            SkillId = skillId;
        }

        public int Sequence { get; }
        public int ProjectileId { get; }
        public int CasterUnitId { get; }
        public int TargetUnitId { get; }
        public int SkillId { get; }
    }

    public readonly struct IDRPG3DApplyEffectSyncEvent
    {
        public IDRPG3DApplyEffectSyncEvent(
            int sequence,
            int effectId,
            int sourceUnitId,
            int targetUnitId,
            float value,
            int buffId)
        {
            Sequence = sequence;
            EffectId = effectId;
            SourceUnitId = sourceUnitId;
            TargetUnitId = targetUnitId;
            Value = value;
            BuffId = buffId;
        }

        public int Sequence { get; }
        public int EffectId { get; }
        public int SourceUnitId { get; }
        public int TargetUnitId { get; }
        public float Value { get; }
        public int BuffId { get; }
    }

    public readonly struct IDRPG3DApplyBuffSyncEvent
    {
        public IDRPG3DApplyBuffSyncEvent(
            int sequence,
            int buffId,
            int sourceUnitId,
            int targetUnitId,
            int stack,
            float remainingTime)
        {
            Sequence = sequence;
            BuffId = buffId;
            SourceUnitId = sourceUnitId;
            TargetUnitId = targetUnitId;
            Stack = stack;
            RemainingTime = remainingTime;
        }

        public int Sequence { get; }
        public int BuffId { get; }
        public int SourceUnitId { get; }
        public int TargetUnitId { get; }
        public int Stack { get; }
        public float RemainingTime { get; }
    }
}
