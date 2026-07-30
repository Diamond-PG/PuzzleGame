using UnityEngine;

public class ClimbHook : MonoBehaviour
{
    public static bool PlayerIsOnHook { get; private set; }

    private static float climbVerticalInput;
    private static ClimbHook activeClimbZone;

    private static float savedPlayerGravityScale;
    private static bool playerGravityWasSaved;

    public static void SetClimbVerticalInput(float value)
    {
        climbVerticalInput = Mathf.Clamp(
            value,
            -1f,
            1f
        );
    }

    [Header("Climb Settings")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private bool disableGravityWhileClimbing = true;

    [Tooltip("Скорость выхода игрока влево или вправо со скобы.")]
    [SerializeField] private float horizontalExitSpeed = 5f;

    [Header("Climb Sound")]
    [SerializeField] private AudioClip climbSound;

    [SerializeField, Range(0f, 1f)]
    private float climbSoundVolume = 0.7f;

    [Tooltip("Интервал между звуками во время лазания.")]
    [SerializeField, Min(0.05f)]
    private float climbSoundInterval = 0.22f;

    [Tooltip("Небольшое случайное изменение высоты звука.")]
    [SerializeField]
    private bool randomizePitch = true;

    [SerializeField]
    private Vector2 climbPitchRange =
        new Vector2(0.96f, 1.04f);

    [Header("Climb Haptics")]
    [SerializeField]
    private bool useClimbHaptics = true;

    [Tooltip("Длительность каждого импульса на Android.")]
    [SerializeField, Range(5, 50)]
    private int androidClimbHapticDurationMs = 10;

    [SerializeField]
    private MicroHaptics.IOSHapticStyle iosClimbHapticStyle =
        MicroHaptics.IOSHapticStyle.Selection;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool playerInside;

    private Rigidbody2D playerRb;
    private PlayerController playerController;
    private PlayerVisual playerVisual;

    private AudioSource climbAudioSource;

    private float nextClimbSoundTime;

    private void Awake()
    {
        climbAudioSource = GetComponent<AudioSource>();

        if (climbAudioSource == null)
        {
            climbAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        climbAudioSource.playOnAwake = false;
        climbAudioSource.loop = false;
        climbAudioSource.spatialBlend = 0f;
        climbAudioSource.volume = climbSoundVolume;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        Rigidbody2D detectedRb =
            collision.GetComponent<Rigidbody2D>();

        PlayerController detectedController =
            collision.GetComponent<PlayerController>();

        PlayerVisual detectedVisual =
            collision.GetComponent<PlayerVisual>();

        if (detectedRb == null ||
            detectedController == null)
        {
            return;
        }

        playerRb = detectedRb;
        playerController = detectedController;
        playerVisual = detectedVisual;

        playerInside = true;

        if (!playerGravityWasSaved)
        {
            savedPlayerGravityScale =
                playerRb.gravityScale;

            playerGravityWasSaved = true;
        }

        activeClimbZone = this;

        PlayerIsOnHook = true;
        climbVerticalInput = 0f;
        nextClimbSoundTime = 0f;

        if (disableGravityWhileClimbing)
            playerRb.gravityScale = 0f;

        playerRb.linearVelocity = Vector2.zero;

        StopClimbSound();

        if (playerVisual != null)
            playerVisual.SetClimbLook(0f);

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook: игрок вошёл в зону {name}",
                this
            );
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (!playerInside)
            return;

        if (activeClimbZone == null)
        {
            activeClimbZone = this;
            PlayerIsOnHook = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInside = false;
        StopClimbSound();

        /*
         * Старая зона не сбрасывает новую активную зону.
         */
        if (activeClimbZone != this)
        {
            ClearLocalReferences();

            if (debugLogs)
            {
                Debug.Log(
                    $"ClimbHook: выход из неактивной зоны {name}",
                    this
                );
            }

            return;
        }

        LeaveActiveClimbZone();

        if (debugLogs)
        {
            Debug.Log(
                $"ClimbHook: выход из активной зоны {name}",
                this
            );
        }
    }

    private void FixedUpdate()
    {
        if (!playerInside ||
            activeClimbZone != this ||
            playerRb == null ||
            playerController == null)
        {
            StopClimbSound();
            return;
        }

        float vertical = climbVerticalInput;

        if (Mathf.Abs(vertical) < 0.1f)
        {
            vertical =
                playerController.GetClimbVerticalInput();
        }

        float horizontal =
            playerController.GetInput().x;

        /*
         * Приоритет у вертикального лазания.
         */
        if (Mathf.Abs(vertical) > 0.1f)
        {
            playerRb.linearVelocity =
                new Vector2(
                    0f,
                    vertical * climbSpeed
                );

            if (playerVisual != null)
                playerVisual.SetClimbLook(vertical);

            UpdateClimbFeedback();
            return;
        }

        /*
         * Если игрок нажал влево или вправо,
         * разрешаем ему выйти из зоны лестницы.
         *
         * Раньше здесь скорость полностью обнулялась,
         * поэтому мобильное управление блокировалось.
         */
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            playerRb.linearVelocity =
                new Vector2(
                    horizontal * horizontalExitSpeed,
                    0f
                );

            StopClimbSound();
            return;
        }

        /*
         * Нет ввода — игрок спокойно висит на скобе.
         */
        playerRb.linearVelocity = Vector2.zero;
        StopClimbSound();
    }

    private void UpdateClimbFeedback()
    {
        if (climbSound == null ||
            climbAudioSource == null)
        {
            return;
        }

        if (Time.time < nextClimbSoundTime)
            return;

        if (randomizePitch)
        {
            float minimumPitch =
                Mathf.Min(
                    climbPitchRange.x,
                    climbPitchRange.y
                );

            float maximumPitch =
                Mathf.Max(
                    climbPitchRange.x,
                    climbPitchRange.y
                );

            climbAudioSource.pitch =
                Random.Range(
                    minimumPitch,
                    maximumPitch
                );
        }
        else
        {
            climbAudioSource.pitch = 1f;
        }

        climbAudioSource.volume =
            climbSoundVolume;

        climbAudioSource.PlayOneShot(
            climbSound,
            climbSoundVolume
        );

        PlayClimbHaptics();

        nextClimbSoundTime =
            Time.time + climbSoundInterval;
    }

    private void PlayClimbHaptics()
    {
        if (!useClimbHaptics)
            return;

        MicroHaptics.Pulse(
            androidClimbHapticDurationMs,
            iosClimbHapticStyle
        );
    }

    private void StopClimbSound()
    {
        if (climbAudioSource == null)
            return;

        if (climbAudioSource.isPlaying)
            climbAudioSource.Stop();

        climbAudioSource.pitch = 1f;
    }

    private void LeaveActiveClimbZone()
    {
        activeClimbZone = null;
        PlayerIsOnHook = false;
        climbVerticalInput = 0f;

        if (playerRb != null)
        {
            if (playerGravityWasSaved)
            {
                playerRb.gravityScale =
                    savedPlayerGravityScale;
            }

            playerRb.linearVelocity =
                new Vector2(
                    playerRb.linearVelocity.x,
                    0f
                );
        }

        playerGravityWasSaved = false;

        if (playerVisual != null)
            playerVisual.ClearClimbLook();

        ClearLocalReferences();
    }

    private void ClearLocalReferences()
    {
        playerRb = null;
        playerController = null;
        playerVisual = null;
    }

    private void OnDisable()
    {
        StopClimbSound();

        if (activeClimbZone == this)
        {
            if (playerRb != null &&
                playerGravityWasSaved)
            {
                playerRb.gravityScale =
                    savedPlayerGravityScale;
            }

            activeClimbZone = null;
            PlayerIsOnHook = false;
            climbVerticalInput = 0f;
            playerGravityWasSaved = false;
        }

        playerInside = false;

        ClearLocalReferences();
    }
}