using GlitchHunter.Constant;
using GlitchHunter.Manager;
using UnityEngine;

namespace GlitchHunter.Handler
{
    public class CollectableHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask collectLayer;

        private bool isActivePrompt = false;

        private void Update()
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, collectLayer))
            {
                Key key = hit.collider.GetComponent<Key>();
                if(key != null)
                {
                    //check distance before active
                    if (!isActivePrompt)
                    {
                        isActivePrompt = true;
                        GlitchHunterConstant.OnShowPrompt?.Invoke(true, "Press F To Collect");
                    }
                    
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        key.OnCollectKey();
                        GlitchHunterConstant.OnShowPrompt?.Invoke(false, "");
                        isActivePrompt = false;
                    }
                }

                //Weapon weapon = hit.collider.GetComponent<Weapon>();

                //if(weapon != null)
                //{
                //    if (!isActivePrompt)
                //    {
                //        isActivePrompt = true;
                //        GlitchHunterConstant.OnShowPrompt?.Invoke(true, "Press F To Collect");
                //    }

                //    if (Input.GetKeyDown(KeyCode.F))
                //    {
                //       // weapon.OnCollectWeapon();
                //        GlitchHunterConstant.OnShowPrompt?.Invoke(false, "");
                //        isActivePrompt = false;
                //    }
                //}
            }
        }
    }
}
