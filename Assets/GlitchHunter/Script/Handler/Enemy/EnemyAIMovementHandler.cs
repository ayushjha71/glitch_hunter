using GlitchHunter.Manager;
using GlitchHunter.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace GlitchHunter.Handler.Enemy
{
    public class EnemyAIMovementHandler : MonoBehaviour
    {
        [SerializeField]
        private NavMeshAgent agent;
        [SerializeField]
        private Animator animator;

        private State currentState;

        void Start()
        {
            currentState = new IdleState(this.gameObject, agent, animator, GameManager.Instance.PlayerPrefab.transform);
        }

        // Update is called once per frame
        void Update()
        {
            currentState = currentState.Process();
        }
    }
}
