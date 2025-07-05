using GlitchHunter.Interface;
using GlitchHunter.Constant;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class CrateHandler : MonoBehaviour, IDamageable
    {
        [SerializeField]
        private float crateMaxHealth = 100;
        [SerializeField]
        private GameObject crateMesh;
        [SerializeField]
        private ParticleSystem hitParticle;
        [SerializeField]
        private ParticleSystem destroyParticle;

        private float mCurrenCratetHealth;
        private bool isDead = false;

        private void Start()
        {
            mCurrenCratetHealth = crateMaxHealth;
        }

        public void Damage(float damage)
        {
            if (isDead) return;

            mCurrenCratetHealth -= damage;
            if (mCurrenCratetHealth <= 1)
            {
                Die();
            }
        }

        private void SpawnParticle(ParticleSystem particle)
        {
            Instantiate(particle, transform.position, transform.rotation);
            particle.transform.SetParent(transform);
            particle.Play();
            Destroy(particle, 2f);
        }

        private void Die()
        {
            SpawnParticle(destroyParticle);
            Destroy(crateMesh);
            isDead = true;
            GlitchHunterConstant.OnSpawnCollectable?.Invoke(this.transform, true);
        }
    }
}
