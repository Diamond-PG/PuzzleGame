
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ============================================================
    // MOVE
    // ============================================================

    [Header("Move")]

    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    // ============================================================
    // WALL BLOCKING
    // ============================================================

    [Header("Wall Blocking")]

    [Tooltip(
        "Насколько вертикальным должен быть контакт, " +
        "чтобы считаться стеной."
    )]
    [SerializeField, Range(0f, 1f)]
    private float minimumWallNormalX = 0.75f;

    // ============================================================
    // MOBILE INPUT
    // ============================================================

    [Header("Mobile Input (optional)")]

    [SerializeField]
    private MobileInput mobileInput;

    // ============================================================
    // TIMER
    // ============================================================

    [Header("Timer (start on first move)")]

    [SerializeField]
    private LevelTimer levelTimer;

    // ============================================================
    // TUTORIAL
    // ============================================================

    [Header("Tutorial")]

    [Tooltip(
        "Контроллер обучения. " +
        "Нужен для фиксации движения влево/вправо."
    )]
    [SerializeField]
    private TutorialTextController tutorialTextController;

    // ============================================================
    // WIN
    // ============================================================

    [Header("Freeze after Win")]

    [SerializeField]
    private GameObject winPanel;

    [SerializeField]
    private bool freezeWhenWinPanelActive = true;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]

    [SerializeField]
    private bool debugWallContacts;

    // ============================================================
    // PRIVATE
    // ============================================================

    private Rigidbody2D rb;

    private Vector2 input;

    private bool timerNotified;

    /*
     * Существующая общая блокировка движения.
     * Сохраняем для других игровых систем.
     */
    private bool movementLocked;

    /*
     * Существующая блокировка действий.
     * Её не изменяем.
     */
    private bool actionLocked;

    /*
     * НОВОЕ:
     * Отдельная блокировка ходьбы влево и вправо.
     *
     * Не блокирует:
     * - прыжок;
     * - удар ногой;
     * - вертикальную физику;
     * - камеру;
     * - анимации.
     */
    private bool horizontalMovementLocked;

    private readonly ContactPoint2D[] contacts =
        new ContactPoint2D[16];

    // ============================================================
    // PUBLIC STATE
    // ============================================================

    public bool IsMovementLocked =>
        movementLocked ||
        actionLocked;

    public bool IsActionLocked =>
        actionLocked;

    public bool IsHorizontalMovementLocked =>
        horizontalMovementLocked;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (mobileInput == null)
        {
            mobileInput =
                FindFirstObjectByType<MobileInput>();
        }

        if (tutorialTextController == null)
        {
            tutorialTextController =
                FindFirstObjectByType<TutorialTextController>();
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        // ========================================================
        // WIN
        // ========================================================

        if (freezeWhenWinPanelActive &&
            winPanel != null &&
            winPanel.activeInHierarchy)
        {
            LockMovement(true);
            return;
        }

        // ========================================================
        // MOVEMENT LOCK
        // ========================================================

        if (IsMovementLocked ||
            horizontalMovementLocked)
        {
            input = Vector2.zero;
            return;
        }

        // ========================================================
        // CLIMB
        // ========================================================

        /*
         * Если герой уже зацепился за скобы,
         * горизонтальное управление не вмешивается.
         *
         * Rigidbody контролирует ClimbHook.
         */
        if (ClimbHook.PlayerIsOnHook)
        {
            input = Vector2.zero;
            return;
        }

        // ========================================================
        // NORMAL HORIZONTAL INPUT
        // ========================================================

        float moveX = ReadHorizontalInput();

        input = new Vector2(
            moveX,
            0f
        );

        // ========================================================
        // TUTORIAL HORIZONTAL INPUT
        // ========================================================

        if (tutorialTextController != null &&
            Mathf.Abs(moveX) > 0.001f)
        {
            tutorialTextController
                .ReportHorizontalTutorialInput(
                    moveX
                );
        }

        // ========================================================
        // TIMER
        // ========================================================

        if (!timerNotified &&
            Mathf.Abs(moveX) > 0.001f)
        {
            timerNotified = true;

            if (levelTimer != null)
            {
                levelTimer.NotifyPlayerMoved();
            }
        }
    }

    // ============================================================
    // FIXED UPDATE
    // ============================================================

    private void FixedUpdate()
    {
        // ========================================================
        // MOVEMENT LOCK
        // ========================================================

        if (IsMovementLocked ||
            horizontalMovementLocked)
        {
            /*
             * Останавливаем только движение по X.
             *
             * Скорость по Y сохраняется.
             * Гравитация и прыжок не отключаются.
             */
            if (rb != null)
            {
                rb.linearVelocity =
                    new Vector2(
                        0f,
                        rb.linearVelocity.y
                    );
            }

            return;
        }

        // ========================================================
        // CLIMB HAS FULL CONTROL
        // ========================================================

        if (ClimbHook.PlayerIsOnHook)
        {
            return;
        }

        // ========================================================
        // NORMAL MOVEMENT
        // ========================================================

        float desiredVelocityX =
            input.x * moveSpeed;

        bool touchingLeftWall = false;
        bool touchingRightWall = false;

        DetectWalls(
            ref touchingLeftWall,
            ref touchingRightWall
        );

        if (desiredVelocityX > 0f &&
            touchingRightWall)
        {
            desiredVelocityX = 0f;
        }

        if (desiredVelocityX < 0f &&
            touchingLeftWall)
        {
            desiredVelocityX = 0f;
        }

        rb.linearVelocity =
            new Vector2(
                desiredVelocityX,
                rb.linearVelocity.y
            );
    }

    // ============================================================
    // HORIZONTAL INPUT
    // ============================================================

    private float ReadHorizontalInput()
    {
        if (actionLocked ||
            horizontalMovementLocked)
        {
            return 0f;
        }

        // ========================================================
        // MOBILE
        // ========================================================

        if (mobileInput != null)
        {
            return Mathf.Clamp(
                mobileInput.Horizontal,
                -1f,
                1f
            );
        }

        // ========================================================
        // KEYBOARD
        // ========================================================

        if (Keyboard.current == null)
        {
            return 0f;
        }

        bool moveLeft =
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed;

        bool moveRight =
            Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed;

        if (moveLeft == moveRight)
        {
            return 0f;
        }

        return moveLeft ? -1f : 1f;
    }

    // ============================================================
    // WALLS
    // ============================================================

    private void DetectWalls(
        ref bool touchingLeftWall,
        ref bool touchingRightWall
    )
    {
        int contactCount =
            rb.GetContacts(contacts);

        for (int i = 0; i < contactCount; i++)
        {
            Vector2 normal =
                contacts[i].normal;

            if (normal.x >= minimumWallNormalX)
            {
                touchingLeftWall = true;
            }

            if (normal.x <= -minimumWallNormalX)
            {
                touchingRightWall = true;
            }
        }

        if (debugWallContacts &&
            (touchingLeftWall ||
             touchingRightWall))
        {
            Debug.Log(
                $"PlayerController: " +
                $"левая стена = {touchingLeftWall}, " +
                $"правая стена = {touchingRightWall}",
                this
            );
        }
    }

    // ============================================================
    // OLD MOVEMENT LOCK
    // ============================================================

    public void LockMovement(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            input = Vector2.zero;

            if (rb != null)
            {
                rb.linearVelocity =
                    Vector2.zero;
            }
        }
    }

    // ============================================================
    // TEMPORARY ACTION LOCK
    // ============================================================

    public void SetActionLock(bool locked)
    {
        actionLocked = locked;

        if (locked)
        {
            input = Vector2.zero;

            if (rb != null)
            {
                rb.linearVelocity =
                    Vector2.zero;
            }
        }
    }

    // ============================================================
    // NEW - HORIZONTAL MOVEMENT LOCK
    // ============================================================

    /*
     * Используется во время перехода
     * после разрушения тюремной двери.
     *
     * true  - запретить ходьбу влево/вправо.
     * false - вернуть обычное управление.
     *
     * Не меняет Time.timeScale.
     * Не выключает PlayerJump или PlayerKick.
     */
    public void SetHorizontalMovementLock(bool locked)
    {
        horizontalMovementLocked = locked;

        if (locked)
        {
            input = Vector2.zero;

            if (rb != null)
            {
                rb.linearVelocity =
                    new Vector2(
                        0f,
                        rb.linearVelocity.y
                    );
            }
        }
    }

    // ============================================================
    // GET NORMAL INPUT
    // ============================================================

    public Vector2 GetInput()
    {
        if (actionLocked ||
            horizontalMovementLocked)
        {
            return Vector2.zero;
        }

        return input;
    }

    // ============================================================
    // GET CLIMB INPUT
    // ============================================================

    public float GetClimbVerticalInput()
    {
        if (actionLocked)
        {
            return 0f;
        }

        float vertical = 0f;

        // ========================================================
        // MOBILE
        // ========================================================

        if (mobileInput != null)
        {
            vertical = mobileInput.Vertical;
        }

        // ========================================================
        // KEYBOARD
        // ========================================================

        else if (Keyboard.current != null)
        {
            bool moveUp =
                Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed;

            bool moveDown =
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed;

            if (moveUp != moveDown)
            {
                vertical = moveUp ? 1f : -1f;
            }
        }

        return Mathf.Clamp(
            vertical,
            -1f,
            1f
        );
    }

    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(
            0f,
            moveSpeed
        );
    }
}
