using GlitchHunter.Constant;
using GlitchHunter.Handler;
using GlitchHunter.Handler.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace GlitchHunter.StateMachine
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

        private float visDist = 20;
        private float visAngle = 360;
        public float shootDist = 10;

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

        // Modify the Process method to handle death
        public State Process()
        {
            if (IsDead() && !(this is DeadState) && !(this is DeathTransitionState))
            {
                return new DeathTransitionState(npc, navMeshAgent, animator, playerTransform);
            }

            if (stage == EVENT.ENTER) Enter();
            if (stage == EVENT.UPDATE) Update();
            if (stage == EVENT.EXIT)
            {
                Exit();
                return state;
            }
            return this;
        }

        public bool IsDead()
        {
            EnemyHealthHandler npcHealth = npc.GetComponent<EnemyHealthHandler>();
            return npcHealth != null && npcHealth.IsDead();
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
            for (int i = 0; i < GameEnvironment.Singleton.Checkpoints.Count; i++)
            {
                GameObject thisCheckPoints = GameEnvironment.Singleton.Checkpoints[i];
                float distance = Vector3.Distance(npc.transform.position, thisCheckPoints.transform.position);
                if (distance < lastDistance)
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
        private float mLastShotTime;
        private float mShootRate = 0.5f; // Time between shots
        private float mDamagePerShot = 2f;
        private LayerMask mShootableMask;

        public AttackState(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform)
            : base(gameObject, agent, anim, transform)
        {
            stateName = STATE.ATTACK;
            mAudioSource = gameObject.GetComponent<AudioSource>();
            mShootableMask = LayerMask.GetMask("Player"); // Make sure your player is on the "Player" layer
        }

        public override void Enter()
        {
            animator.SetTrigger("isShooting");
            navMeshAgent.isStopped = true;
          //  mAudioSource.Play();
            mLastShotTime = Time.time;
            base.Enter();
        }

        public override void Update()
        {
            // Rotate towards player
            Vector3 direction = playerTransform.position - npc.transform.position;
            float angle = Vector3.Angle(direction, npc.transform.forward);
            direction.y = 0;
            npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * mRotationSpeed);

            // Shooting logic
            if (Time.time - mLastShotTime > mShootRate)
            {
                ShootAtPlayer(direction);
                mLastShotTime = Time.time;
            }

            // Transition conditions
            if (!CanAttackPlayer())
            {
                state = new IdleState(npc, navMeshAgent, animator, playerTransform);
                stage = EVENT.EXIT;
            }
        }

        private void ShootAtPlayer(Vector3 directionToPlayer)
        {
            // Play shooting sound
            mAudioSource.Play();
            // Raycast towards player
            if (Physics.Raycast(npc.transform.position, directionToPlayer, out RaycastHit hit, shootDist, mShootableMask))
            {
                // Check if we hit the player
                Debug.Log(hit.transform.gameObject.name);
                var playerHealth = hit.transform.GetComponent<PlayerHealthHandler>();
                if (playerHealth != null)
                {
                    GlitchHunterConstant.OnPlayerHitImpact?.Invoke();
                    if (playerHealth.IsDead())
                    {
                        state = new PatrolState(npc, navMeshAgent, animator, playerTransform);
                    }
                    playerHealth.TakeDamage(mDamagePerShot);
                  //  Debug.Log($"NPC hit player for {mDamagePerShot} damage");
                }
            }
        }

        public override void Exit()
        {
            animator.ResetTrigger("isShooting");
            mAudioSource.Stop();
            base.Exit();
        }
    }

    public class DeathTransitionState : State
    {
        private float transitionTimer = 0f;
        private float transitionDelay = 0.1f; // Small delay to ensure idle animation starts
        private AudioSource mAudioSource;

        public DeathTransitionState(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform)
            : base(gameObject, agent, anim, transform)
        {
            stateName = STATE.IDLE; // Temporarily set to idle for transition
            mAudioSource = gameObject.GetComponent<AudioSource>();
        }

        public override void Enter()
        {
            // First transition to idle state in animator
            animator.SetTrigger("isIdle");
            transitionTimer = 0f;
            mAudioSource.Stop();
            base.Enter();
        }

        public override void Update()
        {
            transitionTimer += Time.deltaTime;

            // After a short delay, transition to actual death state
            if (transitionTimer >= transitionDelay)
            {
                state = new DeadState(npc, navMeshAgent, animator, playerTransform);
                stage = EVENT.EXIT;
            }
        }

        public override void Exit()
        {
            animator.ResetTrigger("isIdle");
        }
    }

    public class DeadState : State
    {
        public DeadState(GameObject gameObject, NavMeshAgent agent, Animator anim, Transform transform)
            : base(gameObject, agent, anim, transform)
        {
            stateName = STATE.SLEEP;
        }

        public override void Enter()
        {
            animator.SetTrigger("isSleeping");
            base.Enter();
        }
    }
}