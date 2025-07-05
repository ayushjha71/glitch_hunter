using GlitchHunter.ABase;
using GlitchHunter.Constant;
using GlitchHunter.Interface;
using GlitchHunter.Manager;
using GlitchHunter.Weapon;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class CollectableWeapon : ABaseCollectable, IEquippable
    {
        [SerializeField] private float dropForwardForce = 10f;
        [SerializeField] private float dropUpwardForce = 5f;
        [SerializeField] private RotateObjectHandler rotationHandler;

        private Transform gunContainerTransform;
        private Transform camTransform;
        public static bool slotFull;

        public override void OnPickup(GameObject collector)
        {
            if (slotFull) return;

            isPickedUp = true;
            slotFull = true;
            GameManager.Instance.IsEquipped = true;

            if (rotationHandler != null)
            {
                rotationHandler.enabled = false;
            }

            camTransform = Camera.main.transform;
            gunContainerTransform = GameManager.Instance.PlayerPrefab.GetComponentInParent<GunContainer>().GunContainers.transform;

            transform.SetParent(gunContainerTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            transform.localScale = Vector3.one;

            rb.isKinematic = true;
            col.isTrigger = true;

            OnEquip();
        }

        public override void OnDrop()
        {
            if (!isPickedUp) return;

            isPickedUp = false;
            slotFull = false;
            GameManager.Instance.IsEquipped = false;

            transform.SetParent(null);

            rb.isKinematic = false;
            col.isTrigger = false;

            // Apply forces correctly
            rb.linearVelocity = GameManager.Instance.PlayerPrefab.GetComponent<CharacterController>().velocity;
            rb.AddForce(camTransform.forward * dropForwardForce, ForceMode.Impulse);
            rb.AddForce(camTransform.up * dropUpwardForce, ForceMode.Impulse);

            if (rotationHandler != null)
            {
                rotationHandler.enabled = true;
            }

            OnUnequip();
        }

        public void OnEquip()
        {
            // Weapon-specific equip logic
        }

        public void OnUnequip()
        {
            // Weapon-specific unequip logic
        }
    }
}