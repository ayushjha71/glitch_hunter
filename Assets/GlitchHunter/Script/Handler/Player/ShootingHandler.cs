using StarterAssets;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GlitchHunter.Handler.Enemy;
using GlitchHunter.Constant;
using GlitchHunter.Manager;
using Unity.Cinemachine;

namespace GlitchHunter.Handler
{
    public class ShootingHandler : MonoBehaviour
    {
        [SerializeField]
        private AudioClip fireAudioClip;
        [SerializeField]
        private Crosshair crossHair;
        public Transform gunContainerTransform;
        [Header("Weapon Settings")]
        [Tooltip("Damage per shot")]
        public int damage = 50;
        [Tooltip("Range of the weapon")]
        public float range = 200f;
        [Tooltip("Time between shots in seconds")]
        public float fireRate = 0.1f;
        [Tooltip("Maximum ammo capacity")]
        public int maxAmmo = 30;
        [Tooltip("Current ammo count")]
        public int currentAmmo;
        [Tooltip("Time to reload in seconds")]
        public float reloadTime = 1f;
        [Tooltip("Layer mask for shootable objects")]
        public LayerMask shootableMask;

        [Header("Zoom Settings")]
        [Tooltip("Zoom field of view")]
        public float zoomFOV = 30f;
        [Tooltip("Zoom speed")]
        public float zoomSpeed = 10f;
        [Tooltip("Normal field of view")]
        public float normalFOV = 60f;

        [Header("Effects")]
        [Tooltip("Muzzle flash particle effect")]
        public ParticleSystem muzzleFlash;
        [SerializeField]
        private Transform muzzleFlashTransform;
        [Tooltip("Impact effect prefab")]
        public GameObject impactEffect;
        [SerializeField]
        private CinemachineCamera playerFollowCamera;

        // Private variables
        private float _nextTimeToFire = 0f;
        private bool _isReloading = false;
        private bool _isZoomed = false;
        private bool canShoot = false;

        private Camera _playerCamera;
        private Coroutine _reloadCoroutine;

        private void Awake()
        {
            _playerCamera = Camera.main;
            currentAmmo = maxAmmo;
        }

        private void OnEnable()
        {
            _isReloading = false;
            GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmo, maxAmmo);
            GlitchHunterConstant.OnUpdateReloadStatus?.Invoke("Attack");
            GlitchHunterConstant.OnShowPlayerUI += CanStartShooting;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnShowPlayerUI -= CanStartShooting;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !_isReloading)
            {
                StartReload();
            }

            HandleZoom();
            HandleShooting();
        }

        private void CanStartShooting(bool isShoot)
        {
            canShoot = isShoot;
        }

        private void StartReload()
        {
            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
            }
            _reloadCoroutine = StartCoroutine(Reload());
        }

        private void HandleShooting()
        {
            if (_isReloading)
                return;
            // Auto fire when holding mouse button
            if (Input.GetKey(KeyCode.Mouse0) && Time.time >= _nextTimeToFire && canShoot && GameManager.Instance.IsEquipped)
            {
                _nextTimeToFire = Time.time + fireRate;
                Shoot();

            }
        }
        
        private void Shoot()
        {
            crossHair.SetScale(CrosshairScale.Shoot, 1);
            if (currentAmmo <= 0)
            {
                if (!_isReloading)
                {
                    StartReload();
                }
                return;
            }

            // Use ammo
            currentAmmo--;
            GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmo, maxAmmo);

            // Play muzzle flash
            if (muzzleFlash != null)
            {
                Instantiate(muzzleFlash, muzzleFlashTransform.position, muzzleFlashTransform.rotation);
                muzzleFlash.Play();
                GameManager.Instance.AudioSource.PlayOneShot(fireAudioClip);
            }

            // Raycast
            if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward,
                out RaycastHit hit, range, shootableMask))
            {
                hit.transform.GetComponent<EnemyHealthHandler>()?.TakeDamage(damage);
                Debug.Log("Hit" + hit.transform.gameObject.name);

                // Play impact effect
                if (impactEffect != null)
                {
                    GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 2f);
                }
            }
        }

        private IEnumerator Reload()
        {
            _isReloading = true;
            crossHair.SetScale(CrosshairScale.Default, 1);
            // Show reload UI
            GlitchHunterConstant.OnUpdateReloadStatus?.Invoke("Reloading...");
            float elapsedTime = 0f;

            while (elapsedTime < reloadTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / reloadTime;
                GlitchHunterConstant.OnReloadSliderValue?.Invoke(progress);
                yield return null;
            }

            // Complete reload
            currentAmmo = maxAmmo;
            _isReloading = false;
            GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmo, maxAmmo);
            GlitchHunterConstant.OnUpdateReloadStatus?.Invoke("Attack");
        }

        private void HandleZoom()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                _isZoomed = true;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                _isZoomed = false;
            }

            // Smooth zoom transition
            float targetFOV = _isZoomed ? zoomFOV : normalFOV;
            playerFollowCamera.Lens.FieldOfView = Mathf.Lerp(playerFollowCamera.Lens.FieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
        }
    }
}