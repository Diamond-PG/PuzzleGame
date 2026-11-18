using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IntroBlackScreenController : MonoBehaviour
{
    // ============================================================
    // 01 - START SETTINGS
    // ============================================================

    [Header("START SETTINGS")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField, Min(0f)] private float blackScreenDelay = 1.5f;

    // ============================================================
    // 02 - FADE SETTINGS
    // ============================================================

    [Header("FADE SETTINGS")]
    [SerializeField, Min(0f)] private float fadeDuration = 2f;

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private Image blackImage;
    private Coroutine fadeCoroutine;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        blackImage = GetComponent<Image>();
        SetAlpha(1f);
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (playOnStart)
            fadeCoroutine = StartCoroutine(PlayIntro());
    }

    // ============================================================
    // INTRO
    // ============================================================

    private IEnumerator PlayIntro()
    {
        if (blackScreenDelay > 0f)
            yield return new WaitForSecondsRealtime(blackScreenDelay);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);

            SetAlpha(1f - Mathf.SmoothStep(0f, 1f, progress));

            yield return null;
        }

        SetAlpha(0f);
        fadeCoroutine = null;
    }

    // ============================================================
    // PUBLIC CONTROLS
    // ============================================================

    public void ShowBlackScreen()
    {
        StopCurrentFade();
        SetAlpha(1f);
    }

    public void HideBlackScreen()
    {
        StopCurrentFade();
        SetAlpha(0f);
    }

    public void FadeOutBlackScreen()
    {
        StopCurrentFade();
        fadeCoroutine = StartCoroutine(PlayIntro());
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private void SetAlpha(float alpha)
    {
        if (blackImage == null)
            return;

        Color color = blackImage.color;
        color.a = Mathf.Clamp01(alpha);
        blackImage.color = color;
    }

    private void StopCurrentFade()
    {
        if (fadeCoroutine == null)
            return;

        StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;
    }
}