using GlitchHunter.Constant;
using GlitchHunter.Manager;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements

namespace GlitchHunter.Handler
{
    public class PlayerHealthHandler : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 1000;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool isDead = false;

        [Header("Effects")]
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private GameObject deathEffect;

        private FirstPersonController playerController;
        private ShootingHandler weaponsHandler;

        private void Awake()
        {
            currentHealth = maxHealth;
            playerController = GetComponent<FirstPersonController>();
            weaponsHandler = GetComponent<ShootingHandler>();
            GlitchHunterConstant.OnUpdateHealthSlider?.Invoke(currentHealth);
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");
            GlitchHunterConstant.OnUpdateHealthSlider?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            // Play death sound
            if (deathSound != null)
            {
                GameManager.Instance.AudioSource.PlayOneShot(deathSound);
            }
            GlitchHunterConstant.OnGameOver?.Invoke();

            // Instantiate death effect
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }

            // Disable player controls
            if (playerController != null)
            {
                playerController.enabled = false;
                weaponsHandler.enabled = false;
            }

            // You might want to add game over logic here
             GameManager.Instance.EndGame();
        }

        public bool IsDead()
        {
             return isDead;
        }
    }
}