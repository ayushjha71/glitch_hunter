using System;
using UnityEngine;
using DG.Tweening;

namespace GlitchHunter.Constant
{
    public static class GlitchHunterConstant
    {
        public static Action<bool> OnShowPlayerUI;
        public static Action<bool, string> OnShowPrompt;
        public static Action<int, int> OnUpdateAmmoUI;
        public static Action<float> OnReloadSliderValue;
        public static Action<float> OnUpdateHealthSlider;
        public static Action<string> OnUpdateReloadStatus;
        public static Action OnGameOver;


        public static Action<int> OnCollectKey;

        public static void FadeIn(CanvasGroup canvasGroup, float endValue, float duration, Action OnCompleted)
        {
            canvasGroup.DOFade(endValue, duration).OnComplete(() => { OnCompleted?.Invoke(); });
        }

        public static void FadeOut(CanvasGroup canvasGroup, float endValue, float duration, Action OnCompleted)
        {
            canvasGroup.DOFade(endValue, duration).OnComplete(() => { OnCompleted?.Invoke(); });
        }
    }
}