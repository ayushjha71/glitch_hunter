using UnityEngine;
using UnityEngine.UI;

public class CombatUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Slider chargeSliderPrefab;

    private void Start()
    {
        SetupWorldCanvas();
    }

    private void SetupWorldCanvas()
    {
        if (worldCanvas == null)
        {
            // Create world space canvas for enemy UI elements
            GameObject canvasObj = new GameObject("EnemyCombatCanvas");
            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.worldCamera = Camera.main;

            // Add CanvasScaler for consistent UI scaling
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Add GraphicRaycaster for UI interactions
            canvasObj.AddComponent<GraphicRaycaster>();

            // Position canvas
            canvasObj.transform.position = transform.position + Vector3.up * 3f;
            canvasObj.transform.localScale = Vector3.one * 0.01f; // Scale down for world space
        }
    }

    public Slider CreateChargeSlider(Transform parent)
    {
        if (chargeSliderPrefab != null)
        {
            Slider slider = Instantiate(chargeSliderPrefab, worldCanvas.transform);
            slider.transform.position = parent.position + Vector3.up * 3f;
            return slider;
        }
        else
        {
            return CreateDefaultChargeSlider(parent);
        }
    }

    private Slider CreateDefaultChargeSlider(Transform parent)
    {
        // Create slider GameObject
        GameObject sliderObj = new GameObject("ChargeSlider");
        sliderObj.transform.SetParent(worldCanvas.transform);
        sliderObj.transform.position = parent.position + Vector3.up * 3f;

        // Add Slider component
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Create fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);

        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.red;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;

        // Setup slider references
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        // Set size
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(200, 20);

        return slider;
    }
}