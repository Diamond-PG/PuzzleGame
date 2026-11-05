
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class GuardianBlink : MonoBehaviour
{
    [Header("Guardian Sprites")]
    [SerializeField] private Sprite eyesOpen;
    [SerializeField] private Sprite eyesClosed;

    [Header("Blink Interval")]
    [Min(0.1f)]
    [SerializeField] private float minBlinkInterval = 2.2f;

    [Min(0.1f)]
    [SerializeField] private float maxBlinkInterval = 5f;

    [Header("Blink Duration")]
    [Min(0.02f)]
    [SerializeField] private float closedEyesDuration = 0.13f;

    [Header("Double Blink")]
    [Range(0f, 1f)]
    [SerializeField] private float doubleBlinkChance = 0.15f;

    [Min(0.02f)]
    [SerializeField] private float doubleBlinkDelay = 0.18f;

    [Header("Playback")]
    [SerializeField] private bool blinkOnStart = true;

    private Image guardianImage;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        guardianImage = GetComponent<Image>();

        if (eyesOpen == null)
            eyesOpen = guardianImage.sprite;
    }

    private void OnEnable()
    {
        if (guardianImage == null)
            guardianImage = GetComponent<Image>();

        ShowOpenEyes();

        if (blinkOnStart)
            StartBlinking();
    }

    private void OnDisable()
    {
        StopBlinking();
        ShowOpenEyes();
    }

    public void StartBlinking()
    {
        StopBlinking();

        if (!isActiveAndEnabled)
            return;

        if (eyesOpen == null || eyesClosed == null)
        {
            Debug.LogWarning(
                "GuardianBlink: Assign both sprites.",
                this
            );
            return;
        }

        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        ShowOpenEyes();
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float minTime = Mathf.Max(
                0.1f, minBlinkInterval
            );

            float maxTime = Mathf.Max(
                minTime, maxBlinkInterval
            );

            float waitTime = Random.Range(
                minTime, maxTime
            );

            yield return new WaitForSecondsRealtime(waitTime);

            yield return BlinkOnce();

            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSecondsRealtime(
                    doubleBlinkDelay
                );

                yield return BlinkOnce();
            }
        }
    }

    private IEnumerator BlinkOnce()
    {
        guardianImage.sprite = eyesClosed;

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.02f, closedEyesDuration)
        );

        ShowOpenEyes();
    }

    private void ShowOpenEyes()
    {
        if (guardianImage != null && eyesOpen != null)
            guardianImage.sprite = eyesOpen;
    }
}
