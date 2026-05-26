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
        private IDRPG3DCombatUnit currentTarget;
        private float nextAttackTime;

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
                    return;
                }
            }

            var targetPosition = currentTarget.transform.position;
            var offset = targetPosition - transform.position;
            offset.y = 0f;
            var sqrDistance = offset.sqrMagnitude;
            if (skillCaster == null)
            {
                skillCaster = GetComponent<IDRPG3DPrototypeSkillCaster>();
            }

            var baseAttackRange = skillCaster != null && skillCaster.HasSkill ? skillCaster.Skill.Range : unit.AttackRange;
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

            var usesSkill = skillCaster != null && skillCaster.HasSkill;
            var attackSpeedMultiplier = Mathf.Max(0.05f, usesSkill ? GetCastSpeedMultiplier() : GetAttackSpeedMultiplier());
            var baseInterval = usesSkill ? skillCaster.Skill.Cooldown : unit.AttackInterval;
            nextAttackTime = Time.time + baseInterval / attackSpeedMultiplier;
            animatorBridge.PlayMeleeAttack(attackSpeedMultiplier);
            Debug.Log($"[IDRPG3D Combat] {name} attacks {currentTarget.name}.");
            if (usesSkill)
            {
                skillCaster.TryCast(currentTarget);
                return;
            }

            currentTarget.TakeDamage(unit.AttackPower, unit);
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
