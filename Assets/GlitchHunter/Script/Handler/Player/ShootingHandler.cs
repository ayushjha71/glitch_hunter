using UnityEngine;
using System.Collections;
using GlitchHunter.Manager;
using GlitchHunter.Constant;
using GlitchHunter.Interface;

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

        private UnityEngine.Camera _playerCamera;
        private Coroutine _reloadCoroutine;

        [Header("Ammo Inventory")]
        [Tooltip("Maximum ammo player can carry")]
        public int maxAmmoInventory = 120;
        [Tooltip("Current ammo in inventory")]
        public int currentAmmoInventory = 30;

        private void Awake()
        {
            _playerCamera = UnityEngine.Camera.main;
            currentAmmo = maxAmmo;
            // Initialize UI - show magazine ammo (current/max) and total inventory
          //  GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmo, maxAmmo);
            GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmoInventory, maxAmmoInventory);
        }

        private void OnEnable()
        {
            _isReloading = false;
            // Update both UIs when enabled
          //  GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmo, maxAmmo);
            GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmoInventory, maxAmmoInventory);
            GlitchHunterConstant.OnUpdateReloadStatus?.Invoke("Attack");
            GlitchHunterConstant.OnShowPlayerUI += CanStartShooting;
            GlitchHunterConstant.OnShootingInput += OnShootingInputReceived;
            GlitchHunterConstant.OnWeaponReloadInput += OnReloadInputReceived;
            GlitchHunterConstant.OnZoomInput += OnZoomInput;
            GlitchHunterConstant.OnAddAmmo += AddAmmoToInventory;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnShowPlayerUI -= CanStartShooting;
            GlitchHunterConstant.OnShootingInput -= OnShootingInputReceived;
            GlitchHunterConstant.OnWeaponReloadInput -= OnReloadInputReceived;
            GlitchHunterConstant.OnZoomInput -= OnZoomInput;
            GlitchHunterConstant.OnAddAmmo -= AddAmmoToInventory;
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

        private void UpdateAmmoUI()
        {
            // Shows: Current Magazine / Total Inventory
            GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmo, currentAmmoInventory);
        }

        private void StartReload()
        {
            // Only reload if we have ammo in inventory and magazine isn't full
            if (_isReloading || currentAmmo == maxAmmo || currentAmmoInventory <= 0)
                return;

            if (_reloadCoroutine != null)
                StopCoroutine(_reloadCoroutine);

            _reloadCoroutine = StartCoroutine(Reload());
        }

        private IEnumerator Reload()
        {
            _isReloading = true;
            GlitchHunterConstant.OnUpdateReloadStatus?.Invoke("Reloading...");

            float elapsedTime = 0f;
            while (elapsedTime < reloadTime)
            {
                elapsedTime += Time.deltaTime;
                GlitchHunterConstant.OnReloadSliderValue?.Invoke(elapsedTime / reloadTime);
                yield return null;
            }

            // Calculate ammo to transfer from inventory to magazine
            int ammoNeeded = maxAmmo - currentAmmo;
            int ammoToTransfer = Mathf.Min(ammoNeeded, currentAmmoInventory);

            currentAmmo += ammoToTransfer;
            currentAmmoInventory -= ammoToTransfer;

            _isReloading = false;
            UpdateAmmoUI();
            GlitchHunterConstant.OnUpdateReloadStatus?.Invoke("Attack");
        }

        private void AddAmmoToInventory(int amount)
        {
            // Only add if we have space
            if (currentAmmoInventory < maxAmmoInventory)
            {
                currentAmmoInventory = Mathf.Min(currentAmmoInventory + amount, maxAmmoInventory);
                UpdateAmmoUI();
            }
        }


        private void HandleShooting()
        {
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
            if (currentAmmo <= 0)
            {
                if (!_isReloading && currentAmmoInventory > 0)
                    StartReload();
                return;
            }

            currentAmmo--;
            UpdateAmmoUI();

            //// Use ammo
            //currentAmmo--;
            //// Update magazine ammo UI
            //GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(currentAmmo, maxAmmo);

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
                hit.transform.GetComponent<IDamageable>()?.Damage(damage);
                Debug.Log("Hit" + hit.transform.gameObject.name);

                // Play impact effect
                if (impactEffect != null)
                {
                    GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 2f);
                }
            }
        }

        private void HandleZoom()
        {
            // Smooth zoom transition
            float targetFOV = mZoomInput ? zoomFOV : normalFOV;
            UnityEngine.Camera.main.fieldOfView = Mathf.Lerp(UnityEngine.Camera.main.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
        }
    }
}