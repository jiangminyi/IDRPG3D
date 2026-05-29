using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DPrototypeProjectile : MonoBehaviour
    {
        [SerializeField] private float hitDistance = 0.25f;
        [SerializeField] private float maxLifetime = 4f;

        private IDRPG3DCombatUnit caster;
        private IDRPG3DCombatUnit target;
        private IDRPG3DPrototypeSkillDefinition skill;
        private IDRPG3DCombatAction action;
        private GameObject impactPrefab;
        private int projectileId;
        private float resourceGainOnImpact;
        private float threatMultiplier = 1f;
        private float spawnTime;
        private bool impacted;

        public void Launch(
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit targetUnit,
            IDRPG3DPrototypeSkillDefinition skillDefinition,
            Vector3 startPosition)
        {
            Launch(source, targetUnit, skillDefinition, startPosition, default, 0);
        }

        public void Launch(
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit targetUnit,
            IDRPG3DPrototypeSkillDefinition skillDefinition,
            Vector3 startPosition,
            IDRPG3DCombatAction combatAction,
            int id)
        {
            Launch(source, targetUnit, skillDefinition, startPosition, combatAction, id, 0f);
        }

        public void Launch(
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit targetUnit,
            IDRPG3DPrototypeSkillDefinition skillDefinition,
            Vector3 startPosition,
            IDRPG3DCombatAction combatAction,
            int id,
            float resourceGain)
        {
            Launch(source, targetUnit, skillDefinition, startPosition, combatAction, id, resourceGain, 1f);
        }

        public void Launch(
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit targetUnit,
            IDRPG3DPrototypeSkillDefinition skillDefinition,
            Vector3 startPosition,
            IDRPG3DCombatAction combatAction,
            int id,
            float resourceGain,
            float threat)
        {
            caster = source;
            target = targetUnit;
            skill = skillDefinition;
            action = combatAction;
            projectileId = id;
            resourceGainOnImpact = Mathf.Max(0f, resourceGain);
            threatMultiplier = Mathf.Max(0f, threat);
            impactPrefab = skillDefinition.ImpactPrefab;
            spawnTime = Time.time;
            impacted = false;
            transform.position = startPosition;
            FaceTarget();
        }

        private void Update()
        {
            if (impacted)
            {
                return;
            }

            if (target == null || !target.IsAlive || Time.time - spawnTime > maxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            var targetPosition = GetTargetPoint();
            var nextPosition = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                skill.ProjectileSpeed * Time.deltaTime);
            transform.position = nextPosition;
            FaceTarget();

            if ((targetPosition - nextPosition).sqrMagnitude <= hitDistance * hitDistance)
            {
                Impact();
            }
        }

        public void ApplyImpactForTest()
        {
            Impact();
        }

        private void Impact()
        {
            if (impacted)
            {
                return;
            }

            impacted = true;
            if (target != null && target.IsAlive)
            {
                if (action.ActionId > 0)
                {
                    IDRPG3DCombatEventStream.PublishProjectileImpact(action, projectileId, target, GetTargetPoint());
                }

                var effects = skill.Effects;
                if (effects != null && effects.Count > 0)
                {
                    for (var i = 0; i < effects.Count; i++)
                    {
                        var result = IDRPG3DPrototypeEffectRunner.Apply(effects[i], caster, target, threatMultiplier);
                        if (action.ActionId > 0)
                        {
                            IDRPG3DCombatEventStream.PublishEffect(action, caster, target, result);
                        }
                    }
                }
                else
                {
                    var result = IDRPG3DPrototypeEffectRunner.Apply(skill.PrimaryEffect, caster, target, threatMultiplier);
                    if (action.ActionId > 0)
                    {
                        IDRPG3DCombatEventStream.PublishEffect(action, caster, target, result);
                    }
                }

                SpawnImpact(GetTargetPoint());
                if (resourceGainOnImpact > 0f && caster != null)
                {
                    var resource = caster.GetComponent<IDRPG3DCombatResource>();
                    resource?.Gain(resourceGainOnImpact);
                }

                if (action.ActionId > 0)
                {
                    IDRPG3DCombatEventStream.EndCast(action, target, GetTargetPoint());
                }
            }

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        private Vector3 GetTargetPoint()
        {
            if (target == null)
            {
                return transform.position;
            }

            return target.transform.position + Vector3.up * 1.05f;
        }

        private void FaceTarget()
        {
            if (target == null)
            {
                return;
            }

            var direction = GetTargetPoint() - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        private void SpawnImpact(Vector3 position)
        {
            if (impactPrefab == null)
            {
                return;
            }

            var impact = Instantiate(impactPrefab, position, Quaternion.identity);
            Destroy(impact, 2.5f);
        }
    }
}
