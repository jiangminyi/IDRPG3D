using System.Collections.Generic;
using System.Reflection;
using IDRPG3D.GameplayPrototype;
using NUnit.Framework;
using UnityEngine;

namespace IDRPG3D.EditorTools.Tests
{
    public sealed class IDRPG3DGameplayCoreTests
    {
        [Test]
        public void FormationSortsHigherMovePriorityCloserToFront()
        {
            var members = new[]
            {
                new IDRPG3DFormationMember(10, teamOrder: 0, movePriority: 10, radius: 0.35f),
                new IDRPG3DFormationMember(20, teamOrder: 1, movePriority: 100, radius: 0.45f),
                new IDRPG3DFormationMember(30, teamOrder: 2, movePriority: 70, radius: 0.4f),
                new IDRPG3DFormationMember(40, teamOrder: 3, movePriority: 70, radius: 0.4f)
            };
            var destinations = new List<IDRPG3DFormationDestination>(4);

            IDRPG3DTeamFormationSolver.BuildFormation(
                members,
                Vector3.zero,
                Vector3.forward,
                sideSpacing: 1.4f,
                rowSpacing: 1.2f,
                destinations);

            Assert.AreEqual(4, destinations.Count);
            Assert.AreEqual(20, destinations[0].UnitId);
            Assert.AreEqual(30, destinations[1].UnitId);
            Assert.AreEqual(40, destinations[2].UnitId);
            Assert.AreEqual(10, destinations[3].UnitId);
            Assert.Greater(destinations[0].WorldPosition.z, destinations[3].WorldPosition.z);
        }

        [Test]
        public void ThreatTableReturnsHighestThreatTargetAndIgnoresDeadTargets()
        {
            var tank = new IDRPG3DThreatTestTarget(true);
            var ranged = new IDRPG3DThreatTestTarget(true);
            var defeated = new IDRPG3DThreatTestTarget(false);
            var table = new IDRPG3DThreatTable<IDRPG3DThreatTestTarget>();

            table.AddThreat(tank, 10f);
            table.AddThreat(ranged, 30f);
            table.AddThreat(defeated, 100f);

            var found = table.TryGetHighestThreatTarget(target => target.IsAlive, out var target);

            Assert.IsTrue(found);
            Assert.AreSame(ranged, target);
        }

        [Test]
        public void AnimatorBridgeDisablesRootMotionOnVisualAnimator()
        {
            var root = new GameObject("AnimatorBridgeTestRoot");
            var visual = new GameObject("Visual");
            try
            {
                visual.transform.SetParent(root.transform);
                var animator = visual.AddComponent<Animator>();
                animator.applyRootMotion = true;

                root.AddComponent<IDRPG3DAnimatorBridge>().Initialize();

                Assert.IsFalse(animator.applyRootMotion);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TerrainVisualGrounderKeepsVisualLocalXZAnchored()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var root = new GameObject("GrounderTestRoot");
            var visual = new GameObject("Visual");
            try
            {
                ground.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                ground.transform.localScale = new Vector3(10f, 0.1f, 10f);
                root.transform.position = new Vector3(0f, 0.5f, 0f);
                visual.transform.SetParent(root.transform);
                visual.transform.localPosition = new Vector3(0.25f, 0.1f, -0.35f);

                var grounder = root.AddComponent<IDRPG3DTerrainVisualGrounder>();
                grounder.Configure(visual.transform, ~0, 0f);
                visual.transform.localPosition = new Vector3(5f, 0.1f, 6f);

                typeof(IDRPG3DTerrainVisualGrounder)
                    .GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(grounder, null);

                Assert.AreEqual(0.25f, visual.transform.localPosition.x, 0.001f);
                Assert.AreEqual(-0.35f, visual.transform.localPosition.z, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(ground);
            }
        }

        [Test]
        public void SkillCasterSpawnsProjectileWithoutImmediateDamage()
        {
            var casterObject = new GameObject("Caster");
            var targetObject = new GameObject("Target");
            var projectilesRoot = new GameObject("Projectiles");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 80f, 5f, 1.5f, 1f, 8f);

                var skillCaster = casterObject.AddComponent<IDRPG3DPrototypeSkillCaster>();
                skillCaster.Configure(IDRPG3DPrototypeSkillDefinition.CreateFrostbolt(), projectilesRoot.transform);

                Assert.IsTrue(skillCaster.TryCast(target));
                Assert.AreEqual(80f, target.Health, 0.001f);
                Assert.AreEqual(1, projectilesRoot.transform.childCount);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(projectilesRoot);
            }
        }

        [Test]
        public void ProjectileAppliesDamageOnImpact()
        {
            var casterObject = new GameObject("Caster");
            var targetObject = new GameObject("Target");
            var projectileObject = new GameObject("Projectile");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 80f, 5f, 1.5f, 1f, 8f);

                var projectile = projectileObject.AddComponent<IDRPG3DPrototypeProjectile>();
                projectile.Launch(caster, target, IDRPG3DPrototypeSkillDefinition.CreateFireball(), Vector3.zero);
                projectile.ApplyImpactForTest();

                Assert.AreEqual(56f, target.Health, 0.001f);
                Assert.IsTrue(target.ThreatTable.TryGetHighestThreatTarget(unit => unit != null, out var threatTarget));
                Assert.AreSame(caster, threatTarget);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(projectileObject);
            }
        }

        [Test]
        public void WorldHealthBarUpdatesFillScaleFromHealthRatio()
        {
            var unitObject = new GameObject("Unit");
            var fillObject = new GameObject("Fill");
            try
            {
                var unit = unitObject.AddComponent<IDRPG3DCombatUnit>();
                unit.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);
                var bar = unitObject.AddComponent<IDRPG3DWorldHealthBar>();
                bar.Configure(unit, fillObject.transform, null, 2.4f, 0.32f, 2.1f);

                unit.TakeDamage(35f, null);

                Assert.AreEqual(0.65f, fillObject.transform.localScale.x, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
                Object.DestroyImmediate(fillObject);
            }
        }

        private sealed class IDRPG3DThreatTestTarget
        {
            public IDRPG3DThreatTestTarget(bool isAlive)
            {
                IsAlive = isAlive;
            }

            public bool IsAlive { get; }
        }
    }
}
