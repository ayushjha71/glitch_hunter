using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GlitchHunter.Handler;
using GlitchHunter.Enum;
using GlitchHunter.Handler.Enemy;
using UnityEngine.UI;
using TMPro;

namespace GlitchHunter.Manager
{
    [System.Serializable]
    public class EnemyData
    {
        [Header("Enemy Settings")]
        public EnemyType EnemyType;
        public GameObject[] EnemyPrefabs;      // Changed from single prefab to array
        public int EnemyCount;                // Number of enemies to spawn (per prefab type)
        public float SpawnInterval;           // Time between spawns for this wave

        [Header("Wave Timing")]
        public float WaveStartTime;           // When this wave starts
        public float WaveDuration;            // How long this wave lasts

        [Header("Key Settings")]
        public GameObject KeyPrefab;          // Unique key prefab for this wave
        public GameObject KeyGuardEnemyPrefab; // Enemy that spawns near this wave's key

        [Header("Effects")]
        public GameObject EnemyDestroyParticleEffect; // Particle effect for this wave's enemy destruction

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        [HideInInspector]
        public float actualKeySpawnTime;      // Calculated randomly between wave start and end
    }

    public class EnemySpawnManager : MonoBehaviour
    {
        [Header("General Settings")]
        // Individual wave settings are now in EnemyData

        [Header("UI Settings")]
        public TMP_Text messageText; // UI Text component for messages
        public float messageDuration = 3f; // How long to show the message

        [Header("Wave Configuration")]
        public List<EnemyData> enemyData = new List<EnemyData>();

        // Runtime variables
        private bool[] waveCompleted;
        private bool[] keyCollected;
        private bool[] keySpawned; // Track if key has been spawned for wave
        private Coroutine[] waveCoroutines;

        // Spawned objects tracking
        private List<GameObject>[] waveEnemies; // Track enemies per wave
        private GameObject[] waveKeys; // Track keys per wave
        private Coroutine messageCoroutine;

        void Start()
        {
            InitializeWaves();

            int waveCount = enemyData.Count;
            waveCompleted = new bool[waveCount];
            keyCollected = new bool[waveCount];
            keySpawned = new bool[waveCount];
            waveCoroutines = new Coroutine[waveCount];
            waveEnemies = new List<GameObject>[waveCount];
            waveKeys = new GameObject[waveCount];

            // Initialize enemy lists for each wave
            for (int i = 0; i < waveCount; i++)
            {
                waveEnemies[i] = new List<GameObject>();
            }

            // Hide message text initially
            if (messageText != null)
                messageText.gameObject.SetActive(false);
        }

        void InitializeWaves()
        {
            // Set default values for waves that aren't configured in inspector
            for (int i = 0; i < enemyData.Count; i++)
            {
                EnemyData data = enemyData[i];

                // Set default wave timings if not set in inspector
                if (data.WaveStartTime == 0 && data.WaveDuration == 0)
                {
                    switch (i)
                    {
                        case 0: // Wave 1
                            data.WaveStartTime = 30f;
                            data.WaveDuration = 60f;
                            break;
                        case 1: // Wave 2
                            data.WaveStartTime = 120f;
                            data.WaveDuration = 40f;
                            break;
                        case 2: // Wave 3
                            data.WaveStartTime = 180f;
                            data.WaveDuration = 40f;
                            break;
                    }
                }

                // Calculate random key spawn time between wave start and end
                data.actualKeySpawnTime = Random.Range(data.WaveStartTime, data.WaveStartTime + data.WaveDuration);

                Debug.Log($"Wave {i + 1}: Start={data.WaveStartTime}s, Duration={data.WaveDuration}s, KeySpawn={data.actualKeySpawnTime:F1}s");
            }
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
            for (int i = 0; i < enemyData.Count; i++)
            {
                EnemyData data = enemyData[i];

                // Start wave if time reached and not already started
                if (GameManager.Instance.GameTime >= data.WaveStartTime && !waveCompleted[i] && waveCoroutines[i] == null)
                {
                    Debug.Log($"Starting Wave {i + 1} at {GameManager.Instance.GameTime:F1}s");
                    waveCoroutines[i] = StartCoroutine(SpawnWave(i));
                }
            }
        }

        void CheckKeySpawns()
        {
            for (int i = 0; i < enemyData.Count; i++)
            {
                EnemyData data = enemyData[i];

                // Spawn key if time reached and not already spawned and key not collected
                if (GameManager.Instance.GameTime >= data.actualKeySpawnTime && !keySpawned[i] && !keyCollected[i])
                {
                    Debug.Log($"Spawning Key {i + 1} at {GameManager.Instance.GameTime:F1}s");
                    SpawnKey(i);
                    keySpawned[i] = true;
                }
            }
        }

        IEnumerator SpawnWave(int waveIndex)
        {
            EnemyData data = enemyData[waveIndex];
            float waveEndTime = data.WaveStartTime + data.WaveDuration;

            while (GameManager.Instance.GameTime < waveEndTime && !waveCompleted[waveIndex])
            {
                // Check if key was collected for this wave
                if (keyCollected[waveIndex])
                {
                    Debug.Log($"Wave {waveIndex + 1} stopped - Key collected!");
                    break;
                }

                // Spawn enemies from this wave
                if (data.EnemyPrefabs != null && data.EnemyPrefabs.Length > 0)
                {
                    SpawnEnemies(data, waveIndex);
                    yield return new WaitForSeconds(data.SpawnInterval);
                }

                yield return new WaitForSeconds(0.1f); // Small delay before next cycle
            }

            // Wave completed - destroy enemies if key wasn't collected
            if (!keyCollected[waveIndex])
            {
                Debug.Log($"Wave {waveIndex + 1} completed by time - destroying enemies");
              //  StartCoroutine(DestroyWaveEnemies(waveIndex));
            }

            waveCompleted[waveIndex] = true;
            Debug.Log($"Wave {waveIndex + 1} completed at {GameManager.Instance.GameTime:F1}s");
        }

        void SpawnEnemies(EnemyData enemyData, int waveIndex)
        {
            if (enemyData.spawnPoints == null || enemyData.spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points available for enemies in wave: " + (waveIndex + 1));
                return;
            }

            if (enemyData.EnemyPrefabs == null || enemyData.EnemyPrefabs.Length == 0)
            {
                Debug.LogWarning("No enemy prefabs assigned for wave: " + (waveIndex + 1));
                return;
            }

            // Spawn each enemy type the specified number of times
            foreach (GameObject enemyPrefab in enemyData.EnemyPrefabs)
            {
                if (enemyPrefab == null) continue;

                // Spawn EnemyCount number of this enemy type
                for (int i = 0; i < enemyData.EnemyCount; i++)
                {
                    // Select a random spawn point for each enemy
                    Transform randomSpawnPoint = enemyData.spawnPoints[Random.Range(0, enemyData.spawnPoints.Length)];

                    if (randomSpawnPoint != null)
                    {
                        GameObject enemy = Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
                        waveEnemies[waveIndex].Add(enemy); // Add to specific wave enemy list

                        // Add enemy cleanup component
                        EnemyCleanup cleanup = enemy.GetComponent<EnemyCleanup>();
                        if (cleanup == null)
                        {
                            cleanup = enemy.AddComponent<EnemyCleanup>();
                        }
                        cleanup.Initialize(this);

                        Debug.Log($"Spawned {enemyPrefab.name} for Wave {waveIndex + 1}");
                    }
                }
            }
        }

        void SpawnKey(int waveIndex)
        {
            EnemyData data = enemyData[waveIndex];

            if (data.KeyPrefab == null)
            {
                Debug.LogWarning($"No key prefab assigned for wave {waveIndex + 1}!");
                return;
            }

            if (data.spawnPoints == null || data.spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points available for key!");
                return;
            }

            // Random spawn point for key from this wave's spawn points
            Transform keySpawnPoint = data.spawnPoints[Random.Range(0, data.spawnPoints.Length)];
            waveKeys[waveIndex] = Instantiate(data.KeyPrefab, keySpawnPoint.position, keySpawnPoint.rotation);

            // Add key collection component
            Key keys = waveKeys[waveIndex].GetComponent<Key>();
            if (keys == null)
            {
                keys = waveKeys[waveIndex].AddComponent<Key>();
            }
            keys.Initialize(this, waveIndex);

            // Spawn guard enemy near the key
            if (data.KeyGuardEnemyPrefab != null)
            {
                Vector3 guardPosition = keySpawnPoint.position + Random.insideUnitSphere * 3f;
                guardPosition.y = keySpawnPoint.position.y; // Keep same Y level
                GameObject guardEnemy = Instantiate(data.KeyGuardEnemyPrefab, guardPosition, Quaternion.identity);
                waveEnemies[waveIndex].Add(guardEnemy); // Add guard to wave enemies

                // Add cleanup component to guard
                EnemyCleanup cleanup = guardEnemy.GetComponent<EnemyCleanup>();
                if (cleanup == null)
                {
                    cleanup = guardEnemy.AddComponent<EnemyCleanup>();
                }
                cleanup.Initialize(this);

                Debug.Log($"Guard enemy spawned near Key {waveIndex + 1}");
            }

            // Show portal key message
            ShowMessage($"Portal Key {waveIndex + 1} spawned! Find it to win!");

            Debug.Log($"Key {waveIndex + 1} spawned at position: ");
        }

        public void OnKeyCollected(int waveIndex)
        {
            keyCollected[waveIndex] = true;
            waveKeys[waveIndex] = null; // Clear the specific wave's key reference

            Debug.Log($"Key {waveIndex + 1} collected! Destroying wave {waveIndex + 1} enemies.");

            // Show collection message
            ShowMessage($"Portal Key {waveIndex + 1} collected! Wave cleared!");

            // Stop the wave coroutine
            if (waveCoroutines[waveIndex] != null)
            {
                StopCoroutine(waveCoroutines[waveIndex]);
                waveCoroutines[waveIndex] = null;
            }

            // Destroy all enemies from this specific wave with particle effects
           // StartCoroutine(DestroyWaveEnemies(waveIndex));
        }

        IEnumerator DestroyWaveEnemies(int waveIndex)
        {
            EnemyData data = enemyData[waveIndex];
            List<GameObject> enemiesToDestroy = new List<GameObject>(waveEnemies[waveIndex]);

            foreach (GameObject enemy in enemiesToDestroy)
            {
                if (enemy != null)
                {
                    // Play particle effect at enemy position using this wave's particle effect
                    if (data.EnemyDestroyParticleEffect != null)
                    {
                        GameObject particles = Instantiate(data.EnemyDestroyParticleEffect, enemy.transform.position, enemy.transform.rotation);
                        // Auto-destroy particles after 3 seconds
                        Destroy(particles, 3f);
                    }

                    // Remove from tracking
                    waveEnemies[waveIndex].Remove(enemy);

                    // Destroy the enemy
                    Destroy(enemy);

                    // Small delay between destructions for visual effect
                    yield return new WaitForSeconds(0.1f);
                }
            }

            Debug.Log($"All enemies from Wave {waveIndex + 1} destroyed!");
        }

        void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageText.gameObject.SetActive(true);

                // Stop previous message coroutine if running
                if (messageCoroutine != null)
                {
                    StopCoroutine(messageCoroutine);
                }

                // Start new message display coroutine
                messageCoroutine = StartCoroutine(HideMessageAfterDelay());
            }
        }

        IEnumerator HideMessageAfterDelay()
        {
            yield return new WaitForSeconds(messageDuration);

            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }

            messageCoroutine = null;
        }

        public void OnEnemyDestroyed(GameObject enemy)
        {
            // Remove from all wave enemy lists
            for (int i = 0; i < waveEnemies.Length; i++)
            {
                if (waveEnemies[i].Contains(enemy))
                {
                    waveEnemies[i].Remove(enemy);
                    break;
                }
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

            // Clean up remaining enemies from all waves
            for (int i = 0; i < waveEnemies.Length; i++)
            {
                foreach (GameObject enemy in waveEnemies[i])
                {
                    if (enemy != null)
                    {
                        Destroy(enemy);
                    }
                }
                waveEnemies[i].Clear();
            }

            // Clean up all keys if still active
            for (int i = 0; i < waveKeys.Length; i++)
            {
                if (waveKeys[i] != null)
                {
                    Destroy(waveKeys[i]);
                }
            }

            Debug.Log("Game Ended!");
        }

        // Public methods for debugging/testing
        public int GetActiveEnemiesInWave(int waveIndex)
        {
            return waveIndex < waveEnemies.Length ? waveEnemies[waveIndex].Count : 0;
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"Game Time: {GameManager.Instance.GameTime:F1}s");

            int totalEnemies = 0;
            for (int i = 0; i < waveEnemies.Length; i++)
            {
                totalEnemies += waveEnemies[i].Count;
            }
            GUI.Label(new Rect(10, 30, 200, 20), $"Total Enemies Active: {totalEnemies}");

            for (int i = 0; i < enemyData.Count; i++)
            {
                string status = waveCompleted[i] ? "Completed" : (keyCollected[i] ? "Key Collected" : "Active");
                int waveEnemyCount = i < waveEnemies.Length ? waveEnemies[i].Count : 0;
                GUI.Label(new Rect(10, 50 + i * 20, 300, 20), $"Wave {i + 1}: {status} - Enemies: {waveEnemyCount}");
            }
        }
    }
}