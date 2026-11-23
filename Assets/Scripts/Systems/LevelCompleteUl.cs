using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool recordsFirstLevelCompletion;

    [Header("Панель победы")]
    [SerializeField] private CanvasGroup levelCompletePanel;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Staged Victory (Optional)")]
    [SerializeField] private CanvasGroup victoryBackdrop;
    [SerializeField, Min(0f)] private float backdropFadeDuration = 0.32f;
    [SerializeField] private CanvasGroup victoryTitle;
    [SerializeField, Min(0f)] private float titleFadeDuration = 0.28f;
    [SerializeField] private VictoryFireworks fireworks;
    [SerializeField] private PanelBoomEffect frameAnimation;

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
    private RectTransform frameRect;
    private RectTransform frameParent;
    private Vector2 lastViewportSize;

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

        if (victoryBackdrop != null && levelCompletePanel != null)
        {
            frameRect = levelCompletePanel.transform as RectTransform;
            frameParent = frameRect != null ? frameRect.parent as RectTransform : null;
        }
    }

    private void LateUpdate()
    {
        if (!IsShown || frameParent == null || lastViewportSize == frameParent.rect.size)
            return;

        lastViewportSize = frameParent.rect.size;
        Vector2 available = lastViewportSize * 0.94f;
        Vector2 authoredSize = frameRect.rect.size * (frameAnimation != null ? frameAnimation.PeakScale : 1f);
        float scale = Mathf.Min(1f, available.x / authoredSize.x, available.y / authoredSize.y);
        if (frameAnimation != null)
            frameAnimation.SetRestingScale(Vector3.one * scale);
        else
            frameRect.localScale = Vector3.one * scale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnlyOnce && IsShown) return;
        if (!other.CompareTag(playerTag)) return;
        if (recordsFirstLevelCompletion && other.GetComponentInParent<PlayerHealth>() is { IsDead: true })
            return;

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
        if (recordsFirstLevelCompletion)
            LevelProgressSave.RecordObjective(1, LevelProgressSave.Objective.Finish);

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

        if (victoryBackdrop != null)
            panelRoutine = StartCoroutine(ShowStagedVictory());
        else
        {
            panelRoutine = StartCoroutine(FadeCanvasGroup(levelCompletePanel, 1f, fadeDuration, true));
            buttonRoutine = StartCoroutine(ShowNextButtonWithDelay());
            PlayWinTextAnimation();
        }

        if (debugLogs)
            Debug.Log("[LEVEL COMPLETE] Victory panel shown.", this);
    }

    public void LoadNextLevel()
    {
        if (PauseManager.IsMenuPaused)
            return;

        if (LevelLaunchSession.Mode == LevelLaunchMode.GalleryReplay)
        {
            LevelLaunchSession.AdvanceReplayOrReturnToMenu();
            return;
        }

        LevelLaunchSession.AdvanceFromStoryLevel(SceneManager.GetActiveScene().path);
    }

    private void HideInstant()
    {
        if (victoryBackdrop != null)
            SetCanvasGroup(victoryBackdrop, 0f, false);
        if (victoryTitle != null)
            SetCanvasGroup(victoryTitle, 0f, false);

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

    private IEnumerator ShowStagedVictory()
    {
        yield return FadeCanvasGroup(victoryBackdrop, 1f, backdropFadeDuration, false);
        yield return FadeCanvasGroup(levelCompletePanel, 1f, fadeDuration, true);
        buttonRoutine = StartCoroutine(ShowNextButtonWithDelay());
        PlayWinTextAnimation();
        yield return FadeCanvasGroup(victoryTitle, 1f, titleFadeDuration, false);
        yield return buttonRoutine;
        if (fireworks != null)
            fireworks.Play();
    }

    private void PlayWinTextAnimation()
    {
        if (winTextAnimator != null && !string.IsNullOrEmpty(winTextAnimState))
            winTextAnimator.Play(winTextAnimState, 0, 0f);
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
                if (victoryBackdrop != null && nextLevelButtonCanvasGroup != null)
                    nextLevelButtonCanvasGroup.alpha = 0.85f * t;

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

        SetCanvasGroup(cg, targetAlpha, enableInputAtEnd);
    }

    private static void SetCanvasGroup(CanvasGroup group, float alpha, bool acceptsInput)
    {
        group.alpha = alpha;
        group.interactable = acceptsInput;
        group.blocksRaycasts = acceptsInput;
    }

    private float Delta()
    {
        if (victoryBackdrop != null && PauseManager.IsMenuPaused)
            return 0f;
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        if (victoryBackdrop != null)
        {
            float elapsed = 0f;
            while (elapsed < seconds || PauseManager.IsMenuPaused)
            {
                yield return null;
                elapsed += Delta();
            }
        }
        else if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }
}
