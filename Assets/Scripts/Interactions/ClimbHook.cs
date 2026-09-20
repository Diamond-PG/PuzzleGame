using System.Collections.Generic;
using UnityEngine;

public class ClimbHook : MonoBehaviour
{
    // ============================================================
    // STATIC STATE
    // ============================================================

    public static bool PlayerIsOnHook { get; private set; }

    private static float climbVerticalInput;
    private static ClimbHook activeClimbZone;

    /*
     * После прыжка с верхней скобы
     * запрещаем повторное автоматическое
     * цепляние до приземления.
     */
    private static bool hookJumpCatchLocked;

    public static void SetClimbVerticalInput(float value)
    {
        climbVerticalInput =
            Mathf.Clamp(
                value,
                -1f,
                1f
            );
    }

    // ============================================================
    // HOOK JUMP PUBLIC
    // ============================================================

    public static bool TryReleaseActiveHookForJump()
    {
        if (activeClimbZone == null)
        {
            return false;
        }

        return
            activeClimbZone
                .ReleaseFromTopForJump();
    }

    public static void NotifyHookJumpLanded()
    {
        hookJumpCatchLocked =
            false;
    }

    public static void ResetHookJumpCatchLock()
    {
        hookJumpCatchLocked =
            false;
    }

    // ============================================================
    // SIDE
    // ============================================================

    public enum ClimbSide
    {
        Auto,
        Left,
        Right
    }

    [Header("Climb Side")]

    [SerializeField]
    private ClimbSide climbSide =
        ClimbSide.Auto;

    // ============================================================
    // CLIMB SETTINGS
    // ============================================================

    [Header("Climb Settings")]

    [SerializeField]
    private float climbSpeed = 3f;

    [SerializeField]
    private bool disableGravityWhileClimbing = true;

    [SerializeField, Range(0.01f, 0.5f)]
    private float verticalInputThreshold = 0.1f;

    [Tooltip(
        "Сколько секунд сохраняем последний ввод вверх/вниз. " +
        "Позволяет нажать ↓ чуть раньше входа в скобы."
    )]
    [SerializeField, Range(0f, 1f)]
    private float inputBufferTime = 0.35f;

    [Tooltip(
        "Если включено, после отпускания ↑/↓ " +
        "игрок продолжает висеть на скобах."
    )]
    [SerializeField]
    private bool holdOnHookWhenInputReleased = true;

    // ============================================================
    // CLIMB ANCHOR X
    // ============================================================

    [Header("Climb Anchor X")]

    [Tooltip(
        "Если включено, при захвате скоб Player " +
        "ставится по X точно в положение Climb Anchor."
    )]
    [SerializeField]
    private bool useClimbAnchor = true;

    [Tooltip(
        "Точка, определяющая положение Player по X " +
        "во время лазания. Координата Y этой точки не используется."
    )]
    [SerializeField]
    private Transform climbAnchor;

    [Tooltip(
        "Если включено, X игрока удерживается на Climb Anchor " +
        "всё время, пока Player находится на скобах."
    )]
    [SerializeField]
    private bool lockXToClimbAnchorWhileClimbing = true;

    // ============================================================
    // TOP STOP
    // ============================================================

    [Header("Top Stop")]

    [SerializeField]
    private bool useTopStop = true;

    [Tooltip(
        "Объект, задающий максимальную высоту подъёма."
    )]
    [SerializeField]
    private Transform topStop;

    [SerializeField, Min(0f)]
    private float topStopTolerance = 0.02f;

    [SerializeField]
    private bool snapToTopStop = true;

    // ============================================================
    // AUTO CATCH
    // ============================================================

    [Header("Automatic Catch")]

    [SerializeField]
    private bool autoCatchWhenFalling = true;

    [SerializeField]
    private float autoCatchMaximumVerticalSpeed = 0.15f;

    [SerializeField, Range(0f, 1f)]
    private float triggerGraceTime = 0.25f;

    [Tooltip(
        "При автоматическом захвате во время падения " +
        "сразу показывать спрайт спуска по скобам."
    )]
    [SerializeField]
    private bool showClimbDownVisualOnAutoCatch = true;

    // ============================================================
    // BOTTOM DROP
    // ============================================================

    [Header("Bottom Drop")]

    [SerializeField]
    private bool allowDropFromBottom = true;

    [SerializeField, Min(0f)]
    private float bottomDropSpeed = 0.5f;

    [SerializeField, Min(0f)]
    private float bottomExitTolerance = 0.05f;

    // ============================================================
    // HORIZONTAL EXIT
    // ============================================================

    [Header("Horizontal Exit")]

    [SerializeField]
    private bool allowHorizontalExit = false;

    [SerializeField]
    private float horizontalExitSpeed = 5f;

    [SerializeField, Range(0.01f, 0.5f)]
    private float horizontalExitThreshold = 0.1f;

    // ============================================================
    // INPUT
    // ============================================================

    [Header("Input")]

    [SerializeField]
    private MobileInput mobileInput;

    // ============================================================
    // SOUND
    // ============================================================

    [Header("Climb Sound")]

    [SerializeField]
    private AudioClip climbSound;

    [SerializeField, Range(0f, 1f)]
    private float climbSoundVolume = 0.7f;

    [SerializeField, Min(0.05f)]
    private float climbSoundInterval = 0.22f;

    [SerializeField]
    private bool randomizePitch = true;

    [SerializeField]
    private Vector2 climbPitchRange =
        new Vector2(
            0.96f,
            1.04f
        );

    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("Climb Haptics")]

    [SerializeField]
    private bool useClimbHaptics = true;

    [SerializeField, Range(5, 50)]
    private int androidClimbHapticDurationMs = 10;

    [SerializeField]
    private MicroHaptics.IOSHapticStyle
        iosClimbHapticStyle =
            MicroHaptics.IOSHapticStyle.Selection;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]

    [SerializeField]
    private bool debugLogs = false;

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private readonly HashSet<Collider2D>
        playerOverlaps =
            new HashSet<Collider2D>();

    private bool isActivelyClimbing;

    private bool hookIsOnRight;

    private Transform playerRoot;

    private Rigidbody2D playerRb;
    private PlayerController playerController;
    private PlayerVisual playerVisual;
    private PlayerHealth playerHealth;
    private PlayerJump playerJump;

    private Collider2D playerBodyCollider;
    private Collider2D zoneCollider;

    private float savedGravityScale;
    private bool gravityWasSaved;

    private AudioSource climbAudioSource;

    private float nextClimbSoundTime;

    private float lastTriggerContactTime =
        -100f;

    private float lastVerticalInputTime =
        -100f;

    private float lastBufferedVertical;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        zoneCollider =
            GetComponent<Collider2D>();

        climbAudioSource =
            GetComponent<AudioSource>();

        if (climbAudioSource == null)
        {
            climbAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        climbAudioSource.playOnAwake =
            false;

        climbAudioSource.loop =
            false;

        climbAudioSource.spatialBlend =
            0f;

        climbAudioSource.volume =
            climbSoundVolume;

        if (mobileInput == null)
        {
            mobileInput =
                FindFirstObjectByType<MobileInput>();
        }
    }

    // ============================================================
    // TRIGGER ENTER
    // ============================================================

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (!TryResolvePlayer(
                collision))
        {
            return;
        }

        playerOverlaps.Add(
            collision
        );

        lastTriggerContactTime =
            Time.time;

        hookIsOnRight =
            ResolveHookSide();

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook ENTER: {name} | " +
                $"Collider = {collision.name} | " +
                $"Count = {playerOverlaps.Count} | " +
                $"Side = {(hookIsOnRight ? "RIGHT" : "LEFT")}",
                this
            );
        }

        if (hookJumpCatchLocked)
        {
            return;
        }

        TryStartClimb();
    }

    // ============================================================
    // TRIGGER STAY
    // ============================================================

    private void OnTriggerStay2D(
        Collider2D collision
    )
    {
        if (!TryResolvePlayer(
                collision))
        {
            return;
        }

        playerOverlaps.Add(
            collision
        );

        lastTriggerContactTime =
            Time.time;
    }

    // ============================================================
    // TRIGGER EXIT
    // ============================================================

    private void OnTriggerExit2D(
        Collider2D collision
    )
    {
        if (!BelongsToCurrentPlayer(
                collision))
        {
            return;
        }

        playerOverlaps.Remove(
            collision
        );

        lastTriggerContactTime =
            Time.time;

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook EXIT: {name} | " +
                $"Collider = {collision.name} | " +
                $"Remaining = {playerOverlaps.Count} | " +
                $"Active = {isActivelyClimbing}",
                this
            );
        }

        if (isActivelyClimbing)
        {
            return;
        }
    }

    // ============================================================
    // FIXED UPDATE
    // ============================================================

    private void FixedUpdate()
    {
        UpdateInputBuffer();

        if (PlayerIsDead())
        {
            if (isActivelyClimbing)
            {
                StopActiveClimb();
            }

            return;
        }

        if (isActivelyClimbing)
        {
            UpdateActiveClimb();
            return;
        }

        if (hookJumpCatchLocked)
        {
            return;
        }

        bool nearHooks =
            playerOverlaps.Count > 0 ||
            Time.time -
            lastTriggerContactTime <=
            triggerGraceTime;

        if (!nearHooks)
        {
            return;
        }

        TryStartClimb();
    }

    // ============================================================
    // INPUT BUFFER
    // ============================================================

    private void UpdateInputBuffer()
    {
        float vertical =
            ReadVerticalInputRaw();

        if (Mathf.Abs(vertical) >
            verticalInputThreshold)
        {
            lastVerticalInputTime =
                Time.time;

            lastBufferedVertical =
                vertical;
        }
    }

    private float GetBufferedVerticalInput()
    {
        float current =
            ReadVerticalInputRaw();

        if (Mathf.Abs(current) >
            verticalInputThreshold)
        {
            return current;
        }

        if (Time.time -
            lastVerticalInputTime <=
            inputBufferTime)
        {
            return lastBufferedVertical;
        }

        return 0f;
    }

    // ============================================================
    // TRY START CLIMB
    // ============================================================

    private void TryStartClimb()
    {
        if (isActivelyClimbing)
        {
            return;
        }

        if (hookJumpCatchLocked)
        {
            return;
        }

        if (playerRb == null ||
            playerController == null)
        {
            return;
        }

        if (activeClimbZone != null &&
            activeClimbZone != this)
        {
            return;
        }

        float vertical =
            GetBufferedVerticalInput();

        /*
         * Обычное цепляние при нажатой
         * стрелке вверх или вниз.
         */
        if (Mathf.Abs(vertical) >
            verticalInputThreshold)
        {
            StartActiveClimb(
                vertical,
                false
            );

            return;
        }

        /*
         * Автоматическое цепляние,
         * когда Player падает через ClimbZone.
         */
        if (autoCatchWhenFalling &&
            PlayerIsAirborne() &&
            playerRb.linearVelocity.y <=
            autoCatchMaximumVerticalSpeed)
        {
            StartActiveClimb(
                0f,
                true
            );
        }
    }

    // ============================================================
    // START ACTIVE CLIMB
    // ============================================================

    private void StartActiveClimb(
        float initialVertical,
        bool automaticCatch
    )
    {
        if (playerRb == null)
        {
            return;
        }

        if (hookJumpCatchLocked)
        {
            return;
        }

        if (activeClimbZone != null &&
            activeClimbZone != this)
        {
            return;
        }

        /*
         * Сохраняем скорость ДО того,
         * как остановим Rigidbody.
         */
        float incomingVerticalSpeed =
            playerRb.linearVelocity.y;

        activeClimbZone =
            this;

        isActivelyClimbing =
            true;

        PlayerIsOnHook =
            true;

        hookIsOnRight =
            ResolveHookSide();

        if (!gravityWasSaved)
        {
            savedGravityScale =
                playerRb.gravityScale;

            gravityWasSaved =
                true;
        }

        if (disableGravityWhileClimbing)
        {
            playerRb.gravityScale =
                0f;
        }

        playerRb.linearVelocity =
            Vector2.zero;

        /*
         * НОВОЕ:
         * как только Player поймал скобы,
         * сразу ставим его в заданный X.
         */
        ApplyClimbAnchorX(
            true
        );

        nextClimbSoundTime =
            0f;

        // ========================================================
        // VISUAL
        // ========================================================

        if (playerVisual != null)
        {
            if (automaticCatch &&
                showClimbDownVisualOnAutoCatch)
            {
                playerVisual.SetClimbLook(
                    -1f,
                    hookIsOnRight
                );
            }
            else if (Mathf.Abs(initialVertical) >
                     verticalInputThreshold)
            {
                playerVisual.SetClimbLook(
                    initialVertical,
                    hookIsOnRight
                );
            }
            else if (incomingVerticalSpeed < 0f)
            {
                playerVisual.SetClimbLook(
                    -1f,
                    hookIsOnRight
                );
            }
        }

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook ACTIVE START: {name} | " +
                $"Initial Vertical = {initialVertical:F2} | " +
                $"Auto Catch = {automaticCatch} | " +
                $"Incoming Y = {incomingVerticalSpeed:F2}",
                this
            );
        }
    }

    // ============================================================
    // ACTIVE CLIMB
    // ============================================================

    private void UpdateActiveClimb()
    {
        if (playerRb == null)
        {
            StopActiveClimb();
            return;
        }

        float horizontal =
            ReadHorizontalInput();

        float vertical =
            ReadVerticalInputRaw();

        // ========================================================
        // HORIZONTAL EXIT
        // ========================================================

        if (allowHorizontalExit &&
            Mathf.Abs(horizontal) >
            horizontalExitThreshold)
        {
            ExitHookHorizontally(
                horizontal
            );

            return;
        }

        /*
         * НОВОЕ:
         * пока Player находится на скобах,
         * удерживаем его на заданном X.
         */
        ApplyClimbAnchorX(
            false
        );

        // ========================================================
        // BOTTOM DROP
        // ========================================================

        if (allowDropFromBottom &&
            vertical <
            -verticalInputThreshold &&
            IsBelowBottomOfZone())
        {
            DropFromBottom();
            return;
        }

        // ========================================================
        // TOP STOP
        // ========================================================

        if (useTopStop &&
            topStop != null &&
            vertical >
            verticalInputThreshold &&
            WillReachOrPassTopStop())
        {
            HoldAtTopStop();

            return;
        }

        // ========================================================
        // OUTSIDE ZONE
        // ========================================================

        if (playerOverlaps.Count == 0)
        {
            if (PlayerIsGrounded())
            {
                StopActiveClimb();
                return;
            }

            if ((!useTopStop ||
                 topStop == null) &&
                IsAboveTopOfZone())
            {
                playerRb.linearVelocity =
                    Vector2.zero;

                return;
            }
        }

        // ========================================================
        // CLIMB UP / DOWN
        // ========================================================

        if (Mathf.Abs(vertical) >
            verticalInputThreshold)
        {
            playerRb.linearVelocity =
                new Vector2(
                    0f,
                    vertical *
                    climbSpeed
                );

            if (playerVisual != null)
            {
                playerVisual.SetClimbLook(
                    vertical,
                    hookIsOnRight
                );
            }

            UpdateClimbFeedback();

            return;
        }

        // ========================================================
        // INPUT RELEASED
        // ========================================================

        if (holdOnHookWhenInputReleased)
        {
            playerRb.linearVelocity =
                Vector2.zero;

            return;
        }

        StopActiveClimb();
    }

    // ============================================================
    // CLIMB ANCHOR X
    // ============================================================

    private void ApplyClimbAnchorX(
        bool forceSnap
    )
    {
        if (!useClimbAnchor ||
            climbAnchor == null ||
            playerRb == null)
        {
            return;
        }

        /*
         * При первом захвате forceSnap = true,
         * поэтому X ставится сразу.
         *
         * После этого положение X поддерживается,
         * только если включён Lock X.
         */
        if (!forceSnap &&
            !lockXToClimbAnchorWhileClimbing)
        {
            return;
        }

        Vector2 position =
            playerRb.position;

        position.x =
            climbAnchor.position.x;

        playerRb.position =
            position;
    }

    // ============================================================
    // TOP STOP
    // ============================================================

    private bool WillReachOrPassTopStop()
    {
        if (!useTopStop ||
            topStop == null ||
            playerRb == null)
        {
            return false;
        }

        float currentY =
            playerRb.position.y;

        float nextY =
            currentY +
            climbSpeed *
            Time.fixedDeltaTime;

        float stopY =
            topStop.position.y;

        return
            currentY >=
            stopY -
            topStopTolerance ||
            nextY >=
            stopY -
            topStopTolerance;
    }

    private bool IsAtTopStop()
    {
        if (!useTopStop ||
            topStop == null ||
            playerRb == null)
        {
            return false;
        }

        return
            playerRb.position.y >=
            topStop.position.y -
            topStopTolerance;
    }

    private void HoldAtTopStop()
    {
        if (playerRb == null ||
            topStop == null)
        {
            return;
        }

        /*
         * На верхнем стопе тоже сохраняем
         * правильное положение по X.
         */
        ApplyClimbAnchorX(
            false
        );

        if (snapToTopStop)
        {
            playerRb.position =
                new Vector2(
                    playerRb.position.x,
                    topStop.position.y
                );
        }

        playerRb.linearVelocity =
            Vector2.zero;

        StopClimbSound();

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook TOP STOP: {name}",
                this
            );
        }
    }

    // ============================================================
    // RELEASE FROM TOP FOR JUMP
    // ============================================================

    private bool ReleaseFromTopForJump()
    {
        if (!isActivelyClimbing)
        {
            return false;
        }

        if (!useTopStop ||
            topStop == null)
        {
            return false;
        }

        if (!IsAtTopStop())
        {
            return false;
        }

        hookJumpCatchLocked =
            true;

        Rigidbody2D rbToRelease =
            playerRb;

        StopActiveClimb();

        if (rbToRelease != null)
        {
            rbToRelease.linearVelocity =
                new Vector2(
                    rbToRelease.linearVelocity.x,
                    0f
                );
        }

        playerOverlaps.Clear();

        lastTriggerContactTime =
            -100f;

        lastVerticalInputTime =
            -100f;

        lastBufferedVertical =
            0f;

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook TOP JUMP RELEASE: {name}",
                this
            );
        }

        return true;
    }

    // ============================================================
    // BOTTOM CHECK
    // ============================================================

    private bool IsBelowBottomOfZone()
    {
        if (zoneCollider == null ||
            playerRoot == null)
        {
            return false;
        }

        Bounds zoneBounds =
            zoneCollider.bounds;

        Bounds playerBounds =
            GetPlayerBounds();

        return
            playerBounds.center.y <
            zoneBounds.min.y -
            bottomExitTolerance;
    }

    // ============================================================
    // TOP CHECK
    // ============================================================

    private bool IsAboveTopOfZone()
    {
        if (zoneCollider == null ||
            playerRoot == null)
        {
            return false;
        }

        Bounds zoneBounds =
            zoneCollider.bounds;

        Bounds playerBounds =
            GetPlayerBounds();

        return
            playerBounds.min.y >
            zoneBounds.max.y;
    }

    // ============================================================
    // PLAYER BOUNDS
    // ============================================================

    private Bounds GetPlayerBounds()
    {
        if (playerBodyCollider != null)
        {
            return
                playerBodyCollider.bounds;
        }

        if (playerRoot != null)
        {
            return
                new Bounds(
                    playerRoot.position,
                    Vector3.one * 0.1f
                );
        }

        return
            new Bounds(
                Vector3.zero,
                Vector3.zero
            );
    }

    // ============================================================
    // DROP FROM BOTTOM
    // ============================================================

    private void DropFromBottom()
    {
        Rigidbody2D rbToDrop =
            playerRb;

        StopActiveClimb();

        if (rbToDrop != null)
        {
            rbToDrop.linearVelocity =
                new Vector2(
                    rbToDrop.linearVelocity.x,
                    -bottomDropSpeed
                );
        }

        playerOverlaps.Clear();

        lastTriggerContactTime =
            -100f;

        lastVerticalInputTime =
            -100f;

        lastBufferedVertical =
            0f;

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook BOTTOM DROP: {name}",
                this
            );
        }
    }

    // ============================================================
    // HORIZONTAL EXIT
    // ============================================================

    private void ExitHookHorizontally(
        float horizontal
    )
    {
        Rigidbody2D rbToExit =
            playerRb;

        StopActiveClimb();

        if (rbToExit != null)
        {
            rbToExit.linearVelocity =
                new Vector2(
                    Mathf.Sign(horizontal) *
                    horizontalExitSpeed,
                    rbToExit.linearVelocity.y
                );
        }

        playerOverlaps.Clear();

        lastTriggerContactTime =
            -100f;

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook HORIZONTAL EXIT: {name}",
                this
            );
        }
    }

    // ============================================================
    // STOP ACTIVE CLIMB
    // ============================================================

    private void StopActiveClimb()
    {
        if (!isActivelyClimbing)
        {
            return;
        }

        isActivelyClimbing =
            false;

        if (activeClimbZone == this)
        {
            activeClimbZone =
                null;
        }

        PlayerIsOnHook =
            false;

        climbVerticalInput =
            0f;

        StopClimbSound();

        if (playerRb != null &&
            gravityWasSaved)
        {
            playerRb.gravityScale =
                savedGravityScale;
        }

        gravityWasSaved =
            false;

        if (playerVisual != null)
        {
            playerVisual.ClearClimbLook();
        }

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook ACTIVE STOP: {name}",
                this
            );
        }
    }

    // ============================================================
    // PLAYER RESOLVE
    // ============================================================

    private bool TryResolvePlayer(
        Collider2D collision
    )
    {
        if (collision == null)
        {
            return false;
        }

        PlayerController detectedController =
            collision.GetComponentInParent<
                PlayerController
            >();

        if (detectedController == null)
        {
            return false;
        }

        Rigidbody2D detectedRb =
            detectedController.GetComponent<
                Rigidbody2D
            >();

        if (detectedRb == null)
        {
            return false;
        }

        PlayerHealth detectedHealth =
            detectedController.GetComponent<
                PlayerHealth
            >();

        if (detectedHealth != null &&
            detectedHealth.IsDead)
        {
            return false;
        }

        playerRoot =
            detectedController.transform;

        playerController =
            detectedController;

        playerRb =
            detectedRb;

        playerVisual =
            detectedController.GetComponent<
                PlayerVisual
            >();

        playerHealth =
            detectedHealth;

        playerJump =
            detectedController.GetComponent<
                PlayerJump
            >();

        Collider2D[] colliders =
            detectedController.GetComponents<
                Collider2D
            >();

        playerBodyCollider =
            null;

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            if (colliders[i] != null &&
                !colliders[i].isTrigger)
            {
                playerBodyCollider =
                    colliders[i];

                break;
            }
        }

        return true;
    }

    private bool BelongsToCurrentPlayer(
        Collider2D collision
    )
    {
        if (collision == null)
        {
            return false;
        }

        PlayerController controller =
            collision.GetComponentInParent<
                PlayerController
            >();

        if (controller == null)
        {
            return false;
        }

        if (playerRoot == null)
        {
            return true;
        }

        return
            controller.transform ==
            playerRoot;
    }

    // ============================================================
    // PLAYER STATE
    // ============================================================

    private bool PlayerIsDead()
    {
        return
            playerHealth != null &&
            playerHealth.IsDead;
    }

    private bool PlayerIsGrounded()
    {
        if (playerJump == null)
        {
            return false;
        }

        return
            playerJump.IsGrounded();
    }

    private bool PlayerIsAirborne()
    {
        if (playerJump == null)
        {
            return
                playerRb != null &&
                Mathf.Abs(
                    playerRb.linearVelocity.y
                ) > 0.05f;
        }

        return
            !playerJump.IsGrounded();
    }

    // ============================================================
    // SIDE
    // ============================================================

    private bool ResolveHookSide()
    {
        if (climbSide ==
            ClimbSide.Left)
        {
            return false;
        }

        if (climbSide ==
            ClimbSide.Right)
        {
            return true;
        }

        if (playerRoot == null)
        {
            return true;
        }

        return
            transform.position.x >=
            playerRoot.position.x;
    }

    // ============================================================
    // INPUT
    // ============================================================

    private float ReadVerticalInputRaw()
    {
        if (Mathf.Abs(climbVerticalInput) >
            verticalInputThreshold)
        {
            return Mathf.Clamp(
                climbVerticalInput,
                -1f,
                1f
            );
        }

        if (mobileInput == null)
        {
            mobileInput =
                FindFirstObjectByType<
                    MobileInput
                >();
        }

        if (mobileInput != null)
        {
            return Mathf.Clamp(
                mobileInput.Vertical,
                -1f,
                1f
            );
        }

        if (playerController != null)
        {
            return Mathf.Clamp(
                playerController
                    .GetClimbVerticalInput(),
                -1f,
                1f
            );
        }

        return 0f;
    }

    private float ReadHorizontalInput()
    {
        if (mobileInput == null)
        {
            mobileInput =
                FindFirstObjectByType<
                    MobileInput
                >();
        }

        if (mobileInput != null)
        {
            return Mathf.Clamp(
                mobileInput.Horizontal,
                -1f,
                1f
            );
        }

        if (playerController != null)
        {
            return Mathf.Clamp(
                playerController
                    .GetInput().x,
                -1f,
                1f
            );
        }

        return 0f;
    }

    // ============================================================
    // FEEDBACK
    // ============================================================

    private void UpdateClimbFeedback()
    {
        if (climbSound == null ||
            climbAudioSource == null)
        {
            return;
        }

        if (Time.time <
            nextClimbSoundTime)
        {
            return;
        }

        if (randomizePitch)
        {
            float minPitch =
                Mathf.Min(
                    climbPitchRange.x,
                    climbPitchRange.y
                );

            float maxPitch =
                Mathf.Max(
                    climbPitchRange.x,
                    climbPitchRange.y
                );

            climbAudioSource.pitch =
                Random.Range(
                    minPitch,
                    maxPitch
                );
        }
        else
        {
            climbAudioSource.pitch =
                1f;
        }

        climbAudioSource.volume =
            climbSoundVolume;

        climbAudioSource.PlayOneShot(
            climbSound,
            climbSoundVolume
        );

        if (useClimbHaptics)
        {
            MicroHaptics.Pulse(
                androidClimbHapticDurationMs,
                iosClimbHapticStyle
            );
        }

        nextClimbSoundTime =
            Time.time +
            climbSoundInterval;
    }

    // ============================================================
    // SOUND
    // ============================================================

    private void StopClimbSound()
    {
        if (climbAudioSource == null)
        {
            return;
        }

        if (climbAudioSource.isPlaying)
        {
            climbAudioSource.Stop();
        }

        climbAudioSource.pitch =
            1f;
    }

    // ============================================================
    // DISABLE
    // ============================================================

    private void OnDisable()
    {
        if (isActivelyClimbing)
        {
            StopActiveClimb();
        }

        if (activeClimbZone == this)
        {
            activeClimbZone =
                null;
        }

        PlayerIsOnHook =
            false;

        playerOverlaps.Clear();

        climbVerticalInput =
            0f;

        StopClimbSound();
    }

    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        climbSpeed =
            Mathf.Max(
                0f,
                climbSpeed
            );

        horizontalExitSpeed =
            Mathf.Max(
                0f,
                horizontalExitSpeed
            );

        bottomDropSpeed =
            Mathf.Max(
                0f,
                bottomDropSpeed
            );

        bottomExitTolerance =
            Mathf.Max(
                0f,
                bottomExitTolerance
            );

        topStopTolerance =
            Mathf.Max(
                0f,
                topStopTolerance
            );

        climbSoundInterval =
            Mathf.Max(
                0.05f,
                climbSoundInterval
            );

        inputBufferTime =
            Mathf.Max(
                0f,
                inputBufferTime
            );

        triggerGraceTime =
            Mathf.Max(
                0f,
                triggerGraceTime
            );
    }
}