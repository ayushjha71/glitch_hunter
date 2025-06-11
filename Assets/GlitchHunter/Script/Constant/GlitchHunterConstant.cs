using System;
using UnityEngine;
using DG.Tweening;

namespace GlitchHunter.Constant
{
    public static class GlitchHunterConstant
    {
        public static float GameTimeCompleted;

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