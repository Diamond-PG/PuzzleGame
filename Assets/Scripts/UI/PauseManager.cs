using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI (Pause Button)")]
    [SerializeField] private Button pauseButton;             // BtnPause
    [SerializeField] private Image pauseButtonImage;          // Image внутри BtnPause
    [SerializeField] private Sprite pauseSprite;              // иконка "Pause"
    [SerializeField] private Sprite playSprite;               // иконка "Play"

    [Header("Pause Menu (CanvasGroup on PauseMenu)")]
    [SerializeField] private CanvasGroup pauseMenuGroup;      // CanvasGroup на объекте PauseMenu

    [Header("Menu Buttons")]
    [SerializeField] private Button btnResume;
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
        Time.timeScale = 1f;

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

        if (btnResume != null)
        {
            btnResume.onClick.RemoveAllListeners();
            btnResume.onClick.AddListener(() =>
            {
                PlayClickSafe();
                Resume();
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

    private void OnDisable()
    {
        // На всякий случай не оставляем игру в паузе
        Time.timeScale = 1f;
        if (gameMusic != null) gameMusic.UnPause();
    }

    public void TogglePause()
    {
        Debug.Log("[PauseManager] TogglePause called. isPaused=" + isPaused);

        if (isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        isPaused = true;

        Time.timeScale = 0f;

        ForceMenuObjectActive();
        SetMenuVisible(true);

        if (pauseButtonImage != null && playSprite != null)
            pauseButtonImage.sprite = playSprite;

        if (gameMusic != null && gameMusic.isPlaying)
            gameMusic.Pause();

        Debug.Log("[PauseManager] PAUSED. timeScale=" + Time.timeScale);
    }

    private void Resume()
    {
        isPaused = false;

        Time.timeScale = 1f;
        SetMenuVisible(false);

        if (pauseButtonImage != null && pauseSprite != null)
            pauseButtonImage.sprite = pauseSprite;

        if (gameMusic != null)
            gameMusic.UnPause();

        Debug.Log("[PauseManager] RESUMED. timeScale=" + Time.timeScale);
    }

    private IEnumerator RestartAfterClick()
    {
        Debug.Log("[PauseManager] RestartAfterClick");

        // снять паузу, чтобы новая сцена не стартанула с timeScale=0
        Time.timeScale = 1f;
        isPaused = false;

        SetMenuVisible(false);
        if (gameMusic != null) gameMusic.UnPause();

        yield return WaitClickRealtime();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator MainMenuAfterClick()
    {
        Debug.Log("[PauseManager] MainMenuAfterClick -> " + mainMenuSceneName);

        Time.timeScale = 1f;
        isPaused = false;

        SetMenuVisible(false);
        if (gameMusic != null) gameMusic.UnPause();

        yield return WaitClickRealtime();

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