using GlitchHunter.ABase;
using GlitchHunter.Constant;
using GlitchHunter.Handler;
using UnityEngine;

namespace GlitchHunter.Collectable
{
    public class CollectableAmmo : ABaseCollectable
    {
        [SerializeField] private int ammoAmount = 30;

        public override void OnDrop()
        {
            // Not needed for ammo pickup
        }

        public override void OnPickup(GameObject collector)
        {
            GlitchHunterConstant.OnAddAmmo?.Invoke(ammoAmount);
            Destroy(gameObject);
        }

        public override bool CanBePickedUp(GameObject collector)
        {
            var shooter = collector.GetComponent<ShootingHandler>();
            return base.CanBePickedUp(collector) &&
                   shooter != null &&
                   shooter.currentAmmoInventory < shooter.maxAmmoInventory;
        }

        public override string GetPromptMessage()
        {
            return promptMessage;
        }
    }
}