using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    [Header("Wall Blocking")]
    [Tooltip(
        "Насколько вертикальным должен быть контакт, " +
        "чтобы считаться стеной."
    )]
    [SerializeField, Range(0f, 1f)]
    private float minimumWallNormalX = 0.75f;

    [Header("Mobile Input (optional)")]
    [SerializeField] private MobileInput mobileInput;

    [Header("Timer (start on first move)")]
    [SerializeField] private LevelTimer levelTimer;

    [Header("Freeze after Win")]
    [SerializeField] private GameObject winPanel;

    [SerializeField]
    private bool freezeWhenWinPanelActive = true;

    [Header("Debug")]
    [SerializeField] private bool debugWallContacts;

    private Rigidbody2D rb;
    private Vector2 input;

    private bool timerNotified;
    private bool movementLocked;

    /*
     * Массив заранее создаётся один раз,
     * чтобы не создавать мусор в памяти
     * каждый физический кадр.
     */
    private readonly ContactPoint2D[] contacts =
        new ContactPoint2D[16];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (mobileInput == null)
        {
            mobileInput =
                FindFirstObjectByType<MobileInput>();
        }
    }

    private void Update()
    {
        if (freezeWhenWinPanelActive &&
            winPanel != null &&
            winPanel.activeInHierarchy)
        {
            LockMovement(true);
            return;
        }

        if (movementLocked)
            return;

        float moveX = ReadHorizontalInput();

        input = new Vector2(
            moveX,
            0f
        );

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

    private void FixedUpdate()
    {
        if (movementLocked)
            return;

        float desiredVelocityX =
            input.x * moveSpeed;

        bool touchingLeftWall = false;
        bool touchingRightWall = false;

        DetectWalls(
            ref touchingLeftWall,
            ref touchingRightWall
        );

        /*
         * Нормаль правой стены направлена влево.
         * Поэтому при движении вправо блокируем X.
         */
        if (desiredVelocityX > 0f &&
            touchingRightWall)
        {
            desiredVelocityX = 0f;
        }

        /*
         * Нормаль левой стены направлена вправо.
         * Поэтому при движении влево блокируем X.
         */
        if (desiredVelocityX < 0f &&
            touchingLeftWall)
        {
            desiredVelocityX = 0f;
        }

        /*
         * Меняем только горизонтальную скорость.
         * Вертикальная скорость сохраняется,
         * поэтому игрок свободно падает вдоль стены.
         */
        rb.linearVelocity = new Vector2(
            desiredVelocityX,
            rb.linearVelocity.y
        );
    }

    private float ReadHorizontalInput()
    {
        if (mobileInput != null)
        {
            return Mathf.Clamp(
                mobileInput.Horizontal,
                -1f,
                1f
            );
        }

        if (Keyboard.current == null)
            return 0f;

        bool moveLeft =
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed;

        bool moveRight =
            Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed;

        /*
         * Если одновременно нажаты обе стороны,
         * игрок не двигается.
         */
        if (moveLeft == moveRight)
            return 0f;

        return moveLeft ? -1f : 1f;
    }

    private void DetectWalls(
        ref bool touchingLeftWall,
        ref bool touchingRightWall
    )
    {
        int contactCount =
            rb.GetContacts(contacts);

        for (int i = 0;
             i < contactCount;
             i++)
        {
            Vector2 normal =
                contacts[i].normal;

            /*
             * Стена слева толкает игрока вправо:
             * normal.x положительный.
             */
            if (normal.x >=
                minimumWallNormalX)
            {
                touchingLeftWall = true;
            }

            /*
             * Стена справа толкает игрока влево:
             * normal.x отрицательный.
             */
            if (normal.x <=
                -minimumWallNormalX)
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

    public void LockMovement(
        bool locked
    )
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

    public Vector2 GetInput()
    {
        return input;
    }

    public float GetClimbVerticalInput()
    {
        float vertical = 0f;

        if (mobileInput != null)
        {
            vertical =
                mobileInput.Vertical;
        }
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
                vertical =
                    moveUp ? 1f : -1f;
            }
        }

        return Mathf.Clamp(
            vertical,
            -1f,
            1f
        );
    }

    private void OnValidate()
    {
        moveSpeed =
            Mathf.Max(
                0f,
                moveSpeed
            );
    }
}