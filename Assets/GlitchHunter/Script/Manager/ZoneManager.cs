using GlitchHunter.Constant;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlitchHunter.Manager
{
    public class ZoneManager : MonoBehaviour
    {
        public static ZoneManager Instance { get; private set; }

        public event Action<ZoneDefinition> OnZoneChanged;

        private List<ZoneDefinition> zones = new List<ZoneDefinition>();
        private int currentActiveZoneIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void RegisterZone(ZoneDefinition zone)
        {
            if (!zones.Contains(zone))
            {
                zones.Add(zone);
                // Sort zones by their associated wave index
                zones.Sort((a, b) => a.associatedWaveIndex.CompareTo(b.associatedWaveIndex));

                // Deactivate all zones except the first one
                zone.SetZoneActive(zone.associatedWaveIndex == 0);
            }
        }

        public void OnWaveCompleted(int waveIndex)
        {
            // Deactivate the zone for this wave
            if (waveIndex < zones.Count)
            {
                zones[waveIndex].SetZoneActive(false);
            }

            // Activate the next zone if available
            if (waveIndex + 1 < zones.Count)
            {
                currentActiveZoneIndex = waveIndex + 1;
                zones[currentActiveZoneIndex].SetZoneActive(true);
                OnZoneChanged?.Invoke(zones[currentActiveZoneIndex]);
            }
            else
            {
                OnZoneChanged?.Invoke(null);
            }
        }

        public ZoneDefinition GetCurrentActiveZone()
        {
            if (currentActiveZoneIndex < zones.Count)
            {
                return zones[currentActiveZoneIndex];
            }
            return null;
        }
    }
}