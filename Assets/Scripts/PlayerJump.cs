using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.05f;
    public Vector2 groundCheckSize = new Vector2(0.35f, 0.06f);

    [Header("Visual")]
    [SerializeField] private PlayerVisual playerVisual;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool jumpLocked;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (playerVisual == null)
            playerVisual = GetComponent<PlayerVisual>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }

        if (jumpLocked && IsGrounded() && rb.linearVelocity.y <= 0.05f)
        {
            jumpLocked = false;
        }
    }

    public void Jump()
    {
        if (jumpLocked)
            return;

        if (!IsGrounded())
            return;

        jumpLocked = true;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (playerVisual != null)
            playerVisual.PlayJumpLookUp();
    }

    private bool IsGrounded()
    {
        Vector2 origin = boxCollider.bounds.center;
        origin.y = boxCollider.bounds.min.y - groundCheckDistance;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            groundCheckSize,
            0f,
            Vector2.down,
            0.02f,
            groundLayer
        );

        return hit.collider != null;
    }

    void OnDrawGizmosSelected()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider == null) return;

        Gizmos.color = Color.green;

        Vector2 origin = boxCollider.bounds.center;
        origin.y = boxCollider.bounds.min.y - groundCheckDistance;

        Gizmos.DrawWireCube(origin, groundCheckSize);
    }
}