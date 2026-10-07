using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TutorialTextController : MonoBehaviour
{
    [Header("TUTORIAL TEXT")]
    [SerializeField] private TMP_Text tutorialText;

    [Header("TUTORIAL PAGES")]
    [TextArea(3, 8)]
    [SerializeField] private string[] pages;

    [Header("LOCALIZATION")]
    [Tooltip("Включает получение текста из Unity Localization.")]
    [SerializeField] private bool useLocalization = true;

    [Tooltip("Название String Table Collection. У нас она называется Tutorial.")]
    [SerializeField] private string localizationTableName = "Tutorial";

    [Tooltip("Префикс ключей. Получатся tutorial_00, tutorial_01, tutorial_02 и т.д.")]
    [SerializeField] private string localizationKeyPrefix = "tutorial_";

    [Header("NAVIGATION BUTTONS")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("START SETTINGS")]
    [SerializeField, Min(0)] private int startPage = 0;

    [Header("NAME INPUT PAGE")]
    [Tooltip("Страница, на которой Хранитель спрашивает имя игрока. Отсчёт начинается с 0.")]
    [SerializeField, Min(0)] private int nameInputPage = 1;

    [Tooltip("Страница после ввода имени. Например: {name}, рад знакомству!")]
    [SerializeField, Min(0)] private int nameConfirmationPage = 2;

    [Header("NAME INPUT CONTROLLER")]
    [SerializeField] private PlayerNameInputController playerNameInputController;

    // =================================================
    // PUZZLE KEEPER VISUAL
    // =================================================

    [Header("PUZZLE KEEPER VISUAL")]
    [Tooltip("UI Image объекта PuzzleKeeper.")]
    [SerializeField] private Image puzzleKeeperImage;

    [Tooltip("Обычная поза Хранителя, которая используется во время знакомства.")]
    [SerializeField] private Sprite dialogueKeeperSprite;

    [Tooltip("Новая поза Хранителя, которая используется во время обучения.")]
    [SerializeField] private Sprite trainingKeeperSprite;

    [Tooltip("Страница, начиная с которой Хранитель переходит в обучающую позу.")]
    [SerializeField, Min(0)] private int trainingStartPage = 13;

    [Header("KEEPER PUZZLE ICON")]
    [Tooltip("RectTransform голубого пазла на груди Хранителя (KeeperPuzzleIcon).")]
    [SerializeField] private RectTransform keeperPuzzleIcon;

    [Tooltip("Позиция голубого пазла на обычной позе Хранителя.")]
    [SerializeField] private Vector2 dialoguePuzzleIconPosition =
        new Vector2(-27.6f, -75.8f);

    [Tooltip("Позиция голубого пазла на обучающей позе Хранителя.")]
    [SerializeField] private Vector2 trainingPuzzleIconPosition =
        new Vector2(-22f, -62f);

    [Tooltip("Размер голубого пазла на обычной позе.")]
    [SerializeField] private Vector2 dialoguePuzzleIconSize =
        new Vector2(27f, 27f);

    [Tooltip("Размер голубого пазла на обучающей позе.")]
    [SerializeField] private Vector2 trainingPuzzleIconSize =
        new Vector2(27f, 27f);

    [Tooltip("Поворот голубого пазла на обычной позе.")]
    [SerializeField] private float dialoguePuzzleIconRotation = -20f;

    [Tooltip("Поворот голубого пазла на обучающей позе.")]
    [SerializeField] private float trainingPuzzleIconRotation = -20f;

    // =================================================
    // INTERACTIVE MOVEMENT TUTORIAL
    // =================================================

    [Header("MOVEMENT TUTORIAL")]

    [Tooltip(
        "Страница, на которой Хранитель впервые ПОКАЗЫВАЕТ " +
        "стрелки движения. Для нас tutorial_13."
    )]
    [SerializeField, Min(0)]
    private int movementIntroPage = 13;

    [Tooltip(
        "Страница практики. Только на ней LEFT и RIGHT " +
        "становятся активными. Для нас tutorial_14."
    )]
    [SerializeField, Min(0)]
    private int movementPracticePage = 14;

    [Tooltip(
        "Страница успеха после выполнения движения. " +
        "Для нас tutorial_15."
    )]
    [SerializeField, Min(0)]
    private int movementSuccessPage = 15;

    // =================================================
    // TUTORIAL CONTROL VISUALS
    // =================================================

    [Header("TUTORIAL CONTROL VISUALS")]

    [Tooltip("CanvasGroup кнопки движения ВЛЕВО.")]
    [SerializeField]
    private CanvasGroup leftControlGroup;

    [Tooltip("CanvasGroup кнопки движения ВПРАВО.")]
    [SerializeField]
    private CanvasGroup rightControlGroup;

    [Tooltip(
        "Остальные элементы управления, которые пока должны " +
        "оставаться приглушёнными и заблокированными. " +
        "Сюда добавляем UP, DOWN, прыжок, удар ногой, руку и т.д."
    )]
    [SerializeField]
    private CanvasGroup[] lockedControlGroups;

    [Tooltip("Прозрачность приглушённой кнопки.")]
    [SerializeField, Range(0f, 1f)]
    private float lockedControlAlpha = 0.30f;

    [Tooltip("Прозрачность показанной/доступной кнопки.")]
    [SerializeField, Range(0f, 1f)]
    private float unlockedControlAlpha = 1f;

    // =================================================
    // PRIVATE
    // =================================================

    private int currentPage = 0;

    private bool nameConfirmed = false;
    private string playerName = "";

    // -------------------------------------------------
    // MOVEMENT TUTORIAL STATE
    // -------------------------------------------------

    /*
     * LEFT и RIGHT запоминаются отдельно.
     *
     * Порядок не имеет значения:
     *
     * LEFT -> RIGHT
     *
     * или
     *
     * RIGHT -> LEFT.
     */
    private bool movementLeftUsed = false;
    private bool movementRightUsed = false;
    private bool movementTutorialCompleted = false;

    // -------------------------------------------------
    // LOCALIZATION CACHE
    // -------------------------------------------------

    private string[] localizedPages;

    private Coroutine localizationPreloadCoroutine;

    private int localizationLoadVersion = 0;

    private bool localizationReady = false;

    private bool tutorialStarted = false;

    // =================================================
    // AWAKE
    // =================================================

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
    }

    // =================================================
    // ENABLE / DISABLE
    // =================================================

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

    // =================================================
    // START
    // =================================================

    private void Start()
    {
        tutorialStarted = true;

        if (pages == null || pages.Length == 0)
        {
            if (tutorialText != null)
            {
                tutorialText.text = "";
                tutorialText.gameObject.SetActive(false);
            }

            if (playerNameInputController != null)
            {
                playerNameInputController.HideNameInput();
            }

            UpdateKeeperVisual();
            UpdateTutorialControls();
            UpdateButtons();
            return;
        }

        currentPage = Mathf.Clamp(
            startPage,
            0,
            pages.Length - 1
        );

        UpdateKeeperVisual();
        UpdateTutorialControls();

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

    // =================================================
    // LOCALIZATION PRELOAD
    // =================================================

    private void BeginLocalizationPreload()
    {
        if (pages == null || pages.Length == 0)
            return;

        localizationLoadVersion++;

        int loadVersion =
            localizationLoadVersion;

        if (localizationPreloadCoroutine != null)
        {
            StopCoroutine(localizationPreloadCoroutine);
            localizationPreloadCoroutine = null;
        }

        localizationReady = false;

        localizedPages =
            new string[pages.Length];

        if (tutorialText != null)
        {
            tutorialText.text = "";
            tutorialText.gameObject.SetActive(false);
        }

        if (playerNameInputController != null)
        {
            playerNameInputController.HideNameInput();
        }

        UpdateButtons();

        localizationPreloadCoroutine =
            StartCoroutine(
                PreloadLocalizedPagesCoroutine(
                    loadVersion
                )
            );
    }

    private IEnumerator PreloadLocalizedPagesCoroutine(
        int loadVersion
    )
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (loadVersion != localizationLoadVersion)
            {
                yield break;
            }

            string key =
                GetLocalizationKey(i);

            var operation =
                LocalizationSettings
                    .StringDatabase
                    .GetLocalizedStringAsync(
                        localizationTableName,
                        key
                    );

            yield return operation;

            if (loadVersion != localizationLoadVersion)
            {
                yield break;
            }

            if (operation.Status ==
                AsyncOperationStatus.Succeeded)
            {
                string localizedText =
                    operation.Result;

                if (!string.IsNullOrWhiteSpace(
                        localizedText
                    ))
                {
                    localizedPages[i] =
                        localizedText;

                    continue;
                }
            }

            localizedPages[i] =
                GetFallbackPageText(i);

            Debug.LogWarning(
                "TutorialTextController: не удалось получить локализованный текст. " +
                "Table = " + localizationTableName +
                ", Key = " + key +
                ". Использован текст из Pages."
            );
        }

        if (loadVersion != localizationLoadVersion)
        {
            yield break;
        }

        localizationReady = true;
        localizationPreloadCoroutine = null;

        ShowCurrentPage();
    }

    // =================================================
    // NAVIGATION
    // =================================================

    public void NextPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (useLocalization &&
            !localizationReady)
        {
            return;
        }

        if (!nameConfirmed &&
            currentPage == nameInputPage)
        {
            return;
        }

        /*
         * На tutorial_14 стрелка Next скрыта.
         * Игрок обязан сначала нажать LEFT и RIGHT.
         */
        if (currentPage == movementPracticePage &&
            !movementTutorialCompleted)
        {
            return;
        }

        int targetPage =
            currentPage + 1;

        if (nameConfirmed &&
            targetPage == nameInputPage)
        {
            targetPage =
                nameConfirmationPage;
        }

        if (targetPage < 0 ||
            targetPage >= pages.Length)
        {
            return;
        }

        currentPage =
            targetPage;

        ShowCurrentPage();
    }

    public void PreviousPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (useLocalization &&
            !localizationReady)
        {
            return;
        }

        if (!nameConfirmed &&
            currentPage == nameInputPage)
        {
            return;
        }

        /*
         * Во время обязательной практики tutorial_14
         * назад тоже не уходим.
         */
        if (currentPage == movementPracticePage &&
            !movementTutorialCompleted)
        {
            return;
        }

        int targetPage =
            currentPage - 1;

        if (nameConfirmed &&
            targetPage == nameInputPage)
        {
            targetPage =
                nameInputPage - 1;
        }

        if (targetPage < 0 ||
            targetPage >= pages.Length)
        {
            return;
        }

        currentPage =
            targetPage;

        ShowCurrentPage();
    }

    // =================================================
    // PAGE DISPLAY
    // =================================================

    private void ShowCurrentPage()
    {
        if (pages == null ||
            pages.Length == 0)
        {
            return;
        }

        if (useLocalization &&
            !localizationReady)
        {
            return;
        }

        currentPage =
            Mathf.Clamp(
                currentPage,
                0,
                pages.Length - 1
            );

        bool shouldShowNameInput =
            !nameConfirmed &&
            currentPage == nameInputPage;

        string pageText =
            GetPageText(currentPage);

        pageText =
            ApplyPlayerName(pageText);

        // -------------------------------------------------
        // ТЕКСТ ХРАНИТЕЛЯ
        // -------------------------------------------------

        if (tutorialText != null)
        {
            tutorialText.text =
                pageText;

            tutorialText.gameObject.SetActive(true);
        }

        // -------------------------------------------------
        // ПОЛЕ ВВОДА ИМЕНИ
        // -------------------------------------------------

        if (playerNameInputController != null)
        {
            if (shouldShowNameInput)
            {
                playerNameInputController.ShowNameInput();
            }
            else
            {
                playerNameInputController.HideNameInput();
            }
        }

        // -------------------------------------------------
        // ВНЕШНИЙ ВИД ХРАНИТЕЛЯ
        // -------------------------------------------------

        UpdateKeeperVisual();

        // -------------------------------------------------
        // СОСТОЯНИЕ ИГРОВЫХ КНОПОК
        // -------------------------------------------------

        UpdateTutorialControls();

        // -------------------------------------------------
        // СТРЕЛКИ ДИАЛОГА
        // -------------------------------------------------

        UpdateButtons();
    }

    // =================================================
    // MOVEMENT TUTORIAL INPUT
    // =================================================

    public void ReportHorizontalTutorialInput(
        float horizontalInput
    )
    {
        /*
         * КЛЮЧЕВОЙ МОМЕНТ:
         *
         * Нажатия LEFT / RIGHT засчитываются
         * ТОЛЬКО на tutorial_14.
         *
         * На tutorial_13 кнопки уже видны и яркие,
         * но нажатия там не считаются.
         */
        if (currentPage != movementPracticePage)
        {
            return;
        }

        if (movementTutorialCompleted)
        {
            return;
        }

        // -------------------------------------------------
        // LEFT
        // -------------------------------------------------

        if (horizontalInput < -0.001f)
        {
            movementLeftUsed =
                true;
        }

        // -------------------------------------------------
        // RIGHT
        // -------------------------------------------------

        if (horizontalInput > 0.001f)
        {
            movementRightUsed =
                true;
        }

        if (movementLeftUsed &&
            movementRightUsed)
        {
            CompleteMovementTutorial();
        }
    }

    private void CompleteMovementTutorial()
    {
        if (movementTutorialCompleted)
        {
            return;
        }

        movementTutorialCompleted =
            true;

        /*
         * После LEFT + RIGHT Хранитель
         * автоматически показывает tutorial_15.
         */
        if (pages != null &&
            movementSuccessPage >= 0 &&
            movementSuccessPage < pages.Length)
        {
            currentPage =
                movementSuccessPage;

            ShowCurrentPage();
        }
        else
        {
            UpdateTutorialControls();
            UpdateButtons();
        }
    }

    // =================================================
    // TUTORIAL CONTROL VISUALS
    // =================================================

    private void UpdateTutorialControls()
    {
        /*
         * ЛОГИКА LEFT / RIGHT:
         *
         * ДО tutorial_13:
         *   приглушены + заблокированы.
         *
         * tutorial_13:
         *   яркие, но ЗАБЛОКИРОВАНЫ.
         *   Хранитель только показывает их игроку.
         *
         * tutorial_14:
         *   яркие + АКТИВНЫЕ.
         *   Игрок должен нажать обе.
         *
         * tutorial_15 и дальше:
         *   остаются яркими + активными.
         *
         * Остальные кнопки пока всегда:
         *   приглушены + заблокированы.
         */

        bool horizontalControlsVisible =
            currentPage >= movementIntroPage;

        bool horizontalControlsInteractable =
            currentPage >= movementPracticePage;

        SetControlGroupState(
            leftControlGroup,
            horizontalControlsVisible,
            horizontalControlsInteractable
        );

        SetControlGroupState(
            rightControlGroup,
            horizontalControlsVisible,
            horizontalControlsInteractable
        );

        if (lockedControlGroups != null)
        {
            for (int i = 0;
                 i < lockedControlGroups.Length;
                 i++)
            {
                SetControlGroupState(
                    lockedControlGroups[i],
                    false,
                    false
                );
            }
        }
    }

    private void SetControlGroupState(
        CanvasGroup group,
        bool visuallyUnlocked,
        bool interactable
    )
    {
        if (group == null)
        {
            return;
        }

        /*
         * Внешний вид и возможность нажатия
         * теперь НЕ зависят друг от друга.
         *
         * Именно поэтому на tutorial_13
         * кнопка может быть яркой,
         * но при этом не работать.
         */
        group.alpha =
            visuallyUnlocked
                ? unlockedControlAlpha
                : lockedControlAlpha;

        group.interactable =
            interactable;

        group.blocksRaycasts =
            interactable;
    }

    // =================================================
    // PAGE TEXT
    // =================================================

    private string GetPageText(
        int pageIndex
    )
    {
        if (useLocalization &&
            localizationReady &&
            localizedPages != null &&
            pageIndex >= 0 &&
            pageIndex < localizedPages.Length &&
            !string.IsNullOrWhiteSpace(
                localizedPages[pageIndex]
            ))
        {
            return localizedPages[pageIndex];
        }

        return GetFallbackPageText(
            pageIndex
        );
    }

    private string GetLocalizationKey(
        int pageIndex
    )
    {
        return localizationKeyPrefix +
               pageIndex.ToString("00");
    }

    private string GetFallbackPageText(
        int pageIndex
    )
    {
        if (pages == null ||
            pageIndex < 0 ||
            pageIndex >= pages.Length)
        {
            return "";
        }

        return pages[pageIndex];
    }

    private string ApplyPlayerName(
        string text
    )
    {
        if (string.IsNullOrEmpty(text))
            return "";

        if (nameConfirmed &&
            !string.IsNullOrWhiteSpace(
                playerName
            ))
        {
            text =
                text.Replace(
                    "{name}",
                    playerName
                );
        }

        return text;
    }

    // =================================================
    // PUZZLE KEEPER VISUAL
    // =================================================

    private void UpdateKeeperVisual()
    {
        bool trainingPose =
            currentPage >= trainingStartPage;

        if (puzzleKeeperImage != null)
        {
            Sprite targetSprite =
                trainingPose
                    ? trainingKeeperSprite
                    : dialogueKeeperSprite;

            if (targetSprite != null)
            {
                puzzleKeeperImage.sprite =
                    targetSprite;
            }
        }

        if (keeperPuzzleIcon != null)
        {
            Vector2 targetPosition =
                trainingPose
                    ? trainingPuzzleIconPosition
                    : dialoguePuzzleIconPosition;

            Vector2 targetSize =
                trainingPose
                    ? trainingPuzzleIconSize
                    : dialoguePuzzleIconSize;

            float targetRotation =
                trainingPose
                    ? trainingPuzzleIconRotation
                    : dialoguePuzzleIconRotation;

            keeperPuzzleIcon.anchoredPosition =
                targetPosition;

            keeperPuzzleIcon.sizeDelta =
                targetSize;

            Vector3 euler =
                keeperPuzzleIcon.localEulerAngles;

            euler.z =
                targetRotation;

            keeperPuzzleIcon.localEulerAngles =
                euler;
        }
    }

    // =================================================
    // LOCALE CHANGE
    // =================================================

    private void OnSelectedLocaleChanged(
        Locale newLocale
    )
    {
        if (!tutorialStarted)
            return;

        if (!useLocalization)
            return;

        if (pages == null ||
            pages.Length == 0)
        {
            return;
        }

        BeginLocalizationPreload();
    }

    // =================================================
    // BUTTON VISIBILITY
    // =================================================

    private void UpdateButtons()
    {
        if (pages == null ||
            pages.Length == 0)
        {
            SetButtonVisible(
                nextButton,
                false
            );

            SetButtonVisible(
                previousButton,
                false
            );

            return;
        }

        if (useLocalization &&
            !localizationReady)
        {
            SetButtonVisible(
                nextButton,
                false
            );

            SetButtonVisible(
                previousButton,
                false
            );

            return;
        }

        if (!nameConfirmed &&
            currentPage == nameInputPage)
        {
            SetButtonVisible(
                nextButton,
                false
            );

            SetButtonVisible(
                previousButton,
                false
            );

            return;
        }

        /*
         * На tutorial_14 обе стрелки диалога
         * скрываются.
         *
         * Игрок должен выполнить действие,
         * а не листать текст.
         */
        if (currentPage == movementPracticePage &&
            !movementTutorialCompleted)
        {
            SetButtonVisible(
                nextButton,
                false
            );

            SetButtonVisible(
                previousButton,
                false
            );

            return;
        }

        int previousPage =
            GetPreviousPageIndex();

        int nextPage =
            GetNextPageIndex();

        bool hasPrevious =
            previousPage >= 0 &&
            previousPage < pages.Length;

        bool hasNext =
            nextPage >= 0 &&
            nextPage < pages.Length;

        SetButtonVisible(
            previousButton,
            hasPrevious
        );

        SetButtonVisible(
            nextButton,
            hasNext
        );
    }

    private int GetNextPageIndex()
    {
        int targetPage =
            currentPage + 1;

        if (nameConfirmed &&
            targetPage == nameInputPage)
        {
            targetPage =
                nameConfirmationPage;
        }

        return targetPage;
    }

    private int GetPreviousPageIndex()
    {
        int targetPage =
            currentPage - 1;

        if (nameConfirmed &&
            targetPage == nameInputPage)
        {
            targetPage =
                nameInputPage - 1;
        }

        return targetPage;
    }

    private void SetButtonVisible(
        Button button,
        bool visible
    )
    {
        if (button != null)
        {
            button.gameObject.SetActive(
                visible
            );
        }
    }

    // =================================================
    // PLAYER NAME
    // =================================================

    public void ConfirmPlayerName(
        string confirmedName
    )
    {
        if (string.IsNullOrWhiteSpace(
                confirmedName
            ))
        {
            return;
        }

        playerName =
            confirmedName.Trim();

        nameConfirmed = true;

        if (pages != null &&
            pages.Length > 0 &&
            nameConfirmationPage >= 0 &&
            nameConfirmationPage <
            pages.Length)
        {
            currentPage =
                nameConfirmationPage;
        }

        ShowCurrentPage();
    }

    // =================================================
    // PUBLIC CONTROLS
    // =================================================

    public void ResetTutorial()
    {
        if (pages == null ||
            pages.Length == 0)
        {
            return;
        }

        currentPage =
            Mathf.Clamp(
                startPage,
                0,
                pages.Length - 1
            );

        nameConfirmed = false;
        playerName = "";

        movementLeftUsed = false;
        movementRightUsed = false;
        movementTutorialCompleted = false;

        UpdateKeeperVisual();
        UpdateTutorialControls();

        if (useLocalization &&
            !localizationReady)
        {
            return;
        }

        ShowCurrentPage();
    }

    public void GoToPage(
        int pageIndex
    )
    {
        if (pages == null ||
            pages.Length == 0)
        {
            return;
        }

        if (useLocalization &&
            !localizationReady)
        {
            return;
        }

        int targetPage =
            Mathf.Clamp(
                pageIndex,
                0,
                pages.Length - 1
            );

        if (nameConfirmed &&
            targetPage == nameInputPage)
        {
            targetPage =
                nameConfirmationPage;
        }

        currentPage =
            targetPage;

        ShowCurrentPage();
    }

    public int GetCurrentPage()
    {
        return currentPage;
    }

    public int GetPageCount()
    {
        return pages != null
            ? pages.Length
            : 0;
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
        return !useLocalization ||
               localizationReady;
    }

    public bool IsMovementTutorialCompleted()
    {
        return movementTutorialCompleted;
    }

    // =================================================
    // DESTROY
    // =================================================

    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(
                NextPage
            );
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(
                PreviousPage
            );
        }
    }

#if UNITY_EDITOR

    // =================================================
    // VALIDATE
    // =================================================

    private void OnValidate()
    {
        if (startPage < 0)
            startPage = 0;

        if (nameInputPage < 0)
            nameInputPage = 0;

        if (nameConfirmationPage < 0)
            nameConfirmationPage = 0;

        if (trainingStartPage < 0)
            trainingStartPage = 0;

        if (movementIntroPage < 0)
            movementIntroPage = 0;

        if (movementPracticePage < 0)
            movementPracticePage = 0;

        if (movementSuccessPage < 0)
            movementSuccessPage = 0;

        lockedControlAlpha =
            Mathf.Clamp01(
                lockedControlAlpha
            );

        unlockedControlAlpha =
            Mathf.Clamp01(
                unlockedControlAlpha
            );

        if (pages != null &&
            pages.Length > 0)
        {
            startPage =
                Mathf.Clamp(
                    startPage,
                    0,
                    pages.Length - 1
                );

            nameInputPage =
                Mathf.Clamp(
                    nameInputPage,
                    0,
                    pages.Length - 1
                );

            nameConfirmationPage =
                Mathf.Clamp(
                    nameConfirmationPage,
                    0,
                    pages.Length - 1
                );

            trainingStartPage =
                Mathf.Clamp(
                    trainingStartPage,
                    0,
                    pages.Length - 1
                );

            movementIntroPage =
                Mathf.Clamp(
                    movementIntroPage,
                    0,
                    pages.Length - 1
                );

            movementPracticePage =
                Mathf.Clamp(
                    movementPracticePage,
                    0,
                    pages.Length - 1
                );

            movementSuccessPage =
                Mathf.Clamp(
                    movementSuccessPage,
                    0,
                    pages.Length - 1
                );
        }
    }

#endif
}