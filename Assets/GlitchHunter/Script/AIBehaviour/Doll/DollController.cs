using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using GlitchHunter.Enum;
using GlitchHunter.Manager;

namespace GlitchHunter.Script.DollMechanic
{
    public class DollController : MonoBehaviour
    {
        [SerializeField] private DollDataContainer dataContainer;
        [SerializeField] private EnemyState currentState = EnemyState.Idle;

        public Action<float> OnMove;         // Triggers movement animation
        public Action<Vector3> OnExplode;    // Triggers explode animation/VFX

        [SerializeField] private Transform _target;
        private NavMeshAgent _agent;
        private bool _hasExploded;
        private const float TARGET_SCALE_MULTIPLIER = 2;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();

            if (_target == null && GameManager.Instance?.PlayerPrefab != null)
            {
                _target = GameManager.Instance.PlayerPrefab.transform;
            }

            StartCoroutine(StateMachine());
        }

        private IEnumerator StateMachine()
        {
            yield return StartCoroutine(IdleState());

            yield return StartCoroutine(ChaseTarget());

            yield return StartCoroutine(Explode());
        }

        private IEnumerator IdleState()
        {
            currentState = EnemyState.Idle;
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator ChaseTarget()
        {
            currentState = EnemyState.Chasing;

            while (_target != null && Vector3.Distance(transform.position, _target.position) > dataContainer.stopDistance)
            {
                _agent.SetDestination(_target.position);
                OnMove?.Invoke(_agent.velocity.magnitude); // Trigger movement animation
                yield return null;
            }

            _agent.ResetPath(); // Stop moving
        }
        

        private IEnumerator Explode()
        {
            currentState = EnemyState.Attacking;

            if (_hasExploded) yield break;
            _hasExploded = true;
           
            yield return StartCoroutine(ScaleBeforeExplode(0.5f, TARGET_SCALE_MULTIPLIER));

          
            if (_target != null)
                OnExplode?.Invoke(_target.position); // Call animation and VFX
            
            yield return new WaitForSeconds(1f);
        }

        
        private IEnumerator ScaleBeforeExplode(float duration, float targetScaleMultiplier)
        {
            var originalScale = transform.localScale;
            var targetScale = originalScale * targetScaleMultiplier;

            var timer = 0f;
            while (timer < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;
        }

    }
}
