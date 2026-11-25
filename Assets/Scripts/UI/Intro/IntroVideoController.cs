using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using DeviceScreen = UnityEngine.Device.Screen;

public sealed class IntroVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private bool enableIntroVideo = true;
    [SerializeField] private VideoClip videoClip;
    [SerializeField, Range(0f, 1f)] private float videoVolume = 1f;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource videoAudio;
    [SerializeField] private RawImage videoImage;

    [Header("Skip button (1920 × 1080 reference units)")]
    [SerializeField] private Button skipButton;
    [SerializeField, Min(1f)] private float skipButtonWidth = 100f;
    [SerializeField, Min(1f)] private float skipButtonHeight = 100f;
    [SerializeField] private float skipButtonOffsetX = -120f;
    [SerializeField] private float skipButtonOffsetY = -90f;
    [SerializeField] private UIClickSfx skipClick;
    [SerializeField] private UIHapticsOnPress skipHaptics;

    [Header("Transition")]
    [SerializeField] private CanvasGroup presentation;
    [SerializeField] private Image letterbox;
    [SerializeField] private Image fade;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.35f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.45f;
    [SerializeField, Min(1f)] private float playbackTimeout = 12f;

    private static bool startupConsumed;
    private bool preparedRequested;
    private bool playbackStarted;
    private bool finishRequested;
    private bool applicationPaused;
    private Rect lastSafeArea;
    private Vector2Int lastScreen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStartup() => startupConsumed = false;

    private void Awake()
    {
        presentation.alpha = 0f;
        presentation.blocksRaycasts = false;
        skipButton.interactable = false;
        skipButton.onClick.AddListener(OnSkipClicked);
        videoPlayer.loopPointReached += OnVideoEnded;
        videoPlayer.errorReceived += OnVideoError;
    }

    public void Prepare()
    {
        if (!enableIntroVideo || startupConsumed || preparedRequested || videoClip == null)
            return;

        preparedRequested = true;
        videoPlayer.clip = videoClip;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.controlledAudioTrackCount = (ushort)(videoClip.audioTrackCount > 0 ? 1 : 0);
        if (videoPlayer.controlledAudioTrackCount > 0)
        {
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, videoAudio);
        }

        videoAudio.volume = videoVolume;
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        videoAudio.mute = !soundEnabled;
        if (skipClick != null && skipClick.TryGetComponent(out AudioSource clickSource))
            clickSource.mute = !soundEnabled;

        float pixelAspect = (float)videoClip.pixelAspectRatioNumerator / Mathf.Max(1f, videoClip.pixelAspectRatioDenominator);
        videoImage.GetComponent<AspectRatioFitter>().aspectRatio = videoClip.width * pixelAspect / videoClip.height;
        videoPlayer.Prepare();
    }

    public bool TryPlay(string menuScene)
    {
        if (!enableIntroVideo || startupConsumed)
            return false;

        Prepare();
        startupConsumed = true;
        StartCoroutine(RunStartup(menuScene));
        return true;
    }

    private IEnumerator RunStartup(string menuScene)
    {
        presentation.alpha = 1f;
        presentation.blocksRaycasts = true;
        fade.raycastTarget = true;
        SetFade(1f);
        UpdateSkipLayout();
        skipButton.gameObject.SetActive(true);
        skipButton.interactable = !finishRequested && preparedRequested;

        float waiting = 0f;
        while (preparedRequested && !finishRequested && (!videoPlayer.isPrepared || applicationPaused))
        {
            UpdateSkipLayout();
            if (!applicationPaused && (waiting += Time.unscaledDeltaTime) >= playbackTimeout)
            {
                RequestFinish();
                break;
            }
            yield return null;
        }

        if (preparedRequested && !finishRequested)
        {
            playbackStarted = true;
            videoPlayer.Play();
            yield return PresentFrames();
        }

        RequestFinish();
        yield return FadeTo(1f, fadeOutDuration);
        ReleaseVideo();
        DontDestroyOnLoad(gameObject);

        yield return SceneManager.LoadSceneAsync(menuScene);
        yield return FadeTo(0f, fadeInDuration);
        Destroy(gameObject);
    }

    private IEnumerator PresentFrames()
    {
        float stalled = 0f;
        float revealElapsed = 0f;
        long lastFrame = -1;
        while (!finishRequested)
        {
            UpdateSkipLayout();
            if (!applicationPaused)
            {
                float delta = Time.unscaledDeltaTime;
                long frame = videoPlayer.frame;
                Texture texture = videoPlayer.texture;
                if (frame >= 0 && texture != null)
                {
                    if (videoImage.texture != texture)
                    {
                        videoImage.texture = texture;
                        videoImage.enabled = true;
                    }
                    revealElapsed += delta;
                    SetFade(fadeInDuration > 0f ? 1f - Mathf.Clamp01(revealElapsed / fadeInDuration) : 0f);
                }

                stalled = frame != lastFrame ? 0f : stalled + delta;
                lastFrame = frame;
                if (stalled >= playbackTimeout)
                    RequestFinish();
            }
            if (!finishRequested)
                yield return null;
        }
    }

    private void UpdateSkipLayout()
    {
        var screen = new Vector2Int(DeviceScreen.width, DeviceScreen.height);
        Rect safe = DeviceScreen.safeArea;
#if UNITY_EDITOR
        // Ordinary Game View may use the host monitor's safe area at custom resolutions.
        // Keep the simulated insets when Device Simulator supplies different screen data.
        if (screen.x == Screen.width && screen.y == Screen.height && safe == Screen.safeArea)
            safe = new Rect(0f, 0f, screen.x, screen.y);
#endif
        if (screen.x <= 0 || screen.y <= 0 || (screen == lastScreen && safe == lastSafeArea))
            return;

        lastScreen = screen;
        lastSafeArea = safe;
        var rect = (RectTransform)skipButton.transform;
        rect.anchorMin = rect.anchorMax = safe.width > 0f && safe.height > 0f
            ? new Vector2(Mathf.Clamp01(safe.xMax / screen.x), Mathf.Clamp01(safe.yMax / screen.y))
            : Vector2.one;
        rect.pivot = Vector2.one;
        rect.sizeDelta = new Vector2(skipButtonWidth, skipButtonHeight);
        rect.anchoredPosition = new Vector2(skipButtonOffsetX, skipButtonOffsetY);
    }

    private void OnSkipClicked()
    {
        if (!skipButton.interactable || finishRequested)
            return;

        skipHaptics?.PlayHaptic();
        RequestFinish();
        skipClick?.PlayClick();
    }

    private void OnVideoEnded(VideoPlayer player) => RequestFinish();

    private void OnVideoError(VideoPlayer player, string message)
    {
        Debug.LogWarning($"Intro video playback failed: {message}", this);
        RequestFinish();
    }

    private void RequestFinish()
    {
        if (finishRequested)
            return;

        finishRequested = true;
        skipButton.interactable = false;
        skipButton.gameObject.SetActive(false);
        videoAudio.mute = true;
        // Hold the last picture under the fade while ending the soundtrack immediately.
        if (playbackStarted && videoPlayer.isPlaying)
            videoPlayer.Pause();
    }

    private void ReleaseVideo()
    {
        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoClip = null;
        videoImage.texture = null;
        videoImage.enabled = false;
        videoAudio.Stop();
        // Only the transition cover should remain visible over the destination scene.
        letterbox.enabled = false;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = fade.color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetFade(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetFade(target);
    }

    private void SetFade(float alpha) => fade.color = new Color(0f, 0f, 0f, alpha);

    private void OnApplicationPause(bool paused)
    {
        applicationPaused = paused;
        if (!playbackStarted || finishRequested)
            return;

        videoAudio.mute = paused || PlayerPrefs.GetInt("SoundEnabled", 1) != 1;
        if (paused)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    private void OnDestroy()
    {
        skipButton.onClick.RemoveListener(OnSkipClicked);
        videoPlayer.loopPointReached -= OnVideoEnded;
        videoPlayer.errorReceived -= OnVideoError;
    }
}
