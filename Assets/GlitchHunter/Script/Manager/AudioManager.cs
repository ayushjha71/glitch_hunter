using GlitchHunter.Constant;
using UnityEngine;

namespace GlitchHunter.Manager
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Bg Audio Source")]
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip initialBgAudio_glitch;
        [SerializeField]
        private AudioClip BgAudio_horror;


        private void Start()
        {
            OnChangeBgAudio(1);
        }

        private void OnEnable()
        {
            GlitchHunterConstant.OnChangeBgAudio += OnChangeBgAudio;
        }

        private void OnDisable()
        {
            GlitchHunterConstant.OnChangeBgAudio -= OnChangeBgAudio;
        }

        private void OnChangeBgAudio(int currentAudio)
        {
            audioSource.clip = null;
            audioSource.Stop();
            switch (currentAudio)
            {
                case 1:
                    {
                        PlayAudio(initialBgAudio_glitch);
                    }
                    break;
                case 2:
                    {
                        PlayAudio(BgAudio_horror);
                    }
                    break;
            }
        }

        private void PlayAudio(AudioClip audioClip)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}