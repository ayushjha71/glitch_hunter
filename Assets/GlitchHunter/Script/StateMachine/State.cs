using GlitchHunter.Constant;
using UnityEngine;
using UnityEngine.AI;

namespace GlitchHunter.State
{
    public class State
    {
        public enum STATE
        {
            IDLE, PATROL, PURSUE, ATTACK, SLEEP
        }

        public enum EVENT
        {
            ENTER, UPDATE, EXIT
        }

        public STATE stateName;
        protected EVENT stage;
        protected GameObject npc;
        protected Animator animator;
        protected Transform playerTransform;
        protected NavMeshAgent navMeshAgent;
        protected State state;

        private float visDist = 10;
        private float visAngle = 30;
        private float shootDist = 7;

        public State(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform)
        {
            npc = gameObject;
            navMeshAgent = agent;
            animator = anim;
            stage = EVENT.ENTER;
            playerTransform = transform;
        }

        public virtual void Enter()
        {
            stage = EVENT.UPDATE;
        }

        public virtual void Update()
        {
            stage = EVENT.UPDATE;
        }
        public virtual void Exit()
        {
            stage = EVENT.EXIT;
        }

        public State Process()
        {
            if (stage == EVENT.ENTER)
            {
                Enter();
            }
            if (stage == EVENT.UPDATE)
            {
                Update();
            }
            if (stage == EVENT.EXIT)
            {
                Exit();
                return state;
            }
            return this;
        }

        public bool CanSeePlayer()
        {
            Vector3 direction = playerTransform.position - npc.transform.position;
            float angle = Vector3.Angle(direction, npc.transform.forward);

            if (direction.magnitude < visDist && angle < visAngle)
            {
                return true;
            }
            return false;
        }

        public bool CanAttackPlayer()
        {
            Vector3 direction = playerTransform.position - npc.transform.position;
            if (direction.magnitude < shootDist)
            {
                return true;
            }
            return false;
        }
    }

    public class IdleState : State
    {
        public IdleState(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform) : base(gameObject, agent, anim, transform)
        {
            stateName = STATE.IDLE;
        }

        public override void Enter()
        {
            animator.SetTrigger("isIdle");
            base.Enter();
        }

        public override void Update()
        {
            if (CanSeePlayer())
            {
                state = new PursueState(npc, navMeshAgent, animator, playerTransform);
                stage = EVENT.EXIT;
            }
            else if (Random.Range(0, 100) < 10)
            {
                state = new PatrolState(npc, navMeshAgent, animator, playerTransform);
                stage = EVENT.EXIT;
            }
        }

        public override void Exit()
        {
            animator.ResetTrigger("isIdle");
            base.Exit();
        }
    }

    public class PatrolState : State
    {
        private int currentIndex = -1;
        public PatrolState(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform) : base(gameObject, agent, anim, transform)
        {
            stateName = STATE.PATROL;
            navMeshAgent.speed = 2;
            navMeshAgent.isStopped = false;
        }

        public override void Enter()
        {
            float lastDistance = Mathf.Infinity;
            for(int i = 0;  i< GameEnvironment.Singleton.Checkpoints.Count; i++)
            {
                GameObject thisCheckPoints = GameEnvironment.Singleton.Checkpoints[i];
                float distance = Vector3.Distance(npc.transform.position, thisCheckPoints.transform.position);
                if(distance < lastDistance)
                {
                    currentIndex = i - 1;
                    lastDistance = distance;
                }
            }
            animator.SetTrigger("isWalking");
            base.Enter();
        }

        public override void Update()
        {
            if (navMeshAgent.remainingDistance < 1)
            {
                if (currentIndex >= GameEnvironment.Singleton.Checkpoints.Count - 1)
                {
                    currentIndex = 0;
                }
                else
                {
                    currentIndex++;
                }
                navMeshAgent.SetDestination(GameEnvironment.Singleton.Checkpoints[currentIndex].transform.position);
            }

            if (CanSeePlayer())
            {
                state = new AttackState(npc, navMeshAgent, animator, playerTransform);
                stage = EVENT.EXIT;
            }
        }

        public override void Exit()
        {
            animator.ResetTrigger("isWalking");
            base.Exit();
        }
    }

    public class PursueState : State
    {
        public PursueState(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform) : base(gameObject, agent, anim, transform)
        {
            stateName = STATE.PURSUE;
            navMeshAgent.speed = 5;
            navMeshAgent.isStopped = false;
        }

        public override void Enter()
        {
            animator.SetTrigger("isRunning");
            base.Enter();
        }

        public override void Update()
        {
            navMeshAgent.SetDestination(playerTransform.position);
            if (navMeshAgent.hasPath)
            {
                if (CanAttackPlayer())
                {
                    state = new AttackState(npc, navMeshAgent, animator, playerTransform);
                    stage = EVENT.EXIT;
                }
                else if (!CanSeePlayer())
                {
                    state = new PatrolState(npc, navMeshAgent, animator, playerTransform);
                    stage = EVENT.EXIT;
                }
            }
        }

        public override void Exit()
        {
            animator.ResetTrigger("isRunning");
            base.Exit();
        }
    }

    public class AttackState : State
    {
        private float mRotationSpeed = 2;
        private AudioSource mAudioSource;

        public AttackState(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform) : base(gameObject, agent, anim, transform)
        {
            stateName = STATE.ATTACK;
            mAudioSource = gameObject.GetComponent<AudioSource>();
        }

        public override void Enter()
        {
            animator.SetTrigger("isShooting");
            navMeshAgent.isStopped = true;
            mAudioSource.Play();
            base.Enter();
        }

        public override void Update()
        {
            Vector3 direction = playerTransform.position - npc.transform.position;
            float angle = Vector3.Angle(direction, npc.transform.forward);
            direction.y = 0;

            npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * mRotationSpeed);

            if (!CanAttackPlayer())
            {
                state = new IdleState(npc, navMeshAgent, animator, playerTransform);
                stage = EVENT.EXIT;
            }
        }

        public override void Exit()
        {
            animator.ResetTrigger("isShooting");
            mAudioSource.Stop();
            base.Exit();
        }
    }
}
