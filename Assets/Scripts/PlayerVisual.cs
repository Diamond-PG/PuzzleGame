using System.Collections;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerController playerController;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite blinkSprite;
    [SerializeField] private Sprite lookRightSprite;
    [SerializeField] private Sprite lookUpSprite;
    [SerializeField] private Sprite lookLeftSprite;
    [SerializeField] private Sprite lookDownSprite;

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.05f;

    [Header("Start Direction")]
    [SerializeField] private LookDirection startDirection = LookDirection.Down;
    [SerializeField] private bool useDirectionSpriteAsIdle = false;

    [Header("Direction Hold After Stop")]
    [SerializeField] private bool holdLookDirectionAfterStop = true;
    [SerializeField] private float holdRightDuration = 0.20f;
    [SerializeField] private float holdLeftDuration = 0.20f;
    [SerializeField] private float holdUpDuration = 0.20f;
    [SerializeField] private float holdDownDuration = 0.20f;

    [Header("Blink Settings")]
    [SerializeField] private bool enableBlink = true;
    [SerializeField] private float blinkInterval = 2.2f;
    [SerializeField] private float blinkDuration = 0.10f;
    [SerializeField] private bool randomizeBlinkInterval = true;
    [SerializeField] private float randomBlinkOffsetMin = 0.05f;
    [SerializeField] private float randomBlinkOffsetMax = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    public enum LookDirection
    {
        Down,
        Right,
        Up,
        Left
    }

    private LookDirection currentDirection = LookDirection.Down;

    private Coroutine blinkRoutine;
    private bool isBlinking;
    private float nextBlinkTime;

    private float lastMoveTime;
    private bool wasMovingLastFrame;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentDirection = startDirection;

        if (useDirectionSpriteAsIdle)
            SetDirectionSprite(currentDirection);
        else
            SetIdleSprite();

        ScheduleNextBlink();
    }

    private void Update()
    {
        UpdateLookDirectionFromInput();
        HandleBlink();
    }

    private void UpdateLookDirectionFromInput()
    {
        if (playerController == null)
            return;

        Vector2 input = playerController.GetInput();
        bool isMoving = input.magnitude >= movementThreshold;

        if (isMoving)
        {
            lastMoveTime = Time.time;

            LookDirection newDirection;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                newDirection = input.x > 0f ? LookDirection.Right : LookDirection.Left;
            }
            else
            {
                newDirection = input.y > 0f ? LookDirection.Up : LookDirection.Down;
            }

            if (newDirection != currentDirection)
            {
                currentDirection = newDirection;

                if (debugLogs)
                    Debug.Log($"[PlayerVisual] Direction changed to: {currentDirection}", this);
            }

            if (!isBlinking)
                SetDirectionSprite(currentDirection);

            wasMovingLastFrame = true;
            return;
        }

        if (holdLookDirectionAfterStop)
        {
            float holdDuration = GetCurrentDirectionHoldDuration();
            float timeSinceStop = Time.time - lastMoveTime;

            if (wasMovingLastFrame || timeSinceStop < holdDuration)
            {
                if (!isBlinking)
                    SetDirectionSprite(currentDirection);

                wasMovingLastFrame = false;
                return;
            }
        }

        if (!isBlinking)
        {
            if (useDirectionSpriteAsIdle)
                SetDirectionSprite(currentDirection);
            else
                SetIdleSprite();
        }

        wasMovingLastFrame = false;
    }

    private float GetCurrentDirectionHoldDuration()
    {
        switch (currentDirection)
        {
            case LookDirection.Right:
                return holdRightDuration;

            case LookDirection.Left:
                return holdLeftDuration;

            case LookDirection.Up:
                return holdUpDuration;

            case LookDirection.Down:
                return holdDownDuration;
        }

        return 0.20f;
    }

    private void HandleBlink()
    {
        if (!enableBlink)
            return;

        if (isBlinking)
            return;

        if (Time.time >= nextBlinkTime)
        {
            if (blinkRoutine != null)
                StopCoroutine(blinkRoutine);

            blinkRoutine = StartCoroutine(BlinkRoutine());
        }
    }

    private IEnumerator BlinkRoutine()
    {
        isBlinking = true;

        if (spriteRenderer != null && blinkSprite != null)
            spriteRenderer.sprite = blinkSprite;

        yield return new WaitForSeconds(blinkDuration);

        isBlinking = false;

        if (playerController != null)
        {
            Vector2 input = playerController.GetInput();
            bool isMoving = input.magnitude >= movementThreshold;

            if (isMoving)
            {
                SetDirectionSprite(currentDirection);
            }
            else if (holdLookDirectionAfterStop)
            {
                float holdDuration = GetCurrentDirectionHoldDuration();
                float timeSinceStop = Time.time - lastMoveTime;

                if (timeSinceStop < holdDuration)
                    SetDirectionSprite(currentDirection);
                else if (useDirectionSpriteAsIdle)
                    SetDirectionSprite(currentDirection);
                else
                    SetIdleSprite();
            }
            else
            {
                if (useDirectionSpriteAsIdle)
                    SetDirectionSprite(currentDirection);
                else
                    SetIdleSprite();
            }
        }
        else
        {
            if (useDirectionSpriteAsIdle)
                SetDirectionSprite(currentDirection);
            else
                SetIdleSprite();
        }

        ScheduleNextBlink();
    }

    private void ScheduleNextBlink()
    {
        float interval = blinkInterval;

        if (randomizeBlinkInterval)
            interval += Random.Range(randomBlinkOffsetMin, randomBlinkOffsetMax);

        nextBlinkTime = Time.time + interval;
    }

    private void SetIdleSprite()
    {
        if (spriteRenderer == null)
            return;

        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    private void SetDirectionSprite(LookDirection direction)
    {
        if (spriteRenderer == null)
            return;

        Sprite targetSprite = idleSprite;

        switch (direction)
        {
            case LookDirection.Right:
                targetSprite = lookRightSprite != null ? lookRightSprite : idleSprite;
                break;

            case LookDirection.Up:
                targetSprite = lookUpSprite != null ? lookUpSprite : idleSprite;
                break;

            case LookDirection.Left:
                targetSprite = lookLeftSprite != null ? lookLeftSprite : idleSprite;
                break;

            case LookDirection.Down:
                targetSprite = lookDownSprite != null ? lookDownSprite : idleSprite;
                break;
        }

        if (targetSprite != null)
            spriteRenderer.sprite = targetSprite;
    }
}