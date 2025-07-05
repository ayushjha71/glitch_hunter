using GlitchHunter.Constant;
using UnityEngine;

namespace GlitchHunter.Handler.Player
{
    public class CameraShakeHandler : MonoBehaviour
    {
        [Header("Shake Settings")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeMagnitude = 0.2f;
        [SerializeField] private float dampingSpeed = 1.0f;
        [SerializeField]
        private Transform cameraShakeTransform;

        private Vector3 initialPosition;
        private float currentShakeDuration;
        private bool isShaking = false;

        //private void Awake()
        //{
        //    cameraTransform = GetComponent<Transform>();
        //}

        private void OnEnable()
        {
            // Subscribe to player hit event
            GlitchHunterConstant.OnPlayerHitImpact += TriggerShake;
        }

        private void OnDisable()
        {
            // Unsubscribe from player hit event
            GlitchHunterConstant.OnPlayerHitImpact -= TriggerShake;
        }

        private void Update()
        {
            if (isShaking)
            {
                if (currentShakeDuration > 0)
                {
                    // Randomize camera position within a sphere
                    cameraShakeTransform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

                    // Reduce shake duration over time
                    currentShakeDuration -= Time.deltaTime * dampingSpeed;
                }
                else
                {
                    // Reset camera position when shake is complete
                    isShaking = false;
                    currentShakeDuration = 0f;
                    cameraShakeTransform.localPosition = initialPosition;
                }
            }
        }

        public void TriggerShake()
        {
            // Only start a new shake if we're not already shaking
            if (!isShaking)
            {
                initialPosition = cameraShakeTransform.localPosition;
                currentShakeDuration = shakeDuration;
                isShaking = true;
            }
        }

        // Optional: Public method to trigger shake with custom parameters
        public void TriggerShake(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
            TriggerShake();
        }
    }
}