using GlitchHunter.Enum;
using GlitchHunter.Handler;
using GlitchHunter.Handler.Enemy;
using UnityEngine;

namespace GlitchHunter.Handler.Enemy.MeleeCombat
{
    public class EnemyMeleeCombatSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyHealthHandler enemyHealth;
        [SerializeField] private EnemyMovementHandler enemyMovement;
      //  [SerializeField] private SphericalMovement sphericalMovement;

        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float minAttackDistance = 0.8f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Detection Settings")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float detectionAngle = 360f;
        [SerializeField] private float heightTolerance = 1.5f;
        [SerializeField] private float detectionRange = 10f;

        // Animation hashes
        private readonly int IsIdle = Animator.StringToHash("IsIdleComat");
        private readonly int IsWalk = Animator.StringToHash("IsWalk");
        private readonly int IsAttackOne = Animator.StringToHash("Attack_1");
        private readonly int IsAttackTwo = Animator.StringToHash("Attack_2");
        private readonly int IsAttackThree = Animator.StringToHash("Attack_3");
        private readonly int IsDead = Animator.StringToHash("IsDead");

        // Combat states
        private float lastAttackTime;
        private bool isAttacking = false;
        private bool playerInAttackRange = false;
        private bool playerInMinAttackDistance = false;
        private bool playerInDetectionRange = false;
        private Transform playerTarget;

        // Special attack logic
        private bool needsSpecialAttack = false;
        private float lastHealthCheckPoint;
        private bool waitingForSpecialAttack = false;
        private bool isInSpecialAttackSequence = false;

        private bool isAletMode = true;


        private void Awake()
        {
            if (enemyHealth == null)
                enemyHealth = GetComponent<EnemyHealthHandler>();

            if (enemyMovement == null)
                enemyMovement = GetComponent<EnemyMovementHandler>();

            lastHealthCheckPoint = enemyHealth.MaxHealth;
        }

        private void Update()
        {
            if (enemyHealth.IsDead())
            {
                UpdateAnimatorState(MeleeAttackState.DEAD);
                return;
            }

            if (playerInAttackRange && isAletMode)
            {
                isAletMode = false;
                //sphericalMovement.StartMoving();
            }

            CheckPlayerPositions();
            CheckSpecialAttackCondition();
            HandleCombatLogic();
            HandleMovementState();
        }

        private void CheckPlayerPositions()
        {
            playerInAttackRange = false;
            playerInMinAttackDistance = false;
            playerInDetectionRange = false;

            // Don't update target positions during attack animations
            if (isAttacking) return;

            Transform previousTarget = playerTarget;
            bool foundPlayerInRange = false;

            // Check detection range first (since it's larger than attack range)
            Collider[] detectedPlayers = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
            foreach (Collider playerCollider in detectedPlayers)
            {
                if (IsValidTarget(playerCollider.transform))
                {
                    playerTarget = playerCollider.transform;
                    playerInDetectionRange = true;
                    foundPlayerInRange = true;
                    break;
                }
            }

            // Only check attack range if player is in detection range
            if (playerInDetectionRange)
            {
                Collider[] hitPlayers = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
                foreach (Collider playerCollider in hitPlayers)
                {
                    if (IsValidTarget(playerCollider.transform))
                    {
                        playerTarget = playerCollider.transform;
                        playerInAttackRange = true;

                        float distance = Vector3.Distance(transform.position, playerTarget.position);
                        if (distance <= minAttackDistance)
                        {
                            playerInMinAttackDistance = true;
                        }
                        break;
                    }
                }
            }

            // If we didn't find any valid players in detection range, clear the target
            if (!foundPlayerInRange)
            {
                playerInDetectionRange = false;
                playerTarget = null;
            }
            // Keep previous target if we lost detection but were previously tracking
            else if (!playerInDetectionRange && previousTarget != null)
            {
                playerTarget = previousTarget;
                playerInDetectionRange = true;
            }
        }

        private bool IsValidTarget(Transform player)
        {
            if (player == null) return false;

            Vector3 directionToPlayer = player.position - transform.position;
            float distance = directionToPlayer.magnitude;

            // Check height tolerance
            if (Mathf.Abs(player.position.y - transform.position.y) > heightTolerance) return false;

            // Check angle
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > detectionAngle / 2f) return false;

            // Check line of sight
            if (!HasLineOfSight(player)) return false;

            return true;
        }

        private void CheckSpecialAttackCondition()
        {
            if (isInSpecialAttackSequence) return;

            float currentHealth = enemyHealth.CurrentHealth;

            // Check if health dropped below 50
            if (currentHealth <= 50 && !needsSpecialAttack && !waitingForSpecialAttack)
            {
                needsSpecialAttack = true;
                waitingForSpecialAttack = true;
                return;
            }

            // Check if health crossed a 100-point threshold
            float currentHealthCheckPoint = Mathf.Floor(currentHealth / 100) * 100;
            if (currentHealthCheckPoint < lastHealthCheckPoint && !needsSpecialAttack && !waitingForSpecialAttack)
            {
                needsSpecialAttack = true;
                waitingForSpecialAttack = true;
                lastHealthCheckPoint = currentHealthCheckPoint;
            }
        }

        private void HandleCombatLogic()
        {
            // Check if cooldown is still active
            if (Time.time < lastAttackTime + attackCooldown)
            {
                isAttacking = false;
                UpdateAnimatorState(MeleeAttackState.IDLE);
                return;
            }

            // Priority 1: Special attack if needed and player is in range or we're in the sequence
            if (needsSpecialAttack)
            {
                if (playerInAttackRange || isInSpecialAttackSequence)
                {
                    PerformSpecialAttack();
                    return;
                }
                else if (playerInDetectionRange)
                {
                    // Chase player to get in range for special attack
                    return;
                }
            }

            // Priority 2: Normal attacks if player is in range
            if (playerInAttackRange)
            {
                if (playerInMinAttackDistance)
                {
                    PerformAttack(MeleeAttackState.Attack_ONE);
                }
                else
                {
                    PerformAttack(MeleeAttackState.Attack_TWO);
                }
            }
        }

        private void HandleMovementState()
        {
            if (isAttacking)
            {
                // Only stop movement for normal attacks
                if (!isInSpecialAttackSequence)
                {
                    enemyMovement.StopMovement();
                }
                return;
            }

            if (playerTarget != null)
            {
                enemyMovement.SetTarget(playerTarget);

                if (playerInAttackRange)
                {
                    // Stop and look at player when in attack range
                    enemyMovement.LookAtTarget(playerTarget.position);
                    enemyMovement.StopMovement();
                    UpdateAnimatorState(MeleeAttackState.IDLE);
                }
                else if ((playerInDetectionRange && !isInSpecialAttackSequence) || (needsSpecialAttack && !isInSpecialAttackSequence))
                {
                    // Chase player when in detection range or need special attack
                    enemyMovement.MoveToTarget();
                    UpdateAnimatorState(MeleeAttackState.WALK);
                    Debug.Log("Player walking state true");
                }
                else
                {
                    // No target, go idle
                    enemyMovement.StopMovement();
                    UpdateAnimatorState(MeleeAttackState.IDLE);
                }
            }
            else
            {
                // No target at all
                enemyMovement.StopMovement();
                UpdateAnimatorState(MeleeAttackState.IDLE);
            }
        }

        private void PerformAttack(MeleeAttackState attackType)
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            UpdateAnimatorState(attackType);
        }

        private void PerformSpecialAttack()
        {
            isAttacking = true;
            isInSpecialAttackSequence = true;
            lastAttackTime = Time.time;
            UpdateAnimatorState(MeleeAttackState.Attack_THREE);
        }

        private void UpdateAnimatorState(MeleeAttackState state)
        {
            // Reset all triggers first to prevent overlap
            animator.ResetTrigger(IsAttackOne);
            animator.ResetTrigger(IsAttackTwo);
            animator.ResetTrigger(IsAttackThree);
            animator.ResetTrigger(IsDead);

            switch (state)
            {
                case MeleeAttackState.IDLE:
                    animator.SetBool(IsIdle, true);
                    animator.SetBool(IsWalk, false);
                    break;
                case MeleeAttackState.WALK:
                    animator.SetBool(IsIdle, false);
                    animator.SetBool(IsWalk, true);
                    break;
                case MeleeAttackState.Attack_ONE:
                    animator.SetTrigger(IsAttackOne);
                    animator.SetBool(IsIdle, false);
                    animator.SetBool(IsWalk, false);
                    break;
                case MeleeAttackState.Attack_TWO:
                    animator.SetTrigger(IsAttackTwo);
                    animator.SetBool(IsIdle, false);
                    animator.SetBool(IsWalk, false);
                    break;
                case MeleeAttackState.Attack_THREE:
                    animator.SetTrigger(IsAttackThree);
                    animator.SetBool(IsIdle, false);
                    animator.SetBool(IsWalk, false);
                    break;
                case MeleeAttackState.DEAD:
                    animator.SetTrigger(IsDead);
                    animator.SetBool(IsIdle, false);
                    animator.SetBool(IsWalk, false);
                    break;
            }
        }

        // Animation Event - Called during attack animations to deal damage
        public void OnAttackHit(float damage, PlayerHealthHandler playerHealth)
        {
            if (!playerInAttackRange || playerTarget == null) return;

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance > attackRange * 1.2f) return;

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        // Animation Event - Called when attack animation ends
        public void OnAttackEnd(AnimationEvent animationEvent)
        {
            isAttacking = false;

            if (isInSpecialAttackSequence)
            {
                isInSpecialAttackSequence = false;
                needsSpecialAttack = false;
                waitingForSpecialAttack = false;
            }

            UpdateAnimatorState(MeleeAttackState.IDLE);
        }

        private bool HasLineOfSight(Transform target)
        {
            RaycastHit hit;
            Vector3 direction = target.position - transform.position;
            return !Physics.Raycast(transform.position, direction, out hit, attackRange) || hit.transform == target;
        }

        private void OnDrawGizmosSelected()
        {
            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Min attack distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minAttackDistance);

            // Detection range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw detection angle
            Vector3 leftDir = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, leftDir * detectionRange);
            Gizmos.DrawRay(transform.position, rightDir * detectionRange);
        }
    }
}