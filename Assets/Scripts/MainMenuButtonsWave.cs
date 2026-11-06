
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MainMenuButtonsWave : MonoBehaviour
{
    [Header("BUTTONS - APPEARANCE ORDER")]
    [SerializeField] private RectTransform newGameVisual;
    [SerializeField] private RectTransform continueVisual;
    [SerializeField] private RectTransform optionsVisual;
    [SerializeField] private RectTransform quitVisual;
    [SerializeField] private RectTransform galleryVisual;

    [Header("APPEARANCE TIMING")]
    [SerializeField, Min(0f)]
    private float startDelay = 0.10f;

    [SerializeField, Min(0f)]
    private float delayBetweenButtons = 0.16f;

    [SerializeField, Min(0.05f)]
    private float animationDuration = 0.50f;

    [Header("APPEARANCE MOVEMENT")]
    [SerializeField, Min(0f)]
    private float moveFromBelow = 125f;

    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("APPEARANCE FADE")]
    [SerializeField] private bool useFade = true;

    [Header("IDLE WAVE - AFTER APPEARANCE")]
    [SerializeField] private bool enableIdleWave = true;

    [SerializeField, Min(0f)]
    private float idleAmplitude = 5f;

    [SerializeField, Min(0f)]
    private float idleSpeed = 0.55f;

    [SerializeField]
    private float idlePhaseOffset = 0.85f;

    [SerializeField]
    private bool animateGalleryIdle = false;

    [Header("GENERAL")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool showDebugLogs = true;

    private class ButtonItem
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Vector2 originalPosition;
    }

    private ButtonItem[] buttons;

    private bool hasStarted;
    private bool waveFinished;

    private float idleTime;
    private Coroutine waveCoroutine;

    private void Awake()
    {
        InitializeButtons();
        HideButtons();
    }

    private void Start()
    {
        hasStarted = true;
        ReplayWave();
    }

    private void OnEnable()
    {
        if (hasStarted)
            ReplayWave();
    }

    private void InitializeButtons()
    {
        RectTransform[] references =
        {
            newGameVisual,
            continueVisual,
            optionsVisual,
            quitVisual,
            galleryVisual
        };

        buttons = new ButtonItem[references.Length];

        for (int i = 0; i < references.Length; i++)
        {
            RectTransform rect = references[i];

            if (rect == null)
            {
                Debug.LogWarning(
                    "[MainMenuButtonsWave] Button " +
                    i + " is not assigned!",
                    this
                );

                continue;
            }

            CanvasGroup group =
                rect.GetComponent<CanvasGroup>();

            if (group == null)
            {
                group = rect.gameObject
                    .AddComponent<CanvasGroup>();
            }

            // Respect parent groups.
            // ContinueButton keeps its locked alpha.
            group.ignoreParentGroups = false;

            buttons[i] = new ButtonItem
            {
                rect = rect,
                group = group,
                originalPosition = rect.anchoredPosition
            };

            Animator animator =
                rect.GetComponent<Animator>();

            if (animator != null && animator.enabled)
            {
                Debug.LogWarning(
                    "[MainMenuButtonsWave] Active Animator " +
                    "may conflict with wave: " +
                    rect.name,
                    rect
                );
            }
        }
    }

    private void HideButtons()
    {
        if (buttons == null)
            return;

        waveFinished = false;

        foreach (ButtonItem button in buttons)
        {
            if (button == null)
                continue;

            button.rect.anchoredPosition =
                button.originalPosition +
                Vector2.down * moveFromBelow;

            button.group.alpha = 0f;
        }
    }

    [ContextMenu("Replay Buttons Wave")]
    public void ReplayWave()
    {
        if (!isActiveAndEnabled || buttons == null)
            return;

        StopAllCoroutines();

        waveCoroutine = null;
        waveFinished = false;
        idleTime = 0f;

        HideButtons();

        if (showDebugLogs)
        {
            Debug.Log(
                "[MainMenuButtonsWave] Wave started.",
                this
            );
        }

        waveCoroutine = StartCoroutine(
            PlayWaveSequence()
        );
    }

    private IEnumerator PlayWaveSequence()
    {
        if (startDelay > 0f)
            yield return WaitDelay(startDelay);

        for (int i = 0; i < buttons.Length; i++)
        {
            ButtonItem button = buttons[i];

            if (button != null)
            {
                StartCoroutine(
                    AnimateSingleButton(button)
                );

                if (showDebugLogs)
                {
                    Debug.Log(
                        "[MainMenuButtonsWave] Showing: " +
                        button.rect.name +
                        ", order " + (i + 1),
                        this
                    );
                }
            }

            if (i < buttons.Length - 1 &&
                delayBetweenButtons > 0f)
            {
                yield return WaitDelay(
                    delayBetweenButtons
                );
            }
        }

        yield return WaitDelay(animationDuration);

        RestoreButtons();

        idleTime = 0f;
        waveFinished = true;
        waveCoroutine = null;

        if (showDebugLogs)
        {
            Debug.Log(
                "[MainMenuButtonsWave] Wave completed.",
                this
            );
        }
    }

    private IEnumerator AnimateSingleButton(
        ButtonItem button)
    {
        float elapsed = 0f;

        Vector2 startPosition =
            button.originalPosition +
            Vector2.down * moveFromBelow;

        while (elapsed < animationDuration)
        {
            float progress = Mathf.Clamp01(
                elapsed / animationDuration
            );

            float movementProgress =
                movementCurve != null &&
                movementCurve.length > 0
                    ? movementCurve.Evaluate(progress)
                    : Mathf.SmoothStep(
                        0f, 1f, progress
                    );

            movementProgress = Mathf.Clamp01(
                movementProgress
            );

            button.rect.anchoredPosition =
                Vector2.LerpUnclamped(
                    startPosition,
                    button.originalPosition,
                    movementProgress
                );

            float fadeProgress =
                Mathf.SmoothStep(
                    0f, 1f, progress
                );

            button.group.alpha =
                useFade ? fadeProgress : 1f;

            elapsed += GetDeltaTime();

            yield return null;
        }

        button.rect.anchoredPosition =
            button.originalPosition;

        button.group.alpha = 1f;
    }

    private IEnumerator WaitDelay(float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += GetDeltaTime();
            yield return null;
        }
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!waveFinished || !enableIdleWave)
            return;

        idleTime += GetDeltaTime();

        for (int i = 0; i < buttons.Length; i++)
        {
            ButtonItem button = buttons[i];

            if (button == null)
                continue;

            // Gallery already uses UIButtonPulse.
            if (i == 4 && !animateGalleryIdle)
                continue;

            float phase =
                idleTime *
                idleSpeed *
                Mathf.PI * 2f +
                i * idlePhaseOffset;

            float offset =
                Mathf.Sin(phase) * idleAmplitude;

            button.rect.anchoredPosition =
                button.originalPosition +
                Vector2.up * offset;
        }
    }

    private void RestoreButtons()
    {
        if (buttons == null)
            return;

        foreach (ButtonItem button in buttons)
        {
            if (button == null)
                continue;

            button.rect.anchoredPosition =
                button.originalPosition;

            button.group.alpha = 1f;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        waveCoroutine = null;
        waveFinished = false;

        RestoreButtons();
    }
}
