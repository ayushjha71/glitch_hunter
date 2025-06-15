using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler.Enemy
{
    public class EnemyCleanup : MonoBehaviour
    {
        private EnemySpawnHandler spawner;

        public void Initialize(EnemySpawnHandler enemySpawner)
        {
            spawner = enemySpawner;
        }

        void OnDestroy()
        {
            if (spawner != null)
            {
                spawner.OnEnemyDestroyed(gameObject);
            }
        }
    }
}