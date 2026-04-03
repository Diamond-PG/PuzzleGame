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

        // Если забыли привязать mobileInput в Inspector — попробуем найти сами
        if (mobileInput == null)
            mobileInput = FindFirstObjectByType<MobileInput>();
    }

    private void Update()
    {
        // 1) Если победная панель активна — стоп управление
        if (freezeWhenWinPanelActive && winPanel != null && winPanel.activeInHierarchy)
        {
            LockMovement(true);
            return;
        }

        // 2) Если движение заблокировано вручную — тоже стоп
        if (movementLocked)
            return;

        float moveX = 0f;
        float moveY = 0f;

        // Мобилка
        if (mobileInput != null)
        {
            moveX = mobileInput.Horizontal;
            moveY = mobileInput.Vertical;
        }
        else
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");
#else
            moveX = 0f;
            moveY = 0f;
#endif
        }

        // Нормализуем, чтобы по диагонали скорость не была больше
        input = new Vector2(moveX, moveY).normalized;

        // Запуск таймера при первом движении
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

        Vector2 newPos = rb.position + input * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    public void LockMovement(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            input = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public Vector2 GetInput()
    {
        return input;
    }
}