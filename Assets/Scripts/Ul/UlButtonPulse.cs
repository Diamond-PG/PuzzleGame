using UnityEngine;

public class UIButtonPulse : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.05f; // насколько кнопка "дышит"
    [SerializeField] private float speed = 4f;        // скорость пульсации
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        // на всякий случай возвращаем базовый размер
        transform.localScale = baseScale;
    }

    private void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float s = 1f + Mathf.Sin(t * speed) * amplitude;
        transform.localScale = baseScale * s;
    }
}