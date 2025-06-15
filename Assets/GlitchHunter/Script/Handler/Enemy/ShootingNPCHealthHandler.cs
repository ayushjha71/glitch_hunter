using UnityEngine;
using UnityEngine.AI;

namespace GlitchHunter.Handler.Enemy
{
    public class ShootingNPCHealthHandler : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private NavMeshAgent currentAgent;
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
            currentAgent.enabled = false;

        }

        public bool IsDead()
        {
            return isDead;
        }
    }
}