using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    public sealed class IDRPG3DPrototypeSkillBook : MonoBehaviour
    {
        private readonly List<IDRPG3DPrototypeSkillRuntime> skills = new List<IDRPG3DPrototypeSkillRuntime>(4);
        private readonly Dictionary<int, float> nextReadyTimeBySkillId = new Dictionary<int, float>();

        private IDRPG3DCombatUnit unit;
        private IDRPG3DCombatResource resource;
        private IDRPG3DPrototypeSkillCaster caster;

        public IReadOnlyList<IDRPG3DPrototypeSkillRuntime> Skills => skills;
        public bool HasSkills => skills.Count > 0;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            unit = GetComponent<IDRPG3DCombatUnit>();
            resource = GetComponent<IDRPG3DCombatResource>();
            caster = GetComponent<IDRPG3DPrototypeSkillCaster>();
            if (caster == null)
            {
                caster = gameObject.AddComponent<IDRPG3DPrototypeSkillCaster>();
            }
        }

        public void Configure(IReadOnlyList<IDRPG3DPrototypeSkillRuntime> configuredSkills, Transform projectileParent)
        {
            Initialize();
            skills.Clear();
            nextReadyTimeBySkillId.Clear();
            if (configuredSkills != null)
            {
                for (var i = 0; i < configuredSkills.Count; i++)
                {
                    if (configuredSkills[i].IsValid)
                    {
                        skills.Add(configuredSkills[i]);
                    }
                }
            }

            caster.Configure(default, projectileParent);
        }

        public bool TrySelectSkill(IDRPG3DCombatUnit enemyTarget, out IDRPG3DPrototypeSkillRuntime skill)
        {
            skill = default;
            if (unit == null || !unit.IsAlive || skills.Count == 0)
            {
                return false;
            }

            var bestPriority = int.MinValue;
            for (var i = 0; i < skills.Count; i++)
            {
                var candidate = skills[i];
                if (!CanCast(candidate))
                {
                    continue;
                }

                if (candidate.TargetRule == IDRPG3DPrototypeSkillTargetRule.AllyLowestHp
                    && !IDRPG3DPrototypeCombatDirector.TryFindLowestHealthAlly(unit, candidate.Definition.Range, includeSelf: true, out _))
                {
                    continue;
                }

                if (candidate.TargetRule == IDRPG3DPrototypeSkillTargetRule.DeadAlly
                    && !IDRPG3DPrototypeCombatDirector.TryFindDeadAlly(unit, candidate.Definition.Range, out _))
                {
                    continue;
                }

                if ((candidate.TargetRule == IDRPG3DPrototypeSkillTargetRule.Enemy
                    || candidate.TargetRule == IDRPG3DPrototypeSkillTargetRule.AreaEnemy)
                    && (enemyTarget == null || !enemyTarget.IsAlive))
                {
                    continue;
                }

                var priority = CalculatePriority(candidate, enemyTarget);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    skill = candidate;
                }
            }

            return skill.IsValid;
        }

        public bool TryCast(IDRPG3DPrototypeSkillRuntime runtime, IDRPG3DCombatUnit enemyTarget)
        {
            if (!runtime.IsValid || !CanCast(runtime))
            {
                return false;
            }

            if (caster == null)
            {
                Initialize();
            }

            if (!caster.TryCast(runtime, enemyTarget))
            {
                return false;
            }

            nextReadyTimeBySkillId[runtime.Definition.ConfigId] = Time.time + runtime.Definition.Cooldown;
            return true;
        }

        private bool CanCast(IDRPG3DPrototypeSkillRuntime skill)
        {
            if (!skill.IsValid)
            {
                return false;
            }

            if (nextReadyTimeBySkillId.TryGetValue(skill.Definition.ConfigId, out var nextReadyTime) && Time.time < nextReadyTime)
            {
                return false;
            }

            if (skill.ResourceCost <= 0f)
            {
                return true;
            }

            if (resource == null)
            {
                resource = GetComponent<IDRPG3DCombatResource>();
            }

            return resource == null || resource.ResourceType != skill.ResourceType || resource.HasEnough(skill.ResourceCost);
        }

        private int CalculatePriority(IDRPG3DPrototypeSkillRuntime candidate, IDRPG3DCombatUnit enemyTarget)
        {
            if (candidate.TargetRule == IDRPG3DPrototypeSkillTargetRule.DeadAlly)
            {
                return 1000;
            }

            if (candidate.TargetRule == IDRPG3DPrototypeSkillTargetRule.AllyLowestHp)
            {
                return 900;
            }

            if (candidate.CastMode == IDRPG3DPrototypeSkillCastMode.Area)
            {
                return IsEnemyInBaseAttackRange(enemyTarget) ? 800 : 650;
            }

            if (candidate.CastMode == IDRPG3DPrototypeSkillCastMode.Charge)
            {
                return IsEnemyInBaseAttackRange(enemyTarget) ? 100 : 750;
            }

            if (!candidate.IsBasicAttack)
            {
                return 600;
            }

            return 0;
        }

        private bool IsEnemyInBaseAttackRange(IDRPG3DCombatUnit enemyTarget)
        {
            if (enemyTarget == null || unit == null)
            {
                return false;
            }

            var offset = enemyTarget.transform.position - unit.transform.position;
            offset.y = 0f;
            var range = unit.AttackRange + enemyTarget.Radius;
            return offset.sqrMagnitude <= range * range;
        }
    }
}
