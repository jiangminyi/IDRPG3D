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
        private GameObject impactPrefab;
        private float spawnTime;
        private bool impacted;

        public void Launch(
            IDRPG3DCombatUnit source,
            IDRPG3DCombatUnit targetUnit,
            IDRPG3DPrototypeSkillDefinition skillDefinition,
            Vector3 startPosition)
        {
            caster = source;
            target = targetUnit;
            skill = skillDefinition;
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
                var effects = skill.Effects;
                if (effects != null && effects.Count > 0)
                {
                    for (var i = 0; i < effects.Count; i++)
                    {
                        IDRPG3DPrototypeEffectRunner.Apply(effects[i], caster, target);
                    }
                }
                else
                {
                    IDRPG3DPrototypeEffectRunner.Apply(skill.PrimaryEffect, caster, target);
                }

                SpawnImpact(GetTargetPoint());
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
