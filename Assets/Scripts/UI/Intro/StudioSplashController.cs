
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudioSplashController : MonoBehaviour
{
    [Header("Studio Logo")]
    [SerializeField] private CanvasGroup studioLogo;

    [Header("Animation Timing")]
    [Min(0f)]
    [SerializeField] private float initialDelay = 0.3f;

    [Min(0.01f)]
    [SerializeField] private float fadeInDuration = 1.5f;

    [Min(0f)]
    [SerializeField] private float visibleDuration = 2f;

    [Min(0.01f)]
    [SerializeField] private float fadeOutDuration = 1.2f;

    [Header("Next Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private IEnumerator Start()
    {
        if (studioLogo == null)
        {
            Debug.LogError(
                "StudioSplashController: Studio Logo is not assigned."
            );
            yield break;
        }

        studioLogo.alpha = 0f;
        studioLogo.interactable = false;
        studioLogo.blocksRaycasts = false;

        yield return new WaitForSecondsRealtime(initialDelay);

        yield return FadeLogo(0f, 1f, fadeInDuration);

        yield return new WaitForSecondsRealtime(visibleDuration);

        yield return FadeLogo(1f, 0f, fadeOutDuration);

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError(
                "StudioSplashController: Scene '" +
                mainMenuSceneName +
                "' is not included in the build."
            );
            yield break;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator FadeLogo(
        float from,
        float to,
        float duration)
    {
        float elapsed = 0f;

        studioLogo.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            // Smooth acceleration and deceleration.
            float smooth = progress * progress *
                           (3f - 2f * progress);

            studioLogo.alpha = Mathf.Lerp(from, to, smooth);

            yield return null;
        }

        studioLogo.alpha = to;
    }
}
