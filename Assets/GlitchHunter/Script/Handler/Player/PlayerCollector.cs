using GlitchHunter.Constant;
using GlitchHunter.Interface;
using UnityEngine;

namespace GlitchHunter.Handler.Player
{
    public class PlayerCollector : MonoBehaviour
    {
        private bool IsFInput = false;
        private bool IsQInput = false;
        private ICollectable currentEquippedItem;

        private void OnEnable()
        {
            GlitchHunterConstant.OnFKeyInputGet += OnReceivedFInput;
            GlitchHunterConstant.OnQKeyInputGet += OnReceivedQInput;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnFKeyInputGet -= OnReceivedFInput;
            GlitchHunterConstant.OnQKeyInputGet -= OnReceivedQInput;
        }

        private void Update()
        {
            if (IsFInput)
            {
                TryCollectItems();
                IsFInput = false; // Reset input
            }

            if (IsQInput && currentEquippedItem != null)
            {
                TryDropItems();
                IsQInput = false; // Reset input
            }
        }

        private void TryCollectItems()
        {
            Collider[] nearbyItems = Physics.OverlapSphere(transform.position, 5f);

            foreach (Collider item in nearbyItems)
            {
                ICollectable collectable = item.GetComponent<ICollectable>();
                if (collectable != null && collectable.CanBePickedUp(gameObject))
                {
                    collectable.OnPickup(gameObject);
                    currentEquippedItem = collectable;
                    break;
                }
            }
        }

        private void TryDropItems()
        {
            if (currentEquippedItem != null)
            {
                currentEquippedItem.OnDrop();
                currentEquippedItem = null;
            }
        }

        private void OnReceivedFInput(bool isReceived)
        {
            IsFInput = isReceived;
        }

        private void OnReceivedQInput(bool isReceived)
        {
            IsQInput = isReceived;
        }
    }
}