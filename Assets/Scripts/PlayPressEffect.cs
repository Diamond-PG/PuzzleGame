using UnityEngine;
using UnityEngine.EventSystems;

public class PlayPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("What to scale (drag PlayButton сюда)")]
    public RectTransform target;

    [Header("Scale settings")]
    public float pressedScale = 0.93f;   // провал
    public float speed = 16f;            // скорость анимации

    Vector3 _startScale;
    Vector3 _targetScale;

    void Awake()
    {
        if (target == null)
            target = transform.parent as RectTransform;

        _startScale = target.localScale;
        _targetScale = _startScale;
    }

    void Update()
    {
        if (target == null) return;
        target.localScale = Vector3.Lerp(target.localScale, _targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _targetScale = _startScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _targetScale = _startScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = _startScale;
    }
}