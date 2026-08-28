using System.Collections;
using UnityEngine;

public class PlayerKick : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerVisual playerVisual;

    [Header("KICK TIMING")]
    [SerializeField] private float kickDuration = 0.20f;
    [SerializeField] private float kickCooldown = 0.25f;

    [Header("MOVEMENT")]
    [SerializeField] private bool stopHorizontalMovement = true;
    [SerializeField] private bool lockMovementDuringKick = true;

    [Header("KICK VOICE")]
    [SerializeField] private AudioSource kickAudioSource;
    [SerializeField] private AudioClip kickVoiceSound;

    [Range(0f, 1f)]
    [SerializeField] private float kickVoiceVolume = 1f;

    [Tooltip(
        "Через сколько секунд после начала удара прозвучит голос."
    )]
    [SerializeField] private float kickVoiceDelay = 0f;

    private bool isKicking;
    private bool facingRight = true;

    private float nextKickTime;

    private Coroutine kickRoutine;
    private Coroutine voiceRoutine;

    public bool IsKicking => isKicking;
    public bool FacingRight => facingRight;

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

        if (kickAudioSource == null)
        {
            AudioSource[] sources =
                GetComponents<AudioSource>();

            if (sources.Length > 0)
            {
                kickAudioSource =
                    sources[sources.Length - 1];
            }
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        /*
         * Пока игрок не бьёт,
         * запоминаем последнее направление движения.
         *
         * Благодаря этому, если рядом нет цели,
         * кнопка ноги ударит туда,
         * куда игрок сейчас смотрит.
         */
        if (!isKicking)
        {
            UpdateFacingDirection();
        }
    }

    // ============================================================
    // FACING
    // ============================================================

    private void UpdateFacingDirection()
    {
        if (rb == null)
            return;

        float horizontalSpeed =
            rb.linearVelocity.x;

        if (horizontalSpeed > 0.05f)
        {
            facingRight = true;
        }
        else if (horizontalSpeed < -0.05f)
        {
            facingRight = false;
        }
    }

    // ============================================================
    // PUBLIC KICK COMMANDS
    // ============================================================

    /*
     * Обычный удар в ту сторону,
     * куда сейчас смотрит игрок.
     */
    public bool Kick()
    {
        return StartKick(
            facingRight
        );
    }

    /*
     * Принудительно вправо.
     */
    public bool KickRight()
    {
        return StartKick(
            true
        );
    }

    /*
     * Принудительно влево.
     */
    public bool KickLeft()
    {
        return StartKick(
            false
        );
    }

    /*
     * Удар в сторону конкретной мировой точки.
     *
     * Это будет использоваться кнопкой ноги,
     * когда рядом есть враг или ящик.
     */
    public bool KickToward(
        Vector3 worldPosition
    )
    {
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
        if (isKicking)
            return false;

        if (Time.time <
            nextKickTime)
        {
            return false;
        }

        if (playerVisual == null)
            return false;

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
        isKicking = true;

        nextKickTime =
            Time.time +
            kickCooldown;

        /*
         * Во время удара останавливаем
         * горизонтальное движение.
         */
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

        /*
         * При необходимости полностью
         * блокируем управление движением
         * на короткое время удара.
         */
        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                false;
        }

        /*
         * Включаем нужный спрайт удара.
         */
        if (kickToRight)
        {
            playerVisual.PlayKickRight();
        }
        else
        {
            playerVisual.PlayKickLeft();
        }

        PlayKickVoice();

        yield return new WaitForSeconds(
            kickDuration
        );

        playerVisual.EndKick();

        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                true;
        }

        isKicking = false;
        kickRoutine = null;
    }

    // ============================================================
    // KICK VOICE
    // ============================================================

    private void PlayKickVoice()
    {
        if (kickAudioSource == null)
            return;

        if (kickVoiceSound == null)
            return;

        if (voiceRoutine != null)
        {
            StopCoroutine(
                voiceRoutine
            );

            voiceRoutine = null;
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
            yield return new WaitForSeconds(
                kickVoiceDelay
            );
        }

        if (kickAudioSource != null &&
            kickVoiceSound != null)
        {
            kickAudioSource.PlayOneShot(
                kickVoiceSound,
                kickVoiceVolume
            );
        }

        voiceRoutine = null;
    }

    // ============================================================
    // DISABLE SAFETY
    // ============================================================

    private void OnDisable()
    {
        if (kickRoutine != null)
        {
            StopCoroutine(
                kickRoutine
            );

            kickRoutine = null;
        }

        if (voiceRoutine != null)
        {
            StopCoroutine(
                voiceRoutine
            );

            voiceRoutine = null;
        }

        if (playerVisual != null)
        {
            playerVisual.EndKick();
        }

        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                true;
        }

        isKicking = false;
    }
}