using System.Collections;
using UnityEngine;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Панель победы (CanvasGroup)")]
    [SerializeField] private CanvasGroup levelCompletePanel;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Кнопка Next Level (опционально)")]
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private Animator nextLevelButtonAnimator;
    [SerializeField] private float nextButtonDelay = 1.0f;

    [Header("Плавное появление кнопки (CanvasGroup)")]
    [SerializeField] private CanvasGroup nextLevelButtonCanvasGroup;
    [SerializeField] private float nextButtonFadeDuration = 0.3f;

    [Header("Текст WIN (опционально)")]
    [SerializeField] private Animator winTextAnimator;
    [SerializeField] private string winTextAnimState = "WinTextPop";

    public bool IsShown { get; private set; }

    private Coroutine panelRoutine;
    private Coroutine buttonRoutine;

    private void Awake()
    {
        // ВАЖНО: панель должна быть активной (галочка включена),
        // а скрывать/показывать мы будем через CanvasGroup.
        if (levelCompletePanel != null)
            levelCompletePanel.gameObject.SetActive(true);

        HideInstant();
    }

    // Вызывай при победе
    public void ShowPanel()
    {
        if (levelCompletePanel == null)
        {
            Debug.LogWarning("LevelCompleteUI: levelCompletePanel не назначен!");
            return;
        }

        // Не показываем дважды
        if (IsShown) return;

        // Остановим предыдущие корутины, если были
        if (panelRoutine != null) StopCoroutine(panelRoutine);
        if (buttonRoutine != null) StopCoroutine(buttonRoutine);

        // Панель точно должна быть активна
        levelCompletePanel.gameObject.SetActive(true);

        IsShown = true;

        // Плавно показать панель
        panelRoutine = StartCoroutine(FadeCanvasGroup(
            levelCompletePanel, 1f, fadeDuration, enableInputAtEnd: true));

        // Кнопка Next Level (если есть)
        buttonRoutine = StartCoroutine(ShowNextButtonWithDelay());

        // Анимация WIN-текста (если есть)
        if (winTextAnimator != null && !string.IsNullOrEmpty(winTextAnimState))
        {
            winTextAnimator.Play(winTextAnimState, 0, 0f);
        }
    }

    // Если нужно скрывать панель (например, при рестарте уровня)
    public void HidePanel()
    {
        if (levelCompletePanel == null) return;

        if (panelRoutine != null) StopCoroutine(panelRoutine);
        if (buttonRoutine != null) StopCoroutine(buttonRoutine);

        IsShown = false;
        HideInstant();
    }

    // ====== PRIVATE ======

    private void HideInstant()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.gameObject.SetActive(true);
            levelCompletePanel.alpha = 0f;
            levelCompletePanel.interactable = false;
            levelCompletePanel.blocksRaycasts = false;
        }

        HideNextButtonInstant();
        IsShown = false;
    }

    private void HideNextButtonInstant()
    {
        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);

        if (nextLevelButtonCanvasGroup != null)
        {
            nextLevelButtonCanvasGroup.alpha = 0f;
            nextLevelButtonCanvasGroup.interactable = false;
            nextLevelButtonCanvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator ShowNextButtonWithDelay()
    {
        // Если кнопки нет — просто выходим
        if (nextLevelButton == null && nextLevelButtonCanvasGroup == null)
            yield break;

        yield return WaitSeconds(nextButtonDelay);

        // Если кнопку делаем через GameObject
        if (nextLevelButton != null)
            nextLevelButton.SetActive(true);

        // Если есть CanvasGroup — сделаем плавное появление
        if (nextLevelButtonCanvasGroup != null)
        {
            // Перед фейдом выключаем клики
            nextLevelButtonCanvasGroup.interactable = false;
            nextLevelButtonCanvasGroup.blocksRaycasts = false;

            yield return FadeCanvasGroup(
                nextLevelButtonCanvasGroup, 1f, nextButtonFadeDuration, enableInputAtEnd: true);
        }

        // Pop-анимация кнопки (если есть)
        if (nextLevelButtonAnimator != null)
            nextLevelButtonAnimator.Play("NextButtonPop", 0, 0f);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, bool enableInputAtEnd)
    {
        if (cg == null) yield break;

        float start = cg.alpha;

        // Пока меняем альфу — блокируем клики
        cg.interactable = false;
        cg.blocksRaycasts = false;

        if (duration <= 0f)
        {
            cg.alpha = targetAlpha;
        }
        else
        {
            float t = 0f;
            while (t < duration)
            {
                t += Delta();
                float k = Mathf.Clamp01(t / duration);
                cg.alpha = Mathf.Lerp(start, targetAlpha, k);
                yield return null;
            }
            cg.alpha = targetAlpha;
        }

        // Финальные значения интерактива
        if (enableInputAtEnd && targetAlpha >= 0.99f)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    private float Delta() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private IEnumerator WaitSeconds(float seconds)
    {
        if (seconds <= 0f) yield break;

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }
}