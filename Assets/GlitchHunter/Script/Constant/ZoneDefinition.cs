using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Constant
{
    public class ZoneDefinition : MonoBehaviour
    {
        [Header("Zone Settings")]
        [Tooltip("Radius of the playable area")]
        public float zoneRadius = 50f;

        [Tooltip("Should the zone ignore Y-axis (cylinder instead of sphere)?")]
        public bool isCylindrical = false;

        [Tooltip("Which wave this zone is associated with (0 = first wave)")]
        public int associatedWaveIndex = 0;

        [Header("Visualization")]
        public bool showGizmo = true;
        public Color gizmoColor = new Color(0, 1, 1, 0.3f);

        // Public getters
        public Vector3 WorldCenter => transform.position;
        public float WorldRadius => zoneRadius * transform.lossyScale.x;
        public bool IsActive { get; private set; } = true;

        private void Start()
        {
            // Register this zone with the ZoneManager
            ZoneManager.Instance.RegisterZone(this);
        }

        public void SetZoneActive(bool active)
        {
            IsActive = active;
            // You could add visual effects here when zone is deactivated
        }

        private void OnDrawGizmos()
        {
            if (!showGizmo || !IsActive) return;

            Gizmos.color = gizmoColor;

            if (isCylindrical)
            {
                // Draw cylinder representation
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation,
                    new Vector3(1, 0, 1) * zoneRadius * 2);
                Gizmos.DrawWireSphere(Vector3.zero, 1);
                Gizmos.matrix = oldMatrix;
            }
            else
            {
                // Draw sphere representation
                Gizmos.DrawWireSphere(transform.position, WorldRadius);
            }
        }
    }
}