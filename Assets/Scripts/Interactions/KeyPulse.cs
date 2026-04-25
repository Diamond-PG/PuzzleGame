using UnityEngine;

public class KeyPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float scaleAmount = 0.08f;
    [SerializeField] private float minScaleOffset = 0.98f; // насколько он может уменьшаться (1 = не уменьшается вообще)

    [SerializeField] private bool useUnscaledTime = false;

    private Vector3 baseScale;

    private void OnEnable()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        // синус от 0 до 1
        float pulse = (Mathf.Sin(t * speed) + 1f) * 0.5f;

        // теперь ограничиваем минимальное уменьшение
        float scale = Mathf.Lerp(minScaleOffset, 1f + scaleAmount, pulse);

        transform.localScale = new Vector3(
            baseScale.x * scale,
            baseScale.y * scale,
            baseScale.z
        );
    }
}