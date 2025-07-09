using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using GlitchHunter.Constant;
using GlitchHunter.Handler.Enemy;

namespace GlitchHunter.Manager
{
    [System.Serializable]
    public class EnemyTypeData
    {
        public GameObject enemyPrefab;
        public int totalCount;                // Total enemies of this type to spawn
        public float spawnInterval = 1f;      // Time between spawns for this type
        [HideInInspector] public int spawnedCount; // How many have been spawned
        [HideInInspector] public int killedCount;  // How many have been killed
        [HideInInspector] public bool halfKilledEventTriggered = false;
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName;
        public EnemyTypeData[] enemyTypes;     // Different enemy types in this wave
        public Transform[] spawnPoints;
        public GameObject enemyDestroyEffect;
    }

    public class EnemySpawnManager : MonoBehaviour
    {
        [Header("Wave Configuration")]
        public WaveData[] waves;

        [Header("Spawn Settings")]
        public float playerCheckRadius = 10f;
        public LayerMask playerLayer;

        [Header("UI Settings")]
        public TMP_Text messageText;
        public float messageDuration = 3f;

        private int currentWaveIndex = 0;
        private int currentEnemyTypeIndex = 0;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine messageCoroutine;
        private bool canActivateFlying = false;

        void Start()
        {
            InitializeWave(waves[currentWaveIndex]);
            StartCoroutine(SpawnRoutine());
        }

        void InitializeWave(WaveData wave)
        {
            foreach (var enemyType in wave.enemyTypes)
            {
                enemyType.spawnedCount = 0;
                enemyType.killedCount = 0;
            }
        }

        IEnumerator SpawnRoutine()
        {
            WaveData currentWave = waves[currentWaveIndex];

            // Spawn initial half of each enemy type
            foreach (var enemyType in currentWave.enemyTypes)
            {
                int initialSpawnCount = Mathf.CeilToInt(enemyType.totalCount / 2f);

                for (int i = 0; i < initialSpawnCount; i++)
                {
                    SpawnEnemy(currentWave, enemyType);
                    yield return new WaitForSeconds(enemyType.spawnInterval);
                }
            }

            // Continue spawning based on kills
            while (currentEnemyTypeIndex < currentWave.enemyTypes.Length)
            {
                EnemyTypeData currentType = currentWave.enemyTypes[currentEnemyTypeIndex];

                // Check if we need to spawn more of this type
                if (currentType.spawnedCount < currentType.totalCount)
                {
                    // Spawn one enemy if we have kills available
                    if (currentType.killedCount > 0)
                    {
                        currentType.killedCount--;
                        SpawnEnemy(currentWave, currentType);
                        yield return new WaitForSeconds(currentType.spawnInterval);
                    }
                }
                else
                {
                    // Wait until all enemies of this type are killed
                    if (GetActiveEnemiesOfType(currentType) > 0)
                    {
                        yield return new WaitForSeconds(1f);
                    }
                    else
                    {
                        // Move to next enemy type
                        currentEnemyTypeIndex++;
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return null;
            }

            // Wave completed
            OnWaveCompleted();
        }

        void SpawnEnemy(WaveData wave, EnemyTypeData enemyType)
        {
            if (enemyType.spawnedCount >= enemyType.totalCount) return;

            Transform spawnPoint = GetValidSpawnPoint(wave.spawnPoints);
            if (spawnPoint == null) return;

            GameObject enemy = Instantiate(enemyType.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            activeEnemies.Add(enemy);
            enemyType.spawnedCount++;

            // Setup enemy components
            EnemyHealthHandler health = enemy.GetComponent<EnemyHealthHandler>();
            if (health != null)
            {
                health.Initialize(this);
            }
            else
            {
                Debug.LogWarning("Enemy prefab missing EnemyHealthHandler component: " + enemyType.enemyPrefab.name);
            }
        }

        Transform GetValidSpawnPoint(Transform[] spawnPoints)
        {
            List<Transform> validPoints = new List<Transform>();

            foreach (Transform point in spawnPoints)
            {
                if (!IsPlayerNear(point.position))
                {
                    validPoints.Add(point);
                }
            }

            return validPoints.Count > 0 ? validPoints[Random.Range(0, validPoints.Count)] : null;
        }

        bool IsPlayerNear(Vector3 position)
        {
            return Physics.CheckSphere(position, playerCheckRadius, playerLayer);
        }

        public void OnEnemyDeath(EnemyTypeData enemyType, GameObject enemy)
        {
            enemyType.killedCount++;
            activeEnemies.Remove(enemy);

            // Check if half of this enemy type has been killed
            if (!enemyType.halfKilledEventTriggered && enemyType.killedCount >= Mathf.CeilToInt(enemyType.totalCount / 2f))
            {
                enemyType.halfKilledEventTriggered = true;
                //Spawn Children prefab 
                GlitchHunterConstant.OnSpawnedChildren?.Invoke();
            }

            // Play death effect
            if (waves[currentWaveIndex].enemyDestroyEffect != null)
            {
                Instantiate(waves[currentWaveIndex].enemyDestroyEffect, enemy.transform.position, Quaternion.identity);
            }

            Destroy(enemy);
        }

        int GetActiveEnemiesOfType(EnemyTypeData type)
        {
            int count = 0;
            foreach (var enemy in activeEnemies)
            {
                if (enemy.GetComponent<EnemyHealthHandler>().enemyType == type)
                {
                    count++;
                }
            }
            return count;
        }

        void OnWaveCompleted()
        {
            ShowMessage($"Wave {currentWaveIndex + 1} Completed!", messageDuration);

            // Activate flying if first wave completed
            if (!canActivateFlying && currentWaveIndex == 0)
            {
                canActivateFlying = true;
                ShowMessage("Flight System Unlocked! Hold Space to Fly", 6f);
                GlitchHunterConstant.OnActivateFlying?.Invoke();
            }

            // Move to next wave if available
            if (currentWaveIndex < waves.Length - 1)
            {
                currentWaveIndex++;
                currentEnemyTypeIndex = 0;
                InitializeWave(waves[currentWaveIndex]);
                StartCoroutine(SpawnRoutine());
            }
        }

        void ShowMessage(string message, float duration)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageText.gameObject.SetActive(true);

                if (messageCoroutine != null)
                    StopCoroutine(messageCoroutine);

                messageCoroutine = StartCoroutine(HideMessageAfterDelay(duration));
            }
        }

        IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (messageText != null)
                messageText.gameObject.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            foreach (var wave in waves)
            {
                if (wave.spawnPoints != null)
                {
                    foreach (Transform point in wave.spawnPoints)
                    {
                        if (point != null)
                            Gizmos.DrawWireSphere(point.position, playerCheckRadius);
                    }
                }
            }
        }
    }
}