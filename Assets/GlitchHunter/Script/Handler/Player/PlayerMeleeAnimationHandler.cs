using GlitchHunter.Constant;
using GlitchHunter.Manager;
using GlitchHunter.Enum;
using UnityEngine;


namespace GlitchHunter.Handler.Player
{
    public class PlayerMeleeAnimationHandler : MonoBehaviour
    {
        [SerializeField]
        private GameObject sword;
        [SerializeField]
        private Animator animator;

        private bool IsSwordActive = false;

        private int IsIdle = Animator.StringToHash("IsIdleComat");
        private int IsWalk = Animator.StringToHash("IsWalk");
        private int IsAttackOne = Animator.StringToHash("Attack_1");
        private int IsAttackTwo = Animator.StringToHash("Attack_2");
        private int IsAttackThree = Animator.StringToHash("Attack_3");
        private int IsDead = Animator.StringToHash("IsDead");

        private void Start()
        {
            sword.gameObject.SetActive(IsSwordActive);
        }

        private void OnEnable()
        {
            GlitchHunterConstant.OnAttackStateChange += UpdateAnimatoState;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnAttackStateChange += UpdateAnimatoState;
        }

        private void Update()
        {
            if (GameManager.Instance.IsMeleeCombatStarted && !IsSwordActive)
            {
                IsSwordActive = true;
                sword.gameObject.SetActive(true);
            }
        }

        private void UpdateAnimatoState(MeleeAttackState state)
        {
            switch (state)
            {
                case MeleeAttackState.Default:
                    {
                        animator.SetTrigger(IsIdle);
                    }
                    break;
                case MeleeAttackState.IDLE:
                    {
                        animator.SetBool(IsIdle, true);
                        animator.SetBool(IsWalk, false);
                    }
                    break;
                case MeleeAttackState.WALK:
                    {
                        animator.SetBool(IsIdle, false);
                        animator.SetBool(IsWalk, true);
                    }
                    break;
                case MeleeAttackState.Attack_ONE:
                    {
                        animator.SetTrigger(IsAttackOne);
                        animator.SetBool(IsWalk, false);
                    }
                    break;
                case MeleeAttackState.Attack_TWO:
                    {
                        animator.SetTrigger(IsAttackTwo);
                        animator.SetBool(IsWalk, false);
                    }
                    break;
                case MeleeAttackState.Attack_THREE:
                    {
                        animator.SetTrigger(IsAttackThree);
                        animator.SetBool(IsWalk, false);
                    }
                    break;
                case MeleeAttackState.DEAD:
                    {
                        animator.SetTrigger(IsDead);
                        animator.SetBool(IsWalk, false);
                    }
                    break;
            }
        }
    }
}
