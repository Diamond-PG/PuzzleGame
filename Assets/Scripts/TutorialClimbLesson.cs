using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Tutorial after tutorial_25.
/// tutorial_26: arrows question + two arrow spotlights + yellow Next.
/// tutorial_27: actual up/down practice, no Next.
/// tutorial_28: stick question + stick spotlight + yellow Next.
/// tutorial_29: approach question with gold Next.
/// tutorial_30: jump + right tip; gameplay resumes after gold Next.
/// Uses the EXISTING left frame, its Next button and its dimmer.
/// </summary>
[DisallowMultipleComponent]
public class TutorialClimbLesson : MonoBehaviour
{
    // These stages follow the existing TutorialLeftFrameReveal stages 0..4.
    // Keep their values stable after releasing the lesson.
    private const int StageArrowsQuestion = 5;
    private const int StagePracticeNone = 6;
    private const int StagePracticeUp = 7;
    private const int StagePracticeDown = 8;
    private const int StageStickQuestion = 9;
    // Stage 10 is kept compatible with already saved free movement.
    private const int StageWalkToStick = 10;
    private const int StageApproachExplanation = 11;
    private const int StageJumpExplanation = 12;

    [Header("01 - EXISTING TUTORIAL REFERENCES")]
    [SerializeField] private TutorialLeftFrameReveal leftFrame;
    [SerializeField] private TMP_Text leftTutorialText;
    [SerializeField] private GameObject tutorialDimmer;
    [SerializeField] private MobileInput mobileInput;
    [SerializeField] private PlayerController playerController;

    [Header("02 - UP / DOWN BUTTON GRAPHICS")]
    [Tooltip("UP arrow visible graphic or its hitbox.")]
    [SerializeField] private RectTransform upButtonGraphic;
    [Tooltip("DOWN arrow visible graphic or its hitbox.")]
    [SerializeField] private RectTransform downButtonGraphic;

    [Header("03 - STICK SPOTLIGHT")]
    [Tooltip("World Transform of the stick to spotlight at tutorial_28.")]
    [SerializeField] private Transform stickTarget;
    [SerializeField] private Camera worldCamera;
    [SerializeField, Min(0f)] private float stickPadding = 0.025f;
    [SerializeField, Min(0.05f)] private float minimumStickWindowWidth = 0.10f;
    [SerializeField, Min(0.05f)] private float minimumStickWindowHeight = 0.12f;

    [Header("04 - CLIMB PRACTICE")]
    [Tooltip("Real vertical distance to cover in EACH direction, in world units.")]
    [SerializeField, Min(0.02f)] private float requiredTravelEachDirection = 0.12f;
    [Tooltip("Ignore the vertical button still held during the start of practice until it is released once. Gameplay resumes after Next on tutorial_26.")]
    [SerializeField] private bool pauseUntilFirstVerticalPress = true;

    [Header("05 - SPOTLIGHT")]
    [SerializeField, Range(64, 512)] private int textureResolution = 256;
    [SerializeField, Range(0f, 0.08f)] private float windowPadding = 0.012f;
    [Tooltip("Size multiplier for BOTH circular UP/DOWN spotlights. 0.65 = smaller; 1 = original.")]
    [SerializeField, Range(0.35f, 1.25f)] private float arrowSpotlightScale = 0.72f;
    [SerializeField, Range(0.001f, 0.08f)] private float windowSoftness = 0.012f;

    [Header("06 - LOCALIZATION: FIVE SHORT PAGES")]
    [SerializeField] private string localizationTable = "Tutorial";

    [SerializeField] private string arrowsQuestionKey = "tutorial_26";
    [SerializeField, TextArea(2, 4)]
    private string arrowsQuestionFallback = "Видишь стрелки вверх и вниз?";

    [SerializeField] private string climbPracticeKey = "tutorial_27";
    [SerializeField, TextArea(2, 4)]
    private string climbPracticeFallback = "Попробуй подняться и спуститься.";

    [SerializeField] private string stickQuestionKey = "tutorial_28";
    [SerializeField, TextArea(2, 4)]
    private string stickQuestionFallback = "Отлично! Видишь палку наверху?";

    [SerializeField] private string stickApproachKey = "tutorial_29";
    [SerializeField, TextArea(2, 4)]
    private string stickApproachFallback = "Поднимись и подойди к ней.";

    [SerializeField] private string jumpHintKey = "tutorial_30";
    [SerializeField, TextArea(2, 4)]
    private string jumpHintFallback =
        "Чтобы запрыгнуть, нажми Прыжок + →";

    // ============================================================
    // 07 - KEEPER POSES (SPRITES CAN BE ASSIGNED LATER)
    // ============================================================

    [System.Serializable]
    private class TutorialKeeperPose
    {
        [Tooltip("Leave None until your matching Guardian sprite is ready.")]
        public Sprite keeperSprite;

        [Header("Keeper RectTransform")]
        public Vector2 keeperPosition = new Vector2(134.8f, -221f);
        public Vector2 keeperSize = new Vector2(230f, 230f);
        public float keeperRotationZ;
        public Vector2 keeperScale = Vector2.one;

        [Header("Blue puzzle icon RectTransform")]
        public Vector2 puzzlePosition = new Vector2(6.9f, -63.8f);
        public Vector2 puzzleSize = new Vector2(27f, 27f);
        public float puzzleRotationZ = 20f;
        public Vector2 puzzleScale = Vector2.one;
    }

    [Header("07 - KEEPER POSES: INDEPENDENT 26-27, 28-29, 30")]
    [SerializeField] private Image keeperImage;
    [SerializeField] private RectTransform keeperPuzzleIcon;

    [Tooltip("26/27: looking down and pointing down. These transforms are independent.")]
    [SerializeField] private TutorialKeeperPose downPointingPose =
        new TutorialKeeperPose();

    [Tooltip("28/29: pointing toward the stick. Assign the existing sideways-pointing Guardian sprite here and adjust its own blue puzzle icon independently of 26/27 and 30. With no sprite assigned, the previous baseline pose is used.")]
    [SerializeField] private TutorialKeeperPose sidePointingPose =
        new TutorialKeeperPose();

    [Tooltip("30: open hand. These transforms are independent.")]
    [SerializeField] private TutorialKeeperPose openPalmPose =
        new TutorialKeeperPose();

    // The captured baseline remains available as a safe fallback while
    // the independent 28/29 sprite has not yet been assigned.
    private bool keeperBaselineCaptured;
    private Sprite baselineKeeperSprite;
    private Vector2 baselineKeeperPosition;
    private Vector2 baselineKeeperSize;
    private Quaternion baselineKeeperRotation;
    private Vector3 baselineKeeperScale;
    private Vector2 baselinePuzzlePosition;
    private Vector2 baselinePuzzleSize;
    private Quaternion baselinePuzzleRotation;
    private Vector3 baselinePuzzleScale;

    // Existing gold arrow from TutorialLeftFrameReveal, not a new UI element.
    private Button leftNextButton;
    private TutorialArrowPulse leftNextPulse;
    private CanvasGroup leftFrameCanvasGroup;

    private Image dimmerImage;
    private Sprite originalDimmerSprite;
    private Image.Type originalDimmerType;
    private bool originalPreserveAspect;
    private bool originalRaycastTarget;
    private Texture2D maskTexture;
    private Sprite maskSprite;
    private Color32[] maskPixels;

    private enum Phase
    {
        Waiting,
        ArrowsQuestion,
        Practice,
        StickQuestion,
        StickApproach,
        JumpHint,
        Finished
    }

    private Phase phase = Phase.Waiting;
    private bool upDone;
    private bool downDone;
    private float lastPlayerY;
    private bool hasLastPlayerY;
    private Transform hero;
    private bool pausedByThis;
    private bool movementLockApplied;
    private float previousTimeScale = 1f;
    private bool practiceInputReleased;

    private string arrowsQuestionText;
    private string climbPracticeText;
    private string stickQuestionText;
    private string stickApproachText;
    private string jumpHintText;
    private Coroutine localeRoutine;
    private int localeVersion;
    private bool isReady;

    private void Awake()
    {
        ResolveReferences();
        if (dimmerImage != null)
        {
            originalDimmerSprite = dimmerImage.sprite;
            originalDimmerType = dimmerImage.type;
            originalPreserveAspect = dimmerImage.preserveAspect;
            originalRaycastTarget = dimmerImage.raycastTarget;
        }

        arrowsQuestionText = arrowsQuestionFallback;
        climbPracticeText = climbPracticeFallback;
        stickQuestionText = stickQuestionFallback;
        stickApproachText = stickApproachFallback;
        jumpHintText = jumpHintFallback;
    }

    private void OnEnable()
    {
        ClimbHook.PlayerGrabbedHook += OnHookGrabbed;
        ClimbHook.TutorialSingleTapCompleted += OnTutorialSingleTapCompleted;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        ResolveReferences();
        BindNextButton();
    }

    private void Start()
    {
        ResolveReferences();
        BindNextButton();
        isReady = true;
        int saved = TutorialProgressManager.SavedLeftTutorialStage;
        if (saved == StageWalkToStick)
            phase = Phase.Finished;

        // Previously learned arrows remain available after reloading.
        if (saved >= StagePracticeNone || TutorialProgressManager.ClimbCompleted)
            SetClimbButtonsEnabled(true);

        BeginLocalizationLoad();
    }

    private void OnDisable()
    {
        ClimbHook.PlayerGrabbedHook -= OnHookGrabbed;
        ClimbHook.TutorialSingleTapCompleted -= OnTutorialSingleTapCompleted;
        ClimbHook.EndTutorialSingleTapClimb();
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        if (leftNextButton != null)
            leftNextButton.onClick.RemoveListener(OnGoldNextClicked);
        localeVersion++;
        if (localeRoutine != null) StopCoroutine(localeRoutine);
        localeRoutine = null;
        if (phase != Phase.Waiting) SetGoldNextVisible(false);
        RestoreKeeperBaseline();
        RestoreDimmer();
        ResumeGame();
        UnlockOtherActions();
    }

    private void OnDestroy()
    {
        if (maskSprite != null) Destroy(maskSprite);
        if (maskTexture != null) Destroy(maskTexture);
    }

    private void ResolveReferences()
    {
        if (leftFrame == null) leftFrame = GetComponent<TutorialLeftFrameReveal>();
        if (leftFrame == null) leftFrame = FindFirstObjectByType<TutorialLeftFrameReveal>();
        if (leftTutorialText == null && leftFrame != null)
        {
            Transform found = leftFrame.transform.Find("TutorialText_Left");
            if (found != null) leftTutorialText = found.GetComponent<TMP_Text>();
        }
        if (tutorialDimmer == null && leftFrame != null && leftFrame.transform.parent != null)
        {
            Transform found = leftFrame.transform.parent.Find("TutorialDimmer");
            if (found != null) tutorialDimmer = found.gameObject;
        }
        if (dimmerImage == null && tutorialDimmer != null)
            dimmerImage = tutorialDimmer.GetComponent<Image>();
        if (mobileInput == null) mobileInput = FindFirstObjectByType<MobileInput>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (worldCamera == null) worldCamera = Camera.main;
        if (keeperImage == null && leftFrame != null)
        {
            Transform keeper = leftFrame.transform.Find("PuzzleKeeper_Left");
            if (keeper != null) keeperImage = keeper.GetComponent<Image>();
        }
        if (keeperPuzzleIcon == null && keeperImage != null)
        {
            Transform icon = keeperImage.transform.Find("KeeperPuzzleIcon");
            if (icon != null) keeperPuzzleIcon = icon.GetComponent<RectTransform>();
        }

        if (leftFrame != null)
        {
            if (leftFrameCanvasGroup == null)
                leftFrameCanvasGroup = leftFrame.GetComponent<CanvasGroup>();
            if (leftNextButton == null)
            {
                Transform child = leftFrame.transform.Find("TutorialNextButton_Left");
                if (child != null) leftNextButton = child.GetComponent<Button>();
            }
            if (leftNextButton != null && leftNextPulse == null)
                leftNextPulse = leftNextButton.GetComponent<TutorialArrowPulse>();
        }
    }

    private void BindNextButton()
    {
        if (leftNextButton == null) return;
        leftNextButton.onClick.RemoveListener(OnGoldNextClicked);
        leftNextButton.onClick.AddListener(OnGoldNextClicked);
    }

    private void SetGoldNextVisible(bool visible)
    {
        if (leftNextButton == null) return;
        // Old frame disables its UI raycasts after tutorial_25.
        if (leftFrameCanvasGroup != null)
        {
            leftFrameCanvasGroup.interactable = visible;
            leftFrameCanvasGroup.blocksRaycasts = visible;
        }
        leftNextButton.gameObject.SetActive(visible);
        leftNextButton.interactable = visible;
        if (leftNextPulse != null) leftNextPulse.SetPulseActive(visible);
    }

    private void OnGoldNextClicked()
    {
        if (phase == Phase.ArrowsQuestion)
        {
            StartPractice();
        }
        else if (phase == Phase.StickQuestion)
        {
            ShowApproachExplanation();
        }
        else if (phase == Phase.StickApproach)
        {
            ShowJumpExplanation();
        }
        else if (phase == Phase.JumpHint)
        {
            ContinueToStick();
        }
    }

    // ============================================================
    // ENABLE THE REAL UP/DOWN HITBOXES AND THEIR VISIBLE SPRITES
    // ============================================================

    private void SetClimbButtonsEnabled(bool interactable)
    {
        SetClimbButtonEnabled(upButtonGraphic, interactable);
        SetClimbButtonEnabled(downButtonGraphic, interactable);
    }

    private static void SetClimbButtonEnabled(RectTransform graphic, bool interactable)
    {
        if (graphic == null) return;

        // The visible BtnUp / BtnDown is a child of BtnUp_Hitbox /
        // BtnDown_Hitbox. The CanvasGroup that makes the arrows dim
        // and non-clickable lives on the HITBOX, not on the graphic.
        Transform hitbox = graphic.parent;
        CanvasGroup group = hitbox != null ? hitbox.GetComponent<CanvasGroup>() : null;
        if (group == null)
            group = graphic.GetComponent<CanvasGroup>();

        if (hitbox != null && !hitbox.gameObject.activeSelf)
            hitbox.gameObject.SetActive(true);
        if (!graphic.gameObject.activeSelf)
            graphic.gameObject.SetActive(true);

        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = interactable;
            group.blocksRaycasts = interactable;
        }
    }

    private void OnHookGrabbed(ClimbHook hook)
    {
        if (!isReady || phase != Phase.Waiting || leftFrame == null ||
            !leftFrame.IsWaitingForLadder || !TutorialProgressManager.DoorBroken)
            return;

        ResolveReferences();
        BindNextButton();
        if (leftTutorialText == null || tutorialDimmer == null || dimmerImage == null ||
            leftNextButton == null)
        {
            Debug.LogWarning("TutorialClimbLesson: Left text, dimmer or gold Next is missing.", this);
            return;
        }
        if (upButtonGraphic == null || downButtonGraphic == null)
        {
            Debug.LogWarning("TutorialClimbLesson: Assign both UP / DOWN graphics.", this);
            return;
        }

        // At this point tutorial_25 has already selected the existing
        // sideways-pointing Keeper. Preserve it for tutorial_28/29.
        CaptureKeeperBaseline();

        hero = playerController != null ? playerController.transform : null;
        if (hero == null)
        {
            Debug.LogWarning("TutorialClimbLesson: PlayerController is missing.", this);
            return;
        }

        int saved = TutorialProgressManager.SavedLeftTutorialStage;
        if (saved == StageWalkToStick)
        {
            phase = Phase.Finished;
            SetClimbButtonsEnabled(true);
            HideKeeperFrame();
            return;
        }

        upDone = saved == StagePracticeUp ||
                 saved == StageStickQuestion ||
                 saved == StageApproachExplanation ||
                 saved == StageJumpExplanation;
        downDone = saved == StagePracticeDown ||
                   saved == StageStickQuestion ||
                   saved == StageApproachExplanation ||
                   saved == StageJumpExplanation;
        lastPlayerY = hero.position.y;
        hasLastPlayerY = true;
        TutorialProgressManager.MarkLadderReached();

        // Restore the exact lesson part after reload.
        if (saved == StageJumpExplanation)
        {
            ShowJumpExplanation();
        }
        else if (saved == StageApproachExplanation)
        {
            ShowApproachExplanation();
        }
        else if (saved == StageStickQuestion || TutorialProgressManager.ClimbCompleted)
        {
            ShowStickQuestion();
        }
        else if (saved >= StagePracticeNone && saved <= StagePracticeDown)
        {
            StartPractice();
        }
        else
        {
            ShowArrowsQuestion();
        }
    }

    private void ShowArrowsQuestion()
    {
        phase = Phase.ArrowsQuestion;
        ApplyKeeperPose(downPointingPose);
        LockOtherActions();
        TutorialProgressManager.SaveLeftTutorialStage(StageArrowsQuestion);
        ShowMessage(arrowsQuestionText);
        SetClimbButtonsEnabled(false);
        ShowArrowSpotlights();
        SetGoldNextVisible(true);
        PauseGame();
    }

    private void StartPractice()
    {
        phase = Phase.Practice;
        ApplyKeeperPose(downPointingPose);
        LockOtherActions();
        TutorialProgressManager.SaveLeftTutorialStage(
            upDone ? StagePracticeUp : downDone ? StagePracticeDown : StagePracticeNone);
        ShowMessage(climbPracticeText);
        SetClimbButtonsEnabled(true);
        ShowArrowSpotlights();
        SetGoldNextVisible(false);
        practiceInputReleased = !pauseUntilFirstVerticalPress;
        lastPlayerY = hero != null ? hero.position.y : 0f;
        hasLastPlayerY = hero != null;
        // A SINGLE short movement is allowed for each UP/DOWN press.
        // Holding either button has no effect beyond that short step.
        ClimbHook.BeginTutorialSingleTapClimb(
            requiredTravelEachDirection, upDone, downDone,
            pauseUntilFirstVerticalPress);

        // During practice, physics runs but movement is safely bounded.
        ResumeGame();
    }

    // ============================================================
    // TUTORIAL 27 - COMPLETE ONE TAP IN EACH DIRECTION
    // ============================================================

    private void OnTutorialSingleTapCompleted(ClimbHook hook, int direction)
    {
        if (phase != Phase.Practice || !ClimbHook.PlayerIsOnHook)
            return;

        // No fixed order. Each direction is credited only once.
        if (direction > 0 && !upDone)
        {
            upDone = true;
            TutorialProgressManager.SaveLeftTutorialStage(StagePracticeUp);
        }
        else if (direction < 0 && !downDone)
        {
            downDone = true;
            TutorialProgressManager.SaveLeftTutorialStage(StagePracticeDown);
        }

        if (upDone && downDone)
        {
            ClimbHook.EndTutorialSingleTapClimb();
            TutorialProgressManager.CompleteClimb();
            ShowStickQuestion();
        }
    }

    private void ShowStickQuestion()
    {
        ClimbHook.EndTutorialSingleTapClimb();
        phase = Phase.StickQuestion;
        ApplyKeeperPose(sidePointingPose);
        LockOtherActions();
        TutorialProgressManager.SaveLeftTutorialStage(StageStickQuestion);
        ShowMessage(stickQuestionText);
        SetClimbButtonsEnabled(false);
        ShowStickSpotlightOrDimmer();
        SetGoldNextVisible(true);
        PauseGame();
    }

    private void ShowApproachExplanation()
    {
        phase = Phase.StickApproach;
        ApplyKeeperPose(sidePointingPose);
        LockOtherActions();
        TutorialProgressManager.SaveLeftTutorialStage(StageApproachExplanation);
        ShowMessage(stickApproachText);
        SetClimbButtonsEnabled(false);
        ShowStickSpotlightOrDimmer();
        SetGoldNextVisible(true);
        PauseGame();
    }

    private void ShowJumpExplanation()
    {
        phase = Phase.JumpHint;
        ApplyKeeperPose(openPalmPose);
        LockOtherActions();
        TutorialProgressManager.SaveLeftTutorialStage(StageJumpExplanation);
        ShowMessage(jumpHintText);
        SetClimbButtonsEnabled(false);
        ShowStickSpotlightOrDimmer();
        SetGoldNextVisible(true);
        PauseGame();
    }

    private void ShowStickSpotlightOrDimmer()
    {
        if (stickTarget != null && TryGetStickViewportRect(out Rect rect))
            RenderSpotlight(rect, new Rect(-1f, -1f, 0f, 0f), false);
        else
            ShowPlainDimmer();
    }

    private void ContinueToStick()
    {
        ClimbHook.EndTutorialSingleTapClimb();
        phase = Phase.Finished;
        // Stage 10 was already the free-to-play checkpoint in existing saves.
        TutorialProgressManager.SaveLeftTutorialStage(StageWalkToStick);
        SetClimbButtonsEnabled(true);
        SetGoldNextVisible(false);
        RestoreDimmer();
        ResumeGame();
        UnlockOtherActions();
        HideKeeperFrame();
    }

    private void LockOtherActions()
    {
        // Keep only UP / DOWN climbing available during these instructions.
        // ClimbHook reads MobileInput.Vertical independently of this lock.
        if (playerController == null) return;
        playerController.SetHorizontalMovementLock(true);
        movementLockApplied = true;
    }

    private void UnlockOtherActions()
    {
        if (!movementLockApplied) return;
        if (playerController != null)
            playerController.SetHorizontalMovementLock(false);
        movementLockApplied = false;
    }

    // ============================================================
    // KEEPER POSE HELPERS (OPTIONAL UNTIL ART IS READY)
    // ============================================================

    private void CaptureKeeperBaseline()
    {
        if (keeperBaselineCaptured || keeperImage == null)
            return;

        RectTransform r = keeperImage.rectTransform;
        baselineKeeperSprite = keeperImage.sprite;
        baselineKeeperPosition = r.anchoredPosition;
        baselineKeeperSize = r.sizeDelta;
        baselineKeeperRotation = r.localRotation;
        baselineKeeperScale = r.localScale;

        if (keeperPuzzleIcon != null)
        {
            baselinePuzzlePosition = keeperPuzzleIcon.anchoredPosition;
            baselinePuzzleSize = keeperPuzzleIcon.sizeDelta;
            baselinePuzzleRotation = keeperPuzzleIcon.localRotation;
            baselinePuzzleScale = keeperPuzzleIcon.localScale;
        }
        keeperBaselineCaptured = true;
    }

    private void ApplyKeeperPose(TutorialKeeperPose pose)
    {
        // Do not alter the working Keeper until the new sprite has
        // actually been assigned by the user in the Inspector.
        if (pose == null || pose.keeperSprite == null || keeperImage == null)
        {
            RestoreKeeperBaseline();
            return;
        }

        CaptureKeeperBaseline();
        keeperImage.sprite = pose.keeperSprite;
        RectTransform r = keeperImage.rectTransform;
        r.anchoredPosition = pose.keeperPosition;
        r.sizeDelta = pose.keeperSize;
        r.localRotation = Quaternion.Euler(0f, 0f, pose.keeperRotationZ);
        r.localScale = new Vector3(pose.keeperScale.x,
            pose.keeperScale.y, baselineKeeperScale.z);

        if (keeperPuzzleIcon != null)
        {
            keeperPuzzleIcon.anchoredPosition = pose.puzzlePosition;
            keeperPuzzleIcon.sizeDelta = pose.puzzleSize;
            keeperPuzzleIcon.localRotation = Quaternion.Euler(
                0f, 0f, pose.puzzleRotationZ);
            keeperPuzzleIcon.localScale = new Vector3(pose.puzzleScale.x,
                pose.puzzleScale.y, baselinePuzzleScale.z);
        }
    }

    private void RestoreKeeperBaseline()
    {
        if (!keeperBaselineCaptured || keeperImage == null)
            return;

        keeperImage.sprite = baselineKeeperSprite;
        RectTransform r = keeperImage.rectTransform;
        r.anchoredPosition = baselineKeeperPosition;
        r.sizeDelta = baselineKeeperSize;
        r.localRotation = baselineKeeperRotation;
        r.localScale = baselineKeeperScale;

        if (keeperPuzzleIcon != null)
        {
            keeperPuzzleIcon.anchoredPosition = baselinePuzzlePosition;
            keeperPuzzleIcon.sizeDelta = baselinePuzzleSize;
            keeperPuzzleIcon.localRotation = baselinePuzzleRotation;
            keeperPuzzleIcon.localScale = baselinePuzzleScale;
        }
    }

    private void ShowMessage(string message)
    {
        if (leftTutorialText == null) return;
        ShowKeeperFrame();
        leftTutorialText.text = message ?? "";
        leftTutorialText.gameObject.SetActive(true);
        leftTutorialText.ForceMeshUpdate();
    }

    private void ShowKeeperFrame()
    {
        if (leftFrameCanvasGroup != null)
            leftFrameCanvasGroup.alpha = 1f;
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

    // The old frame script stays alive for tutorial_23..25.
    // Hide its empty frame once it has released the player to the ladder.
    // Keep already-unlocked arrow hitboxes enabled on every fresh load,
    // even if another tutorial component initialized them afterward.
    private void LateUpdate()
    {
        if (leftFrame == null) return;
        int saved = TutorialProgressManager.SavedLeftTutorialStage;
        if (phase == Phase.Finished ||
            (phase == Phase.Waiting && leftFrame.IsWaitingForLadder))
        {
            HideKeeperFrame();
        }

        if (saved >= StagePracticeNone || TutorialProgressManager.ClimbCompleted)
        {
            if (phase == Phase.Waiting || phase == Phase.Finished || phase == Phase.Practice)
                SetClimbButtonsEnabled(true);
        }
    }

    private void PauseGame()
    {
        if (pausedByThis) return;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pausedByThis = true;
    }

    private void ResumeGame()
    {
        if (!pausedByThis) return;
        Time.timeScale = previousTimeScale;
        pausedByThis = false;
    }

    private void ShowPlainDimmer()
    {
        RestoreDimmerImageOnly();
        tutorialDimmer.SetActive(true);
        dimmerImage.raycastTarget = false;
    }

    private void ShowArrowSpotlights()
    {
        Rect upRect, downRect;
        if (!TryGetButtonViewportRect(upButtonGraphic, out upRect) ||
            !TryGetButtonViewportRect(downButtonGraphic, out downRect))
        {
            ShowPlainDimmer();
            return;
        }
        RenderSpotlight(upRect, downRect, true);
    }

    private bool TryGetButtonViewportRect(RectTransform target, out Rect result)
    {
        result = new Rect();
        if (target == null || Screen.width <= 0 || Screen.height <= 0)
            return false;

        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? (canvas.worldCamera != null ? canvas.worldCamera : Camera.main)
            : null;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        float xMin = float.PositiveInfinity, yMin = float.PositiveInfinity;
        float xMax = float.NegativeInfinity, yMax = float.NegativeInfinity;
        foreach (Vector3 corner in corners)
        {
            Vector2 point = RectTransformUtility.WorldToScreenPoint(uiCamera, corner);
            xMin = Mathf.Min(xMin, point.x / Screen.width);
            xMax = Mathf.Max(xMax, point.x / Screen.width);
            yMin = Mathf.Min(yMin, point.y / Screen.height);
            yMax = Mathf.Max(yMax, point.y / Screen.height);
        }
        result = Rect.MinMaxRect(xMin - windowPadding, yMin - windowPadding,
            xMax + windowPadding, yMax + windowPadding);
        return true;
    }

    private bool TryGetStickViewportRect(out Rect result)
    {
        result = new Rect();
        if (stickTarget == null || worldCamera == null) return false;
        Renderer renderer = stickTarget.GetComponentInChildren<Renderer>();
        Bounds b = renderer != null
            ? renderer.bounds
            : new Bounds(stickTarget.position, new Vector3(0.4f, 0.2f, 0.1f));
        Vector3 min = b.min, max = b.max;
        Vector3[] points = {
            new Vector3(min.x, min.y, b.center.z),
            new Vector3(min.x, max.y, b.center.z),
            new Vector3(max.x, min.y, b.center.z),
            new Vector3(max.x, max.y, b.center.z)
        };
        float xMin = float.PositiveInfinity, xMax = float.NegativeInfinity;
        float yMin = float.PositiveInfinity, yMax = float.NegativeInfinity;
        foreach (Vector3 p in points)
        {
            Vector3 v = worldCamera.WorldToViewportPoint(p);
            if (v.z <= 0f) return false;
            xMin = Mathf.Min(xMin, v.x);
            xMax = Mathf.Max(xMax, v.x);
            yMin = Mathf.Min(yMin, v.y);
            yMax = Mathf.Max(yMax, v.y);
        }
        float w = Mathf.Max(minimumStickWindowWidth, xMax - xMin + 2f * stickPadding);
        float h = Mathf.Max(minimumStickWindowHeight, yMax - yMin + 2f * stickPadding);
        result = new Rect((xMin + xMax - w) * .5f, (yMin + yMax - h) * .5f, w, h);
        return true;
    }

    private void EnsureTexture()
    {
        int resolution = Mathf.Clamp(textureResolution, 64, 512);
        if (maskTexture != null && maskTexture.width == resolution) return;
        if (maskSprite != null) Destroy(maskSprite);
        if (maskTexture != null) Destroy(maskTexture);
        maskTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        maskTexture.wrapMode = TextureWrapMode.Clamp;
        maskTexture.filterMode = FilterMode.Bilinear;
        maskPixels = new Color32[resolution * resolution];
        maskSprite = Sprite.Create(maskTexture, new Rect(0, 0, resolution, resolution),
            new Vector2(.5f, .5f), 100f);
        maskSprite.name = "TutorialClimbSpotlights_Runtime";
    }

    private void RenderSpotlight(Rect first, Rect second, bool two)
    {
        if (tutorialDimmer == null || dimmerImage == null) return;
        EnsureTexture();
        int resolution = maskTexture.width;

        // The source UI graphics are square in screen pixels.
        // Convert normalized X into Y units so the holes render
        // as real round circles, not horizontal ovals.
        float aspect = Screen.height > 0
            ? (float)Screen.width / Screen.height : 1f;
        Vector2 circleOne = new Vector2(first.center.x * aspect, first.center.y);
        Vector2 circleTwo = new Vector2(second.center.x * aspect, second.center.y);
        float radiusOne = .5f * Mathf.Max(first.width * aspect, first.height);
        float radiusTwo = .5f * Mathf.Max(second.width * aspect, second.height);
        if (two)
        {
            // User-adjustable radius; no button RectTransform is modified.
            radiusOne *= arrowSpotlightScale;
            radiusTwo *= arrowSpotlightScale;
        }

        if (two)
        {
            // Prevent the two adjacent spotlights from becoming
            // one connected transparent stripe.
            float distance = Vector2.Distance(circleOne, circleTwo);
            float available = Mathf.Max(.002f, distance - 2.5f * windowSoftness);
            float totalRadii = radiusOne + radiusTwo;
            if (totalRadii > available && totalRadii > .0001f)
            {
                float scale = available / totalRadii;
                radiusOne *= scale;
                radiusTwo *= scale;
            }
        }

        for (int y = 0; y < resolution; y++)
        {
            float ny = (float)y / (resolution - 1);
            for (int x = 0; x < resolution; x++)
            {
                float nx = (float)x / (resolution - 1);
                float distance;
                if (two)
                {
                    Vector2 point = new Vector2(nx * aspect, ny);
                    float d1 = Vector2.Distance(point, circleOne) - radiusOne;
                    float d2 = Vector2.Distance(point, circleTwo) - radiusTwo;
                    distance = Mathf.Min(d1, d2);
                }
                else
                {
                    // Keep the original rectangular spotlight for the stick.
                    distance = SignedRectDistance(nx, ny, first);
                }
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(distance / windowSoftness));
                maskPixels[y * resolution + x] =
                    new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
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

    private static float SignedRectDistance(float x, float y, Rect rect)
    {
        float dx = Mathf.Abs(x - rect.center.x) - rect.width * .5f;
        float dy = Mathf.Abs(y - rect.center.y) - rect.height * .5f;
        return Mathf.Max(dx, dy);
    }

    private void RestoreDimmerImageOnly()
    {
        if (dimmerImage == null) return;
        dimmerImage.sprite = originalDimmerSprite;
        dimmerImage.type = originalDimmerType;
        dimmerImage.preserveAspect = originalPreserveAspect;
        dimmerImage.raycastTarget = originalRaycastTarget;
    }

    private void RestoreDimmer()
    {
        if (dimmerImage == null) return;
        RestoreDimmerImageOnly();
        if (tutorialDimmer != null) tutorialDimmer.SetActive(false);
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (isReady) BeginLocalizationLoad();
    }

    private void BeginLocalizationLoad()
    {
        localeVersion++;
        if (localeRoutine != null) StopCoroutine(localeRoutine);
        localeRoutine = StartCoroutine(LoadLocalizedText(localeVersion));
    }

    private IEnumerator LoadLocalizedText(int version)
    {
        string[] keys = {
            arrowsQuestionKey, climbPracticeKey,
            stickQuestionKey, stickApproachKey, jumpHintKey
        };
        string[] texts = {
            arrowsQuestionFallback, climbPracticeFallback,
            stickQuestionFallback, stickApproachFallback, jumpHintFallback
        };

        for (int i = 0; i < keys.Length; i++)
        {
            var request = LocalizationSettings.StringDatabase
                .GetLocalizedStringAsync(localizationTable, keys[i]);
            yield return request;
            if (version != localeVersion) yield break;
            if (request.Status == AsyncOperationStatus.Succeeded &&
                !string.IsNullOrWhiteSpace(request.Result))
                texts[i] = request.Result;
        }

        arrowsQuestionText = texts[0];
        climbPracticeText = texts[1];
        stickQuestionText = texts[2];
        stickApproachText = texts[3];
        jumpHintText = texts[4];

        if (phase == Phase.ArrowsQuestion) ShowMessage(arrowsQuestionText);
        else if (phase == Phase.Practice) ShowMessage(climbPracticeText);
        else if (phase == Phase.StickQuestion) ShowMessage(stickQuestionText);
        else if (phase == Phase.StickApproach) ShowMessage(stickApproachText);
        else if (phase == Phase.JumpHint) ShowMessage(jumpHintText);
        localeRoutine = null;
    }

#if UNITY_EDITOR
    // Test ONLY the new climb lesson again from tutorial_26.
    // The door, name, main lessons, and left Keeper 23..25 remain saved.
    [UnityEditor.MenuItem("Tools/Realm of the Lost Puzzle/Reset Climb Lesson For Testing")]
    private static void ResetClimbLessonForTesting()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode before resetting the climb lesson.");
            return;
        }

        bool confirmed = UnityEditor.EditorUtility.DisplayDialog(
            "Reset Climb Lesson",
            "Return the left Keeper to the 'walk to the ladder' stage " +
            "so the next real grab starts at tutorial_26?\n\n" +
            "The broken door and earlier lessons will be preserved.",
            "Reset Climb Lesson",
            "Cancel");

        if (!confirmed) return;

        TutorialProgressManager.SaveLeftTutorialStage(4);
        // ClimbCompleted must also be cleared for a genuine new test.
        // These keys are the unchanged V1 keys in TutorialProgressManager.
        PlayerPrefs.DeleteKey("ROLP_Tutorial_V1_ClimbCompleted");
        PlayerPrefs.DeleteKey("ROLP_Tutorial_V1_LadderReached");
        PlayerPrefs.Save();
        Debug.Log("Climb lesson reset for testing. Next grab starts at tutorial_26.");
    }

    // Re-test the stick explanation WITHOUT repeating the UP/DOWN lesson.
    [UnityEditor.MenuItem("Tools/Realm of the Lost Puzzle/Reset Stick Lesson For Testing")]
    private static void ResetStickLessonForTesting()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode before resetting the stick lesson.");
            return;
        }

        bool confirmed = UnityEditor.EditorUtility.DisplayDialog(
            "Reset Stick Lesson",
            "Replay tutorial_28, tutorial_29 and tutorial_30 after the next ladder grab?\n\n" +
            "Already learned UP/DOWN controls and all earlier progress stay saved.",
            "Reset Stick Lesson",
            "Cancel");

        if (!confirmed) return;
        TutorialProgressManager.CompleteClimb();
        TutorialProgressManager.SaveLeftTutorialStage(StageStickQuestion);
        Debug.Log("Stick lesson ready to test from tutorial_28.");
    }
#endif

    private void OnValidate()
    {
        // Unity serializes Inspector text: modifying the initializer alone
        // would leave the previously saved long sentence unchanged.
        if (jumpHintFallback ==
            "Чтобы запрыгнуть вправо, нажми прыжок и стрелку вправо.")
        {
            jumpHintFallback = "Чтобы запрыгнуть, нажми Прыжок + →";
        }

        requiredTravelEachDirection = Mathf.Max(.02f, requiredTravelEachDirection);
        minimumStickWindowWidth = Mathf.Max(.05f, minimumStickWindowWidth);
        minimumStickWindowHeight = Mathf.Max(.05f, minimumStickWindowHeight);
        textureResolution = Mathf.Clamp(textureResolution, 64, 512);
        windowSoftness = Mathf.Max(.001f, windowSoftness);
        arrowSpotlightScale = Mathf.Clamp(arrowSpotlightScale, .35f, 1.25f);
    }
}
