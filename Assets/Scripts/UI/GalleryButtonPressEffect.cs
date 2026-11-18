
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class GalleryButtonPressEffect : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Press Effect")]
    [Range(0.8f, 1f)]
    [SerializeField] private float pressedScale = 0.95f;

    [Range(0.5f, 1f)]
    [SerializeField] private float pressedAlpha = 0.85f;

    [SerializeField] private float pressedOffsetY = -4f;

    [Header("Animation")]
    [Min(1f)]
    [SerializeField] private float animationSpeed = 20f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Button button;

    private Vector3 originalScale;
    private Vector2 originalPosition;
    private float originalAlpha;

    private bool isPressed;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        originalAlpha = canvasGroup.alpha;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!button.IsInteractable())
            return;

        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }

    private void Update()
    {
        if (!button.IsInteractable())
            isPressed = false;

        float targetScale = isPressed ? pressedScale : 1f;
        float targetAlpha = isPressed
            ? originalAlpha * pressedAlpha
            : originalAlpha;

        Vector2 targetPosition = originalPosition;

        if (isPressed)
            targetPosition.y += pressedOffsetY;

        Vector3 targetLocalScale = new Vector3(
            originalScale.x * targetScale,
            originalScale.y * targetScale,
            originalScale.z
        );

        float t = 1f - Mathf.Exp(
            -animationSpeed * Time.unscaledDeltaTime
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetLocalScale,
            t
        );

        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            t
        );

        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            targetAlpha,
            t
        );
    }

    private void OnDisable()
    {
        isPressed = false;

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
            rectTransform.anchoredPosition = originalPosition;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = originalAlpha;
    }
}
