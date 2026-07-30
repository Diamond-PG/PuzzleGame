using System.Collections;
using UnityEngine;

public class SpringPad : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float launchForce = 12f;
    [SerializeField] private float minImpactSpeedToActivate = 0.2f;

    [Header("Trigger Rules")]
    [SerializeField] private float cooldown = 0.25f;
    [SerializeField] private float playerAboveTolerance = 0.08f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer springRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private float pressedTime = 0.08f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip springSound;

    [SerializeField, Range(0f, 1f)]
    private float springVolume = 0.7f;

    [Tooltip("Немного изменяет высоту звука при каждом срабатывании.")]
    [SerializeField]
    private bool randomizePitch = true;

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.95f, 1.05f);

    [Header("Haptics")]
    [Tooltip("Включить вибрацию при срабатывании этой пружины.")]
    [SerializeField]
    private bool useHaptics = true;

    [Tooltip("Длительность вибрации на Android в миллисекундах.")]
    [SerializeField, Range(5, 150)]
    private int androidHapticDurationMs = 25;

    [Tooltip("Сила хаптика на устройствах iOS.")]
    [SerializeField]
    private MicroHaptics.IOSHapticStyle iosHapticStyle =
        MicroHaptics.IOSHapticStyle.Light;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Collider2D springCollider;
    private bool canActivate = true;
    private float originalPitch = 1f;
    private Coroutine visualRoutine;

    private void Awake()
    {
        springCollider =
            GetComponent<Collider2D>();

        if (springRenderer == null)
        {
            springRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            originalPitch =
                audioSource.pitch;

            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (springRenderer != null &&
            normalSprite != null)
        {
            springRenderer.sprite =
                normalSprite;
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        TryActivate(collision);
    }

    private void TryActivate(
        Collision2D collision
    )
    {
        if (!canActivate)
            return;

        if (!collision.collider.CompareTag("Player"))
            return;

        Rigidbody2D playerRb =
            collision.collider
                .GetComponentInParent<Rigidbody2D>();

        if (playerRb == null ||
            springCollider == null)
        {
            return;
        }

        float impactSpeed =
            Mathf.Abs(
                collision.relativeVelocity.y
            );

        if (impactSpeed <
            minImpactSpeedToActivate)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "SpringPad: удар слишком слабый.",
                    this
                );
            }

            return;
        }

        float playerBottomY =
            collision.collider.bounds.min.y;

        float springTopY =
            springCollider.bounds.max.y;

        if (playerBottomY <
            springTopY - playerAboveTolerance)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "SpringPad: игрок не сверху пружины.",
                    this
                );
            }

            return;
        }

        Activate(
            playerRb
        );
    }

    private void Activate(
        Rigidbody2D playerRb
    )
    {
        canActivate = false;

        PlayerLanding playerLanding =
            playerRb.GetComponent<PlayerLanding>();

        /*
         * Сообщаем системе приземления,
         * что игрок оттолкнулся от пружины.
         */
        if (playerLanding != null)
        {
            playerLanding.NotifySpringBounce();
        }

        playerRb.linearVelocity =
            new Vector2(
                playerRb.linearVelocity.x,
                0f
            );

        playerRb.AddForce(
            Vector2.up * launchForce,
            ForceMode2D.Impulse
        );

        PlaySpringSound();
        PlaySpringHaptics();

        if (debugLogs)
        {
            Debug.Log(
                "SpringPad: ПРУЖИНА СРАБОТАЛА!",
                this
            );
        }

        if (visualRoutine != null)
        {
            StopCoroutine(
                visualRoutine
            );
        }

        visualRoutine =
            StartCoroutine(
                SpringVisualRoutine()
            );

        CancelInvoke(
            nameof(ResetCooldown)
        );

        Invoke(
            nameof(ResetCooldown),
            cooldown
        );
    }

    private void PlaySpringSound()
    {
        if (audioSource == null ||
            springSound == null)
        {
            return;
        }

        if (randomizePitch)
        {
            float minPitch =
                Mathf.Min(
                    pitchRange.x,
                    pitchRange.y
                );

            float maxPitch =
                Mathf.Max(
                    pitchRange.x,
                    pitchRange.y
                );

            audioSource.pitch =
                Random.Range(
                    minPitch,
                    maxPitch
                );
        }
        else
        {
            audioSource.pitch =
                originalPitch;
        }

        audioSource.PlayOneShot(
            springSound,
            springVolume
        );
    }

    private void PlaySpringHaptics()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            androidHapticDurationMs,
            iosHapticStyle
        );
    }

    private IEnumerator SpringVisualRoutine()
    {
        if (springRenderer != null &&
            pressedSprite != null)
        {
            springRenderer.sprite =
                pressedSprite;
        }

        yield return new WaitForSeconds(
            pressedTime
        );

        if (springRenderer != null &&
            normalSprite != null)
        {
            springRenderer.sprite =
                normalSprite;
        }

        visualRoutine = null;
    }

    private void ResetCooldown()
    {
        canActivate = true;
    }

    private void OnDisable()
    {
        CancelInvoke(
            nameof(ResetCooldown)
        );

        if (visualRoutine != null)
        {
            StopCoroutine(
                visualRoutine
            );

            visualRoutine = null;
        }

        if (springRenderer != null &&
            normalSprite != null)
        {
            springRenderer.sprite =
                normalSprite;
        }

        if (audioSource != null)
        {
            audioSource.pitch =
                originalPitch;
        }

        canActivate = true;
    }
}