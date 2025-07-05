using GlitchHunter.Constant;
using GlitchHunter.Enum;
using GlitchHunter.Handler.Enemy;
using GlitchHunter.Interface;
using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler.Player
{
    public class SwordHandler : MonoBehaviour
    {
        [SerializeField]
        private float damangeAmount = 50;

        private IDamageable damageable;

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
                damageable = other.gameObject.GetComponent<IDamageable>();

                if(damageable != null)
                {
                    damageable.Damage(damangeAmount);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(damageable != null)
            {
                damageable = null;
            }
        }

        private void MeleeInputHandler()
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                GlitchHunterConstant.OnAttackStateChange?.Invoke(MeleeAttackState.Attack_ONE);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                GlitchHunterConstant.OnAttackStateChange?.Invoke(MeleeAttackState.Attack_TWO);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                GlitchHunterConstant.OnAttackStateChange?.Invoke(MeleeAttackState.Attack_THREE);
            }
        }
    }
}
