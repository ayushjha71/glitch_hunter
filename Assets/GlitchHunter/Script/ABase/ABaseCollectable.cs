using GlitchHunter.Constant;
using GlitchHunter.Interface;
using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.ABase
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public abstract class ABaseCollectable : MonoBehaviour, ICollectable
    {
        [SerializeField] protected float pickupRange = 5f;
        [SerializeField] protected string promptMessage = "Press F to pick up";

        protected Rigidbody rb;
        protected Collider col;
        protected bool isPickedUp = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }

        public virtual bool CanBePickedUp(GameObject collector)
        {
            float distance = Vector3.Distance(collector.transform.position, transform.position);
            return distance <= pickupRange && !isPickedUp;
        }

        public virtual string GetPromptMessage()
        {
            return promptMessage;
        }

        public abstract void OnPickup(GameObject collector);

        public abstract void OnDrop();

        protected virtual void Update()
        {
            if (!GameManager.Instance.IsGameStarted) return;

            if (CanBePickedUp(GameManager.Instance.PlayerPrefab))
            {
                GlitchHunterConstant.OnShowPrompt?.Invoke(GetPromptMessage());
            }
        }
    }
}