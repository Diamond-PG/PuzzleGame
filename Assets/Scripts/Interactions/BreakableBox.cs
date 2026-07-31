using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BreakableBox : MonoBehaviour
{
    [Header("Box Settings")]
    public int hitsToBreak = 2;
    public Transform player;
    public float interactDistance = 1.5f;

    [Header("Haptics")]
    [SerializeField] private bool useHaptics = true;

    [Tooltip("Вибрация при первом ударе по обычному ящику.")]
    [SerializeField, Range(5, 100)]
    private int firstHitHapticMs = 18;

    [Tooltip("Удлинённая вибрация непосредственно в момент разрушения ящика.")]
    [SerializeField, Range(5, 200)]
    private int breakHapticMs = 75;

    [Header("Effects")]
    public GameObject breakEffect;
    public float breakEffectLifetime = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip firstHitSound;
    public AudioClip breakSound;

    [Range(0f, 1f)]
    public float firstHitVolume = 1f;

    [Range(0f, 1f)]
    public float breakVolume = 1f;

    [Header("Box Sprites")]
    public Sprite normalSprite;
    public Sprite crackedSprite;
    public Sprite brokenSprite;
    public float brokenSpriteDuration = 0.22f;

    [Header("Hit Effect")]
    public float hitScaleMultiplier = 0.9f;
    public float hitEffectDuration = 0.08f;

    [Header("First Hit Shake")]
    public float firstHitShakeDuration = 0.20f;
    public float firstHitShakeAmountX = 0.035f;
    public float firstHitShakeAmountY = 0.008f;
    public float firstHitShakeSpeed = 28f;

    [Header("Final Break Shake")]
    public float finalShakeDuration = 0.45f;
    public float finalShakeAmountX = 0.06f;
    public float finalShakeAmountY = 0.01f;
    public float finalShakeSpeed = 35f;

    [Header("Broken Pieces")]
    public GameObject[] woodChips;

    [Tooltip("Время первого осыпания щепки")]
    public float chipFallDuration = 0.22f;

    [Tooltip("Время дополнительного падения щепки вниз")]
    public float chipExtraFallDuration = 0.26f;

    [Tooltip("Насколько ещё щепка опускается вниз после первого падения")]
    public float chipExtraDropDistance = 0.18f;

    [Tooltip("Разброс по X при финальном падении")]
    public float chipHorizontalSpread = 0.06f;

    [Tooltip("Сколько щепки лежат на полу до исчезновения")]
    public float chipStayDuration = 0.55f;

    [Tooltip("Сколько времени щепки плавно исчезают")]
    public float chipFadeDuration = 0.25f;

    [Tooltip("Минимальный финальный угол, когда щепка ложится плашмя")]
    public float chipEndRotMin = 55f;

    [Tooltip("Максимальный финальный угол, когда щепка ложится плашмя")]
    public float chipEndRotMax = 125f;

    [Tooltip("Небольшой подъём щепок, чтобы не проваливались визуально в пол")]
    public float chipGroundLift = 0.045f;

    private int hits;
    private Camera mainCamera;

    private SpriteRenderer boxSpriteRenderer;
    private SpriteRenderer boxBackgroundRenderer;
    private Collider2D boxCollider;

    private Vector3 originalLocalScale;
    private Vector3 originalLocalPosition;

    private float hitEffectTimer;
    private bool isPlayingHitEffect;
    private bool isBreaking;
    private bool isBusy;

    private void Awake()
    {
        mainCamera = Camera.main;

        boxSpriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        Transform background = transform.Find("Box_Background");

        if (background != null)
            boxBackgroundRenderer = background.GetComponent<SpriteRenderer>();

        originalLocalScale = transform.localScale;
        originalLocalPosition = transform.localPosition;
    }

    private void Start()
    {
        ResetBoxState();
    }

    private void OnEnable()
    {
        ResetBoxState();
    }

    private void ResetBoxState()
    {
        StopAllCoroutines();

        hits = 0;
        isBreaking = false;
        isBusy = false;
        isPlayingHitEffect = false;
        hitEffectTimer = 0f;

        transform.localScale = originalLocalScale;
        transform.localPosition = originalLocalPosition;

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled = true;
            boxSpriteRenderer.color = Color.white;

            if (normalSprite != null)
                boxSpriteRenderer.sprite = normalSprite;
        }

        if (boxBackgroundRenderer != null)
            boxBackgroundRenderer.enabled = true;

        if (boxCollider != null)
            boxCollider.enabled = true;
    }

    private void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        UpdateHitEffect();

        if (isBreaking ||
            isBusy ||
            mainCamera == null ||
            boxCollider == null)
        {
            return;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition =
                Mouse.current.position.ReadValue();

            TryHitBox(screenPosition);
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            TryHitBox(touchPosition);
        }
    }

    private void TryHitBox(Vector2 screenPosition)
    {
        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        Vector2 point2D =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        Collider2D hitCollider =
            Physics2D.OverlapPoint(point2D);

        if (hitCollider != boxCollider)
            return;

        if (player == null)
        {
            Debug.LogWarning(
                "Player не назначен в BreakableBox!"
            );

            return;
        }

        float distance =
            Vector2.Distance(
                player.position,
                transform.position
            );

        if (distance > interactDistance)
        {
            Debug.Log("Слишком далеко от ящика");
            return;
        }

        hits++;

        Debug.Log(
            "Box hit: " +
            hits +
            " / " +
            hitsToBreak
        );

        PlayHitEffect();

        if (hits < hitsToBreak)
        {
            PlayFirstHitHaptic();
            PlayFirstHitSound();

            StartCoroutine(
                FirstHitSequence()
            );
        }
        else
        {
            /*
             * Здесь вибрацию больше не запускаем.
             * Она сработает позже, непосредственно
             * в момент визуального разрушения ящика.
             */
            StartCoroutine(
                FinalBreakSequence()
            );
        }
    }

    private void PlayFirstHitHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            firstHitHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }

    private void PlayBreakHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            breakHapticMs,
            MicroHaptics.IOSHapticStyle.Heavy
        );
    }

    private void PlayHitEffect()
    {
        transform.localScale =
            originalLocalScale *
            hitScaleMultiplier;

        hitEffectTimer = hitEffectDuration;
        isPlayingHitEffect = true;
    }

    private void UpdateHitEffect()
    {
        if (!isPlayingHitEffect)
            return;

        hitEffectTimer -= Time.deltaTime;

        if (hitEffectTimer <= 0f)
        {
            transform.localScale =
                originalLocalScale;

            isPlayingHitEffect = false;
        }
    }

    private void PlayFirstHitSound()
    {
        if (audioSource == null ||
            firstHitSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            firstHitSound,
            firstHitVolume
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

    private IEnumerator FirstHitSequence()
    {
        isBusy = true;

        yield return StartCoroutine(
            ShakeBox(
                firstHitShakeDuration,
                firstHitShakeAmountX,
                firstHitShakeAmountY,
                firstHitShakeSpeed
            )
        );

        if (boxSpriteRenderer != null &&
            crackedSprite != null)
        {
            boxSpriteRenderer.sprite =
                crackedSprite;
        }

        transform.localPosition =
            originalLocalPosition;

        transform.localScale =
            originalLocalScale;

        isBusy = false;
    }

    private IEnumerator FinalBreakSequence()
    {
        isBusy = true;
        isBreaking = true;

        yield return StartCoroutine(
            ShakeBox(
                finalShakeDuration,
                finalShakeAmountX,
                finalShakeAmountY,
                finalShakeSpeed
            )
        );

        transform.localPosition =
            originalLocalPosition;

        transform.localScale =
            originalLocalScale;

        if (boxBackgroundRenderer != null)
            boxBackgroundRenderer.enabled = false;

        if (boxSpriteRenderer != null)
        {
            if (brokenSprite != null)
            {
                boxSpriteRenderer.sprite =
                    brokenSprite;
            }
            else if (crackedSprite != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite;
            }
        }

        /*
         * Всё запускается одновременно:
         * удлинённая вибрация, звук, пыль и щепки.
         */
        PlayBreakHaptic();
        PlayBreakSound();
        SpawnBreakEffect();
        SpawnBrokenPieces();

        yield return new WaitForSeconds(
            brokenSpriteDuration
        );

        HideAndFinishBreak();
    }

    private IEnumerator ShakeBox(
        float duration,
        float amountX,
        float amountY,
        float speed
    )
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float x =
                Mathf.Sin(timer * speed) *
                amountX;

            float y =
                Mathf.Cos(
                    timer *
                    speed *
                    0.5f
                ) *
                amountY;

            transform.localPosition =
                originalLocalPosition +
                new Vector3(
                    x,
                    y,
                    0f
                );

            yield return null;
        }

        transform.localPosition =
            originalLocalPosition;
    }

    private void HideAndFinishBreak()
    {
        if (boxSpriteRenderer != null)
            boxSpriteRenderer.enabled = false;

        if (boxBackgroundRenderer != null)
            boxBackgroundRenderer.enabled = false;

        if (boxCollider != null)
            boxCollider.enabled = false;

        Debug.Log("Box broken!");

        float totalChipTime =
            chipFallDuration +
            chipExtraFallDuration +
            chipStayDuration +
            chipFadeDuration +
            0.15f;

        Destroy(
            gameObject,
            totalChipTime
        );
    }

    private void SpawnBreakEffect()
    {
        if (breakEffect == null)
            return;

        GameObject effect =
            Instantiate(
                breakEffect,
                transform.position,
                Quaternion.identity
            );

        ParticleSystem[] particleSystems =
            effect.GetComponentsInChildren<ParticleSystem>(
                true
            );

        for (int i = 0;
             i < particleSystems.Length;
             i++)
        {
            particleSystems[i].Clear();
            particleSystems[i].Play();
        }

        Destroy(
            effect,
            breakEffectLifetime
        );
    }

    private void SpawnBrokenPieces()
    {
        if (woodChips == null ||
            woodChips.Length == 0)
        {
            return;
        }

        int count =
            Mathf.Min(
                woodChips.Length,
                6
            );

        Vector3[] startOffsets =
        {
            new Vector3(-0.16f,  0.08f, 0f),
            new Vector3( 0.00f,  0.09f, 0f),
            new Vector3( 0.16f,  0.08f, 0f),
            new Vector3(-0.14f, -0.02f, 0f),
            new Vector3( 0.02f, -0.03f, 0f),
            new Vector3( 0.15f, -0.01f, 0f)
        };

        Vector3[] endOffsets =
        {
            new Vector3(-0.24f, -0.10f, 0f),
            new Vector3( 0.00f, -0.12f, 0f),
            new Vector3( 0.24f, -0.10f, 0f),
            new Vector3(-0.18f, -0.22f, 0f),
            new Vector3( 0.02f, -0.24f, 0f),
            new Vector3( 0.19f, -0.21f, 0f)
        };

        for (int i = 0;
             i < count;
             i++)
        {
            GameObject prefab =
                woodChips[i];

            if (prefab == null)
                continue;

            Vector3 startPosition =
                transform.position +
                startOffsets[i];

            Vector3 endPosition =
                transform.position +
                endOffsets[i];

            Quaternion startRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Random.Range(
                        -10f,
                        10f
                    )
                );

            GameObject chip =
                Instantiate(
                    prefab,
                    startPosition,
                    startRotation
                );

            Rigidbody2D chipRigidbody =
                chip.GetComponent<Rigidbody2D>();

            if (chipRigidbody != null)
                chipRigidbody.simulated = false;

            Collider2D chipCollider =
                chip.GetComponent<Collider2D>();

            if (chipCollider != null)
                chipCollider.enabled = false;

            StartCoroutine(
                AnimateChip(
                    chip,
                    startPosition,
                    endPosition,
                    i
                )
            );
        }
    }

    private IEnumerator AnimateChip(
        GameObject chip,
        Vector3 startPosition,
        Vector3 endPosition,
        int index
    )
    {
        if (chip == null)
            yield break;

        SpriteRenderer spriteRenderer =
            chip.GetComponent<SpriteRenderer>();

        Collider2D chipCollider =
            chip.GetComponent<Collider2D>();

        Rigidbody2D chipRigidbody =
            chip.GetComponent<Rigidbody2D>();

        Color startColor =
            Color.white;

        if (spriteRenderer != null)
            startColor = spriteRenderer.color;

        float startRotation =
            chip.transform.eulerAngles.z;

        float middleRotation =
            startRotation +
            Random.Range(
                -18f,
                18f
            );

        float endRotation =
            index % 2 == 0
                ? -Random.Range(
                    chipEndRotMin,
                    chipEndRotMax
                )
                : Random.Range(
                    chipEndRotMin,
                    chipEndRotMax
                );

        float timer = 0f;

        while (timer < chipFallDuration)
        {
            if (chip == null)
                yield break;

            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    chipFallDuration
                );

            chip.transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress
                );

            chip.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        startRotation,
                        middleRotation,
                        progress
                    )
                );

            yield return null;
        }

        Vector3 extraFallStart =
            chip.transform.position;

        Vector3 extraFallEnd =
            extraFallStart +
            new Vector3(
                Random.Range(
                    -chipHorizontalSpread,
                    chipHorizontalSpread
                ),
                -chipExtraDropDistance +
                chipGroundLift,
                0f
            );

        float fallTimer = 0f;

        while (fallTimer <
               chipExtraFallDuration)
        {
            if (chip == null)
                yield break;

            fallTimer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    fallTimer /
                    chipExtraFallDuration
                );

            float curvedProgress =
                progress * progress;

            chip.transform.position =
                Vector3.Lerp(
                    extraFallStart,
                    extraFallEnd,
                    curvedProgress
                );

            chip.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        middleRotation,
                        endRotation,
                        progress
                    )
                );

            yield return null;
        }

        chip.transform.position =
            extraFallEnd;

        chip.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                endRotation
            );

        float stayTimer = 0f;

        while (stayTimer <
               chipStayDuration)
        {
            if (chip == null)
                yield break;

            stayTimer += Time.deltaTime;
            yield return null;
        }

        float fadeTimer = 0f;

        while (fadeTimer <
               chipFadeDuration)
        {
            if (chip == null)
                yield break;

            fadeTimer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    fadeTimer /
                    chipFadeDuration
                );

            if (spriteRenderer != null)
            {
                Color color =
                    startColor;

                color.a =
                    Mathf.Lerp(
                        startColor.a,
                        0f,
                        progress
                    );

                spriteRenderer.color =
                    color;
            }

            yield return null;
        }

        if (spriteRenderer != null)
        {
            Color color =
                spriteRenderer.color;

            color.a = 0f;

            spriteRenderer.color =
                color;

            spriteRenderer.enabled =
                false;
        }

        if (chipCollider != null)
            chipCollider.enabled = false;

        if (chipRigidbody != null)
            chipRigidbody.simulated = false;

        chip.SetActive(false);
        Destroy(chip);
    }
}