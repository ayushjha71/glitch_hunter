using GlitchHunter.Constant;
using GlitchHunter.Data;
using GlitchHunter.Manager;
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
    private CanvasGroup inGamePanel;
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

    [SerializeField]
    private CanvasGroup gameOverPanel;

    private int currentIndex = 0;

    private void Start()
    {
        startBtn.onClick.AddListener(StartBtnClick);
        OkBtn.onClick.AddListener(OnClickOkButton);
    }

    private void OnEnable()
    {
        GlitchHunterConstant.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GlitchHunterConstant.OnGameOver -= OnGameOver;
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
        GlitchHunterConstant.FadeIn(promptPanel, 1, duration, StartTimer);
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
            GlitchHunterConstant.FadeOut(promptPanel, 0, 0.3f, null);
            GlitchHunterConstant.OnShowPlayerUI?.Invoke(true);
        }
    }

    private void OnGameOver()
    {
        GlitchHunterConstant.FadeOut(inGamePanel, 0, 0.3f, null);
        GlitchHunterConstant.FadeIn(gameOverPanel, 1, 0.3f, null);
    }

    private void StartTimer()
    {
        GameManager.Instance.IsGameStarted = true;
        Timer.StartTimer(300, null);
    }
}
