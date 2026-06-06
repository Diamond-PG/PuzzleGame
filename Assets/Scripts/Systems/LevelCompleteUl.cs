using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Панель победы")]
    [SerializeField] private CanvasGroup levelCompletePanel;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Next Level")]
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private CanvasGroup nextLevelButtonCanvasGroup;
    [SerializeField] private float nextButtonDelay = 1f;
    [SerializeField] private float nextButtonFadeDuration = 0.3f;

    [Header("Падение кнопки Next Level")]
    [SerializeField] private float nextButtonStartY = 450f;
    [SerializeField] private float nextButtonEndY = 0f;
    [SerializeField] private float nextButtonDropDuration = 0.45f;

    [Header("Текст победы")]
    [SerializeField] private Animator winTextAnimator;
    [SerializeField] private string winTextAnimState = "WinTextPop";

    [Header("Звук победы")]
    [SerializeField] private AudioSource victoryAudioSource;

    [Header("Музыка уровня")]
    [SerializeField] private AudioSource gameMusicAudioSource;
    [SerializeField] private bool stopGameMusicOnVictory = true;

    [Header("Что скрыть на время победы")]
    [SerializeField] private GameObject[] hideOnVictory;

    [Header("Что отключить на время победы")]
    [SerializeField] private MonoBehaviour[] disableScriptsOnVictory;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public bool IsShown { get; private set; }

    private Coroutine panelRoutine;
    private Coroutine buttonRoutine;
    private RectTransform nextButtonRect;

    private void Awake()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.gameObject.SetActive(true);

        if (nextLevelButton != null)
        {
            nextButtonRect = nextLevelButton.GetComponent<RectTransform>();

            Button btn = nextLevelButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(LoadNextLevel);
                btn.onClick.AddListener(LoadNextLevel);
            }
        }

        HideInstant();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnlyOnce && IsShown) return;
        if (!other.CompareTag(playerTag)) return;

        ShowPanel();
    }

    public void ShowPanel()
    {
        if (levelCompletePanel == null)
        {
            Debug.LogWarning("LevelCompleteUI: Level Complete Panel не назначен!", this);
            return;
        }

        if (IsShown) return;

        IsShown = true;

        foreach (GameObject obj in hideOnVictory)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (MonoBehaviour script in disableScriptsOnVictory)
        {
            if (script != null)
                script.enabled = false;
        }

        if (stopGameMusicOnVictory && gameMusicAudioSource != null)
            gameMusicAudioSource.Stop();

        if (victoryAudioSource != null)
            victoryAudioSource.Play();

        if (panelRoutine != null) StopCoroutine(panelRoutine);
        if (buttonRoutine != null) StopCoroutine(buttonRoutine);

        levelCompletePanel.gameObject.SetActive(true);

        panelRoutine = StartCoroutine(FadeCanvasGroup(levelCompletePanel, 1f, fadeDuration, true));
        buttonRoutine = StartCoroutine(ShowNextButtonWithDelay());

        if (winTextAnimator != null && !string.IsNullOrEmpty(winTextAnimState))
            winTextAnimator.Play(winTextAnimState, 0, 0f);

        if (debugLogs)
            Debug.Log("[LEVEL COMPLETE] Victory panel shown.", this);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            SceneManager.LoadScene(currentIndex);
    }

    private void HideInstant()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.alpha = 0f;
            levelCompletePanel.interactable = false;
            levelCompletePanel.blocksRaycasts = false;
        }

        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);

        if (nextLevelButtonCanvasGroup != null)
        {
            nextLevelButtonCanvasGroup.alpha = 0f;
            nextLevelButtonCanvasGroup.interactable = false;
            nextLevelButtonCanvasGroup.blocksRaycasts = false;
        }

        if (nextButtonRect != null)
            nextButtonRect.anchoredPosition = new Vector2(nextButtonRect.anchoredPosition.x, nextButtonStartY);

        IsShown = false;
    }

    private IEnumerator ShowNextButtonWithDelay()
    {
        yield return WaitSeconds(nextButtonDelay);

        if (nextLevelButton == null)
            yield break;

        nextLevelButton.SetActive(true);

        if (nextLevelButtonCanvasGroup != null)
        {
            nextLevelButtonCanvasGroup.alpha = 0f;
            nextLevelButtonCanvasGroup.interactable = false;
            nextLevelButtonCanvasGroup.blocksRaycasts = false;
        }

        if (nextButtonRect != null)
        {
            Vector2 startPos = new Vector2(nextButtonRect.anchoredPosition.x, nextButtonStartY);
            Vector2 endPos = new Vector2(nextButtonRect.anchoredPosition.x, nextButtonEndY);

            nextButtonRect.anchoredPosition = startPos;

            float timer = 0f;

            while (timer < nextButtonDropDuration)
            {
                timer += Delta();
                float t = Mathf.Clamp01(timer / nextButtonDropDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                nextButtonRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                yield return null;
            }

            nextButtonRect.anchoredPosition = endPos;
        }

        if (nextLevelButtonCanvasGroup != null)
            yield return FadeCanvasGroup(nextLevelButtonCanvasGroup, 1f, nextButtonFadeDuration, true);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, bool enableInputAtEnd)
    {
        if (cg == null) yield break;

        float start = cg.alpha;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Delta();
            float t = Mathf.Clamp01(timer / duration);
            cg.alpha = Mathf.Lerp(start, targetAlpha, t);
            yield return null;
        }

        cg.alpha = targetAlpha;
        cg.interactable = enableInputAtEnd;
        cg.blocksRaycasts = enableInputAtEnd;
    }

    private float Delta()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }
}