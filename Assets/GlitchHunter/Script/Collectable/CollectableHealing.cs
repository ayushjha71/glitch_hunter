using GlitchHunter.ABase;
using GlitchHunter.Handler;
using GlitchHunter.Interface;
using UnityEngine;

namespace GlitchHunter.Collectable
{
    public class CollectableHealing : ABaseCollectable, IConsumable
    {
        [SerializeField] private int healAmount = 25;

        public override void OnPickup(GameObject collector)
        {
            Consume(collector);
        }

        public override void OnDrop()
        {
            // Healing items are consumed immediately
        }

        public void Consume(GameObject consumer)
        {
            PlayerHealthHandler healthSystem = consumer.GetComponent<PlayerHealthHandler>();
            if (healthSystem != null)
            {
                healthSystem.Heal(healAmount);
                Destroy(gameObject);
            }
        }

        public override bool CanBePickedUp(GameObject collector)
        {
            PlayerHealthHandler healthSystem = collector.GetComponent<PlayerHealthHandler>();
            return base.CanBePickedUp(collector) && healthSystem.CanHeal();
        }
    }
}