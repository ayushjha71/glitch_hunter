using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using GlitchHunter.Manager;
using GlitchHunter.Constant;
using GlitchHunter.Handler.Enemy;

namespace GlitchHunter.Handler
{
    public class ShootingHandler : MonoBehaviour
    {
        [SerializeField]
        private AudioClip fireAudioClip;
        [SerializeField]
        private Crosshair crossHair;
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

        // Private variables
        private float _nextTimeToFire = 0f;
        private bool _isReloading = false;
        private bool _isZoomed = false;
        private bool canShoot = false;
        private bool canDestroy = false;

        //Input 
        private bool mShootInput = false;
        private bool mReloadInput = false;
        private bool mZoomInput = false;

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
            GlitchHunterConstant.OnShootingInput += OnShootingInputReceived;
            GlitchHunterConstant.OnWeaponReloadInput += OnReloadInputReceived;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnShowPlayerUI -= CanStartShooting;
            GlitchHunterConstant.OnShootingInput -= OnShootingInputReceived;
            GlitchHunterConstant.OnWeaponReloadInput -= OnReloadInputReceived;
        }

        private void Update()
        {
            // Only block shooting during melee combat, not during flying or movement
            if (GameManager.Instance.IsMeleeCombatStarted)
            {
                return;
            }

            if (mReloadInput && currentAmmo < maxAmmo && !_isReloading)
            {
                StartReload();
            }

            HandleZoom();
            HandleShooting();
        }

        private void OnShootingInputReceived(bool shootInput)
        {
            mShootInput = shootInput;
        }

        private void OnReloadInputReceived(bool reloadInput)
        {
            mReloadInput = reloadInput;
        }

        private void OnZoomInput(bool zoomInput)
        {
            mZoomInput = zoomInput;
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
            // Only check for essential blocking conditions
            // Removed GameManager.Instance.IsEquipped check to allow shooting while flying/moving
            if (_isReloading || !canShoot || !GameManager.Instance.IsEquipped)
                return;

            // Auto fire when holding mouse button
            if (mShootInput && Time.time >= _nextTimeToFire)
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
            if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out RaycastHit hit, range, shootableMask))
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
            // Smooth zoom transition
            float targetFOV = mZoomInput ? zoomFOV : normalFOV;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
        }
    }
}