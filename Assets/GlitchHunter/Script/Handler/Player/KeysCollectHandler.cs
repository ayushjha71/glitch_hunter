using GlitchHunter.Constant;
using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class KeysCollectHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask collectLayer;
        [SerializeField] private float pickUpRange = 5f;
        private bool isActivePrompt = false;
        private Key currentKey = null; // Track which key we're currently looking at

        private void Update()
        {
            CheckForKey();
        }

        private void CheckForKey()
        {
            // Reset current key reference
            Key detectedKey = null;

            // Use a sphere cast to detect nearby keys
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, pickUpRange, collectLayer);

            float closestDistance = float.MaxValue;

            // Find the closest key within range
            foreach (Collider collider in nearbyColliders)
            {
                Key key = collider.GetComponent<Key>();
                if (key != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);

                    // Check if this key is in front of the player using dot product
                    Vector3 directionToKey = (collider.transform.position - Camera.main.transform.position).normalized;
                    float dot = Vector3.Dot(Camera.main.transform.forward, directionToKey);

                    // Only consider keys that are somewhat in front of the player (dot > 0.3 means roughly 70 degree cone)
                    if (dot > 0.3f && distance < closestDistance)
                    {
                        closestDistance = distance;
                        detectedKey = key;
                    }
                }
            }

            // Update prompt based on detected key
            if (detectedKey != null)
            {
                // If we found a key and it's different from the current one, or we weren't showing a prompt
                if (currentKey != detectedKey || !isActivePrompt)
                {
                    currentKey = detectedKey;
                    isActivePrompt = true;
                    GlitchHunterConstant.OnShowPrompt?.Invoke("Press F To Collect");
                }

                // Handle key collection input
                if (Input.GetKeyDown(KeyCode.F))
                {
                    currentKey.OnCollectKey();
                    GlitchHunterConstant.OnShowPrompt?.Invoke("");
                    isActivePrompt = false;
                    currentKey = null;
                }
            }
            else
            {
                // No key detected, hide prompt if it was showing
                if (isActivePrompt)
                {
                    isActivePrompt = false;
                    currentKey = null;
                    GlitchHunterConstant.OnShowPrompt?.Invoke("");
                }
            }
        }

        // Optional: Draw debug information
        private void OnDrawGizmosSelected()
        {
            // Draw pickup range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickUpRange);

            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * pickUpRange);
        }
    }
}