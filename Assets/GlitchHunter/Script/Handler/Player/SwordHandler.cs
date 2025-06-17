using GlitchHunter.Constant;
using GlitchHunter.Handler.Enemy;
using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler.Player
{
    public class SwordHandler : MonoBehaviour
    {
        [SerializeField]
        private float damangeAmount = 50;

        private EnemyHealthHandler mEnemyHealth;

        private void Update()
        {
            if (GameManager.Instance.IsMeleeCombatStarted)
            {
                MeleeInputHandler();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Target"))
            {
                mEnemyHealth = other.gameObject.GetComponent<EnemyHealthHandler>();

                if(mEnemyHealth != null)
                {
                    mEnemyHealth.TakeDamage(damangeAmount);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(mEnemyHealth != null)
            {
                mEnemyHealth = null;
            }
        }

        private void MeleeInputHandler()
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                GlitchHunterConstant.OnAttackStateChange?.Invoke(PlayerAttackState.Attack_ONE);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                GlitchHunterConstant.OnAttackStateChange?.Invoke(PlayerAttackState.Attack_TWO);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                GlitchHunterConstant.OnAttackStateChange?.Invoke(PlayerAttackState.Attack_THREE);
            }
        }
    }
}
