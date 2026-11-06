
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UIButtonPressEffect : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform visual;
    [SerializeField] private Button button;

    [Header("Press Animation")]
    [SerializeField] private bool enableEffect = true;

    [SerializeField, Range(0.75f, 1f)]
    private float pressedScale = 0.94f;

    [SerializeField, Min(0.01f)]
    private float pressDuration = 0.08f;

    [Header("Release Animation")]
    [SerializeField, Range(1f, 1.2f)]
    private float bounceScale = 1.04f;

    [SerializeField, Min(0.01f)]
    private float bounceDuration = 0.10f;

    [SerializeField, Min(0.01f)]
    private float returnDuration = 0.16f;

    [Header("Time Settings")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 originalScale;
    private Coroutine animationRoutine;
    private bool pointerPressed;
    private bool initialized;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (visual == null)
        {
            Debug.LogWarning(
                "[UIButtonPressEffect] Visual is not assigned!",
                this
            );
            return;
        }

        originalScale = visual.localScale;
        initialized = true;
    }

    private void OnEnable()
    {
        pointerPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanAnimate())
            return;

        pointerPressed = true;

        StartAnimation(
            PressRoutine()
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReleaseButton();
    }

    private bool CanAnimate()
    {
        if (!enableEffect || !initialized || visual == null)
            return false;

        if (button != null && !button.IsInteractable())
            return false;

        return true;
    }

    private void ReleaseButton()
    {
        if (!pointerPressed)
            return;

        pointerPressed = false;

        if (!initialized || visual == null)
            return;

        StartAnimation(
            ReleaseRoutine()
        );
    }

    private void StartAnimation(IEnumerator routine)
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        animationRoutine = StartCoroutine(routine);
    }

    private IEnumerator PressRoutine()
    {
        yield return ScaleTo(
            originalScale * pressedScale,
            pressDuration
        );

        animationRoutine = null;
    }

    private IEnumerator ReleaseRoutine()
    {
        // Small elastic bounce.
        yield return ScaleTo(
            originalScale * bounceScale,
            bounceDuration
        );

        // Smooth return to original size.
        yield return ScaleTo(
            originalScale,
            returnDuration
        );

        animationRoutine = null;
    }

    private IEnumerator ScaleTo(
        Vector3 targetScale,
        float duration)
    {
        Vector3 startScale = visual.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            // Smoothstep easing.
            t = t * t * (3f - 2f * t);

            visual.localScale =
                Vector3.LerpUnclamped(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        visual.localScale = targetScale;
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        pointerPressed = false;

        if (initialized && visual != null)
            visual.localScale = originalScale;
    }
}
