//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using GlitchHunter.Handler.Enemy;

//public class EnemySpawner : MonoBehaviour
//{
//    [System.Serializable]
//    public class EnemyType
//    {
//        public GameObject[] prefabVariants; // Different prefabs for this enemy type
//        public int maxCount; // Max enemies of this type to spawn
//        [HideInInspector] public int currentCount; // Currently spawned count
//        [HideInInspector] public int killedCount; // Killed count
//    }

//    [System.Serializable]
//    public class SpawnZone
//    {
//        public string zoneName;
//        public Transform[] spawnPoints;
//        public EnemyType[] enemyTypes; // Enemy types for this zone
//        public Transform keySpawnPoint; // Where key spawns
//        [HideInInspector] public bool keySpawned;
//    }

//    [Header("Configuration")]
//    public SpawnZone[] zones;
//    public float playerCheckRadius = 10f;
//    public LayerMask playerLayer;
//    public GameObject keyPrefab;

//    private int currentEnemyTypeIndex = 0;
//    private bool allEnemiesDefeated = false;

//    private void Start()
//    {
//        InitializeZones();
//        StartCoroutine(SpawnEnemies());
//    }

//    private void InitializeZones()
//    {
//        foreach (SpawnZone zone in zones)
//        {
//            zone.keySpawned = false;
//            foreach (EnemyType type in zone.enemyTypes)
//            {
//                type.currentCount = 0;
//                type.killedCount = 0;
//            }
//        }
//    }

//    private IEnumerator SpawnEnemies()
//    {
//        while (!allEnemiesDefeated)
//        {
//            bool shouldSpawnNextType = true;

//            // Try to spawn current enemy type in all zones
//            foreach (SpawnZone zone in zones)
//            {
//                if (currentEnemyTypeIndex < zone.enemyTypes.Length)
//                {
//                    EnemyType currentType = zone.enemyTypes[currentEnemyTypeIndex];

//                    // Check if we need to spawn more of this type
//                    if (currentType.killedCount + currentType.currentCount < currentType.maxCount)
//                    {
//                        shouldSpawnNextType = false;

//                        // Try to spawn at each point
//                        foreach (Transform spawnPoint in zone.spawnPoints)
//                        {
//                            if (currentType.currentCount >= currentType.maxCount)
//                                break;

//                            if (!IsPlayerNear(spawnPoint.position))
//                            {
//                                SpawnEnemy(currentType, spawnPoint.position);
//                                yield return new WaitForSeconds(0.5f); // Small spawn delay
//                            }
//                        }
//                    }
//                }
//            }

//            // Move to next enemy type if current type is done
//            if (shouldSpawnNextType)
//            {
//                if (currentEnemyTypeIndex < zones[0].enemyTypes.Length - 1)
//                {
//                    currentEnemyTypeIndex++;
//                    yield return new WaitForSeconds(2f); // Delay between types
//                }
//                else // All enemy types done
//                {
//                    allEnemiesDefeated = true;
//                    SpawnKey();
//                }
//            }

//            yield return new WaitForSeconds(1f); // Main loop delay
//        }
//    }

//    private void SpawnEnemy(EnemyType type, Vector3 position)
//    {
//        if (type.prefabVariants.Length == 0) return;

//        GameObject prefab = type.prefabVariants[Random.Range(0, type.prefabVariants.Length)];
//        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

//        // Setup enemy death reporting
//        EnemyHealthHandler health = enemy.GetComponent<EnemyHealthHandler>();
//        if (health == null) health = enemy.AddComponent<EnemyHealthHandler>();
//       // health.OnDeath += () => OnEnemyDeath(type);

//        type.currentCount++;
//    }

//    private void SpawnKey()
//    {
//        foreach (SpawnZone zone in zones)
//        {
//            if (!zone.keySpawned && zone.keySpawnPoint != null)
//            {
//                Instantiate(keyPrefab, zone.keySpawnPoint.position, Quaternion.identity);
//                zone.keySpawned = true;
//            }
//        }
//    }

//    private void OnEnemyDeath(EnemyType type)
//    {
//        type.currentCount--;
//        type.killedCount++;
//    }

//    private bool IsPlayerNear(Vector3 position)
//    {
//        return Physics.CheckSphere(position, playerCheckRadius, playerLayer);
//    }

//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.yellow;
//        foreach (SpawnZone zone in zones)
//        {
//            if (zone.spawnPoints != null)
//            {
//                foreach (Transform point in zone.spawnPoints)
//                {
//                    if (point != null) Gizmos.DrawWireSphere(point.position, playerCheckRadius);
//                }
//            }
//        }
//    }
//}