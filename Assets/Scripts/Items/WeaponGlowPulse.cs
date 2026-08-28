using UnityEngine;

public class WeaponGlowPulse : MonoBehaviour
{
    [Header("GLOW OBJECT")]
    [SerializeField] private SpriteRenderer glowRenderer;

    [Header("SCALE PULSE")]
    [Tooltip("Насколько сильно свечение увеличивается и уменьшается.")]
    [SerializeField, Range(0f, 0.5f)]
    private float scaleAmount = 0.08f;

    [Tooltip("Скорость пульсации.")]
    [SerializeField, Range(0.1f, 5f)]
    private float pulseSpeed = 1.4f;

    [Header("ALPHA PULSE")]
    [Tooltip("Минимальная прозрачность свечения.")]
    [SerializeField, Range(0f, 1f)]
    private float minAlpha = 0.75f;

    [Tooltip("Максимальная прозрачность свечения.")]
    [SerializeField, Range(0f, 1f)]
    private float maxAlpha = 1f;

    [Header("OPTIONS")]
    [Tooltip("Пульсировать по размеру.")]
    [SerializeField] private bool pulseScale = true;

    [Tooltip("Пульсировать по прозрачности.")]
    [SerializeField] private bool pulseAlpha = true;

    private Vector3 startScale;
    private Color startColor;

    private void Awake()
    {
        if (glowRenderer == null)
        {
            glowRenderer = GetComponent<SpriteRenderer>();
        }

        startScale = transform.localScale;

        if (glowRenderer != null)
        {
            startColor = glowRenderer.color;
        }
    }

    private void OnEnable()
    {
        startScale = transform.localScale;

        if (glowRenderer != null)
        {
            startColor = glowRenderer.color;
        }
    }

    private void Update()
    {
        float wave =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        if (pulseScale)
        {
            float scaleMultiplier =
                Mathf.Lerp(
                    1f - scaleAmount,
                    1f + scaleAmount,
                    wave
                );

            transform.localScale =
                new Vector3(
                    startScale.x * scaleMultiplier,
                    startScale.y * scaleMultiplier,
                    startScale.z
                );
        }

        if (pulseAlpha &&
            glowRenderer != null)
        {
            float alpha =
                Mathf.Lerp(
                    minAlpha,
                    maxAlpha,
                    wave
                );

            Color c = startColor;
            c.a = alpha;

            glowRenderer.color = c;
        }
    }

    private void OnDisable()
    {
        transform.localScale = startScale;

        if (glowRenderer != null)
        {
            glowRenderer.color = startColor;
        }
    }
}