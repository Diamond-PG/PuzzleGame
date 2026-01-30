using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float pressedScale = 1.12f;
    [SerializeField] private float speed = 20f;

    private RectTransform rt;
    private Vector3 normal;
    private Vector3 target;
    private bool isPressed;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        normal = rt.localScale;
        target = normal;
    }

    private void Update()
    {
        rt.localScale = Vector3.Lerp(rt.localScale, target, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        target = normal * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        target = normal;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // если палец/мышь ушли с кнопки — возвращаем размер
        if (!isPressed) return;
        isPressed = false;
        target = normal;
    }
}