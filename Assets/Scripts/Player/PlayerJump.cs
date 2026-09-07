using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerVisual playerVisual;

    [Header("Ladder / Hook")]
    [Tooltip(
        "Если включено, кнопка прыжка не работает, " +
        "пока игрок держится за лестницу / hook."
    )]
    [SerializeField] private bool blockJumpWhileOnHook = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerVisual == null)
            playerVisual = GetComponent<PlayerVisual>();
    }

    public void Jump()
    {
        /*
         * ВАЖНО:
         * кнопка Jump больше НЕ управляет подъёмом.
         *
         * Если игрок находится на лестнице / hook,
         * прыжок просто игнорируется.
         *
         * Подъём и спуск выполняются только
         * стрелками Up / Down.
         */
        if (blockJumpWhileOnHook &&
            ClimbHook.PlayerIsOnHook)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "PlayerJump: Jump blocked while player is on hook.",
                    this
                );
            }

            return;
        }

        /*
         * Прыгать можно только с земли.
         */
        if (!IsGrounded())
            return;

        if (rb == null)
            return;

        /*
         * Обнуляем старую вертикальную скорость,
         * чтобы высота прыжка была стабильной.
         */
        rb.linearVelocity =
            new Vector2(
                rb.linearVelocity.x,
                0f
            );

        rb.AddForce(
            Vector2.up * jumpForce,
            ForceMode2D.Impulse
        );

        if (playerVisual != null)
        {
            playerVisual.PlayJumpLookUp();
        }

        if (debugLogs)
        {
            Debug.Log(
                "PlayerJump: Jump!",
                this
            );
        }
    }

    public bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}