using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrisonBreakDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform doorVisual;
    [SerializeField] private SpriteRenderer doorSpriteRenderer;
    [SerializeField] private Collider2D doorCollider;

    [SerializeField] private Transform player;
    [SerializeField] private PlayerKick playerKick;

    [Header("Background Behind Door")]
    [Tooltip("Чёрный фон за решёткой. Видим у целой и повреждённой двери, скрываем после разрушения.")]
    [SerializeField] private GameObject blackBackground;

    [Header("Door Sprites")]
    [SerializeField] private Sprite intactSprite;
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private Sprite brokenSprite;

    [Header("Door Settings")]
    [SerializeField] private int hitsToBreak = 8;
    [SerializeField] private int damagedSpriteHit = 4;
    [SerializeField] private float interactDistance = 1.6f;

    [Tooltip("Через сколько секунд после начала удара ногой дверь получает удар.")]
    [SerializeField] private float kickImpactDelay = 0.08f;

    [Header("Cartoon Punch Animation")]
    [SerializeField] private float punchDuration = 0.16f;

    [SerializeField] private float minPunchScaleX = 1.035f;
    [SerializeField] private float maxPunchScaleX = 1.10f;

    [SerializeField] private float minPunchScaleY = 0.985f;
    [SerializeField] private float maxPunchScaleY = 0.95f;

    [SerializeField] private float maxPunchMoveX = 0.045f;

    [Header("Final Break")]
    [SerializeField] private float finalPunchScaleX = 1.14f;
    [SerializeField] private float finalPunchScaleY = 0.92f;

    [SerializeField] private float finalPunchDuration = 0.20f;

    [Tooltip("Небольшая пауза перед переключением на сломанную дверь.")]
    [SerializeField] private float finalBreakPause = 0.035f;

    // =========================================================
    // WOOD CHIPS - 4TH HIT
    // =========================================================

    [Header("Wood Chips - 4th Hit")]

    [Tooltip("Две маленькие щепки, которые вылетают на 4-м ударе.")]
    [SerializeField] private Sprite[] damagedHitChips;

    [Tooltip("Размер маленьких щепок.")]
    [SerializeField] private float damagedChipScale = 0.08f;

    [Tooltip("Разброс маленьких щепок по X.")]
    [SerializeField] private float damagedChipSpreadX = 0.10f;

    [Tooltip("Насколько щепки сначала подлетают вверх.")]
    [SerializeField] private float damagedChipLift = 0.10f;

    // =========================================================
    // WOOD CHIPS - FINAL
    // =========================================================

    [Header("Wood Chips - Final Break")]

    [Tooltip("Все пять щепок, которые вылетают на последнем ударе.")]
    [SerializeField] private Sprite[] finalBreakChips;

    [Tooltip("Минимальный размер финальных щепок.")]
    [SerializeField] private float finalChipScaleMin = 0.08f;

    [Tooltip("Максимальный размер финальных щепок.")]
    [SerializeField] private float finalChipScaleMax = 0.13f;

    [Tooltip("Разброс финальных щепок по X.")]
    [SerializeField] private float finalChipSpreadX = 0.18f;

    [Tooltip("Насколько финальные щепки подлетают вверх.")]
    [SerializeField] private float finalChipLift = 0.14f;

    // =========================================================
    // CHIP ANIMATION
    // =========================================================

    [Header("Wood Chip Animation")]

    [Tooltip("Сколько длится первый короткий вылет щепки.")]
    [SerializeField] private float chipLaunchDuration = 0.16f;

    [Tooltip("Сколько длится падение щепки на пол.")]
    [SerializeField] private float chipFallDuration = 0.34f;

    [Tooltip("Сколько секунд щепка лежит на полу.")]
    [SerializeField] private float chipStayDuration = 2.0f;

    [Tooltip("Сколько длится плавное исчезновение.")]
    [SerializeField] private float chipFadeDuration = 0.25f;

    [Tooltip("Насколько ниже центра двери находится пол.")]
    [SerializeField] private float chipFloorOffsetY = 0.42f;

    [Tooltip("Небольшой подъём над полом, чтобы щепки не выглядели утопленными.")]
    [SerializeField] private float chipGroundLift = 0.015f;

    [Tooltip("Минимальный финальный угол щепки на полу.")]
    [SerializeField] private float chipEndRotationMin = 55f;

    [Tooltip("Максимальный финальный угол щепки на полу.")]
    [SerializeField] private float chipEndRotationMax = 125f;

    // =========================================================
    // HAPTICS
    // =========================================================

    [Header("Haptics")]
    [SerializeField] private bool useHaptics = true;

    [SerializeField, Range(5, 120)]
    private int normalHitHapticMs = 22;

    [SerializeField, Range(5, 200)]
    private int finalHitHapticMs = 100;

    // =========================================================
    // AUDIO
    // =========================================================

    [Header("Door Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip breakSound;

    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float breakVolume = 1f;

    // =========================================================
    // DEBUG
    // =========================================================

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Camera mainCamera;

    private int hits;
    private bool isBusy;
    private bool isBroken;

    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRotation;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (doorVisual == null)
            doorVisual = transform;

        if (doorSpriteRenderer == null && doorVisual != null)
            doorSpriteRenderer = doorVisual.GetComponent<SpriteRenderer>();

        if (doorSpriteRenderer == null)
            doorSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorCollider == null && doorVisual != null)
            doorCollider = doorVisual.GetComponent<Collider2D>();

        if (playerKick == null && player != null)
            playerKick = player.GetComponent<PlayerKick>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (doorVisual != null)
        {
            originalLocalPosition = doorVisual.localPosition;
            originalLocalScale = doorVisual.localScale;
            originalLocalRotation = doorVisual.localRotation;
        }
    }

    private void Start()
    {
        ResetDoorState();
    }

    private void OnEnable()
    {
        ResetDoorState();
    }

    private void OnValidate()
    {
        if (hitsToBreak < 2)
            hitsToBreak = 2;

        if (damagedSpriteHit < 1)
            damagedSpriteHit = 1;

        if (damagedSpriteHit >= hitsToBreak)
            damagedSpriteHit = hitsToBreak - 1;

        if (damagedChipScale < 0.01f)
            damagedChipScale = 0.01f;

        if (finalChipScaleMin < 0.01f)
            finalChipScaleMin = 0.01f;

        if (finalChipScaleMax < finalChipScaleMin)
            finalChipScaleMax = finalChipScaleMin;

        if (chipLaunchDuration < 0.01f)
            chipLaunchDuration = 0.01f;

        if (chipFallDuration < 0.01f)
            chipFallDuration = 0.01f;

        if (chipStayDuration < 0f)
            chipStayDuration = 0f;

        if (chipFadeDuration < 0.01f)
            chipFadeDuration = 0.01f;
    }

    private void ResetDoorState()
    {
        StopAllCoroutines();

        hits = 0;
        isBusy = false;
        isBroken = false;

        if (doorVisual != null)
        {
            doorVisual.localPosition = originalLocalPosition;
            doorVisual.localScale = originalLocalScale;
            doorVisual.localRotation = originalLocalRotation;
        }

        if (doorSpriteRenderer != null)
        {
            doorSpriteRenderer.enabled = true;
            doorSpriteRenderer.color = Color.white;

            if (intactSprite != null)
                doorSpriteRenderer.sprite = intactSprite;
        }

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (blackBackground != null)
            blackBackground.SetActive(true);
    }

    private void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (isBroken ||
            isBusy ||
            mainCamera == null ||
            doorCollider == null)
        {
            return;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryHitDoor(Mouse.current.position.ReadValue());
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryHitDoor(
                Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }
    }

    private void TryHitDoor(Vector2 screenPosition)
    {
        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(mainCamera.transform.position.z)
                )
            );

        Vector2 point2D =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        Collider2D hitCollider =
            Physics2D.OverlapPoint(point2D);

        if (hitCollider == null)
            return;

        if (hitCollider != doorCollider)
            return;

        if (player == null)
        {
            Debug.LogWarning(
                "Player не назначен в PrisonBreakDoor!"
            );

            return;
        }

        if (playerKick == null)
        {
            playerKick =
                player.GetComponent<PlayerKick>();
        }

        if (playerKick == null)
        {
            Debug.LogWarning(
                "PlayerKick не найден на Player!"
            );

            return;
        }

        float distance =
            Vector2.Distance(
                player.position,
                doorVisual.position
            );

        if (distance > interactDistance)
        {
            if (debugLogs)
                Debug.Log("Слишком далеко от двери.");

            return;
        }

        bool kickStarted =
            playerKick.KickToward(
                doorVisual.position
            );

        if (!kickStarted)
            return;

        StartCoroutine(
            HitDoorSequence()
        );
    }

    private IEnumerator HitDoorSequence()
    {
        isBusy = true;

        yield return new WaitForSeconds(
            kickImpactDelay
        );

        hits++;

        if (debugLogs)
        {
            Debug.Log(
                "Door hit: " +
                hits +
                " / " +
                hitsToBreak
            );
        }

        if (hits < hitsToBreak)
        {
            PlayHitHaptic();
            PlayHitSound();

            float hitProgress =
                Mathf.InverseLerp(
                    1f,
                    hitsToBreak - 1f,
                    hits
                );

            float scaleX =
                Mathf.Lerp(
                    minPunchScaleX,
                    maxPunchScaleX,
                    hitProgress
                );

            float scaleY =
                Mathf.Lerp(
                    minPunchScaleY,
                    maxPunchScaleY,
                    hitProgress
                );

            yield return StartCoroutine(
                CartoonPunch(
                    punchDuration,
                    scaleX,
                    scaleY,
                    maxPunchMoveX
                )
            );

            if (hits == damagedSpriteHit)
            {
                if (damagedSprite != null &&
                    doorSpriteRenderer != null)
                {
                    doorSpriteRenderer.sprite =
                        damagedSprite;
                }

                SpawnDamagedHitChips();
            }
        }
        else
        {
            PlayBreakHaptic();
            PlayBreakSound();

            yield return StartCoroutine(
                CartoonPunch(
                    finalPunchDuration,
                    finalPunchScaleX,
                    finalPunchScaleY,
                    maxPunchMoveX * 1.5f
                )
            );

            if (finalBreakPause > 0f)
            {
                yield return new WaitForSeconds(
                    finalBreakPause
                );
            }

            if (doorSpriteRenderer != null &&
                brokenSprite != null)
            {
                doorSpriteRenderer.sprite =
                    brokenSprite;
            }

            if (blackBackground != null)
                blackBackground.SetActive(false);

            SpawnFinalBreakChips();

            if (doorCollider != null)
                doorCollider.enabled = false;

            isBroken = true;

            if (debugLogs)
            {
                Debug.Log(
                    "Door broken. Проход открыт."
                );
            }
        }

        if (doorVisual != null)
        {
            doorVisual.localPosition =
                originalLocalPosition;

            doorVisual.localScale =
                originalLocalScale;

            doorVisual.localRotation =
                originalLocalRotation;
        }

        isBusy = false;
    }

    private IEnumerator CartoonPunch(
        float duration,
        float targetScaleX,
        float targetScaleY,
        float moveX
    )
    {
        if (doorVisual == null)
            yield break;

        float timer = 0f;

        float direction = 1f;

        if (player != null)
        {
            direction =
                player.position.x <= doorVisual.position.x
                    ? 1f
                    : -1f;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            float punch =
                Mathf.Sin(
                    t * Mathf.PI
                );

            float currentScaleX =
                Mathf.Lerp(
                    1f,
                    targetScaleX,
                    punch
                );

            float currentScaleY =
                Mathf.Lerp(
                    1f,
                    targetScaleY,
                    punch
                );

            doorVisual.localScale =
                new Vector3(
                    originalLocalScale.x * currentScaleX,
                    originalLocalScale.y * currentScaleY,
                    originalLocalScale.z
                );

            doorVisual.localPosition =
                originalLocalPosition +
                new Vector3(
                    direction *
                    moveX *
                    punch,
                    0f,
                    0f
                );

            yield return null;
        }

        doorVisual.localScale =
            originalLocalScale;

        doorVisual.localPosition =
            originalLocalPosition;
    }

    private void SpawnDamagedHitChips()
    {
        if (damagedHitChips == null ||
            damagedHitChips.Length == 0)
        {
            return;
        }

        int count =
            Mathf.Min(
                2,
                damagedHitChips.Length
            );

        for (int i = 0; i < count; i++)
        {
            Sprite sprite =
                damagedHitChips[i];

            if (sprite == null)
                continue;

            float side =
                i == 0 ? -1f : 1f;

            SpawnAnimatedChip(
                sprite,
                damagedChipScale,
                side * damagedChipSpreadX,
                damagedChipLift,
                i
            );
        }
    }

    private void SpawnFinalBreakChips()
    {
        if (finalBreakChips == null ||
            finalBreakChips.Length == 0)
        {
            return;
        }

        int count =
            Mathf.Min(
                5,
                finalBreakChips.Length
            );

        for (int i = 0; i < count; i++)
        {
            Sprite sprite =
                finalBreakChips[i];

            if (sprite == null)
                continue;

            float randomScale =
                Random.Range(
                    finalChipScaleMin,
                    finalChipScaleMax
                );

            float horizontalOffset =
                Random.Range(
                    -finalChipSpreadX,
                    finalChipSpreadX
                );

            float lift =
                Random.Range(
                    finalChipLift * 0.70f,
                    finalChipLift
                );

            SpawnAnimatedChip(
                sprite,
                randomScale,
                horizontalOffset,
                lift,
                i
            );
        }
    }

    private void SpawnAnimatedChip(
        Sprite sprite,
        float scale,
        float horizontalOffset,
        float lift,
        int index
    )
    {
        if (sprite == null ||
            doorVisual == null)
        {
            return;
        }

        GameObject chip =
            new GameObject(
                "DoorWoodChip"
            );

        SpriteRenderer chipRenderer =
            chip.AddComponent<SpriteRenderer>();

        chipRenderer.sprite =
            sprite;

        if (doorSpriteRenderer != null)
        {
            chipRenderer.sortingLayerID =
                doorSpriteRenderer.sortingLayerID;

            chipRenderer.sortingOrder =
                doorSpriteRenderer.sortingOrder + 1;

            if (doorSpriteRenderer.sharedMaterial != null)
            {
                chipRenderer.sharedMaterial =
                    doorSpriteRenderer.sharedMaterial;
            }
        }

        chip.transform.localScale =
            Vector3.one * scale;

        Vector3 startPosition =
            doorVisual.position +
            new Vector3(
                Random.Range(-0.06f, 0.06f),
                Random.Range(-0.03f, 0.07f),
                0f
            );

        chip.transform.position =
            startPosition;

        float startRotation =
            Random.Range(
                -20f,
                20f
            );

        chip.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                startRotation
            );

        StartCoroutine(
            AnimateDoorChip(
                chip,
                chipRenderer,
                startPosition,
                horizontalOffset,
                lift,
                startRotation,
                index
            )
        );
    }

    private IEnumerator AnimateDoorChip(
        GameObject chip,
        SpriteRenderer chipRenderer,
        Vector3 startPosition,
        float horizontalOffset,
        float lift,
        float startRotation,
        int index
    )
    {
        if (chip == null)
            yield break;

        Color startColor =
            Color.white;

        if (chipRenderer != null)
            startColor = chipRenderer.color;

        Vector3 launchEndPosition =
            startPosition +
            new Vector3(
                horizontalOffset,
                lift,
                0f
            );

        float launchRotation =
            startRotation +
            Random.Range(
                -30f,
                30f
            );

        float timer = 0f;

        while (timer < chipLaunchDuration)
        {
            if (chip == null)
                yield break;

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    chipLaunchDuration
                );

            float eased =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            chip.transform.position =
                Vector3.Lerp(
                    startPosition,
                    launchEndPosition,
                    eased
                );

            chip.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        startRotation,
                        launchRotation,
                        t
                    )
                );

            yield return null;
        }

        Vector3 fallStartPosition =
            chip.transform.position;

        float floorY =
            doorVisual.position.y -
            chipFloorOffsetY +
            chipGroundLift;

        Vector3 fallEndPosition =
            new Vector3(
                fallStartPosition.x +
                Random.Range(
                    -0.05f,
                    0.05f
                ),
                floorY,
                fallStartPosition.z
            );

        float endRotation =
            index % 2 == 0
                ? -Random.Range(
                    chipEndRotationMin,
                    chipEndRotationMax
                )
                : Random.Range(
                    chipEndRotationMin,
                    chipEndRotationMax
                );

        float fallTimer = 0f;

        while (fallTimer < chipFallDuration)
        {
            if (chip == null)
                yield break;

            fallTimer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    fallTimer /
                    chipFallDuration
                );

            float fallCurve =
                t * t;

            chip.transform.position =
                Vector3.Lerp(
                    fallStartPosition,
                    fallEndPosition,
                    fallCurve
                );

            chip.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        launchRotation,
                        endRotation,
                        t
                    )
                );

            yield return null;
        }

        chip.transform.position =
            fallEndPosition;

        chip.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                endRotation
            );

        if (chipStayDuration > 0f)
        {
            yield return new WaitForSeconds(
                chipStayDuration
            );
        }

        float fadeTimer = 0f;

        while (fadeTimer < chipFadeDuration)
        {
            if (chip == null)
                yield break;

            fadeTimer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    fadeTimer /
                    chipFadeDuration
                );

            if (chipRenderer != null)
            {
                Color color =
                    startColor;

                color.a =
                    Mathf.Lerp(
                        startColor.a,
                        0f,
                        t
                    );

                chipRenderer.color =
                    color;
            }

            yield return null;
        }

        if (chip != null)
            Destroy(chip);
    }

    private void PlayHitHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            normalHitHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }

    private void PlayBreakHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            finalHitHapticMs,
            MicroHaptics.IOSHapticStyle.Heavy
        );
    }

    private void PlayHitSound()
    {
        if (audioSource == null ||
            hitSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            hitSound,
            hitVolume
        );
    }

    private void PlayBreakSound()
    {
        if (audioSource == null ||
            breakSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            breakSound,
            breakVolume
        );
    }
}