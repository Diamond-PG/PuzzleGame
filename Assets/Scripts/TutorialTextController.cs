using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TutorialTextController : MonoBehaviour
{
    // ============================================================
    // 01 - TUTORIAL TEXT
    // ============================================================

    [Header("TUTORIAL TEXT")]
    [SerializeField] private TMP_Text tutorialText;

    // ============================================================
    // 02 - TUTORIAL PAGES
    // ============================================================

    [Header("TUTORIAL PAGES")]
    [TextArea(3, 8)]
    [SerializeField] private string[] pages;

    // ============================================================
    // 03 - LOCALIZATION
    // ============================================================

    [Header("LOCALIZATION")]
    [SerializeField] private bool useLocalization = true;
    [SerializeField] private string localizationTableName = "Tutorial";
    [SerializeField] private string localizationKeyPrefix = "tutorial_";

    // ============================================================
    // 04 - NAVIGATION BUTTONS
    // ============================================================

    [Header("NAVIGATION BUTTONS")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    // ============================================================
    // 05 - START SETTINGS
    // ============================================================

    [Header("START SETTINGS")]
    [SerializeField, Min(0)] private int startPage = 0;

    // ============================================================
    // 06 - NAME INPUT
    // ============================================================

    [Header("NAME INPUT")]
    [SerializeField, Min(0)] private int nameInputPage = 1;
    [SerializeField, Min(0)] private int nameConfirmationPage = 2;
    [SerializeField] private PlayerNameInputController playerNameInputController;

    // ============================================================
    // 07 - KEEPER SHARED REFERENCES
    // ============================================================

    [Header("KEEPER - SHARED REFERENCES")]
    [SerializeField] private Image puzzleKeeperImage;
    [SerializeField] private RectTransform keeperPuzzleIcon;

    // ============================================================
    // 08 - KEEPER 1 - DIALOGUE
    // ============================================================

    [Header("KEEPER 1 - DIALOGUE")]
    [SerializeField] private Sprite dialogueKeeperSprite;

    [SerializeField] private Vector2 dialoguePuzzleIconPosition =
        new Vector2(-27.6f, -75.8f);

    [SerializeField] private Vector2 dialoguePuzzleIconSize =
        new Vector2(27f, 27f);

    [SerializeField] private float dialoguePuzzleIconRotation = -20f;

    // ============================================================
    // 09 - KEEPER 2 - TRAINING
    // ============================================================

    [Header("KEEPER 2 - TRAINING")]
    [SerializeField] private Sprite trainingKeeperSprite;

    [SerializeField, Min(0)] private int trainingStartPage = 13;

    [SerializeField] private Vector2 trainingPuzzleIconPosition =
        new Vector2(-22f, -62f);

    [SerializeField] private Vector2 trainingPuzzleIconSize =
        new Vector2(27f, 27f);

    [SerializeField] private float trainingPuzzleIconRotation = -20f;

    // ============================================================
    // 10 - KEEPER 3 - REJOICES
    // ============================================================

    [Header("KEEPER 3 - REJOICES")]
    [SerializeField] private Sprite rejoicesKeeperSprite;

    [SerializeField, Min(0)] private int rejoicesStartPage = 20;
    [SerializeField, Min(0)] private int rejoicesEndPage = 21;

    [SerializeField] private Vector2 rejoicesPuzzleIconPosition =
        new Vector2(-22f, -62f);

    [SerializeField] private Vector2 rejoicesPuzzleIconSize =
        new Vector2(27f, 27f);

    [SerializeField] private float rejoicesPuzzleIconRotation = -20f;

    // ============================================================
    // 11 - KEEPER 4 - DOOR
    // ============================================================

    [Header("KEEPER 4 - DOOR")]
    [SerializeField] private Sprite doorKeeperSprite;

    [SerializeField, Min(0)] private int doorStartPage = 22;
    [SerializeField, Min(0)] private int doorEndPage = 22;

    [SerializeField] private Vector2 doorPuzzleIconPosition =
        new Vector2(-22f, -62f);

    [SerializeField] private Vector2 doorPuzzleIconSize =
        new Vector2(27f, 27f);

    [SerializeField] private float doorPuzzleIconRotation = -20f;

    // ============================================================
    // 12 - MOVEMENT TUTORIAL
    // ============================================================

    [Header("MOVEMENT TUTORIAL")]
    [SerializeField, Min(0)] private int movementIntroPage = 13;
    [SerializeField, Min(0)] private int movementPracticePage = 14;
    [SerializeField, Min(0)] private int movementSuccessPage = 15;

    // ============================================================
    // 13 - JUMP TUTORIAL
    // ============================================================

    [Header("JUMP TUTORIAL")]
    [SerializeField, Min(0)] private int jumpPracticePage = 16;
    [SerializeField, Min(0)] private int jumpSuccessPage = 17;
    [SerializeField] private CanvasGroup jumpControlGroup;

    // ============================================================
    // 14 - KICK TUTORIAL
    // ============================================================

    [Header("KICK TUTORIAL")]
    [SerializeField, Min(0)] private int kickIntroPage = 18;
    [SerializeField, Min(0)] private int kickPracticePage = 19;
    [SerializeField, Min(0)] private int kickSuccessPage = 20;
    [SerializeField] private CanvasGroup kickControlGroup;

    // ============================================================
    // 15 - TUTORIAL CONTROL VISUALS
    // ============================================================

    [Header("TUTORIAL CONTROL VISUALS")]
    [SerializeField] private CanvasGroup leftControlGroup;
    [SerializeField] private CanvasGroup rightControlGroup;
    [SerializeField] private CanvasGroup[] lockedControlGroups;

    [SerializeField, Range(0f, 1f)]
    private float lockedControlAlpha = 0.30f;

    [SerializeField, Range(0f, 1f)]
    private float unlockedControlAlpha = 1f;

    // ============================================================
    // 16 - DARKNESS MASK TUTORIAL WINDOWS
    // ============================================================

    [Header("DARKNESS MASK TUTORIAL WINDOWS")]
    [SerializeField] private PrisonDarknessMask prisonDarknessMask;

    // ============================================================
    // 17 - AUTOSAVE - MAIN KEEPER
    // ============================================================

    [Header("AUTOSAVE - MAIN KEEPER")]
    [Tooltip("Remember the exact right Keeper dialogue page. " +
             "Existing lesson progress is preserved.")]
    [SerializeField] private bool enableExactPageAutosave = true;

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private int currentPage;
    private bool nameConfirmed;
    private string playerName = "";

    private bool movementLeftUsed;
    private bool movementRightUsed;
    private bool movementTutorialCompleted;
    private bool jumpTutorialCompleted;
    private bool kickTutorialCompleted;

    private bool tutorialSuppressed;
    private bool tutorialStarted;
    private bool presentationVisible = true;

    private string[] localizedPages;
    private Coroutine localizationPreloadCoroutine;
    private int localizationLoadVersion;
    private bool localizationReady;

    private int lastKeeperPose = -1;
    private Sprite lastKeeperSprite;
    private Vector2 lastPuzzlePosition;
    private Vector2 lastPuzzleSize;
    private float lastPuzzleRotation;
    private bool keeperVisualInitialized;

    private int lastAppliedMaskStage = -1;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextPage);
            nextButton.onClick.AddListener(NextPage);
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(PreviousPage);
            previousButton.onClick.AddListener(PreviousPage);
        }

        RestoreSavedProgress();

        tutorialSuppressed = TutorialProgressManager.IntroCompleted;
    }

    // ============================================================
    // ENABLE / DISABLE
    // ============================================================

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged +=
            OnSelectedLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -=
            OnSelectedLocaleChanged;

        localizationLoadVersion++;

        if (localizationPreloadCoroutine != null)
        {
            StopCoroutine(localizationPreloadCoroutine);
            localizationPreloadCoroutine = null;
        }
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        tutorialStarted = true;

        if (prisonDarknessMask == null)
            prisonDarknessMask = FindFirstObjectByType<PrisonDarknessMask>();

        if (pages == null || pages.Length == 0)
        {
            tutorialSuppressed = true;
            HideTutorialPresentation();
            UpdateTutorialControls();
            return;
        }

        if (tutorialSuppressed)
        {
            currentPage = Mathf.Clamp(
                doorStartPage, 0, pages.Length - 1
            );

            HideTutorialPresentation();
            UpdateTutorialControls();
            UpdateDarknessMaskWindows();
            return;
        }

        currentPage = Mathf.Clamp(
            GetResumePage(), 0, pages.Length - 1
        );

        presentationVisible = true;

        UpdateKeeperVisual();
        UpdateTutorialControls();
        UpdateDarknessMaskWindows();

        if (useLocalization)
        {
            BeginLocalizationPreload();
        }
        else
        {
            localizationReady = true;
            ShowCurrentPage();
        }
    }

    // ============================================================
    // SAVED PROGRESS
    // ============================================================

    private void RestoreSavedProgress()
    {
        playerName = TutorialProgressManager.PlayerName;
        nameConfirmed = TutorialProgressManager.HasPlayerName;

        movementTutorialCompleted =
            TutorialProgressManager.MovementCompleted;

        jumpTutorialCompleted =
            TutorialProgressManager.JumpCompleted;

        kickTutorialCompleted =
            TutorialProgressManager.KickCompleted;

        movementLeftUsed = movementTutorialCompleted ||
            TutorialProgressManager.MovementLeftUsed;

        movementRightUsed = movementTutorialCompleted ||
            TutorialProgressManager.MovementRightUsed;
    }

    private int GetResumePage()
    {
        // The exact page is preferred when a saved page exists.
        // Legacy saves without a page still use the original
        // milestone-based resume logic below.
        if (enableExactPageAutosave &&
            TutorialProgressManager.HasSavedMainTutorialPage)
        {
            int savedPage = TutorialProgressManager.SavedMainTutorialPage;

            if (IsValidPage(savedPage))
            {
                // Do not skip name entry with an old saved page.
                if (!nameConfirmed)
                {
                    if (savedPage <= nameInputPage)
                        return savedPage;
                }
                else
                {
                    // Once the name is confirmed, don't send the
                    // player back to the name input field.
                    return savedPage == nameInputPage
                        ? nameConfirmationPage
                        : savedPage;
                }
            }
        }

        if (!nameConfirmed)
            return startPage;

        if (!movementTutorialCompleted)
            return movementIntroPage;

        if (!jumpTutorialCompleted)
            return jumpPracticePage;

        if (!kickTutorialCompleted)
            return kickIntroPage;

        if (!TutorialProgressManager.DoorLessonReached)
            return rejoicesStartPage;

        return doorStartPage;
    }

    // ============================================================
    // PRESENTATION VISIBILITY
    // ============================================================

    private void HideTutorialPresentation()
    {
        presentationVisible = false;

        if (tutorialText != null)
        {
            tutorialText.text = "";
            tutorialText.gameObject.SetActive(false);
        }

        if (playerNameInputController != null)
            playerNameInputController.HideNameInput();

        if (puzzleKeeperImage != null)
            puzzleKeeperImage.gameObject.SetActive(false);

        if (keeperPuzzleIcon != null)
            keeperPuzzleIcon.gameObject.SetActive(false);

        SetButtonVisible(nextButton, false);
        SetButtonVisible(previousButton, false);
    }

    public void SetTutorialPresentationVisible(bool visible)
    {
        if (tutorialSuppressed)
        {
            HideTutorialPresentation();
            return;
        }

        presentationVisible = visible;

        if (!visible)
        {
            if (tutorialText != null)
                tutorialText.gameObject.SetActive(false);

            if (playerNameInputController != null)
                playerNameInputController.HideNameInput();

            if (puzzleKeeperImage != null)
                puzzleKeeperImage.gameObject.SetActive(false);

            if (keeperPuzzleIcon != null)
                keeperPuzzleIcon.gameObject.SetActive(false);

            SetButtonVisible(nextButton, false);
            SetButtonVisible(previousButton, false);
            return;
        }

        if (!useLocalization || localizationReady)
            ShowCurrentPage();
    }

    public bool ShouldShowTutorial()
    {
        return !tutorialSuppressed &&
               pages != null &&
               pages.Length > 0;
    }

    // ============================================================
    // KEEPER LIVE PREVIEW
    // ============================================================

    private void LateUpdate()
    {
        if (!tutorialStarted || tutorialSuppressed)
            return;

        int pose = GetKeeperPose();

        Sprite sprite;
        Vector2 position;
        Vector2 size;
        float rotation;

        GetKeeperSettings(
            pose,
            out sprite,
            out position,
            out size,
            out rotation
        );

        if (!keeperVisualInitialized ||
            lastKeeperPose != pose ||
            lastKeeperSprite != sprite ||
            lastPuzzlePosition != position ||
            lastPuzzleSize != size ||
            !Mathf.Approximately(lastPuzzleRotation, rotation))
        {
            ApplyKeeperSettings(
                pose, sprite, position, size, rotation
            );
        }
    }

    // ============================================================
    // LOCALIZATION
    // ============================================================

    private void BeginLocalizationPreload()
    {
        if (tutorialSuppressed ||
            pages == null ||
            pages.Length == 0)
            return;

        localizationLoadVersion++;
        int loadVersion = localizationLoadVersion;

        if (localizationPreloadCoroutine != null)
        {
            StopCoroutine(localizationPreloadCoroutine);
            localizationPreloadCoroutine = null;
        }

        localizationReady = false;
        localizedPages = new string[pages.Length];

        if (tutorialText != null)
        {
            tutorialText.text = "";
            tutorialText.gameObject.SetActive(false);
        }

        if (playerNameInputController != null)
            playerNameInputController.HideNameInput();

        UpdateButtons();

        localizationPreloadCoroutine = StartCoroutine(
            PreloadLocalizedPagesCoroutine(loadVersion)
        );
    }

    private IEnumerator PreloadLocalizedPagesCoroutine(int loadVersion)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (loadVersion != localizationLoadVersion)
                yield break;

            string key = GetLocalizationKey(i);

            var operation =
                LocalizationSettings.StringDatabase
                    .GetLocalizedStringAsync(
                        localizationTableName, key
                    );

            yield return operation;

            if (loadVersion != localizationLoadVersion)
                yield break;

            if (operation.Status ==
                AsyncOperationStatus.Succeeded &&
                !string.IsNullOrWhiteSpace(operation.Result))
            {
                localizedPages[i] = operation.Result;
            }
            else
            {
                localizedPages[i] = GetFallbackPageText(i);

                Debug.LogWarning(
                    "TutorialTextController: localization failed. " +
                    "Table = " + localizationTableName +
                    ", Key = " + key +
                    ". Using Pages fallback."
                );
            }
        }

        if (loadVersion != localizationLoadVersion)
            yield break;

        localizationReady = true;
        localizationPreloadCoroutine = null;

        ShowCurrentPage();
    }

    // ============================================================
    // NAVIGATION
    // ============================================================

    public void NextPage()
    {
        if (!CanNavigate() || IsPracticePageBlocked())
            return;

        int targetPage = GetNextPageIndex();

        if (!IsValidPage(targetPage))
            return;

        currentPage = targetPage;
        ShowCurrentPage();
    }

    public void PreviousPage()
    {
        if (!CanNavigate() || IsPracticePageBlocked())
            return;

        int targetPage = GetPreviousPageIndex();

        if (!IsValidPage(targetPage))
            return;

        currentPage = targetPage;
        ShowCurrentPage();
    }

    private bool CanNavigate()
    {
        if (tutorialSuppressed || !presentationVisible ||
            pages == null || pages.Length == 0)
            return false;

        if (useLocalization && !localizationReady)
            return false;

        if (!nameConfirmed && currentPage == nameInputPage)
            return false;

        return true;
    }

    private bool IsPracticePageBlocked()
    {
        if (currentPage == movementPracticePage &&
            !movementTutorialCompleted)
            return true;

        if (currentPage == jumpPracticePage &&
            !jumpTutorialCompleted)
            return true;

        if (currentPage == kickPracticePage &&
            !kickTutorialCompleted)
            return true;

        return false;
    }

    // ============================================================
    // PAGE DISPLAY
    // ============================================================

    private void ShowCurrentPage()
    {
        if (tutorialSuppressed)
        {
            HideTutorialPresentation();
            UpdateTutorialControls();
            UpdateDarknessMaskWindows();
            return;
        }

        if (pages == null || pages.Length == 0)
            return;

        if (useLocalization && !localizationReady)
            return;

        currentPage = Mathf.Clamp(
            currentPage, 0, pages.Length - 1
        );

        if (currentPage >= doorStartPage)
            TutorialProgressManager.ReachDoorLesson();

        // Save on each real page change (and on first display),
        // not only on application quit. This is reliable on mobile.
        if (enableExactPageAutosave)
            TutorialProgressManager.SaveMainTutorialPage(currentPage);

        bool shouldShowNameInput =
            !nameConfirmed && currentPage == nameInputPage;

        if (!presentationVisible)
        {
            UpdateTutorialControls();
            UpdateDarknessMaskWindows();
            UpdateButtons();
            return;
        }

        string pageText = ApplyPlayerName(
            GetPageText(currentPage)
        );

        if (tutorialText != null)
        {
            tutorialText.text = pageText;
            tutorialText.gameObject.SetActive(true);
        }

        if (playerNameInputController != null)
        {
            if (shouldShowNameInput)
                playerNameInputController.ShowNameInput();
            else
                playerNameInputController.HideNameInput();
        }

        if (puzzleKeeperImage != null)
            puzzleKeeperImage.gameObject.SetActive(true);

        if (keeperPuzzleIcon != null)
            keeperPuzzleIcon.gameObject.SetActive(true);

        UpdateKeeperVisual();
        UpdateTutorialControls();
        UpdateDarknessMaskWindows();
        UpdateButtons();
    }

    // ============================================================
    // MOVEMENT TUTORIAL
    // ============================================================

    public void ReportHorizontalTutorialInput(float horizontalInput)
    {
        if (tutorialSuppressed ||
            currentPage != movementPracticePage ||
            movementTutorialCompleted)
            return;

        if (horizontalInput < -0.001f && !movementLeftUsed)
        {
            movementLeftUsed = true;
            TutorialProgressManager.MarkMovementLeftUsed();
        }

        if (horizontalInput > 0.001f && !movementRightUsed)
        {
            movementRightUsed = true;
            TutorialProgressManager.MarkMovementRightUsed();
        }

        if (movementLeftUsed && movementRightUsed)
            CompleteMovementTutorial();
    }

    private void CompleteMovementTutorial()
    {
        if (movementTutorialCompleted)
            return;

        movementTutorialCompleted = true;
        TutorialProgressManager.CompleteMovement();

        if (IsValidPage(movementSuccessPage))
        {
            currentPage = movementSuccessPage;
            ShowCurrentPage();
        }
        else
        {
            UpdateTutorialControls();
            UpdateDarknessMaskWindows();
            UpdateButtons();
        }
    }

    // ============================================================
    // JUMP TUTORIAL
    // ============================================================

    public void ReportJumpTutorialInput()
    {
        if (tutorialSuppressed ||
            currentPage != jumpPracticePage ||
            jumpTutorialCompleted)
            return;

        CompleteJumpTutorial();
    }

    private void CompleteJumpTutorial()
    {
        if (jumpTutorialCompleted)
            return;

        jumpTutorialCompleted = true;
        TutorialProgressManager.CompleteJump();

        if (IsValidPage(jumpSuccessPage))
        {
            currentPage = jumpSuccessPage;
            ShowCurrentPage();
        }
        else
        {
            UpdateTutorialControls();
            UpdateDarknessMaskWindows();
            UpdateButtons();
        }
    }

    // ============================================================
    // KICK TUTORIAL
    // ============================================================

    public void ReportKickTutorialInput()
    {
        if (tutorialSuppressed ||
            currentPage != kickPracticePage ||
            kickTutorialCompleted)
            return;

        CompleteKickTutorial();
    }

    private void CompleteKickTutorial()
    {
        if (kickTutorialCompleted)
            return;

        kickTutorialCompleted = true;
        TutorialProgressManager.CompleteKick();

        if (IsValidPage(kickSuccessPage))
        {
            currentPage = kickSuccessPage;
            ShowCurrentPage();
        }
        else
        {
            UpdateTutorialControls();
            UpdateDarknessMaskWindows();
            UpdateButtons();
        }
    }

    // ============================================================
    // CONTROL VISUALS
    // ============================================================

    private void UpdateTutorialControls()
    {
        bool horizontalVisible =
            movementTutorialCompleted ||
            currentPage >= movementIntroPage;

        bool horizontalInteractable =
            movementTutorialCompleted ||
            currentPage >= movementPracticePage;

        SetControlGroupState(
            leftControlGroup,
            horizontalVisible,
            horizontalInteractable
        );

        SetControlGroupState(
            rightControlGroup,
            horizontalVisible,
            horizontalInteractable
        );

        if (lockedControlGroups != null)
        {
            foreach (CanvasGroup group in lockedControlGroups)
            {
                if (group == jumpControlGroup ||
                    group == kickControlGroup)
                    continue;

                SetControlGroupState(group, false, false);
            }
        }

        bool jumpUnlocked =
            jumpTutorialCompleted ||
            currentPage >= jumpPracticePage;

        SetControlGroupState(
            jumpControlGroup,
            jumpUnlocked,
            jumpUnlocked
        );

        if (kickTutorialCompleted)
        {
            SetControlGroupState(kickControlGroup, true, true);
        }
        else if (currentPage < kickIntroPage)
        {
            SetControlGroupState(kickControlGroup, false, false);
        }
        else if (currentPage < kickPracticePage)
        {
            SetControlGroupPreviewState(kickControlGroup);
        }
        else
        {
            SetControlGroupState(kickControlGroup, true, true);
        }
    }

    private void SetControlGroupState(
        CanvasGroup group,
        bool visuallyUnlocked,
        bool interactable
    )
    {
        if (group == null)
            return;

        group.alpha = visuallyUnlocked
            ? unlockedControlAlpha
            : lockedControlAlpha;

        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }

    private void SetControlGroupPreviewState(CanvasGroup group)
    {
        if (group == null)
            return;

        group.alpha = unlockedControlAlpha;
        group.interactable = true;
        group.blocksRaycasts = false;
    }

    // ============================================================
    // DARKNESS MASK TUTORIAL WINDOWS
    // ============================================================

    private void UpdateDarknessMaskWindows()
    {
        if (prisonDarknessMask == null)
            return;

        int stage = 0;

        if (movementTutorialCompleted ||
            currentPage >= movementIntroPage)
        {
            stage = 1;
        }

        if (jumpTutorialCompleted ||
            currentPage >= jumpPracticePage)
        {
            stage = 2;
        }

        if (kickTutorialCompleted ||
            currentPage >= kickIntroPage)
        {
            stage = 3;
        }

        // Do not close previously opened windows
        // when navigating back to an earlier page.
        stage = Mathf.Max(stage, lastAppliedMaskStage);

        if (stage == lastAppliedMaskStage)
            return;

        lastAppliedMaskStage = stage;
        prisonDarknessMask.SetTutorialStage(stage);
    }

    // ============================================================
    // TEXT HELPERS
    // ============================================================

    private string GetPageText(int pageIndex)
    {
        if (useLocalization &&
            localizationReady &&
            localizedPages != null &&
            pageIndex >= 0 &&
            pageIndex < localizedPages.Length &&
            !string.IsNullOrWhiteSpace(localizedPages[pageIndex]))
        {
            return localizedPages[pageIndex];
        }

        return GetFallbackPageText(pageIndex);
    }

    private string GetLocalizationKey(int pageIndex)
    {
        return localizationKeyPrefix +
               pageIndex.ToString("00");
    }

    private string GetFallbackPageText(int pageIndex)
    {
        if (!IsValidPage(pageIndex))
            return "";

        return pages[pageIndex];
    }

    private string ApplyPlayerName(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        if (nameConfirmed &&
            !string.IsNullOrWhiteSpace(playerName))
        {
            return text.Replace("{name}", playerName);
        }

        return text;
    }

    private bool IsValidPage(int pageIndex)
    {
        return pages != null &&
               pageIndex >= 0 &&
               pageIndex < pages.Length;
    }

    // ============================================================
    // KEEPER VISUAL
    // ============================================================

    private int GetKeeperPose()
    {
        if (currentPage >= doorStartPage &&
            currentPage <= doorEndPage &&
            doorKeeperSprite != null)
            return 4;

        if (currentPage >= rejoicesStartPage &&
            currentPage <= rejoicesEndPage &&
            rejoicesKeeperSprite != null)
            return 3;

        if (currentPage >= trainingStartPage)
            return 2;

        return 1;
    }

    private void GetKeeperSettings(
        int pose,
        out Sprite sprite,
        out Vector2 position,
        out Vector2 size,
        out float rotation
    )
    {
        switch (pose)
        {
            case 4:
                sprite = doorKeeperSprite;
                position = doorPuzzleIconPosition;
                size = doorPuzzleIconSize;
                rotation = doorPuzzleIconRotation;
                break;

            case 3:
                sprite = rejoicesKeeperSprite;
                position = rejoicesPuzzleIconPosition;
                size = rejoicesPuzzleIconSize;
                rotation = rejoicesPuzzleIconRotation;
                break;

            case 2:
                sprite = trainingKeeperSprite;
                position = trainingPuzzleIconPosition;
                size = trainingPuzzleIconSize;
                rotation = trainingPuzzleIconRotation;
                break;

            default:
                sprite = dialogueKeeperSprite;
                position = dialoguePuzzleIconPosition;
                size = dialoguePuzzleIconSize;
                rotation = dialoguePuzzleIconRotation;
                break;
        }
    }

    private void UpdateKeeperVisual()
    {
        if (tutorialSuppressed)
            return;

        int pose = GetKeeperPose();

        Sprite sprite;
        Vector2 position;
        Vector2 size;
        float rotation;

        GetKeeperSettings(
            pose,
            out sprite,
            out position,
            out size,
            out rotation
        );

        ApplyKeeperSettings(
            pose, sprite, position, size, rotation
        );
    }

    private void ApplyKeeperSettings(
        int pose,
        Sprite sprite,
        Vector2 position,
        Vector2 size,
        float rotation
    )
    {
        if (puzzleKeeperImage != null && sprite != null)
            puzzleKeeperImage.sprite = sprite;

        if (keeperPuzzleIcon != null)
        {
            keeperPuzzleIcon.anchoredPosition = position;
            keeperPuzzleIcon.sizeDelta = size;

            Vector3 euler = keeperPuzzleIcon.localEulerAngles;
            euler.z = rotation;
            keeperPuzzleIcon.localEulerAngles = euler;
        }

        lastKeeperPose = pose;
        lastKeeperSprite = sprite;
        lastPuzzlePosition = position;
        lastPuzzleSize = size;
        lastPuzzleRotation = rotation;
        keeperVisualInitialized = true;
    }

    // ============================================================
    // LOCALE CHANGE
    // ============================================================

    private void OnSelectedLocaleChanged(Locale newLocale)
    {
        if (!tutorialStarted ||
            tutorialSuppressed ||
            !useLocalization ||
            pages == null ||
            pages.Length == 0)
            return;

        BeginLocalizationPreload();
    }

    // ============================================================
    // BUTTONS
    // ============================================================

    private void UpdateButtons()
    {
        if (tutorialSuppressed ||
            !presentationVisible ||
            pages == null ||
            pages.Length == 0 ||
            (useLocalization && !localizationReady) ||
            (!nameConfirmed && currentPage == nameInputPage) ||
            IsPracticePageBlocked())
        {
            SetButtonVisible(nextButton, false);
            SetButtonVisible(previousButton, false);
            return;
        }

        SetButtonVisible(
            previousButton,
            IsValidPage(GetPreviousPageIndex())
        );

        SetButtonVisible(
            nextButton,
            IsValidPage(GetNextPageIndex())
        );
    }

    private int GetNextPageIndex()
    {
        int targetPage = currentPage + 1;

        if (nameConfirmed && targetPage == nameInputPage)
            targetPage = nameConfirmationPage;

        return targetPage;
    }

    private int GetPreviousPageIndex()
    {
        int targetPage = currentPage - 1;

        if (nameConfirmed && targetPage == nameInputPage)
            targetPage = nameInputPage - 1;

        return targetPage;
    }

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
            button.gameObject.SetActive(visible);
    }

    // ============================================================
    // PLAYER NAME
    // ============================================================

    public void ConfirmPlayerName(string confirmedName)
    {
        if (string.IsNullOrWhiteSpace(confirmedName))
            return;

        playerName = confirmedName.Trim();
        nameConfirmed = true;

        TutorialProgressManager.SavePlayerName(playerName);

        if (IsValidPage(nameConfirmationPage))
            currentPage = nameConfirmationPage;

        ShowCurrentPage();
    }

    // ============================================================
    // PUBLIC CONTROLS
    // ============================================================

    public void ResetTutorial()
    {
        if (pages == null || pages.Length == 0)
            return;

        RestoreSavedProgress();

        tutorialSuppressed =
            TutorialProgressManager.IntroCompleted;

        currentPage = tutorialSuppressed
            ? Mathf.Clamp(doorStartPage, 0, pages.Length - 1)
            : Mathf.Clamp(GetResumePage(), 0, pages.Length - 1);

        keeperVisualInitialized = false;
        lastAppliedMaskStage = -1;

        if (tutorialSuppressed)
        {
            HideTutorialPresentation();
            UpdateTutorialControls();
            UpdateDarknessMaskWindows();
            return;
        }

        presentationVisible = true;
        UpdateKeeperVisual();
        UpdateTutorialControls();
        UpdateDarknessMaskWindows();

        if (!useLocalization || localizationReady)
            ShowCurrentPage();
    }

    public void GoToPage(int pageIndex)
    {
        if (tutorialSuppressed ||
            pages == null ||
            pages.Length == 0 ||
            (useLocalization && !localizationReady))
            return;

        int targetPage = Mathf.Clamp(
            pageIndex, 0, pages.Length - 1
        );

        if (nameConfirmed && targetPage == nameInputPage)
            targetPage = nameConfirmationPage;

        currentPage = targetPage;
        ShowCurrentPage();
    }

    public int GetCurrentPage()
    {
        return currentPage;
    }

    public int GetPageCount()
    {
        return pages != null ? pages.Length : 0;
    }

    public bool IsNameConfirmed()
    {
        return nameConfirmed;
    }

    public string GetConfirmedPlayerName()
    {
        return playerName;
    }

    public bool IsLocalizationReady()
    {
        return !useLocalization || localizationReady;
    }

    public bool IsMovementTutorialCompleted()
    {
        return movementTutorialCompleted;
    }

    public bool IsJumpTutorialCompleted()
    {
        return jumpTutorialCompleted;
    }

    public bool IsKickTutorialCompleted()
    {
        return kickTutorialCompleted;
    }

    public bool IsDoorLessonUnlocked()
    {
        return TutorialProgressManager.DoorLessonReached;
    }

    public bool IsIntroTutorialCompleted()
    {
        return TutorialProgressManager.IntroCompleted;
    }

    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);

        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousPage);
    }

#if UNITY_EDITOR

    // ============================================================
    // VALIDATION
    // ============================================================

    private void OnValidate()
    {
        startPage = Mathf.Max(0, startPage);

        nameInputPage = Mathf.Max(0, nameInputPage);
        nameConfirmationPage = Mathf.Max(0, nameConfirmationPage);

        trainingStartPage = Mathf.Max(0, trainingStartPage);

        rejoicesStartPage = Mathf.Max(0, rejoicesStartPage);
        rejoicesEndPage = Mathf.Max(
            rejoicesStartPage, rejoicesEndPage
        );

        doorStartPage = Mathf.Max(0, doorStartPage);
        doorEndPage = Mathf.Max(
            doorStartPage, doorEndPage
        );

        movementIntroPage = Mathf.Max(0, movementIntroPage);
        movementPracticePage = Mathf.Max(0, movementPracticePage);
        movementSuccessPage = Mathf.Max(0, movementSuccessPage);

        jumpPracticePage = Mathf.Max(0, jumpPracticePage);
        jumpSuccessPage = Mathf.Max(0, jumpSuccessPage);

        kickIntroPage = Mathf.Max(0, kickIntroPage);
        kickPracticePage = Mathf.Max(0, kickPracticePage);
        kickSuccessPage = Mathf.Max(0, kickSuccessPage);

        lockedControlAlpha = Mathf.Clamp01(lockedControlAlpha);
        unlockedControlAlpha = Mathf.Clamp01(unlockedControlAlpha);

        if (pages == null || pages.Length == 0)
            return;

        int maxPage = pages.Length - 1;

        startPage = Mathf.Clamp(startPage, 0, maxPage);

        nameInputPage = Mathf.Clamp(nameInputPage, 0, maxPage);
        nameConfirmationPage = Mathf.Clamp(
            nameConfirmationPage, 0, maxPage
        );

        trainingStartPage = Mathf.Clamp(
            trainingStartPage, 0, maxPage
        );

        rejoicesStartPage = Mathf.Clamp(
            rejoicesStartPage, 0, maxPage
        );

        rejoicesEndPage = Mathf.Clamp(
            rejoicesEndPage, rejoicesStartPage, maxPage
        );

        doorStartPage = Mathf.Clamp(doorStartPage, 0, maxPage);

        doorEndPage = Mathf.Clamp(
            doorEndPage, doorStartPage, maxPage
        );

        movementIntroPage = Mathf.Clamp(
            movementIntroPage, 0, maxPage
        );

        movementPracticePage = Mathf.Clamp(
            movementPracticePage, 0, maxPage
        );

        movementSuccessPage = Mathf.Clamp(
            movementSuccessPage, 0, maxPage
        );

        jumpPracticePage = Mathf.Clamp(
            jumpPracticePage, 0, maxPage
        );

        jumpSuccessPage = Mathf.Clamp(
            jumpSuccessPage, 0, maxPage
        );

        kickIntroPage = Mathf.Clamp(
            kickIntroPage, 0, maxPage
        );

        kickPracticePage = Mathf.Clamp(
            kickPracticePage, 0, maxPage
        );

        kickSuccessPage = Mathf.Clamp(
            kickSuccessPage, 0, maxPage
        );
    }

#endif
}