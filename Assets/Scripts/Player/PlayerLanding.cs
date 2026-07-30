using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLanding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Landing Detection")]
    [Tooltip("Минимальная высота падения, после которой включается звук приземления.")]
    [SerializeField, Min(0f)]
    private float minimumLandingDistance = 0.35f;

    [Tooltip("Начиная с этой высоты снимается 1 единица здоровья.")]
    [SerializeField, Min(0f)]
    private float mediumFallDistance = 2.5f;

    [Tooltip("Начиная с этой высоты снимаются 2 единицы здоровья.")]
    [SerializeField, Min(0f)]
    private float heavyFallDistance = 5.5f;

    [Tooltip("Помогает отличать пол и платформу от стены или потолка.")]
    [SerializeField, Range(0f, 1f)]
    private float minimumGroundNormalY = 0.35f;

    [Tooltip(
        "Сколько физических кадров игрок должен действительно находиться " +
        "в воздухе, прежде чем система разрешит новое приземление."
    )]
    [SerializeField, Range(1, 10)]
    private int airborneFramesToArmLanding = 2;

    [Header("Landing Audio")]
    [SerializeField] private AudioSource landingAudioSource;

    [Tooltip("Обычный звук приземления без Ow.")]
    [SerializeField] private AudioClip normalLandingClip;

    [Tooltip("Соединённый звук приземления вместе с Ow.")]
    [SerializeField] private AudioClip hurtLandingClip;

    [Header("Small Landing Sound")]
    [SerializeField, Range(0f, 1f)]
    private float smallLandingVolume = 0.75f;

    [SerializeField, Range(0.5f, 1.5f)]
    private float smallLandingPitch = 1f;

    [Header("Medium Landing Sound")]
    [SerializeField, Range(0f, 1f)]
    private float mediumLandingVolume = 0.9f;

    [SerializeField, Range(0.5f, 1.5f)]
    private float mediumLandingPitch = 1f;

    [Header("Heavy Landing Sound")]
    [SerializeField, Range(0f, 1f)]
    private float heavyLandingVolume = 1f;

    [SerializeField, Range(0.5f, 1.5f)]
    private float heavyLandingPitch = 0.94f;

    [Header("Landing Vibration — Android")]
    [SerializeField, Range(5, 200)]
    private int smallLandingVibrationMs = 12;

    [SerializeField, Range(5, 200)]
    private int mediumLandingVibrationMs = 28;

    [SerializeField, Range(5, 200)]
    private int heavyLandingVibrationMs = 45;

    [Header("Landing Vibration — iOS")]
    [SerializeField]
    private MicroHaptics.IOSHapticStyle smallLandingIOSStyle =
        MicroHaptics.IOSHapticStyle.Light;

    [SerializeField]
    private MicroHaptics.IOSHapticStyle mediumLandingIOSStyle =
        MicroHaptics.IOSHapticStyle.Medium;

    [SerializeField]
    private MicroHaptics.IOSHapticStyle heavyLandingIOSStyle =
        MicroHaptics.IOSHapticStyle.Heavy;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private bool initialized;

    /*
     * true только тогда, когда игрок действительно
     * находился в воздухе и новое приземление разрешено.
     */
    private bool landingArmed;

    /*
     * Отслеживается высота текущего полёта.
     */
    private bool isTrackingFall;

    /*
     * После пружины следующее касание обычной поверхности
     * должно дать хотя бы обычный звук.
     */
    private bool forceNextLandingSoundAfterSpring;

    private float highestAirPositionY;
    private float previousVerticalVelocity;

    private int consecutiveAirborneFrames;
    private int lastProcessedPhysicsFrame = -1;

    private void Awake()
    {
        FindReferences();

        if (landingAudioSource == null)
        {
            landingAudioSource =
                gameObject.AddComponent<AudioSource>();

            landingAudioSource.playOnAwake = false;
            landingAudioSource.loop = false;
            landingAudioSource.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        InitializeState();
    }

    private void FixedUpdate()
    {
        if (!initialized)
            InitializeState();

        if (playerJump == null ||
            rb == null)
        {
            return;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            ResetLandingState();
            return;
        }

        if (ClimbHook.PlayerIsOnHook)
        {
            ResetLandingState();
            return;
        }

        bool isGrounded =
            playerJump.IsGrounded();

        float currentPositionY =
            transform.position.y;

        previousVerticalVelocity =
            rb.linearVelocity.y;

        if (!isGrounded)
        {
            consecutiveAirborneFrames++;

            /*
             * Одного ложного кадра GroundCheck недостаточно.
             * Только после нескольких подряд кадров в воздухе
             * разрешаем новое приземление.
             */
            if (!landingArmed &&
                consecutiveAirborneFrames >=
                airborneFramesToArmLanding)
            {
                landingArmed = true;
                isTrackingFall = true;

                highestAirPositionY =
                    currentPositionY;

                if (debugLogs)
                {
                    Debug.Log(
                        "PlayerLanding: новое приземление разрешено.",
                        this
                    );
                }
            }

            if (isTrackingFall &&
                currentPositionY >
                highestAirPositionY)
            {
                highestAirPositionY =
                    currentPositionY;
            }
        }
        else
        {
            consecutiveAirborneFrames = 0;
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        TryProcessLanding(
            collision
        );
    }

    private void OnCollisionStay2D(
        Collision2D collision
    )
    {
        /*
         * Нужен для TilemapCollider2D и CompositeCollider2D,
         * когда разные платформы входят в один общий коллайдер.
         */
        TryProcessLanding(
            collision
        );
    }

    private void TryProcessLanding(
        Collision2D collision
    )
    {
        if (!initialized ||
            !landingArmed ||
            !isTrackingFall)
        {
            return;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return;
        }

        SpringPad springPad =
            collision.collider
                .GetComponentInParent<SpringPad>();

        /*
         * Падение непосредственно на пружину
         * никогда не считается обычным приземлением.
         */
        if (springPad != null)
            return;

        if (!HasGroundContact(collision))
            return;

        /*
         * Игрок ещё явно движется вверх.
         * Например, касается бокового края платформы.
         */
        if (previousVerticalVelocity > 0.15f)
            return;

        /*
         * Enter и Stay могут прийти в одном кадре.
         */
        int currentFrame =
            Time.frameCount;

        if (lastProcessedPhysicsFrame ==
            currentFrame)
        {
            return;
        }

        lastProcessedPhysicsFrame =
            currentFrame;

        float fallDistance =
            Mathf.Max(
                0f,
                highestAirPositionY -
                transform.position.y
            );

        bool forceNormalSound =
            forceNextLandingSoundAfterSpring;

        /*
         * Сразу блокируем повторную обработку.
         * Она будет разрешена только после настоящего
         * нового отрыва от земли.
         */
        landingArmed = false;
        isTrackingFall = false;
        consecutiveAirborneFrames = 0;
        forceNextLandingSoundAfterSpring = false;

        HandleLanding(
            fallDistance,
            forceNormalSound
        );
    }

    private bool HasGroundContact(
        Collision2D collision
    )
    {
        int contactCount =
            collision.contactCount;

        for (int i = 0;
             i < contactCount;
             i++)
        {
            ContactPoint2D contact =
                collision.GetContact(i);

            if (contact.normal.y >=
                minimumGroundNormalY)
            {
                return true;
            }
        }

        return false;
    }

    public void NotifySpringBounce()
    {
        /*
         * Пружина сама гарантированно начинает новый полёт.
         * Здесь не ждём обычные два кадра GroundCheck.
         */
        landingArmed = true;
        isTrackingFall = true;

        consecutiveAirborneFrames =
            airborneFramesToArmLanding;

        highestAirPositionY =
            transform.position.y;

        forceNextLandingSoundAfterSpring = true;

        previousVerticalVelocity = 0f;
        initialized = true;

        if (debugLogs)
        {
            Debug.Log(
                "PlayerLanding: начат новый полёт после пружины.",
                this
            );
        }
    }

    private void FindReferences()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    private void InitializeState()
    {
        FindReferences();

        if (playerJump == null ||
            rb == null)
        {
            initialized = false;

            if (debugLogs)
            {
                Debug.LogWarning(
                    "PlayerLanding: не найден PlayerJump или Rigidbody2D.",
                    this
                );
            }

            return;
        }

        bool isGrounded =
            playerJump.IsGrounded();

        if (isGrounded)
        {
            landingArmed = false;
            isTrackingFall = false;
            consecutiveAirborneFrames = 0;
        }
        else
        {
            landingArmed = true;
            isTrackingFall = true;

            consecutiveAirborneFrames =
                airborneFramesToArmLanding;

            highestAirPositionY =
                transform.position.y;
        }

        previousVerticalVelocity =
            rb.linearVelocity.y;

        initialized = true;
    }

    private void ResetLandingState()
    {
        landingArmed = false;
        isTrackingFall = false;

        forceNextLandingSoundAfterSpring = false;

        consecutiveAirborneFrames = 0;

        highestAirPositionY =
            transform.position.y;
    }

    private void HandleLanding(
        float fallDistance,
        bool forceNormalLandingSound
    )
    {
        if (fallDistance <
            minimumLandingDistance)
        {
            if (forceNormalLandingSound)
            {
                PlaySmallLanding();

                if (debugLogs)
                {
                    Debug.Log(
                        $"PlayerLanding: касание земли после пружины. " +
                        $"Высота: {fallDistance:F2}. Урон: 0.",
                        this
                    );
                }
            }
            else if (debugLogs)
            {
                Debug.Log(
                    $"PlayerLanding: слишком маленькое падение. " +
                    $"Высота: {fallDistance:F2}.",
                    this
                );
            }

            return;
        }

        if (fallDistance >=
            heavyFallDistance)
        {
            PlayLandingSound(
                hurtLandingClip,
                heavyLandingVolume,
                heavyLandingPitch
            );

            MicroHaptics.Pulse(
                heavyLandingVibrationMs,
                heavyLandingIOSStyle
            );

            if (playerHealth != null)
            {
                playerHealth.TakeFallDamage(2);
            }

            if (debugLogs)
            {
                Debug.Log(
                    $"PlayerLanding: сильное падение. " +
                    $"Высота: {fallDistance:F2}. Урон: 2.",
                    this
                );
            }

            return;
        }

        if (fallDistance >=
            mediumFallDistance)
        {
            PlayLandingSound(
                hurtLandingClip,
                mediumLandingVolume,
                mediumLandingPitch
            );

            MicroHaptics.Pulse(
                mediumLandingVibrationMs,
                mediumLandingIOSStyle
            );

            if (playerHealth != null)
            {
                playerHealth.TakeFallDamage(1);
            }

            if (debugLogs)
            {
                Debug.Log(
                    $"PlayerLanding: среднее падение. " +
                    $"Высота: {fallDistance:F2}. Урон: 1.",
                    this
                );
            }

            return;
        }

        PlaySmallLanding();

        if (debugLogs)
        {
            Debug.Log(
                $"PlayerLanding: маленькое падение. " +
                $"Высота: {fallDistance:F2}. Урон: 0.",
                this
            );
        }
    }

    private void PlaySmallLanding()
    {
        PlayLandingSound(
            normalLandingClip,
            smallLandingVolume,
            smallLandingPitch
        );

        MicroHaptics.Pulse(
            smallLandingVibrationMs,
            smallLandingIOSStyle
        );
    }

    private void PlayLandingSound(
        AudioClip clip,
        float volume,
        float pitch
    )
    {
        if (landingAudioSource == null ||
            clip == null)
        {
            return;
        }

        landingAudioSource.pitch =
            Mathf.Clamp(
                pitch,
                0.5f,
                1.5f
            );

        landingAudioSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }

    private void OnValidate()
    {
        minimumLandingDistance =
            Mathf.Max(
                0f,
                minimumLandingDistance
            );

        mediumFallDistance =
            Mathf.Max(
                minimumLandingDistance,
                mediumFallDistance
            );

        heavyFallDistance =
            Mathf.Max(
                mediumFallDistance + 0.01f,
                heavyFallDistance
            );

        airborneFramesToArmLanding =
            Mathf.Max(
                1,
                airborneFramesToArmLanding
            );
    }
}