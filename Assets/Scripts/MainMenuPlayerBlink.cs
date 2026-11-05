
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class MainMenuPlayerBlink : MonoBehaviour
{
    [Header("POSE 1 - POINTING AT GAME TITLE")]
    [SerializeField] private Image playerImage;
    [SerializeField] private Sprite eyesOpenSprite;
    [SerializeField] private Sprite eyesClosedSprite;

    [Header("POSE 2 - HANDS TOGETHER")]
    [SerializeField] private Sprite restEyesOpenSprite;
    [SerializeField] private Sprite restWinkSprite;

    [Header("HAND TRANSITION")]
    [SerializeField] private bool useTransitionSprite = true;
    [SerializeField] private Sprite transitionSprite;

    [Min(0f)]
    [SerializeField] private float transitionDuration = 0.14f;

    [Header("POSE TIMING")]
    [SerializeField] private bool cyclePoses = true;

    [Min(0.2f)]
    [SerializeField] private float titlePoseDuration = 5f;

    [Min(0.2f)]
    [SerializeField] private float restPoseDuration = 3f;

    [Header("TITLE BLINK TIMING")]
    [Min(0.1f)]
    [SerializeField] private float minBlinkInterval = 2.2f;

    [Min(0.1f)]
    [SerializeField] private float maxBlinkInterval = 5f;

    [Min(0.02f)]
    [SerializeField] private float blinkDuration = 0.12f;

    [Header("NATURAL DOUBLE BLINK")]
    [Range(0f, 1f)]
    [SerializeField] private float doubleBlinkChance = 0.10f;

    [Min(0.02f)]
    [SerializeField] private float doubleBlinkGap = 0.14f;

    [Header("REST POSE WINK")]
    [SerializeField] private bool winkOnceInRest = true;

    [Min(0.05f)]
    [SerializeField] private float restWinkDelay = 1.2f;

    [Header("PLAYBACK")]
    [SerializeField] private bool useUnscaledTime = true;

    private enum PlayerPose
    {
        Title,
        Rest
    }

    private PlayerPose currentPose = PlayerPose.Title;

    private Coroutine transitionRoutine;

    private float poseTimer;
    private float nextBlinkTimer;
    private float blinkTimer;
    private float doubleBlinkTimer;

    private bool isBlinking;
    private bool isSecondBlink;
    private bool waitingForDoubleBlink;
    private bool isTransitioning;

    private bool HasRestSprites =>
        restEyesOpenSprite != null &&
        restWinkSprite != null;

    private void Reset()
    {
        playerImage = GetComponent<Image>();
    }

    private void Awake()
    {
        if (playerImage == null)
            playerImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (playerImage == null)
            playerImage = GetComponent<Image>();

        if (playerImage == null ||
            eyesOpenSprite == null ||
            eyesClosedSprite == null)
        {
            Debug.LogWarning(
                "MainMenuPlayerBlink: Missing Image " +
                "or Title sprites.", this
            );

            return;
        }

        currentPose = PlayerPose.Title;

        poseTimer = 0f;

        isBlinking = false;
        isSecondBlink = false;
        waitingForDoubleBlink = false;
        isTransitioning = false;

        transitionRoutine = null;

        playerImage.sprite = eyesOpenSprite;

        ScheduleBlinkForCurrentPose();
    }

    private void Update()
    {
        if (playerImage == null ||
            eyesOpenSprite == null ||
            eyesClosedSprite == null)
            return;

        if (isTransitioning)
            return;

        float dt = GetDeltaTime();

        if (cyclePoses && HasRestSprites)
        {
            poseTimer += dt;

            float duration =
                currentPose == PlayerPose.Title
                    ? titlePoseDuration
                    : restPoseDuration;

            if (poseTimer >= duration)
            {
                PlayerPose nextPose =
                    currentPose == PlayerPose.Title
                        ? PlayerPose.Rest
                        : PlayerPose.Title;

                CancelBlink();

                isTransitioning = true;

                transitionRoutine = StartCoroutine(
                    ChangePose(nextPose)
                );

                return;
            }
        }

        UpdateBlink(dt);
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;
    }

    private IEnumerator ChangePose(PlayerPose nextPose)
    {
        // Show the intermediate hands position.
        // No transparency or overlapping images.
        if (useTransitionSprite &&
            transitionSprite != null &&
            transitionDuration > 0f)
        {
            playerImage.sprite = transitionSprite;

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += GetDeltaTime();
                yield return null;
            }
        }

        // Switch to the destination pose.
        currentPose = nextPose;

        playerImage.sprite = GetOpenSprite();

        poseTimer = 0f;

        isBlinking = false;
        isSecondBlink = false;
        waitingForDoubleBlink = false;

        ScheduleBlinkForCurrentPose();

        transitionRoutine = null;
        isTransitioning = false;
    }

    private void UpdateBlink(float dt)
    {
        if (isBlinking)
        {
            blinkTimer -= dt;

            if (blinkTimer <= 0f)
                FinishBlink();

            return;
        }

        if (waitingForDoubleBlink)
        {
            doubleBlinkTimer -= dt;

            if (doubleBlinkTimer <= 0f)
            {
                waitingForDoubleBlink = false;
                StartBlink(true);
            }

            return;
        }

        nextBlinkTimer -= dt;

        if (nextBlinkTimer <= 0f)
            StartBlink(false);
    }

    private void StartBlink(bool secondBlink)
    {
        isBlinking = true;
        isSecondBlink = secondBlink;

        blinkTimer = blinkDuration;

        Sprite closed = GetClosedSprite();

        if (closed != null)
            playerImage.sprite = closed;
    }

    private void FinishBlink()
    {
        isBlinking = false;

        playerImage.sprite = GetOpenSprite();

        // In the resting pose, perform one wink.
        if (currentPose == PlayerPose.Rest &&
            winkOnceInRest)
        {
            nextBlinkTimer = float.PositiveInfinity;
            return;
        }

        // Double blinking is used in the title pose.
        if (currentPose == PlayerPose.Title &&
            !isSecondBlink &&
            Random.value < doubleBlinkChance)
        {
            waitingForDoubleBlink = true;
            doubleBlinkTimer = doubleBlinkGap;
        }
        else
        {
            ScheduleBlinkForCurrentPose();
        }
    }

    private void CancelBlink()
    {
        isBlinking = false;
        isSecondBlink = false;
        waitingForDoubleBlink = false;

        playerImage.sprite = GetOpenSprite();
    }

    private void ScheduleBlinkForCurrentPose()
    {
        if (currentPose == PlayerPose.Rest &&
            winkOnceInRest)
        {
            float latestWinkTime = Mathf.Max(
                0f,
                restPoseDuration - blinkDuration - 0.05f
            );

            nextBlinkTimer = Mathf.Min(
                restWinkDelay,
                latestWinkTime
            );
        }
        else
        {
            nextBlinkTimer = Random.Range(
                minBlinkInterval,
                Mathf.Max(
                    minBlinkInterval,
                    maxBlinkInterval
                )
            );
        }
    }

    private Sprite GetOpenSprite()
    {
        if (currentPose == PlayerPose.Rest &&
            restEyesOpenSprite != null)
        {
            return restEyesOpenSprite;
        }

        return eyesOpenSprite;
    }

    private Sprite GetClosedSprite()
    {
        if (currentPose == PlayerPose.Rest &&
            restWinkSprite != null)
        {
            return restWinkSprite;
        }

        return eyesClosedSprite;
    }

    private void OnDisable()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        isTransitioning = false;
        isBlinking = false;
        isSecondBlink = false;
        waitingForDoubleBlink = false;

        if (playerImage != null &&
            eyesOpenSprite != null)
        {
            playerImage.sprite = eyesOpenSprite;
        }
    }

    private void OnValidate()
    {
        titlePoseDuration = Mathf.Max(
            0.2f, titlePoseDuration
        );

        restPoseDuration = Mathf.Max(
            0.2f, restPoseDuration
        );

        transitionDuration = Mathf.Max(
            0f, transitionDuration
        );

        minBlinkInterval = Mathf.Max(
            0.1f, minBlinkInterval
        );

        maxBlinkInterval = Mathf.Max(
            minBlinkInterval,
            maxBlinkInterval
        );

        blinkDuration = Mathf.Max(
            0.02f, blinkDuration
        );

        doubleBlinkGap = Mathf.Max(
            0.02f, doubleBlinkGap
        );

        restWinkDelay = Mathf.Max(
            0.05f, restWinkDelay
        );
    }
}
