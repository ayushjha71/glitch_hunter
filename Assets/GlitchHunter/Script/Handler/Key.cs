using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class Key : MonoBehaviour
    {
        private AudioSource mAudioSource;
        private EnemySpawnManager spawner;
        private int waveIndex;

        private void Start()
        {
            mAudioSource = GetComponent<AudioSource>(); 
        }

        public void Initialize(EnemySpawnManager enemySpawner, int wave)
        {
            spawner = enemySpawner;
            waveIndex = wave;
        }

        public void OnCollectKey()
        {
            mAudioSource.Play();
            spawner.OnKeyCollected(waveIndex);
            Destroy(gameObject);
        }
    }
}