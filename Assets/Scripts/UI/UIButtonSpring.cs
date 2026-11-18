using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSpring : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float pressedScale = 1.12f;
    [SerializeField] private float pressTime = 0.06f;
    [SerializeField] private float releaseTime = 0.10f;

    [Header("Optional")]
    [SerializeField] private bool useUnscaledTime = true; // полезно позже, когда Pause будет через Time.timeScale

    private Vector3 _defaultScale;
    private Coroutine _routine;

    private void Awake()
    {
        _defaultScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartSpring(_defaultScale * pressedScale, pressTime);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartSpring(_defaultScale, releaseTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // если палец/мышь уехали с кнопки — вернуть в норму
        StartSpring(_defaultScale, releaseTime);
    }

    private void StartSpring(Vector3 target, float duration)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ScaleTo(target, duration));
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);

            // лёгкая "пружинка" (smooth)
            float eased = 1f - Mathf.Pow(1f - k, 3f); // easeOutCubic
            transform.localScale = Vector3.LerpUnclamped(start, target, eased);

            yield return null;
        }

        transform.localScale = target;
        _routine = null;
    }
}