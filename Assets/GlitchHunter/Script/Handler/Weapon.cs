using GlitchHunter.Constant;
using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody rb;
        [SerializeField]
        private Collider collider;
        [SerializeField]
        private float pickupRange = 5;
        [SerializeField]
        private float dropForwardForce;
        [SerializeField]
        private float dropUpwardForce;


        private Transform gunContainerTransform;
        private Transform camTransform;
        private Transform playerTransform;
        private bool IsEquipped;
        private bool isActivePrompt = false;
        private bool IsInitialized = false;

        public static bool slotFull;

        private void InitializedComponents()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                camTransform = Camera.main.transform;
                playerTransform = GameManager.Instance.PlayerPrefab.transform;
                gunContainerTransform = GameManager.Instance.PlayerPrefab.GetComponent<ShootingHandler>().gunContainerTransform;
            }
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameStarted)
            {
                return;
            }

            if (GameManager.Instance.IsGameStarted && !IsInitialized)
            {
                InitializedComponents();
            }

            Vector3 distanceToPlayer = playerTransform.position - transform.position;

            if ((!IsEquipped && distanceToPlayer.magnitude <= pickupRange && !slotFull))
            {
                Debug.Log("Gun Not Pickup IsActive Prompt false");
                if (!isActivePrompt)
                {
                    isActivePrompt = true;
                    GlitchHunterConstant.OnShowPrompt?.Invoke(true, "Press F To Collect");
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    Debug.Log("Gun Not Pickup IsActive Prompt false Pickup funation called");
                    PickUp();
                    GlitchHunterConstant.OnShowPrompt?.Invoke(false, "");
                    isActivePrompt = false;
                }
            }
            if (IsEquipped && Input.GetKeyDown(KeyCode.Q))
            {
                Drop();
            }
        }

        private void PickUp()
        {
            IsEquipped = true;
            slotFull = true;

            transform.SetParent(gunContainerTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            transform.localScale = Vector3.one;
            rb.isKinematic = true;
            collider.isTrigger = true;
        }

        private void Drop()
        {
            IsEquipped = false;
            slotFull = false;

            transform.SetParent(null);

            rb.isKinematic = false;
            collider.isTrigger = false;

            //Hand object carries momentum of player
            rb.angularVelocity = playerTransform.GetComponent<Rigidbody>().angularVelocity;


            rb.AddForce(camTransform.forward * dropForwardForce, ForceMode.Impulse);
            rb.AddForce(camTransform.forward * dropUpwardForce, ForceMode.Impulse);

            float random = UnityEngine.Random.Range(-1, 1);

            rb.AddTorque(new Vector3(random, random, random));
        }
    }
}
