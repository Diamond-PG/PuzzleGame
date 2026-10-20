using System.Collections;
using UnityEngine;

public class PrisonBreakDoor : MonoBehaviour
{
    // ============================================================
    // 01. REFERENCES
    // ============================================================

    [Header("REFERENCES")]
    [SerializeField] private Transform doorVisual;
    [SerializeField] private SpriteRenderer doorSpriteRenderer;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    // ============================================================
    // 02. BACKGROUND BEHIND DOOR
    // ============================================================

    [Header("BACKGROUND BEHIND DOOR")]
    [Tooltip("Чёрный фон за дверью. Скрывается после разрушения.")]
    [SerializeField] private GameObject blackBackground;

    // ============================================================
    // 03. DOOR SPRITES
    // ============================================================

    [Header("DOOR SPRITES")]
    [SerializeField] private Sprite intactSprite;
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private Sprite brokenSprite;

    // ============================================================
    // 04. DOOR SETTINGS
    // ============================================================

    [Header("DOOR SETTINGS")]
    [Tooltip("Количество ударов до полного разрушения.")]
    [SerializeField, Min(2)] private int hitsToBreak = 8;

    [Tooltip("Удар, на котором появляется повреждённый спрайт.")]
    [SerializeField, Min(1)] private int damagedSpriteHit = 4;

    // ============================================================
    // 05. TUTORIAL DOOR LOCK
    // ============================================================

    [Header("TUTORIAL DOOR LOCK")]

    [Tooltip("Защищать дверь до нужной страницы обучения.")]
    [SerializeField] private bool lockDoorDuringTutorial = true;

    [Tooltip("Объект с компонентом TutorialTextController.")]
    [SerializeField] private TutorialTextController tutorialController;

    [Tooltip("Страница, на которой начинается проявление двери.")]
    [SerializeField, Min(0)] private int unlockDoorAtPage = 22;

    [Tooltip("Маска, которая плавно открывает дверь.")]
    [SerializeField] private PrisonDarknessMask prisonDarknessMask;

    [Tooltip("Показывать сообщения о заблокированных ударах.")]
    [SerializeField] private bool logBlockedKicks = false;

    // ============================================================
    // 06. CARTOON PUNCH ANIMATION
    // ============================================================

    [Header("CARTOON PUNCH ANIMATION")]
    [SerializeField] private float punchDuration = 0.16f;
    [SerializeField] private float minPunchScaleX = 1.035f;
    [SerializeField] private float maxPunchScaleX = 1.10f;
    [SerializeField] private float minPunchScaleY = 0.985f;
    [SerializeField] private float maxPunchScaleY = 0.95f;
    [SerializeField] private float maxPunchMoveX = 0.045f;

    // ============================================================
    // 07. FINAL BREAK
    // ============================================================

    [Header("FINAL BREAK")]
    [SerializeField] private float finalPunchScaleX = 1.14f;
    [SerializeField] private float finalPunchScaleY = 0.92f;
    [SerializeField] private float finalPunchDuration = 0.20f;
    [SerializeField] private float finalBreakPause = 0.035f;

    // ============================================================
    // 08. WOOD CHIPS - DAMAGED HIT
    // ============================================================

    [Header("WOOD CHIPS - 4TH HIT")]
    [SerializeField] private Sprite[] damagedHitChips;
    [SerializeField] private float damagedChipScale = 0.08f;
    [SerializeField] private float damagedChipSpreadX = 0.10f;
    [SerializeField] private float damagedChipLift = 0.10f;

    // ============================================================
    // 09. WOOD CHIPS - FINAL BREAK
    // ============================================================

    [Header("WOOD CHIPS - FINAL BREAK")]
    [SerializeField] private Sprite[] finalBreakChips;
    [SerializeField] private float finalChipScaleMin = 0.08f;
    [SerializeField] private float finalChipScaleMax = 0.13f;
    [SerializeField] private float finalChipSpreadX = 0.18f;
    [SerializeField] private float finalChipLift = 0.14f;

    // ============================================================
    // 10. WOOD CHIP ANIMATION
    // ============================================================

    [Header("WOOD CHIP ANIMATION")]
    [SerializeField] private float chipLaunchDuration = 0.16f;
    [SerializeField] private float chipFallDuration = 0.34f;
    [SerializeField] private float chipStayDuration = 2f;
    [SerializeField] private float chipFadeDuration = 0.25f;
    [SerializeField] private float chipFloorOffsetY = 0.42f;
    [SerializeField] private float chipGroundLift = 0.015f;
    [SerializeField] private float chipEndRotationMin = 55f;
    [SerializeField] private float chipEndRotationMax = 125f;

    // ============================================================
    // 11. HAPTICS
    // ============================================================

    [Header("HAPTICS")]
    [SerializeField] private bool useHaptics = true;
    [SerializeField, Range(5, 120)] private int normalHitHapticMs = 22;
    [SerializeField, Range(5, 200)] private int finalHitHapticMs = 100;

    // ============================================================
    // 12. DOOR AUDIO
    // ============================================================

    [Header("DOOR AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip breakSound;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float breakVolume = 1f;

    // ============================================================
    // 13. PLAYER CELEBRATION AUDIO
    // ============================================================

    [Header("PLAYER CELEBRATION AUDIO")]
    [SerializeField] private bool usePlayerCelebrationSound = true;
    [SerializeField] private AudioSource playerCelebrationAudioSource;
    [SerializeField] private AudioClip playerCelebrationSound;
    [SerializeField, Range(0f, 1f)] private float playerCelebrationVolume = 1f;
    [SerializeField, Min(0f)] private float playerCelebrationDelay = 0f;

    // ============================================================
    // 14. EDITOR TEST START
    // ============================================================

    [Header("EDITOR TEST START")]
    [Tooltip("Только в Unity Editor: пропустить вступление и начать со сломанной дверью.")]
    [SerializeField] private bool skipPrisonIntroForTesting = false;

    // ============================================================
    // 15. DEBUG
    // ============================================================

    [Header("DEBUG")]
    [SerializeField] private bool debugLogs = false;

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private int hits;
    private bool isBusy;
    private bool isBroken;

    private bool doorUnlockedByTutorial;

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
            doorVisual = transform;

        if (doorSpriteRenderer == null && doorVisual != null)
            doorSpriteRenderer = doorVisual.GetComponent<SpriteRenderer>();

        if (doorSpriteRenderer == null)
            doorSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorCollider == null && doorVisual != null)
            doorCollider = doorVisual.GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (tutorialController == null)
            tutorialController = FindFirstObjectByType<TutorialTextController>();

        if (prisonDarknessMask == null)
            prisonDarknessMask = FindFirstObjectByType<PrisonDarknessMask>();

        FindPlayer();

        if (doorVisual != null)
        {
            originalLocalPosition = doorVisual.localPosition;
            originalLocalScale = doorVisual.localScale;
            originalLocalRotation = doorVisual.localRotation;
        }

        doorUnlockedByTutorial =
            TutorialProgressManager.DoorLessonReached;

        if (ShouldSkipPrisonIntroForTesting())
            ApplyBrokenStateForTesting();
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (ShouldSkipPrisonIntroForTesting())
        {
            ApplyBrokenStateForTesting();
            return;
        }

        ResetDoorState();
    }

    // ============================================================
    // TUTORIAL LOCK
    // ============================================================

    private void Update()
    {
        if (isBroken)
            return;

        if (!lockDoorDuringTutorial)
            return;

        if (!doorUnlockedByTutorial)
        {
            if (TutorialProgressManager.DoorLessonReached)
            {
                UnlockDoorForCurrentRun();
            }
            else if (tutorialController != null &&
                     tutorialController.GetCurrentPage() >= unlockDoorAtPage)
            {
                UnlockDoorForCurrentRun();
            }
        }
    }

    private void UnlockDoorForCurrentRun()
    {
        if (doorUnlockedByTutorial)
            return;

        doorUnlockedByTutorial = true;

        TutorialProgressManager.ReachDoorLesson();

        if (prisonDarknessMask != null)
            prisonDarknessMask.BeginDoorReveal(this);

        if (debugLogs)
        {
            Debug.Log(
                "PrisonBreakDoor: tutorial_" +
                unlockDoorAtPage.ToString("00") +
                " reached. Door reveal started.",
                this
            );
        }
    }

    private bool IsDoorLockedByTutorial()
    {
        if (!lockDoorDuringTutorial)
            return false;

        if (!doorUnlockedByTutorial)
        {
            if (TutorialProgressManager.DoorLessonReached ||
                (tutorialController != null &&
                 tutorialController.GetCurrentPage() >= unlockDoorAtPage))
            {
                UnlockDoorForCurrentRun();
            }
            else
            {
                return true;
            }
        }

        // The door must remain protected while it is appearing.
        if (prisonDarknessMask == null)
        {
            if (logBlockedKicks)
            {
                Debug.LogWarning(
                    "PrisonBreakDoor: Darkness Mask is missing. " +
                    "Door remains locked.",
                    this
                );
            }

            return true;
        }

        return !prisonDarknessMask.IsDoorRevealComplete;
    }

    public bool CanReceiveKick()
    {
        return !isBroken &&
               !isBusy &&
               !IsDoorLockedByTutorial();
    }

    // ============================================================
    // RECEIVE KICK
    // ============================================================

    public void ReceiveKick(int damage)
    {
        if (isBroken || isBusy || damage <= 0)
            return;

        if (IsDoorLockedByTutorial())
        {
            if (logBlockedKicks)
            {
                Debug.Log(
                    "PrisonBreakDoor: kick blocked until " +
                    "door reveal is complete.",
                    this
                );
            }

            return;
        }

        StartCoroutine(HitDoorSequence(damage));
    }

    // ============================================================
    // HIT DOOR SEQUENCE
    // ============================================================

    private IEnumerator HitDoorSequence(int damage)
    {
        isBusy = true;

        hits += Mathf.Max(1, damage);
        hits = Mathf.Min(hits, hitsToBreak);

        if (debugLogs)
        {
            Debug.Log(
                "Door hit: " + hits + " / " + hitsToBreak,
                this
            );
        }

        if (hits < hitsToBreak)
        {
            PlayHitHaptic();
            PlayHitSound();

            float hitProgress = Mathf.InverseLerp(
                1f,
                hitsToBreak - 1f,
                hits
            );

            float scaleX = Mathf.Lerp(
                minPunchScaleX,
                maxPunchScaleX,
                hitProgress
            );

            float scaleY = Mathf.Lerp(
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
                if (damagedSprite != null && doorSpriteRenderer != null)
                    doorSpriteRenderer.sprite = damagedSprite;

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
                yield return new WaitForSeconds(finalBreakPause);

            if (doorSpriteRenderer != null && brokenSprite != null)
                doorSpriteRenderer.sprite = brokenSprite;

            if (blackBackground != null)
                blackBackground.SetActive(false);

            SpawnFinalBreakChips();

            if (doorCollider != null)
                doorCollider.enabled = false;

            isBroken = true;

            // Block ONLY left/right movement immediately after
            // the real door destruction. Camera and jumping stay active.
            LockPlayerHorizontalMovementAfterBreak();

            // ====================================================
            // SAVE ACTUAL DOOR DESTRUCTION
            // ====================================================

            TutorialProgressManager.MarkDoorBroken();

            // ====================================================
            // PLAYER CELEBRATION
            // ====================================================

            if (usePlayerCelebrationSound &&
                playerCelebrationSound != null)
            {
                StartCoroutine(
                    PlayPlayerCelebrationSoundRoutine()
                );
            }

            if (debugLogs)
                Debug.Log("Door broken. Проход открыт.", this);
        }

        RestoreDoorTransform();

        isBusy = false;
    }

    // ============================================================
    // DOOR TRANSFORM
    // ============================================================

    private void RestoreDoorTransform()
    {
        if (doorVisual == null)
            return;

        doorVisual.localPosition = originalLocalPosition;
        doorVisual.localScale = originalLocalScale;
        doorVisual.localRotation = originalLocalRotation;
    }

    // ============================================================
    // CARTOON PUNCH ANIMATION
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
            FindPlayer();

        float safeDuration = Mathf.Max(0.01f, duration);
        float timer = 0f;
        float direction = 1f;

        if (player != null)
        {
            direction = player.position.x <= doorVisual.position.x
                ? 1f
                : -1f;
        }

        while (timer < safeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / safeDuration);
            float punch = Mathf.Sin(t * Mathf.PI);

            float currentScaleX = Mathf.Lerp(
                1f,
                targetScaleX,
                punch
            );

            float currentScaleY = Mathf.Lerp(
                1f,
                targetScaleY,
                punch
            );

            doorVisual.localScale = new Vector3(
                originalLocalScale.x * currentScaleX,
                originalLocalScale.y * currentScaleY,
                originalLocalScale.z
            );

            doorVisual.localPosition = originalLocalPosition +
                new Vector3(direction * moveX * punch, 0f, 0f);

            yield return null;
        }

        RestoreDoorTransform();
    }

    // ============================================================
    // DAMAGED HIT WOOD CHIPS
    // ============================================================

    private void SpawnDamagedHitChips()
    {
        if (damagedHitChips == null || damagedHitChips.Length == 0)
            return;

        int count = Mathf.Min(2, damagedHitChips.Length);

        for (int i = 0; i < count; i++)
        {
            Sprite sprite = damagedHitChips[i];

            if (sprite == null)
                continue;

            float side = i == 0 ? -1f : 1f;

            SpawnAnimatedChip(
                sprite,
                damagedChipScale,
                side * damagedChipSpreadX,
                damagedChipLift,
                i
            );
        }
    }

    // ============================================================
    // FINAL BREAK WOOD CHIPS
    // ============================================================

    private void SpawnFinalBreakChips()
    {
        if (finalBreakChips == null || finalBreakChips.Length == 0)
            return;

        int count = Mathf.Min(5, finalBreakChips.Length);

        for (int i = 0; i < count; i++)
        {
            Sprite sprite = finalBreakChips[i];

            if (sprite == null)
                continue;

            float randomScale = Random.Range(
                finalChipScaleMin,
                finalChipScaleMax
            );

            float horizontalOffset = Random.Range(
                -finalChipSpreadX,
                finalChipSpreadX
            );

            float lift = Random.Range(
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

    // ============================================================
    // CREATE WOOD CHIP
    // ============================================================

    private void SpawnAnimatedChip(
        Sprite sprite,
        float scale,
        float horizontalOffset,
        float lift,
        int index
    )
    {
        if (sprite == null || doorVisual == null)
            return;

        GameObject chip = new GameObject("DoorWoodChip");

        SpriteRenderer chipRenderer =
            chip.AddComponent<SpriteRenderer>();

        chipRenderer.sprite = sprite;

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

        chip.transform.localScale = Vector3.one * scale;

        Vector3 startPosition = doorVisual.position +
            new Vector3(
                Random.Range(-0.06f, 0.06f),
                Random.Range(-0.03f, 0.07f),
                0f
            );

        chip.transform.position = startPosition;

        float startRotation = Random.Range(-20f, 20f);

        chip.transform.rotation = Quaternion.Euler(
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
    // ANIMATE WOOD CHIP
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

        Color startColor = Color.white;

        if (chipRenderer != null)
            startColor = chipRenderer.color;

        Vector3 launchEndPosition = startPosition +
            new Vector3(horizontalOffset, lift, 0f);

        float launchRotation = startRotation +
            Random.Range(-30f, 30f);

        float safeLaunchDuration =
            Mathf.Max(0.01f, chipLaunchDuration);

        float timer = 0f;

        while (timer < safeLaunchDuration)
        {
            if (chip == null)
                yield break;

            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / safeLaunchDuration
            );

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            chip.transform.position = Vector3.Lerp(
                startPosition,
                launchEndPosition,
                eased
            );

            chip.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Lerp(startRotation, launchRotation, t)
            );

            yield return null;
        }

        Vector3 fallStartPosition = chip.transform.position;

        float floorY = doorVisual.position.y -
            chipFloorOffsetY +
            chipGroundLift;

        Vector3 fallEndPosition = new Vector3(
            fallStartPosition.x + Random.Range(-0.05f, 0.05f),
            floorY,
            fallStartPosition.z
        );

        float endRotation = index % 2 == 0
            ? -Random.Range(chipEndRotationMin, chipEndRotationMax)
            : Random.Range(chipEndRotationMin, chipEndRotationMax);

        float safeFallDuration =
            Mathf.Max(0.01f, chipFallDuration);

        float fallTimer = 0f;

        while (fallTimer < safeFallDuration)
        {
            if (chip == null)
                yield break;

            fallTimer += Time.deltaTime;

            float t = Mathf.Clamp01(
                fallTimer / safeFallDuration
            );

            float fallCurve = t * t;

            chip.transform.position = Vector3.Lerp(
                fallStartPosition,
                fallEndPosition,
                fallCurve
            );

            chip.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Lerp(launchRotation, endRotation, t)
            );

            yield return null;
        }

        chip.transform.position = fallEndPosition;

        chip.transform.rotation = Quaternion.Euler(
            0f,
            0f,
            endRotation
        );

        if (chipStayDuration > 0f)
            yield return new WaitForSeconds(chipStayDuration);

        float safeFadeDuration =
            Mathf.Max(0.01f, chipFadeDuration);

        float fadeTimer = 0f;

        while (fadeTimer < safeFadeDuration)
        {
            if (chip == null)
                yield break;

            fadeTimer += Time.deltaTime;

            float t = Mathf.Clamp01(
                fadeTimer / safeFadeDuration
            );

            if (chipRenderer != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, t);
                chipRenderer.color = color;
            }

            yield return null;
        }

        if (chip != null)
            Destroy(chip);
    }

    // ============================================================
    // PLAYER CELEBRATION AUDIO
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
            yield break;

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
            Vector3 soundPosition = player != null
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
    // DOOR AUDIO
    // ============================================================

    private void PlayHitSound()
    {
        if (audioSource == null || hitSound == null)
            return;

        audioSource.PlayOneShot(hitSound, hitVolume);
    }

    private void PlayBreakSound()
    {
        if (audioSource == null || breakSound == null)
            return;

        audioSource.PlayOneShot(breakSound, breakVolume);
    }

    // ============================================================
    // PLAYER REFERENCE
    // ============================================================

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            player = playerObject.transform;
    }

    // ============================================================
    // HORIZONTAL MOVEMENT LOCK AFTER REAL DOOR BREAK
    // ============================================================

    private void LockPlayerHorizontalMovementAfterBreak()
    {
        if (player == null)
            FindPlayer();

        PlayerController playerController = null;

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();

            if (playerController == null)
                playerController = player.GetComponentInParent<PlayerController>();
        }

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            playerController.SetHorizontalMovementLock(true);
        }
        else
        {
            Debug.LogWarning(
                "PrisonBreakDoor: PlayerController not found. " +
                "Horizontal movement lock was not applied.",
                this
            );
        }
    }

    // ============================================================
    // EDITOR TEST START
    // ============================================================

    private bool ShouldSkipPrisonIntroForTesting()
    {
        return Application.isEditor &&
               skipPrisonIntroForTesting;
    }

    private void ApplyBrokenStateForTesting()
    {
        StopAllCoroutines();

        hits = hitsToBreak;
        isBusy = false;
        isBroken = true;

        RestoreDoorTransform();

        if (doorSpriteRenderer != null)
        {
            doorSpriteRenderer.enabled = true;
            doorSpriteRenderer.color = Color.white;

            if (brokenSprite != null)
                doorSpriteRenderer.sprite = brokenSprite;
        }

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (blackBackground != null)
            blackBackground.SetActive(false);

        if (debugLogs)
        {
            Debug.Log(
                "PrisonBreakDoor: тестовый старт. Дверь разрушена.",
                this
            );
        }
    }

    // ============================================================
    // RESET DOOR
    // ============================================================

    private void ResetDoorState()
    {
        StopAllCoroutines();

        hits = 0;
        isBusy = false;
        isBroken = false;

        doorUnlockedByTutorial =
            TutorialProgressManager.DoorLessonReached;

        RestoreDoorTransform();

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

        // If the door lesson was already reached on a previous run,
        // the mask still needs to reveal the door in this run.
        if (doorUnlockedByTutorial && prisonDarknessMask != null)
        {
            prisonDarknessMask.BeginDoorReveal(this);
        }
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private void OnValidate()
    {
        hitsToBreak = Mathf.Max(2, hitsToBreak);

        damagedSpriteHit = Mathf.Clamp(
            damagedSpriteHit,
            1,
            hitsToBreak - 1
        );

        unlockDoorAtPage = Mathf.Max(0, unlockDoorAtPage);

        damagedChipScale = Mathf.Max(0.01f, damagedChipScale);

        finalChipScaleMin = Mathf.Max(
            0.01f,
            finalChipScaleMin
        );

        finalChipScaleMax = Mathf.Max(
            finalChipScaleMin,
            finalChipScaleMax
        );

        chipLaunchDuration = Mathf.Max(
            0.01f,
            chipLaunchDuration
        );

        chipFallDuration = Mathf.Max(
            0.01f,
            chipFallDuration
        );

        chipStayDuration = Mathf.Max(
            0f,
            chipStayDuration
        );

        chipFadeDuration = Mathf.Max(
            0.01f,
            chipFadeDuration
        );

        playerCelebrationDelay = Mathf.Max(
            0f,
            playerCelebrationDelay
        );
    }
}