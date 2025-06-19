using UnityEngine;

namespace GlitchHunter.Handler.Enemy.MeleeCombat
{
    public class EnemyMovementHandler : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float chaseSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float stoppingDistance = 1.5f;

        private Transform target;
        private Rigidbody rb;
        private bool isChasing = false;
        private float currentSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            currentSpeed = walkSpeed;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void LookAtTarget(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Keep the enemy upright

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public void MoveToTarget()
        {
            if (target == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // Determine if we should chase or walk
            isChasing = distanceToTarget > stoppingDistance;
            currentSpeed = isChasing ? chaseSpeed : walkSpeed;

            if (isChasing)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                direction.y = 0; // Keep movement horizontal

                // Move using Rigidbody for proper physics interaction
                Vector3 newPosition = transform.position + direction * currentSpeed * Time.deltaTime;
                rb.MovePosition(newPosition);

                // Smoothly rotate toward target
                LookAtTarget(target.position);
            }
            else
            {
                // Stop movement when close enough
                rb.linearVelocity = Vector3.zero;
            }
        }

        public void StopMovement()
        {
            rb.linearVelocity = Vector3.zero;
            isChasing = false;
        }

        public bool IsChasing()
        {
            return isChasing;
        }
    }
}