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
}
