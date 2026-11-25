
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

    private Object puzzleFinaleOwner;
    private RigidbodyConstraints2D constraintsBeforePuzzle;
    private bool awaitingPuzzleClickRelease;
    private bool awaitingPuzzleDirectionRelease;

    internal bool IsPuzzleFinaleLocked => puzzleFinaleOwner != null;
    internal bool PuzzleGameplayActionsBlocked => IsPuzzleFinaleLocked || awaitingPuzzleClickRelease;
    internal int PuzzleActionVersion { get; private set; }

    public bool IsMovementLocked =>
        movementLocked ||
        actionLocked ||
        IsPuzzleFinaleLocked;

    public bool IsActionLocked =>
        actionLocked || PuzzleGameplayActionsBlocked || PauseManager.IsMenuPaused;

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
        if (PauseManager.IsMenuPaused)
        {
            input = Vector2.zero;
            return;
        }

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
        if (IsPuzzleFinaleLocked)
        {
            StopBodyForPuzzle();
            return;
        }

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

        if (Keyboard.current == null || awaitingPuzzleDirectionRelease)
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
            horizontalMovementLocked ||
            IsPuzzleFinaleLocked)
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
        if (actionLocked || PuzzleGameplayActionsBlocked)
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

        else if (Keyboard.current != null && !awaitingPuzzleDirectionRelease)
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

    internal bool LockForPuzzleFinale(Object owner)
    {
        if (puzzleFinaleOwner != null)
            return puzzleFinaleOwner == owner;

        if (owner == null || rb == null)
            return false;

        puzzleFinaleOwner = owner;
        PuzzleActionVersion++;
        constraintsBeforePuzzle = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        ClearPuzzleDirections();
        StopBodyForPuzzle();
        return true;
    }

    internal void UnlockAfterPuzzleFinale(Object owner)
    {
        if (puzzleFinaleOwner != owner)
            return;

        ClearPuzzleDirections();
        awaitingPuzzleDirectionRelease = DirectionKeyPressed();
        awaitingPuzzleClickRelease = true;
        if (rb != null)
        {
            StopBodyForPuzzle();
            rb.constraints = constraintsBeforePuzzle;
        }
        puzzleFinaleOwner = null;
    }

    private void ClearPuzzleDirections()
    {
        input = Vector2.zero;
        if (mobileInput != null)
            mobileInput.ReleaseDirections();
        ClimbHook.SetClimbVerticalInput(0f);
    }

    private void StopBodyForPuzzle()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void LateUpdate()
    {
        if (awaitingPuzzleDirectionRelease && !DirectionKeyPressed())
            awaitingPuzzleDirectionRelease = false;

        // Keep the lock through the frame that delivers a held button's release event.
        if (awaitingPuzzleClickRelease && !GameplayClickPressed())
            awaitingPuzzleClickRelease = false;
    }

    private static bool DirectionKeyPressed()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard keyboard &&
                (keyboard.aKey.isPressed || keyboard.dKey.isPressed || keyboard.wKey.isPressed || keyboard.sKey.isPressed ||
                 keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed ||
                 keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed))
            {
                return true;
            }
        }

        return false;
    }

    private static bool GameplayClickPressed()
    {
        // A press can outlast a change of the current device (for example a mouse
        // click alongside an existing touch). Check each device still holding UI input.
        foreach (var device in InputSystem.devices)
        {
            switch (device)
            {
                case Mouse mouse when mouse.leftButton.isPressed:
                case Pen pen when pen.tip.isPressed:
                case Gamepad gamepad when gamepad.buttonSouth.isPressed:
                case Keyboard keyboard when keyboard.enterKey.isPressed ||
                    keyboard.numpadEnterKey.isPressed || keyboard.spaceKey.isPressed:
                    return true;
                case Touchscreen touchscreen:
                    foreach (var touch in touchscreen.touches)
                        if (touch.press.isPressed)
                            return true;
                    break;
            }
        }

        return false;
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
