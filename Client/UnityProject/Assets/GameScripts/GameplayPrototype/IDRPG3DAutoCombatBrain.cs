using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    [RequireComponent(typeof(IDRPG3DNavMoveAgent))]
    public sealed class IDRPG3DAutoCombatBrain : MonoBehaviour
    {
        private IDRPG3DCombatUnit unit;
        private IDRPG3DNavMoveAgent mover;
        private IDRPG3DAnimatorBridge animatorBridge;
        private IDRPG3DPrototypeSkillCaster skillCaster;
        private IDRPG3DPrototypeSkillBook skillBook;
        private IDRPG3DCombatUnit currentTarget;
        private IDRPG3DCombatUnit committedTarget;
        private IDRPG3DPrototypeSkillRuntime committedSkill;
        private float nextAttackTime;
        private float committedHitTime;
        private float nextThreatRecheckTime;
        private bool hasCommittedAttack;
        private bool committedUsesRuntimeSkill;
        private bool committedUsesLegacySkill;

        private const float AttackCommitDelay = 0.18f;
        private const float DefaultGlobalRecovery = 0.2f;
        private const float ThreatRecheckInterval = 0.35f;
        private const float MeleeThreatSwitchRatio = 1.1f;
        private const float RangedThreatSwitchRatio = 1.3f;

        public bool HasTarget => currentTarget != null && currentTarget.IsAlive;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Initialize()
        {
            unit = GetComponent<IDRPG3DCombatUnit>();
            mover = GetComponent<IDRPG3DNavMoveAgent>();
            animatorBridge = GetComponent<IDRPG3DAnimatorBridge>();
            if (animatorBridge == null)
            {
                animatorBridge = gameObject.AddComponent<IDRPG3DAnimatorBridge>();
            }
            skillCaster = GetComponent<IDRPG3DPrototypeSkillCaster>();
            skillBook = GetComponent<IDRPG3DPrototypeSkillBook>();
            mover.Initialize();
            animatorBridge.Initialize();
        }

        public void SetTarget(IDRPG3DCombatUnit target)
        {
            if (target == unit)
            {
                return;
            }

            currentTarget = target;
        }

        public void ClearTarget()
        {
            currentTarget = null;
        }

        private void Tick(float deltaTime)
        {
            if (unit == null || !unit.IsAlive)
            {
                mover?.Stop();
                ClearCommittedAttack();
                return;
            }

            if (hasCommittedAttack)
            {
                TickCommittedAttack();
                return;
            }

            if (unit.Faction == IDRPG3DCombatFaction.Hero && !HasTarget)
            {
                return;
            }

            if (!HasTarget)
            {
                if (unit.ThreatTable.TryGetHighestThreatTarget(target => target != null && target.IsAlive, out var threatTarget))
                {
                    currentTarget = threatTarget;
                }
                else
                {
                    var fallbackRadius = unit != null ? unit.AggroRadius : 0f;
                    if (!IDRPG3DPrototypeCombatDirector.TryFindNearestEnemy(unit, fallbackRadius, out currentTarget))
                    {
                        return;
                    }
                }
            }
            else
            {
                ReevaluateThreatTarget();
            }

            var targetPosition = currentTarget.transform.position;
            var offset = targetPosition - transform.position;
            offset.y = 0f;
            var sqrDistance = offset.sqrMagnitude;
            if (skillCaster == null)
            {
                skillCaster = GetComponent<IDRPG3DPrototypeSkillCaster>();
            }
            if (skillBook == null)
            {
                skillBook = GetComponent<IDRPG3DPrototypeSkillBook>();
            }

            IDRPG3DPrototypeSkillRuntime runtimeSkill = default;
            var hasRuntimeSkill = skillBook != null && skillBook.TrySelectSkill(currentTarget, out runtimeSkill);

            var baseAttackRange = hasRuntimeSkill
                ? runtimeSkill.Definition.Range
                : skillCaster != null && skillCaster.HasSkill ? skillCaster.Skill.Range : unit.AttackRange;
            var attackRange = baseAttackRange + currentTarget.Radius;
            if (sqrDistance > attackRange * attackRange)
            {
                mover.MoveTo(targetPosition, Mathf.Max(0.15f, baseAttackRange * 0.75f));
                return;
            }

            mover.Stop();
            mover.FacePosition(targetPosition);

            if (Time.time < nextAttackTime)
            {
                return;
            }

            var usesSkill = hasRuntimeSkill || skillCaster != null && skillCaster.HasSkill;
            var attackSpeedMultiplier = Mathf.Max(0.05f, usesSkill ? GetCastSpeedMultiplier() : GetAttackSpeedMultiplier());
            var baseInterval = IsBasicRuntimeSkill(hasRuntimeSkill, runtimeSkill)
                ? runtimeSkill.Definition.Cooldown
                : hasRuntimeSkill ? DefaultGlobalRecovery : usesSkill ? skillCaster.Skill.Cooldown : unit.AttackInterval;
            nextAttackTime = Time.time + baseInterval / attackSpeedMultiplier;

            if (hasRuntimeSkill && runtimeSkill.CastMode == IDRPG3DPrototypeSkillCastMode.Charge)
            {
                IDRPG3DPrototypeDebugLog.Combat($"[IDRPG3D Combat] {name} charges {currentTarget.name}.");
                skillBook.TryCast(runtimeSkill, currentTarget);
                return;
            }

            CommitAttack(currentTarget, runtimeSkill, hasRuntimeSkill, usesSkill, attackSpeedMultiplier);
        }

        private void CommitAttack(
            IDRPG3DCombatUnit target,
            IDRPG3DPrototypeSkillRuntime runtimeSkill,
            bool hasRuntimeSkill,
            bool usesSkill,
            float attackSpeedMultiplier)
        {
            committedTarget = target;
            committedSkill = runtimeSkill;
            committedUsesRuntimeSkill = hasRuntimeSkill;
            committedUsesLegacySkill = usesSkill && !hasRuntimeSkill;
            committedHitTime = Time.time + AttackCommitDelay / attackSpeedMultiplier;
            hasCommittedAttack = true;
            animatorBridge.PlayMeleeAttack(attackSpeedMultiplier);
            IDRPG3DPrototypeDebugLog.Combat($"[IDRPG3D Combat] {name} attacks {target.name}.");
        }

        private void TickCommittedAttack()
        {
            mover?.Stop();
            if (committedTarget == null || !committedTarget.IsAlive)
            {
                ClearCommittedAttack();
                return;
            }

            mover?.FacePosition(committedTarget.transform.position);
            if (Time.time < committedHitTime)
            {
                return;
            }

            if (committedUsesRuntimeSkill)
            {
                skillBook.TryCast(committedSkill, committedTarget);
            }
            else if (committedUsesLegacySkill)
            {
                skillCaster.TryCast(committedTarget);
            }
            else
            {
                committedTarget.TakeDamage(unit.AttackPower, unit);
            }

            ClearCommittedAttack();
        }

        private void ReevaluateThreatTarget()
        {
            if (unit.Faction != IDRPG3DCombatFaction.Enemy || Time.time < nextThreatRecheckTime)
            {
                return;
            }

            nextThreatRecheckTime = Time.time + ThreatRecheckInterval;
            if (!unit.ThreatTable.TryGetHighestThreatTarget(target => target != null && target.IsAlive, out var bestTarget, out var bestThreat)
                || bestTarget == null
                || bestTarget == currentTarget)
            {
                return;
            }

            var currentThreat = unit.ThreatTable.GetThreat(currentTarget);
            var switchRatio = IsCurrentTargetInMeleeRange() ? MeleeThreatSwitchRatio : RangedThreatSwitchRatio;
            if (currentThreat <= 0f || bestThreat >= currentThreat * switchRatio)
            {
                IDRPG3DPrototypeDebugLog.Combat($"[IDRPG3D Combat] {name} switches target from {currentTarget.name} to {bestTarget.name}. Threat {currentThreat:0.#}->{bestThreat:0.#}");
                currentTarget = bestTarget;
                ClearCommittedAttack();
            }
        }

        private bool IsCurrentTargetInMeleeRange()
        {
            if (currentTarget == null || unit == null)
            {
                return false;
            }

            var offset = currentTarget.transform.position - transform.position;
            offset.y = 0f;
            var range = unit.AttackRange + currentTarget.Radius;
            return offset.sqrMagnitude <= range * range;
        }

        private static bool IsBasicRuntimeSkill(bool hasRuntimeSkill, IDRPG3DPrototypeSkillRuntime runtimeSkill)
        {
            return hasRuntimeSkill && runtimeSkill.IsBasicAttack;
        }

        private void ClearCommittedAttack()
        {
            hasCommittedAttack = false;
            committedTarget = null;
            committedSkill = default;
            committedUsesRuntimeSkill = false;
            committedUsesLegacySkill = false;
        }

        private float GetAttackSpeedMultiplier()
        {
            var buffController = GetComponent<IDRPG3DPrototypeBuffController>();
            return buffController != null ? buffController.AttackSpeedMultiplier : 1f;
        }

        private float GetCastSpeedMultiplier()
        {
            var buffController = GetComponent<IDRPG3DPrototypeBuffController>();
            return buffController != null ? buffController.CastSpeedMultiplier : 1f;
        }
    }
}
