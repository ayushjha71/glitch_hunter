using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler.Enemy
{
    public class EnemyCleanup : MonoBehaviour
    {
        private EnemySpawnManager spawner;

        public void Initialize(EnemySpawnManager enemySpawner)
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