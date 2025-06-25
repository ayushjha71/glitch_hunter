using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Constant
{
    public class PlayerZoneClamp : MonoBehaviour
    {
        [Tooltip("How aggressively to push player back (0-1)")]
        [Range(0, 1)] public float pushFactor = 0.9f;

        [Header("Effects")]
        public ParticleSystem boundaryEffect;
        public AudioClip boundarySound;

        private CharacterController characterController;
        private AudioSource audioSource;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            Vector3 zoneCenter = GameManager.Instance.WorldCenter;
            float radius = GameManager.Instance.WorldRadius;

            // Calculate vector from zone center to player
            Vector3 toPlayer = transform.position - zoneCenter;

            float distance = toPlayer.magnitude;

            // If player is outside boundary
            if (distance > radius)
            {
                // Calculate how much we need to push player back
                Vector3 pushDirection = toPlayer.normalized;
                float overshoot = distance - radius;
                Vector3 correction = pushDirection * (overshoot * pushFactor);

                // Apply correction
                if (characterController != null)
                {
                    characterController.Move(-correction);
                }
                else
                {
                    transform.position -= correction;
                }

                // Play effects
                PlayBoundaryEffects();
            }
        }

        private void PlayBoundaryEffects()
        {
            if (boundaryEffect != null && !boundaryEffect.isPlaying)
            {
                boundaryEffect.Play();
            }

            if (boundarySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(boundarySound);
            }
        }
    }
}