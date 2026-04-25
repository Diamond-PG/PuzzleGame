using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonPressSpring : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float pressedScale = 0.90f; // сделаем заметнее
    [SerializeField] private float downSpeed = 30f;
    [SerializeField] private float upSpeed = 20f;

    [Header("Optional")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 startScale;
    private Vector3 targetScale;
    private bool isPressed;

    private void Awake()
    {
        startScale = transform.localScale;
        targetScale = startScale;
    }

    private void OnEnable()
    {
        // на случай если кнопка включается/выключается
        startScale = transform.localScale;
        targetScale = startScale;
        isPressed = false;
    }

    private void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float speed = isPressed ? downSpeed : upSpeed;

        // плавно тянем масштаб к цели
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 1f - Mathf.Exp(-speed * dt));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = startScale * pressedScale;
        // Debug.Log("DOWN: " + gameObject.name);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        targetScale = startScale;
        // Debug.Log("UP: " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // если палец/мышь ушла с кнопки — отпускаем
        isPressed = false;
        targetScale = startScale;
    }
}