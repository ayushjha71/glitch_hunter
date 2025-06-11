using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GlitchHunter.Constant
{
    public sealed class GameEnvironment
    {
        private List<GameObject> mCheckpoints = new List<GameObject>();

        private static GameEnvironment Instance;

        public List<GameObject> Checkpoints => mCheckpoints;
        
        public static GameEnvironment Singleton
        {
            get
            {
                if(Instance == null)
                {
                    Instance = new GameEnvironment();
                    Instance.Checkpoints.AddRange(GameObject.FindGameObjectsWithTag("Checkpoints"));

                    //If we need the Checkpoint is Accesnding Order, and name the waypoint "CheckoutPoint1, CheckoutPoint2" so on
                    // Instance.mCheckpoints = Instance.mCheckpoints.OrderBy(wayPoints => wayPoints.name).ToList();
                }
                return Instance;
            }
        }
    }
}
