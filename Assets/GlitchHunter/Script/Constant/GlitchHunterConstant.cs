using System;
using UnityEngine;
using DG.Tweening;
using GlitchHunter.Enum;

namespace GlitchHunter.Constant
{
    public static class GlitchHunterConstant
    {
        //Input
        public static Action<Vector2> OnMoveInput;
        public static Action<Vector2> OnLookInput;
        public static Action<bool> OnJumpInput;
        public static Action<bool> OnSprintInput;
        public static Action<bool> OnShootingInput;
        public static Action<bool> OnZoomInput;
        public static Action<bool> OnWeaponReloadInput;
        public static Action OnSwitchController;

        //UI
        public static Action<bool> OnShowPlayerUI;
        public static Action<string> OnShowPrompt;
        public static Action<int, int> OnUpdateAmmoUI;
        public static Action<float> OnReloadSliderValue;
        public static Action<float> OnUpdateHealthSlider;
        public static Action<string> OnUpdateReloadStatus;
        public static Action OnGameOver;

        //Hit Impact
        public static Action OnPlayerHitImpact;

        //Combat 
        public static Action<MeleeAttackState> OnAttackStateChange;

        public static Action<int> OnCollectKey;

        public static ControllerType CurrentControllerType = ControllerType.FIRST_PERSON;

        public static int ENEMY_WAVE_COMPLETED_INDEX = 0;

        public static void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

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