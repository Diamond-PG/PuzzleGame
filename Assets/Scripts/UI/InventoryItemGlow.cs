using UnityEngine;
using UnityEngine.UI;

public class InventoryItemGlow : MonoBehaviour
{
    [Header("PULSE")]
    [SerializeField] private float pulseSpeed = 1.4f;
    [SerializeField] private float scaleAmount = 0.08f;

    [Header("ALPHA")]
    [SerializeField, Range(0f, 1f)]
    private float minAlpha = 0.55f;

    [SerializeField, Range(0f, 1f)]
    private float maxAlpha = 0.85f;

    private RectTransform rectTransform;
    private Image image;

    private Vector3 baseScale;

    public void Setup(
        float newPulseSpeed,
        float newScaleAmount,
        float newMinAlpha,
        float newMaxAlpha
    )
    {
        pulseSpeed = newPulseSpeed;
        scaleAmount = newScaleAmount;
        minAlpha = newMinAlpha;
        maxAlpha = newMaxAlpha;
    }

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        image =
            GetComponent<Image>();

        if (rectTransform != null)
        {
            baseScale =
                rectTransform.localScale;
        }
        else
        {
            baseScale =
                Vector3.one;
        }
    }

    private void OnEnable()
    {
        if (rectTransform != null)
        {
            baseScale =
                rectTransform.localScale;
        }
    }

    private void Update()
    {
        if (rectTransform == null)
            return;

        float wave =
            (Mathf.Sin(
                Time.unscaledTime *
                pulseSpeed
            ) + 1f) * 0.5f;

        float scale =
            Mathf.Lerp(
                1f - scaleAmount,
                1f + scaleAmount,
                wave
            );

        rectTransform.localScale =
            baseScale * scale;

        if (image != null)
        {
            Color color =
                image.color;

            color.a =
                Mathf.Lerp(
                    minAlpha,
                    maxAlpha,
                    wave
                );

            image.color =
                color;
        }
    }
}