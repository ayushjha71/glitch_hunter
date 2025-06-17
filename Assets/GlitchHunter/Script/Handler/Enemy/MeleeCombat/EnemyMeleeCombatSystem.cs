using UnityEngine;
using UnityEngine.UI;

public class EnemyMeleeCombatSystem : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float minDetectionRange = 5f;
    [SerializeField] private float maxDetectionRange = 8f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Distances")]
    [SerializeField] private float closeAttackDistance = 2f;
    [SerializeField] private float mediumAttackDistance = 4f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Third Attack Settings")]
    [SerializeField] private float thirdAttackChargeTime = 3f;
    [SerializeField] private Slider chargeSlider;

    [Header("Attack Cooldowns")]
    [SerializeField] private float attack1Cooldown = 1f;
    [SerializeField] private float attack2Cooldown = 1.5f;
    [SerializeField] private float attack3Cooldown = 2f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    // Components
    private Animator animator;
    private Rigidbody rb;
    private Transform player;

    // State Management
    private EnemyState currentState = EnemyState.Idle;
    private float distanceToPlayer;
    private bool isPlayerInRange;
    private bool canAttack = true;

    // Third Attack System
    private float chargeTimer = 0f;
    private bool isChargingThirdAttack = false;
    private bool canUseThirdAttack = true;

    // Attack Cooldown Timers
    private float attack1Timer;
    private float attack2Timer;
    private float attack3Timer;

    // Animation Hash IDs (for performance)
    private int isIdleHash;
    private int isIdleCombatHash;
    private int isWalkHash;
    private int attack1Hash;
    private int attack2Hash;
    private int attack3Hash;

    public enum EnemyState
    {
        Idle,
        Alert,
        Chasing,
        Attacking,
        ChargingAttack
    }

    void Start()
    {
        InitializeComponents();
        InitializeAnimationHashes();
        SetupChargeSlider();
        FindPlayer();
    }

    void Update()
    {
        if (player == null) return;

        UpdateDistanceToPlayer();
        UpdateCooldownTimers();
        HandleStateLogic();
        UpdateAnimatorParameters();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    #region Initialization

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on " + gameObject.name);
        }
    }

    private void InitializeAnimationHashes()
    {
        isIdleHash = Animator.StringToHash("IsIdle");
        isIdleCombatHash = Animator.StringToHash("IsIdleCombat");
        isWalkHash = Animator.StringToHash("IsWalk");
        attack1Hash = Animator.StringToHash("Attack_1");
        attack2Hash = Animator.StringToHash("Attack_2");
        attack3Hash = Animator.StringToHash("Attack_3");
    }

    private void SetupChargeSlider()
    {
        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 1f;
            chargeSlider.value = 0f;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure player has 'Player' tag.");
        }
    }

    #endregion

    #region Update Methods

    private void UpdateDistanceToPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= maxDetectionRange;
    }

    private void UpdateCooldownTimers()
    {
        if (attack1Timer > 0) attack1Timer -= Time.deltaTime;
        if (attack2Timer > 0) attack2Timer -= Time.deltaTime;
        if (attack3Timer > 0) attack3Timer -= Time.deltaTime;

        canAttack = attack1Timer <= 0 && attack2Timer <= 0 && attack3Timer <= 0;
    }

    #endregion

    #region State Logic

    private void HandleStateLogic()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Alert:
                HandleAlertState();
                break;
            case EnemyState.Chasing:
                HandleChasingState();
                break;
            case EnemyState.Attacking:
                HandleAttackingState();
                break;
            case EnemyState.ChargingAttack:
                HandleChargingAttackState();
                break;
        }
    }

    private void HandleIdleState()
    {
        if (isPlayerInRange && distanceToPlayer <= minDetectionRange)
        {
            ChangeState(EnemyState.Alert);
        }
    }

    private void HandleAlertState()
    {
        if (!isPlayerInRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        LookAtPlayer();

        // Check for immediate attacks
        if (canAttack && distanceToPlayer <= closeAttackDistance)
        {
            PerformAttack1();
        }
        else if (canAttack && distanceToPlayer <= mediumAttackDistance)
        {
            PerformAttack2();
        }
        else if (distanceToPlayer > mediumAttackDistance && canUseThirdAttack)
        {
            ChangeState(EnemyState.ChargingAttack);
        }
        else if (distanceToPlayer > stoppingDistance)
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    private void HandleChasingState()
    {
        if (!isPlayerInRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        LookAtPlayer();

        if (distanceToPlayer <= stoppingDistance)
        {
            ChangeState(EnemyState.Alert);
        }
    }

    private void HandleAttackingState()
    {
        // This state is handled by animation events
        // The enemy will return to Alert state after attack animation completes
    }

    private void HandleChargingAttackState()
    {
        if (!isPlayerInRange)
        {
            ResetThirdAttackCharge();
            ChangeState(EnemyState.Idle);
            return;
        }

        LookAtPlayer();

        // Update charge timer
        chargeTimer += Time.deltaTime;
        float chargeProgress = chargeTimer / thirdAttackChargeTime;

        if (chargeSlider != null)
        {
            chargeSlider.value = chargeProgress;
        }

        // Check if charge is complete
        if (chargeProgress >= 1f)
        {
            PerformAttack3();
            ResetThirdAttackCharge();
        }

        // Cancel charge if player gets too close
        if (distanceToPlayer <= closeAttackDistance && canAttack)
        {
            ResetThirdAttackCharge();
            PerformAttack1();
        }
        else if (distanceToPlayer <= mediumAttackDistance && canAttack)
        {
            ResetThirdAttackCharge();
            PerformAttack2();
        }
    }

    #endregion

    #region Attack Methods

    private void PerformAttack1()
    {
        ChangeState(EnemyState.Attacking);
        animator.SetTrigger(attack1Hash);
        attack1Timer = attack1Cooldown;
        Debug.Log("Performing Attack 1 - Close Range");
    }

    private void PerformAttack2()
    {
        ChangeState(EnemyState.Attacking);
        animator.SetTrigger(attack2Hash);
        attack2Timer = attack2Cooldown;
        Debug.Log("Performing Attack 2 - Medium Range");
    }

    private void PerformAttack3()
    {
        ChangeState(EnemyState.Attacking);
        animator.SetTrigger(attack3Hash);
        attack3Timer = attack3Cooldown;
        canUseThirdAttack = false;

        // Reset third attack availability after cooldown
        Invoke(nameof(ResetThirdAttackAvailability), attack3Cooldown + 2f);

        Debug.Log("Performing Attack 3 - Charged Attack");
    }

    private void ResetThirdAttackCharge()
    {
        isChargingThirdAttack = false;
        chargeTimer = 0f;

        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
            chargeSlider.value = 0f;
        }
    }

    private void ResetThirdAttackAvailability()
    {
        canUseThirdAttack = true;
    }

    #endregion

    #region Movement and Rotation

    private void HandleMovement()
    {
        if (currentState == EnemyState.Chasing)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Keep movement on horizontal plane

            Vector3 targetPosition = transform.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep rotation on horizontal plane

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Animator Updates

    private void UpdateAnimatorParameters()
    {
        // Reset all boolean parameters
        animator.SetBool(isIdleHash, false);
        animator.SetBool(isIdleCombatHash, false);
        animator.SetBool(isWalkHash, false);

        // Set appropriate parameter based on current state
        switch (currentState)
        {
            case EnemyState.Idle:
                animator.SetBool(isIdleHash, true);
                break;
            case EnemyState.Alert:
            case EnemyState.ChargingAttack:
                animator.SetBool(isIdleCombatHash, true);
                break;
            case EnemyState.Chasing:
                animator.SetBool(isWalkHash, true);
                break;
            case EnemyState.Attacking:
                // Attack triggers are handled in attack methods
                break;
        }
    }

    #endregion

    #region State Management

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        ExitCurrentState();
        currentState = newState;
        EnterNewState();

        Debug.Log($"Enemy state changed to: {newState}");
    }

    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.ChargingAttack:
                ResetThirdAttackCharge();
                break;
        }
    }

    private void EnterNewState()
    {
        switch (currentState)
        {
            case EnemyState.ChargingAttack:
                isChargingThirdAttack = true;
                if (chargeSlider != null)
                {
                    chargeSlider.gameObject.SetActive(true);
                }
                break;
        }
    }

    #endregion

    #region Animation Events

    // Call this method from animation events when attack animations complete
    public void OnAttackComplete()
    {
        ChangeState(EnemyState.Alert);
    }

    // Call this method from animation events when damage should be dealt
    public void OnDealDamage()
    {
        // Implement damage dealing logic here
        // Check if player is within attack range and deal damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 1.5f,
            1f, playerLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Deal damage to player
                Debug.Log("Damage dealt to player!");
                // Add your damage dealing code here
                // hitCollider.GetComponent<PlayerHealth>()?.TakeDamage(damageAmount);
            }
        }
    }

    #endregion

    #region Debug and Gizmos

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw detection ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, minDetectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, maxDetectionRange);

        // Draw attack ranges
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, closeAttackDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, mediumAttackDistance);

        // Draw damage detection area
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, 1f);
    }

    #endregion
}