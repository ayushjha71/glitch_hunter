using GlitchHunter.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace GlitchHunter.Weapon
{
    public class GunContainer : MonoBehaviour
    {
        [SerializeField]
        private GameObject playerTransform;
        [SerializeField]
        private GameObject pistelContainer;
        [SerializeField]
        private GameObject gunContainer;
        [SerializeField]
        private GameObject meleeContainer;


        public GameObject PlayerTranform => playerTransform;
        public GameObject PistelContainer => pistelContainer;
        public GameObject GunContainers => gunContainer;
        public GameObject MeleeContainer => meleeContainer;


        private bool canDestroy = false;

        private void OnEnable()
        {
            GameManager.Instance.PlayerPrefab = PlayerTranform;
        }

        public void Update()
        {
            if (GameManager.Instance.IsMeleeCombatStarted)
            {
                if (!canDestroy)
                {
                    canDestroy = true;
                    Destroy(GunContainers);
                }
                return;
            }
        }
    }
}
