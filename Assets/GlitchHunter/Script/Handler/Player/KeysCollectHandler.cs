using GlitchHunter.Constant;
using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class KeysCollectHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask collectLayer;
        [SerializeField] private float pickUpRange = 5;

        private bool isActivePrompt = false;

        private void Update()
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, collectLayer))
            {
                Vector3 distanceToPlayer = GameManager.Instance.PlayerPrefab.transform.position - transform.position;

                if(distanceToPlayer.magnitude >= pickUpRange)
                {
                    return;
                }

                Key key = hit.collider.GetComponent<Key>();
                if (key != null)
                {
                    //check distance before active
                    if (!isActivePrompt)
                    {
                        isActivePrompt = true;
                        GlitchHunterConstant.OnShowPrompt?.Invoke("Press F To Collect");
                    }

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        key.OnCollectKey();
                        GlitchHunterConstant.OnShowPrompt?.Invoke("");
                        isActivePrompt = false;
                    }
                }
            }
        }
    }
}
