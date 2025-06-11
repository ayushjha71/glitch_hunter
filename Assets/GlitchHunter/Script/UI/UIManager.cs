using GlitchHunter.Constant;
using GlitchHunter.Data;
using GlitchHunter.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup mainCanvas;
    [SerializeField]
    private CanvasGroup mainScreen;
    [SerializeField]
    private float duration = 0.5f;
    [SerializeField]
    private Button startBtn;

    [SerializeField]
    private CanvasGroup cutSceneCanvas;
    [SerializeField]
    private VideoPlayer videoPlayer;

    [Header("In Game UI")]
    [SerializeField]
    private UITextData uiTextScriptableObject;
    [SerializeField]
    private CanvasGroup promptPanel;
    [SerializeField]
    private TMP_Text promptText;
    [SerializeField]
    private Button OkBtn;
    [SerializeField]
    private AudioSource audioSource;

    private int currentIndex = 0;

    private void Start()
    {
        startBtn.onClick.AddListener(StartBtnClick);
        OkBtn.onClick.AddListener(OnClickOkButton);
    }

    private void StartBtnClick()
    {
        GlitchHunterConstant.FadeOut(mainScreen, 0, duration, OnStartCutScene);
    }

    private void OnStartCutScene()
    {
        GlitchHunterConstant.FadeIn(cutSceneCanvas, 1, 5, OnVideoPlayOver);
    }

    private void OnVideoPlayOver()
    {
        GlitchHunterConstant.FadeOut(cutSceneCanvas, 0, duration, null);
        GlitchHunterConstant.FadeOut(mainScreen, 0, duration, null);
        GlitchHunterConstant.FadeIn(promptPanel, 1, duration, null);
        mainScreen.gameObject.SetActive(false);
        if (uiTextScriptableObject != null && uiTextScriptableObject.UIData.Length > 0)
        {
            currentIndex = 0;
            promptText.text = uiTextScriptableObject.UIData[currentIndex].Messages;
          //  audioSource.clip = uiTextScriptableObject.UIData[currentIndex].AudioClip;
        }
        else
        {
            Debug.LogWarning("No string data assigned or empty!");
            promptText.text = "Welcome to new World";
        }
    }

    private void OnClickOkButton()
    {
        currentIndex++;

        if (currentIndex < uiTextScriptableObject.UIData.Length)
        {
            promptText.text = uiTextScriptableObject.UIData[currentIndex].Messages;
           // audioSource.clip = uiTextScriptableObject.UIData[currentIndex].AudioClip;
        }
        else
        {
            // All messages shown, deactivate UI
            //gameObject.SetActive(false);
            GlitchHunterConstant.FadeOut(promptPanel, 0, 0.3f, StartTimer);
            
        }
    }

    private void StartTimer()
    {
        Timer.StartTimer(300, null);
    }
}
