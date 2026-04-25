using UnityEngine;

public class GoalGlowPulse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRendererToPulse;

    [Header("Scale Pulse")]
    [SerializeField] private bool pulseScale = true;
    [SerializeField] private Vector3 baseScale = new Vector3(3.2f, 4.0f, 1f);
    [SerializeField] private Vector3 pulseScaleMax = new Vector3(3.6f, 4.4f, 1f);
    [SerializeField] private float scalePulseSpeed = 2.2f;

    [Header("Alpha Pulse")]
    [SerializeField] private bool pulseAlpha = true;
    [SerializeField] private float minAlpha = 0.35f;
    [SerializeField] private float maxAlpha = 0.60f;
    [SerializeField] private float alphaPulseSpeed = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Color baseColor;

    private void Awake()
    {
        if (spriteRendererToPulse == null)
            spriteRendererToPulse = GetComponent<SpriteRenderer>();

        if (spriteRendererToPulse != null)
            baseColor = spriteRendererToPulse.color;

        transform.localScale = baseScale;

        if (debugLogs)
            Debug.Log("[GoalGlowPulse] Awake complete.", this);
    }

    private void Update()
    {
        float time = Time.time;

        if (pulseScale)
        {
            float tScale = (Mathf.Sin(time * scalePulseSpeed) + 1f) * 0.5f;
            transform.localScale = Vector3.Lerp(baseScale, pulseScaleMax, tScale);
        }

        if (pulseAlpha && spriteRendererToPulse != null)
        {
            float tAlpha = (Mathf.Sin(time * alphaPulseSpeed) + 1f) * 0.5f;
            float alpha01 = Mathf.Lerp(minAlpha, maxAlpha, tAlpha);

            Color c = baseColor;
            c.a = alpha01;
            spriteRendererToPulse.color = c;
        }
    }
}