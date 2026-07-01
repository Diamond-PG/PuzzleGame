using UnityEngine;

public class HeartPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float scaleAmount = 0.12f;
    [SerializeField] private float minScaleOffset = 0.95f;

    [SerializeField] private bool useUnscaledTime = false;

    private Vector3 baseScale;
    private bool hasBaseScale;

    private void OnEnable()
    {
        ResetBaseScale();
    }

    private void Update()
    {
        if (!hasBaseScale)
            ResetBaseScale();

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        float pulse = (Mathf.Sin(t * speed) + 1f) * 0.5f;

        float scale = Mathf.Lerp(minScaleOffset, 1f + scaleAmount, pulse);

        transform.localScale = new Vector3(
            baseScale.x * scale,
            baseScale.y * scale,
            baseScale.z
        );
    }

    public void ResetBaseScale()
    {
        baseScale = transform.localScale;
        hasBaseScale = true;
    }

    public void SetBaseScale(Vector3 newBaseScale)
    {
        baseScale = newBaseScale;
        hasBaseScale = true;
        transform.localScale = newBaseScale;
    }
}