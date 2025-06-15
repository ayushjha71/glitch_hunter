using GlitchHunter.Enum;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GlitchHunter.Handler.Enemy
{
    public class EnemyHealthHandler : MonoBehaviour
    {
        [SerializeField] private EnemyType type;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private NavMeshAgent currentAgent;
        [SerializeField] private ParticleSystem deadEffect;
        [SerializeField] private AudioClip deadAudioClip;
        [SerializeField] private AudioSource audioSource;


        private float mCurrentHealth;
        private bool isDead = false;

        private void Start()
        {
            mCurrentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            mCurrentHealth -= damage;
            Debug.Log($"NPC took {damage} damage. Current health: {mCurrentHealth}");

            if (mCurrentHealth <= 0)
            {
                Die();
            }
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
            DeadEffect();
        }

        private void DeadEffectBaseOnEnemyType(EnemyType type)
        {
            switch(type)
            {
                case EnemyType.SHOOTING:
                    {
                       
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
            if(deadEffect != null)
            {
                Instantiate(deadEffect, transform.position, transform.rotation);
            }
            yield return new WaitForSeconds(5);
            DeadEffectBaseOnEnemyType(type);
            Destroy(deadEffect);
        }

        public bool IsDead()
        {
            return isDead;
        }
    }
}