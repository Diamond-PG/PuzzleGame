using System.Collections;
using UnityEngine;

public class PlayerKick : MonoBehaviour
{
    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("REFERENCES")]

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private PlayerVisual playerVisual;

    [SerializeField]
    private PlayerHealth playerHealth;


    // ============================================================
    // KICK TIMING
    // ============================================================

    [Header("KICK TIMING")]

    [SerializeField]
    private float kickDuration = 0.20f;

    [SerializeField]
    private float kickCooldown = 0.25f;


    // ============================================================
    // FACING
    // ============================================================

    [Header("FACING")]

    [Tooltip(
        "Минимальный ввод игрока по X, " +
        "который считается намеренным движением " +
        "и меняет сторону удара."
    )]
    [SerializeField, Range(0f, 1f)]
    private float facingInputThreshold = 0.01f;


    // ============================================================
    // MOVEMENT
    // ============================================================

    [Header("MOVEMENT")]

    [SerializeField]
    private bool stopHorizontalMovement = true;

    [SerializeField]
    private bool lockMovementDuringKick = true;


    // ============================================================
    // KICK VOICE
    // ============================================================

    [Header("KICK VOICE")]

    [SerializeField]
    private AudioSource kickAudioSource;

    [SerializeField]
    private AudioClip kickVoiceSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float kickVoiceVolume = 1f;

    [SerializeField]
    private float kickVoiceDelay = 0f;


    // ============================================================
    // STATE
    // ============================================================

    private bool isKicking;

    /*
     * Запоминаем последнее НАМЕРЕННОЕ
     * направление игрока.
     *
     * ВАЖНО:
     * физические толчки врагов это значение
     * больше не меняют.
     */
    private bool facingRight = true;

    private float nextKickTime;

    private Coroutine kickRoutine;
    private Coroutine voiceRoutine;


    // ============================================================
    // PUBLIC STATE
    // ============================================================

    public bool IsKicking =>
        isKicking;

    public bool FacingRight =>
        facingRight;


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

        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }

        if (playerVisual == null)
        {
            playerVisual =
                GetComponent<PlayerVisual>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                GetComponent<PlayerHealth>();
        }

        if (kickAudioSource == null)
        {
            AudioSource[] sources =
                GetComponents<AudioSource>();

            if (sources.Length > 0)
            {
                kickAudioSource =
                    sources[
                        sources.Length - 1
                    ];
            }
        }
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (PlayerIsDead())
        {
            return;
        }

        /*
         * Пока игрок НЕ бьёт ногой,
         * отслеживаем только настоящий
         * пользовательский ввод.
         *
         * Rigidbody здесь больше
         * НЕ используется для стороны.
         */
        if (!isKicking)
        {
            UpdateFacingDirectionFromInput();
        }
    }


    // ============================================================
    // CHECKS
    // ============================================================

    public bool PlayerIsDead()
    {
        return
            playerHealth != null &&
            playerHealth.IsDead;
    }


    private bool GameplayActionsBlocked()
    {
        if (PlayerIsDead())
        {
            return true;
        }

        if (playerVisual != null &&
            playerVisual.GameplayActionsLocked)
        {
            return true;
        }

        if (playerController != null &&
            playerController.IsActionLocked)
        {
            return true;
        }

        return false;
    }


    // ============================================================
    // FACING
    // ============================================================

    private void UpdateFacingDirectionFromInput()
    {
        if (playerController == null)
        {
            return;
        }

        /*
         * Берём именно ввод игрока:
         *
         * MobileInput
         * или
         * A / D
         * или
         * стрелки.
         *
         * Толчок охранника сюда не попадает.
         */
        Vector2 movementInput =
            playerController.GetInput();

        float horizontalInput =
            movementInput.x;

        if (horizontalInput >
            facingInputThreshold)
        {
            facingRight =
                true;
        }
        else if (horizontalInput <
                 -facingInputThreshold)
        {
            facingRight =
                false;
        }

        /*
         * Если horizontalInput == 0:
         *
         * направление НЕ меняем.
         *
         * Поэтому после того как Player
         * перестал идти вправо,
         * он продолжает смотреть вправо,
         * пока сам не нажмёт влево.
         */
    }


    // ============================================================
    // PUBLIC KICK
    // ============================================================

    public bool Kick()
    {
        if (GameplayActionsBlocked())
        {
            return false;
        }

        /*
         * На всякий случай прямо перед ударом
         * ещё раз читаем текущий ввод.
         *
         * Например:
         * игрок держит RIGHT и сразу жмёт Kick.
         */
        UpdateFacingDirectionFromInput();

        return StartKick(
            facingRight
        );
    }


    public bool KickRight()
    {
        if (GameplayActionsBlocked())
        {
            return false;
        }

        return StartKick(
            true
        );
    }


    public bool KickLeft()
    {
        if (GameplayActionsBlocked())
        {
            return false;
        }

        return StartKick(
            false
        );
    }


    public bool KickToward(
        Vector3 worldPosition
    )
    {
        if (GameplayActionsBlocked())
        {
            return false;
        }

        bool kickToRight =
            worldPosition.x >=
            transform.position.x;

        return StartKick(
            kickToRight
        );
    }


    // ============================================================
    // START KICK
    // ============================================================

    private bool StartKick(
        bool kickToRight
    )
    {
        if (GameplayActionsBlocked())
        {
            return false;
        }

        if (isKicking)
        {
            return false;
        }

        if (Time.time <
            nextKickTime)
        {
            return false;
        }

        if (playerVisual == null)
        {
            return false;
        }

        /*
         * Фиксируем сторону именно
         * в момент начала удара.
         */
        facingRight =
            kickToRight;

        kickRoutine =
            StartCoroutine(
                KickRoutine(
                    kickToRight
                )
            );

        return true;
    }


    // ============================================================
    // KICK ROUTINE
    // ============================================================

    private IEnumerator KickRoutine(
        bool kickToRight
    )
    {
        if (GameplayActionsBlocked())
        {
            yield break;
        }

        isKicking =
            true;

        nextKickTime =
            Time.time +
            kickCooldown;


        // --------------------------------------------------------
        // STOP HORIZONTAL MOVEMENT
        // --------------------------------------------------------

        if (stopHorizontalMovement &&
            rb != null)
        {
            Vector2 velocity =
                rb.linearVelocity;

            velocity.x =
                0f;

            rb.linearVelocity =
                velocity;
        }


        // --------------------------------------------------------
        // LOCK MOVEMENT
        // --------------------------------------------------------

        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                false;
        }


        // --------------------------------------------------------
        // BLOCK SAFETY
        // --------------------------------------------------------

        if (GameplayActionsBlocked())
        {
            FinishKickBecauseBlocked();

            yield break;
        }


        // --------------------------------------------------------
        // VISUAL
        // --------------------------------------------------------

        /*
         * Используем сторону,
         * которая была зафиксирована
         * ДО начала Coroutine.
         *
         * Теперь физика уже никак
         * не способна перевернуть удар.
         */
        if (kickToRight)
        {
            playerVisual.PlayKickRight();
        }
        else
        {
            playerVisual.PlayKickLeft();
        }


        // --------------------------------------------------------
        // VOICE
        // --------------------------------------------------------

        PlayKickVoice();


        // --------------------------------------------------------
        // WAIT
        // --------------------------------------------------------

        float timer =
            0f;

        while (timer <
               kickDuration)
        {
            if (PlayerIsDead())
            {
                FinishKickAfterDeath();

                yield break;
            }

            timer +=
                Time.deltaTime;

            yield return null;
        }


        // --------------------------------------------------------
        // END VISUAL
        // --------------------------------------------------------

        playerVisual.EndKick();


        // --------------------------------------------------------
        // UNLOCK MOVEMENT
        // --------------------------------------------------------

        if (lockMovementDuringKick &&
            playerController != null &&
            !PlayerIsDead())
        {
            playerController.enabled =
                true;
        }

        isKicking =
            false;

        kickRoutine =
            null;
    }


    // ============================================================
    // VOICE
    // ============================================================

    private void PlayKickVoice()
    {
        if (GameplayActionsBlocked())
        {
            return;
        }

        if (kickAudioSource == null ||
            kickVoiceSound == null)
        {
            return;
        }

        if (voiceRoutine != null)
        {
            StopCoroutine(
                voiceRoutine
            );

            voiceRoutine =
                null;
        }

        voiceRoutine =
            StartCoroutine(
                KickVoiceRoutine()
            );
    }


    private IEnumerator KickVoiceRoutine()
    {
        if (kickVoiceDelay > 0f)
        {
            float timer =
                0f;

            while (timer <
                   kickVoiceDelay)
            {
                if (PlayerIsDead())
                {
                    voiceRoutine =
                        null;

                    yield break;
                }

                timer +=
                    Time.deltaTime;

                yield return null;
            }
        }

        if (PlayerIsDead())
        {
            voiceRoutine =
                null;

            yield break;
        }

        if (kickAudioSource != null &&
            kickVoiceSound != null)
        {
            kickAudioSource.PlayOneShot(
                kickVoiceSound,
                kickVoiceVolume
            );
        }

        voiceRoutine =
            null;
    }


    // ============================================================
    // BLOCK / DEATH SAFETY
    // ============================================================

    private void FinishKickBecauseBlocked()
    {
        if (voiceRoutine != null)
        {
            StopCoroutine(
                voiceRoutine
            );

            voiceRoutine =
                null;
        }

        if (playerVisual != null)
        {
            playerVisual.EndKick();
        }

        if (!PlayerIsDead() &&
            lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                true;
        }

        isKicking =
            false;

        kickRoutine =
            null;
    }


    private void FinishKickAfterDeath()
    {
        if (voiceRoutine != null)
        {
            StopCoroutine(
                voiceRoutine
            );

            voiceRoutine =
                null;
        }

        if (kickAudioSource != null)
        {
            kickAudioSource.Stop();
        }

        if (playerVisual != null)
        {
            playerVisual.EndKick();
        }

        isKicking =
            false;

        kickRoutine =
            null;
    }


    // ============================================================
    // DISABLE
    // ============================================================

    private void OnDisable()
    {
        if (kickRoutine != null)
        {
            StopCoroutine(
                kickRoutine
            );

            kickRoutine =
                null;
        }

        if (voiceRoutine != null)
        {
            StopCoroutine(
                voiceRoutine
            );

            voiceRoutine =
                null;
        }

        if (kickAudioSource != null &&
            PlayerIsDead())
        {
            kickAudioSource.Stop();
        }

        if (playerVisual != null)
        {
            playerVisual.EndKick();
        }

        if (!PlayerIsDead() &&
            lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                true;
        }

        isKicking =
            false;
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        kickDuration =
            Mathf.Max(
                0.01f,
                kickDuration
            );

        kickCooldown =
            Mathf.Max(
                0f,
                kickCooldown
            );

        kickVoiceVolume =
            Mathf.Clamp01(
                kickVoiceVolume
            );

        kickVoiceDelay =
            Mathf.Max(
                0f,
                kickVoiceDelay
            );

        facingInputThreshold =
            Mathf.Clamp01(
                facingInputThreshold
            );
    }
}