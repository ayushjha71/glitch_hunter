using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GlitchHunter.Constant;

namespace GlitchHunter.UI
{
    public class InGameUIHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject hitImpactPanel;

        [SerializeField] private CanvasGroup promtPanel;
        [SerializeField] private TMP_Text promtText;

        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text reloadStatusText;
        [SerializeField] private Slider reloadProgressSlider;
        [SerializeField] private Slider healthProgressSlider;
        [SerializeField] private CanvasGroup playerUICanvasGroup;

        private Coroutine HitImpactCorutine;

        private void OnEnable()
        {
            GlitchHunterConstant.OnUpdateHealthSlider += HealthProgressSlider;
            GlitchHunterConstant.OnUpdateAmmoUI += UpdateAmmoUI;
            GlitchHunterConstant.OnUpdateReloadStatus += ReloadStatusText;
            GlitchHunterConstant.OnReloadSliderValue += ReloadSlider;
            GlitchHunterConstant.OnShowPlayerUI += ShowPlayerUI;
            GlitchHunterConstant.OnShowPrompt += OnShowPromptPanel; 
           // GlitchHunterConstant.OnPlayerHitImpact += OnPlyerHitImpact;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnUpdateHealthSlider -= HealthProgressSlider;
            GlitchHunterConstant.OnUpdateAmmoUI -= UpdateAmmoUI;
            GlitchHunterConstant.OnUpdateReloadStatus -= ReloadStatusText;
            GlitchHunterConstant.OnReloadSliderValue -= ReloadSlider;
            GlitchHunterConstant.OnShowPlayerUI -= ShowPlayerUI;
            GlitchHunterConstant.OnShowPrompt -= OnShowPromptPanel;
            //GlitchHunterConstant.OnPlayerHitImpact -= OnPlyerHitImpact;
        }

        private void ShowPlayerUI(bool isActive)
        {
            GlitchHunterConstant.FadeIn(playerUICanvasGroup, 1, 0.3f, null);
        }

        private void HealthProgressSlider(float currentHealth)
        {
            healthProgressSlider.maxValue = 1000;
            healthProgressSlider.value = currentHealth;
        }

        private void ReloadSlider(float progress)
        {
            reloadProgressSlider.value = progress;
        }

        private void ReloadStatusText(string msg)
        {
            reloadStatusText?.SetText(msg);
        }

        private void OnShowPromptPanel(string msg)
        {
            StartCoroutine(ShowPromtPanelCorutine(msg));
        }

        private void OnPlyerHitImpact()
        {
           // StopCoroutine(HitImpactCorutine);
            HitImpactCorutine = StartCoroutine(ShowHitImpact());
        }

        private IEnumerator ShowPromtPanelCorutine(string msg)
        {
            GlitchHunterConstant.FadeIn(promtPanel, 1, 0.3f, null);
            promtText?.SetText(msg);
            yield return new WaitForSeconds(3);
            GlitchHunterConstant.FadeOut(promtPanel, 0, 0.3f, null);
        }

        private IEnumerator ShowHitImpact()
        {
           // StopCoroutine(HitImpactCorutine);
            hitImpactPanel.SetActive(true);
            yield return new WaitForSeconds(0.05f);
            hitImpactPanel.SetActive(false);
        }

        private void UpdateAmmoUI(int currentAmmo, int maxAmmo)
        {
            if (ammoText != null)
            {
                ammoText.SetText($"{currentAmmo} / {maxAmmo}");
                reloadProgressSlider.value = (float)currentAmmo;
            }
        }
    }
}
