using GlitchHunter.Constant;
using UnityEngine;
using System.Collections.Generic;
using GlitchHunter.Handler.Enemy;

namespace GlitchHunter.Manager
{
    [System.Serializable]
    public class ChildrenData
    {
        public GameObject Children;
        public GameObject[] guardEnemy;
        public int EnemyCount;
        public float spawnRadius;
        [HideInInspector] public int spawnedCount = 0;
        [HideInInspector] public int killedCount = 0;
    }

    public class ChildrenSpawner : MonoBehaviour
    {
        [SerializeField]
        private ChildrenData childrenData;
        [SerializeField]
        private Transform[] childrenSpawnPoint;
        [SerializeField]
        private LayerMask playerLayer;
        [SerializeField]
        private float playerCheckRadius = 5f;

        private GameObject currentChildren;
        private List<GameObject> activeEnemies = new List<GameObject>();

        private void OnEnable()
        {
            GlitchHunterConstant.OnSpawnedChildren += OnSpawnedChildren;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnSpawnedChildren -= OnSpawnedChildren;
        }

        private void OnSpawnedChildren()
        {
            SpawnChildren();
            SpawnInitialEnemies();
        }

        private void SpawnChildren()
        {
            // Find a spawn point where player isn't nearby
            Transform safeSpawnPoint = GetSafeSpawnPoint();
            if (safeSpawnPoint == null)
            {
                Debug.LogWarning("No safe spawn point found for children!");
                return;
            }

            // Spawn the children
            currentChildren = Instantiate(childrenData.Children, safeSpawnPoint.position, safeSpawnPoint.rotation);
        }

        private Transform GetSafeSpawnPoint()
        {
            List<Transform> safePoints = new List<Transform>();

            foreach (Transform point in childrenSpawnPoint)
            {
                if (!Physics.CheckSphere(point.position, playerCheckRadius, playerLayer))
                {
                    safePoints.Add(point);
                }
            }

            return safePoints.Count > 0 ? safePoints[Random.Range(0, safePoints.Count)] : null;
        }

        private void SpawnInitialEnemies()
        {
            // Reset counts
            childrenData.spawnedCount = 0;
            childrenData.killedCount = 0;

            // Spawn half of the enemies initially
            int initialSpawnCount = Mathf.CeilToInt(childrenData.EnemyCount / 2f);

            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            if (childrenData.spawnedCount >= childrenData.EnemyCount || currentChildren == null)
                return;

            // Get random enemy type from guardEnemy array
            GameObject enemyPrefab = childrenData.guardEnemy[Random.Range(0, childrenData.guardEnemy.Length)];

            // Calculate spawn position around children within spawnRadius
            Vector2 randomCircle = Random.insideUnitCircle * childrenData.spawnRadius;
            Vector3 spawnPosition = currentChildren.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Spawn the enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(enemy);
            childrenData.spawnedCount++;

            // Setup enemy components
            //EnemyHealthHandler health = enemy.GetComponent<EnemyHealthHandler>();
            //if (health != null)
            //{
            //    health.Initialize(this);
            //}
            //else
            //{
            //    Debug.LogWarning("Guard enemy prefab missing EnemyHealthHandler component: " + enemyPrefab.name);
            //}
        }

        public void OnEnemyKilled(GameObject enemy)
        {
            childrenData.killedCount++;
            activeEnemies.Remove(enemy);

            // Spawn a new enemy if we haven't reached total count
            if (childrenData.spawnedCount < childrenData.EnemyCount)
            {
                SpawnEnemy();
            }

            // Destroy the killed enemy
            Destroy(enemy);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in childrenSpawnPoint)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, playerCheckRadius);
            }

            if (currentChildren != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentChildren.transform.position, childrenData.spawnRadius);
            }
        }
    }
}