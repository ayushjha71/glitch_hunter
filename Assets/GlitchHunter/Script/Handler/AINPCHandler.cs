using GlitchHunter.State;
using UnityEngine;
using UnityEngine.AI;

public class AINPCHandler : MonoBehaviour
{
    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Animator animator;

    State currentState;

    void Start()
    {
        currentState = new IdleState(this.gameObject, agent, animator, playerTransform);
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
    }
}
