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
        public void WaveSpawnResolverPlacesNormalWaveAheadOfRouteAnchor()
        {
            var wave = new IDRPG3DWaveDefinition(
                waveId: 101,
                stageId: 1,
                waveIndex: 1,
                IDRPG3DWaveSpawnMode.SplineAhead,
                enemyId: 1001,
                enemyLevel: 1,
                count: 4,
                spawnDistanceAhead: 18f,
                spawnRadius: 3f,
                spawnAnchorId: string.Empty,
                isBoss: false,
                nextWaveDelay: 2f);

            var result = IDRPG3DWaveSpawnResolver.Resolve(
                wave,
                routeAnchorPosition: new Vector3(3f, 0f, 5f),
                routeForward: Vector3.forward,
                anchors: null);

            Assert.IsTrue(result.Found);
            Assert.AreEqual(new Vector3(3f, 0f, 23f), result.Position);
            Assert.AreEqual(Vector3.forward, result.Forward);
        }

        [Test]
        public void WaveSpawnResolverUsesFixedAnchorForBossWave()
        {
            var anchorObject = new GameObject("BossSpawn");
            try
            {
                anchorObject.transform.SetPositionAndRotation(new Vector3(12f, 0.5f, 30f), Quaternion.Euler(0f, 145f, 0f));
                var anchor = anchorObject.AddComponent<IDRPG3DSpawnAnchor>();
                anchor.Configure("stage_01_boss_01");
                var wave = new IDRPG3DWaveDefinition(
                    waveId: 105,
                    stageId: 1,
                    waveIndex: 5,
                    IDRPG3DWaveSpawnMode.FixedAnchor,
                    enemyId: 9001,
                    enemyLevel: 3,
                    count: 1,
                    spawnDistanceAhead: 0f,
                    spawnRadius: 0f,
                    spawnAnchorId: "stage_01_boss_01",
                    isBoss: true,
                    nextWaveDelay: 5f);

                var result = IDRPG3DWaveSpawnResolver.Resolve(
                    wave,
                    routeAnchorPosition: Vector3.zero,
                    routeForward: Vector3.forward,
                    anchors: new[] { anchor });

                Assert.IsTrue(result.Found);
                Assert.AreEqual(anchorObject.transform.position, result.Position);
                Assert.AreEqual(anchorObject.transform.forward, result.Forward);
            }
            finally
            {
                Object.DestroyImmediate(anchorObject);
            }
        }

        [Test]
        public void WaveControllerWaitsUntilCurrentWaveEnemiesAreDead()
        {
            var controllerObject = new GameObject("WaveController");
            var enemyObject = new GameObject("Enemy");
            try
            {
                var enemy = enemyObject.AddComponent<IDRPG3DCombatUnit>();
                enemy.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 20f, 4f, 1.5f, 1f, 6f);
                var controller = controllerObject.AddComponent<IDRPG3DWaveController>();
                controller.Configure(
                    new[]
                    {
                        CreateTestWave(1, nextWaveDelay: 0.5f),
                        CreateTestWave(2, nextWaveDelay: 0.5f)
                    },
                    () => Vector3.zero,
                    () => Vector3.forward,
                    System.Array.Empty<IDRPG3DSpawnAnchor>(),
                    (_, _) => new[] { enemy },
                    loopStage: false);

                controller.StartStage();
                controller.TickForTest(1f);

                Assert.AreEqual(1, controller.CurrentWaveIndex);
                Assert.AreEqual(1, controller.ActiveEnemyCount);
                Assert.IsFalse(controller.WaitingForNextWave);
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(enemyObject);
            }
        }

        [Test]
        public void WaveControllerAdvancesAfterWaveEnemiesDieAndDelayExpires()
        {
            var controllerObject = new GameObject("WaveController");
            var firstEnemyObject = new GameObject("EnemyWave1");
            var secondEnemyObject = new GameObject("EnemyWave2");
            try
            {
                var firstEnemy = firstEnemyObject.AddComponent<IDRPG3DCombatUnit>();
                firstEnemy.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 20f, 4f, 1.5f, 1f, 6f);
                var secondEnemy = secondEnemyObject.AddComponent<IDRPG3DCombatUnit>();
                secondEnemy.Configure(1002, 0, IDRPG3DCombatFaction.Enemy, 10, 20f, 4f, 1.5f, 1f, 6f);
                var controller = controllerObject.AddComponent<IDRPG3DWaveController>();
                controller.Configure(
                    new[]
                    {
                        CreateTestWave(1, nextWaveDelay: 0.5f),
                        CreateTestWave(2, nextWaveDelay: 0.5f)
                    },
                    () => Vector3.zero,
                    () => Vector3.forward,
                    System.Array.Empty<IDRPG3DSpawnAnchor>(),
                    (wave, _) => wave.WaveIndex == 1 ? new[] { firstEnemy } : new[] { secondEnemy },
                    loopStage: false);

                controller.StartStage();
                firstEnemy.TakeDamage(100f, null);
                controller.TickForTest(0.1f);
                Assert.IsTrue(controller.WaitingForNextWave);
                Assert.AreEqual(1, controller.CurrentWaveIndex);

                controller.TickForTest(0.5f);

                Assert.IsFalse(controller.WaitingForNextWave);
                Assert.AreEqual(2, controller.CurrentWaveIndex);
                Assert.AreEqual(1, controller.ActiveEnemyCount);
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(firstEnemyObject);
                Object.DestroyImmediate(secondEnemyObject);
            }
        }

        [Test]
        public void WaveEnemySpawnAppliesConfiguredVisualScale()
        {
            var enemyObject = new GameObject("ScaledEnemy");
            try
            {
                IDRPG3DUnitVisualScale.Apply(enemyObject.transform, 3f);

                Assert.AreEqual(new Vector3(3f, 3f, 3f), enemyObject.transform.localScale);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
            }
        }

        [Test]
        public void WaveDefinitionStoresSpawnEngageMode()
        {
            var wave = new IDRPG3DWaveDefinition(
                waveId: 101,
                stageId: 1,
                waveIndex: 1,
                IDRPG3DWaveSpawnMode.SplineAhead,
                enemyId: 1001,
                enemyLevel: 1,
                count: 3,
                spawnDistanceAhead: 16f,
                spawnRadius: 2.4f,
                spawnAnchorId: string.Empty,
                isBoss: false,
                nextWaveDelay: 2f,
                IDRPG3DWaveEngageMode.RushTeam);

            Assert.AreEqual(IDRPG3DWaveEngageMode.RushTeam, wave.SpawnEngageMode);
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
        public void SkillCasterAppliesNonProjectileSkillImmediately()
        {
            var casterObject = new GameObject("Caster");
            var targetObject = new GameObject("Target");
            var projectilesRoot = new GameObject("Projectiles");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(2, 1, IDRPG3DCombatFaction.Hero, 90, 100f, 5f, 1.5f, 1f, 8f);
                target.TakeDamage(45f, null);

                var skillCaster = casterObject.AddComponent<IDRPG3DPrototypeSkillCaster>();
                skillCaster.Configure(
                    new IDRPG3DPrototypeSkillDefinition(
                        "heal",
                        "Heal",
                        level: 1,
                        new[] { IDRPG3DPrototypeEffectDefinition.Heal(200301, 35f) },
                        range: 7f,
                        cooldown: 2.2f,
                        projectileSpeed: 0f,
                        fallbackColor: Color.green),
                    projectilesRoot.transform);

                Assert.IsTrue(skillCaster.TryCast(target));
                Assert.AreEqual(90f, target.Health, 0.001f);
                Assert.AreEqual(0, projectilesRoot.transform.childCount);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(projectilesRoot);
            }
        }

        [Test]
        public void SkillConfigBuilderCreatesRuntimeDefinitionFromConfigRecord()
        {
            var record = new IDRPG3DPrototypeSkillConfigRecord(
                100101,
                "frostbolt",
                "Frostbolt",
                200101,
                18f,
                8.5f,
                1.45f,
                12f,
                "Assets/ThirdParty/Blink/Tools/RPGBuilder/Art/CombatVisuals/Projectiles/Frostbolt.prefab",
                "Assets/ThirdParty/Blink/Tools/RPGBuilder/Art/CombatVisuals/Muzzle/FrostMuzzle.prefab",
                "Assets/ThirdParty/Blink/Tools/RPGBuilder/ThirdPartyAssets/GabrielAguiarProductions/Unique_Projectiles_Volume_2/Prefabs/Hits/vfx_Hit_IceSpike01_Blue.prefab",
                new Color(0.25f, 0.78f, 1f, 1f));

            var skill = IDRPG3DPrototypeSkillConfigBuilder.Build(record);

            Assert.AreEqual("frostbolt", skill.SkillId);
            Assert.AreEqual("Frostbolt", skill.DisplayName);
            Assert.AreEqual(200101, skill.PrimaryEffect.EffectId);
            Assert.AreEqual(18f, skill.Damage, 0.001f);
            Assert.AreEqual(8.5f, skill.Range, 0.001f);
            Assert.AreEqual(1.45f, skill.Cooldown, 0.001f);
            Assert.AreEqual(12f, skill.ProjectileSpeed, 0.001f);
            Assert.AreEqual(new Color(0.25f, 0.78f, 1f, 1f), skill.FallbackColor);
            Assert.IsTrue(skill.IsValid);
        }

        [Test]
        public void SkillConfigBuilderAttachesConfiguredBuffToPrimaryEffect()
        {
            var slow = IDRPG3DPrototypeBuffDefinition.StatModifier(
                500101,
                "frost_slow",
                "Frost Slow",
                3f,
                2,
                IDRPG3DPrototypeStatType.MoveSpeed,
                IDRPG3DPrototypeModifierType.Percent,
                -0.3f);
            var record = new IDRPG3DPrototypeSkillConfigRecord(
                100101,
                "frostbolt",
                "Frostbolt",
                200101,
                18f,
                8.5f,
                1.45f,
                12f,
                string.Empty,
                string.Empty,
                string.Empty,
                new Color(0.25f, 0.78f, 1f, 1f),
                slow);

            var skill = IDRPG3DPrototypeSkillConfigBuilder.Build(record);

            Assert.IsTrue(skill.PrimaryEffect.HasBuff);
            Assert.AreEqual(500101, skill.PrimaryEffect.Buff.BuffId);
            Assert.AreEqual(2, skill.PrimaryEffect.Buff.MaxStack);
            Assert.AreEqual(IDRPG3DPrototypeStatType.MoveSpeed, skill.PrimaryEffect.Buff.StatType);
        }

        [Test]
        public void SkillConfigBuilderCreatesLevelAwareMultiEffectSkill()
        {
            var slow = IDRPG3DPrototypeBuffDefinition.StatModifier(
                500101,
                "frost_slow",
                "Frost Slow",
                3f,
                1,
                IDRPG3DPrototypeStatType.MoveSpeed,
                IDRPG3DPrototypeModifierType.Percent,
                -0.25f);
            var record = new IDRPG3DPrototypeSkillConfigRecord(
                1001,
                "frostbolt",
                "Frostbolt",
                level: 2,
                range: 9f,
                cooldown: 1.3f,
                projectileSpeed: 13f,
                projectilePrefabPath: string.Empty,
                muzzlePrefabPath: string.Empty,
                impactPrefabPath: string.Empty,
                fallbackColor: new Color(0.25f, 0.78f, 1f, 1f),
                effects: new[]
                {
                    IDRPG3DPrototypeEffectDefinition.Damage(200111, 26f),
                    IDRPG3DPrototypeEffectDefinition.AddBuff(200112, slow)
                });

            var skill = IDRPG3DPrototypeSkillConfigBuilder.Build(record);

            Assert.AreEqual("frostbolt", skill.SkillId);
            Assert.AreEqual(2, skill.Level);
            Assert.AreEqual(2, skill.Effects.Count);
            Assert.AreEqual(26f, skill.Damage, 0.001f);
            Assert.AreEqual(200111, skill.PrimaryEffect.EffectId);
            Assert.AreEqual(200112, skill.Effects[1].EffectId);
            Assert.AreEqual(500101, skill.Effects[1].Buff.BuffId);
        }

        [Test]
        public void CombatSyncEventsUseStableIds()
        {
            var cast = IDRPG3DCombatSyncEvents.CastSkill(
                sequence: 32,
                casterUnitId: 1,
                targetUnitId: 1001,
                skillId: 100101,
                skillKey: "frostbolt");

            var projectile = IDRPG3DCombatSyncEvents.SpawnProjectile(
                sequence: cast.Sequence,
                projectileId: 300101,
                casterUnitId: cast.CasterUnitId,
                targetUnitId: cast.TargetUnitId,
                skillId: cast.SkillId);
            var effect = IDRPG3DCombatSyncEvents.ApplyEffect(
                sequence: cast.Sequence,
                effectId: 200101,
                sourceUnitId: cast.CasterUnitId,
                targetUnitId: cast.TargetUnitId,
                value: 18f,
                buffId: 0);
            var buff = IDRPG3DCombatSyncEvents.ApplyBuff(
                sequence: cast.Sequence,
                buffId: 500101,
                sourceUnitId: cast.CasterUnitId,
                targetUnitId: cast.TargetUnitId,
                stack: 1,
                remainingTime: 3f);

            Assert.AreEqual(32, cast.Sequence);
            Assert.AreEqual(1, cast.CasterUnitId);
            Assert.AreEqual(1001, cast.TargetUnitId);
            Assert.AreEqual(100101, cast.SkillId);
            Assert.AreEqual("frostbolt", cast.SkillKey);
            Assert.AreEqual(300101, projectile.ProjectileId);
            Assert.AreEqual(100101, projectile.SkillId);
            Assert.AreEqual(200101, effect.EffectId);
            Assert.AreEqual(18f, effect.Value, 0.001f);
            Assert.AreEqual(0, effect.BuffId);
            Assert.AreEqual(500101, buff.BuffId);
            Assert.AreEqual(1, buff.Stack);
            Assert.AreEqual(3f, buff.RemainingTime, 0.001f);
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
        public void EffectRunnerAppliesDamageAndThreat()
        {
            var casterObject = new GameObject("Caster");
            var targetObject = new GameObject("Target");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 80f, 5f, 1.5f, 1f, 8f);
                var effect = IDRPG3DPrototypeEffectDefinition.Damage(200101, 18f);

                var result = IDRPG3DPrototypeEffectRunner.Apply(effect, caster, target);

                Assert.IsTrue(result.Applied);
                Assert.AreEqual(18f, result.Value, 0.001f);
                Assert.AreEqual(62f, target.Health, 0.001f);
                Assert.IsTrue(target.ThreatTable.TryGetHighestThreatTarget(unit => unit != null, out var threatTarget));
                Assert.AreSame(caster, threatTarget);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void AutoCombatBrainAttacksTargetInsideHorizontalRange()
        {
            var enemyObject = new GameObject("EnemyBrain");
            var heroObject = new GameObject("HeroTarget");
            try
            {
                enemyObject.transform.position = new Vector3(0f, 0f, 0f);
                heroObject.transform.position = new Vector3(0.8f, 5f, 0f);

                enemyObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
                enemyObject.AddComponent<IDRPG3DNavMoveAgent>();
                var enemy = enemyObject.AddComponent<IDRPG3DCombatUnit>();
                enemy.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 12f, 1.4f, 1f, 8f);

                var hero = heroObject.AddComponent<IDRPG3DCombatUnit>();
                hero.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 1.5f, 1f, 8f);

                var brain = enemyObject.AddComponent<IDRPG3DAutoCombatBrain>();
                brain.Initialize();
                brain.SetTarget(hero);

                typeof(IDRPG3DAutoCombatBrain)
                    .GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(brain, new object[] { 0.1f });

                Assert.Less(hero.Health, 100f);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(heroObject);
            }
        }

        [Test]
        public void EffectRunnerHealsWithoutExceedingMaxHealth()
        {
            var healerObject = new GameObject("Healer");
            var targetObject = new GameObject("Target");
            try
            {
                var healer = healerObject.AddComponent<IDRPG3DCombatUnit>();
                healer.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(2, 1, IDRPG3DCombatFaction.Hero, 90, 100f, 5f, 1.5f, 1f, 8f);
                target.TakeDamage(55f, null);

                var result = IDRPG3DPrototypeEffectRunner.Apply(
                    IDRPG3DPrototypeEffectDefinition.Heal(200301, 40f),
                    healer,
                    target);

                Assert.IsTrue(result.Applied);
                Assert.AreEqual(40f, result.Value, 0.001f);
                Assert.AreEqual(85f, target.Health, 0.001f);

                IDRPG3DPrototypeEffectRunner.Apply(IDRPG3DPrototypeEffectDefinition.Heal(200302, 40f), healer, target);
                Assert.AreEqual(100f, target.Health, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(healerObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void CombatUnitRegeneratesHealthOverTimeWithoutExceedingMaxHealth()
        {
            var unitObject = new GameObject("RegenUnit");
            try
            {
                var unit = unitObject.AddComponent<IDRPG3DCombatUnit>();
                unit.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 1.5f, 1f, 8f, 10f);
                unit.TakeDamage(35f, null);

                unit.TickForTest(1.5f);

                Assert.AreEqual(80f, unit.Health, 0.001f);

                unit.TickForTest(5f);

                Assert.AreEqual(100f, unit.Health, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
            }
        }

        [Test]
        public void ArmorBuffReducesIncomingDamage()
        {
            var attackerObject = new GameObject("Attacker");
            var targetObject = new GameObject("Target");
            try
            {
                var attacker = attackerObject.AddComponent<IDRPG3DCombatUnit>();
                attacker.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 1.5f, 1f, 8f);
                var armor = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500401,
                    "devotion_aura_armor",
                    "Devotion Aura",
                    3f,
                    1,
                    IDRPG3DPrototypeStatType.Armor,
                    IDRPG3DPrototypeModifierType.Add,
                    6f);

                targetObject.AddComponent<IDRPG3DPrototypeBuffController>().ApplyBuff(armor, target);
                target.TakeDamage(20f, attacker);

                Assert.AreEqual(6f, target.BonusArmor, 0.001f);
                Assert.AreEqual(86f, target.Health, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(attackerObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void BuffControllerStacksRefreshesAndExpiresMoveSpeedModifier()
        {
            var unitObject = new GameObject("Unit");
            try
            {
                var controller = unitObject.AddComponent<IDRPG3DPrototypeBuffController>();
                var slow = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500101,
                    "frost_slow",
                    "Frost Slow",
                    duration: 3f,
                    maxStack: 2,
                    IDRPG3DPrototypeStatType.MoveSpeed,
                    IDRPG3DPrototypeModifierType.Percent,
                    -0.2f);

                controller.ApplyBuff(slow, null);
                controller.ApplyBuff(slow, null);

                Assert.AreEqual(1, controller.ActiveBuffCount);
                Assert.AreEqual(2, controller.GetStack(500101));
                Assert.AreEqual(0.6f, controller.MoveSpeedMultiplier, 0.001f);

                controller.Tick(2f);
                Assert.AreEqual(1, controller.ActiveBuffCount);
                Assert.AreEqual(0.6f, controller.MoveSpeedMultiplier, 0.001f);

                controller.Tick(1.01f);
                Assert.AreEqual(0, controller.ActiveBuffCount);
                Assert.AreEqual(1f, controller.MoveSpeedMultiplier, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
            }
        }

        [Test]
        public void FrostSlowControlsBothMoveSpeedAndAttackSpeed()
        {
            var unitObject = new GameObject("SlowedUnit");
            try
            {
                var controller = unitObject.AddComponent<IDRPG3DPrototypeBuffController>();
                var slow = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500101,
                    "frost_slow",
                    "Frost Slow",
                    duration: 3f,
                    maxStack: 1,
                    IDRPG3DPrototypeStatType.MoveSpeed,
                    IDRPG3DPrototypeModifierType.Percent,
                    -0.6f);

                controller.ApplyBuff(slow, null);

                Assert.AreEqual(0.4f, controller.MoveSpeedMultiplier, 0.001f);
                Assert.AreEqual(0.4f, controller.AttackSpeedMultiplier, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
            }
        }

        [Test]
        public void ControlImmuneBossIgnoresFrostSlowBuff()
        {
            var casterObject = new GameObject("Caster");
            var bossObject = new GameObject("Boss");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var boss = bossObject.AddComponent<IDRPG3DCombatUnit>();
                boss.Configure(9001, 0, IDRPG3DCombatFaction.Enemy, 10, 1000f, 12f, 2f, 1f, 8f);
                boss.SetBoss(true);
                var slow = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500101,
                    "frost_slow",
                    "Frost Slow",
                    duration: 3f,
                    maxStack: 1,
                    IDRPG3DPrototypeStatType.MoveSpeed,
                    IDRPG3DPrototypeModifierType.Percent,
                    -0.6f);

                var result = IDRPG3DPrototypeEffectRunner.Apply(
                    IDRPG3DPrototypeEffectDefinition.AddBuff(200102, slow),
                    caster,
                    boss);

                var buffs = bossObject.GetComponent<IDRPG3DPrototypeBuffController>();
                Assert.IsFalse(result.Applied);
                Assert.IsNull(buffs);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(bossObject);
            }
        }

        [Test]
        public void NavMoveAgentAppliesMoveSpeedBuffMultiplierToAgentSpeed()
        {
            var unitObject = new GameObject("Mover");
            try
            {
                var unityAgent = unitObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
                var mover = unitObject.AddComponent<IDRPG3DNavMoveAgent>();
                var buffs = unitObject.AddComponent<IDRPG3DPrototypeBuffController>();
                mover.Initialize();
                mover.SetMoveStats(4f, 16f, 720f);
                var slow = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500101,
                    "frost_slow",
                    "Frost Slow",
                    duration: 3f,
                    maxStack: 1,
                    IDRPG3DPrototypeStatType.MoveSpeed,
                    IDRPG3DPrototypeModifierType.Percent,
                    -0.6f);

                buffs.ApplyBuff(slow, null);
                mover.TickForTest();

                Assert.AreEqual(0.4f, mover.CurrentSpeedMultiplier, 0.001f);
                Assert.AreEqual(1.6f, unityAgent.speed, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
            }
        }

        [Test]
        public void DotBuffTicksDamageOverTime()
        {
            var casterObject = new GameObject("Caster");
            var targetObject = new GameObject("Target");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);
                var burn = IDRPG3DPrototypeBuffDefinition.DamageOverTime(
                    500201,
                    "fire_burn",
                    "Burn",
                    duration: 4f,
                    maxStack: 1,
                    tickInterval: 1f,
                    tickValue: 5f);
                var controller = targetObject.AddComponent<IDRPG3DPrototypeBuffController>();

                controller.ApplyBuff(burn, caster);
                controller.Tick(0.5f);
                Assert.AreEqual(100f, target.Health, 0.001f);

                controller.Tick(0.5f);
                Assert.AreEqual(95f, target.Health, 0.001f);

                controller.Tick(2f);
                Assert.AreEqual(85f, target.Health, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void AuraBuffAppliesArmorToNearbyAllies()
        {
            var sourceObject = new GameObject("AuraSource");
            var allyObject = new GameObject("Ally");
            var farAllyObject = new GameObject("FarAlly");
            var enemyObject = new GameObject("Enemy");
            try
            {
                var source = sourceObject.AddComponent<IDRPG3DCombatUnit>();
                source.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var ally = allyObject.AddComponent<IDRPG3DCombatUnit>();
                ally.Configure(2, 1, IDRPG3DCombatFaction.Hero, 90, 100f, 5f, 8f, 1f, 8f);
                var farAlly = farAllyObject.AddComponent<IDRPG3DCombatUnit>();
                farAlly.Configure(3, 2, IDRPG3DCombatFaction.Hero, 80, 100f, 5f, 8f, 1f, 8f);
                var enemy = enemyObject.AddComponent<IDRPG3DCombatUnit>();
                enemy.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 8f, 1f, 8f);
                allyObject.transform.position = new Vector3(2f, 0f, 0f);
                farAllyObject.transform.position = new Vector3(8f, 0f, 0f);
                enemyObject.transform.position = new Vector3(2f, 0f, 1f);
                var armor = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500401,
                    "devotion_aura_armor",
                    "Devotion Aura",
                    2f,
                    1,
                    IDRPG3DPrototypeStatType.Armor,
                    IDRPG3DPrototypeModifierType.Add,
                    5f);
                var aura = IDRPG3DPrototypeBuffDefinition.Aura(
                    500400,
                    "devotion_aura",
                    "Devotion Aura",
                    duration: 10f,
                    tickInterval: 1f,
                    auraRadius: 4f,
                    armor);

                var controller = sourceObject.AddComponent<IDRPG3DPrototypeBuffController>();
                controller.ApplyBuff(aura, source);
                controller.Tick(1f);

                Assert.AreEqual(5f, ally.BonusArmor, 0.001f);
                Assert.AreEqual(0f, farAlly.BonusArmor, 0.001f);
                Assert.AreEqual(0f, enemy.BonusArmor, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(sourceObject);
                Object.DestroyImmediate(allyObject);
                Object.DestroyImmediate(farAllyObject);
                Object.DestroyImmediate(enemyObject);
            }
        }

        [Test]
        public void ProjectileRunsConfiguredEffectOnImpact()
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
                var slow = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500101,
                    "frost_slow",
                    "Frost Slow",
                    3f,
                    1,
                    IDRPG3DPrototypeStatType.MoveSpeed,
                    IDRPG3DPrototypeModifierType.Percent,
                    -0.3f);
                var effect = IDRPG3DPrototypeEffectDefinition.DamageWithBuff(200101, 18f, slow);
                var skill = new IDRPG3DPrototypeSkillDefinition(
                    "frostbolt",
                    "Frostbolt",
                    effect,
                    8.5f,
                    1.45f,
                    12f,
                    new Color(0.25f, 0.78f, 1f, 1f));

                var projectile = projectileObject.AddComponent<IDRPG3DPrototypeProjectile>();
                projectile.Launch(caster, target, skill, Vector3.zero);
                projectile.ApplyImpactForTest();

                var buffs = target.GetComponent<IDRPG3DPrototypeBuffController>();
                Assert.AreEqual(62f, target.Health, 0.001f);
                Assert.IsNotNull(buffs);
                Assert.AreEqual(1, buffs.GetStack(500101));
                Assert.AreEqual(0.7f, buffs.MoveSpeedMultiplier, 0.001f);
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

        private static IDRPG3DWaveDefinition CreateTestWave(int waveIndex, float nextWaveDelay)
        {
            return new IDRPG3DWaveDefinition(
                waveId: 100 + waveIndex,
                stageId: 1,
                waveIndex: waveIndex,
                IDRPG3DWaveSpawnMode.SplineAhead,
                enemyId: 1001,
                enemyLevel: 1,
                count: 1,
                spawnDistanceAhead: 8f,
                spawnRadius: 1f,
                spawnAnchorId: string.Empty,
                isBoss: false,
                nextWaveDelay: nextWaveDelay);
        }
    }
}
