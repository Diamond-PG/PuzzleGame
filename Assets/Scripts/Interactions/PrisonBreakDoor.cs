using System.Collections;
using UnityEngine;

public class PrisonBreakDoor : MonoBehaviour
{
    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("REFERENCES")]

    [SerializeField]
    private Transform doorVisual;

    [SerializeField]
    private SpriteRenderer doorSpriteRenderer;

    [SerializeField]
    private Collider2D doorCollider;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private string playerTag = "Player";

    // ============================================================
    // BACKGROUND
    // ============================================================

    [Header("BACKGROUND BEHIND DOOR")]

    [Tooltip(
        "Чёрный фон за решёткой. " +
        "После разрушения двери скрывается."
    )]
    [SerializeField]
    private GameObject blackBackground;

    // ============================================================
    // SPRITES
    // ============================================================

    [Header("DOOR SPRITES")]

    [SerializeField]
    private Sprite intactSprite;

    [SerializeField]
    private Sprite damagedSprite;

    [SerializeField]
    private Sprite brokenSprite;

    // ============================================================
    // DOOR SETTINGS
    // ============================================================

    [Header("DOOR SETTINGS")]

    [Tooltip(
        "Сколько ударов нужно для полного разрушения двери."
    )]
    [SerializeField, Min(2)]
    private int hitsToBreak = 8;

    [Tooltip(
        "На каком ударе дверь переключается " +
        "на повреждённый спрайт."
    )]
    [SerializeField, Min(1)]
    private int damagedSpriteHit = 4;

    // ============================================================
    // CARTOON PUNCH
    // ============================================================

    [Header("CARTOON PUNCH ANIMATION")]

    [SerializeField]
    private float punchDuration = 0.16f;

    [SerializeField]
    private float minPunchScaleX = 1.035f;

    [SerializeField]
    private float maxPunchScaleX = 1.10f;

    [SerializeField]
    private float minPunchScaleY = 0.985f;

    [SerializeField]
    private float maxPunchScaleY = 0.95f;

    [SerializeField]
    private float maxPunchMoveX = 0.045f;

    // ============================================================
    // FINAL BREAK
    // ============================================================

    [Header("FINAL BREAK")]

    [SerializeField]
    private float finalPunchScaleX = 1.14f;

    [SerializeField]
    private float finalPunchScaleY = 0.92f;

    [SerializeField]
    private float finalPunchDuration = 0.20f;

    [Tooltip(
        "Маленькая пауза перед появлением " +
        "полностью сломанной двери."
    )]
    [SerializeField]
    private float finalBreakPause = 0.035f;

    // ============================================================
    // WOOD CHIPS - DAMAGED HIT
    // ============================================================

    [Header("WOOD CHIPS - 4TH HIT")]

    [Tooltip(
        "Две маленькие щепки на ударе, " +
        "когда появляется повреждённая дверь."
    )]
    [SerializeField]
    private Sprite[] damagedHitChips;

    [SerializeField]
    private float damagedChipScale = 0.08f;

    [SerializeField]
    private float damagedChipSpreadX = 0.10f;

    [SerializeField]
    private float damagedChipLift = 0.10f;

    // ============================================================
    // WOOD CHIPS - FINAL
    // ============================================================

    [Header("WOOD CHIPS - FINAL BREAK")]

    [Tooltip(
        "Щепки, которые вылетают " +
        "при полном разрушении двери."
    )]
    [SerializeField]
    private Sprite[] finalBreakChips;

    [SerializeField]
    private float finalChipScaleMin = 0.08f;

    [SerializeField]
    private float finalChipScaleMax = 0.13f;

    [SerializeField]
    private float finalChipSpreadX = 0.18f;

    [SerializeField]
    private float finalChipLift = 0.14f;

    // ============================================================
    // CHIP ANIMATION
    // ============================================================

    [Header("WOOD CHIP ANIMATION")]

    [SerializeField]
    private float chipLaunchDuration = 0.16f;

    [SerializeField]
    private float chipFallDuration = 0.34f;

    [SerializeField]
    private float chipStayDuration = 2.0f;

    [SerializeField]
    private float chipFadeDuration = 0.25f;

    [SerializeField]
    private float chipFloorOffsetY = 0.42f;

    [SerializeField]
    private float chipGroundLift = 0.015f;

    [SerializeField]
    private float chipEndRotationMin = 55f;

    [SerializeField]
    private float chipEndRotationMax = 125f;

    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("HAPTICS")]

    [SerializeField]
    private bool useHaptics = true;

    [SerializeField, Range(5, 120)]
    private int normalHitHapticMs = 22;

    [SerializeField, Range(5, 200)]
    private int finalHitHapticMs = 100;

    // ============================================================
    // DOOR AUDIO
    // ============================================================

    [Header("DOOR AUDIO")]

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip hitSound;

    [SerializeField]
    private AudioClip breakSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float hitVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField]
    private float breakVolume = 1f;

    // ============================================================
    // PLAYER CELEBRATION AUDIO
    // ============================================================

    [Header("PLAYER CELEBRATION AUDIO")]

    [Tooltip(
        "Включить радостный крик героя " +
        "после полного разрушения двери."
    )]
    [SerializeField]
    private bool usePlayerCelebrationSound = true;

    [Tooltip(
        "Отдельный AudioSource для голоса героя. " +
        "Можно оставить None — тогда используется Door AudioSource."
    )]
    [SerializeField]
    private AudioSource playerCelebrationAudioSource;

    [Tooltip(
        "Радостный крик героя, например 'Woo-hoo!'."
    )]
    [SerializeField]
    private AudioClip playerCelebrationSound;

    [Tooltip(
        "Громкость радостного крика героя."
    )]
    [SerializeField, Range(0f, 1f)]
    private float playerCelebrationVolume = 1f;

    [Tooltip(
        "Задержка после разрушения двери до крика героя. " +
        "Позволяет точно попасть в момент, когда он поднимает руки."
    )]
    [SerializeField, Min(0f)]
    private float playerCelebrationDelay = 0f;

    // ============================================================
    // EDITOR TESTING
    // ============================================================

    [Header("EDITOR TEST START")]

    [Tooltip(
        "Только для тестирования в Unity Editor. " +
        "Если включено, вступление в темнице пропускается, " +
        "дверь сразу считается разрушенной, а герой начинает " +
        "в обычном игровом состоянии там, куда он установлен в Scene. " +
        "В собранной игре этот переключатель игнорируется."
    )]
    [SerializeField]
    private bool skipPrisonIntroForTesting = false;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = false;

    // ============================================================
    // PRIVATE
    // ============================================================

    private int hits;

    private bool isBusy;
    private bool isBroken;

    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRotation;

    public bool IsBroken => isBroken;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (doorVisual == null)
        {
            doorVisual =
                transform;
        }

        if (doorSpriteRenderer == null &&
            doorVisual != null)
        {
            doorSpriteRenderer =
                doorVisual.GetComponent<SpriteRenderer>();
        }

        if (doorSpriteRenderer == null)
        {
            doorSpriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (doorCollider == null)
        {
            doorCollider =
                GetComponent<Collider2D>();
        }

        if (doorCollider == null &&
            doorVisual != null)
        {
            doorCollider =
                doorVisual.GetComponent<Collider2D>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        FindPlayer();

        if (doorVisual != null)
        {
            originalLocalPosition =
                doorVisual.localPosition;

            originalLocalScale =
                doorVisual.localScale;

            originalLocalRotation =
                doorVisual.localRotation;
        }

        /*
         * Устанавливаем тестовое состояние уже в Awake.
         *
         * Благодаря этому PlayerVisual в своём Start()
         * сразу увидит IsBroken = true и включит обычный
         * спрайт без грустного лица и без празднования.
         */
        if (ShouldSkipPrisonIntroForTesting())
        {
            ApplyBrokenStateForTesting();
        }
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (ShouldSkipPrisonIntroForTesting())
        {
            /*
             * Повторно закрепляем тестовое состояние,
             * чтобы другие Start-методы не восстановили дверь.
             */
            ApplyBrokenStateForTesting();
            return;
        }

        ResetDoorState();
    }

    // ============================================================
    // EDITOR TESTING
    // ============================================================

    private bool ShouldSkipPrisonIntroForTesting()
    {
        /*
         * В собранной игре Application.isEditor будет false.
         * Поэтому даже забытая галочка не пропустит вступление.
         */
        return
            Application.isEditor &&
            skipPrisonIntroForTesting;
    }

    private void ApplyBrokenStateForTesting()
    {
        StopAllCoroutines();

        hits =
            hitsToBreak;

        isBusy =
            false;

        isBroken =
            true;

        if (doorVisual != null)
        {
            doorVisual.localPosition =
                originalLocalPosition;

            doorVisual.localScale =
                originalLocalScale;

            doorVisual.localRotation =
                originalLocalRotation;
        }

        if (doorSpriteRenderer != null)
        {
            doorSpriteRenderer.enabled =
                true;

            doorSpriteRenderer.color =
                Color.white;

            if (brokenSprite != null)
            {
                doorSpriteRenderer.sprite =
                    brokenSprite;
            }
        }

        if (doorCollider != null)
        {
            doorCollider.enabled =
                false;
        }

        if (blackBackground != null)
        {
            blackBackground.SetActive(
                false
            );
        }

        if (debugLogs)
        {
            Debug.Log(
                "PrisonBreakDoor: включён тестовый старт. " +
                "Вступление пропущено, дверь считается разрушенной.",
                this
            );
        }
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private void OnValidate()
    {
        if (hitsToBreak < 2)
        {
            hitsToBreak = 2;
        }

        if (damagedSpriteHit < 1)
        {
            damagedSpriteHit = 1;
        }

        if (damagedSpriteHit >=
            hitsToBreak)
        {
            damagedSpriteHit =
                hitsToBreak - 1;
        }

        if (damagedChipScale < 0.01f)
        {
            damagedChipScale = 0.01f;
        }

        if (finalChipScaleMin < 0.01f)
        {
            finalChipScaleMin = 0.01f;
        }

        if (finalChipScaleMax <
            finalChipScaleMin)
        {
            finalChipScaleMax =
                finalChipScaleMin;
        }

        if (chipLaunchDuration < 0.01f)
        {
            chipLaunchDuration = 0.01f;
        }

        if (chipFallDuration < 0.01f)
        {
            chipFallDuration = 0.01f;
        }

        if (chipStayDuration < 0f)
        {
            chipStayDuration = 0f;
        }

        if (chipFadeDuration < 0.01f)
        {
            chipFadeDuration = 0.01f;
        }

        if (playerCelebrationDelay < 0f)
        {
            playerCelebrationDelay = 0f;
        }
    }

    // ============================================================
    // FIND PLAYER
    // ============================================================

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                playerTag
            );

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }
    }

    // ============================================================
    // RESET
    // ============================================================

    private void ResetDoorState()
    {
        StopAllCoroutines();

        hits = 0;
        isBusy = false;
        isBroken = false;

        if (doorVisual != null)
        {
            doorVisual.localPosition =
                originalLocalPosition;

            doorVisual.localScale =
                originalLocalScale;

            doorVisual.localRotation =
                originalLocalRotation;
        }

        if (doorSpriteRenderer != null)
        {
            doorSpriteRenderer.enabled =
                true;

            doorSpriteRenderer.color =
                Color.white;

            if (intactSprite != null)
            {
                doorSpriteRenderer.sprite =
                    intactSprite;
            }
        }

        if (doorCollider != null)
        {
            doorCollider.enabled =
                true;
        }

        if (blackBackground != null)
        {
            blackBackground.SetActive(
                true
            );
        }
    }

    // ============================================================
    // NEW LEG ATTACK SYSTEM
    // ============================================================

    public void ReceiveKick(
        int damage
    )
    {
        if (isBroken ||
            isBusy)
        {
            return;
        }

        if (damage <= 0)
            return;

        StartCoroutine(
            HitDoorSequence(
                damage
            )
        );
    }

    // ============================================================
    // HIT DOOR
    // ============================================================

    private IEnumerator HitDoorSequence(
        int damage
    )
    {
        isBusy = true;

        hits +=
            Mathf.Max(
                1,
                damage
            );

        hits =
            Mathf.Min(
                hits,
                hitsToBreak
            );

        if (debugLogs)
        {
            Debug.Log(
                "Door hit: " +
                hits +
                " / " +
                hitsToBreak,
                this
            );
        }

        if (hits <
            hitsToBreak)
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

            if (hits ==
                damagedSpriteHit)
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
                    maxPunchMoveX *
                    1.5f
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
            {
                blackBackground.SetActive(
                    false
                );
            }

            SpawnFinalBreakChips();

            if (doorCollider != null)
            {
                doorCollider.enabled =
                    false;
            }

            isBroken =
                true;

            if (usePlayerCelebrationSound &&
                playerCelebrationSound != null)
            {
                StartCoroutine(
                    PlayPlayerCelebrationSoundRoutine()
                );
            }

            if (debugLogs)
            {
                Debug.Log(
                    "Door broken. Проход открыт.",
                    this
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

        isBusy =
            false;
    }

    // ============================================================
    // PLAYER CELEBRATION SOUND
    // ============================================================

    private IEnumerator PlayPlayerCelebrationSoundRoutine()
    {
        if (playerCelebrationDelay > 0f)
        {
            yield return new WaitForSeconds(
                playerCelebrationDelay
            );
        }

        if (playerCelebrationSound == null)
        {
            yield break;
        }

        if (playerCelebrationAudioSource != null)
        {
            playerCelebrationAudioSource.PlayOneShot(
                playerCelebrationSound,
                playerCelebrationVolume
            );
        }
        else if (audioSource != null)
        {
            audioSource.PlayOneShot(
                playerCelebrationSound,
                playerCelebrationVolume
            );
        }
        else
        {
            Vector3 soundPosition =
                player != null
                    ? player.position
                    : transform.position;

            AudioSource.PlayClipAtPoint(
                playerCelebrationSound,
                soundPosition,
                playerCelebrationVolume
            );
        }

        if (debugLogs)
        {
            Debug.Log(
                "Player celebration sound played.",
                this
            );
        }
    }

    // ============================================================
    // CARTOON PUNCH
    // ============================================================

    private IEnumerator CartoonPunch(
        float duration,
        float targetScaleX,
        float targetScaleY,
        float moveX
    )
    {
        if (doorVisual == null)
            yield break;

        if (player == null)
        {
            FindPlayer();
        }

        float safeDuration =
            Mathf.Max(
                0.01f,
                duration
            );

        float timer = 0f;

        float direction = 1f;

        if (player != null)
        {
            direction =
                player.position.x <=
                doorVisual.position.x
                    ? 1f
                    : -1f;
        }

        while (timer <
               safeDuration)
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    safeDuration
                );

            float punch =
                Mathf.Sin(
                    t *
                    Mathf.PI
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
                    originalLocalScale.x *
                    currentScaleX,

                    originalLocalScale.y *
                    currentScaleY,

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

        doorVisual.localRotation =
            originalLocalRotation;
    }

    // ============================================================
    // DAMAGED HIT CHIPS
    // ============================================================

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

        for (int i = 0;
             i < count;
             i++)
        {
            Sprite sprite =
                damagedHitChips[i];

            if (sprite == null)
                continue;

            float side =
                i == 0
                    ? -1f
                    : 1f;

            SpawnAnimatedChip(
                sprite,
                damagedChipScale,
                side *
                damagedChipSpreadX,
                damagedChipLift,
                i
            );
        }
    }

    // ============================================================
    // FINAL CHIPS
    // ============================================================

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

        for (int i = 0;
             i < count;
             i++)
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
                    finalChipLift *
                    0.70f,
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

    // ============================================================
    // CREATE CHIP
    // ============================================================

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
                doorSpriteRenderer.sortingOrder +
                1;

            if (doorSpriteRenderer.sharedMaterial !=
                null)
            {
                chipRenderer.sharedMaterial =
                    doorSpriteRenderer.sharedMaterial;
            }
        }

        chip.transform.localScale =
            Vector3.one *
            scale;

        Vector3 startPosition =
            doorVisual.position +
            new Vector3(
                Random.Range(
                    -0.06f,
                    0.06f
                ),
                Random.Range(
                    -0.03f,
                    0.07f
                ),
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

    // ============================================================
    // CHIP ANIMATION
    // ============================================================

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
        {
            startColor =
                chipRenderer.color;
        }

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

        float safeLaunchDuration =
            Mathf.Max(
                0.01f,
                chipLaunchDuration
            );

        float timer = 0f;

        while (timer <
               safeLaunchDuration)
        {
            if (chip == null)
                yield break;

            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    safeLaunchDuration
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

        float safeFallDuration =
            Mathf.Max(
                0.01f,
                chipFallDuration
            );

        float fallTimer = 0f;

        while (fallTimer <
               safeFallDuration)
        {
            if (chip == null)
                yield break;

            fallTimer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    fallTimer /
                    safeFallDuration
                );

            float fallCurve =
                t *
                t;

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

        float safeFadeDuration =
            Mathf.Max(
                0.01f,
                chipFadeDuration
            );

        float fadeTimer = 0f;

        while (fadeTimer <
               safeFadeDuration)
        {
            if (chip == null)
                yield break;

            fadeTimer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    fadeTimer /
                    safeFadeDuration
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
        {
            Destroy(
                chip
            );
        }
    }

    // ============================================================
    // HAPTICS
    // ============================================================

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

    // ============================================================
    // AUDIO
    // ============================================================

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