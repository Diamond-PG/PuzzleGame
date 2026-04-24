using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    [Header("Mobile Input (optional)")]
    [SerializeField] private MobileInput mobileInput;

    [Header("Timer (start on first move)")]
    [SerializeField] private LevelTimer levelTimer;
    private bool timerNotified;

    [Header("Freeze after Win")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool freezeWhenWinPanelActive = true;

    private bool movementLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (mobileInput == null)
            mobileInput = FindFirstObjectByType<MobileInput>();
    }

    private void Update()
    {
        if (freezeWhenWinPanelActive && winPanel != null && winPanel.activeInHierarchy)
        {
            LockMovement(true);
            return;
        }

        if (movementLocked)
            return;

        float moveX = 0f;
        float moveY = 0f;

        // Мобильное управление
        if (mobileInput != null)
        {
            moveX = mobileInput.Horizontal;

            // ВАЖНО:
            // Вертикальное движение отключаем,
            // потому что кнопка вверх теперь отвечает за прыжок.
            moveY = 0f;
        }
        else
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            moveX = Input.GetAxisRaw("Horizontal");

            // ВАЖНО:
            // Вертикаль отключена, чтобы не мешала прыжку.
            moveY = 0f;
#else
            moveX = 0f;
            moveY = 0f;
#endif
        }

        input = new Vector2(moveX, moveY).normalized;

        if (!timerNotified && input.sqrMagnitude > 0.0001f)
        {
            timerNotified = true;

            if (levelTimer != null)
                levelTimer.NotifyPlayerMoved();
        }
    }

    private void FixedUpdate()
    {
        if (movementLocked)
            return;

        // ВАЖНО:
        // Меняем только X-скорость.
        // Y-скорость оставляем как есть, чтобы прыжок и гравитация работали.
        rb.linearVelocity = new Vector2(input.x * moveSpeed, rb.linearVelocity.y);
    }

    public void LockMovement(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            input = Vector2.zero;

            // ВАЖНО:
            // При блокировке полностью останавливаем игрока.
            rb.linearVelocity = Vector2.zero;
        }
    }

    public Vector2 GetInput()
    {
        return input;
    }
}