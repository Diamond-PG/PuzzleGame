
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Realm of the Lost Puzzle
///
/// Tutorial 49-54: Keeper returns after the first puzzle piece.
///
/// - Does not modify PuzzlePiecePickup.
/// - Does not trigger Level Complete.
/// - Waits for the entire pickup animation.
/// - Reuses TutorialFrame_Left and TutorialDimmer.
/// - Pauses gameplay during the conversation.
/// - Resumes gameplay after page 54.
/// - Saves completed dialogue independently.
/// - Each page has independent Keeper and medallion settings.
/// </summary>
[DisallowMultipleComponent]
public sealed class TutorialPuzzleFinale : MonoBehaviour
{
    private const int CompletedHeartLessonStage = 35;

    [System.Serializable]
    private class KeeperPose
    {
        [Header("KEEPER SPRITE")]
        public Sprite keeperSprite;

        [Header("KEEPER TRANSFORM")]
        public Vector2 keeperPosition =
            new Vector2(134.8f, -221f);

        public Vector2 keeperSize =
            new Vector2(230f, 230f);

        public float keeperRotationZ = 0f;

        public Vector2 keeperScale = Vector2.one;

        [Header("BLUE PUZZLE MEDALLION")]
        public Vector2 puzzlePosition =
            new Vector2(6.9f, -63.8f);

        public Vector2 puzzleSize =
            new Vector2(27f, 27f);

        public float puzzleRotationZ = 20f;

        public Vector2 puzzleScale = Vector2.one;
    }

    [Header("01 - FIRST PUZZLE PIECE")]
    [Tooltip("The first PuzzlePiecePickup from Level 1.")]
    [SerializeField]
    private PuzzlePiecePickup firstPuzzlePiece;

    [Tooltip("SpriteRenderer on the same puzzle piece.")]
    [SerializeField]
    private SpriteRenderer puzzlePieceRenderer;

    [Tooltip("Delay after the piece disappears.")]
    [SerializeField, Min(0f)]
    private float waitAfterPuzzleDisappears = 0.25f;

    [SerializeField]
    private bool requireCompletedHeartLesson = true;

    [Header("02 - EXISTING LEFT KEEPER FRAME")]
    [SerializeField]
    private TMP_Text leftTutorialText;

    [SerializeField]
    private CanvasGroup leftFrameCanvasGroup;

    [SerializeField]
    private Button goldNextButton;

    [SerializeField]
    private TutorialArrowPulse goldNextPulse;

    [SerializeField]
    private Image keeperImage;

    [SerializeField]
    private RectTransform keeperPuzzleIcon;

    [SerializeField]
    private GameObject tutorialDimmer;

    [Header("03 - PLAYER AND CONTROLS")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private CanvasGroup actionControlsGroup;

    [SerializeField]
    private CanvasGroup inventoryControlsGroup;

    [Header("04 - LOCALIZATION")]
    [SerializeField]
    private string localizationTable = "Tutorial";

    [SerializeField]
    private string page49Key = "tutorial_49";

    [SerializeField, TextArea(2, 4)]
    private string page49Fallback =
        "Первый фрагмент найден!";

    [SerializeField]
    private string page50Key = "tutorial_50";

    [SerializeField, TextArea(2, 4)]
    private string page50Fallback =
        "Ты вернул часть утраченной картины!";

    [SerializeField]
    private string page51Key = "tutorial_51";

    [SerializeField, TextArea(2, 4)]
    private string page51Fallback =
        "Спасибо, {name}. Это первый шаг";

    [SerializeField]
    private string page52Key = "tutorial_52";

    [SerializeField, TextArea(2, 4)]
    private string page52Fallback =
        "к разгадке нашей тайны.";

    [SerializeField]
    private string page53Key = "tutorial_53";

    [SerializeField, TextArea(2, 4)]
    private string page53Fallback =
        "Наш путь только начинается.";

    [SerializeField]
    private string page54Key = "tutorial_54";

    [SerializeField, TextArea(2, 4)]
    private string page54Fallback =
        "До скорой встречи, {name}!";

    [Header("05 - HAPPY KEEPER (TUTORIAL 49)")]
    [SerializeField]
    private KeeperPose page49Pose = new KeeperPose();

    [Header("06 - KEEPER POSE (TUTORIAL 50)")]
    [SerializeField]
    private KeeperPose page50Pose = new KeeperPose();

    [Header("07 - KEEPER POSE (TUTORIAL 51)")]
    [SerializeField]
    private KeeperPose page51Pose = new KeeperPose();

    [Header("08 - KEEPER POSE (TUTORIAL 52)")]
    [SerializeField]
    private KeeperPose page52Pose = new KeeperPose();

    [Header("09 - KEEPER POSE (TUTORIAL 53)")]
    [SerializeField]
    private KeeperPose page53Pose = new KeeperPose();

    [Header("10 - GOODBYE KEEPER (TUTORIAL 54)")]
    [Tooltip("Assign the new waving goodbye sprite.")]
    [SerializeField]
    private KeeperPose page54Pose = new KeeperPose();

    [Header("11 - FINAL FADE")]
    [SerializeField, Min(0f)]
    private float finalFadeDuration = 0.75f;

    [Header("12 - SAVING")]
    [SerializeField]
    private bool saveCompletedDialogue = true;

    [SerializeField]
    private string saveKey =
        "TutorialPuzzleFinale_Completed";

    [Header("13 - DEBUG")]
    [SerializeField]
    private bool debugLogs = false;

    private enum FinalePhase
    {
        Waiting,
        WaitingForAnimation,
        Conversation,
        Fading,
        Complete
    }

    private FinalePhase phase = FinalePhase.Waiting;

    private int currentPage = 49;

    private string[] localizedLines = new string[6];

    private int localizationVersion;

    private Coroutine localizationRoutine;
    private Coroutine puzzleWaitRoutine;
    private Coroutine fadeRoutine;

    private float previousTimeScale = 1f;

    private bool pausedByFinale;
    private bool movementLockedByFinale;

    private bool actionGroupCaptured;
    private bool inventoryGroupCaptured;

    private bool originalActionRaycasts = true;
    private bool originalInventoryRaycasts = true;

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

    private Image dimmerImage;
    private Sprite originalDimmerSprite;
    private Image.Type originalDimmerType;
    private bool originalDimmerRaycastTarget;
    private bool originalDimmerActive;

    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        ResolveReferences();

        localizedLines[0] = page49Fallback;
        localizedLines[1] = page50Fallback;
        localizedLines[2] = page51Fallback;
        localizedLines[3] = page52Fallback;
        localizedLines[4] = page53Fallback;
        localizedLines[5] = page54Fallback;

        if (dimmerImage != null)
        {
            originalDimmerSprite = dimmerImage.sprite;
            originalDimmerType = dimmerImage.type;
            originalDimmerRaycastTarget =
                dimmerImage.raycastTarget;
        }

        if (tutorialDimmer != null)
        {
            originalDimmerActive =
                tutorialDimmer.activeSelf;
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged +=
            OnLocaleChanged;

        LoadLocalization();
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

        if (saveCompletedDialogue &&
            PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            phase = FinalePhase.Complete;
        }
    }

    private void Update()
    {
        if (phase != FinalePhase.Waiting)
            return;

        if (firstPuzzlePiece == null)
            return;

        if (requireCompletedHeartLesson &&
            TutorialProgressManager.SavedLeftTutorialStage <
            CompletedHeartLessonStage)
        {
            return;
        }

        if (!firstPuzzlePiece.IsPickedUp)
            return;

        phase = FinalePhase.WaitingForAnimation;

        if (puzzleWaitRoutine != null)
            StopCoroutine(puzzleWaitRoutine);

        puzzleWaitRoutine =
            StartCoroutine(WaitForPuzzleAnimation());
    }

    // ============================================================
    // WAIT FOR PUZZLE ANIMATION
    // ============================================================

    private IEnumerator WaitForPuzzleAnimation()
    {
        while (firstPuzzlePiece != null)
        {
            if (puzzlePieceRenderer != null)
            {
                if (!puzzlePieceRenderer.enabled)
                    break;
            }
            else if (!firstPuzzlePiece.gameObject.activeInHierarchy)
            {
                break;
            }

            yield return null;
        }

        if (waitAfterPuzzleDisappears > 0f)
        {
            yield return new WaitForSecondsRealtime(
                waitAfterPuzzleDisappears);
        }

        while (leftFrameCanvasGroup != null &&
               leftFrameCanvasGroup.alpha > 0.01f)
        {
            yield return null;
        }

        puzzleWaitRoutine = null;

        if (phase == FinalePhase.WaitingForAnimation)
        {
            BeginConversation();
        }
    }

    // ============================================================
    // BEGIN CONVERSATION
    // ============================================================

    private void BeginConversation()
    {
        ResolveReferences();

        if (leftTutorialText == null ||
            leftFrameCanvasGroup == null ||
            goldNextButton == null ||
            keeperImage == null ||
            playerController == null)
        {
            Debug.LogWarning(
                "[PUZZLE FINALE] Missing Keeper UI or Player references.",
                this);

            phase = FinalePhase.Waiting;
            return;
        }

        CaptureKeeperBaseline();
        PauseGameplay();

        phase = FinalePhase.Conversation;
        currentPage = 49;

        ShowPage();

        if (debugLogs)
        {
            Debug.Log(
                "[PUZZLE FINALE] Conversation started.",
                this);
        }
    }

    // ============================================================
    // DISPLAY PAGE 49-54
    // ============================================================

    private void ShowPage()
    {
        if (phase != FinalePhase.Conversation)
            return;

        int index = currentPage - 49;

        if (index < 0 || index >= localizedLines.Length)
            return;

        KeeperPose pose = GetPoseForPage(currentPage);

        ApplyKeeperPose(pose);

        leftTutorialText.gameObject.SetActive(true);
        leftTutorialText.text =
            InsertPlayerName(localizedLines[index]);

        leftTutorialText.ForceMeshUpdate();

        leftFrameCanvasGroup.alpha = 1f;
        leftFrameCanvasGroup.interactable = true;
        leftFrameCanvasGroup.blocksRaycasts = true;

        if (tutorialDimmer != null)
        {
            tutorialDimmer.SetActive(true);

            if (dimmerImage != null)
                dimmerImage.raycastTarget = false;
        }

        goldNextButton.gameObject.SetActive(true);
        goldNextButton.interactable = true;

        if (goldNextPulse != null)
            goldNextPulse.SetPulseActive(true);

        if (debugLogs)
        {
            Debug.Log(
                "[PUZZLE FINALE] Tutorial " + currentPage,
                this);
        }
    }

    private KeeperPose GetPoseForPage(int page)
    {
        switch (page)
        {
            case 49:
                return page49Pose;

            case 50:
                return page50Pose;

            case 51:
                return page51Pose;

            case 52:
                return page52Pose;

            case 53:
                return page53Pose;

            case 54:
                return page54Pose;
        }

        return page49Pose;
    }

    // ============================================================
    // GOLD NEXT BUTTON
    // ============================================================

    private void OnGoldNextClicked()
    {
        if (phase != FinalePhase.Conversation)
            return;

        if (currentPage < 54)
        {
            currentPage++;
            ShowPage();
        }
        else
        {
            FinishConversation();
        }
    }

    // ============================================================
    // FINISH AND FADE
    // ============================================================

    private void FinishConversation()
    {
        if (phase != FinalePhase.Conversation)
            return;

        phase = FinalePhase.Fading;

        if (saveCompletedDialogue)
        {
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
        }

        goldNextButton.interactable = false;

        if (goldNextPulse != null)
            goldNextPulse.SetPulseActive(false);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;

        float startingAlpha =
            leftFrameCanvasGroup != null
                ? leftFrameCanvasGroup.alpha
                : 1f;

        while (finalFadeDuration > 0f &&
               elapsed < finalFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (leftFrameCanvasGroup != null)
            {
                leftFrameCanvasGroup.alpha =
                    Mathf.Lerp(
                        startingAlpha,
                        0f,
                        Mathf.Clamp01(
                            elapsed / finalFadeDuration)
                    );
            }

            yield return null;
        }

        fadeRoutine = null;

        HideKeeper();
        RestoreDimmer();
        RestoreKeeperBaseline();
        ResumeGameplay();

        phase = FinalePhase.Complete;

        if (debugLogs)
        {
            Debug.Log(
                "[PUZZLE FINALE] Dialogue completed. Gameplay resumed.",
                this);
        }

        // No Level Complete.
        // The player continues Level 1 normally.
    }

    // ============================================================
    // PLAYER NAME
    // ============================================================

    private string InsertPlayerName(string message)
    {
        if (string.IsNullOrEmpty(message))
            return "";

        string playerName =
            TutorialProgressManager.PlayerName;

        if (!string.IsNullOrWhiteSpace(playerName))
        {
            return message.Replace(
                "{name}",
                playerName);
        }

        return message
            .Replace(", {name}", "")
            .Replace("，{name}", "")
            .Replace(" {name}", "")
            .Replace("{name}", "");
    }

    // ============================================================
    // KEEPER POSES
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
            baselinePuzzlePosition =
                keeperPuzzleIcon.anchoredPosition;

            baselinePuzzleSize =
                keeperPuzzleIcon.sizeDelta;

            baselinePuzzleRotation =
                keeperPuzzleIcon.localRotation;

            baselinePuzzleScale =
                keeperPuzzleIcon.localScale;
        }

        keeperBaselineCaptured = true;
    }

    private void ApplyKeeperPose(KeeperPose pose)
    {
        if (keeperImage == null || pose == null)
            return;

        if (pose.keeperSprite != null)
            keeperImage.sprite = pose.keeperSprite;

        RectTransform r = keeperImage.rectTransform;

        r.anchoredPosition = pose.keeperPosition;
        r.sizeDelta = pose.keeperSize;

        r.localRotation = Quaternion.Euler(
            0f, 0f, pose.keeperRotationZ);

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

        keeperPuzzleIcon.localRotation =
            Quaternion.Euler(
                0f, 0f, pose.puzzleRotationZ);

        keeperPuzzleIcon.localScale =
            new Vector3(
                pose.puzzleScale.x,
                pose.puzzleScale.y,
                keeperPuzzleIcon.localScale.z);
    }

    private void RestoreKeeperBaseline()
    {
        if (!keeperBaselineCaptured || keeperImage == null)
            return;

        RectTransform r = keeperImage.rectTransform;

        keeperImage.sprite = baselineKeeperSprite;
        r.anchoredPosition = baselineKeeperPosition;
        r.sizeDelta = baselineKeeperSize;
        r.localRotation = baselineKeeperRotation;
        r.localScale = baselineKeeperScale;

        if (keeperPuzzleIcon == null)
            return;

        keeperPuzzleIcon.anchoredPosition =
            baselinePuzzlePosition;

        keeperPuzzleIcon.sizeDelta =
            baselinePuzzleSize;

        keeperPuzzleIcon.localRotation =
            baselinePuzzleRotation;

        keeperPuzzleIcon.localScale =
            baselinePuzzleScale;
    }

    // ============================================================
    // GAMEPLAY PAUSE
    // ============================================================

    private void PauseGameplay()
    {
        if (!pausedByFinale)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            pausedByFinale = true;
        }

        if (!movementLockedByFinale &&
            playerController != null)
        {
            playerController.SetHorizontalMovementLock(true);
            movementLockedByFinale = true;
        }

        if (actionControlsGroup != null)
        {
            if (!actionGroupCaptured)
            {
                originalActionRaycasts =
                    actionControlsGroup.blocksRaycasts;

                actionGroupCaptured = true;
            }

            actionControlsGroup.blocksRaycasts = false;
        }

        if (inventoryControlsGroup != null)
        {
            if (!inventoryGroupCaptured)
            {
                originalInventoryRaycasts =
                    inventoryControlsGroup.blocksRaycasts;

                inventoryGroupCaptured = true;
            }

            inventoryControlsGroup.blocksRaycasts = false;
        }
    }

    private void ResumeGameplay()
    {
        if (actionControlsGroup != null &&
            actionGroupCaptured)
        {
            actionControlsGroup.blocksRaycasts =
                originalActionRaycasts;
        }

        if (inventoryControlsGroup != null &&
            inventoryGroupCaptured)
        {
            inventoryControlsGroup.blocksRaycasts =
                originalInventoryRaycasts;
        }

        actionGroupCaptured = false;
        inventoryGroupCaptured = false;

        if (movementLockedByFinale &&
            playerController != null)
        {
            playerController.SetHorizontalMovementLock(false);
        }

        movementLockedByFinale = false;

        if (pausedByFinale)
            Time.timeScale = previousTimeScale;

        pausedByFinale = false;
    }

    // ============================================================
    // UI RESTORATION
    // ============================================================

    private void HideKeeper()
    {
        if (leftFrameCanvasGroup != null)
        {
            leftFrameCanvasGroup.alpha = 0f;
            leftFrameCanvasGroup.interactable = false;
            leftFrameCanvasGroup.blocksRaycasts = false;
        }

        if (leftTutorialText != null)
            leftTutorialText.text = "";

        if (goldNextButton != null)
        {
            goldNextButton.interactable = false;
            goldNextButton.gameObject.SetActive(false);
        }

        if (goldNextPulse != null)
            goldNextPulse.SetPulseActive(false);
    }

    private void RestoreDimmer()
    {
        if (dimmerImage != null)
        {
            dimmerImage.sprite = originalDimmerSprite;
            dimmerImage.type = originalDimmerType;

            dimmerImage.raycastTarget =
                originalDimmerRaycastTarget;
        }

        if (tutorialDimmer != null)
        {
            tutorialDimmer.SetActive(
                originalDimmerActive);
        }
    }

    // ============================================================
    // REFERENCES
    // ============================================================

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
                goldNextButton.GetComponent<
                    TutorialArrowPulse>();
        }

        if (keeperImage == null)
        {
            Transform t =
                transform.Find("PuzzleKeeper_Left");

            if (t != null)
                keeperImage =
                    t.GetComponent<Image>();
        }

        if (keeperPuzzleIcon == null &&
            keeperImage != null)
        {
            Transform t =
                keeperImage.transform.Find(
                    "KeeperPuzzleIcon");

            if (t != null)
            {
                keeperPuzzleIcon =
                    t.GetComponent<RectTransform>();
            }
        }

        if (tutorialDimmer == null &&
            transform.parent != null)
        {
            Transform t =
                transform.parent.Find(
                    "TutorialDimmer");

            if (t != null)
                tutorialDimmer = t.gameObject;
        }

        if (dimmerImage == null &&
            tutorialDimmer != null)
        {
            dimmerImage =
                tutorialDimmer.GetComponent<Image>();
        }

        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<PlayerController>();
        }

        if (puzzlePieceRenderer == null &&
            firstPuzzlePiece != null)
        {
            puzzlePieceRenderer =
                firstPuzzlePiece.GetComponent<
                    SpriteRenderer>();
        }

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            if (actionControlsGroup == null)
            {
                Transform t =
                    canvas.transform.Find(
                        "ActionControls");

                if (t != null)
                {
                    actionControlsGroup =
                        t.GetComponent<CanvasGroup>();

                    if (actionControlsGroup == null)
                    {
                        actionControlsGroup =
                            t.gameObject.AddComponent<
                                CanvasGroup>();
                    }
                }
            }

            if (inventoryControlsGroup == null)
            {
                Transform t =
                    canvas.transform.Find(
                        "InventoryPanel");

                if (t != null)
                {
                    inventoryControlsGroup =
                        t.GetComponent<CanvasGroup>();

                    if (inventoryControlsGroup == null)
                    {
                        inventoryControlsGroup =
                            t.gameObject.AddComponent<
                                CanvasGroup>();
                    }
                }
            }
        }
    }

    // ============================================================
    // LOCALIZATION
    // ============================================================

    private void OnLocaleChanged(Locale locale)
    {
        LoadLocalization();
    }

    private void LoadLocalization()
    {
        localizationVersion++;

        if (localizationRoutine != null)
            StopCoroutine(localizationRoutine);

        localizationRoutine = StartCoroutine(
            LoadLocalizationRoutine(localizationVersion));
    }

    private IEnumerator LoadLocalizationRoutine(int version)
    {
        string[] keys =
        {
            page49Key,
            page50Key,
            page51Key,
            page52Key,
            page53Key,
            page54Key
        };

        string[] fallback =
        {
            page49Fallback,
            page50Fallback,
            page51Fallback,
            page52Fallback,
            page53Fallback,
            page54Fallback
        };

        string[] results =
            (string[])fallback.Clone();

        for (int i = 0; i < keys.Length; i++)
        {
            var request =
                LocalizationSettings.StringDatabase
                    .GetLocalizedStringAsync(
                        localizationTable,
                        keys[i]);

            yield return request;

            if (version != localizationVersion)
                yield break;

            if (request.Status ==
                AsyncOperationStatus.Succeeded &&
                !string.IsNullOrWhiteSpace(request.Result))
            {
                results[i] = request.Result;
            }
        }

        localizedLines = results;

        localizationRoutine = null;

        if (phase == FinalePhase.Conversation)
            ShowPage();
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -=
            OnLocaleChanged;

        if (goldNextButton != null)
        {
            goldNextButton.onClick.RemoveListener(
                OnGoldNextClicked);
        }

        localizationVersion++;

        if (localizationRoutine != null)
            StopCoroutine(localizationRoutine);

        if (puzzleWaitRoutine != null)
            StopCoroutine(puzzleWaitRoutine);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        localizationRoutine = null;
        puzzleWaitRoutine = null;
        fadeRoutine = null;

        if (phase == FinalePhase.Conversation ||
            phase == FinalePhase.Fading)
        {
            HideKeeper();
            RestoreDimmer();
            RestoreKeeperBaseline();
            ResumeGameplay();
        }
    }

    private void OnValidate()
    {
        waitAfterPuzzleDisappears =
            Mathf.Max(
                0f,
                waitAfterPuzzleDisappears);

        finalFadeDuration =
            Mathf.Max(
                0f,
                finalFadeDuration);
    }
}
