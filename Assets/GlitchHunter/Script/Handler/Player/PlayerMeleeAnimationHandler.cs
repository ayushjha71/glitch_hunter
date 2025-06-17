using GlitchHunter.Constant;
using GlitchHunter.Manager;
using UnityEngine;


namespace GlitchHunter.Handler.Player
{
    public enum PlayerAttackState
    {
        Default,
        Attack_ONE,
        Attack_TWO,
        Attack_THREE,
        DEAD
    }

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

        private void UpdateAnimatoState(PlayerAttackState state)
        {
            switch (state)
            {
                case PlayerAttackState.Default:
                    {
                        animator.SetTrigger(IsIdle);
                    }
                    break;
                case PlayerAttackState.Attack_ONE:
                    {
                        animator.SetTrigger(IsAttackOne);
                    }
                    break;
                case PlayerAttackState.Attack_TWO:
                    {
                        animator.SetTrigger(IsAttackTwo);
                    }
                    break;
                case PlayerAttackState.Attack_THREE:
                    {
                        animator.SetTrigger(IsAttackThree);
                    }
                    break;
                case PlayerAttackState.DEAD:
                    {
                        animator.SetTrigger(IsDead);
                    }
                    break;
            }
        }
    }
}
