using GlitchHunter.Constant;
using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GlitchHunter.UI
{
    public class InGameUIHandler : MonoBehaviour
    {
        [Header("UI References")]

        [SerializeField] private CanvasGroup promtPanel;
        [SerializeField] private TMP_Text promtText;

        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text reloadStatusText;
        [SerializeField] private Slider reloadProgressSlider;
        [SerializeField] private Slider healthProgressSlider;
        [SerializeField] private CanvasGroup playerUICanvasGroup;

        private void OnEnable()
        {
            GlitchHunterConstant.OnUpdateHealthSlider += HealthProgressSlider;
            GlitchHunterConstant.OnUpdateAmmoUI += UpdateAmmoUI;
            GlitchHunterConstant.OnUpdateReloadStatus += ReloadStatusText;
            GlitchHunterConstant.OnReloadSliderValue += ReloadSlider;
            GlitchHunterConstant.OnShowPlayerUI += ShowPlayerUI;
            GlitchHunterConstant.OnShowPrompt += OnShowPromptPanel;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnUpdateHealthSlider -= HealthProgressSlider;
            GlitchHunterConstant.OnUpdateAmmoUI -= UpdateAmmoUI;
            GlitchHunterConstant.OnUpdateReloadStatus -= ReloadStatusText;
            GlitchHunterConstant.OnReloadSliderValue -= ReloadSlider;
            GlitchHunterConstant.OnShowPlayerUI -= ShowPlayerUI;
            GlitchHunterConstant.OnShowPrompt -= OnShowPromptPanel;
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

        private IEnumerator ShowPromtPanelCorutine(string msg)
        {
            GlitchHunterConstant.FadeIn(promtPanel, 1, 0.3f, null);
            promtText?.SetText(msg);
            Debug.Log("Prompt panel showing");
            yield return new WaitForSeconds(5);
            GlitchHunterConstant.FadeOut(promtPanel, 0, 0.3f, null);
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
