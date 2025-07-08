using GlitchHunter.Interface;
using GlitchHunter.Constant;
using GlitchHunter.Manager;
using System.Collections;
using GlitchHunter.Enum;
using UnityEngine.AI;
using UnityEngine;

namespace GlitchHunter.Handler.Enemy
{
    public class EnemyHealthHandler : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyType type;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private NavMeshAgent currentAgent;
        [SerializeField] private ParticleSystem deadEffect;
        [SerializeField] private AudioClip deadAudioClip;
        [SerializeField] private AudioSource audioSource;

        public EnemyTypeData enemyType { get; private set; }
        private EnemySpawnManager spawnManager;

        private float mCurrentHealth;
        private bool isDead = false;

        public float CurrentHealth => mCurrentHealth;
        public float MaxHealth => maxHealth;

        private void Start()
        {
            mCurrentHealth = maxHealth;
        }

        public void Initialize(EnemySpawnManager manager)
        {
            spawnManager = manager;
            // You'll need to set enemyType reference here based on your setup
        }

        private void Die()
        {
            audioSource.clip = deadAudioClip;
            audioSource.Play();
            isDead = true;
            if(currentAgent != null)
            {
                currentAgent.enabled = false;
            }
            StartCoroutine(DeadEffect());
        }

        private void DeadEffectBaseOnEnemyType(EnemyType type)
        {
            switch(type)
            {
                case EnemyType.SHOOTING:
                    {
                        GlitchHunterConstant.OnSpawnCollectable?.Invoke(transform, false);
                    }
                    break;
                case EnemyType.Guard:
                    {
                        Destroy(this.gameObject);
                    }
                    break;
            }
        }

        private IEnumerator DeadEffect()
        {
            spawnManager.OnEnemyDeath(enemyType, gameObject);
            if (deadEffect != null)
            {
                Instantiate(deadEffect, transform.position, transform.rotation);
            }
            yield return new WaitForSeconds(3);
            Destroy(deadEffect);
            DeadEffectBaseOnEnemyType(type);
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void Damage(float damage)
        {
            if (isDead) return;

            mCurrentHealth -= damage;
            Debug.Log($"NPC took {damage} damage. Current health: {mCurrentHealth}");

            if (mCurrentHealth <= 1)
            {
                Die();
            }
        }
    }
}