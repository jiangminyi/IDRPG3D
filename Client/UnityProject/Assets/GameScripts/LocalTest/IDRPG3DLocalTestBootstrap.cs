using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Dreamteck.Splines;
using IDRPG3D.GameplayPrototype;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace IDRPG3D.LocalTest
{
    public sealed class IDRPG3DLocalTestBootstrap : MonoBehaviour
    {
        private const string ServerHost = "127.0.0.1";
        private const int GameServerPort = 20000;
        private const int MongoPort = 27017;
        private const string GameServerProtocol = "KCP/UDP";
        private const string DefaultRouteName = "Route_Main_01";
        private const string DefaultHeroName = "Hero1";
        private const string DefaultEnemyName = "Enemy";
        private const string BossAnchorId = "stage_01_boss_01";
        private const string SpawnedEnemiesRootName = "Prototype_WaveEnemies";
        private const string ProjectilesRootName = "Prototype_Projectiles";
        private const string TerrainLayerName = "Terrain";
        private const string GroundTerrainLayerName = "GroundTerrain";
        private const int PrototypeStageId = 1;
        private const float PrototypeHeroDetectionRadius = 24f;
        private const int FrostboltSkillId = 1001;
        private const int FireballSkillId = 1002;
        private const int HealSkillId = 1003;
        private const int DevotionAuraSkillId = 1004;
        private const string FrostboltProjectilePath = "Assets/ThirdParty/Blink/Tools/RPGBuilder/Art/CombatVisuals/Projectiles/Frostbolt.prefab";
        private const string FrostboltMuzzlePath = "Assets/ThirdParty/Blink/Tools/RPGBuilder/Art/CombatVisuals/Muzzle/FrostMuzzle.prefab";
        private const string FrostboltImpactPath = "Assets/ThirdParty/Blink/Tools/RPGBuilder/ThirdPartyAssets/GabrielAguiarProductions/Unique_Projectiles_Volume_2/Prefabs/Hits/vfx_Hit_IceSpike01_Blue.prefab";
        private const string FireballProjectilePath = "Assets/ThirdParty/Blink/Tools/RPGBuilder/Art/CombatVisuals/Projectiles/Fireball.prefab";
        private const string FireballMuzzlePath = "Assets/ThirdParty/Blink/Tools/RPGBuilder/Art/CombatVisuals/Muzzle/FireMuzzle.prefab";
        private const string FireballImpactPath = "Assets/ThirdParty/Blink/Tools/RPGBuilder/ThirdPartyAssets/GabrielAguiarProductions/Unique_Projectiles_Volume_2/Prefabs/Hits/vfx_Hit_Fireball04_Orange.prefab";

        private static readonly string[] HeroNames = { "Hero1", "Hero2", "Hero3" };
        private static readonly string[] EnemyNames = { "Enemy", "Enemy1", "Enemy2", "Enemy3", "Monster_DebugCapsule" };

        private InputField accountInput;
        private Text statusText;
        private Text endpointText;
        private Button startServerButton;
        private Button checkPortsButton;
        private Button loginButton;
        private Button enterWorldButton;
        private Button startIdleButton;
        private Button stopIdleButton;
        private Button createTeamButton;

        private string currentAccount = "local_player_001";
        private Font defaultFont;
        private IDRPG3DLocalSkillConfigLoader skillConfigLoader;
        private IDRPG3DLocalWaveConfigLoader waveConfigLoader;
        private Dictionary<string, GameObject> enemyTemplates;
        private IReadOnlyList<IDRPG3DCombatUnit> activeHeroUnits = Array.Empty<IDRPG3DCombatUnit>();

        private void Awake()
        {
            BuildSceneVisuals();
            SetupGameplayPrototype();
            BuildUI();
            AppendStatus("Local test scene ready.");
            AppendStatus("Gameplay prototype: heroes follow route, clear waves, and fight a fixed boss anchor.");
            StartCoroutine(CheckPortsRoutine());
        }

        private void SetupGameplayPrototype()
        {
            var route = FindRoute();
            var heroes = FindNamedObjects(HeroNames);
            var enemies = FindNamedObjects(EnemyNames);
            if (route == null || heroes.Count == 0 || enemies.Count == 0)
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Gameplay prototype skipped. Route found: {route != null}, Heroes: {heroes.Count}, Enemies: {enemies.Count}.");
                return;
            }

            for (var i = 0; i < heroes.Count; i++)
            {
                DisableLegacySplineFollower(heroes[i]);
            }
            EnsureNavMeshSurface();
            var projectileRoot = EnsureProjectilesRoot();

            var heroUnits = new List<IDRPG3DCombatUnit>(heroes.Count);
            skillConfigLoader = new IDRPG3DLocalSkillConfigLoader();
            for (var i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                var unit = ConfigureUnit(hero, 1 + i, i, IDRPG3DCombatFaction.Hero, 100 - i * 10, 220f, 118f, 1.8f, 1.25f, 7f, 3.2f, 10f);
                ConfigureHeroPrototypeSkill(hero, projectileRoot);
                heroUnits.Add(unit);
            }
            ConfigureHeroSupportPrototype(heroUnits);
            activeHeroUnits = heroUnits;

            var routeController = GetComponent<IDRPG3DTeamRouteController>();
            if (routeController == null)
            {
                routeController = gameObject.AddComponent<IDRPG3DTeamRouteController>();
            }

            routeController.Configure(route, heroUnits);
            routeController.SetDetectionRadius(PrototypeHeroDetectionRadius);
            waveConfigLoader = new IDRPG3DLocalWaveConfigLoader();
            enemyTemplates = BuildEnemyTemplates(enemies);
            var anchors = new List<IDRPG3DSpawnAnchor> { EnsureBossSpawnAnchor(route) };
            ConfigureWaveController(routeController, anchors);
            Debug.Log($"[IDRPG3D LocalTest] Gameplay prototype wired: {heroUnits.Count} heroes follow route and clear configured waves.");
        }

        private static SplineComputer FindRoute()
        {
            var routeObject = GameObject.Find(DefaultRouteName);
            if (routeObject != null && routeObject.TryGetComponent<SplineComputer>(out var namedRoute))
            {
                return namedRoute;
            }

            return FindObjectOfType<SplineComputer>();
        }

        private static List<GameObject> FindNamedObjects(IReadOnlyList<string> names)
        {
            var results = new List<GameObject>(names.Count);
            for (var i = 0; i < names.Count; i++)
            {
                var target = GameObject.Find(names[i]);
                if (target != null)
                {
                    results.Add(target);
                }
            }

            return results;
        }

        private static Dictionary<string, GameObject> BuildEnemyTemplates(IReadOnlyList<GameObject> enemies)
        {
            var templates = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                templates[enemy.name] = enemy;
                if (!templates.ContainsKey(DefaultEnemyName))
                {
                    templates[DefaultEnemyName] = enemy;
                }

                enemy.SetActive(false);
            }

            return templates;
        }

        private IDRPG3DSpawnAnchor EnsureBossSpawnAnchor(SplineComputer route)
        {
            var anchors = FindObjectsOfType<IDRPG3DSpawnAnchor>(true);
            for (var i = 0; i < anchors.Length; i++)
            {
                if (anchors[i] != null && string.Equals(anchors[i].AnchorId, BossAnchorId, StringComparison.OrdinalIgnoreCase))
                {
                    return anchors[i];
                }
            }

            var anchorObject = GameObject.Find("BossSpawn_Stage01");
            if (anchorObject == null)
            {
                anchorObject = new GameObject("BossSpawn_Stage01");
            }

            var anchor = anchorObject.GetComponent<IDRPG3DSpawnAnchor>();
            if (anchor == null)
            {
                anchor = anchorObject.AddComponent<IDRPG3DSpawnAnchor>();
            }

            anchor.Configure(BossAnchorId);
            PlaceBossAnchor(anchorObject.transform, route);
            return anchor;
        }

        private static void PlaceBossAnchor(Transform anchor, SplineComputer route)
        {
            if (anchor == null)
            {
                return;
            }

            if (route != null)
            {
                var sample = new SplineSample();
                route.Evaluate(0.82, ref sample);
                var forward = sample.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.forward;
                }

                anchor.SetPositionAndRotation(sample.position, Quaternion.LookRotation(forward.normalized, Vector3.up));
                return;
            }

            anchor.SetPositionAndRotation(new Vector3(0f, 0f, 18f), Quaternion.identity);
        }

        private void ConfigureWaveController(
            IDRPG3DTeamRouteController routeController,
            IReadOnlyList<IDRPG3DSpawnAnchor> anchors)
        {
            if (routeController == null || waveConfigLoader == null)
            {
                return;
            }

            if (!waveConfigLoader.TryBuildStage(PrototypeStageId, out var stageConfig))
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Stage wave config not found. StageId={PrototypeStageId}");
                return;
            }

            var waveController = GetComponent<IDRPG3DWaveController>();
            if (waveController == null)
            {
                waveController = gameObject.AddComponent<IDRPG3DWaveController>();
            }

            waveController.Configure(
                stageConfig.Waves,
                () => routeController.AnchorPosition,
                () => routeController.AnchorForward,
                anchors,
                SpawnWaveEnemies,
                stageConfig.LoopStage);
            waveController.StartStage();
            Debug.Log($"[IDRPG3D LocalTest] Wave controller started. Stage={stageConfig.StageKey}, Waves={stageConfig.Waves.Count}, Loop={stageConfig.LoopStage}.");
        }

        private IReadOnlyList<IDRPG3DCombatUnit> SpawnWaveEnemies(
            IDRPG3DWaveDefinition wave,
            IDRPG3DWaveSpawnPoint spawnPoint)
        {
            var spawnedUnits = new List<IDRPG3DCombatUnit>(wave.Count);
            if (waveConfigLoader == null || !waveConfigLoader.TryBuildEnemyStats(wave.EnemyId, wave.EnemyLevel, out var enemyConfig))
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Enemy config not found. EnemyId={wave.EnemyId}, Lv={wave.EnemyLevel}");
                return spawnedUnits;
            }

            if (!TryGetEnemyTemplate(enemyConfig.TemplateKey, out var template))
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] Enemy template not found. TemplateKey={enemyConfig.TemplateKey}");
                return spawnedUnits;
            }

            var root = EnsureSpawnedEnemiesRoot();
            for (var i = 0; i < wave.Count; i++)
            {
                var position = CalculateEnemySpawnPosition(spawnPoint, wave, i);
                var enemyObject = Instantiate(template, position, Quaternion.LookRotation(-spawnPoint.Forward, Vector3.up), root);
                IDRPG3DUnitVisualScale.Apply(enemyObject.transform, enemyConfig.VisualScale);
                enemyObject.name = $"{enemyConfig.EnemyKey}_Lv{enemyConfig.Level}_Wave{wave.WaveIndex}_{i + 1}";
                enemyObject.SetActive(true);
                var unit = ConfigureEnemyFromConfig(enemyObject, enemyConfig, wave, i);
                TryStartWaveEnemyEngage(unit, wave);
                spawnedUnits.Add(unit);
            }

            return spawnedUnits;
        }

        private void TryStartWaveEnemyEngage(IDRPG3DCombatUnit enemy, IDRPG3DWaveDefinition wave)
        {
            if (enemy == null || wave.SpawnEngageMode != IDRPG3DWaveEngageMode.RushTeam)
            {
                return;
            }

            if (!TryFindNearestAliveHero(enemy.transform.position, out var target))
            {
                return;
            }

            var brain = enemy.GetComponent<IDRPG3DAutoCombatBrain>();
            if (brain == null)
            {
                brain = enemy.gameObject.AddComponent<IDRPG3DAutoCombatBrain>();
            }

            brain.Initialize();
            brain.SetTarget(target);
            Debug.Log($"[IDRPG3D LocalTest] Enemy {enemy.name} rushes {target.name}. Wave={wave.WaveIndex}");
        }

        private bool TryFindNearestAliveHero(Vector3 position, out IDRPG3DCombatUnit hero)
        {
            hero = null;
            var bestSqrDistance = float.MaxValue;
            if (activeHeroUnits == null)
            {
                return false;
            }

            for (var i = 0; i < activeHeroUnits.Count; i++)
            {
                var candidate = activeHeroUnits[i];
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                var sqrDistance = (candidate.transform.position - position).sqrMagnitude;
                if (sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                bestSqrDistance = sqrDistance;
                hero = candidate;
            }

            return hero != null;
        }

        private bool TryGetEnemyTemplate(string templateKey, out GameObject template)
        {
            template = null;
            if (enemyTemplates == null || enemyTemplates.Count == 0)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(templateKey) && enemyTemplates.TryGetValue(templateKey, out template))
            {
                return template != null;
            }

            return enemyTemplates.TryGetValue(DefaultEnemyName, out template) && template != null;
        }

        private static Transform EnsureSpawnedEnemiesRoot()
        {
            var root = GameObject.Find(SpawnedEnemiesRootName);
            if (root == null)
            {
                root = new GameObject(SpawnedEnemiesRootName);
            }

            return root.transform;
        }

        private static Vector3 CalculateEnemySpawnPosition(
            IDRPG3DWaveSpawnPoint spawnPoint,
            IDRPG3DWaveDefinition wave,
            int index)
        {
            if (wave.Count <= 1 || wave.SpawnRadius <= 0f)
            {
                return spawnPoint.Position;
            }

            var forward = spawnPoint.Forward.sqrMagnitude > 0.0001f ? spawnPoint.Forward.normalized : Vector3.forward;
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var row = index / 3;
            var column = index % 3 - 1;
            var sideOffset = right * column * Mathf.Min(1.25f, wave.SpawnRadius);
            var depthOffset = -forward * row * 1.2f;
            return spawnPoint.Position + sideOffset + depthOffset;
        }

        private static IDRPG3DCombatUnit ConfigureEnemyFromConfig(
            GameObject enemyObject,
            IDRPG3DLocalEnemyConfig enemyConfig,
            IDRPG3DWaveDefinition wave,
            int order)
        {
            var unitId = wave.WaveId * 100 + order + 1;
            var unit = ConfigureUnit(
                enemyObject,
                unitId,
                order,
                IDRPG3DCombatFaction.Enemy,
                enemyConfig.MovePriority,
                enemyConfig.Health,
                enemyConfig.Attack,
                enemyConfig.AttackRange,
                enemyConfig.AttackInterval,
                enemyConfig.AggroRadius,
                enemyConfig.MoveSpeed);
            unit.SetBoss(wave.IsBoss);
            ConfigureEnemyHealthBar(enemyObject, unit, enemyConfig.VisualScale);
            return unit;
        }

        private static void DisableLegacySplineFollower(GameObject hero)
        {
            var follower = hero.GetComponent<SplineFollower>();
            if (follower != null)
            {
                follower.follow = false;
                follower.enabled = false;
            }
        }

        private void EnsureNavMeshSurface()
        {
            var surface = FindObjectOfType<NavMeshSurface>();
            if (surface == null)
            {
                surface = gameObject.AddComponent<NavMeshSurface>();
                surface.collectObjects = CollectObjects.All;
            }

            surface.layerMask = ResolveNavMeshLayerMask();
            if (surface.navMeshData == null)
            {
                surface.BuildNavMesh();
            }
        }

        private static LayerMask ResolveNavMeshLayerMask()
        {
            var mask = 0;
            AddLayerToMask(TerrainLayerName, ref mask);
            AddLayerToMask(GroundTerrainLayerName, ref mask);
            return mask != 0 ? mask : LayerMask.GetMask("Default");
        }

        private static void AddLayerToMask(string layerName, ref int mask)
        {
            var layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                mask |= 1 << layer;
            }
        }

        private static IDRPG3DCombatUnit ConfigureUnit(
            GameObject target,
            int id,
            int order,
            IDRPG3DCombatFaction faction,
            int priority,
            float health,
            float damage,
            float range,
            float interval,
            float aggro,
            float moveSpeed,
            float healthRegenPerSecond = 0f)
        {
            EnsureCollider(target);

            var agent = target.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = target.AddComponent<NavMeshAgent>();
            }
            agent.radius = 0.35f;
            agent.height = 1.8f;
            agent.speed = moveSpeed;
            agent.acceleration = 16f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0.08f;
            if (NavMesh.SamplePosition(target.transform.position, out var navHit, 3f, NavMesh.AllAreas))
            {
                target.transform.position = navHit.position;
            }

            var animatorBridge = target.GetComponent<IDRPG3DAnimatorBridge>();
            if (animatorBridge == null)
            {
                animatorBridge = target.AddComponent<IDRPG3DAnimatorBridge>();
            }
            TryConfigurePrototypeAnimationClips(animatorBridge);

            var mover = target.GetComponent<IDRPG3DNavMoveAgent>();
            if (mover == null)
            {
                mover = target.AddComponent<IDRPG3DNavMoveAgent>();
            }
            mover.Initialize();
            mover.SetMoveStats(moveSpeed, 16f, 720f);

            var unit = target.GetComponent<IDRPG3DCombatUnit>();
            if (unit == null)
            {
                unit = target.AddComponent<IDRPG3DCombatUnit>();
            }
            unit.Configure(id, order, faction, priority, health, damage, range, interval, aggro, healthRegenPerSecond);

            var brain = target.GetComponent<IDRPG3DAutoCombatBrain>();
            if (brain == null)
            {
                brain = target.AddComponent<IDRPG3DAutoCombatBrain>();
            }
            brain.Initialize();

            if (target.GetComponent<IDRPG3DSelectableUnit>() == null)
            {
                target.AddComponent<IDRPG3DSelectableUnit>();
            }

            ConfigureGrounding(target);

            return unit;
        }

        private static Transform EnsureProjectilesRoot()
        {
            var root = GameObject.Find(ProjectilesRootName);
            if (root == null)
            {
                root = new GameObject(ProjectilesRootName);
            }

            return root.transform;
        }

        private void ConfigureHeroPrototypeSkill(GameObject hero, Transform projectileRoot)
        {
            if (hero.name.Equals("Hero2", StringComparison.OrdinalIgnoreCase))
            {
                var skillCaster = EnsureSkillCaster(hero);
                skillCaster.Configure(CreateConfiguredOrFallbackSkill(
                    FrostboltSkillId,
                    IDRPG3DPrototypeSkillDefinition.CreateFrostbolt(
                        LoadEditorPrefab(FrostboltProjectilePath),
                        LoadEditorPrefab(FrostboltMuzzlePath),
                        LoadEditorPrefab(FrostboltImpactPath))), projectileRoot);
            }
            else if (hero.name.Equals("Hero3", StringComparison.OrdinalIgnoreCase))
            {
                var skillCaster = EnsureSkillCaster(hero);
                skillCaster.Configure(CreateConfiguredOrFallbackSkill(
                    FireballSkillId,
                    IDRPG3DPrototypeSkillDefinition.CreateFireball(
                        LoadEditorPrefab(FireballProjectilePath),
                        LoadEditorPrefab(FireballMuzzlePath),
                        LoadEditorPrefab(FireballImpactPath))), projectileRoot);
            }
        }

        private void ConfigureHeroSupportPrototype(IReadOnlyList<IDRPG3DCombatUnit> heroUnits)
        {
            if (heroUnits.Count == 0)
            {
                return;
            }

            ApplyConfiguredEffects(DevotionAuraSkillId, heroUnits[0], heroUnits[0], "Devotion Aura");

            if (heroUnits.Count <= 1)
            {
                return;
            }

            heroUnits[1].TakeDamage(20f, null);
            ApplyConfiguredEffects(HealSkillId, heroUnits[0], heroUnits[1], "Heal");
        }

        private void ApplyConfiguredEffects(
            int skillId,
            IDRPG3DCombatUnit caster,
            IDRPG3DCombatUnit target,
            string debugName)
        {
            if (skillConfigLoader == null || !skillConfigLoader.TryBuildSkill(skillId, out var record))
            {
                Debug.LogWarning($"[IDRPG3D LocalTest] {debugName} config not found. SkillId={skillId}");
                return;
            }

            for (var i = 0; i < record.Effects.Count; i++)
            {
                IDRPG3DPrototypeEffectRunner.Apply(record.Effects[i], caster, target);
            }

            Debug.Log($"[IDRPG3D LocalTest] Applied configured {debugName} SkillId={skillId} Lv{record.Level}.");
        }

        private IDRPG3DPrototypeSkillDefinition CreateConfiguredOrFallbackSkill(
            int skillId,
            IDRPG3DPrototypeSkillDefinition fallback)
        {
            if (skillConfigLoader != null && skillConfigLoader.TryBuildSkill(skillId, out var record))
            {
                return IDRPG3DPrototypeSkillConfigBuilder.Build(
                    record,
                    LoadEditorPrefab(record.ProjectilePrefabPath),
                    LoadEditorPrefab(record.MuzzlePrefabPath),
                    LoadEditorPrefab(record.ImpactPrefabPath));
            }

            return fallback;
        }

        private static IDRPG3DPrototypeSkillCaster EnsureSkillCaster(GameObject hero)
        {
            var skillCaster = hero.GetComponent<IDRPG3DPrototypeSkillCaster>();
            return skillCaster != null ? skillCaster : hero.AddComponent<IDRPG3DPrototypeSkillCaster>();
        }

        private static void ConfigureEnemyHealthBar(GameObject enemy, IDRPG3DCombatUnit unit)
        {
            var healthBar = enemy.GetComponent<IDRPG3DWorldHealthBar>();
            if (healthBar == null)
            {
                healthBar = enemy.AddComponent<IDRPG3DWorldHealthBar>();
            }

            healthBar.Configure(unit, null, null, 2.6f, 0.36f, 2.25f);
        }

        private static void ConfigureEnemyHealthBar(GameObject enemy, IDRPG3DCombatUnit unit, float visualScale)
        {
            var healthBar = enemy.GetComponent<IDRPG3DWorldHealthBar>();
            if (healthBar == null)
            {
                healthBar = enemy.AddComponent<IDRPG3DWorldHealthBar>();
            }

            var scale = Mathf.Max(1f, visualScale);
            healthBar.Configure(unit, null, null, 2.6f * scale, 0.36f * Mathf.Min(1.5f, scale), 2.25f * scale);
        }

        private static GameObject LoadEditorPrefab(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
#else
            return null;
#endif
        }

        private static void TryConfigurePrototypeAnimationClips(IDRPG3DAnimatorBridge animatorBridge)
        {
#if UNITY_EDITOR
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType("IDRPG3D.EditorTools.IDRPG3DPrototypeAnimationClipLibrary");
                var method = type?.GetMethod("Configure", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { animatorBridge });
                    return;
                }
            }
#endif
        }

        private static void ConfigureGrounding(GameObject target)
        {
            var animator = target.GetComponentInChildren<Animator>();
            if (animator == null || animator.transform == target.transform)
            {
                return;
            }

            var grounder = target.GetComponent<IDRPG3DTerrainVisualGrounder>();
            if (grounder == null)
            {
                grounder = target.AddComponent<IDRPG3DTerrainVisualGrounder>();
            }

            grounder.Configure(animator.transform, ResolveNavMeshLayerMask(), 0f);
        }

        private static void EnsureCollider(GameObject target)
        {
            if (target.GetComponent<Collider>() != null)
            {
                return;
            }

            var capsule = target.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.height = 1.8f;
            capsule.radius = 0.35f;
        }

        private void BuildSceneVisuals()
        {
            var cameraObject = GameObject.Find("Main Camera");
            if (cameraObject == null)
            {
                cameraObject = new GameObject("Main Camera");
                cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 4.8f, -8.5f), Quaternion.Euler(28f, 0f, 0f));
            cameraObject.GetComponent<Camera>().fieldOfView = 45f;
            cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;

            if (GameObject.Find("Directional Light") == null)
            {
                var lightObject = new GameObject("Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            }

            if (GameObject.Find("Arena_Floor") == null)
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Arena_Floor";
                SetGameObjectLayer(floor, TerrainLayerName);
                floor.transform.localScale = new Vector3(5f, 1f, 5f);
            }

            if (GameObject.Find(DefaultHeroName) == null && GameObject.Find("Hero_DebugCapsule") == null)
            {
                var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                hero.name = "Hero_DebugCapsule";
                hero.transform.position = new Vector3(-1.4f, 1f, 0f);
            }

            if (GameObject.Find(DefaultEnemyName) == null && GameObject.Find("Monster_DebugCapsule") == null)
            {
                var monster = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                monster.name = DefaultEnemyName;
                monster.transform.position = new Vector3(1.4f, 1f, 0f);
            }
        }

        private static void SetGameObjectLayer(GameObject target, string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                target.layer = layer;
            }
        }

        private void BuildUI()
        {
            EnsureEventSystem();

            var canvas = CreateCanvas();
            ClearChildren(canvas.transform);

            var panel = CreatePanel(canvas.transform);

            CreateText(panel.transform, "IDRPG3D Local Flow", 22, TextAnchor.MiddleLeft, new Vector2(20f, -20f), new Vector2(520f, 34f));
            endpointText = CreateText(panel.transform, $"GameServer: {ServerHost}:{GameServerPort} {GameServerProtocol}    MongoDB: {ServerHost}:{MongoPort}", 14, TextAnchor.MiddleLeft, new Vector2(20f, -58f), new Vector2(560f, 26f));

            CreateText(panel.transform, "Account", 14, TextAnchor.MiddleLeft, new Vector2(20f, -98f), new Vector2(90f, 26f));
            accountInput = CreateInput(panel.transform, currentAccount, new Vector2(112f, -96f), new Vector2(238f, 30f));

            startServerButton = CreateButton(panel.transform, "Start Server", new Vector2(366f, -96f), new Vector2(120f, 30f), StartServer);
            checkPortsButton = CreateButton(panel.transform, "Check Ports", new Vector2(494f, -96f), new Vector2(120f, 30f), () => StartCoroutine(CheckPortsRoutine()));

            loginButton = CreateButton(panel.transform, "Login", new Vector2(20f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Login"));
            enterWorldButton = CreateButton(panel.transform, "Enter World", new Vector2(142f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Enter World"));
            startIdleButton = CreateButton(panel.transform, "Start Idle", new Vector2(264f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Start Idle"));
            stopIdleButton = CreateButton(panel.transform, "Stop Idle", new Vector2(386f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Stop Idle"));
            createTeamButton = CreateButton(panel.transform, "Create Team", new Vector2(508f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Create Team"));

            statusText = CreateText(panel.transform, string.Empty, 14, TextAnchor.UpperLeft, new Vector2(20f, -194f), new Vector2(590f, 210f));
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private Canvas CreateCanvas()
        {
            var canvasObject = GameObject.Find("LocalTestCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("LocalTestCanvas");
            }

            var canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private GameObject CreatePanel(Transform parent)
        {
            var panel = new GameObject("LocalTestPanel");
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(28f, -28f);
            rect.sizeDelta = new Vector2(640f, 430f);

            var image = panel.AddComponent<Image>();
            image.color = new Color(0.07f, 0.08f, 0.10f, 0.92f);
            return panel;
        }

        private Text CreateText(Transform parent, string text, int fontSize, TextAnchor alignment, Vector2 position, Vector2 size)
        {
            var textObject = new GameObject(text.Replace(" ", "_"));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var label = textObject.AddComponent<Text>();
            label.text = text;
            label.font = GetDefaultFont();
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private Font GetDefaultFont()
        {
            if (defaultFont != null)
            {
                return defaultFont;
            }

            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return defaultFont;
        }

        private InputField CreateInput(Transform parent, string text, Vector2 position, Vector2 size)
        {
            var inputObject = new GameObject("AccountInput");
            inputObject.transform.SetParent(parent, false);

            var rect = inputObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = inputObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.92f);

            var input = inputObject.AddComponent<InputField>();
            var textComponent = CreateText(inputObject.transform, text, 14, TextAnchor.MiddleLeft, new Vector2(8f, -2f), new Vector2(size.x - 16f, size.y - 4f));
            textComponent.color = new Color(0.08f, 0.09f, 0.11f, 1f);
            input.textComponent = textComponent;
            input.text = text;
            input.onEndEdit.AddListener(value => currentAccount = string.IsNullOrWhiteSpace(value) ? currentAccount : value.Trim());
            return input;
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label.Replace(" ", "_") + "Button");
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.33f, 0.58f, 1f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            var text = CreateText(buttonObject.transform, label, 14, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = Color.white;
            return button;
        }

        private void StartServer()
        {
            AppendStatus("Start server requested. Use PowerShell command from README for now.");
            AppendStatus("dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- -m Develop -g 1");
        }

        private void PlaceholderNetworkAction(string actionName)
        {
            currentAccount = string.IsNullOrWhiteSpace(accountInput.text) ? currentAccount : accountInput.text.Trim();
            AppendStatus($"{actionName}: UI is ready. Fantasy.Unity RPC binding is the next step. Account={currentAccount}");
        }

        private IEnumerator CheckPortsRoutine()
        {
            SetButtonsInteractable(false);
            yield return null;

            var mongoOpen = IsTcpOpen(ServerHost, MongoPort, 250);

            endpointText.text = $"GameServer: {ServerHost}:{GameServerPort} {GameServerProtocol}    MongoDB: {ServerHost}:{MongoPort} {(mongoOpen ? "OPEN" : "CLOSED")}";
            AppendStatus($"Local check -> GameServer:{GameServerProtocol} verify by Fantasy connect, MongoDB:{(mongoOpen ? "open" : "closed")}");
            SetButtonsInteractable(true);
        }

        private static bool IsTcpOpen(string host, int port, int timeoutMs)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMs));
                    if (!connected)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (startServerButton != null) startServerButton.interactable = interactable;
            if (checkPortsButton != null) checkPortsButton.interactable = interactable;
            if (loginButton != null) loginButton.interactable = interactable;
            if (enterWorldButton != null) enterWorldButton.interactable = interactable;
            if (startIdleButton != null) startIdleButton.interactable = interactable;
            if (stopIdleButton != null) stopIdleButton.interactable = interactable;
            if (createTeamButton != null) createTeamButton.interactable = interactable;
        }

        private void AppendStatus(string line)
        {
            Debug.Log($"[IDRPG3D LocalTest] {line}");
            if (statusText == null)
            {
                return;
            }

            var prefix = DateTime.Now.ToString("HH:mm:ss");
            statusText.text = $"{prefix}  {line}\n{statusText.text}";
        }
    }
}
