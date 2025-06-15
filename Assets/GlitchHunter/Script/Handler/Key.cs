using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class Key : MonoBehaviour
    {
        private EnemySpawnManager spawner;
        private int waveIndex;

        public void Initialize(EnemySpawnManager enemySpawner, int wave)
        {
            spawner = enemySpawner;
            waveIndex = wave;
        }

        public void OnCollectKey()
        {
            spawner.OnKeyCollected(waveIndex);
            Destroy(gameObject);
        }
    }
}