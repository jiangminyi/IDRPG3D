using UnityEngine;
using UnityEngine.AI;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    public sealed class IDRPG3DPrototypeSkillCaster : MonoBehaviour
    {
        private static readonly System.Collections.Generic.List<IDRPG3DCombatUnit> AreaTargets = new System.Collections.Generic.List<IDRPG3DCombatUnit>(16);

        [SerializeField] private IDRPG3DPrototypeSkillDefinition skill;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private Vector3 castOffset = new Vector3(0f, 1.2f, 0.25f);
        [SerializeField] private float fallbackProjectileScale = 0.22f;
        [SerializeField] private float chargeTravelDuration = 0.35f;
        [SerializeField] private float chargeSampleRadius = 1.5f;

        private IDRPG3DCombatUnit unit;
        private IDRPG3DCombatResource resource;
        private Coroutine activeChargeRoutine;
        private float activeThreatMultiplier = 1f;

        public IDRPG3DPrototypeSkillDefinition Skill => skill;
        public bool HasSkill => skill.IsValid;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            unit = GetComponent<IDRPG3DCombatUnit>();
            resource = GetComponent<IDRPG3DCombatResource>();
        }

        public void Configure(IDRPG3DPrototypeSkillDefinition skillDefinition, Transform parent)
        {
            skill = skillDefinition;
            projectileParent = parent;
            Initialize();
        }

        public bool TryCast(IDRPG3DCombatUnit target)
        {
            if (!HasSkill || target == null || !target.IsAlive)
            {
                return false;
            }

            if (unit == null)
            {
                Initialize();
            }

            activeThreatMultiplier = 1f;
            var startPosition = transform.TransformPoint(castOffset);
            var action = IDRPG3DCombatEventStream.BeginCast(unit, target, skill, startPosition);
            SpawnMuzzle(startPosition);

            if (!skill.UsesProjectile)
            {
                ApplyEffects(target, action);
                IDRPG3DCombatEventStream.EndCast(action, target, target.transform.position);
                return true;
            }

            var projectileObject = CreateProjectileObject(startPosition);
            var projectile = projectileObject.GetComponent<IDRPG3DPrototypeProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<IDRPG3DPrototypeProjectile>();
            }

            var projectileId = IDRPG3DCombatEventStream.PublishProjectileSpawn(action, startPosition);
            projectile.Launch(unit, target, skill, startPosition, action, projectileId);
            return true;
        }

        public bool TryCast(IDRPG3DPrototypeSkillRuntime runtime, IDRPG3DCombatUnit enemyTarget)
        {
            if (!runtime.IsValid)
            {
                return false;
            }

            if (unit == null)
            {
                Initialize();
            }

            if (!TryResolveTarget(runtime, enemyTarget, out var target))
            {
                return false;
            }

            resource ??= GetComponent<IDRPG3DCombatResource>();
            if (resource != null
                && resource.ResourceType == runtime.ResourceType
                && !resource.TrySpend(runtime.ResourceCost))
            {
                return false;
            }

            skill = runtime.Definition;
            activeThreatMultiplier = Mathf.Max(0f, runtime.ThreatMultiplier);
            var startPosition = transform.TransformPoint(castOffset);
            var action = IDRPG3DCombatEventStream.BeginCast(unit, target, skill, startPosition);
            SpawnMuzzle(startPosition);

            if (runtime.CastMode == IDRPG3DPrototypeSkillCastMode.Charge)
            {
                StartCharge(runtime, target, action);
                return true;
            }

            if (runtime.CastMode == IDRPG3DPrototypeSkillCastMode.Area)
            {
                ApplyAreaEffects(runtime, target, action);
                IDRPG3DCombatEventStream.EndCast(action, target, target.transform.position);
                GainRuntimeResource(runtime);
                SpawnImpact(target.transform.position, skill.ImpactPrefab);
                return true;
            }

            if (!skill.UsesProjectile || runtime.CastMode == IDRPG3DPrototypeSkillCastMode.Melee || runtime.CastMode == IDRPG3DPrototypeSkillCastMode.Instant || runtime.CastMode == IDRPG3DPrototypeSkillCastMode.Charge)
            {
                ApplyEffects(target, action);
                IDRPG3DCombatEventStream.EndCast(action, target, target.transform.position);
                GainRuntimeResource(runtime);
                SpawnImpact(target.transform.position, skill.ImpactPrefab);
                return true;
            }

            var projectileObject = CreateProjectileObject(startPosition);
            var projectile = projectileObject.GetComponent<IDRPG3DPrototypeProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<IDRPG3DPrototypeProjectile>();
            }

            var projectileId = IDRPG3DCombatEventStream.PublishProjectileSpawn(action, startPosition);
            projectile.Launch(unit, target, skill, startPosition, action, projectileId, runtime.ResourceGain, runtime.ThreatMultiplier);
            return true;
        }

        private void StartCharge(
            IDRPG3DPrototypeSkillRuntime runtime,
            IDRPG3DCombatUnit target,
            IDRPG3DCombatAction action)
        {
            if (activeChargeRoutine != null)
            {
                StopCoroutine(activeChargeRoutine);
            }

            activeChargeRoutine = StartCoroutine(ChargeRoutine(runtime, target, action));
        }

        private System.Collections.IEnumerator ChargeRoutine(
            IDRPG3DPrototypeSkillRuntime runtime,
            IDRPG3DCombatUnit target,
            IDRPG3DCombatAction action)
        {
            var mover = GetComponent<IDRPG3DNavMoveAgent>();
            mover?.Stop();

            var agent = GetComponent<NavMeshAgent>();
            var start = transform.position;
            var destination = FindChargeDestination(target);
            if (agent != null && NavMesh.SamplePosition(destination, out var hit, chargeSampleRadius, agent.areaMask))
            {
                destination = hit.position;
            }

            var elapsed = 0f;
            var duration = Mathf.Max(0.05f, chargeTravelDuration);
            while (elapsed < duration && target != null && target.IsAlive)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var next = Vector3.Lerp(start, destination, t);
                FacePosition(target.transform.position);
                MoveChargeFrame(agent, next);
                yield return null;
            }

            MoveChargeFrame(agent, destination);
            if (target != null && target.IsAlive)
            {
                FacePosition(target.transform.position);
                ApplyEffects(target, action);
                IDRPG3DCombatEventStream.EndCast(action, target, target.transform.position);
                GainRuntimeResource(runtime);
                SpawnImpact(target.transform.position, skill.ImpactPrefab);
            }

            activeChargeRoutine = null;
        }

        private void MoveChargeFrame(NavMeshAgent agent, Vector3 position)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.Warp(position);
                return;
            }

            transform.position = position;
        }

        private void FacePosition(Vector3 worldPosition)
        {
            var direction = worldPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private bool TryResolveTarget(
            IDRPG3DPrototypeSkillRuntime runtime,
            IDRPG3DCombatUnit enemyTarget,
            out IDRPG3DCombatUnit target)
        {
            target = null;
            if (runtime.TargetRule == IDRPG3DPrototypeSkillTargetRule.Self)
            {
                target = unit;
            }
            else if (runtime.TargetRule == IDRPG3DPrototypeSkillTargetRule.AllyLowestHp)
            {
                IDRPG3DPrototypeCombatDirector.TryFindLowestHealthAlly(unit, runtime.Definition.Range, includeSelf: true, out target);
            }
            else if (runtime.TargetRule == IDRPG3DPrototypeSkillTargetRule.DeadAlly)
            {
                IDRPG3DPrototypeCombatDirector.TryFindDeadAlly(unit, runtime.Definition.Range, out target);
            }
            else
            {
                target = enemyTarget;
            }

            if (target == null)
            {
                return false;
            }

            return runtime.TargetRule == IDRPG3DPrototypeSkillTargetRule.DeadAlly
                ? !target.IsAlive
                : target.IsAlive;
        }

        private void ApplyAreaEffects(
            IDRPG3DPrototypeSkillRuntime runtime,
            IDRPG3DCombatUnit primaryTarget,
            IDRPG3DCombatAction action)
        {
            IDRPG3DPrototypeCombatDirector.FindAreaEnemies(unit, transform.position, runtime.Definition.Range, AreaTargets);
            for (var i = 0; i < AreaTargets.Count; i++)
            {
                ApplyEffects(AreaTargets[i], action);
            }
        }

        private Vector3 FindChargeDestination(IDRPG3DCombatUnit target)
        {
            var direction = target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return transform.position;
            }

            return target.transform.position - direction.normalized * Mathf.Max(0.6f, unit.Radius + target.Radius + 0.2f);
        }

        private void GainRuntimeResource(IDRPG3DPrototypeSkillRuntime runtime)
        {
            if (runtime.ResourceGain <= 0f)
            {
                return;
            }

            resource ??= GetComponent<IDRPG3DCombatResource>();
            if (resource == null || resource.ResourceType != runtime.ResourceType)
            {
                return;
            }

            resource.Gain(runtime.ResourceGain);
        }

        private void ApplyEffects(IDRPG3DCombatUnit target, IDRPG3DCombatAction action)
        {
            var effects = skill.Effects;
            if (effects != null && effects.Count > 0)
            {
                for (var i = 0; i < effects.Count; i++)
                {
                    var result = IDRPG3DPrototypeEffectRunner.Apply(effects[i], unit, target, activeThreatMultiplier);
                    IDRPG3DCombatEventStream.PublishEffect(action, unit, target, result);
                }

                return;
            }

            var primaryResult = IDRPG3DPrototypeEffectRunner.Apply(skill.PrimaryEffect, unit, target, activeThreatMultiplier);
            IDRPG3DCombatEventStream.PublishEffect(action, unit, target, primaryResult);
        }

        private GameObject CreateProjectileObject(Vector3 startPosition)
        {
            GameObject projectileObject;
            if (skill.ProjectilePrefab != null)
            {
                projectileObject = Instantiate(skill.ProjectilePrefab, startPosition, Quaternion.identity, projectileParent);
            }
            else
            {
                projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projectileObject.name = $"Prototype_{skill.SkillId}_Projectile";
                projectileObject.transform.SetParent(projectileParent, true);
                projectileObject.transform.position = startPosition;
                projectileObject.transform.localScale = Vector3.one * fallbackProjectileScale;
                var collider = projectileObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }

                var renderer = projectileObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
                    if (material != null)
                    {
                        material.color = skill.FallbackColor;
                    }
                }

                var light = projectileObject.AddComponent<Light>();
                light.color = skill.FallbackColor;
                light.range = 2f;
                light.intensity = 1.5f;
            }

            return projectileObject;
        }

        private void SpawnMuzzle(Vector3 startPosition)
        {
            if (skill.MuzzlePrefab == null)
            {
                return;
            }

            var muzzle = Instantiate(skill.MuzzlePrefab, startPosition, transform.rotation, projectileParent);
            Destroy(muzzle, 1.5f);
        }

        private static void SpawnImpact(Vector3 position, GameObject impactPrefab)
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
