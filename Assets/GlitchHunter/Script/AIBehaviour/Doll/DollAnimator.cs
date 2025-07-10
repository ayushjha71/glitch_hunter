using UnityEngine;

namespace GlitchHunter.Script.DollMechanic
{
    public class DollAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private DollDataContainer dataContainer;

        private DollController _dollController;
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int Explode = Animator.StringToHash("Explode");

        private AudioSource _audioSource;
        private void OnEnable()
        {
            animator = GetComponent<Animator>();
            _audioSource = GetComponentInParent<AudioSource>();
            _dollController = GetComponentInParent<DollController>();
            
            _dollController.OnMove += OnMove;
            _dollController.OnExplode += OnExplodeAnim;
        }

        private void OnDisable()
        {
            _dollController.OnMove -= OnMove;
            _dollController.OnExplode -= OnExplodeAnim;
        }

        private void OnExplodeAnim(Vector3 target)
        {
            animator.SetTrigger(Explode);
        }

        private void OnMove(float speed)
        {
            animator.SetFloat(MoveSpeed, speed);
        }

        public void OnExplode()
        {
            Destroy(GetComponentInParent<DollController>().gameObject);
        }
        
        public void HandleExplosionVFX()
        {
            // VFX, damage, destroy etc.
            var vfx = Instantiate(dataContainer.explosionEffect, transform.position  + new Vector3(-1,1,0), Quaternion.identity);
            if (vfx != null)
            {
                vfx.GetComponent<ParticleSystem>().Play();
            }
        }
        // public void PlayFootStep()
        // {
        //     var randomFootSteps = Random.Range(0, dataContainer.footsteps.Length);
        //     _audioSource.PlayOneShot(dataContainer.footsteps[randomFootSteps]);
        // }
    }
}