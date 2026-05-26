using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DWaveController : MonoBehaviour
    {
        private readonly List<IDRPG3DWaveDefinition> waves = new List<IDRPG3DWaveDefinition>();
        private readonly List<IDRPG3DCombatUnit> activeEnemies = new List<IDRPG3DCombatUnit>(16);
        private readonly List<IDRPG3DSpawnAnchor> spawnAnchors = new List<IDRPG3DSpawnAnchor>();

        private Func<Vector3> routeAnchorProvider;
        private Func<Vector3> routeForwardProvider;
        private Func<IDRPG3DWaveDefinition, IDRPG3DWaveSpawnPoint, IReadOnlyList<IDRPG3DCombatUnit>> enemySpawner;
        private bool loopStage;
        private bool running;
        private bool waitingForNextWave;
        private int waveCursor = -1;
        private float nextWaveTimer;

        public int CurrentWaveIndex => waveCursor >= 0 && waveCursor < waves.Count ? waves[waveCursor].WaveIndex : 0;
        public int ActiveEnemyCount => activeEnemies.Count;
        public bool WaitingForNextWave => waitingForNextWave;
        public bool IsRunning => running;

        public void Configure(
            IReadOnlyList<IDRPG3DWaveDefinition> stageWaves,
            Func<Vector3> routeAnchor,
            Func<Vector3> routeForward,
            IReadOnlyList<IDRPG3DSpawnAnchor> anchors,
            Func<IDRPG3DWaveDefinition, IDRPG3DWaveSpawnPoint, IReadOnlyList<IDRPG3DCombatUnit>> spawnEnemies,
            bool loopStage)
        {
            waves.Clear();
            if (stageWaves != null)
            {
                for (var i = 0; i < stageWaves.Count; i++)
                {
                    waves.Add(stageWaves[i]);
                }

                waves.Sort((left, right) => left.WaveIndex.CompareTo(right.WaveIndex));
            }

            spawnAnchors.Clear();
            if (anchors != null)
            {
                for (var i = 0; i < anchors.Count; i++)
                {
                    if (anchors[i] != null)
                    {
                        spawnAnchors.Add(anchors[i]);
                    }
                }
            }

            routeAnchorProvider = routeAnchor;
            routeForwardProvider = routeForward;
            enemySpawner = spawnEnemies;
            this.loopStage = loopStage;
            running = false;
            waitingForNextWave = false;
            waveCursor = -1;
            nextWaveTimer = 0f;
            ClearActiveEnemies();
        }

        public void StartStage()
        {
            if (waves.Count == 0 || enemySpawner == null)
            {
                running = false;
                return;
            }

            running = true;
            waitingForNextWave = false;
            waveCursor = 0;
            SpawnCurrentWave();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void TickForTest(float deltaTime)
        {
            Tick(deltaTime);
        }

        private void Tick(float deltaTime)
        {
            if (!running)
            {
                return;
            }

            PruneDeadEnemies();
            if (activeEnemies.Count > 0)
            {
                return;
            }

            if (!waitingForNextWave)
            {
                waitingForNextWave = true;
                nextWaveTimer = waveCursor >= 0 && waveCursor < waves.Count ? waves[waveCursor].NextWaveDelay : 0f;
                return;
            }

            nextWaveTimer -= Mathf.Max(0f, deltaTime);
            if (nextWaveTimer > 0f)
            {
                return;
            }

            AdvanceWave();
        }

        private void AdvanceWave()
        {
            waveCursor++;
            if (waveCursor >= waves.Count)
            {
                if (!loopStage)
                {
                    running = false;
                    waitingForNextWave = false;
                    return;
                }

                waveCursor = 0;
            }

            waitingForNextWave = false;
            SpawnCurrentWave();
        }

        private void SpawnCurrentWave()
        {
            ClearActiveEnemies();
            if (waveCursor < 0 || waveCursor >= waves.Count)
            {
                return;
            }

            var wave = waves[waveCursor];
            var spawnPoint = IDRPG3DWaveSpawnResolver.Resolve(
                wave,
                routeAnchorProvider != null ? routeAnchorProvider() : transform.position,
                routeForwardProvider != null ? routeForwardProvider() : transform.forward,
                spawnAnchors);
            if (!spawnPoint.Found)
            {
                Debug.LogWarning($"[IDRPG3D Wave] Spawn point not found. WaveId={wave.WaveId}, Anchor={wave.SpawnAnchorId}");
                return;
            }

            var spawned = enemySpawner(wave, spawnPoint);
            if (spawned == null)
            {
                return;
            }

            for (var i = 0; i < spawned.Count; i++)
            {
                TrackEnemy(spawned[i]);
            }

            Debug.Log($"[IDRPG3D Wave] Spawned wave {wave.WaveIndex}. Enemies={activeEnemies.Count}, Boss={wave.IsBoss}.");
        }

        private void TrackEnemy(IDRPG3DCombatUnit enemy)
        {
            if (enemy == null || !enemy.IsAlive || activeEnemies.Contains(enemy))
            {
                return;
            }

            activeEnemies.Add(enemy);
            enemy.Died += OnEnemyDied;
        }

        private void OnEnemyDied(IDRPG3DCombatUnit enemy)
        {
            if (enemy != null)
            {
                enemy.Died -= OnEnemyDied;
            }

            activeEnemies.Remove(enemy);
        }

        private void PruneDeadEnemies()
        {
            for (var i = activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = activeEnemies[i];
                if (enemy != null && enemy.IsAlive)
                {
                    continue;
                }

                if (enemy != null)
                {
                    enemy.Died -= OnEnemyDied;
                }

                activeEnemies.RemoveAt(i);
            }
        }

        private void ClearActiveEnemies()
        {
            for (var i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i] != null)
                {
                    activeEnemies[i].Died -= OnEnemyDied;
                }
            }

            activeEnemies.Clear();
        }
    }
}
