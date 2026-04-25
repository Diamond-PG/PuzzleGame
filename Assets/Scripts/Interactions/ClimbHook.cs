using UnityEngine;

public class ClimbHook : MonoBehaviour
{
    public static bool PlayerIsOnHook { get; private set; }

    private static float climbVerticalInput;

    public static void SetClimbVerticalInput(float value)
    {
        climbVerticalInput = Mathf.Clamp(value, -1f, 1f);
    }

    [Header("Climb Settings")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private bool disableGravityWhileClimbing = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool playerInside;
    private Rigidbody2D playerRb;
    private PlayerController playerController;
    private PlayerVisual playerVisual;

    private float originalGravityScale;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerRb = collision.GetComponent<Rigidbody2D>();
        playerController = collision.GetComponent<PlayerController>();
        playerVisual = collision.GetComponent<PlayerVisual>();

        if (playerRb == null || playerController == null)
            return;

        originalGravityScale = playerRb.gravityScale;

        playerInside = true;
        PlayerIsOnHook = true;
        climbVerticalInput = 0f;

        if (disableGravityWhileClimbing)
            playerRb.gravityScale = 0f;

        playerRb.linearVelocity = Vector2.zero;

        if (playerVisual != null)
            playerVisual.SetClimbLook(0f);

        if (debugLogs)
            Debug.Log("ClimbHook: игрок вошёл в скобу", this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (playerRb != null)
        {
            playerRb.gravityScale = originalGravityScale;
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        }

        if (playerVisual != null)
            playerVisual.ClearClimbLook();

        playerInside = false;
        PlayerIsOnHook = false;
        climbVerticalInput = 0f;

        playerRb = null;
        playerController = null;
        playerVisual = null;

        if (debugLogs)
            Debug.Log("ClimbHook: игрок вышел из скобы", this);
    }

    private void FixedUpdate()
    {
        if (!playerInside || playerRb == null || playerController == null)
            return;

        float vertical = climbVerticalInput;

        if (Mathf.Abs(vertical) < 0.1f)
            vertical = playerController.GetClimbVerticalInput();

        if (Mathf.Abs(vertical) > 0.1f)
        {
            playerRb.linearVelocity = new Vector2(0f, vertical * climbSpeed);

            if (playerVisual != null)
                playerVisual.SetClimbLook(vertical);
        }
        else
        {
            playerRb.linearVelocity = Vector2.zero;
        }
    }
}