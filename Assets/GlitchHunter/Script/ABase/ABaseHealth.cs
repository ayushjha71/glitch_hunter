using System.Collections;
using UnityEngine;

namespace GlitchHunter.ABase
{
    public class ABaseHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;

        [Header("References")]
        [SerializeField] private Animator animator;

        public int CurrentHealth
        {
            get
            {
                return currentHealth;
            }
            set
            {
                currentHealth = value;
            }
        }

        public Animator CurrentAnimator => animator;


        private void Start()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            animator.SetTrigger("isSleeping");
            StartCoroutine(HandleDeath());
        }

        private IEnumerator HandleDeath()
        {
            yield return new WaitForSeconds(10f);
            Destroy(this.gameObject);

        }
    }
}
