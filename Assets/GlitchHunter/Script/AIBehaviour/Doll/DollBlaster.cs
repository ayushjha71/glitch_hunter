using UnityEngine;
using System.Collections;
using GlitchHunter.Handler;

namespace GlitchHunter.Script.DollMechanic
{
    public class DollBlaster : MonoBehaviour
    {
        [SerializeField] private DollDataContainer dataContainer;
        [SerializeField] private AudioSource audioSource;

        private bool _isExploded;
        private DollController _dollController;

        private void OnEnable()
        {
            _dollController = GetComponentInParent<DollController>();
            audioSource = GetComponent<AudioSource>();
            _dollController.OnExplode += OnExplode;
        }

        private void OnDisable()
        {
            _dollController.OnExplode -= OnExplode;
        }

        private void OnExplode(Vector3 target)
        {
            var distance = Vector3.Distance(transform.position, target);
            StartCoroutine(StartExplosion());
        }

        private IEnumerator StartExplosion()
        {
            _isExploded = true;

            yield return new WaitForSeconds(dataContainer.explosionDelay);

            //TODO TestingPurpose need to be removed!!
            //VisualExplosionRadius();
          
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, dataContainer.explosionRadius);
            foreach (var hit in hitColliders)
            {
                audioSource.PlayOneShot(dataContainer.explosionSound);

                var health = hit.GetComponent<PlayerHealthHandler>();
                if (health != null)
                {
                    health.TakeDamage(50); 
                    Debug.Log("Player hit with " + dataContainer.explosionDamage + " damage!");
                }
            }
        }

        private void VisualExplosionRadius()
        {
            GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.transform.position = transform.position;
            debugSphere.transform.localScale = Vector3.one * (dataContainer.explosionRadius * 2); // Diameter
            debugSphere.GetComponent<Collider>().enabled = false;
            debugSphere.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.1f);
            Destroy(debugSphere, 2f);
        }
    }
}