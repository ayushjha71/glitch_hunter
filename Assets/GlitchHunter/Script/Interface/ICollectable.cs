using UnityEngine;

namespace GlitchHunter.Interface
{
    public interface ICollectable
    {
        void OnPickup(GameObject collector);
        void OnDrop();
        bool CanBePickedUp(GameObject collector);
        string GetPromptMessage();
    }
}
