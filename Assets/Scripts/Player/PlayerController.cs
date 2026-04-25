using UnityEngine;
using UnityEngine.InputSystem;

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

        if (mobileInput != null)
        {
            moveX = mobileInput.Horizontal;
        }
        else
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    moveX = -1f;

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    moveX = 1f;
            }
        }

        input = new Vector2(moveX, 0f).normalized;

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

        rb.linearVelocity = new Vector2(input.x * moveSpeed, rb.linearVelocity.y);
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

    public float GetClimbVerticalInput()
    {
        float vertical = 0f;

        if (mobileInput != null)
        {
            vertical = mobileInput.Vertical;
        }
        else
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    vertical = 1f;

                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    vertical = -1f;
            }
        }

        return vertical;
    }
}