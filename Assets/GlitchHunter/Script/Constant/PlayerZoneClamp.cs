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
        private ZoneDefinition lastActiveZone;
        private Vector3 playerPositionWhenZoneChanged;
        private bool justChangedZone = false;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            audioSource = GameManager.Instance.AudioSource;
        }

        private void Update()
        {
            ZoneDefinition currentZone = ZoneManager.Instance.GetCurrentActiveZone();

            // If no zone is active, player can move freely
            if (currentZone == null || !currentZone.IsActive)
            {
                lastActiveZone = null;
                justChangedZone = false;
                return;
            }

            // Check if zone just changed
            if (currentZone != lastActiveZone)
            {
                lastActiveZone = currentZone;
                playerPositionWhenZoneChanged = transform.position;
                justChangedZone = true;
                return; // Skip clamping for this frame
            }

            Vector3 zoneCenter = currentZone.WorldCenter;
            float radius = currentZone.WorldRadius;
            Vector3 toPlayer = transform.position - zoneCenter;
            float distance = toPlayer.magnitude;

            // If zone just changed, only start clamping when player tries to move further away
            if (justChangedZone)
            {
                Vector3 toPlayerWhenZoneChanged = playerPositionWhenZoneChanged - zoneCenter;
                float distanceWhenZoneChanged = toPlayerWhenZoneChanged.magnitude;

                // If player is moving further away from zone center, start clamping
                if (distance > distanceWhenZoneChanged)
                {
                    justChangedZone = false; // Start normal clamping from now on
                }
                else
                {
                    return; // Let player move freely until they try to go further out
                }
            }

            // Normal clamping logic - only clamp if player is outside boundary
            if (distance > radius)
            {
                // Calculate the exact boundary position
                Vector3 clampedPosition = zoneCenter + (toPlayer.normalized * radius);

                // Move player back to the boundary edge
                if (characterController != null)
                {
                    Vector3 correction = (clampedPosition - transform.position) * pushFactor;
                    characterController.Move(correction);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, clampedPosition, pushFactor);
                }

                // Play boundary effects
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