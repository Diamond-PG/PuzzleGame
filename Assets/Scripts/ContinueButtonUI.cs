
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContinueButtonUI : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button hitboxButton;
    [SerializeField] private RectTransform visual;

    [Header("Availability")]
    [SerializeField, Range(0f, 1f)]
    private float lockedAlpha = 0.7f;

    [SerializeField, Range(0f, 1f)]
    private float unlockedAlpha = 1f;

    [SerializeField, Min(1)]
    private int minimumSavedTutorialPage = 1;

    [Header("Script Animation")]
    [SerializeField] private bool animateOnPress = true;

    [SerializeField, Range(0.8f, 1f)]
    private float pressedScale = 0.96f;

    [SerializeField, Min(1f)]
    private float animationSpeed = 18f;

    private Vector3 originalScale = Vector3.one;
    private bool isAvailable;
    private bool isPressed;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponentInParent<CanvasGroup>();

        if (hitboxButton == null)
            hitboxButton = GetComponent<Button>();

        if (visual != null)
            originalScale = visual.localScale;
    }

    private void OnEnable()
    {
        isPressed = false;
        RefreshState();
    }

    private void Start()
    {
        RefreshState();
    }

    // Check existing saved tutorial progress.
    private bool HasSavedProgress()
    {
        return
            TutorialProgressManager.SavedMainTutorialPage
                >= minimumSavedTutorialPage ||

            TutorialProgressManager.HasPlayerName ||

            TutorialProgressManager.HasSavedLeftTutorialStage ||

            TutorialProgressManager.MovementLeftUsed ||
            TutorialProgressManager.MovementRightUsed ||

            TutorialProgressManager.MovementCompleted ||
            TutorialProgressManager.JumpCompleted ||
            TutorialProgressManager.KickCompleted ||

            TutorialProgressManager.DoorLessonReached ||
            TutorialProgressManager.DoorBroken ||

            TutorialProgressManager.LadderReached ||
            TutorialProgressManager.ClimbCompleted ||

            TutorialProgressManager.SavedDoorHits > 0;
    }

    public void RefreshState()
    {
        isAvailable = HasSavedProgress();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isAvailable
                ? unlockedAlpha
                : lockedAlpha;

            canvasGroup.interactable = isAvailable;
            canvasGroup.blocksRaycasts = isAvailable;
        }

        if (hitboxButton != null)
            hitboxButton.interactable = isAvailable;

        if (!isAvailable)
            isPressed = false;

        Debug.Log(
            "[ContinueButtonUI] Saved progress: " +
            isAvailable
        );
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isAvailable || !animateOnPress)
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

    private void LateUpdate()
    {
        if (visual == null)
            return;

        float scaleMultiplier =
            isAvailable && isPressed && animateOnPress
                ? pressedScale
                : 1f;

        Vector3 targetScale =
            originalScale * scaleMultiplier;

        float t = 1f - Mathf.Exp(
            -animationSpeed * Time.unscaledDeltaTime
        );

        visual.localScale = Vector3.Lerp(
            visual.localScale,
            targetScale,
            t
        );
    }

    private void OnDisable()
    {
        isPressed = false;

        if (visual != null)
            visual.localScale = originalScale;
    }
}
