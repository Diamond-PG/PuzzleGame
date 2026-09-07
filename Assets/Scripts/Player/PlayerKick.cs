using System.Collections;
using UnityEngine;

public class PlayerKick : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private PlayerHealth playerHealth;

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
         * После смерти вообще больше
         * не обновляем направление удара.
         */
        if (PlayerIsDead())
            return;

        if (!isKicking)
        {
            UpdateFacingDirection();
        }
    }

    // ============================================================
    // DEAD CHECK
    // ============================================================

    public bool PlayerIsDead()
    {
        return
            playerHealth != null &&
            playerHealth.IsDead;
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

    public bool Kick()
    {
        if (PlayerIsDead())
            return false;

        return StartKick(
            facingRight
        );
    }

    public bool KickRight()
    {
        if (PlayerIsDead())
            return false;

        return StartKick(
            true
        );
    }

    public bool KickLeft()
    {
        if (PlayerIsDead())
            return false;

        return StartKick(
            false
        );
    }

    public bool KickToward(
        Vector3 worldPosition
    )
    {
        if (PlayerIsDead())
            return false;

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
        if (PlayerIsDead())
            return false;

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
        if (PlayerIsDead())
        {
            yield break;
        }

        isKicking = true;

        nextKickTime =
            Time.time +
            kickCooldown;

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

        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                false;
        }

        /*
         * Ещё одна проверка непосредственно
         * перед спрайтом и голосом.
         */
        if (PlayerIsDead())
        {
            FinishKickAfterDeath();
            yield break;
        }

        if (kickToRight)
        {
            playerVisual.PlayKickRight();
        }
        else
        {
            playerVisual.PlayKickLeft();
        }

        PlayKickVoice();

        float timer = 0f;

        while (timer < kickDuration)
        {
            /*
             * Если игрок умер прямо
             * во время анимации удара,
             * сразу всё прекращаем.
             */
            if (PlayerIsDead())
            {
                FinishKickAfterDeath();
                yield break;
            }

            timer +=
                Time.deltaTime;

            yield return null;
        }

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
        if (PlayerIsDead())
            return;

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
            float timer = 0f;

            while (timer <
                   kickVoiceDelay)
            {
                if (PlayerIsDead())
                {
                    voiceRoutine = null;
                    yield break;
                }

                timer +=
                    Time.deltaTime;

                yield return null;
            }
        }

        if (PlayerIsDead())
        {
            voiceRoutine = null;
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

        voiceRoutine = null;
    }

    // ============================================================
    // DEATH SAFETY
    // ============================================================

    private void FinishKickAfterDeath()
    {
        if (voiceRoutine != null)
        {
            StopCoroutine(
                voiceRoutine
            );

            voiceRoutine = null;
        }

        /*
         * Если сам голос уже успел запуститься,
         * останавливаем AudioSource.
         *
         * Это гарантирует тишину после смерти.
         */
        if (kickAudioSource != null)
        {
            kickAudioSource.Stop();
        }

        if (playerVisual != null)
        {
            playerVisual.EndKick();
        }

        /*
         * ВАЖНО:
         * после смерти специально НЕ включаем
         * PlayerController обратно.
         *
         * Иначе можно случайно вернуть управление
         * мёртвому игроку.
         */
        isKicking = false;
        kickRoutine = null;
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

        if (kickAudioSource != null &&
            PlayerIsDead())
        {
            kickAudioSource.Stop();
        }

        if (playerVisual != null)
        {
            playerVisual.EndKick();
        }

        /*
         * Контроллер возвращаем только
         * если Player ещё жив.
         */
        if (!PlayerIsDead() &&
            lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled =
                true;
        }

        isKicking = false;
    }
}