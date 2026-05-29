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
        public void ThreatTableReportsThreatValueForTargets()
        {
            var tank = new IDRPG3DThreatTestTarget(true);
            var ranged = new IDRPG3DThreatTestTarget(true);
            var table = new IDRPG3DThreatTable<IDRPG3DThreatTestTarget>();

            table.AddThreat(tank, 10f);
            table.AddThreat(ranged, 30f);

            var found = table.TryGetHighestThreatTarget(target => target.IsAlive, out var target, out var threat);

            Assert.IsTrue(found);
            Assert.AreSame(ranged, target);
            Assert.AreEqual(30f, threat, 0.001f);
            Assert.AreEqual(10f, table.GetThreat(tank), 0.001f);
        }

        [Test]
        public void CombatResourceSpendsGainsAndTicksWithinBounds()
        {
            var unitObject = new GameObject("ResourceHero");
            try
            {
                var resource = unitObject.AddComponent<IDRPG3DCombatResource>();
                resource.Configure(IDRPG3DCombatResourceType.Mana, maxValue: 100f, initialValue: 40f, regenPerSecond: 5f);

                Assert.IsTrue(resource.TrySpend(25f));
                Assert.AreEqual(15f, resource.CurrentValue);
                Assert.IsFalse(resource.TrySpend(20f));

                resource.Gain(200f);
                Assert.AreEqual(100f, resource.CurrentValue);

                resource.Configure(IDRPG3DCombatResourceType.Mana, maxValue: 100f, initialValue: 10f, regenPerSecond: 5f);
                resource.TickForTest(2f);
                Assert.AreEqual(20f, resource.CurrentValue);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
            }
        }

        [Test]
        public void HeroProgressionLevelsUpAndUpdatesUnitLevel()
        {
            var unitObject = new GameObject("ProgressionHero");
            try
            {
                var unit = unitObject.AddComponent<IDRPG3DCombatUnit>();
                unit.Configure(1, 0, IDRPG3DCombatFaction.Hero, 10, 100f, 10f, 1.5f, 1f, 8f);
                var progression = unitObject.AddComponent<IDRPG3DHeroProgression>();
                progression.Configure(
                    unit,
                    heroId: 1,
                    startLevel: 1,
                    maxLevel: 3,
                    requiredExperienceByLevel: new[] { 0, 50, 120 });

                progression.AddExperience(60);

                Assert.AreEqual(2, progression.Level);
                Assert.AreEqual(2, unit.Level);
                Assert.AreEqual(10, progression.CurrentExperience);
                Assert.AreEqual(120, progression.NextLevelRequiredExperience);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
            }
        }

        [Test]
        public void StageLevelResolverClampsDynamicWaveLevel()
        {
            var stage = new IDRPG3DStageLevelRule(
                "PartyAverage",
                minEnemyLevel: 3,
                maxEnemyLevel: 8,
                baseLevelOffset: 0,
                powerScale: 1f);
            var lowWave = new IDRPG3DWaveLevelRule("Inherit", fixedEnemyLevel: 0, levelOffset: -10, minEnemyLevel: 0, maxEnemyLevel: 0);
            var highWave = new IDRPG3DWaveLevelRule("Inherit", fixedEnemyLevel: 0, levelOffset: 10, minEnemyLevel: 0, maxEnemyLevel: 0);
            var fixedWave = new IDRPG3DWaveLevelRule("Fixed", fixedEnemyLevel: 6, levelOffset: 0, minEnemyLevel: 0, maxEnemyLevel: 0);

            Assert.AreEqual(3, IDRPG3DStageLevelResolver.ResolveEnemyLevel(stage, lowWave, partyReferenceLevel: 5, fallbackLevel: 1));
            Assert.AreEqual(8, IDRPG3DStageLevelResolver.ResolveEnemyLevel(stage, highWave, partyReferenceLevel: 5, fallbackLevel: 1));
            Assert.AreEqual(6, IDRPG3DStageLevelResolver.ResolveEnemyLevel(stage, fixedWave, partyReferenceLevel: 1, fallbackLevel: 1));
        }

        [Test]
        public void WorldUnitBarUpdatesLevelText()
        {
            var unitObject = new GameObject("LevelUnit");
            var prefab = new GameObject("WorldBarPrefab");
            var healthBar = new GameObject("HealthBar");
            var level = new GameObject("Level");
            try
            {
                healthBar.transform.SetParent(prefab.transform, false);
                level.transform.SetParent(prefab.transform, false);
                var text = level.AddComponent<UnityEngine.UI.Text>();

                var unit = unitObject.AddComponent<IDRPG3DCombatUnit>();
                unit.Configure(1, 0, IDRPG3DCombatFaction.Hero, 10, 100f, 10f, 1.5f, 1f, 8f);
                unit.SetLevel(4);
                var bar = unitObject.AddComponent<IDRPG3DWorldUnitBar>();
                bar.Configure(unit, prefab, Vector3.zero, 1f, false, 0f);
                var runtimeText = bar.BarRootForTest.GetComponentInChildren<UnityEngine.UI.Text>(true);

                Assert.AreEqual("Lv.4", runtimeText.text);

                unit.SetLevel(5);
                Assert.AreEqual("Lv.5", runtimeText.text);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void CameraTargetFollowerTracksAliveHeroCenterWithoutChangingRotation()
        {
            var targetObject = new GameObject("CameraTarget");
            var heroObject = new GameObject("Hero");
            var secondHeroObject = new GameObject("SecondHero");
            var enemyObject = new GameObject("Enemy");
            var deadObject = new GameObject("DeadHero");
            try
            {
                targetObject.transform.rotation = Quaternion.Euler(12f, 34f, 56f);
                heroObject.transform.position = new Vector3(2f, 1f, 4f);
                secondHeroObject.transform.position = new Vector3(8f, 3f, 10f);
                enemyObject.transform.position = new Vector3(100f, 100f, 100f);
                deadObject.transform.position = new Vector3(100f, 100f, 100f);

                var hero = heroObject.AddComponent<IDRPG3DCombatUnit>();
                hero.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 1.5f, 1f, 8f);
                var secondHero = secondHeroObject.AddComponent<IDRPG3DCombatUnit>();
                secondHero.Configure(2, 1, IDRPG3DCombatFaction.Hero, 90, 100f, 5f, 1.5f, 1f, 8f);
                var enemy = enemyObject.AddComponent<IDRPG3DCombatUnit>();
                enemy.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);
                var dead = deadObject.AddComponent<IDRPG3DCombatUnit>();
                dead.Configure(3, 2, IDRPG3DCombatFaction.Hero, 80, 100f, 5f, 1.5f, 1f, 8f);
                dead.TakeDamage(200f, null);

                var follower = targetObject.AddComponent<IDRPG3DCameraTargetFollower>();
                follower.Configure(new[] { hero, secondHero, enemy, dead });
                var originalRotation = targetObject.transform.rotation;

                follower.TickForTest();

                Assert.AreEqual(new Vector3(5f, 2f, 7f), targetObject.transform.position);
                Assert.AreEqual(originalRotation, targetObject.transform.rotation);
                Assert.AreEqual(2, follower.TrackedUnitCountForTest);
            }
            finally
            {
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(heroObject);
                Object.DestroyImmediate(secondHeroObject);
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(deadObject);
            }
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
        public void AnimatorBridgeAppliesMoveSpeedMultiplierToPlaybackSpeed()
        {
            var root = new GameObject("AnimatorBridgeMoveSpeedRoot");
            try
            {
                var bridge = root.AddComponent<IDRPG3DAnimatorBridge>();

                bridge.SetMoveSpeed(2f, 0.4f);

                Assert.AreEqual(0.4f, bridge.CurrentPlaybackSpeed, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void AnimatorBridgeAppliesAttackSpeedMultiplierToAttackPlaybackSpeed()
        {
            var root = new GameObject("AnimatorBridgeAttackSpeedRoot");
            try
            {
                var bridge = root.AddComponent<IDRPG3DAnimatorBridge>();
                bridge.ConfigureClips(null, null, null, new AnimationClip(), null);

                bridge.PlayMeleeAttack(1.8f);

                Assert.AreEqual(1.8f, bridge.CurrentPlaybackSpeed, 0.001f);
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
        public void ConfiguredProjectileEffectPrefabsDoNotUseLegacyParticleShaders()
        {
            var prefabPaths = new[]
            {
                "Assets/AssetRaw/Effects/Skills/Frostbolt/Frostbolt.prefab",
                "Assets/AssetRaw/Effects/Skills/Frostbolt/FrostMuzzle.prefab",
                "Assets/AssetRaw/Effects/Skills/Frostbolt/vfx_Hit_IceSpike01_Blue.prefab",
                "Assets/AssetRaw/Effects/Skills/Fireball/Fireball.prefab",
                "Assets/AssetRaw/Effects/Skills/Fireball/FireMuzzle.prefab",
                "Assets/AssetRaw/Effects/Skills/Fireball/vfx_Hit_Fireball04_Orange.prefab"
            };

            for (var i = 0; i < prefabPaths.Length; i++)
            {
                AssertProjectilePrefabDoesNotUseLegacyParticleShaders(prefabPaths[i]);
            }
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
        public void SkillCasterPublishesProjectileSkillLifecycleEvents()
        {
            var casterObject = new GameObject("Caster");
            var targetObject = new GameObject("Target");
            var events = new List<IDRPG3DCombatEvent>();
            try
            {
                IDRPG3DCombatEventStream.ResetForTest();
                IDRPG3DCombatEventStream.EventPublished += events.Add;

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
                var skillRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1001,
                    "frostbolt",
                    "Frostbolt",
                    level: 1,
                    range: 8.5f,
                    cooldown: 1.45f,
                    projectileSpeed: 12f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: new Color(0.25f, 0.78f, 1f, 1f),
                    effects: new[]
                    {
                        IDRPG3DPrototypeEffectDefinition.DamageWithBuff(200101, 18f, slow)
                    });
                var skillCaster = casterObject.AddComponent<IDRPG3DPrototypeSkillCaster>();
                skillCaster.Configure(IDRPG3DPrototypeSkillConfigBuilder.Build(skillRecord), null);

                Assert.IsTrue(skillCaster.TryCast(target));
                var projectile = UnityEngine.Object.FindFirstObjectByType<IDRPG3DPrototypeProjectile>();
                Assert.IsNotNull(projectile);

                projectile.ApplyImpactForTest();

                Assert.AreEqual(6, events.Count);
                Assert.AreEqual(IDRPG3DCombatEventType.CastStart, events[0].EventType);
                Assert.AreEqual(IDRPG3DCombatEventType.ProjectileSpawn, events[1].EventType);
                Assert.AreEqual(IDRPG3DCombatEventType.ProjectileImpact, events[2].EventType);
                Assert.AreEqual(IDRPG3DCombatEventType.ApplyEffect, events[3].EventType);
                Assert.AreEqual(IDRPG3DCombatEventType.ApplyBuff, events[4].EventType);
                Assert.AreEqual(IDRPG3DCombatEventType.CastEnd, events[5].EventType);
                Assert.AreEqual(1, events[0].ActionId);
                Assert.AreEqual(1001, events[0].SkillId);
                Assert.AreEqual("frostbolt", events[0].SkillKey);
                Assert.AreEqual(events[0].ActionId, events[5].ActionId);
                Assert.AreEqual(events[1].ProjectileId, events[2].ProjectileId);
                Assert.AreEqual(200101, events[3].EffectId);
                Assert.AreEqual(18f, events[3].Value, 0.001f);
                Assert.AreEqual(target.Health, events[3].TargetHealth, 0.001f);
                Assert.AreEqual(500101, events[4].BuffId);
                Assert.AreEqual(1, events[4].Stack);
                Assert.AreEqual(3f, events[4].RemainingTime, 0.001f);
            }
            finally
            {
                IDRPG3DCombatEventStream.EventPublished -= events.Add;
                IDRPG3DCombatEventStream.ResetForTest();
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void SkillCasterPublishesInstantSkillLifecycleEvents()
        {
            var casterObject = new GameObject("Caster");
            var targetObject = new GameObject("Target");
            var events = new List<IDRPG3DCombatEvent>();
            try
            {
                IDRPG3DCombatEventStream.ResetForTest();
                IDRPG3DCombatEventStream.EventPublished += events.Add;

                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 80f, 5f, 1.5f, 1f, 8f);
                var skillRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1003,
                    "smite",
                    "Smite",
                    level: 1,
                    range: 8f,
                    cooldown: 1.2f,
                    projectileSpeed: 0f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.white,
                    effects: new[]
                    {
                        IDRPG3DPrototypeEffectDefinition.Damage(200301, 12f)
                    });
                var skillCaster = casterObject.AddComponent<IDRPG3DPrototypeSkillCaster>();
                skillCaster.Configure(IDRPG3DPrototypeSkillConfigBuilder.Build(skillRecord), null);

                Assert.IsTrue(skillCaster.TryCast(target));

                Assert.AreEqual(3, events.Count);
                Assert.AreEqual(IDRPG3DCombatEventType.CastStart, events[0].EventType);
                Assert.AreEqual(IDRPG3DCombatEventType.ApplyEffect, events[1].EventType);
                Assert.AreEqual(IDRPG3DCombatEventType.CastEnd, events[2].EventType);
                Assert.AreEqual(1003, events[0].SkillId);
                Assert.AreEqual(200301, events[1].EffectId);
                Assert.AreEqual(68f, events[1].TargetHealth, 0.001f);
            }
            finally
            {
                IDRPG3DCombatEventStream.EventPublished -= events.Add;
                IDRPG3DCombatEventStream.ResetForTest();
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
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
        public void EffectRunnerAppliesDamageThreatMultiplier()
        {
            var casterObject = new GameObject("ThreatCaster");
            var targetObject = new GameObject("ThreatTarget");
            var otherObject = new GameObject("OtherThreat");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var other = otherObject.AddComponent<IDRPG3DCombatUnit>();
                other.Configure(2, 1, IDRPG3DCombatFaction.Hero, 90, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 200f, 5f, 1.5f, 1f, 8f);
                target.ThreatTable.AddThreat(other, 90f);

                var result = IDRPG3DPrototypeEffectRunner.Apply(
                    IDRPG3DPrototypeEffectDefinition.Damage(200101, 20f),
                    caster,
                    target,
                    threatMultiplier: 5f);

                Assert.IsTrue(result.Applied);
                Assert.IsTrue(target.ThreatTable.TryGetHighestThreatTarget(unit => unit != null, out var threatTarget));
                Assert.AreSame(caster, threatTarget);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
                Object.DestroyImmediate(otherObject);
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
        public void AutoCombatBrainCommitsAttackAfterTargetMovesOutOfRange()
        {
            var enemyObject = new GameObject("CommittedEnemyBrain");
            var heroObject = new GameObject("CommittedHeroTarget");
            try
            {
                enemyObject.transform.position = Vector3.zero;
                heroObject.transform.position = new Vector3(1.2f, 0f, 0f);

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
                    ?.Invoke(brain, new object[] { 0.01f });

                Assert.AreEqual(100f, hero.Health, 0.001f);

                heroObject.transform.position = new Vector3(5f, 0f, 0f);
                typeof(IDRPG3DAutoCombatBrain)
                    .GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(brain, new object[] { 0.2f });

                Assert.Less(hero.Health, 100f);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(heroObject);
            }
        }

        [Test]
        public void AutoCombatBrainFindsNewEnemyWhenCurrentTargetDiesWithoutThreatFallback()
        {
            var enemyObject = new GameObject("RetargetEnemyBrain");
            var deadHeroObject = new GameObject("DeadHeroTarget");
            var liveHeroObject = new GameObject("LiveHeroTarget");
            try
            {
                enemyObject.transform.position = Vector3.zero;
                deadHeroObject.transform.position = new Vector3(1f, 0f, 0f);
                liveHeroObject.transform.position = new Vector3(1.2f, 0f, 0f);

                enemyObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
                enemyObject.AddComponent<IDRPG3DNavMoveAgent>();
                var enemy = enemyObject.AddComponent<IDRPG3DCombatUnit>();
                enemy.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 12f, 1.4f, 1f, 8f);

                var deadHero = deadHeroObject.AddComponent<IDRPG3DCombatUnit>();
                deadHero.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 1.5f, 1f, 8f);
                deadHero.TakeDamage(200f, enemy);

                var liveHero = liveHeroObject.AddComponent<IDRPG3DCombatUnit>();
                liveHero.Configure(2, 1, IDRPG3DCombatFaction.Hero, 90, 100f, 5f, 1.5f, 1f, 8f);

                var brain = enemyObject.AddComponent<IDRPG3DAutoCombatBrain>();
                brain.Initialize();
                brain.SetTarget(deadHero);

                typeof(IDRPG3DAutoCombatBrain)
                    .GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(brain, new object[] { 0.1f });

                var currentTarget = typeof(IDRPG3DAutoCombatBrain)
                    .GetField("currentTarget", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(brain);
                Assert.AreSame(liveHero, currentTarget);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(deadHeroObject);
                Object.DestroyImmediate(liveHeroObject);
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
        public void EffectRunnerTreatsAreaDamageAsDamage()
        {
            var casterObject = new GameObject("AreaCaster");
            var targetObject = new GameObject("AreaTarget");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);

                var result = IDRPG3DPrototypeEffectRunner.Apply(
                    IDRPG3DPrototypeEffectDefinition.AreaDamage(210301, 30f),
                    caster,
                    target);

                Assert.IsTrue(result.Applied);
                Assert.AreEqual(30f, result.Value, 0.001f);
                Assert.AreEqual(70f, target.Health, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void EffectRunnerCanResurrectDeadAlly()
        {
            var healerObject = new GameObject("Priest");
            var targetObject = new GameObject("DeadAlly");
            try
            {
                var healer = healerObject.AddComponent<IDRPG3DCombatUnit>();
                healer.Configure(3, 0, IDRPG3DCombatFaction.Hero, 50, 100f, 5f, 8f, 1f, 8f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1, 1, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 1.5f, 1f, 8f);
                target.TakeDamage(200f, null);

                var result = IDRPG3DPrototypeEffectRunner.Apply(
                    IDRPG3DPrototypeEffectDefinition.Resurrect(230301, 0.35f),
                    healer,
                    target);

                Assert.IsTrue(result.Applied);
                Assert.AreEqual(0.35f, result.Value, 0.001f);
                Assert.IsTrue(target.IsAlive);
                Assert.AreEqual(35f, target.Health, 0.001f);
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
        public void BuffControllerCalculatesCastSpeedMultiplier()
        {
            var unitObject = new GameObject("CasterSpeedUnit");
            try
            {
                var controller = unitObject.AddComponent<IDRPG3DPrototypeBuffController>();
                var haste = IDRPG3DPrototypeBuffDefinition.StatModifier(
                    500301,
                    "cast_haste",
                    "Cast Haste",
                    duration: 3f,
                    maxStack: 1,
                    IDRPG3DPrototypeStatType.CastSpeed,
                    IDRPG3DPrototypeModifierType.Percent,
                    0.35f);

                controller.ApplyBuff(haste, null);

                Assert.AreEqual(1.35f, controller.CastSpeedMultiplier, 0.001f);
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
        public void SkillBookPrioritizesReadyNonBasicSkillBeforeBasicAttack()
        {
            var casterObject = new GameObject("Mage");
            var targetObject = new GameObject("Enemy");
            try
            {
                var caster = casterObject.AddComponent<IDRPG3DCombatUnit>();
                caster.Configure(2, 0, IDRPG3DCombatFaction.Hero, 60, 100f, 5f, 8.5f, 1.5f, 8f);
                var resource = casterObject.AddComponent<IDRPG3DCombatResource>();
                resource.Configure(IDRPG3DCombatResourceType.Mana, 100f, 100f, 0f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);
                var basicRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1201,
                    "mage_basic_shot",
                    "Arcane Shot",
                    level: 1,
                    range: 8.5f,
                    cooldown: 1.55f,
                    projectileSpeed: 13f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.white,
                    IDRPG3DCombatResourceType.Mana,
                    resourceCost: 0f,
                    resourceGain: 0f,
                    IDRPG3DPrototypeSkillCastMode.Projectile,
                    IDRPG3DPrototypeSkillTargetRule.Enemy,
                    threatMultiplier: 1f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.Damage(220101, 13f) });
                var frostRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1202,
                    "frostbolt",
                    "Frostbolt",
                    level: 1,
                    range: 8.5f,
                    cooldown: 3f,
                    projectileSpeed: 12f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.cyan,
                    IDRPG3DCombatResourceType.Mana,
                    resourceCost: 15f,
                    resourceGain: 0f,
                    IDRPG3DPrototypeSkillCastMode.Projectile,
                    IDRPG3DPrototypeSkillTargetRule.Enemy,
                    threatMultiplier: 1f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.Damage(220201, 20f) });
                var skillBook = casterObject.AddComponent<IDRPG3DPrototypeSkillBook>();
                skillBook.Configure(
                    new[]
                    {
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(basicRecord),
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(frostRecord)
                    },
                    null);

                Assert.IsTrue(skillBook.TrySelectSkill(target, out var selected));
                Assert.AreEqual(1202, selected.Definition.ConfigId);
            }
            finally
            {
                Object.DestroyImmediate(casterObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void SkillBookPrioritizesResurrectionBeforeHealing()
        {
            var priestObject = new GameObject("Priest");
            var woundedAllyObject = new GameObject("WoundedAlly");
            var deadAllyObject = new GameObject("DeadAlly");
            var targetObject = new GameObject("Enemy");
            try
            {
                var priest = priestObject.AddComponent<IDRPG3DCombatUnit>();
                priest.Configure(3, 0, IDRPG3DCombatFaction.Hero, 50, 100f, 5f, 8.5f, 1.5f, 8f);
                priestObject.AddComponent<IDRPG3DCombatResource>()
                    .Configure(IDRPG3DCombatResourceType.Mana, 120f, 120f, 0f);

                var woundedAlly = woundedAllyObject.AddComponent<IDRPG3DCombatUnit>();
                woundedAlly.Configure(2, 1, IDRPG3DCombatFaction.Hero, 60, 100f, 5f, 8.5f, 1.5f, 8f);
                woundedAlly.TakeDamage(50f, null);

                var deadAlly = deadAllyObject.AddComponent<IDRPG3DCombatUnit>();
                deadAlly.Configure(1, 2, IDRPG3DCombatFaction.Hero, 100, 100f, 5f, 1.8f, 1.2f, 8f);
                deadAlly.TakeDamage(200f, null);

                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);

                var basicRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1301,
                    "priest_basic_shot",
                    "Holy Bolt",
                    level: 1,
                    range: 8.5f,
                    cooldown: 1.65f,
                    projectileSpeed: 12f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.white,
                    IDRPG3DCombatResourceType.Mana,
                    resourceCost: 0f,
                    resourceGain: 0f,
                    IDRPG3DPrototypeSkillCastMode.Projectile,
                    IDRPG3DPrototypeSkillTargetRule.Enemy,
                    threatMultiplier: 1f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.Damage(230101, 11f) });
                var healRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1302,
                    "heal",
                    "Heal",
                    level: 1,
                    range: 8f,
                    cooldown: 5f,
                    projectileSpeed: 0f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.green,
                    IDRPG3DCombatResourceType.Mana,
                    resourceCost: 20f,
                    resourceGain: 0f,
                    IDRPG3DPrototypeSkillCastMode.Instant,
                    IDRPG3DPrototypeSkillTargetRule.AllyLowestHp,
                    threatMultiplier: 0.5f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.Heal(230201, 36f) });
                var resurrectRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1303,
                    "resurrection",
                    "Resurrection",
                    level: 1,
                    range: 8f,
                    cooldown: 30f,
                    projectileSpeed: 0f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.yellow,
                    IDRPG3DCombatResourceType.Mana,
                    resourceCost: 60f,
                    resourceGain: 0f,
                    IDRPG3DPrototypeSkillCastMode.Instant,
                    IDRPG3DPrototypeSkillTargetRule.DeadAlly,
                    threatMultiplier: 0f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.Resurrect(230301, 0.35f) });
                var skillBook = priestObject.AddComponent<IDRPG3DPrototypeSkillBook>();
                skillBook.Configure(
                    new[]
                    {
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(basicRecord),
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(healRecord),
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(resurrectRecord)
                    },
                    null);

                Assert.IsTrue(skillBook.TrySelectSkill(target, out var selected));
                Assert.AreEqual(1303, selected.Definition.ConfigId);
            }
            finally
            {
                Object.DestroyImmediate(priestObject);
                Object.DestroyImmediate(woundedAllyObject);
                Object.DestroyImmediate(deadAllyObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void SkillBookPrioritizesThunderOverChargeWhenAlreadyInMelee()
        {
            var warriorObject = new GameObject("Warrior");
            var targetObject = new GameObject("Enemy");
            try
            {
                warriorObject.transform.position = Vector3.zero;
                targetObject.transform.position = new Vector3(1.2f, 0f, 0f);
                var warrior = warriorObject.AddComponent<IDRPG3DCombatUnit>();
                warrior.Configure(1, 0, IDRPG3DCombatFaction.Hero, 100, 260f, 22f, 1.8f, 1.25f, 8f);
                warriorObject.AddComponent<IDRPG3DCombatResource>()
                    .Configure(IDRPG3DCombatResourceType.Rage, 100f, 100f, 0f);
                var target = targetObject.AddComponent<IDRPG3DCombatUnit>();
                target.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);

                var basicRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1101,
                    "warrior_basic_attack",
                    "Warrior Strike",
                    level: 1,
                    range: 1.8f,
                    cooldown: 1.25f,
                    projectileSpeed: 0f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.white,
                    IDRPG3DCombatResourceType.Rage,
                    resourceCost: 0f,
                    resourceGain: 10f,
                    IDRPG3DPrototypeSkillCastMode.Melee,
                    IDRPG3DPrototypeSkillTargetRule.Enemy,
                    threatMultiplier: 1f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.Damage(210101, 22f) });
                var chargeRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1102,
                    "warrior_charge",
                    "Charge",
                    level: 1,
                    range: 9f,
                    cooldown: 8f,
                    projectileSpeed: 0f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.yellow,
                    IDRPG3DCombatResourceType.Rage,
                    resourceCost: 0f,
                    resourceGain: 0f,
                    IDRPG3DPrototypeSkillCastMode.Charge,
                    IDRPG3DPrototypeSkillTargetRule.Enemy,
                    threatMultiplier: 1.2f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.Damage(210201, 14f) });
                var thunderRecord = new IDRPG3DPrototypeSkillConfigRecord(
                    1103,
                    "thunder_clap",
                    "Thunder Clap",
                    level: 1,
                    range: 4f,
                    cooldown: 6f,
                    projectileSpeed: 0f,
                    projectilePrefabPath: string.Empty,
                    muzzlePrefabPath: string.Empty,
                    impactPrefabPath: string.Empty,
                    fallbackColor: Color.yellow,
                    IDRPG3DCombatResourceType.Rage,
                    resourceCost: 20f,
                    resourceGain: 0f,
                    IDRPG3DPrototypeSkillCastMode.Area,
                    IDRPG3DPrototypeSkillTargetRule.AreaEnemy,
                    threatMultiplier: 4f,
                    effects: new[] { IDRPG3DPrototypeEffectDefinition.AreaDamage(210301, 30f) });
                var skillBook = warriorObject.AddComponent<IDRPG3DPrototypeSkillBook>();
                skillBook.Configure(
                    new[]
                    {
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(basicRecord),
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(chargeRecord),
                        IDRPG3DPrototypeSkillConfigBuilder.BuildRuntime(thunderRecord)
                    },
                    null);

                Assert.IsTrue(skillBook.TrySelectSkill(target, out var selected));
                Assert.AreEqual(1103, selected.Definition.ConfigId);
            }
            finally
            {
                Object.DestroyImmediate(warriorObject);
                Object.DestroyImmediate(targetObject);
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

        [Test]
        public void WorldUnitBarInstantiatesPrefabAndUpdatesHealthAndResource()
        {
            var unitObject = new GameObject("Unit");
            var prefab = new GameObject("WorldBar_TestPrefab");
            try
            {
                var healthBar = new GameObject("HealthBar");
                healthBar.transform.SetParent(prefab.transform, false);
                var resourceBar = new GameObject("ResourceBar");
                resourceBar.transform.SetParent(prefab.transform, false);

                var unit = unitObject.AddComponent<IDRPG3DCombatUnit>();
                unit.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);
                var bar = unitObject.AddComponent<IDRPG3DWorldUnitBar>();
                bar.Configure(unit, prefab, new Vector3(0f, 2.2f, 0f), 1f, true, 0.75f);

                unit.TakeDamage(35f, null);

                Assert.IsNotNull(bar.BarRootForTest);
                Assert.IsTrue(bar.ResourceBarForTest.gameObject.activeSelf);
                Assert.AreEqual(0.65f, bar.HealthFillForTest, 0.001f);
                Assert.AreEqual(0.75f, bar.ResourceFillForTest, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void WorldUnitBarConfiguresProjectPrefabsWithoutProceduralProgressBarErrors()
        {
            AssertWorldUnitBarCanConfigurePrefab("Assets/AssetRaw/UI/WorldBars/WorldBar_Monster_HealthOnly.prefab", showResourceBar: false);
            AssertWorldUnitBarCanConfigurePrefab("Assets/AssetRaw/UI/WorldBars/WorldBar_Warrior_HealthRage.prefab", showResourceBar: true);
            AssertWorldUnitBarCanConfigurePrefab("Assets/AssetRaw/UI/WorldBars/WorldBar_Mage_HealthMana.prefab", showResourceBar: true);
        }

        [Test]
        public void WorldBarPrefabsKeepCompactRuntimeTransforms()
        {
            AssertWorldBarPrefabKeepsCompactRuntimeTransforms("Assets/AssetRaw/UI/WorldBars/WorldBar_Monster_HealthOnly.prefab", expectedBarCount: 1);
            AssertWorldBarPrefabKeepsCompactRuntimeTransforms("Assets/AssetRaw/UI/WorldBars/WorldBar_Warrior_HealthRage.prefab", expectedBarCount: 2);
            AssertWorldBarPrefabKeepsCompactRuntimeTransforms("Assets/AssetRaw/UI/WorldBars/WorldBar_Mage_HealthMana.prefab", expectedBarCount: 2);
        }

        [Test]
        public void WorldBarPrefabsUseProjectOwnedMaterialsAndConfiguredProceduralBars()
        {
            AssertWorldBarChildUsesConfiguredProgressBar(
                "Assets/AssetRaw/UI/WorldBars/WorldBar_Monster_HealthOnly.prefab",
                "HealthBar",
                "Assets/AssetRaw/Materials/WorldBars/WorldBar_Ragged_Health_Red.mat");
            AssertWorldBarChildUsesConfiguredProgressBar(
                "Assets/AssetRaw/UI/WorldBars/WorldBar_Warrior_HealthRage.prefab",
                "HealthBar",
                "Assets/AssetRaw/Materials/WorldBars/WorldBar_Ragged_Health_Red.mat");
            AssertWorldBarChildUsesConfiguredProgressBar(
                "Assets/AssetRaw/UI/WorldBars/WorldBar_Warrior_HealthRage.prefab",
                "ResourceBar",
                "Assets/AssetRaw/Materials/WorldBars/WorldBar_Ragged_Rage_Yellow.mat");
            AssertWorldBarChildUsesConfiguredProgressBar(
                "Assets/AssetRaw/UI/WorldBars/WorldBar_Mage_HealthMana.prefab",
                "HealthBar",
                "Assets/AssetRaw/Materials/WorldBars/WorldBar_Ragged_Health_Red.mat");
            AssertWorldBarChildUsesConfiguredProgressBar(
                "Assets/AssetRaw/UI/WorldBars/WorldBar_Mage_HealthMana.prefab",
                "ResourceBar",
                "Assets/AssetRaw/Materials/WorldBars/WorldBar_Ragged_Mana_Blue.mat");
        }

        [Test]
        public void WorldBarResourceMaterialsUseReadableManaAndRageColors()
        {
            AssertWorldBarMaterialColor(
                "Assets/AssetRaw/Materials/WorldBars/WorldBar_Ragged_Mana_Blue.mat",
                "_Fill_Color_No_Gradient",
                new Color(0.12f, 0.44f, 1f, 1f));
            AssertWorldBarMaterialColor(
                "Assets/AssetRaw/Materials/WorldBars/WorldBar_Ragged_Rage_Yellow.mat",
                "_Fill_Color_No_Gradient",
                new Color(1f, 0.42f, 0.04f, 1f));
        }

        private sealed class IDRPG3DThreatTestTarget
        {
            public IDRPG3DThreatTestTarget(bool isAlive)
            {
                IsAlive = isAlive;
            }

            public bool IsAlive { get; }
        }

        private static void AssertProjectilePrefabDoesNotUseLegacyParticleShaders(string prefabPath)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Missing projectile effect prefab: {prefabPath}");

            var dependencies = UnityEditor.AssetDatabase.GetDependencies(prefabPath, true);
            for (var i = 0; i < dependencies.Length; i++)
            {
                var dependencyPath = dependencies[i];
                Assert.IsFalse(
                    dependencyPath.StartsWith("Assets/ThirdParty/", System.StringComparison.OrdinalIgnoreCase),
                    $"{prefabPath} should use project-owned effect assets, but depends on {dependencyPath}.");

                if (!dependencyPath.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(dependencyPath);
                var shaderName = material != null && material.shader != null ? material.shader.name : string.Empty;
                Assert.IsFalse(
                    shaderName.StartsWith("Legacy Shaders/Particles", System.StringComparison.Ordinal),
                    $"{prefabPath} depends on legacy particle shader material {dependencyPath} ({shaderName}).");
            }
        }

        private static void AssertWorldBarPrefabKeepsCompactRuntimeTransforms(string prefabPath, int expectedBarCount)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Missing world bar prefab: {prefabPath}");

            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            Assert.AreEqual(expectedBarCount, meshFilters.Length, prefabPath);

            for (var i = 0; i < meshFilters.Length; i++)
            {
                var mesh = meshFilters[i].sharedMesh;
                Assert.IsNotNull(mesh, $"{prefabPath} has a bar without a mesh.");
                Assert.LessOrEqual(meshFilters[i].transform.localScale.x, 0.35f, $"{prefabPath} world bar scale should stay compact.");
                Assert.LessOrEqual(meshFilters[i].transform.localScale.y, 0.35f, $"{prefabPath} world bar scale should stay compact.");
            }
        }

        private static void AssertWorldBarChildUsesConfiguredProgressBar(string prefabPath, string childName, string materialPath)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var expectedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Assert.IsNotNull(prefab, $"Missing world bar prefab: {prefabPath}");
            Assert.IsNotNull(expectedMaterial, $"Missing world bar material: {materialPath}");

            var child = FindChildByName(prefab.transform, childName);
            Assert.IsNotNull(child, $"{prefabPath} is missing child {childName}.");

            var renderer = child.GetComponent<MeshRenderer>();
            Assert.IsNotNull(renderer, $"{prefabPath}/{childName} is missing a MeshRenderer.");
            Assert.AreSame(expectedMaterial, renderer.sharedMaterial, $"{prefabPath}/{childName} should use project-owned material {materialPath}.");

            var progressBar = FindProceduralProgressBar(child);
            Assert.IsNotNull(progressBar, $"{prefabPath}/{childName} is missing ProceduralProgressBar.");

            var type = progressBar.GetType();
            Assert.AreSame(expectedMaterial, GetFieldValue<Material>(type, progressBar, "progressBarMaterial"), $"{prefabPath}/{childName} progressBarMaterial is not configured.");
            Assert.AreSame(renderer, GetFieldValue<MeshRenderer>(type, progressBar, "barRenderer"), $"{prefabPath}/{childName} barRenderer is not configured.");
            Assert.IsTrue(GetFieldValue<bool>(type, progressBar, "instantiateMaterialOnStart"), $"{prefabPath}/{childName} should instantiate a runtime material.");
            Assert.IsTrue(GetFieldValue<bool>(type, progressBar, "UseInitialFillAmount"), $"{prefabPath}/{childName} should initialize its fill amount.");
            Assert.AreEqual(1f, GetFieldValue<float>(type, progressBar, "InitialFillAmount"), 0.001f, $"{prefabPath}/{childName} should start filled.");
        }

        private static void AssertWorldUnitBarCanConfigurePrefab(string prefabPath, bool showResourceBar)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Missing world bar prefab: {prefabPath}");

            var unitObject = new GameObject("WorldBarPrefabRuntimeTest");
            try
            {
                var unit = unitObject.AddComponent<IDRPG3DCombatUnit>();
                unit.Configure(1001, 0, IDRPG3DCombatFaction.Enemy, 10, 100f, 5f, 1.5f, 1f, 8f);
                var bar = unitObject.AddComponent<IDRPG3DWorldUnitBar>();

                Assert.DoesNotThrow(() => bar.Configure(unit, prefab, new Vector3(0f, 2.2f, 0f), 1f, showResourceBar, 1f), prefabPath);
                Assert.IsNotNull(bar.BarRootForTest, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(unitObject);
            }
        }

        private static void AssertWorldBarMaterialColor(string materialPath, string propertyName, Color expected)
        {
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Assert.IsNotNull(material, $"Missing world bar material: {materialPath}");
            Assert.IsTrue(material.HasProperty(propertyName), $"{materialPath} is missing shader property {propertyName}.");
            Assert.AreEqual(expected, material.GetColor(propertyName), $"{materialPath} has the wrong {propertyName} color.");
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            var children = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private static Component FindProceduralProgressBar(Transform bar)
        {
            var components = bar.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].GetType().FullName == "INab.UI.ProceduralProgressBar")
                {
                    return components[i];
                }
            }

            return null;
        }

        private static T GetFieldValue<T>(System.Type type, Component component, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(field, $"{type.FullName} is missing field {fieldName}.");
            return (T)field.GetValue(component);
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
