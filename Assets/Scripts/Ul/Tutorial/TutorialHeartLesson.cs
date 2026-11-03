
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Continuation of TutorialClubLesson after tutorial_44.
///
/// 45: Bonus heart congratulations.
/// 46: Explain bonus hearts, flash oval spotlight.
/// 47: Keeper explains survival.
/// 48: Keeper warns player, then fades away.
///
/// Stage 34 from the older version resumes on page 48.
/// Stage 35 means the heart tutorial is complete.
///
/// Attach to Canvas/TutorialRoot/TutorialFrame_Left.
/// </summary>
[DisallowMultipleComponent]
public sealed class TutorialHeartLesson : MonoBehaviour
{
    private const int StageBoxUnlocked = 29;
    private const int StageHeartCongratulations = 30;
    private const int StageHeartExplanation = 31;
    private const int StageFarewellSurvival = 32;
    private const int StageFarewellWarning = 33;
    private const int StageLegacyFarewellByName = 34;
    private const int StageHeartCompleted = 35;

    [Serializable]
    private class IndependentKeeperPose
    {
        public Sprite keeperSprite;

        [Header("KEEPER RECT TRANSFORM")]
        public Vector2 keeperPosition =
            new Vector2(134.8f, -221f);

        public Vector2 keeperSize =
            new Vector2(230f, 230f);

        public float keeperRotationZ;
        public Vector2 keeperScale = Vector2.one;

        [Header("BLUE PUZZLE MEDALLION - INDEPENDENT")]
        public Vector2 puzzlePosition =
            new Vector2(6.9f, -63.8f);

        public Vector2 puzzleSize =
            new Vector2(27f, 27f);

        public float puzzleRotationZ = 20f;
        public Vector2 puzzleScale = Vector2.one;
    }

    [Header("01 - FIRST HEART PICKUP (ONLY THIS BOX)")]
    [SerializeField]
    private BonusHeartPickup firstHeartPickup;

    [SerializeField]
    private Transform firstHeartBoxTarget;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField, Min(0.5f)]
    private float resumeNearBoxDistance = 5f;

    [SerializeField, Min(0f)]
    private float afterPickupUIWait = 0.22f;

    [Header("02 - EXISTING LEFT KEEPER FRAME")]
    [SerializeField] private TMP_Text leftTutorialText;
    [SerializeField] private CanvasGroup leftFrameCanvasGroup;
    [SerializeField] private Button goldNextButton;
    [SerializeField] private TutorialArrowPulse goldNextPulse;
    [SerializeField] private GameObject tutorialDimmer;
    [SerializeField] private Image keeperImage;
    [SerializeField] private RectTransform keeperPuzzleIcon;

    [Header("03 - EXISTING HEARTS UI")]
    [SerializeField] private HeartsUI heartsUI;
    [SerializeField] private RectTransform heartsContainer;
    [SerializeField] private RectTransform bonusHeartBadge;

    [Header("04 - TEXTS / LOCALIZATION (TUTORIAL 45-46)")]
    [SerializeField]
    private string localizationTable = "Tutorial";

    [SerializeField]
    private string congratulationsKey = "tutorial_45";

    [SerializeField, TextArea(2, 4)]
    private string congratulationsFallback =
        "Поздравляю! Ты нашёл сердечко!";

    [SerializeField]
    private string explanationKey = "tutorial_46";

    [SerializeField, TextArea(2, 4)]
    private string explanationFallback =
        "Оно пополняет твой запас сердечек!";

    [Header("05 - HAPPY KEEPER (TUTORIAL 45)")]
    [SerializeField]
    private IndependentKeeperPose happyKeeperPose =
        new IndependentKeeperPose();

    [Header("06 - OPEN PALM KEEPER (TUTORIAL 46)")]
    [SerializeField]
    private IndependentKeeperPose openPalmKeeperPose =
        new IndependentKeeperPose();

    [Header("07 - OVAL SPOTLIGHT (TUTORIAL 46)")]

    [Tooltip("Horizontal offset relative to screen width.")]
    [SerializeField]
    private float ovalOffsetX = 0f;

    [Tooltip("Vertical offset relative to screen height.")]
    [SerializeField]
    private float ovalOffsetY = 0f;

    [Tooltip("Controls oval width.")]
    [SerializeField, Min(0.2f)]
    private float ovalWidthMultiplier = 1.12f;

    [Tooltip("Controls oval height.")]
    [SerializeField, Min(0.2f)]
    private float ovalHeightMultiplier = 1.55f;

    [SerializeField, Range(0f, 0.2f)]
    private float ovalExtraWidth = 0.012f;

    [SerializeField, Range(0f, 0.2f)]
    private float ovalExtraHeight = 0.018f;

    [SerializeField, Range(64, 512)]
    private int spotlightTextureResolution = 256;

    [SerializeField, Range(0.002f, 0.20f)]
    private float ovalEdgeSoftness = 0.06f;

    [Header("08 - THREE SPOTLIGHT FLASHES (TUTORIAL 46)")]
    [SerializeField, Range(1, 10)]
    private int flashCount = 3;

    [SerializeField, Min(0.05f)]
    private float flashVisibleSeconds = 0.8f;

    [SerializeField, Min(0.05f)]
    private float flashHiddenSeconds = 0.5f;

    [SerializeField, Min(0f)]
    private float flashStartDelay = 0.35f;

    [Header("09 - FINAL LESSON TEXTS / LOCALIZATION (TUTORIAL 47-48)")]

    [SerializeField]
    private string survivalKey = "tutorial_47";

    [SerializeField, TextArea(2, 4)]
    private string survivalFallback =
        "Теперь ты знаешь, как выжить в этих стенах.";

    [SerializeField]
    private string warningKey = "tutorial_48";

    [SerializeField, TextArea(2, 4)]
    private string warningFallback =
        "Дальше всё зависит от тебя. Будь начеку!";

    [Header("10 - OPEN PALM KEEPER (TUTORIAL 47)")]
    [Tooltip("Independent pose for page 47.")]
    [SerializeField]
    private IndependentKeeperPose survivalKeeperPose =
        new IndependentKeeperPose
        {
            puzzlePosition = new Vector2(-13f, -63.8f)
        };

    [Header("11 - OPEN PALM KEEPER (TUTORIAL 48)")]
    [Tooltip("Independent pose for page 48.")]
    [SerializeField]
    private IndependentKeeperPose warningKeeperPose =
        new IndependentKeeperPose
        {
            puzzlePosition = new Vector2(-13f, -63.8f)
        };

    [Header("12 - KEEPER FADE AFTER TUTORIAL 48")]
    [SerializeField, Min(0f)]
    private float goodbyeFadeSeconds = 0.65f;

    private enum Phase
    {
        Waiting,
        Congratulations,
        Explanation,
        FarewellSurvival,
        FarewellWarning,
        FadingOut,
        Complete
    }

    private Phase phase = Phase.Waiting;

    private Image dimmerImage;
    private Sprite originalDimmerSprite;
    private Image.Type originalDimmerType;
    private bool originalDimmerPreserveAspect;
    private bool originalDimmerRaycastTarget;

    private Texture2D maskTexture;
    private Sprite maskSprite;
    private Color32[] maskPixels;

    private Coroutine flashRoutine;
    private Coroutine pickupWaitRoutine;
    private Coroutine localizationRoutine;
    private Coroutine goodbyeFadeRoutine;

    private int localizationVersion;

    private string localizedCongratulations;
    private string localizedExplanation;
    private string localizedSurvival;
    private string localizedWarning;

    private float previousTimeScale = 1f;
    private bool pausedByLesson;
    private bool movementLockedByLesson;

    private bool keeperBaselineCaptured;
    private Sprite keeperBaselineSprite;
    private Vector2 keeperBaselinePosition;
    private Vector2 keeperBaselineSize;
    private Quaternion keeperBaselineRotation;
    private Vector3 keeperBaselineScale;

    private Vector2 puzzleBaselinePosition;
    private Vector2 puzzleBaselineSize;
    private Quaternion puzzleBaselineRotation;
    private Vector3 puzzleBaselineScale;

    private readonly List<CanvasGroup> lockedControlGroups =
        new List<CanvasGroup>();

    private readonly List<bool> lockedControlPreviousRaycasts =
        new List<bool>();

    private void Awake()
    {
        ResolveReferences();

        localizedCongratulations = congratulationsFallback;
        localizedExplanation = explanationFallback;
        localizedSurvival = survivalFallback;
        localizedWarning = warningFallback;

        if (dimmerImage != null)
        {
            originalDimmerSprite = dimmerImage.sprite;
            originalDimmerType = dimmerImage.type;
            originalDimmerPreserveAspect =
                dimmerImage.preserveAspect;
            originalDimmerRaycastTarget =
                dimmerImage.raycastTarget;
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged +=
            OnLocaleChanged;

        LoadLocalizedLines();
    }

    private void Start()
    {
        ResolveReferences();

        if (goldNextButton != null)
        {
            goldNextButton.onClick.RemoveListener(
                OnGoldNextClicked);

            goldNextButton.onClick.AddListener(
                OnGoldNextClicked);
        }

        if (firstHeartPickup != null)
        {
            firstHeartPickup.PickupCompleted -=
                OnFirstHeartPickupCompleted;

            firstHeartPickup.PickupCompleted +=
                OnFirstHeartPickupCompleted;
        }
        else
        {
            Debug.LogWarning(
                "[HEART TUTORIAL] Assign FIRST BonusHeartPickup in section 01.",
                this);
        }

        if (TutorialProgressManager.SavedLeftTutorialStage >=
            StageHeartCompleted)
        {
            phase = Phase.Complete;
        }
    }

    private void Update()
    {
        if (phase != Phase.Waiting)
            return;

        int saved =
            TutorialProgressManager.SavedLeftTutorialStage;

        if (saved != StageHeartCongratulations &&
            saved != StageHeartExplanation &&
            saved != StageFarewellSurvival &&
            saved != StageFarewellWarning &&
            saved != StageLegacyFarewellByName)
        {
            return;
        }

        if (firstHeartBoxTarget == null ||
            playerController == null)
        {
            return;
        }

        if (Vector2.Distance(
            firstHeartBoxTarget.position,
            playerController.transform.position) >
            resumeNearBoxDistance)
        {
            return;
        }

        switch (saved)
        {
            case StageHeartCongratulations:
                ShowCongratulationsPage();
                break;

            case StageHeartExplanation:
                ShowExplanationPage();
                break;

            case StageFarewellSurvival:
                ShowSurvivalPage();
                break;

            case StageFarewellWarning:
            case StageLegacyFarewellByName:
                ShowWarningPage();
                break;
        }
    }

    private void OnFirstHeartPickupCompleted(
        BonusHeartPickup source)
    {
        if (source != firstHeartPickup ||
            phase != Phase.Waiting)
        {
            return;
        }

        if (TutorialProgressManager.SavedLeftTutorialStage !=
            StageBoxUnlocked)
        {
            return;
        }

        if (pickupWaitRoutine != null)
            StopCoroutine(pickupWaitRoutine);

        pickupWaitRoutine =
            StartCoroutine(WaitForHeartPopThenShowKeeper());
    }

    private IEnumerator WaitForHeartPopThenShowKeeper()
    {
        if (afterPickupUIWait > 0f)
        {
            yield return new WaitForSecondsRealtime(
                afterPickupUIWait);
        }

        pickupWaitRoutine = null;

        if (phase == Phase.Waiting &&
            TutorialProgressManager.SavedLeftTutorialStage ==
            StageBoxUnlocked)
        {
            ShowCongratulationsPage();
        }
    }

    private void ShowCongratulationsPage()
    {
        if (!PrepareKeeper())
            return;

        phase = Phase.Congratulations;

        TutorialProgressManager.SaveLeftTutorialStage(
            StageHeartCongratulations);

        ApplyKeeperPose(happyKeeperPose);
        SetKeeperMessage(localizedCongratulations);
        ShowPlainDimmer();
        SetGoldNextVisible(true);
    }

    private void ShowExplanationPage()
    {
        if (!PrepareKeeper())
            return;

        phase = Phase.Explanation;

        TutorialProgressManager.SaveLeftTutorialStage(
            StageHeartExplanation);

        ApplyKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedExplanation);
        ShowHeartOvalSpotlight(true);
        SetGoldNextVisible(true);

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(
            FlashHeartsSpotlight());
    }

    private void ShowSurvivalPage()
    {
        if (!PrepareKeeper())
            return;

        StopHeartFlash();

        phase = Phase.FarewellSurvival;

        TutorialProgressManager.SaveLeftTutorialStage(
            StageFarewellSurvival);

        ApplyKeeperPose(
            survivalKeeperPose,
            openPalmKeeperPose.keeperSprite);

        SetKeeperMessage(localizedSurvival);
        ShowPlainDimmer();
        SetGoldNextVisible(true);
    }

    private void ShowWarningPage()
    {
        if (!PrepareKeeper())
            return;

        StopHeartFlash();

        phase = Phase.FarewellWarning;

        TutorialProgressManager.SaveLeftTutorialStage(
            StageFarewellWarning);

        ApplyKeeperPose(
            warningKeeperPose,
            openPalmKeeperPose.keeperSprite);

        SetKeeperMessage(localizedWarning);
        ShowPlainDimmer();
        SetGoldNextVisible(true);
    }

    private void StopHeartFlash()
    {
        if (flashRoutine == null)
            return;

        StopCoroutine(flashRoutine);
        flashRoutine = null;
    }

    private void FinishFarewell()
    {
        if (phase != Phase.FarewellWarning)
            return;

        TutorialProgressManager.SaveLeftTutorialStage(
            StageHeartCompleted);

        phase = Phase.FadingOut;

        StopHeartFlash();
        SetGoldNextVisible(false);
        RestoreDimmerImage();

        if (goodbyeFadeRoutine != null)
            StopCoroutine(goodbyeFadeRoutine);

        goodbyeFadeRoutine =
            StartCoroutine(FadeKeeperAndResume());
    }

    private IEnumerator FadeKeeperAndResume()
    {
        float startAlpha =
            leftFrameCanvasGroup != null
                ? leftFrameCanvasGroup.alpha
                : 1f;

        float elapsed = 0f;

        while (goodbyeFadeSeconds > 0f &&
            elapsed < goodbyeFadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            if (leftFrameCanvasGroup != null)
            {
                leftFrameCanvasGroup.alpha = Mathf.Lerp(
                    startAlpha,
                    0f,
                    Mathf.Clamp01(
                        elapsed / goodbyeFadeSeconds));
            }

            yield return null;
        }

        goodbyeFadeRoutine = null;

        HideKeeperFrame();
        RestoreDimmer();
        RestoreKeeperBaseline();
        ReleaseGameplay();

        phase = Phase.Complete;
    }

    private bool PrepareKeeper()
    {
        ResolveReferences();

        if (leftFrameCanvasGroup == null ||
            leftTutorialText == null ||
            goldNextButton == null ||
            tutorialDimmer == null ||
            dimmerImage == null ||
            playerController == null)
        {
            Debug.LogWarning(
                "[HEART TUTORIAL] Missing left frame, dimmer or player reference.",
                this);

            return false;
        }

        CaptureKeeperBaseline();
        PauseAndLockGameplay();

        leftFrameCanvasGroup.alpha = 1f;

        return true;
    }

    private void OnGoldNextClicked()
    {
        if (phase == Phase.Congratulations)
        {
            ShowExplanationPage();
        }
        else if (phase == Phase.Explanation)
        {
            ShowSurvivalPage();
        }
        else if (phase == Phase.FarewellSurvival)
        {
            ShowWarningPage();
        }
        else if (phase == Phase.FarewellWarning)
        {
            FinishFarewell();
        }
    }

    private IEnumerator FlashHeartsSpotlight()
    {
        if (flashStartDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                flashStartDelay);
        }

        for (int i = 0; i < flashCount; i++)
        {
            if (phase != Phase.Explanation)
                yield break;

            ShowHeartOvalSpotlight(true);

            yield return new WaitForSecondsRealtime(
                flashVisibleSeconds);

            if (phase != Phase.Explanation)
                yield break;

            ShowHeartOvalSpotlight(false);

            yield return new WaitForSecondsRealtime(
                flashHiddenSeconds);
        }

        if (phase == Phase.Explanation)
            ShowHeartOvalSpotlight(true);

        flashRoutine = null;
    }

    private void SetKeeperMessage(string value)
    {
        if (leftTutorialText == null)
            return;

        leftTutorialText.gameObject.SetActive(true);
        leftTutorialText.text = value ?? "";
        leftTutorialText.ForceMeshUpdate();

        if (leftFrameCanvasGroup != null)
            leftFrameCanvasGroup.alpha = 1f;
    }

    private void SetGoldNextVisible(bool visible)
    {
        if (leftFrameCanvasGroup != null)
        {
            leftFrameCanvasGroup.interactable = visible;
            leftFrameCanvasGroup.blocksRaycasts = visible;
        }

        if (goldNextButton != null)
        {
            goldNextButton.gameObject.SetActive(visible);
            goldNextButton.interactable = visible;
        }

        if (goldNextPulse != null)
            goldNextPulse.SetPulseActive(visible);
    }

    private void HideKeeperFrame()
    {
        if (leftFrameCanvasGroup != null)
        {
            leftFrameCanvasGroup.alpha = 0f;
            leftFrameCanvasGroup.interactable = false;
            leftFrameCanvasGroup.blocksRaycasts = false;
        }

        if (leftTutorialText != null)
            leftTutorialText.text = "";
    }

    private void CaptureKeeperBaseline()
    {
        if (keeperBaselineCaptured ||
            keeperImage == null)
        {
            return;
        }

        keeperBaselineCaptured = true;

        RectTransform r = keeperImage.rectTransform;

        keeperBaselineSprite = keeperImage.sprite;
        keeperBaselinePosition = r.anchoredPosition;
        keeperBaselineSize = r.sizeDelta;
        keeperBaselineRotation = r.localRotation;
        keeperBaselineScale = r.localScale;

        if (keeperPuzzleIcon != null)
        {
            puzzleBaselinePosition =
                keeperPuzzleIcon.anchoredPosition;

            puzzleBaselineSize =
                keeperPuzzleIcon.sizeDelta;

            puzzleBaselineRotation =
                keeperPuzzleIcon.localRotation;

            puzzleBaselineScale =
                keeperPuzzleIcon.localScale;
        }
    }

    private void ApplyKeeperPose(
        IndependentKeeperPose pose,
        Sprite fallbackSprite = null)
    {
        if (keeperImage == null || pose == null)
            return;

        if (pose.keeperSprite != null)
            keeperImage.sprite = pose.keeperSprite;
        else if (fallbackSprite != null)
            keeperImage.sprite = fallbackSprite;

        RectTransform r = keeperImage.rectTransform;

        r.anchoredPosition = pose.keeperPosition;
        r.sizeDelta = pose.keeperSize;

        r.localRotation = Quaternion.Euler(
            0f,
            0f,
            pose.keeperRotationZ);

        r.localScale = new Vector3(
            pose.keeperScale.x,
            pose.keeperScale.y,
            r.localScale.z);

        if (keeperPuzzleIcon == null)
            return;

        keeperPuzzleIcon.anchoredPosition =
            pose.puzzlePosition;

        keeperPuzzleIcon.sizeDelta =
            pose.puzzleSize;

        keeperPuzzleIcon.localRotation = Quaternion.Euler(
            0f,
            0f,
            pose.puzzleRotationZ);

        keeperPuzzleIcon.localScale = new Vector3(
            pose.puzzleScale.x,
            pose.puzzleScale.y,
            keeperPuzzleIcon.localScale.z);
    }

    private void RestoreKeeperBaseline()
    {
        if (!keeperBaselineCaptured ||
            keeperImage == null)
        {
            return;
        }

        RectTransform r = keeperImage.rectTransform;

        keeperImage.sprite = keeperBaselineSprite;
        r.anchoredPosition = keeperBaselinePosition;
        r.sizeDelta = keeperBaselineSize;
        r.localRotation = keeperBaselineRotation;
        r.localScale = keeperBaselineScale;

        if (keeperPuzzleIcon == null)
            return;

        keeperPuzzleIcon.anchoredPosition =
            puzzleBaselinePosition;

        keeperPuzzleIcon.sizeDelta =
            puzzleBaselineSize;

        keeperPuzzleIcon.localRotation =
            puzzleBaselineRotation;

        keeperPuzzleIcon.localScale =
            puzzleBaselineScale;
    }

    private void PauseAndLockGameplay()
    {
        if (!pausedByLesson)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            pausedByLesson = true;
        }

        if (!movementLockedByLesson &&
            playerController != null)
        {
            playerController.SetHorizontalMovementLock(true);
            movementLockedByLesson = true;
        }

        LockControlCanvasGroups();
    }

    private void LockControlCanvasGroups()
    {
        if (lockedControlGroups.Count > 0)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        TryLockControlGroup(
            canvas.transform.Find("ActionControls"));

        TryLockControlGroup(
            canvas.transform.Find("MobileControls"));

        TryLockControlGroup(
            canvas.transform.Find("InventoryPanel"));
    }

    private void TryLockControlGroup(Transform target)
    {
        if (target == null)
            return;

        CanvasGroup group =
            target.GetComponent<CanvasGroup>();

        if (group == null)
            group = target.gameObject.AddComponent<CanvasGroup>();

        lockedControlGroups.Add(group);

        lockedControlPreviousRaycasts.Add(
            group.blocksRaycasts);

        group.blocksRaycasts = false;
    }

    private void ReleaseGameplay()
    {
        for (int i = 0;
            i < lockedControlGroups.Count;
            i++)
        {
            if (lockedControlGroups[i] != null)
            {
                lockedControlGroups[i].blocksRaycasts =
                    lockedControlPreviousRaycasts[i];
            }
        }

        lockedControlGroups.Clear();
        lockedControlPreviousRaycasts.Clear();

        if (movementLockedByLesson &&
            playerController != null)
        {
            playerController.SetHorizontalMovementLock(false);
        }

        movementLockedByLesson = false;

        if (pausedByLesson)
            Time.timeScale = previousTimeScale;

        pausedByLesson = false;
    }

    private void ShowPlainDimmer()
    {
        RestoreDimmerImage();

        if (tutorialDimmer != null)
            tutorialDimmer.SetActive(true);

        if (dimmerImage != null)
            dimmerImage.raycastTarget = false;
    }

    private void ShowHeartOvalSpotlight(bool bright)
    {
        if (tutorialDimmer == null ||
            dimmerImage == null ||
            heartsContainer == null ||
            Screen.width <= 0 ||
            Screen.height <= 0)
        {
            ShowPlainDimmer();
            return;
        }

        Vector2 min = new Vector2(
            float.PositiveInfinity,
            float.PositiveInfinity);

        Vector2 max = new Vector2(
            float.NegativeInfinity,
            float.NegativeInfinity);

        AddRectScreenBounds(
            heartsContainer,
            ref min,
            ref max);

        if (bonusHeartBadge != null &&
            bonusHeartBadge.gameObject.activeInHierarchy)
        {
            AddRectScreenBounds(
                bonusHeartBadge,
                ref min,
                ref max);
        }

        if (float.IsInfinity(min.x) ||
            float.IsInfinity(min.y))
        {
            ShowPlainDimmer();
            return;
        }

        float screenW = Screen.width;
        float screenH = Screen.height;

        float width = Mathf.Max(
            0.01f,
            ((max.x - min.x) / screenW +
             2f * ovalExtraWidth) *
            ovalWidthMultiplier);

        float height = Mathf.Max(
            0.01f,
            ((max.y - min.y) / screenH +
             2f * ovalExtraHeight) *
            ovalHeightMultiplier);

        float cx =
            (min.x + max.x) * 0.5f / screenW +
            ovalOffsetX;

        float cy =
            (min.y + max.y) * 0.5f / screenH +
            ovalOffsetY;

        float radiusX = width * 0.5f;
        float radiusY = height * 0.5f;

        EnsureMaskTexture();

        int n = maskTexture.width;

        float insideAlpha =
            bright ? 0f : 0.92f;

        for (int y = 0; y < n; y++)
        {
            float ny = (float)y / (n - 1);

            for (int x = 0; x < n; x++)
            {
                float nx = (float)x / (n - 1);

                float dx = (nx - cx) / radiusX;
                float dy = (ny - cy) / radiusY;

                float distance = Mathf.Sqrt(
                    dx * dx + dy * dy);

                float t = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        (distance - 1f) /
                        ovalEdgeSoftness));

                float alpha = Mathf.Lerp(
                    insideAlpha,
                    1f,
                    t);

                maskPixels[y * n + x] =
                    new Color32(
                        255,
                        255,
                        255,
                        (byte)Mathf.RoundToInt(
                            alpha * 255f));
            }
        }

        maskTexture.SetPixels32(maskPixels);
        maskTexture.Apply(false, false);

        dimmerImage.sprite = maskSprite;
        dimmerImage.type = Image.Type.Simple;
        dimmerImage.preserveAspect = false;
        dimmerImage.raycastTarget = false;

        tutorialDimmer.SetActive(true);
    }

    private static void AddRectScreenBounds(
        RectTransform rect,
        ref Vector2 min,
        ref Vector2 max)
    {
        Canvas canvas =
            rect.GetComponentInParent<Canvas>();

        Camera cam =
            canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? (canvas.worldCamera != null
                    ? canvas.worldCamera
                    : Camera.main)
                : null;

        Vector3[] corners = new Vector3[4];

        rect.GetWorldCorners(corners);

        foreach (Vector3 corner in corners)
        {
            Vector2 p =
                RectTransformUtility.WorldToScreenPoint(
                    cam,
                    corner);

            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
    }

    private void EnsureMaskTexture()
    {
        int n = Mathf.Clamp(
            spotlightTextureResolution,
            64,
            512);

        if (maskTexture != null &&
            maskTexture.width == n)
        {
            return;
        }

        if (maskSprite != null)
            Destroy(maskSprite);

        if (maskTexture != null)
            Destroy(maskTexture);

        maskTexture = new Texture2D(
            n,
            n,
            TextureFormat.RGBA32,
            false);

        maskTexture.wrapMode = TextureWrapMode.Clamp;
        maskTexture.filterMode = FilterMode.Bilinear;

        maskPixels = new Color32[n * n];

        maskSprite = Sprite.Create(
            maskTexture,
            new Rect(0, 0, n, n),
            new Vector2(.5f, .5f),
            100f);

        maskSprite.name =
            "TutorialHeartOvalSpotlight_Runtime";
    }

    private void RestoreDimmerImage()
    {
        if (dimmerImage == null)
            return;

        dimmerImage.sprite = originalDimmerSprite;
        dimmerImage.type = originalDimmerType;

        dimmerImage.preserveAspect =
            originalDimmerPreserveAspect;

        dimmerImage.raycastTarget =
            originalDimmerRaycastTarget;
    }

    private void RestoreDimmer()
    {
        RestoreDimmerImage();

        if (tutorialDimmer != null)
            tutorialDimmer.SetActive(false);
    }

    private void ResolveReferences()
    {
        if (leftFrameCanvasGroup == null)
        {
            leftFrameCanvasGroup =
                GetComponent<CanvasGroup>();
        }

        if (leftTutorialText == null)
        {
            Transform t =
                transform.Find("TutorialText_Left");

            if (t != null)
                leftTutorialText =
                    t.GetComponent<TMP_Text>();
        }

        if (goldNextButton == null)
        {
            Transform t =
                transform.Find("TutorialNextButton_Left");

            if (t != null)
                goldNextButton =
                    t.GetComponent<Button>();
        }

        if (goldNextPulse == null &&
            goldNextButton != null)
        {
            goldNextPulse =
                goldNextButton.GetComponent<TutorialArrowPulse>();
        }

        if (keeperImage == null)
        {
            Transform t =
                transform.Find("PuzzleKeeper_Left");

            if (t != null)
                keeperImage = t.GetComponent<Image>();
        }

        if (keeperPuzzleIcon == null &&
            keeperImage != null)
        {
            Transform t =
                keeperImage.transform.Find("KeeperPuzzleIcon");

            if (t != null)
                keeperPuzzleIcon =
                    t.GetComponent<RectTransform>();
        }

        if (tutorialDimmer == null &&
            transform.parent != null)
        {
            Transform t =
                transform.parent.Find("TutorialDimmer");

            if (t != null)
                tutorialDimmer = t.gameObject;
        }

        if (dimmerImage == null &&
            tutorialDimmer != null)
        {
            dimmerImage =
                tutorialDimmer.GetComponent<Image>();
        }

        if (heartsUI == null)
        {
            heartsUI =
                FindFirstObjectByType<HeartsUI>();
        }

        if (heartsUI != null)
        {
            if (heartsContainer == null)
            {
                Transform t =
                    heartsUI.transform.Find("HeartsContainer");

                if (t != null)
                    heartsContainer =
                        t.GetComponent<RectTransform>();
            }

            if (bonusHeartBadge == null)
            {
                Transform t =
                    heartsUI.transform.Find("BonusHeartBadge");

                if (t != null)
                    bonusHeartBadge =
                        t.GetComponent<RectTransform>();
            }
        }

        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<PlayerController>();
        }

        if (firstHeartPickup == null &&
            firstHeartBoxTarget != null)
        {
            firstHeartPickup =
                firstHeartBoxTarget
                    .GetComponentInChildren<BonusHeartPickup>(true);
        }
    }

    private void OnLocaleChanged(Locale locale)
    {
        LoadLocalizedLines();
    }

    private void LoadLocalizedLines()
    {
        localizationVersion++;

        if (localizationRoutine != null)
            StopCoroutine(localizationRoutine);

        localizationRoutine =
            StartCoroutine(
                LoadLocalizedLinesRoutine(
                    localizationVersion));
    }

    private IEnumerator LoadLocalizedLinesRoutine(
        int version)
    {
        string[] keys =
        {
            congratulationsKey,
            explanationKey,
            survivalKey,
            warningKey
        };

        string[] lines =
        {
            congratulationsFallback,
            explanationFallback,
            survivalFallback,
            warningFallback
        };

        for (int i = 0; i < keys.Length; i++)
        {
            var request =
                LocalizationSettings.StringDatabase
                    .GetLocalizedStringAsync(
                        localizationTable,
                        keys[i]);

            yield return request;

            if (localizationVersion != version)
                yield break;

            if (request.Status ==
                    AsyncOperationStatus.Succeeded &&
                !string.IsNullOrWhiteSpace(request.Result))
            {
                lines[i] = request.Result;
            }
        }

        localizedCongratulations = lines[0];
        localizedExplanation = lines[1];
        localizedSurvival = lines[2];
        localizedWarning = lines[3];

        localizationRoutine = null;

        if (phase == Phase.Congratulations)
        {
            SetKeeperMessage(localizedCongratulations);
        }
        else if (phase == Phase.Explanation)
        {
            SetKeeperMessage(localizedExplanation);
        }
        else if (phase == Phase.FarewellSurvival)
        {
            SetKeeperMessage(localizedSurvival);
        }
        else if (phase == Phase.FarewellWarning)
        {
            SetKeeperMessage(localizedWarning);
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -=
            OnLocaleChanged;

        if (goldNextButton != null)
        {
            goldNextButton.onClick.RemoveListener(
                OnGoldNextClicked);
        }

        if (firstHeartPickup != null)
        {
            firstHeartPickup.PickupCompleted -=
                OnFirstHeartPickupCompleted;
        }

        localizationVersion++;

        if (localizationRoutine != null)
            StopCoroutine(localizationRoutine);

        localizationRoutine = null;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = null;

        if (pickupWaitRoutine != null)
            StopCoroutine(pickupWaitRoutine);

        pickupWaitRoutine = null;

        if (goodbyeFadeRoutine != null)
            StopCoroutine(goodbyeFadeRoutine);

        goodbyeFadeRoutine = null;

        if (phase == Phase.Congratulations ||
            phase == Phase.Explanation ||
            phase == Phase.FarewellSurvival ||
            phase == Phase.FarewellWarning ||
            phase == Phase.FadingOut)
        {
            SetGoldNextVisible(false);
            HideKeeperFrame();
            RestoreDimmer();
            RestoreKeeperBaseline();
            ReleaseGameplay();
        }
    }

    private void OnDestroy()
    {
        if (maskSprite != null)
            Destroy(maskSprite);

        if (maskTexture != null)
            Destroy(maskTexture);
    }

    private void OnValidate()
    {
        resumeNearBoxDistance =
            Mathf.Max(0.5f, resumeNearBoxDistance);

        afterPickupUIWait =
            Mathf.Max(0f, afterPickupUIWait);

        ovalWidthMultiplier =
            Mathf.Max(0.2f, ovalWidthMultiplier);

        ovalHeightMultiplier =
            Mathf.Max(0.2f, ovalHeightMultiplier);

        ovalEdgeSoftness =
            Mathf.Clamp(ovalEdgeSoftness, 0.002f, 0.2f);

        flashCount = Mathf.Clamp(flashCount, 1, 10);

        flashVisibleSeconds =
            Mathf.Max(0.05f, flashVisibleSeconds);

        flashHiddenSeconds =
            Mathf.Max(0.05f, flashHiddenSeconds);

        flashStartDelay =
            Mathf.Max(0f, flashStartDelay);

        goodbyeFadeSeconds =
            Mathf.Max(0f, goodbyeFadeSeconds);
    }
}
