using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI (Pause Button)")]
    [SerializeField] private Button pauseButton;             // BtnPause
    [SerializeField] private Image pauseButtonImage;          // Image внутри BtnPause
    [SerializeField] private Sprite pauseSprite;              // иконка "Pause"
    [SerializeField] private Sprite playSprite;               // иконка "Play"

    [Header("Level 1 Introduction (Optional)")]
    [SerializeField] private Button introPauseButton;
    [SerializeField] private TutorialKeeperFade keeperFade;
    [SerializeField] private CanvasGroup mainPauseButtonGroup;
    [SerializeField] private Canvas mainPauseButtonCanvas;
    [SerializeField] private PrisonDarknessMask prisonDarknessMask;
    [SerializeField] private LevelCompleteUI levelCompletion;

    [Header("Pause Menu (CanvasGroup on PauseMenu)")]
    [SerializeField] private CanvasGroup pauseMenuGroup;      // CanvasGroup на объекте PauseMenu

    [Header("Menu Buttons")]
    [FormerlySerializedAs("btnResume")]
    [SerializeField] private Button btnCheckpoint;
    [SerializeField] private CanvasGroup checkpointGroup;
    [SerializeField] private RectTransform menuFrame;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnMainMenu;

    [Header("Audio")]
    [Tooltip("AudioSource с музыкой игры (GameMusic). TimeScale на звук не влияет.")]
    [SerializeField] private AudioSource gameMusic;

    [Header("Main Menu Scene Name")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI Click SFX")]
    [SerializeField] private UIClickSfx uiClickSfx;           // висит на UIAudio

    [Header("Fallback delay if click length unknown")]
    [SerializeField, Range(0.02f, 0.5f)]
    private float sceneLoadDelay = 0.15f;

    [Header("Extra delay added on top of click length (safety)")]
    [SerializeField, Range(0.00f, 0.15f)]
    private float clickExtraDelay = 0.03f;

    private bool isPaused;
    private bool isLoadingScene;
    private float timeScaleBeforePause;
    private bool pausedGameMusic;
    private Vector2 menuViewportSize;

    public static bool IsMenuPaused { get; private set; }
    internal static float TutorialDeltaTime => IsMenuPaused ? 0f : Time.unscaledDeltaTime;

    internal static IEnumerator WaitForTutorialSeconds(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration || IsMenuPaused)
        {
            yield return null;
            elapsed += TutorialDeltaTime;
        }
    }

    private void Awake()
    {
        Debug.Log("[PauseManager] Awake on: " + gameObject.name);

        if (pauseButton == null)
            pauseButton = GetComponent<Button>();

        // Unity 6: ищем даже на выключенных объектах
        if (uiClickSfx == null)
            uiClickSfx = Object.FindFirstObjectByType<UIClickSfx>(FindObjectsInactive.Include);

        // На старте меню скрыто (PauseMenu должен быть активен, чтобы CanvasGroup работал)
        ForceMenuObjectActive();
        SetMenuVisible(false);

        isPaused = false;
        isLoadingScene = false;

        // ===== Bind buttons =====
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(() =>
            {
                PlayClickSafe();
                TogglePause();
            });
        }
        else
        {
            Debug.LogWarning("[PauseManager] pauseButton = NULL (не назначена и нет Button на объекте)");
        }

        SetCheckpointAvailable(false);

        if (introPauseButton != null)
        {
            introPauseButton.onClick.AddListener(() =>
            {
                PlayClickSafe();
                TogglePause();
            });
        }

        if (btnRestart != null)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(() =>
            {
                if (isLoadingScene) return;
                isLoadingScene = true;

                PlayClickSafe();
                StartCoroutine(RestartAfterClick());
            });
        }

        if (btnMainMenu != null)
        {
            btnMainMenu.onClick.RemoveAllListeners();
            btnMainMenu.onClick.AddListener(() =>
            {
                if (isLoadingScene) return;
                isLoadingScene = true;

                PlayClickSafe();
                StartCoroutine(MainMenuAfterClick());
            });
        }
    }

    private void Start() => RefreshPauseButtons();

    private void LateUpdate()
    {
        RefreshPauseButtons();
        if (!isPaused || menuFrame == null)
            return;

        Vector2 viewportSize = ((RectTransform)menuFrame.parent).rect.size;
        if (viewportSize == menuViewportSize)
            return;

        menuViewportSize = viewportSize;
        Vector2 frameSize = menuFrame.rect.size;
        float scale = Mathf.Min(1f, 0.92f * viewportSize.x / frameSize.x,
            0.92f * viewportSize.y / frameSize.y);
        menuFrame.localScale = Vector3.one * scale;
    }

    public void SetCheckpointAvailable(bool available)
    {
        if (btnCheckpoint != null)
            btnCheckpoint.interactable = available;

        if (checkpointGroup != null)
        {
            checkpointGroup.alpha = available ? 1f : 0.5f;
            checkpointGroup.blocksRaycasts = available;
        }
    }

    private void RefreshPauseButtons()
    {
        if (isPaused || introPauseButton == null || mainPauseButtonGroup == null || prisonDarknessMask == null)
            return;

        bool introFinished = keeperFade != null
            ? keeperFade.HasIntroPauseFadedOut
            : prisonDarknessMask.IsFullLevelRevealComplete;
        bool useMainButton = introFinished || (levelCompletion != null && levelCompletion.IsShown);
        introPauseButton.gameObject.SetActive(!useMainButton);
        mainPauseButtonGroup.alpha = useMainButton ? 1f : 0f;
        mainPauseButtonGroup.interactable = useMainButton;
        mainPauseButtonGroup.blocksRaycasts = useMainButton;
    }

    private void OnDisable()
    {
        // На всякий случай не оставляем игру в паузе
        if (!isPaused)
            return;

        if (isLoadingScene)
            IsMenuPaused = false;
        else
            Resume();
    }

    public void TogglePause()
    {
        if (isLoadingScene)
            return;

        Debug.Log("[PauseManager] TogglePause called. isPaused=" + isPaused);

        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        timeScaleBeforePause = Time.timeScale;
        isPaused = true;
        IsMenuPaused = true;

        Time.timeScale = 0f;

        ForceMenuObjectActive();
        SetMenuVisible(true);

        SetPauseIcon(playSprite);
        if (mainPauseButtonCanvas != null && pauseMenuGroup != null &&
            pauseMenuGroup.TryGetComponent(out Canvas menuCanvas))
        {
            mainPauseButtonCanvas.overrideSorting = true;
            mainPauseButtonCanvas.sortingLayerID = menuCanvas.sortingLayerID;
            mainPauseButtonCanvas.sortingOrder = menuCanvas.sortingOrder + 1;
        }

        pausedGameMusic = gameMusic != null && gameMusic.isPlaying;
        if (pausedGameMusic)
            gameMusic.Pause();

        Debug.Log("[PauseManager] PAUSED. timeScale=" + Time.timeScale);
    }

    private void Resume()
    {
        if (!isPaused || isLoadingScene)
            return;

        isPaused = false;
        IsMenuPaused = false;

        Time.timeScale = timeScaleBeforePause;
        SetMenuVisible(false);

        SetPauseIcon(pauseSprite);
        if (mainPauseButtonCanvas != null)
            mainPauseButtonCanvas.overrideSorting = false;

        if (pausedGameMusic && gameMusic != null)
            gameMusic.UnPause();
        pausedGameMusic = false;

        Debug.Log("[PauseManager] RESUMED. timeScale=" + Time.timeScale);
    }

    private void SetPauseIcon(Sprite sprite)
    {
        if (sprite == null)
            return;

        if (pauseButtonImage != null)
            pauseButtonImage.sprite = sprite;

        if (introPauseButton != null && introPauseButton.image != null)
            introPauseButton.image.sprite = sprite;
    }

    private IEnumerator RestartAfterClick()
    {
        Debug.Log("[PauseManager] RestartAfterClick");

        // снять паузу, чтобы новая сцена не стартанула с timeScale=0
        yield return WaitClickRealtime();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator MainMenuAfterClick()
    {
        Debug.Log("[PauseManager] MainMenuAfterClick -> " + mainMenuSceneName);

        yield return WaitClickRealtime();

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator WaitClickRealtime()
    {
        // Если у нас есть длина клипа — ждём её (почти всю) + небольшой запас.
        // Если нет — ждём fallback sceneLoadDelay.
        float wait = sceneLoadDelay;

        if (uiClickSfx != null)
        {
            float len = uiClickSfx.ClickLength;
            if (len > 0.01f)
                wait = Mathf.Max(sceneLoadDelay, len + clickExtraDelay);
        }

        yield return new WaitForSecondsRealtime(wait);
    }

    private void SetMenuVisible(bool visible)
    {
        if (pauseMenuGroup == null)
        {
            Debug.LogWarning("[PauseManager] pauseMenuGroup = NULL (не назначен CanvasGroup)");
            return;
        }

        pauseMenuGroup.alpha = visible ? 1f : 0f;
        pauseMenuGroup.interactable = visible;
        pauseMenuGroup.blocksRaycasts = visible;
    }

    private void ForceMenuObjectActive()
    {
        if (pauseMenuGroup == null) return;

        if (!pauseMenuGroup.gameObject.activeSelf)
            pauseMenuGroup.gameObject.SetActive(true);
    }

    private void PlayClickSafe()
    {
        if (uiClickSfx != null)
            uiClickSfx.PlayClick();
    }
}
