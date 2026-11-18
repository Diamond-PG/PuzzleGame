using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TutorialArrowPulse : MonoBehaviour
{
    // ============================================================
    // 01 - TUTORIAL CONTROLLER
    // ============================================================

    [Header("TUTORIAL CONTROLLER")]

    [SerializeField]
    private TutorialTextController tutorialController;

    // ============================================================
    // 02 - PULSE SETTINGS
    // ============================================================

    [Header("PULSE SETTINGS")]

    [Tooltip("Maximum extra scale of the arrow.")]
    [SerializeField, Range(0.01f, 0.3f)]
    private float pulseAmount = 0.08f;

    [Tooltip("Time for one complete pulse.")]
    [SerializeField, Range(0.4f, 3f)]
    private float pulsePeriod = 1.2f;

    [Tooltip("Continue pulsing when game time is paused.")]
    [SerializeField]
    private bool useUnscaledTime = true;

    // ============================================================
    // 03 - TUTORIAL PAGES
    // ============================================================

    [Header("PULSE ON TUTORIAL PAGES")]

    [Tooltip(
        "Pages where the Next arrow should pulse. " +
        "Page 24 is reserved for the ladder explanation."
    )]
    [SerializeField]
    private int[] pulsePages = { 0, 24 };

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private RectTransform arrowRect;

    private Vector3 originalScale;

    private float elapsedTime;

    private bool forcePulse;

    private bool initialized;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        Initialize();
    }

    // ============================================================
    // INITIALIZE
    // ============================================================

    private void Initialize()
    {
        if (initialized)
            return;

        arrowRect = GetComponent<RectTransform>();

        originalScale = arrowRect.localScale;

        if (tutorialController == null)
        {
            tutorialController =
                GetComponentInParent<TutorialTextController>();
        }

        initialized = true;
    }

    // ============================================================
    // ON ENABLE
    // ============================================================

    private void OnEnable()
    {
        Initialize();

        elapsedTime = 0f;

        ResetArrowScale();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (arrowRect == null)
            return;

        if (!ShouldPulse())
        {
            ResetArrowScale();
            return;
        }

        float deltaTime = useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        elapsedTime += deltaTime;

        float safePeriod =
            Mathf.Max(0.01f, pulsePeriod);

        float angle =
            elapsedTime *
            Mathf.PI *
            2f /
            safePeriod;

        // Gentle animation from original size
        // to enlarged size and back.

        float wave =
            0.5f -
            0.5f * Mathf.Cos(angle);

        float multiplier =
            1f + pulseAmount * wave;

        arrowRect.localScale = new Vector3(
            originalScale.x * multiplier,
            originalScale.y * multiplier,
            originalScale.z
        );
    }

    // ============================================================
    // CHECK IF PULSE IS NEEDED
    // ============================================================

    private bool ShouldPulse()
    {
        if (forcePulse)
            return true;

        if (tutorialController == null)
            return false;

        if (!tutorialController.ShouldShowTutorial())
            return false;

        if (!tutorialController.IsLocalizationReady())
            return false;

        int currentPage =
            tutorialController.GetCurrentPage();

        if (pulsePages == null)
            return false;

        for (int i = 0; i < pulsePages.Length; i++)
        {
            if (pulsePages[i] == currentPage)
                return true;
        }

        return false;
    }

    // ============================================================
    // MANUAL PULSE CONTROL
    // ============================================================

    public void SetPulseActive(bool active)
    {
        forcePulse = active;

        if (!active)
        {
            elapsedTime = 0f;
            ResetArrowScale();
        }
    }

    // ============================================================
    // RESET SCALE
    // ============================================================

    private void ResetArrowScale()
    {
        if (arrowRect == null)
            return;

        arrowRect.localScale = originalScale;
    }

    // ============================================================
    // ON DISABLE
    // ============================================================

    private void OnDisable()
    {
        ResetArrowScale();
        elapsedTime = 0f;
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        ResetArrowScale();
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private void OnValidate()
    {
        pulseAmount =
            Mathf.Clamp(pulseAmount, 0.01f, 0.3f);

        pulsePeriod =
            Mathf.Clamp(pulsePeriod, 0.4f, 3f);
    }
}