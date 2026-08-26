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

    private LookDirection currentDirection =
        LookDirection.Idle;

    private bool isBlinking;
    private bool isHurt;
    private bool isClimbing;
    private bool isKicking;

    private float nextBlinkTime;
    private float lastInputTime;

    private Sprite activeKickSprite;

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
        currentDirection =
            LookDirection.Idle;

        SetIdleSprite();
        ScheduleBlink();
    }

    private void Update()
    {
        /*
         * Пока игрок бьёт ногой,
         * никакая обычная визуальная логика
         * не должна перебивать Kick Sprite.
         */
        if (isKicking)
            return;

        if (isHurt)
            return;

        UpdateLookDirection();
        HandleBlink();
    }

    private void LateUpdate()
    {
        /*
         * Даже если какой-либо другой скрипт
         * попытается поменять спрайт во время удара,
         * Kick Sprite снова ставится в конце кадра.
         *
         * При этом PlayerHealth всё ещё может
         * включать/выключать SpriteRenderer,
         * поэтому моргание после урона продолжится.
         */
        if (!isKicking)
            return;

        if (spriteRenderer == null)
            return;

        if (activeKickSprite == null)
            return;

        if (spriteRenderer.sprite !=
            activeKickSprite)
        {
            spriteRenderer.sprite =
                activeKickSprite;
        }
    }

    private void UpdateLookDirection()
    {
        if (isClimbing)
            return;

        if (rb != null)
        {
            if (rb.linearVelocity.y >
                jumpLookUpVelocity)
            {
                currentDirection =
                    LookDirection.Up;

                if (!isBlinking)
                {
                    SetDirectionSprite(
                        currentDirection
                    );
                }

                return;
            }

            if (rb.linearVelocity.y <
                fallLookDownVelocity)
            {
                currentDirection =
                    LookDirection.Down;

                if (!isBlinking)
                {
                    SetDirectionSprite(
                        currentDirection
                    );
                }

                return;
            }
        }

        if (playerController == null)
            return;

        Vector2 input =
            playerController.GetInput();

        if (input.magnitude >
            movementThreshold)
        {
            lastInputTime =
                Time.time;

            if (Mathf.Abs(input.x) >
                Mathf.Abs(input.y))
            {
                currentDirection =
                    input.x > 0f
                        ? LookDirection.Right
                        : LookDirection.Left;
            }
            else
            {
                currentDirection =
                    input.y > 0f
                        ? LookDirection.Up
                        : LookDirection.Down;
            }

            if (!isBlinking)
            {
                SetDirectionSprite(
                    currentDirection
                );
            }

            return;
        }

        if (Time.time -
            lastInputTime >=
            returnToIdleDelay)
        {
            currentDirection =
                LookDirection.Idle;

            if (!isBlinking)
            {
                SetIdleSprite();
            }
        }
    }

    private void HandleBlink()
    {
        if (isBlinking)
            return;

        if (Time.time <
            nextBlinkTime)
        {
            return;
        }

        blinkRoutine =
            StartCoroutine(
                BlinkRoutine()
            );
    }

    private IEnumerator BlinkRoutine()
    {
        isBlinking = true;

        if (blinkSprite != null &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite =
                blinkSprite;
        }

        yield return new WaitForSeconds(
            blinkDuration
        );

        isBlinking = false;

        if (!isHurt &&
            !isKicking)
        {
            RestoreCurrentSprite();
        }

        ScheduleBlink();

        blinkRoutine = null;
    }

    private void ScheduleBlink()
    {
        nextBlinkTime =
            Time.time +
            blinkInterval +
            Random.Range(
                0.1f,
                0.5f
            );
    }

    private void SetIdleSprite()
    {
        if (spriteRenderer != null &&
            idleSprite != null)
        {
            spriteRenderer.sprite =
                idleSprite;
        }
    }

    private void SetDirectionSprite(
        LookDirection direction
    )
    {
        if (spriteRenderer == null)
            return;

        Sprite targetSprite =
            idleSprite;

        switch (direction)
        {
            case LookDirection.Right:
                targetSprite =
                    lookRightSprite;
                break;

            case LookDirection.Left:
                targetSprite =
                    lookLeftSprite;
                break;

            case LookDirection.Up:
                targetSprite =
                    lookUpSprite;
                break;

            case LookDirection.Down:
                targetSprite =
                    lookDownSprite;
                break;

            case LookDirection.Idle:
                targetSprite =
                    idleSprite;
                break;
        }

        if (targetSprite != null)
        {
            spriteRenderer.sprite =
                targetSprite;
        }
    }

    private void RestoreCurrentSprite()
    {
        if (currentDirection ==
            LookDirection.Idle)
        {
            SetIdleSprite();
        }
        else
        {
            SetDirectionSprite(
                currentDirection
            );
        }
    }

    public void PlayKickRight()
    {
        PlayKick(true);
    }

    public void PlayKickLeft()
    {
        PlayKick(false);
    }

    public void PlayKick(
        bool kickRight
    )
    {
        if (spriteRenderer == null)
            return;

        /*
         * ВАЖНО:
         * раньше удар блокировался,
         * если isHurt == true.
         *
         * Теперь игрок может начать удар
         * даже во время визуальной реакции
         * на полученный урон.
         */

        /*
         * Если сейчас отображается Hurt Sprite,
         * останавливаем только эту визуальную
         * реакцию.
         *
         * Моргание от PlayerHealth НЕ трогаем.
         * Оно продолжит включать и выключать
         * SpriteRenderer как раньше.
         */
        if (hurtRoutine != null)
        {
            StopCoroutine(
                hurtRoutine
            );

            hurtRoutine = null;
        }

        isHurt = false;

        /*
         * Обычное автоматическое моргание глаз
         * PlayerVisual во время удара нам не нужно.
         */
        if (blinkRoutine != null)
        {
            StopCoroutine(
                blinkRoutine
            );

            blinkRoutine = null;
        }

        isBlinking = false;
        isKicking = true;

        activeKickSprite =
            kickRight
                ? kickRightSprite
                : kickLeftSprite;

        if (activeKickSprite != null)
        {
            /*
             * Не принуждаем enabled = true.
             *
             * Это важно:
             * если PlayerHealth сейчас
             * делает мигание после урона,
             * он сам управляет enabled.
             *
             * Поэтому Kick Sprite будет
             * моргать вместе с игроком.
             */
            spriteRenderer.sprite =
                activeKickSprite;
        }
        else
        {
            Debug.LogWarning(
                kickRight
                    ? "Kick Right Sprite не назначен в PlayerVisual!"
                    : "Kick Left Sprite не назначен в PlayerVisual!"
            );
        }
    }

    public void EndKick()
    {
        if (!isKicking)
            return;

        isKicking = false;
        activeKickSprite = null;

        currentDirection =
            LookDirection.Idle;

        SetIdleSprite();
        ScheduleBlink();
    }

    public void PlayJumpLookUp()
    {
        if (isKicking ||
            isHurt)
        {
            return;
        }

        currentDirection =
            LookDirection.Up;

        if (!isBlinking)
        {
            SetDirectionSprite(
                LookDirection.Up
            );
        }
    }

    public void PlayHurtVisual()
    {
        /*
         * Если игрок уже сам начал удар,
         * Hurt Sprite не перебивает ногу.
         *
         * Само моргание/неуязвимость
         * по-прежнему делает PlayerHealth.
         */
        if (isKicking)
            return;

        if (hurtRoutine != null)
        {
            StopCoroutine(
                hurtRoutine
            );
        }

        hurtRoutine =
            StartCoroutine(
                HurtRoutine()
            );
    }

    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        isBlinking = false;

        if (blinkRoutine != null)
        {
            StopCoroutine(
                blinkRoutine
            );

            blinkRoutine = null;
        }

        if (hurtSprite != null &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite =
                hurtSprite;
        }

        yield return new WaitForSeconds(
            hurtDuration
        );

        isHurt = false;

        /*
         * За время Hurt игрок мог начать удар.
         * В таком случае ничего не перебиваем.
         */
        if (!isKicking)
        {
            currentDirection =
                LookDirection.Idle;

            SetIdleSprite();
            ScheduleBlink();
        }

        hurtRoutine = null;
    }

    public void SetClimbLook(
        float vertical
    )
    {
        if (isKicking ||
            isHurt)
        {
            return;
        }

        isClimbing = true;

        if (vertical > 0.1f)
        {
            currentDirection =
                LookDirection.Up;
        }
        else if (vertical < -0.1f)
        {
            currentDirection =
                LookDirection.Down;
        }

        if (!isBlinking)
        {
            SetDirectionSprite(
                currentDirection
            );
        }
    }

    public void ClearClimbLook()
    {
        isClimbing = false;

        if (isKicking ||
            isHurt)
        {
            return;
        }

        currentDirection =
            LookDirection.Idle;

        SetIdleSprite();
    }
}