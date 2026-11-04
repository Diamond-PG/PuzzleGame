
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CrownSparkleWave : MonoBehaviour
{
    [Header("Five Sparkles - Left To Right")]
    [SerializeField] private Image[] sparkles = new Image[5];

    [Header("Wave Timing")]
    [Min(0.01f)]
    [SerializeField] private float delayBetweenSparkles = 0.20f;

    [Min(0.05f)]
    [SerializeField] private float sparkleDuration = 0.65f;

    [Min(0f)]
    [SerializeField] private float pauseBetweenWaves = 5f;

    [Header("Fade")]
    [Range(0.01f, 0.95f)]
    [SerializeField] private float fadeInPortion = 0.35f;

    [Range(0.01f, 0.95f)]
    [SerializeField] private float fadeOutPortion = 0.45f;

    [Header("Appearance")]
    [Range(0f, 1f)]
    [SerializeField] private float sparkleBrightness = 1f;

    [Min(0.01f)]
    [SerializeField] private float sparkleSize = 1f;

    [Header("Playback")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine waveCoroutine;
    private Color[] originalColors;
    private Vector3[] originalScales;

    private void Awake()
    {
        originalColors = new Color[sparkles.Length];
        originalScales = new Vector3[sparkles.Length];

        for (int i = 0; i < sparkles.Length; i++)
        {
            Image image = sparkles[i];

            if (image == null)
                continue;

            originalColors[i] = image.color;
            originalScales[i] = image.rectTransform.localScale;

            SetAlpha(i, 0f);
        }
    }

    private void OnEnable()
    {
        if (playOnStart)
            PlayWave();
    }

    private void OnDisable()
    {
        StopWave();

        if (sparkles == null)
            return;

        for (int i = 0; i < sparkles.Length; i++)
            SetAlpha(i, 0f);
    }

    public void PlayWave()
    {
        StopWave();

        if (!isActiveAndEnabled || sparkles == null)
            return;

        waveCoroutine = StartCoroutine(WaveRoutine());
    }

    public void StopWave()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        if (sparkles == null)
            return;

        for (int i = 0; i < sparkles.Length; i++)
            SetAlpha(i, 0f);
    }

    private IEnumerator WaveRoutine()
    {
        do
        {
            float totalWaveTime =
                (sparkles.Length - 1) * delayBetweenSparkles
                + sparkleDuration;

            float elapsed = 0f;

            while (elapsed < totalWaveTime)
            {
                for (int i = 0; i < sparkles.Length; i++)
                {
                    float localTime =
                        elapsed - i * delayBetweenSparkles;

                    float intensity = EvaluateSparkle(localTime);

                    SetAlpha(i, intensity * sparkleBrightness);
                    SetSize(i);
                }

                yield return null;

                elapsed += useUnscaledTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;
            }

            for (int i = 0; i < sparkles.Length; i++)
                SetAlpha(i, 0f);

            float pauseElapsed = 0f;

            while (pauseElapsed < pauseBetweenWaves)
            {
                yield return null;

                pauseElapsed += useUnscaledTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;
            }

        } while (loop);

        waveCoroutine = null;
    }

    private float EvaluateSparkle(float time)
    {
        if (time < 0f || time >= sparkleDuration)
            return 0f;

        float normalized = time / sparkleDuration;

        float fadeInEnd = fadeInPortion;
        float fadeOutStart = 1f - fadeOutPortion;

        if (normalized < fadeInEnd)
        {
            float t = normalized / fadeInEnd;
            return Mathf.SmoothStep(0f, 1f, t);
        }

        if (normalized > fadeOutStart)
        {
            float t =
                (normalized - fadeOutStart) /
                Mathf.Max(0.001f, 1f - fadeOutStart);

            return 1f - Mathf.SmoothStep(0f, 1f, t);
        }

        return 1f;
    }

    private void SetAlpha(int index, float alpha)
    {
        if (index < 0 || index >= sparkles.Length)
            return;

        Image image = sparkles[index];

        if (image == null)
            return;

        Color color = originalColors[index];
        color.a = originalColors[index].a *
                  Mathf.Clamp01(alpha);

        image.color = color;
    }

    private void SetSize(int index)
    {
        if (index < 0 || index >= sparkles.Length)
            return;

        Image image = sparkles[index];

        if (image == null)
            return;

        image.rectTransform.localScale =
            originalScales[index] * sparkleSize;
    }
}
