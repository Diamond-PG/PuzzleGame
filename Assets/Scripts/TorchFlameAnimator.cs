
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TorchFlameAnimator : MonoBehaviour
{
    [Header("Six Fire Sprites")]
    [SerializeField] private Sprite[] fireFrames = new Sprite[6];

    [Header("Animation")]
    [SerializeField, Range(4f, 20f)]
    private float framesPerSecond = 10f;

    [SerializeField] private bool randomStartFrame = true;
    [SerializeField] private bool randomFrameDirection = false;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Natural Flicker")]
    [SerializeField, Range(0f, 0.3f)]
    private float brightnessFlicker = 0.08f;

    [SerializeField, Range(0f, 0.15f)]
    private float sizeFlicker = 0.035f;

    [SerializeField, Range(0.5f, 10f)]
    private float flickerSpeed = 3f;

    [Header("Individual Flame")]
    [SerializeField] private float timeOffset = 0f;

    private Image flameImage;
    private Vector3 originalScale;
    private Color originalColor;

    private float elapsedTime;
    private float flickerSeed;
    private int frameIndex;
    private int frameDirection = 1;

    private void Awake()
    {
        flameImage = GetComponent<Image>();

        originalScale = transform.localScale;
        originalColor = flameImage.color;

        flickerSeed = Random.Range(0f, 100f);

        if (randomStartFrame && fireFrames.Length > 0)
            frameIndex = Random.Range(0, fireFrames.Length);

        if (randomFrameDirection && Random.value > 0.5f)
            frameDirection = -1;

        elapsedTime = timeOffset;

        UpdateSprite();
    }

    private void Update()
    {
        float deltaTime = useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        elapsedTime += deltaTime;

        UpdateAnimation();
        UpdateFlicker();
    }

    private void UpdateAnimation()
    {
        if (fireFrames == null || fireFrames.Length == 0)
            return;

        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);

        while (elapsedTime >= frameTime)
        {
            elapsedTime -= frameTime;

            frameIndex += frameDirection;

            if (frameIndex >= fireFrames.Length)
                frameIndex = 0;

            if (frameIndex < 0)
                frameIndex = fireFrames.Length - 1;

            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if (flameImage == null ||
            fireFrames == null ||
            fireFrames.Length == 0)
            return;

        Sprite sprite = fireFrames[frameIndex];

        if (sprite != null)
            flameImage.sprite = sprite;
    }

    private void UpdateFlicker()
    {
        float t = Time.unscaledTime * flickerSpeed + flickerSeed;

        float noise = Mathf.PerlinNoise(t, flickerSeed);
        float centeredNoise = noise * 2f - 1f;

        float alphaMultiplier =
            1f + centeredNoise * brightnessFlicker;

        Color color = originalColor;
        color.a = Mathf.Clamp01(
            originalColor.a * alphaMultiplier
        );

        flameImage.color = color;

        float scaleMultiplier =
            1f + centeredNoise * sizeFlicker;

        transform.localScale =
            originalScale * scaleMultiplier;
    }

    private void OnDisable()
    {
        if (flameImage != null)
            flameImage.color = originalColor;

        transform.localScale = originalScale;
    }
}
