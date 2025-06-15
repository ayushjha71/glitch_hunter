using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GlitchHunter.Handler;
using GlitchHunter.Enum;
using GlitchHunter.Handler.Enemy;

namespace GlitchHunter.Manager
{
    [System.Serializable]
    public class EnemyData
    {
        public EnemyType EnemyType;
        public GameObject EnemyPrefab;
        public int EnemyCount;                // Number of enemies to spawn
        public float SpawnInterval;      // Time between spawns for this wave
        public float StartTime;
        [Header("Spawn Points")]
        public Transform[] spawnPoints;
    }

    [System.Serializable]
    public class WaveData
    {
        public float startTime;
        public float duration;
        public float keySpawnTime;
        public List<EnemyData> enemies;
    }

    public class EnemySpawnHandler : MonoBehaviour
    {
        [Header("Key Settings")]
        public GameObject keyPrefab;
        public GameObject keyGuardEnemyPrefab; // Enemy that spawns near key

        [Header("Wave Configuration")]
        public List<EnemyData> enemyData = new List<EnemyData>();

        // Wave configuration
        private List<WaveData> waves = new List<WaveData>();

        // Runtime variables
        private int currentWaveIndex = 0;
        private bool[] waveCompleted;
        private bool[] keyCollected;
        private Coroutine[] waveCoroutines;

        // Spawned objects tracking
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private GameObject currentKey;

        void Start()
        {
            InitializeWaves();
            waveCompleted = new bool[waves.Count];
            keyCollected = new bool[waves.Count];
            waveCoroutines = new Coroutine[waves.Count];
        }

        void InitializeWaves()
        {
            // Wave 1: 30s - 90s (1.5 min duration)
            WaveData wave1 = new WaveData
            {
                startTime = 30f,
                duration = 60f, // 1.5 min duration
                keySpawnTime = 60f, // Key spawns at 1 min game time
                enemies = new List<EnemyData>(enemyData)
            };

            // Wave 2: 120s - 180s (1 min duration) 
            WaveData wave2 = new WaveData
            {
                startTime = 120f, // 2 min
                duration = 60f, // 1 min duration  
                keySpawnTime = 150f, // Key spawns at 2.5 min game time
                enemies = new List<EnemyData>(enemyData)
            };

            // Wave 3: 180s - 240s (1 min duration)
            WaveData wave3 = new WaveData
            {
                startTime = 180f, // 3 min
                duration = 60f, // 1 min duration
                keySpawnTime = 210f, // Key spawns at 3.5 min game time  
                enemies = new List<EnemyData>(enemyData)
            };

            waves.Add(wave1);
            waves.Add(wave2);
            waves.Add(wave3);
        }

        void Update()
        {
            if (!GameManager.Instance.IsGameStarted) return;

            // Check for wave starts
            CheckWaveStarts();

            // Check for key spawns
            CheckKeySpawns();
        }

        void CheckWaveStarts()
        {
            for (int i = 0; i < waves.Count; i++)
            {
                WaveData wave = waves[i];

                // Start wave if time reached and not already started
                if (GameManager.Instance.GameTime >= wave.startTime && !waveCompleted[i] && waveCoroutines[i] == null)
                {
                    Debug.Log($"Starting Wave {i + 1} at {GameManager.Instance.GameTime:F1}s");
                    waveCoroutines[i] = StartCoroutine(SpawnWave(i));
                }
            }
        }

        void CheckKeySpawns()
        {
            for (int i = 0; i < waves.Count; i++)
            {
                WaveData wave = waves[i];

                // Spawn key if time reached and not already spawned
                if (GameManager.Instance.GameTime >= wave.keySpawnTime && !keyCollected[i] && currentKey == null)
                {
                    Debug.Log($"Spawning Key {i + 1} at {GameManager.Instance.GameTime:F1}s");
                    SpawnKey(i);
                }
            }
        }

        IEnumerator SpawnWave(int waveIndex)
        {
            WaveData wave = waves[waveIndex];
            float waveEndTime = wave.startTime + wave.duration;

            while (GameManager.Instance.GameTime < waveEndTime && !waveCompleted[waveIndex])
            {
                // Check if key was collected for this wave
                if (keyCollected[waveIndex])
                {
                    Debug.Log($"Wave {waveIndex + 1} stopped - Key collected!");
                    break;
                }

                // Spawn enemies from this wave
                foreach (EnemyData enemyData in wave.enemies)
                {
                    if (enemyData.EnemyPrefab != null)
                    {
                        SpawnEnemy(enemyData);
                        yield return new WaitForSeconds(enemyData.SpawnInterval);
                    }
                }

                yield return new WaitForSeconds(0.1f); // Small delay before next cycle
            }

            waveCompleted[waveIndex] = true;
            Debug.Log($"Wave {waveIndex + 1} completed at {GameManager.Instance.GameTime:F1}s");
        }

        void SpawnEnemy(EnemyData enemyData)
        {
            if (enemyData.spawnPoints == null || enemyData.spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points available for enemy: " + enemyData.EnemyPrefab.name);
                return;
            }

            // Spawn the specified number of enemies at random spawn points
            for (int i = 0; i < enemyData.EnemyCount; i++)
            {
                // Select a random spawn point
                Transform randomSpawnPoint = enemyData.spawnPoints[Random.Range(0, enemyData.spawnPoints.Length)];

                if (randomSpawnPoint != null)
                {
                    GameObject enemy = Instantiate(enemyData.EnemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
                    spawnedEnemies.Add(enemy);

                    // Add enemy cleanup component
                    EnemyCleanup cleanup = enemy.GetComponent<EnemyCleanup>();
                    if (cleanup == null)
                    {
                        cleanup = enemy.AddComponent<EnemyCleanup>();
                    }
                    cleanup.Initialize(this);

                    Debug.Log($"Spawned {enemyData.EnemyPrefab.name} at {randomSpawnPoint.position}");
                }
            }
        }

        void SpawnKey(int waveIndex)
        {
            // Collect all spawn points from all enemy data
            List<Transform> allSpawnPoints = new List<Transform>();
            foreach (EnemyData data in enemyData)
            {
                if (data.spawnPoints != null)
                {
                    allSpawnPoints.AddRange(data.spawnPoints);
                }
            }

            if (allSpawnPoints.Count == 0)
            {
                Debug.LogWarning("No spawn points available for key!");
                return;
            }

            // Random spawn point for key from all available spawn points
            Transform keySpawnPoint = allSpawnPoints[Random.Range(0, allSpawnPoints.Count)];
            currentKey = Instantiate(keyPrefab, keySpawnPoint.position, keySpawnPoint.rotation);

            // Add key collection component
            Key keyCollector = currentKey.GetComponent<Key>();
            if (keyCollector == null)
            {
                keyCollector = currentKey.AddComponent<Key>();
            }
            keyCollector.Initialize(this, waveIndex);

            // Spawn guard enemy near the key
            if (keyGuardEnemyPrefab != null)
            {
                Vector3 guardPosition = keySpawnPoint.position + Random.insideUnitSphere * 3f;
                guardPosition.y = keySpawnPoint.position.y; // Keep same Y level
                GameObject guardEnemy = Instantiate(keyGuardEnemyPrefab, guardPosition, Quaternion.identity);
                spawnedEnemies.Add(guardEnemy);

                // Add cleanup component to guard
                EnemyCleanup cleanup = guardEnemy.GetComponent<EnemyCleanup>();
                if (cleanup == null)
                {
                    cleanup = guardEnemy.AddComponent<EnemyCleanup>();
                }
                cleanup.Initialize(this);

                Debug.Log($"Guard enemy spawned near Key {waveIndex + 1}");
            }

            Debug.Log($"Key {waveIndex + 1} spawned at position: {keySpawnPoint.position}");
        }

        public void OnKeyCollected(int waveIndex)
        {
            keyCollected[waveIndex] = true;
            currentKey = null;

            Debug.Log($"Key {waveIndex + 1} collected! Stopping wave {waveIndex + 1} spawning.");

            // Stop the wave coroutine
            if (waveCoroutines[waveIndex] != null)
            {
                StopCoroutine(waveCoroutines[waveIndex]);
                waveCoroutines[waveIndex] = null;
            }
        }

        public void OnEnemyDestroyed(GameObject enemy)
        {
            if (spawnedEnemies.Contains(enemy))
            {
                spawnedEnemies.Remove(enemy);
            }
        }

        void EndGame()
        {
            // Stop all wave coroutines
            for (int i = 0; i < waveCoroutines.Length; i++)
            {
                if (waveCoroutines[i] != null)
                {
                    StopCoroutine(waveCoroutines[i]);
                }
            }

            // Clean up remaining enemies
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            spawnedEnemies.Clear();

            // Clean up key if still active
            if (currentKey != null)
            {
                Destroy(currentKey);
            }

            Debug.Log("Game Ended!");
        }

        // Public methods for debugging/testing
        public int GetCurrentWave() => currentWaveIndex;

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"Game Time: {GameManager.Instance.GameTime:F1}s");
            GUI.Label(new Rect(10, 30, 200, 20), $"Enemies Active: {spawnedEnemies.Count}");

            for (int i = 0; i < waves.Count; i++)
            {
                string status = waveCompleted[i] ? "Completed" : (keyCollected[i] ? "Key Collected" : "Active");
                GUI.Label(new Rect(10, 50 + i * 20, 200, 20), $"Wave {i + 1}: {status}");
            }
        }
    }
}