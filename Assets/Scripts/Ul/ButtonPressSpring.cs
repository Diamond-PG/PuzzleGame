using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressSpring : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float pressScale = 1.15f;     // насколько увеличиваем при нажатии
    [SerializeField] private float releaseUndershoot = 0.94f; // насколько "проваливаемся" после отпускания
    [SerializeField] private float pressTime = 0.06f;      // скорость увеличения
    [SerializeField] private float bounceTime = 0.12f;     // скорость отскока
    [SerializeField] private float returnTime = 0.10f;     // скорость возврата в норму

    [Header("Use Unscaled Time")]
    [SerializeField] private bool useUnscaledTime = true;

    private RectTransform rt;
    private Vector3 normalScale;
    private Coroutine routine;
    private bool pressed;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        normalScale = rt.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        StartAnim(PressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
        StartAnim(ReleaseRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Если палец ушёл с кнопки — отпускаем
        if (!pressed) return;
        pressed = false;
        StartAnim(ReleaseRoutine());
    }

    private void StartAnim(System.Collections.IEnumerator r)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(r);
    }

    private float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private System.Collections.IEnumerator PressRoutine()
    {
        Vector3 target = normalScale * pressScale;
        yield return LerpScale(target, pressTime);
    }

    private System.Collections.IEnumerator ReleaseRoutine()
    {
        // 1) чуть "проваливаемся" ниже нормального
        Vector3 undershoot = normalScale * releaseUndershoot;
        yield return LerpScale(undershoot, bounceTime);

        // 2) возвращаемся в норму
        yield return LerpScale(normalScale, returnTime);
    }

    private System.Collections.IEnumerator LerpScale(Vector3 target, float time)
    {
        Vector3 start = rt.localScale;
        if (time <= 0f)
        {
            rt.localScale = target;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Dt() / time;

            // smoothstep (мягко)
            float s = t * t * (3f - 2f * t);
            rt.localScale = Vector3.LerpUnclamped(start, target, s);

            yield return null;
        }

        rt.localScale = target;
    }
}