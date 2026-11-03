using System.Collections;
using UnityEngine;

public class TutorialKeeperFade : MonoBehaviour
{
    // ============================================================
    // 01 - KEEPER APPEARANCE
    // ============================================================

    [Header("KEEPER APPEARANCE")]
    [SerializeField] private CanvasGroup tutorialFrame;
    [SerializeField] private CanvasGroup puzzleKeeper;

    // ============================================================
    // 02 - TIMING
    // ============================================================

    [Header("TIMING")]
    [SerializeField, Min(0f)]
    private float appearDelay = 1.2f;

    [SerializeField, Min(0.01f)]
    private float fadeDuration = 0.8f;

    // ============================================================
    // 03 - TUTORIAL PROGRESS
    // ============================================================

    [Header("TUTORIAL PROGRESS")]
    [SerializeField]
    private bool hideIfIntroCompleted = true;

    // ============================================================
    // 04 - TUTORIAL CONTROLLER
    // ============================================================

    [Header("TUTORIAL CONTROLLER")]
    [SerializeField]
    private TutorialTextController tutorialTextController;

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private Coroutine activeFade;
    private bool introductionFinished;
    private bool appearanceStarted;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (tutorialTextController == null)
        {
            tutorialTextController =
                GetComponentInChildren<TutorialTextController>(true);
        }

        if (tutorialTextController == null)
        {
            tutorialTextController =
                FindFirstObjectByType<TutorialTextController>(
                    FindObjectsInactive.Include
                );
        }

        // Hide the visuals before the first frame.
        SetGroupsAlpha(0f);
        SetGroupsInteraction(false);

        introductionFinished =
            hideIfIntroCompleted &&
            TutorialProgressManager.IntroCompleted;
    }

    // ============================================================
    // START
    // ============================================================

    private IEnumerator Start()
    {
        if (tutorialFrame == null || puzzleKeeper == null)
        {
            Debug.LogWarning(
                "TutorialKeeperFade: Assign both Canvas Groups."
            );

            yield break;
        }

        if (introductionFinished)
        {
            HideImmediately();
            yield break;
        }

        // Wait one frame so TutorialTextController.Start()
        // can initialize its current page.
        yield return null;

        if (tutorialTextController != null &&
            !tutorialTextController.ShouldShowTutorial())
        {
            HideImmediately();
            yield break;
        }

        if (tutorialTextController != null)
        {
            tutorialTextController
                .SetTutorialPresentationVisible(false);
        }

        SetGroupsAlpha(0f);
        SetGroupsInteraction(false);

        // The initial pause starts at scene startup.
        float remainingDelay = Mathf.Max(
            0f,
            appearDelay - Time.timeSinceLevelLoad
        );

        if (remainingDelay > 0f)
            yield return new WaitForSeconds(remainingDelay);

        if (introductionFinished)
            yield break;

        appearanceStarted = true;

        // Restore text, buttons and Keeper while
        // their CanvasGroups are still invisible.
        if (tutorialTextController != null)
        {
            tutorialTextController
                .SetTutorialPresentationVisible(true);
        }

        yield return FadeGroups(0f, 1f, fadeDuration);

        if (!introductionFinished)
            SetGroupsInteraction(true);
    }

    // ============================================================
    // 05 - FADE ANIMATION
    // ============================================================

    private IEnumerator FadeGroups(
        float from,
        float to,
        float duration
    )
    {
        if (duration <= 0f)
        {
            SetGroupsAlpha(to);
            yield break;
        }

        float elapsed = 0f;

        SetGroupsAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            float smoothProgress =
                progress * progress *
                (3f - 2f * progress);

            float alpha = Mathf.Lerp(
                from,
                to,
                smoothProgress
            );

            SetGroupsAlpha(alpha);

            yield return null;
        }

        SetGroupsAlpha(to);
    }

    // ============================================================
    // 06 - GROUP HELPERS
    // ============================================================

    private void SetGroupsAlpha(float alpha)
    {
        if (tutorialFrame != null)
            tutorialFrame.alpha = alpha;

        if (puzzleKeeper != null)
            puzzleKeeper.alpha = alpha;
    }

    private void SetGroupsInteraction(bool enabled)
    {
        if (tutorialFrame != null)
        {
            tutorialFrame.interactable = enabled;
            tutorialFrame.blocksRaycasts = enabled;
        }

        if (puzzleKeeper != null)
        {
            puzzleKeeper.interactable = enabled;
            puzzleKeeper.blocksRaycasts = enabled;
        }
    }

    // ============================================================
    // 07 - HIDE IMMEDIATELY
    // ============================================================

    public void HideImmediately()
    {
        introductionFinished = true;

        if (activeFade != null)
        {
            StopCoroutine(activeFade);
            activeFade = null;
        }

        SetGroupsAlpha(0f);
        SetGroupsInteraction(false);

        if (tutorialTextController != null)
        {
            tutorialTextController
                .SetTutorialPresentationVisible(false);
        }
    }

    // ============================================================
    // 08 - HIDE SMOOTHLY
    // ============================================================

    public void HideSmoothly()
    {
        if (activeFade != null)
        {
            StopCoroutine(activeFade);
            activeFade = null;
        }

        introductionFinished = true;
        SetGroupsInteraction(false);

        activeFade = StartCoroutine(
            HideSmoothlyCoroutine()
        );
    }

    private IEnumerator HideSmoothlyCoroutine()
    {
        float startAlpha = 1f;

        if (tutorialFrame != null)
            startAlpha = tutorialFrame.alpha;

        yield return FadeGroups(
            startAlpha,
            0f,
            fadeDuration
        );

        if (tutorialTextController != null)
        {
            tutorialTextController
                .SetTutorialPresentationVisible(false);
        }

        activeFade = null;
    }

    // ============================================================
    // 09 - PUBLIC STATUS
    // ============================================================

    public bool IsIntroductionFinished()
    {
        return introductionFinished;
    }

    public bool HasAppearanceStarted()
    {
        return appearanceStarted;
    }

    public bool IsIntroductionVisible()
    {
        return !introductionFinished &&
               tutorialFrame != null &&
               tutorialFrame.alpha > 0.001f;
    }
}