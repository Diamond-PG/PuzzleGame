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
    [SerializeField] private Sprite hurtSprite;

    [Header("Kick Sprites")]
    [SerializeField] private Sprite kickRightSprite;
    [SerializeField] private Sprite kickLeftSprite;

    [Header("Look Settings")]
    [SerializeField] private float movementThreshold = 0.05f;
    [SerializeField] private float returnToIdleDelay = 0.20f;

    [Header("Jump / Fall Look")]
    [SerializeField] private float jumpLookUpVelocity = 0.15f;
    [SerializeField] private float fallLookDownVelocity = -0.15f;

    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 2.2f;
    [SerializeField] private float blinkDuration = 0.1f;

    [Header("Hurt Settings")]
    [SerializeField] private float hurtDuration = 0.45f;

    private enum LookDirection
    {
        Idle,
        Down,
        Right,
        Up,
        Left
    }

    private LookDirection currentDirection = LookDirection.Idle;

    private bool isBlinking;
    private bool isHurt;
    private bool isClimbing;
    private bool isKicking;

    private float nextBlinkTime;
    private float lastInputTime;

    private Coroutine blinkRoutine;
    private Coroutine hurtRoutine;

    public bool IsKicking => isKicking;

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
        currentDirection = LookDirection.Idle;

        SetIdleSprite();
        ScheduleBlink();
    }

    private void Update()
    {
        if (isHurt || isKicking)
            return;

        UpdateLookDirection();
        HandleBlink();
    }

    private void UpdateLookDirection()
    {
        if (isClimbing)
            return;

        if (rb != null)
        {
            if (rb.linearVelocity.y > jumpLookUpVelocity)
            {
                currentDirection = LookDirection.Up;

                if (!isBlinking)
                    SetDirectionSprite(currentDirection);

                return;
            }

            if (rb.linearVelocity.y < fallLookDownVelocity)
            {
                currentDirection = LookDirection.Down;

                if (!isBlinking)
                    SetDirectionSprite(currentDirection);

                return;
            }
        }

        if (playerController == null)
            return;

        Vector2 input = playerController.GetInput();

        if (input.magnitude > movementThreshold)
        {
            lastInputTime = Time.time;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                currentDirection =
                    input.x > 0
                        ? LookDirection.Right
                        : LookDirection.Left;
            }
            else
            {
                currentDirection =
                    input.y > 0
                        ? LookDirection.Up
                        : LookDirection.Down;
            }

            if (!isBlinking)
                SetDirectionSprite(currentDirection);

            return;
        }

        if (Time.time - lastInputTime >= returnToIdleDelay)
        {
            currentDirection = LookDirection.Idle;

            if (!isBlinking)
                SetIdleSprite();
        }
    }

    private void HandleBlink()
    {
        if (isBlinking)
            return;

        if (Time.time < nextBlinkTime)
            return;

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        isBlinking = true;

        if (blinkSprite != null &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite = blinkSprite;
        }

        yield return new WaitForSeconds(blinkDuration);

        isBlinking = false;

        if (!isHurt && !isKicking)
        {
            if (currentDirection == LookDirection.Idle)
                SetIdleSprite();
            else
                SetDirectionSprite(currentDirection);
        }

        ScheduleBlink();
        blinkRoutine = null;
    }

    private void ScheduleBlink()
    {
        nextBlinkTime =
            Time.time +
            blinkInterval +
            Random.Range(0.1f, 0.5f);
    }

    private void SetIdleSprite()
    {
        if (spriteRenderer != null &&
            idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }

    private void SetDirectionSprite(
        LookDirection direction
    )
    {
        if (spriteRenderer == null)
            return;

        Sprite targetSprite = idleSprite;

        switch (direction)
        {
            case LookDirection.Right:
                targetSprite = lookRightSprite;
                break;

            case LookDirection.Left:
                targetSprite = lookLeftSprite;
                break;

            case LookDirection.Up:
                targetSprite = lookUpSprite;
                break;

            case LookDirection.Down:
                targetSprite = lookDownSprite;
                break;

            case LookDirection.Idle:
                targetSprite = idleSprite;
                break;
        }

        if (targetSprite != null)
            spriteRenderer.sprite = targetSprite;
    }

    public void PlayKickRight()
    {
        PlayKick(true);
    }

    public void PlayKickLeft()
    {
        PlayKick(false);
    }

    public void PlayKick(bool kickRight)
    {
        if (spriteRenderer == null)
            return;

        if (isHurt)
            return;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        isBlinking = false;
        isKicking = true;

        Sprite kickSprite =
            kickRight
                ? kickRightSprite
                : kickLeftSprite;

        if (kickSprite != null)
            spriteRenderer.sprite = kickSprite;
    }

    public void EndKick()
    {
        if (!isKicking)
            return;

        isKicking = false;

        currentDirection = LookDirection.Idle;
        SetIdleSprite();

        ScheduleBlink();
    }

    public void PlayJumpLookUp()
    {
        if (isKicking || isHurt)
            return;

        currentDirection = LookDirection.Up;

        if (!isBlinking)
            SetDirectionSprite(LookDirection.Up);
    }

    public void PlayHurtVisual()
    {
        if (hurtRoutine != null)
            StopCoroutine(hurtRoutine);

        hurtRoutine = StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        isKicking = false;
        isBlinking = false;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (hurtSprite != null &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite = hurtSprite;
        }

        yield return new WaitForSeconds(hurtDuration);

        isHurt = false;

        currentDirection = LookDirection.Idle;
        SetIdleSprite();

        ScheduleBlink();

        hurtRoutine = null;
    }

    public void SetClimbLook(float vertical)
    {
        if (isKicking || isHurt)
            return;

        isClimbing = true;

        if (vertical > 0.1f)
            currentDirection = LookDirection.Up;
        else if (vertical < -0.1f)
            currentDirection = LookDirection.Down;

        if (!isBlinking)
            SetDirectionSprite(currentDirection);
    }

    public void ClearClimbLook()
    {
        isClimbing = false;

        if (isKicking || isHurt)
            return;

        currentDirection = LookDirection.Idle;
        SetIdleSprite();
    }
}