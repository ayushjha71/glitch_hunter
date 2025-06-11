using UnityEngine;
using TMPro;
using System;
using GlitchHunter.Constant;

namespace GlitchHunter.UI
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private float timeValue = 300;

        private static float timeRemaining;
        private static bool isRunning;
        private static Action currentCallback;

        public static void StartTimer(float duration, Action callback)
        {
            if (isRunning) return;

            timeRemaining = duration;
            isRunning = true;
            currentCallback = callback;
        }

        public static void StopTimer()
        {
            isRunning = false;
            timeRemaining = 0;
            currentCallback = null;
        }

        void Update()
        {
            if (!isRunning) return;

            if (timeRemaining > 0)
            {
                GlitchHunterConstant.GameTimeCompleted += Time.deltaTime;
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                timeRemaining = 0;
                isRunning = false;
                currentCallback?.Invoke();
                currentCallback = null;
                UpdateTimerUI();
            }
        }

        void UpdateTimerUI()
        {
            if (timerText != null)
            {
                int totalSeconds = Mathf.CeilToInt(timeRemaining);
                int hours = totalSeconds / 3600;
                int minutes = (totalSeconds % 3600) / 60;
                int seconds = totalSeconds % 60;

                timerText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
        }
    }
}
