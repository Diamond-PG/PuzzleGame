using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    // ============================================================
    // JUMP SETTINGS
    // ============================================================

    [Header("Jump Settings")]

    [SerializeField]
    private float jumpForce = 7f;

    [SerializeField]
    private Transform groundCheck;

    [SerializeField]
    private float groundCheckRadius = 0.12f;

    [SerializeField]
    private LayerMask groundLayer;

    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("References")]

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private PlayerVisual playerVisual;

    [SerializeField]
    private PlayerController playerController;

    // ============================================================
    // JUMP AUDIO
    // ============================================================

    [Header("Jump Audio")]

    [Tooltip(
        "Отдельный AudioSource для звуков прыжка. " +
        "Можно оставить None — он создастся автоматически."
    )]
    [SerializeField]
    private AudioSource jumpAudioSource;

    [SerializeField]
    private bool useJumpAudio = true;

    [Tooltip(
        "Основной звуковой эффект прыжка."
    )]
    [SerializeField]
    private AudioClip jumpMovementSound;

    [Tooltip(
        "Короткий голосовой звук героя, например мягкое 'Хы'."
    )]
    [SerializeField]
    private AudioClip jumpVoiceSound;

    [Tooltip(
        "Громкость основного эффекта прыжка."
    )]
    [SerializeField, Range(0f, 1f)]
    private float jumpMovementVolume = 0.75f;

    [Tooltip(
        "Громкость голосового звука героя."
    )]
    [SerializeField, Range(0f, 1f)]
    private float jumpVoiceVolume = 0.30f;

    // ============================================================
    // JUMP HAPTICS
    // ============================================================

    [Header("Jump Haptics")]

    [SerializeField]
    private bool useJumpHaptics = true;

    [SerializeField, Range(5, 100)]
    private int jumpHapticMs = 18;

    // ============================================================
    // LADDER / HOOK
    // ============================================================

    [Header("Ladder / Hook")]

    [SerializeField]
    private bool blockJumpWhileOnHook = true;

    [SerializeField]
    private bool allowJumpFromTopOfHook = true;

    [Tooltip(
        "Отдельная сила прыжка с верхней скобы."
    )]
    [SerializeField, Min(0f)]
    private float hookJumpForce = 8.5f;

    [Tooltip(
        "После прыжка со скобы принудительно " +
        "возвращать обычную гравитацию Player."
    )]
    [SerializeField]
    private bool restoreGravityOnHookJump = true;

    // ============================================================
    // JUMP STATE
    // ============================================================

    [Header("Jump State")]

    [SerializeField]
    private float leaveGroundTimeout = 0.15f;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]

    [SerializeField]
    private bool debugLogs = false;

    // ============================================================
    // PRIVATE
    // ============================================================

    private bool jumpInProgress;
    private bool hasLeftGround;

    private bool currentJumpStartedFromHook;

    private float jumpStartedTime;

    /*
     * Обычная Gravity Scale Player.
     * Запоминаем при старте сцены.
     */
    private float normalGravityScale = 1f;

    // ============================================================
    // PUBLIC
    // ============================================================

    public bool IsJumpInProgress =>
        jumpInProgress;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        if (playerVisual == null)
        {
            playerVisual =
                GetComponent<PlayerVisual>();
        }

        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }

        /*
         * Если отдельный AudioSource не назначен,
         * создаём его автоматически на Player.
         */
        if (jumpAudioSource == null)
        {
            jumpAudioSource =
                gameObject.AddComponent<AudioSource>();

            jumpAudioSource.playOnAwake =
                false;

            jumpAudioSource.loop =
                false;

            jumpAudioSource.spatialBlend =
                0f;
        }

        /*
         * На Player сейчас Gravity Scale = 3.
         * Именно это значение здесь запомнится.
         */
        if (rb != null)
        {
            normalGravityScale =
                rb.gravityScale;
        }

        /*
         * На случай перезапуска сцены
         * снимаем старую статическую блокировку.
         */
        ClimbHook.ResetHookJumpCatchLock();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        UpdateJumpState();
    }

    // ============================================================
    // JUMP
    // ============================================================

    public void Jump()
    {
        if (playerVisual != null &&
            playerVisual.GameplayActionsLocked)
        {
            return;
        }

        if (playerController != null &&
            playerController.IsActionLocked)
        {
            return;
        }

        // ========================================================
        // HOOK JUMP
        // ========================================================

        if (ClimbHook.PlayerIsOnHook)
        {
            if (allowJumpFromTopOfHook &&
                ClimbHook
                    .TryReleaseActiveHookForJump())
            {
                /*
                 * Дополнительная страховка.
                 * Даже если ClimbHook по какой-то причине
                 * оставил Gravity Scale = 0,
                 * здесь возвращаем нормальную.
                 */
                if (restoreGravityOnHookJump &&
                    rb != null)
                {
                    rb.gravityScale =
                        normalGravityScale;
                }

                StartRealJump(
                    hookJumpForce,
                    true
                );

                return;
            }

            if (blockJumpWhileOnHook)
            {
                if (debugLogs)
                {
                    Debug.Log(
                        "PlayerJump: blocked - player is on hook but not at TopStop.",
                        this
                    );
                }

                return;
            }
        }

        // ========================================================
        // NORMAL JUMP
        // ========================================================

        if (!IsGrounded())
        {
            return;
        }

        StartRealJump(
            jumpForce,
            false
        );
    }

    // ============================================================
    // START REAL JUMP
    // ============================================================

    private void StartRealJump(
        float force,
        bool fromHook
    )
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity =
            new Vector2(
                rb.linearVelocity.x,
                0f
            );

        rb.AddForce(
            Vector2.up *
            force,
            ForceMode2D.Impulse
        );

        jumpInProgress =
            true;

        currentJumpStartedFromHook =
            fromHook;

        /*
         * Со скобы Player уже в воздухе.
         */
        hasLeftGround =
            fromHook;

        jumpStartedTime =
            Time.time;

        if (playerVisual != null)
        {
            playerVisual.PlayJumpLookUp();
        }

        /*
         * Одновременно запускаем:
         * 1. Основной эффект прыжка.
         * 2. Голосовое "Хы".
         */
        PlayJumpSounds();

        PlayJumpHaptic();

        if (debugLogs)
        {
            Debug.Log(
                fromHook
                    ? $"PlayerJump: HOOK JUMP | Force = {force:F2} | Gravity = {rb.gravityScale:F2}"
                    : $"PlayerJump: NORMAL JUMP | Force = {force:F2}",
                this
            );
        }
    }

    // ============================================================
    // JUMP AUDIO
    // ============================================================

    private void PlayJumpSounds()
    {
        if (!useJumpAudio ||
            jumpAudioSource == null)
        {
            return;
        }

        /*
         * PlayOneShot позволяет двум звукам
         * воспроизводиться одновременно.
         */
        if (jumpMovementSound != null)
        {
            jumpAudioSource.PlayOneShot(
                jumpMovementSound,
                Mathf.Clamp01(
                    jumpMovementVolume
                )
            );
        }

        if (jumpVoiceSound != null)
        {
            jumpAudioSource.PlayOneShot(
                jumpVoiceSound,
                Mathf.Clamp01(
                    jumpVoiceVolume
                )
            );
        }
    }

    // ============================================================
    // UPDATE STATE
    // ============================================================

    private void UpdateJumpState()
    {
        if (!jumpInProgress)
        {
            return;
        }

        bool grounded =
            IsGrounded();

        // ========================================================
        // WAIT UNTIL PLAYER LEAVES GROUND
        // ========================================================

        if (!hasLeftGround)
        {
            if (!grounded)
            {
                hasLeftGround =
                    true;
            }
            else if (Time.time -
                     jumpStartedTime >
                     leaveGroundTimeout)
            {
                if (rb != null &&
                    rb.linearVelocity.y > 0.05f)
                {
                    hasLeftGround =
                        true;
                }
            }

            return;
        }

        // ========================================================
        // LANDING
        // ========================================================

        if (grounded &&
            rb != null &&
            rb.linearVelocity.y <= 0.1f)
        {
            jumpInProgress =
                false;

            hasLeftGround =
                false;

            if (currentJumpStartedFromHook)
            {
                /*
                 * Теперь после настоящего приземления
                 * снова разрешаем цепляться за скобы.
                 */
                ClimbHook.NotifyHookJumpLanded();
            }

            currentJumpStartedFromHook =
                false;

            if (debugLogs)
            {
                Debug.Log(
                    "PlayerJump: jump finished.",
                    this
                );
            }
        }
    }

    // ============================================================
    // HAPTIC
    // ============================================================

    private void PlayJumpHaptic()
    {
        if (!useJumpHaptics)
        {
            return;
        }

        MicroHaptics.Pulse(
            jumpHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }

    // ============================================================
    // GROUNDED
    // ============================================================

    public bool IsGrounded()
    {
        if (groundCheck == null)
        {
            return false;
        }

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // ============================================================
    // GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }

    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        jumpForce =
            Mathf.Max(
                0f,
                jumpForce
            );

        hookJumpForce =
            Mathf.Max(
                0f,
                hookJumpForce
            );

        groundCheckRadius =
            Mathf.Max(
                0.01f,
                groundCheckRadius
            );

        leaveGroundTimeout =
            Mathf.Max(
                0f,
                leaveGroundTimeout
            );

        jumpMovementVolume =
            Mathf.Clamp01(
                jumpMovementVolume
            );

        jumpVoiceVolume =
            Mathf.Clamp01(
                jumpVoiceVolume
            );
    }
}