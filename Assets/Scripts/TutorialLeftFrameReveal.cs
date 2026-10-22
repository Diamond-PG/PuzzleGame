
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class TutorialLeftFrameReveal : MonoBehaviour
{
    // ============================================================
    // 01 - REFERENCES
    // ============================================================

    [Header("REFERENCES")]

    [SerializeField]
    private PrisonDarknessMask prisonDarknessMask;

    [SerializeField]
    private TMP_Text tutorialTextLeft;

    [SerializeField]
    private GameObject tutorialDimmer;

    [SerializeField]
    private Button tutorialNextButtonLeft;

    [SerializeField]
    private TutorialArrowPulse nextArrowPulse;

    // The same PlayerController that the door locks on its final hit.
    // Optional reference: resolved automatically if not assigned.
    [Header("DOOR - HORIZONTAL MOVEMENT UNLOCK")]
    [SerializeField]
    private PlayerController playerController;

    // ============================================================
    // KEEPER - TWO INDEPENDENT POSES
    // ============================================================

    [Header("KEEPER - UI REFERENCES")]

    [Tooltip("Image component on PuzzleKeeper_Left. Auto-found if empty.")]
    [SerializeField]
    private Image keeperImage;

    [Tooltip("RectTransform of KeeperPuzzleIcon. Auto-found if empty.")]
    [SerializeField]
    private RectTransform keeperPuzzleIconRect;

    [System.Serializable]
    private class KeeperPoseSettings
    {
        [Tooltip("Keeper sprite for this pose. For the happy pose, None uses the original sprite.")]
        public Sprite keeperSprite;

        [Header("Keeper position, size, rotation and scale")]
        public Vector2 keeperPosition = new Vector2(134.8f, -221f);
        public Vector2 keeperSize = new Vector2(230f, 230f);
        public float keeperRotationZ = 0f;
        public Vector2 keeperScale = Vector2.one;

        [Header("Blue puzzle position, size, rotation and scale")]
        public Vector2 puzzlePosition = new Vector2(6.9f, -63.8f);
        public Vector2 puzzleSize = new Vector2(27f, 27f);
        public float puzzleRotationZ = 20f;
        public Vector2 puzzleScale = Vector2.one;
    }

    [Header("KEEPER - HAPPY POSE (TUTORIAL 23)")]

    [Tooltip(
        "ON (recommended): preserve the exact appearance already set " +
        "on PuzzleKeeper_Left and KeeperPuzzleIcon in the scene. " +
        "OFF: use the Happy Pose values below."
    )]
    [SerializeField]
    private bool preserveOriginalHappyPose = true;

    [SerializeField]
    private KeeperPoseSettings happyPose = new KeeperPoseSettings();

    [Header("KEEPER - POINTING POSE (TUTORIAL 24 AND 25)")]

    [Tooltip(
        "Choose the existing Guardian sprite pointing slightly upward. " +
        "Keeper and blue puzzle each have their own pose settings."
    )]
    [SerializeField]
    private KeeperPoseSettings pointingPose = new KeeperPoseSettings();

    // ============================================================
    // 02 - LEFT FRAME APPEARANCE
    // ============================================================

    [Header("LEFT FRAME APPEARANCE")]

    [SerializeField, Min(0f)]
    private float appearanceDelay = 0.2f;

    [SerializeField, Min(0.01f)]
    private float appearanceDuration = 0.8f;

    [SerializeField]
    private bool useUnscaledTime = true;

    // ============================================================
    // 03 - WAIT FOR CAMERA
    // ============================================================

    [Header("CAMERA - WAIT BEFORE GREETING")]

    [Tooltip(
        "The hero. If empty, searches for the Player object."
    )]
    [SerializeField]
    private Transform playerForCameraCheck;

    [Tooltip(
        "Minimum time after the full level is revealed."
    )]
    [SerializeField, Min(0f)]
    private float minimumCameraWait = 0.5f;

    [Tooltip(
        "How long the camera should remain nearly still."
    )]
    [SerializeField, Min(0.01f)]
    private float cameraStableDuration = 0.35f;

    [Tooltip(
        "Maximum normal waiting time, provided the hero is visible."
    )]
    [SerializeField, Min(0.1f)]
    private float maximumCameraWait = 4f;

    [Tooltip(
        "Maximum camera movement speed considered stable."
    )]
    [SerializeField, Min(0.001f)]
    private float cameraSpeedThreshold = 0.05f;

    [Tooltip(
        "Do not start the greeting until the hero is on screen."
    )]
    [SerializeField]
    private bool requirePlayerVisible = true;

    [SerializeField, Range(0f, 0.2f)]
    private float playerScreenPadding = 0.02f;

    // ============================================================
    // 04 - TUTORIAL GAME PAUSE
    // ============================================================

    [Header("TUTORIAL GAME PAUSE")]

    [SerializeField]
    private bool pauseGameDuringGreeting = true;

    [SerializeField]
    private bool dimScreenDuringGreeting = true;

    // ============================================================
    // 05 - LADDER HIGHLIGHT
    // ============================================================

    [Header("LADDER HIGHLIGHT")]

    [SerializeField]
    private Transform ladderTarget;

    [SerializeField]
    private Camera worldCamera;

    [SerializeField]
    private bool highlightLadder = true;

    [Header("LADDER WINDOW POSITION")]

    [SerializeField]
    private Vector2 ladderWindowOffset = Vector2.zero;

    [Header("LADDER WINDOW PADDING")]

    [SerializeField, Range(0f, 0.15f)]
    private float ladderPaddingX = 0.012f;

    [SerializeField, Range(0f, 0.15f)]
    private float ladderPaddingY = 0.015f;

    [Header("LADDER WINDOW MINIMUM SIZE")]

    [SerializeField, Range(0.01f, 0.5f)]
    private float ladderMinimumWidth = 0.06f;

    [SerializeField, Range(0.01f, 0.8f)]
    private float ladderMinimumHeight = 0.16f;

    [Header("LADDER WINDOW SOFTNESS")]

    [SerializeField, Range(0.001f, 0.1f)]
    private float ladderEdgeSoftness = 0.015f;

    [Header("LADDER WINDOW FALLBACK WORLD SIZE")]

    [SerializeField]
    private Vector2 fallbackLadderWorldSize =
        new Vector2(0.7f, 1.5f);

    [Header("LADDER HIGHLIGHT PERFORMANCE")]

    [SerializeField, Range(64, 512)]
    private int spotlightResolution = 256;

    [SerializeField, Range(0.05f, 0.5f)]
    private float spotlightCheckInterval = 0.15f;

    // ============================================================
    // 06 - LOCALIZATION
    // ============================================================

    [Header("LEFT FRAME LOCALIZATION")]

    [SerializeField]
    private string localizationTableName = "Tutorial";

    [Header("TUTORIAL 23 - GREETING")]

    [SerializeField]
    private string greetingKey = "tutorial_23";

    [SerializeField, TextArea(2, 4)]
    private string greetingFallback =
        "Отлично! Ты справился! Путь свободен!";

    [Header("TUTORIAL 24 - LADDER QUESTION")]

    [SerializeField]
    private string approachLadderKey = "tutorial_24";

    [SerializeField, TextArea(2, 4)]
    private string approachLadderFallback =
        "Видишь скобную лестницу?";

    [Header("TUTORIAL 25 - LADDER INSTRUCTION")]

    [SerializeField]
    private string grabLadderKey = "tutorial_25";

    [SerializeField, TextArea(2, 4)]
    private string grabLadderFallback =
        "Подойди к ней и ухватись за скобы.";

    // ============================================================
    // 06A - AUTOSAVE - LEFT KEEPER
    // ============================================================

    [Header("AUTOSAVE - LEFT KEEPER")]

    [Tooltip(
        "Save and restore the exact left Keeper stage: " +
        "tutorial_23, tutorial_24, tutorial_25 or walk to ladder. " +
        "Does not change the configured keeper poses or ladder spotlight."
    )]
    [SerializeField]
    private bool enableExactLeftStageAutosave = true;

    // ============================================================
    // 07 - PRIVATE STATE
    // ============================================================

    private enum TutorialStage
    {
        WaitingForReveal,
        Greeting,
        LadderQuestion,
        LadderInstruction,
        WalkToLadder
    }

    private TutorialStage currentStage =
        TutorialStage.WaitingForReveal;

    // Read only when the scene starts. Do not alter the saved stage
    // while waiting for the camera or localization to become ready.
    private TutorialStage stageToRestoreAfterReveal =
        TutorialStage.Greeting;

    private CanvasGroup frameCanvasGroup;

    private Image dimmerImage;
    private Sprite originalDimmerSprite;
    private Image.Type originalDimmerType;
    private bool originalPreserveAspect;

    private Texture2D spotlightTexture;
    private Sprite spotlightSprite;
    private Color32[] spotlightPixels;

    private Rect lastLadderWindow;
    private bool hasLastLadderWindow;

    private float nextSpotlightCheckTime;

    private bool started;
    private bool localizationReady;

    private bool appearanceStarted;
    private bool appearanceFinished;

    private bool gamePausedByTutorial;
    private float previousTimeScale = 1f;

    private Coroutine appearanceRoutine;
    private Coroutine localizationRoutine;

    private int localizationVersion;

    private string greetingText;
    private string ladderQuestionText;
    private string ladderInstructionText;

    private bool warnedMissingLadder;
    private bool warnedCameraTimeout;
    private bool warnedMissingKeeperPointingSprite;

    // Capture actual scene values, so the existing joyful pose
    // remains pixel-identical by default.
    private Sprite originalKeeperSprite;
    private Vector2 originalKeeperPosition;
    private Vector2 originalKeeperSize;
    private Quaternion originalKeeperRotation;
    private Vector3 originalKeeperScale;

    private Vector2 originalPuzzlePosition;
    private Vector2 originalPuzzleSize;
    private Quaternion originalPuzzleRotation;
    private Vector3 originalPuzzleScale;
    private bool originalKeeperPoseCaptured;

    // ============================================================
    // 08 - PUBLIC STATE
    // ============================================================

    public bool IsAppearanceStarted =>
        appearanceStarted;

    public bool IsAppearanceFinished =>
        appearanceFinished;

    public bool IsWaitingForLadder =>
        currentStage == TutorialStage.WalkToLadder;

    // ============================================================
    // 09 - AWAKE
    // ============================================================

    private void Awake()
    {
        frameCanvasGroup =
            GetComponent<CanvasGroup>();

        FindReferences();

        CaptureOriginalKeeperPose();

        frameCanvasGroup.alpha = 0f;
        frameCanvasGroup.interactable = false;
        frameCanvasGroup.blocksRaycasts = false;

        if (tutorialDimmer != null)
        {
            dimmerImage =
                tutorialDimmer.GetComponent<Image>();

            if (dimmerImage != null)
            {
                originalDimmerSprite =
                    dimmerImage.sprite;

                originalDimmerType =
                    dimmerImage.type;

                originalPreserveAspect =
                    dimmerImage.preserveAspect;
            }

            tutorialDimmer.SetActive(false);
        }

        if (tutorialNextButtonLeft != null)
        {
            tutorialNextButtonLeft.gameObject
                .SetActive(false);
        }
    }

    // ============================================================
    // 10 - FIND REFERENCES
    // ============================================================

    private void FindReferences()
    {
        if (tutorialTextLeft == null)
        {
            Transform found =
                transform.Find("TutorialText_Left");

            if (found != null)
            {
                tutorialTextLeft =
                    found.GetComponent<TMP_Text>();
            }
        }

        if (tutorialNextButtonLeft == null)
        {
            Transform found =
                transform.Find(
                    "TutorialNextButton_Left"
                );

            if (found != null)
            {
                tutorialNextButtonLeft =
                    found.GetComponent<Button>();
            }
        }

        if (tutorialNextButtonLeft != null &&
            nextArrowPulse == null)
        {
            nextArrowPulse =
                tutorialNextButtonLeft
                    .GetComponent<TutorialArrowPulse>();
        }

        if (tutorialDimmer == null &&
            transform.parent != null)
        {
            Transform found =
                transform.parent.Find(
                    "TutorialDimmer"
                );

            if (found != null)
            {
                tutorialDimmer = found.gameObject;
            }
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (playerForCameraCheck == null)
        {
            GameObject player =
                GameObject.Find("Player");

            if (player != null)
            {
                playerForCameraCheck =
                    player.transform;
            }
        }

        // Resolve the player's movement component without
        // requiring any new Inspector assignment.
        if (playerController == null && playerForCameraCheck != null)
        {
            playerController =
                playerForCameraCheck.GetComponent<PlayerController>();

            if (playerController == null)
            {
                playerController =
                    playerForCameraCheck.GetComponentInParent<PlayerController>();
            }
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        // Keeper sits directly inside TutorialFrame_Left.
        // The blue puzzle is a child of the keeper.
        if (keeperImage == null)
        {
            Transform keeperObject = transform.Find("PuzzleKeeper_Left");

            if (keeperObject != null)
                keeperImage = keeperObject.GetComponent<Image>();
        }

        if (keeperPuzzleIconRect == null && keeperImage != null)
        {
            Transform puzzleObject =
                keeperImage.transform.Find("KeeperPuzzleIcon");

            if (puzzleObject != null)
                keeperPuzzleIconRect =
                    puzzleObject.GetComponent<RectTransform>();
        }
    }

    // ============================================================
    // 11 - ON ENABLE
    // ============================================================

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged +=
            OnSelectedLocaleChanged;

        FindReferences();

        if (tutorialNextButtonLeft != null)
        {
            tutorialNextButtonLeft.onClick
                .RemoveListener(OnNextButtonClicked);

            tutorialNextButtonLeft.onClick
                .AddListener(OnNextButtonClicked);
        }
    }

    // ============================================================
    // 12 - ON DISABLE
    // ============================================================

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -=
            OnSelectedLocaleChanged;

        if (tutorialNextButtonLeft != null)
        {
            tutorialNextButtonLeft.onClick
                .RemoveListener(OnNextButtonClicked);
        }

        localizationVersion++;

        if (localizationRoutine != null)
        {
            StopCoroutine(localizationRoutine);
            localizationRoutine = null;
        }

        if (appearanceRoutine != null)
        {
            StopCoroutine(appearanceRoutine);
            appearanceRoutine = null;
        }

        if (nextArrowPulse != null)
        {
            nextArrowPulse.SetPulseActive(false);
        }

        ResumeGame();
        HideDimmer();
    }

    // ============================================================
    // 13 - START
    // ============================================================

    private void Start()
    {
        if (prisonDarknessMask == null)
        {
            prisonDarknessMask =
                FindFirstObjectByType<PrisonDarknessMask>();
        }

        FindReferences();

        if (prisonDarknessMask == null ||
            tutorialTextLeft == null ||
            tutorialNextButtonLeft == null)
        {
            Debug.LogError(
                "TutorialLeftFrameReveal: " +
                "A required reference is missing.",
                this
            );

            enabled = false;
            return;
        }

        tutorialTextLeft.text = "";
        tutorialTextLeft.gameObject.SetActive(true);

        currentStage =
            TutorialStage.WaitingForReveal;

        stageToRestoreAfterReveal = GetSavedLeftStage();

        started = true;

        BeginLocalizationLoad();
    }

    // ============================================================
    // 14 - UPDATE
    // ============================================================

    private void Update()
    {
        // The level reveal has completed,
        // but do NOT pause the game immediately.

        if (!appearanceStarted &&
            localizationReady &&
            prisonDarknessMask != null &&
            prisonDarknessMask.IsFullLevelRevealComplete)
        {
            appearanceStarted = true;

            appearanceRoutine = StartCoroutine(
                WaitForCameraThenRevealRoutine()
            );
        }

        if (currentStage ==
                TutorialStage.LadderQuestion ||
            currentStage ==
                TutorialStage.LadderInstruction)
        {
            UpdateLadderSpotlight();
        }
    }

    // ============================================================
    // 15 - WAIT FOR CAMERA TO REACH PLAYER
    // ============================================================

    private IEnumerator WaitForCameraThenRevealRoutine()
    {
        FindReferences();

        float elapsed = 0f;
        float stableElapsed = 0f;

        Vector3 previousCameraPosition =
            worldCamera != null
                ? worldCamera.transform.position
                : Vector3.zero;

        // Wait until camera movement has settled.
        // Uses REAL time, so this does not depend
        // on the game's Time.timeScale.

        while (true)
        {
            yield return new WaitForEndOfFrame();

            float delta =
                Mathf.Max(
                    0.0001f,
                    Time.unscaledDeltaTime
                );

            elapsed += delta;

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (worldCamera == null)
            {
                if (elapsed >= maximumCameraWait)
                    break;

                continue;
            }

            Vector3 currentCameraPosition =
                worldCamera.transform.position;

            Vector2 movement = new Vector2(
                currentCameraPosition.x -
                    previousCameraPosition.x,

                currentCameraPosition.y -
                    previousCameraPosition.y
            );

            float cameraSpeed =
                movement.magnitude / delta;

            previousCameraPosition =
                currentCameraPosition;

            if (cameraSpeed <= cameraSpeedThreshold)
            {
                stableElapsed += delta;
            }
            else
            {
                stableElapsed = 0f;
            }

            bool playerVisible =
                IsPlayerVisibleInCamera();

            bool minimumTimePassed =
                elapsed >= minimumCameraWait;

            bool cameraIsStable =
                stableElapsed >= cameraStableDuration;

            bool normalReady =
                minimumTimePassed &&
                cameraIsStable &&
                playerVisible;

            // A camera with a permanent tiny movement
            // should not block the tutorial forever,
            // provided the player is already visible.

            bool timeoutReady =
                elapsed >= maximumCameraWait &&
                playerVisible;

            if (normalReady || timeoutReady)
            {
                break;
            }

            // If the player is still off screen,
            // do not freeze the camera at the
            // wrong location.

            if (elapsed >= maximumCameraWait &&
                !playerVisible &&
                !warnedCameraTimeout)
            {
                Debug.LogWarning(
                    "TutorialLeftFrameReveal: " +
                    "Waiting for the camera to show " +
                    "the player before starting " +
                    "the greeting.",
                    this
                );

                warnedCameraTimeout = true;
            }
        }

        // Keep the existing Inspector appearance
        // delay, but apply it BEFORE pausing.

        if (appearanceDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                appearanceDelay
            );
        }

        // ========================================================
        // CAMERA IS READY
        // START GREETING / DIMMING / PAUSE TOGETHER
        // ========================================================

        currentStage = stageToRestoreAfterReveal;

        ApplyKeeperPose(currentStage != TutorialStage.Greeting);
        ShowCurrentStageText();

        // On reload after tutorial_25 the hero is already free to
        // approach the ladder. Keep the Keeper visible, without
        // reintroducing a pause, darkness or navigation arrow.
        if (currentStage == TutorialStage.WalkToLadder)
        {
            HideDimmer();
            ResumeGame();
            UnlockPlayerHorizontalMovement();
            SetArrowPulse(false);

            tutorialNextButtonLeft.gameObject.SetActive(false);
            frameCanvasGroup.alpha = 1f;
            frameCanvasGroup.interactable = false;
            frameCanvasGroup.blocksRaycasts = false;

            appearanceFinished = true;
            appearanceRoutine = null;
            SaveCurrentLeftStage();
            yield break;
        }

        // Restored tutorial_24 and tutorial_25 show the same exact
        // ladder spotlight and pointing pose as ordinary navigation.
        if (currentStage == TutorialStage.LadderQuestion ||
            currentStage == TutorialStage.LadderInstruction)
        {
            ShowLadderDimmer();
        }
        else if (dimScreenDuringGreeting)
        {
            ShowNormalDimmer();
        }

        if (pauseGameDuringGreeting)
        {
            PauseGame();
        }

        SaveCurrentLeftStage();

        // Fade in the left frame.
        // Time.unscaledDeltaTime keeps this animation
        // working while the level is paused.

        frameCanvasGroup.alpha = 0f;

        frameCanvasGroup.interactable = true;
        frameCanvasGroup.blocksRaycasts = true;

        tutorialNextButtonLeft.gameObject
            .SetActive(true);

        tutorialNextButtonLeft.interactable = true;

        SetArrowPulse(true);

        float elapsedFade = 0f;

        float duration =
            Mathf.Max(
                0.01f,
                appearanceDuration
            );

        while (elapsedFade < duration)
        {
            float deltaTime =
                (useUnscaledTime || gamePausedByTutorial)
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

            elapsedFade += deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsedFade / duration
                );

            frameCanvasGroup.alpha =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            yield return null;
        }

        frameCanvasGroup.alpha = 1f;

        appearanceFinished = true;
        appearanceRoutine = null;
    }

    // ============================================================
    // 16 - CHECK PLAYER VISIBILITY
    // ============================================================

    private bool IsPlayerVisibleInCamera()
    {
        if (!requirePlayerVisible)
            return true;

        if (playerForCameraCheck == null)
        {
            GameObject player =
                GameObject.Find("Player");

            if (player != null)
            {
                playerForCameraCheck =
                    player.transform;
            }
        }

        // No player reference:
        // do not permanently block the tutorial.

        if (playerForCameraCheck == null)
            return true;

        if (worldCamera == null)
            return false;

        Vector3 viewport =
            worldCamera.WorldToViewportPoint(
                playerForCameraCheck.position
            );

        float padding =
            Mathf.Clamp(
                playerScreenPadding,
                0f,
                0.2f
            );

        return
            viewport.z > 0f &&
            viewport.x >= padding &&
            viewport.x <= 1f - padding &&
            viewport.y >= padding &&
            viewport.y <= 1f - padding;
    }

    // ============================================================
    // 17 - PAUSE GAME
    // ============================================================

    private void PauseGame()
    {
        if (gamePausedByTutorial)
            return;

        previousTimeScale = Time.timeScale;

        Time.timeScale = 0f;

        gamePausedByTutorial = true;
    }

    // ============================================================
    // 18 - RESUME GAME
    // ============================================================

    private void ResumeGame()
    {
        if (!gamePausedByTutorial)
            return;

        Time.timeScale = previousTimeScale;

        gamePausedByTutorial = false;
    }

    // ============================================================
    // AUTOSAVE - EXACT LEFT KEEPER STAGE
    // ============================================================

    private TutorialStage GetSavedLeftStage()
    {
        // The left Keeper belongs to the story AFTER the door breaks.
        // On a fresh playthrough always begin with tutorial_23.
        if (!enableExactLeftStageAutosave ||
            !TutorialProgressManager.DoorBroken ||
            !TutorialProgressManager.HasSavedLeftTutorialStage)
        {
            return TutorialStage.Greeting;
        }

        int saved = TutorialProgressManager.SavedLeftTutorialStage;

        if (saved == (int)TutorialStage.Greeting ||
            saved == (int)TutorialStage.LadderQuestion ||
            saved == (int)TutorialStage.LadderInstruction)
        {
            return (TutorialStage)saved;
        }

        // Also tolerate stages added by a future ladder tutorial:
        // this older controller must not force the story backward.
        if (saved >= (int)TutorialStage.WalkToLadder)
        {
            return TutorialStage.WalkToLadder;
        }

        return TutorialStage.Greeting;
    }

    private void SaveCurrentLeftStage()
    {
        if (!enableExactLeftStageAutosave ||
            !TutorialProgressManager.DoorBroken ||
            currentStage == TutorialStage.WaitingForReveal)
        {
            return;
        }

        // A future controller can own higher stage numbers. Do not
        // overwrite a newer checkpoint with this controller's stage 4.
        if (TutorialProgressManager.HasSavedLeftTutorialStage &&
            TutorialProgressManager.SavedLeftTutorialStage >
                (int)TutorialStage.WalkToLadder)
        {
            return;
        }

        TutorialProgressManager.SaveLeftTutorialStage(
            (int)currentStage
        );
    }

    // ============================================================
    // 19 - NEXT BUTTON
    // ============================================================

    private void OnNextButtonClicked()
    {
        if (!appearanceFinished ||
            !localizationReady)
        {
            return;
        }

        switch (currentStage)
        {
            case TutorialStage.Greeting:

                // tutorial_23 -> tutorial_24

                currentStage =
                    TutorialStage.LadderQuestion;

                ApplyKeeperPose(true);
                ShowCurrentStageText();

                ShowLadderDimmer();

                SetArrowPulse(true);

                SaveCurrentLeftStage();

                break;

            case TutorialStage.LadderQuestion:

                // tutorial_24 -> tutorial_25

                currentStage =
                    TutorialStage.LadderInstruction;

                ApplyKeeperPose(true);
                ShowCurrentStageText();

                ShowLadderDimmer();

                SetArrowPulse(true);

                SaveCurrentLeftStage();

                break;

            case TutorialStage.LadderInstruction:

                // Player can now approach the ladder.

                currentStage =
                    TutorialStage.WalkToLadder;

                SetArrowPulse(false);

                tutorialNextButtonLeft.gameObject
                    .SetActive(false);

                tutorialTextLeft.text = "";

                HideDimmer();

                ResumeGame();

                // The door disabled ONLY left/right movement.
                // Return that control now that tutorial_25 is done.
                UnlockPlayerHorizontalMovement();

                frameCanvasGroup.interactable =
                    false;

                frameCanvasGroup.blocksRaycasts =
                    false;

                SaveCurrentLeftStage();

                break;
        }
    }

    // ============================================================
    // UNLOCK HORIZONTAL MOVEMENT AFTER TUTORIAL 25
    // ============================================================

    private void UnlockPlayerHorizontalMovement()
    {
        if (playerController == null)
            FindReferences();

        if (playerController != null)
        {
            playerController.SetHorizontalMovementLock(false);
        }
        else
        {
            Debug.LogWarning(
                "TutorialLeftFrameReveal: PlayerController not found. " +
                "Cannot unlock horizontal movement after tutorial_25.",
                this
            );
        }
    }

    // ============================================================
    // KEEPER - CAPTURE ORIGINAL HAPPY APPEARANCE
    // ============================================================

    private void CaptureOriginalKeeperPose()
    {
        if (keeperImage == null)
            return;

        RectTransform keeperRect = keeperImage.rectTransform;

        if (keeperRect == null)
            return;

        originalKeeperSprite = keeperImage.sprite;
        originalKeeperPosition = keeperRect.anchoredPosition;
        originalKeeperSize = keeperRect.sizeDelta;
        originalKeeperRotation = keeperRect.localRotation;
        originalKeeperScale = keeperRect.localScale;

        if (keeperPuzzleIconRect != null)
        {
            originalPuzzlePosition = keeperPuzzleIconRect.anchoredPosition;
            originalPuzzleSize = keeperPuzzleIconRect.sizeDelta;
            originalPuzzleRotation = keeperPuzzleIconRect.localRotation;
            originalPuzzleScale = keeperPuzzleIconRect.localScale;
        }

        originalKeeperPoseCaptured = true;
    }

    // ============================================================
    // KEEPER - APPLY HAPPY OR POINTING POSE
    // ============================================================

    private void ApplyKeeperPose(bool showPointing)
    {
        if (keeperImage == null)
            FindReferences();

        if (keeperImage == null)
            return;

        if (showPointing &&
            (pointingPose == null || pointingPose.keeperSprite == null))
        {
            // Avoid moving the keeper or puzzle if the pointing
            // sprite has not yet been assigned in the Inspector.
            if (!warnedMissingKeeperPointingSprite)
            {
                Debug.LogWarning(
                    "TutorialLeftFrameReveal: assign the existing " +
                    "upward-pointing Guardian sprite in " +
                    "KEEPER - POINTING POSE.",
                    this
                );

                warnedMissingKeeperPointingSprite = true;
            }

            return;
        }

        if (!showPointing && preserveOriginalHappyPose &&
            originalKeeperPoseCaptured)
        {
            RestoreOriginalKeeperPose();
            return;
        }

        KeeperPoseSettings pose = showPointing ? pointingPose : happyPose;

        if (pose == null)
            return;

        RectTransform keeperRect = keeperImage.rectTransform;
        keeperImage.sprite = pose.keeperSprite != null
            ? pose.keeperSprite
            : originalKeeperSprite;

        keeperRect.anchoredPosition = pose.keeperPosition;
        keeperRect.sizeDelta = new Vector2(
            Mathf.Max(1f, pose.keeperSize.x),
            Mathf.Max(1f, pose.keeperSize.y)
        );
        keeperRect.localRotation = Quaternion.Euler(
            0f, 0f, pose.keeperRotationZ
        );
        keeperRect.localScale = new Vector3(
            pose.keeperScale.x, pose.keeperScale.y,
            originalKeeperPoseCaptured ? originalKeeperScale.z : 1f
        );

        if (keeperPuzzleIconRect != null)
        {
            keeperPuzzleIconRect.anchoredPosition = pose.puzzlePosition;
            keeperPuzzleIconRect.sizeDelta = new Vector2(
                Mathf.Max(1f, pose.puzzleSize.x),
                Mathf.Max(1f, pose.puzzleSize.y)
            );
            keeperPuzzleIconRect.localRotation = Quaternion.Euler(
                0f, 0f, pose.puzzleRotationZ
            );
            keeperPuzzleIconRect.localScale = new Vector3(
                pose.puzzleScale.x, pose.puzzleScale.y,
                originalKeeperPoseCaptured ? originalPuzzleScale.z : 1f
            );
        }
    }

    private void RestoreOriginalKeeperPose()
    {
        if (keeperImage == null || !originalKeeperPoseCaptured)
            return;

        RectTransform keeperRect = keeperImage.rectTransform;
        keeperImage.sprite = originalKeeperSprite;
        keeperRect.anchoredPosition = originalKeeperPosition;
        keeperRect.sizeDelta = originalKeeperSize;
        keeperRect.localRotation = originalKeeperRotation;
        keeperRect.localScale = originalKeeperScale;

        if (keeperPuzzleIconRect != null)
        {
            keeperPuzzleIconRect.anchoredPosition = originalPuzzlePosition;
            keeperPuzzleIconRect.sizeDelta = originalPuzzleSize;
            keeperPuzzleIconRect.localRotation = originalPuzzleRotation;
            keeperPuzzleIconRect.localScale = originalPuzzleScale;
        }
    }

    // ============================================================
    // 20 - ARROW PULSE
    // ============================================================

    private void SetArrowPulse(bool active)
    {
        if (nextArrowPulse != null)
        {
            nextArrowPulse.SetPulseActive(active);
        }
    }

    // ============================================================
    // 21 - NORMAL DIMMER
    // ============================================================

    private void ShowNormalDimmer()
    {
        if (tutorialDimmer == null)
            return;

        RestoreDimmerImage();

        tutorialDimmer.SetActive(true);
    }

    // ============================================================
    // 22 - LADDER DIMMER
    // ============================================================

    private void ShowLadderDimmer()
    {
        if (tutorialDimmer == null)
            return;

        tutorialDimmer.SetActive(true);

        hasLastLadderWindow = false;
        nextSpotlightCheckTime = 0f;

        if (!highlightLadder ||
            ladderTarget == null)
        {
            if (highlightLadder &&
                ladderTarget == null &&
                !warnedMissingLadder)
            {
                Debug.LogWarning(
                    "TutorialLeftFrameReveal: " +
                    "Ladder Target is missing.",
                    this
                );

                warnedMissingLadder = true;
            }

            ShowNormalDimmer();
            return;
        }

        UpdateLadderSpotlight(true);
    }

    // ============================================================
    // 23 - HIDE DIMMER
    // ============================================================

    private void HideDimmer()
    {
        RestoreDimmerImage();

        if (tutorialDimmer != null)
        {
            tutorialDimmer.SetActive(false);
        }
    }

    private void RestoreDimmerImage()
    {
        if (dimmerImage == null)
            return;

        dimmerImage.sprite =
            originalDimmerSprite;

        dimmerImage.type =
            originalDimmerType;

        dimmerImage.preserveAspect =
            originalPreserveAspect;
    }

    // ============================================================
    // 24 - UPDATE LADDER SPOTLIGHT
    // ============================================================

    private void UpdateLadderSpotlight(
        bool forceUpdate = false)
    {
        if (!highlightLadder ||
            ladderTarget == null ||
            tutorialDimmer == null ||
            dimmerImage == null)
        {
            return;
        }

        if (!forceUpdate &&
            Time.unscaledTime <
                nextSpotlightCheckTime)
        {
            return;
        }

        nextSpotlightCheckTime =
            Time.unscaledTime +
            spotlightCheckInterval;

        if (!TryGetLadderViewportRect(
                out Rect ladderRect))
        {
            return;
        }

        bool changed =
            !hasLastLadderWindow ||
            Mathf.Abs(
                ladderRect.x -
                lastLadderWindow.x
            ) > 0.001f ||
            Mathf.Abs(
                ladderRect.y -
                lastLadderWindow.y
            ) > 0.001f ||
            Mathf.Abs(
                ladderRect.width -
                lastLadderWindow.width
            ) > 0.001f ||
            Mathf.Abs(
                ladderRect.height -
                lastLadderWindow.height
            ) > 0.001f;

        if (!forceUpdate && !changed)
            return;

        lastLadderWindow = ladderRect;
        hasLastLadderWindow = true;

        RenderSpotlight(ladderRect);

        dimmerImage.sprite = spotlightSprite;
        dimmerImage.type = Image.Type.Simple;
        dimmerImage.preserveAspect = false;
    }

    // ============================================================
    // 25 - FIND LADDER WORLD BOUNDS
    // ============================================================

    private Bounds GetLadderWorldBounds()
    {
        Renderer[] renderers =
            ladderTarget
                .GetComponentsInChildren<Renderer>(
                    true
                );

        Bounds result = new Bounds();
        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            Bounds bounds = renderer.bounds;

            if (bounds.size.sqrMagnitude <
                0.0001f)
            {
                continue;
            }

            if (!found)
            {
                result = bounds;
                found = true;
            }
            else
            {
                result.Encapsulate(bounds);
            }
        }

        if (found)
            return result;

        Collider2D[] colliders =
            ladderTarget
                .GetComponentsInChildren<Collider2D>(
                    true
                );

        foreach (Collider2D collider in colliders)
        {
            if (collider == null)
                continue;

            Bounds bounds = collider.bounds;

            if (bounds.size.sqrMagnitude <
                0.0001f)
            {
                continue;
            }

            if (!found)
            {
                result = bounds;
                found = true;
            }
            else
            {
                result.Encapsulate(bounds);
            }
        }

        if (found)
            return result;

        return new Bounds(
            ladderTarget.position,
            new Vector3(
                Mathf.Max(
                    0.01f,
                    fallbackLadderWorldSize.x
                ),
                Mathf.Max(
                    0.01f,
                    fallbackLadderWorldSize.y
                ),
                0.1f
            )
        );
    }

    // ============================================================
    // 26 - GET LADDER VIEWPORT RECT
    // ============================================================

    private bool TryGetLadderViewportRect(
        out Rect result)
    {
        result = new Rect();

        if (ladderTarget == null)
            return false;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
            return false;

        Bounds worldBounds =
            GetLadderWorldBounds();

        Vector3 min = worldBounds.min;
        Vector3 max = worldBounds.max;

        Vector3[] points =
        {
            new Vector3(
                min.x, min.y, worldBounds.center.z
            ),
            new Vector3(
                min.x, max.y, worldBounds.center.z
            ),
            new Vector3(
                max.x, min.y, worldBounds.center.z
            ),
            new Vector3(
                max.x, max.y, worldBounds.center.z
            )
        };

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;

        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        foreach (Vector3 point in points)
        {
            Vector3 viewport =
                worldCamera.WorldToViewportPoint(
                    point
                );

            if (viewport.z <= 0f)
                return false;

            minX = Mathf.Min(minX, viewport.x);
            minY = Mathf.Min(minY, viewport.y);

            maxX = Mathf.Max(maxX, viewport.x);
            maxY = Mathf.Max(maxY, viewport.y);
        }

        float centerX =
            (minX + maxX) * 0.5f +
            ladderWindowOffset.x;

        float centerY =
            (minY + maxY) * 0.5f +
            ladderWindowOffset.y;

        float width =
            Mathf.Max(
                ladderMinimumWidth,
                maxX - minX +
                ladderPaddingX * 2f
            );

        float height =
            Mathf.Max(
                ladderMinimumHeight,
                maxY - minY +
                ladderPaddingY * 2f
            );

        result = new Rect(
            centerX - width * 0.5f,
            centerY - height * 0.5f,
            width,
            height
        );

        return true;
    }

    // ============================================================
    // 27 - CREATE SPOTLIGHT TEXTURE
    // ============================================================

    private void EnsureSpotlightTexture()
    {
        int resolution =
            Mathf.Clamp(
                spotlightResolution,
                64,
                512
            );

        if (spotlightTexture != null &&
            spotlightTexture.width == resolution)
        {
            return;
        }

        DestroySpotlightResources();

        spotlightTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        spotlightTexture.wrapMode =
            TextureWrapMode.Clamp;

        spotlightTexture.filterMode =
            FilterMode.Bilinear;

        spotlightPixels =
            new Color32[
                resolution * resolution
            ];

        spotlightSprite = Sprite.Create(
            spotlightTexture,
            new Rect(
                0f,
                0f,
                resolution,
                resolution
            ),
            new Vector2(0.5f, 0.5f),
            100f
        );

        spotlightSprite.name =
            "TutorialLadderSpotlight_Runtime";
    }

    // ============================================================
    // 28 - RENDER SPOTLIGHT
    // ============================================================

    private void RenderSpotlight(Rect opening)
    {
        EnsureSpotlightTexture();

        int width = spotlightTexture.width;
        int height = spotlightTexture.height;

        float centerX = opening.center.x;
        float centerY = opening.center.y;

        float halfWidth =
            opening.width * 0.5f;

        float halfHeight =
            opening.height * 0.5f;

        float softness =
            Mathf.Max(
                0.001f,
                ladderEdgeSoftness
            );

        for (int y = 0; y < height; y++)
        {
            float ny =
                (float)y / (height - 1);

            for (int x = 0; x < width; x++)
            {
                float nx =
                    (float)x / (width - 1);

                float dx =
                    Mathf.Abs(nx - centerX) -
                    halfWidth;

                float dy =
                    Mathf.Abs(ny - centerY) -
                    halfHeight;

                float outside =
                    Mathf.Max(dx, dy);

                float alpha =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.Clamp01(
                            outside / softness
                        )
                    );

                spotlightPixels[
                    y * width + x
                ] = new Color32(
                    255,
                    255,
                    255,
                    (byte)Mathf.RoundToInt(
                        alpha * 255f
                    )
                );
            }
        }

        spotlightTexture.SetPixels32(
            spotlightPixels
        );

        spotlightTexture.Apply(
            false,
            false
        );
    }

    // ============================================================
    // 29 - BEGIN LOCALIZATION
    // ============================================================

    private void BeginLocalizationLoad()
    {
        localizationVersion++;

        int version = localizationVersion;

        if (localizationRoutine != null)
        {
            StopCoroutine(localizationRoutine);
            localizationRoutine = null;
        }

        localizationReady = false;

        localizationRoutine =
            StartCoroutine(
                LoadAllTexts(version)
            );
    }

    // ============================================================
    // 30 - LOAD ALL THREE MESSAGES
    // ============================================================

    private IEnumerator LoadAllTexts(int version)
    {
        string[] keys =
        {
            greetingKey,
            approachLadderKey,
            grabLadderKey
        };

        string[] fallbacks =
        {
            string.IsNullOrWhiteSpace(
                greetingFallback
            )
                ? "Отлично! Ты справился! Путь свободен!"
                : greetingFallback,

            string.IsNullOrWhiteSpace(
                approachLadderFallback
            )
                ? "Видишь скобную лестницу?"
                : approachLadderFallback,

            string.IsNullOrWhiteSpace(
                grabLadderFallback
            )
                ? "Подойди к ней и ухватись за скобы."
                : grabLadderFallback
        };

        string[] results = new string[3];

        for (int i = 0; i < keys.Length; i++)
        {
            var operation =
                LocalizationSettings.StringDatabase
                    .GetLocalizedStringAsync(
                        localizationTableName,
                        keys[i]
                    );

            yield return operation;

            if (version != localizationVersion)
                yield break;

            if (operation.Status ==
                    AsyncOperationStatus.Succeeded &&
                !string.IsNullOrWhiteSpace(
                    operation.Result
                ))
            {
                results[i] = operation.Result;
            }
            else
            {
                results[i] = fallbacks[i];

                Debug.LogWarning(
                    "TutorialLeftFrameReveal: " +
                    "Localization missing: " +
                    keys[i],
                    this
                );
            }
        }

        greetingText = results[0];
        ladderQuestionText = results[1];
        ladderInstructionText = results[2];

        localizationReady = true;
        localizationRoutine = null;

        ShowCurrentStageText();
    }

    // ============================================================
    // 31 - SHOW CURRENT STAGE TEXT
    // ============================================================

    private void ShowCurrentStageText()
    {
        if (tutorialTextLeft == null)
            return;

        string textToShow = "";

        switch (currentStage)
        {
            case TutorialStage.WaitingForReveal:
            case TutorialStage.Greeting:

                textToShow = greetingText;
                break;

            case TutorialStage.LadderQuestion:

                textToShow = ladderQuestionText;
                break;

            case TutorialStage.LadderInstruction:

                textToShow = ladderInstructionText;
                break;

            case TutorialStage.WalkToLadder:

                textToShow = "";
                break;
        }

        tutorialTextLeft.text =
            textToShow ?? "";

        tutorialTextLeft.gameObject
            .SetActive(true);

        tutorialTextLeft.ForceMeshUpdate();
    }

    // ============================================================
    // 32 - LANGUAGE CHANGE
    // ============================================================

    private void OnSelectedLocaleChanged(Locale locale)
    {
        if (!started)
            return;

        BeginLocalizationLoad();
    }

    // ============================================================
    // 33 - INSPECTOR VALIDATION
    // ============================================================

    private void OnValidate()
    {
        appearanceDelay =
            Mathf.Max(0f, appearanceDelay);

        appearanceDuration =
            Mathf.Max(0.01f, appearanceDuration);

        minimumCameraWait =
            Mathf.Max(0f, minimumCameraWait);

        cameraStableDuration =
            Mathf.Max(0.01f, cameraStableDuration);

        maximumCameraWait =
            Mathf.Max(
                minimumCameraWait,
                maximumCameraWait
            );

        cameraSpeedThreshold =
            Mathf.Max(
                0.001f,
                cameraSpeedThreshold
            );

        fallbackLadderWorldSize.x =
            Mathf.Max(
                0.01f,
                fallbackLadderWorldSize.x
            );

        fallbackLadderWorldSize.y =
            Mathf.Max(
                0.01f,
                fallbackLadderWorldSize.y
            );
    }

    // ============================================================
    // 34 - CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        DestroySpotlightResources();
    }

    private void DestroySpotlightResources()
    {
        if (spotlightSprite != null)
        {
            Destroy(spotlightSprite);
            spotlightSprite = null;
        }

        if (spotlightTexture != null)
        {
            Destroy(spotlightTexture);
            spotlightTexture = null;
        }

        spotlightPixels = null;
    }
}
