//using UnityEngine;
//using System.Collections;
//using GlitchHunter.Handler;
//using GlitchHunter.Constant;
//using StarterAssets;
//using TMPro;
//using Unity.VisualScripting;

//namespace GlitchHunter.Manager
//{
//    [System.Serializable]
//    public class WeaponUpgradeData
//    {
//        [Header("Weapon Stats")]
//        public string weaponName;
//        public GameObject weaponMesh;
//        public int maxAmmo;
//        public int damage;
//        public float fireRate;
//        public float range;

//        [Header("Effects")]
//        public GameObject upgradeParticleEffect;
//        public AudioClip upgradeSound;
//       // public Material weaponMaterial; // Optional: Different material for upgraded weapon
//    }

//    public class WeaponUpgradeManager : MonoBehaviour
//    {
//        [Header("Upgrade Settings")]
//        [Tooltip("Time before wave start to trigger upgrade")]
//        public float upgradeTimeBeforeWave = 5f;
//        [Tooltip("Duration of upgrade process")]
//        public float upgradeDuration = 3f;

//        [Header("Weapon Upgrades")]
//        public WeaponUpgradeData[] weaponUpgrades;

//        private ShootingHandler shootingHandler;
//        private FirstPersonController playerController;

//        //[Header("UI Elements")]
//        //public TMP_Text upgradeMessageText;
//        //public GameObject upgradeProgressBar;
//        //public UnityEngine.UI.Slider upgradeSlider;

//        [Header("Effects")]
//        public ParticleSystem upgradeParticleSystem;
//        public AudioSource audioSource;
//        [SerializeField]
//        private LayerMask originalPlayerLayer;


//        //public GameObject upgradeScreenEffect; // Optional: Screen overlay during upgrade

//        // Private variables
//        [SerializeField]
//        private EnemySpawnManager enemySpawnManager;
//        private int currentWeaponLevel = 0;
//        private bool[] waveUpgradeTriggered;
//        private bool isUpgrading = false;
//        private Coroutine upgradeCoroutine;

//        // Original player stats (to restore after upgrade)
//        private bool originalCanMove;

//        void Start()
//        {

//            if (enemySpawnManager == null)
//            {
//                Debug.LogError("EnemySpawnManager not found! WeaponUpgradeManager requires it.");
//                return;
//            }

//            // Initialize upgrade tracking
//            waveUpgradeTriggered = new bool[enemySpawnManager.enemyData.Count];

//            // Hide upgrade UI initially
//            //if (upgradeMessageText != null)
//            //    upgradeMessageText.gameObject.SetActive(false);
//            //if (upgradeProgressBar != null)
//            //    upgradeProgressBar.SetActive(false);
//            //if (upgradeScreenEffect != null)
//            //    upgradeScreenEffect.SetActive(false);

//        }

//        void Update()
//        {
//            if (!GameManager.Instance.IsGameStarted || isUpgrading)
//                return;

//            CheckForUpgrades();
//        }

//        void CheckForUpgrades()
//        {
//            for (int i = 0; i < enemySpawnManager.enemyData.Count; i++)
//            {
//                // Skip if upgrade already triggered for this wave
//                if (waveUpgradeTriggered[i])
//                    continue;

//                var waveData = enemySpawnManager.enemyData[i];
//                float timeUntilWave = waveData.WaveStartTime - GameManager.Instance.GameTime;

//                // Trigger upgrade if we're within the upgrade time window
//                if (timeUntilWave <= upgradeTimeBeforeWave && timeUntilWave > 0)
//                {
//                    waveUpgradeTriggered[i] = true;
//                    StartWeaponUpgrade(i);
//                    break; // Only upgrade once per frame
//                }
//            }
//        }

//        void StartWeaponUpgrade(int waveIndex)
//        {
//            if (currentWeaponLevel < weaponUpgrades.Length)
//            {
//                Debug.Log($"Starting weapon upgrade for Wave {waveIndex + 1}");
//                upgradeCoroutine = StartCoroutine(PerformWeaponUpgrade(waveIndex));
//            }
//        }

//        IEnumerator PerformWeaponUpgrade(int waveIndex)
//        {
//            isUpgrading = true;

//            // Disable player movement and shooting
//            DisablePlayerControls();

//            // Make player invincible
//          //  MakePlayerInvincible(true);

//            // Show upgrade UI
//          //  ShowUpgradeUI($"Weapon Upgrade Incoming for Wave {waveIndex + 1}!");

//            // Play upgrade sound
//            if (audioSource != null && weaponUpgrades[currentWeaponLevel].upgradeSound != null)
//            {
//                audioSource.PlayOneShot(weaponUpgrades[currentWeaponLevel].upgradeSound);
//            }

//            // Start upgrade particle effects
//            if (upgradeParticleSystem != null)
//            {
//                upgradeParticleSystem.Play();
//            }

//            // Show screen effect
//            //if (upgradeScreenEffect != null)
//            //{
//            //    upgradeScreenEffect.SetActive(true);
//            //}

//            // Upgrade progress animation
//            float elapsedTime = 0f;
//            while (elapsedTime < upgradeDuration)
//            {
//                elapsedTime += Time.deltaTime;
//                float progress = elapsedTime / upgradeDuration;

//                // Update progress bar
//                //if (upgradeSlider != null)
//                //{
//                //    upgradeSlider.value = progress;
//                //}

//                //// Update message with progress
//                //if (upgradeMessageText != null)
//                //{
//                //    upgradeMessageText.text = $"Upgrading Weapon... {(progress * 100):F0}%";
//                //}

//                yield return null;
//            }

//            // Apply weapon upgrade
//            ApplyWeaponUpgrade();

//            // Show completion message
//            //if (upgradeMessageText != null)
//            //{
//            //    upgradeMessageText.text = $"Weapon Upgraded to {weaponUpgrades[currentWeaponLevel].weaponName}!";
//            //}

//            // Wait a moment to show completion
//            yield return new WaitForSeconds(1f);

//            // Hide upgrade UI
//           // HideUpgradeUI();

//            // Hide screen effect
//            //if (upgradeScreenEffect != null)
//            //{
//            //    upgradeScreenEffect.SetActive(false);
//            //}

//            // Stop particle effects
//            if (upgradeParticleSystem != null)
//            {
//                upgradeParticleSystem.Stop();
//            }

//            // Re-enable player controls
//            EnablePlayerControls();

//            // Remove invincibility
//          //  MakePlayerInvincible(false);

//            isUpgrading = false;

//            Debug.Log($"Weapon upgrade completed! Now using {weaponUpgrades[currentWeaponLevel].weaponName}");
//        }

//        void ApplyWeaponUpgrade()
//        {
//            if (currentWeaponLevel >= weaponUpgrades.Length)
//            {
//                Debug.LogWarning("No more weapon upgrades available!");
//                return;
//            }

//            WeaponUpgradeData upgrade = weaponUpgrades[currentWeaponLevel];

//            if (shootingHandler == null)
//            {
//                shootingHandler = GameManager.Instance.PlayerPrefab.GetComponent<ShootingHandler>();
//            }

//            // Update weapon mesh
//            if (upgrade.weaponMesh != null && shootingHandler.gunContainerTransform != null)
//            {
//                // Destroy old weapon mesh
//                GameObject obj =  shootingHandler.gunContainerTransform.GetChild(1).gameObject;
//                //foreach (Transform child in shootingHandler.gunContainerTransform)
//                //{
//                //    if (child.gameObject.activeSelf)
//                //    {
//                //        child.transform.GetChild(1)
//                       Destroy(obj);
//                //    }
//                //}

//                // Instantiate new weapon mesh
//                GameObject newWeapon = Instantiate(upgrade.weaponMesh, shootingHandler.gunContainerTransform);
//                newWeapon.transform.localPosition = Vector3.zero;
//                newWeapon.transform.localRotation = Quaternion.identity;

//                // Apply material if provided
//                //if (upgrade.weaponMaterial != null)
//                //{
//                //    Renderer weaponRenderer = newWeapon.GetComponent<Renderer>();
//                //    if (weaponRenderer != null)
//                //    {
//                //        weaponRenderer.material = upgrade.weaponMaterial;
//                //    }
//                //}
//            }

//            // Update weapon stats
//            shootingHandler.maxAmmo = upgrade.maxAmmo;
//            shootingHandler.currentAmmo = upgrade.maxAmmo; // Give full ammo on upgrade
//            shootingHandler.damage = upgrade.damage;
//            shootingHandler.fireRate = upgrade.fireRate;
//            shootingHandler.range = upgrade.range;

//            // Update UI
//            GlitchHunterConstant.OnUpdateAmmoUI?.Invoke(shootingHandler.currentAmmo, shootingHandler.maxAmmo);

//            // Spawn upgrade particle effect at weapon position
//            if (upgrade.upgradeParticleEffect != null && shootingHandler.gunContainerTransform != null)
//            {
//                GameObject particles = Instantiate(upgrade.upgradeParticleEffect,
//                    shootingHandler.gunContainerTransform.position,
//                    shootingHandler.gunContainerTransform.rotation);
//                Destroy(particles, 5f);
//            }

//            currentWeaponLevel++;
//        }

//        void DisablePlayerControls()
//        {
//            // Disable movement
//            if(playerController == null)
//            {
//                playerController = GameManager.Instance.PlayerPrefab.GetComponent<FirstPersonController>();
//            }
//            if (playerController != null)
//            {
//                originalCanMove = playerController.enabled;
//                playerController.enabled = false;
//            }

//            // Disable shooting by setting canShoot to false
//            GlitchHunterConstant.OnShowPlayerUI?.Invoke(false);

//            Debug.Log("Player controls disabled for upgrade");
//        }

//        void EnablePlayerControls()
//        {
//            if (playerController == null)
//            {
//                playerController = GameManager.Instance.PlayerPrefab.GetComponent<FirstPersonController>();
//            }
//            // Re-enable movement
//            if (playerController != null)
//            {
//                playerController.enabled = originalCanMove;
//            }

//            // Re-enable shooting
//            GlitchHunterConstant.OnShowPlayerUI?.Invoke(true);

//            Debug.Log("Player controls enabled after upgrade");
//        }

//        //void MakePlayerInvincible(bool invincible)
//        //{
//        //    if (playerObject == null) return;

//        //    if (invincible)
//        //    {
//        //        // Move player to invincibility layer
//        //        SetLayerRecursively(playerObject, invincibilityLayer);
//        //        Debug.Log("Player is now invincible during upgrade");
//        //    }
//        //    else
//        //    {
//        //        // Restore original layer
//        //        SetLayerRecursively(playerObject, originalPlayerLayer);
//        //        Debug.Log("Player invincibility removed");
//        //    }
//        //}

//        //void ShowUpgradeUI(string message)
//        //{
//        //    if (upgradeMessageText != null)
//        //    {
//        //        upgradeMessageText.text = message;
//        //        upgradeMessageText.gameObject.SetActive(true);
//        //    }

//        //    if (upgradeProgressBar != null)
//        //    {
//        //        upgradeProgressBar.SetActive(true);
//        //    }

//        //    if (upgradeSlider != null)
//        //    {
//        //        upgradeSlider.value = 0f;
//        //    }
//        //}

//        //void HideUpgradeUI()
//        //{
//        //    if (upgradeMessageText != null)
//        //    {
//        //        upgradeMessageText.gameObject.SetActive(false);
//        //    }

//        //    if (upgradeProgressBar != null)
//        //    {
//        //        upgradeProgressBar.SetActive(false);
//        //    }
//        //}

//        // Public methods for external access
//        public bool IsUpgrading()
//        {
//            return isUpgrading;
//        }

//        public int GetCurrentWeaponLevel()
//        {
//            return currentWeaponLevel;
//        }

//        public string GetCurrentWeaponName()
//        {
//            if (currentWeaponLevel > 0 && currentWeaponLevel <= weaponUpgrades.Length)
//            {
//                return weaponUpgrades[currentWeaponLevel - 1].weaponName;
//            }
//            return "Basic Weapon";
//        }

//        // Force upgrade (for testing)
//        [ContextMenu("Force Upgrade")]
//        public void ForceUpgrade()
//        {
//            if (!isUpgrading && currentWeaponLevel < weaponUpgrades.Length)
//            {
//                StartCoroutine(PerformWeaponUpgrade(0));
//            }
//        }

//        void OnDestroy()
//        {
//            // Clean up coroutine if object is destroyed
//            if (upgradeCoroutine != null)
//            {
//                StopCoroutine(upgradeCoroutine);
//            }
//        }

//        void OnGUI()
//        {
//            // Debug information
//            GUI.Label(new Rect(10, 200, 300, 20), $"Current Weapon: {GetCurrentWeaponName()}");
//            GUI.Label(new Rect(10, 220, 300, 20), $"Weapon Level: {currentWeaponLevel}/{weaponUpgrades.Length}");
//            GUI.Label(new Rect(10, 240, 300, 20), $"Is Upgrading: {isUpgrading}");
//        }
//    }
//}