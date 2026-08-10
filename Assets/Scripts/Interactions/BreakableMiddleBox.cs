using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BreakableMiddleBox : MonoBehaviour
{
    [Header("Box Settings")]
    public int hitsToBreak = 3;
    public Transform player;
    public float interactDistance = 1.5f;

    [Header("Player Kick")]
    [SerializeField] private PlayerKick playerKick;

    [Tooltip("Через сколько секунд после начала удара нога реально попадает по ящику.")]
    [SerializeField] private float kickImpactDelay = 0.08f;

    [Header("Haptics")]
    [SerializeField] private bool useHaptics = true;

    [Tooltip("Вибрация при первом ударе по среднему ящику.")]
    [SerializeField, Range(5, 100)]
    private int firstHitHapticMs = 18;

    [Tooltip("Вибрация при втором ударе по среднему ящику.")]
    [SerializeField, Range(5, 120)]
    private int secondHitHapticMs = 28;

    [Tooltip("Удлинённая вибрация непосредственно в момент разрушения среднего ящика.")]
    [SerializeField, Range(5, 200)]
    private int breakHapticMs = 90;

    [Header("Reward")]
    [SerializeField] private GoalRevealFromBox rewardReveal;
    [SerializeField] private bool revealRewardOnBreak = true;
    [SerializeField] private float rewardRevealDelay = 0.05f;

    [Header("Reward Safe Detach")]
    [SerializeField] private bool detachRewardBeforeDestroy = true;
    [SerializeField] private float rewardDetachDelayAfterReveal = 0.65f;
    [SerializeField] private HeartPulse rewardHeartPulse;

    [Header("Effects")]
    public GameObject breakEffect;
    public float breakEffectLifetime = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip firstHitSound;
    public AudioClip secondHitSound;
    public AudioClip breakSound;

    [Range(0f, 1f)]
    public float firstHitVolume = 1f;

    [Range(0f, 1f)]
    public float secondHitVolume = 1f;

    [Range(0f, 1f)]
    public float breakVolume = 1f;

    [Header("Box Sprites")]
    public Sprite normalSprite;
    public Sprite crackedSprite;
    public Sprite crackedSprite2;
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

    [Header("Second Hit Shake")]
    public float secondHitShakeDuration = 0.25f;
    public float secondHitShakeAmountX = 0.045f;
    public float secondHitShakeAmountY = 0.010f;
    public float secondHitShakeSpeed = 30f;

    [Header("Final Break Shake")]
    public float finalShakeDuration = 0.45f;
    public float finalShakeAmountX = 0.06f;
    public float finalShakeAmountY = 0.01f;
    public float finalShakeSpeed = 35f;

    [Header("Broken Pieces")]
    public GameObject[] woodChips;

    public float chipFallDuration = 0.22f;
    public float chipExtraFallDuration = 0.26f;
    public float chipExtraDropDistance = 0.18f;
    public float chipHorizontalSpread = 0.06f;
    public float chipStayDuration = 0.55f;
    public float chipFadeDuration = 0.25f;
    public float chipEndRotMin = 55f;
    public float chipEndRotMax = 125f;
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
    private bool rewardDetached;

    private void Awake()
    {
        mainCamera = Camera.main;

        boxSpriteRenderer =
            GetComponent<SpriteRenderer>();

        boxCollider =
            GetComponent<Collider2D>();

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        if (rewardReveal == null)
        {
            rewardReveal =
                GetComponentInChildren<GoalRevealFromBox>(true);
        }

        if (rewardHeartPulse == null &&
            rewardReveal != null)
        {
            rewardHeartPulse =
                rewardReveal.GetComponent<HeartPulse>();
        }

        if (playerKick == null &&
            player != null)
        {
            playerKick =
                player.GetComponent<PlayerKick>();
        }

        Transform background =
            transform.Find("Box_Background");

        if (background != null)
        {
            boxBackgroundRenderer =
                background.GetComponent<SpriteRenderer>();
        }

        originalLocalScale =
            transform.localScale;

        originalLocalPosition =
            transform.localPosition;
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
        rewardDetached = false;

        hitEffectTimer = 0f;

        transform.localScale =
            originalLocalScale;

        transform.localPosition =
            originalLocalPosition;

        if (rewardHeartPulse != null)
        {
            rewardHeartPulse.enabled = false;
        }

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled = true;
            boxSpriteRenderer.color = Color.white;

            if (normalSprite != null)
            {
                boxSpriteRenderer.sprite =
                    normalSprite;
            }
        }

        if (boxBackgroundRenderer != null)
        {
            boxBackgroundRenderer.enabled =
                true;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }

        if (rewardReveal != null)
        {
            rewardReveal.HideGoalImmediate();
        }
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

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
            TryRequestHit(
                Mouse.current.position.ReadValue()
            );
        }

        if (!isBusy &&
            Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryRequestHit(
                Touchscreen.current
                    .primaryTouch
                    .position
                    .ReadValue()
            );
        }
    }

    private void TryRequestHit(
        Vector2 screenPosition
    )
    {
        if (!IsValidScreenPosition(screenPosition))
        {
            return;
        }

        float cameraDistance =
            Mathf.Abs(
                transform.position.z -
                mainCamera.transform.position.z
            );

        Vector3 screenPoint =
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                cameraDistance
            );

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                screenPoint
            );

        if (float.IsNaN(worldPosition.x) ||
            float.IsNaN(worldPosition.y) ||
            float.IsInfinity(worldPosition.x) ||
            float.IsInfinity(worldPosition.y))
        {
            return;
        }

        Vector2 point2D =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        Collider2D hitCollider =
            Physics2D.OverlapPoint(point2D);

        if (hitCollider != boxCollider)
        {
            return;
        }

        if (player == null)
        {
            Debug.LogWarning(
                "Player не назначен в BreakableMiddleBox!"
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
            Debug.Log(
                "Слишком далеко от среднего ящика"
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

        bool kickStarted =
            playerKick.KickToward(
                transform.position
            );

        if (!kickStarted)
        {
            return;
        }

        isBusy = true;

        StartCoroutine(
            KickImpactSequence()
        );
    }

    private bool IsValidScreenPosition(
        Vector2 position
    )
    {
        if (float.IsNaN(position.x) ||
            float.IsNaN(position.y) ||
            float.IsInfinity(position.x) ||
            float.IsInfinity(position.y))
        {
            return false;
        }

        if (position.x < 0f ||
            position.y < 0f ||
            position.x > Screen.width ||
            position.y > Screen.height)
        {
            return false;
        }

        return true;
    }

    private IEnumerator KickImpactSequence()
    {
        if (kickImpactDelay > 0f)
        {
            yield return new WaitForSeconds(
                kickImpactDelay
            );
        }

        if (isBreaking)
        {
            yield break;
        }

        hits++;

        Debug.Log(
            "Middle box hit: " +
            hits +
            " / " +
            hitsToBreak
        );

        PlayHitEffect();

        if (hits < hitsToBreak)
        {
            if (hits == 1)
            {
                PlayFirstHitHaptic();
                PlayFirstHitSound();
            }
            else if (hits == 2)
            {
                PlaySecondHitHaptic();
                PlaySecondHitSound();
            }

            yield return StartCoroutine(
                HitSequence()
            );
        }
        else
        {
            yield return StartCoroutine(
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

    private void PlaySecondHitHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            secondHitHapticMs,
            MicroHaptics.IOSHapticStyle.Medium
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

        hitEffectTimer =
            hitEffectDuration;

        isPlayingHitEffect =
            true;
    }

    private void UpdateHitEffect()
    {
        if (!isPlayingHitEffect)
            return;

        hitEffectTimer -=
            Time.deltaTime;

        if (hitEffectTimer <= 0f)
        {
            transform.localScale =
                originalLocalScale;

            isPlayingHitEffect =
                false;
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

    private void PlaySecondHitSound()
    {
        if (audioSource == null ||
            secondHitSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            secondHitSound,
            secondHitVolume
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

    private IEnumerator HitSequence()
    {
        if (hits == 1)
        {
            yield return StartCoroutine(
                ShakeBox(
                    firstHitShakeDuration,
                    firstHitShakeAmountX,
                    firstHitShakeAmountY,
                    firstHitShakeSpeed
                )
            );
        }
        else if (hits == 2)
        {
            yield return StartCoroutine(
                ShakeBox(
                    secondHitShakeDuration,
                    secondHitShakeAmountX,
                    secondHitShakeAmountY,
                    secondHitShakeSpeed
                )
            );
        }

        ApplyDamageSprite();

        transform.localPosition =
            originalLocalPosition;

        transform.localScale =
            originalLocalScale;

        isBusy = false;
    }

    private void ApplyDamageSprite()
    {
        if (boxSpriteRenderer == null)
            return;

        if (hits == 1)
        {
            if (crackedSprite != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite;
            }
        }
        else if (hits == 2)
        {
            if (crackedSprite2 != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite2;
            }
            else if (crackedSprite != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite;
            }
        }
    }

    private IEnumerator FinalBreakSequence()
    {
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
        {
            boxBackgroundRenderer.enabled =
                false;
        }

        if (boxSpriteRenderer != null)
        {
            if (brokenSprite != null)
            {
                boxSpriteRenderer.sprite =
                    brokenSprite;
            }
            else if (crackedSprite2 != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite2;
            }
            else if (crackedSprite != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite;
            }
        }

        PlayBreakHaptic();
        PlayBreakSound();
        SpawnBreakEffect();
        SpawnBrokenPieces();

        if (revealRewardOnBreak &&
            rewardReveal != null)
        {
            StartCoroutine(
                RevealRewardAfterDelay()
            );
        }

        float safeWait =
            brokenSpriteDuration;

        if (revealRewardOnBreak &&
            rewardReveal != null)
        {
            safeWait =
                Mathf.Max(
                    safeWait,
                    rewardRevealDelay +
                    rewardDetachDelayAfterReveal +
                    0.05f
                );
        }

        yield return new WaitForSeconds(
            safeWait
        );

        HideAndFinishBreak();
    }

    private IEnumerator RevealRewardAfterDelay()
    {
        if (rewardRevealDelay > 0f)
        {
            yield return new WaitForSeconds(
                rewardRevealDelay
            );
        }

        if (rewardHeartPulse != null)
        {
            rewardHeartPulse.enabled = false;
        }

        rewardReveal.RevealGoal();

        if (rewardDetachDelayAfterReveal > 0f)
        {
            yield return new WaitForSeconds(
                rewardDetachDelayAfterReveal
            );
        }

        DetachRewardFromBox();

        if (rewardHeartPulse != null)
        {
            rewardHeartPulse.SetBaseScale(
                rewardHeartPulse.transform.localScale
            );

            rewardHeartPulse.enabled = true;
        }
    }

    private void DetachRewardFromBox()
    {
        if (!detachRewardBeforeDestroy)
            return;

        if (rewardDetached)
            return;

        if (rewardReveal == null)
            return;

        rewardReveal.transform.SetParent(
            null,
            true
        );

        rewardDetached = true;
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
                Mathf.Sin(
                    timer * speed
                ) *
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
        DetachRewardFromBox();

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled =
                false;
        }

        if (boxBackgroundRenderer != null)
        {
            boxBackgroundRenderer.enabled =
                false;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled =
                false;
        }

        Debug.Log(
            "Middle box broken!"
        );

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
            {
                chipRigidbody.simulated =
                    false;
            }

            Collider2D chipCollider =
                chip.GetComponent<Collider2D>();

            if (chipCollider != null)
            {
                chipCollider.enabled =
                    false;
            }

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
        {
            startColor =
                spriteRenderer.color;
        }

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

        Vector3 fallStartPosition =
            chip.transform.position;

        Vector3 fallEndPosition =
            fallStartPosition +
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
                    fallStartPosition,
                    fallEndPosition,
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
            fallEndPosition;

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
        {
            chipCollider.enabled =
                false;
        }

        if (chipRigidbody != null)
        {
            chipRigidbody.simulated =
                false;
        }

        chip.SetActive(false);
        Destroy(chip);
    }
}