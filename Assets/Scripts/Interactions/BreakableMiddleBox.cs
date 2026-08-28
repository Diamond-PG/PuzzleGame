using System.Collections;
using UnityEngine;

public class BreakableMiddleBox : MonoBehaviour
{
    // ============================================================
    // BOX SETTINGS
    // ============================================================

    [Header("BOX SETTINGS")]

    [Tooltip("Сколько ударов нужно для разрушения среднего ящика.")]
    [SerializeField, Min(1)]
    private int hitsToBreak = 3;

    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("HAPTICS")]

    [SerializeField]
    private bool useHaptics = true;

    [Tooltip("Вибрация при первом ударе.")]
    [SerializeField, Range(5, 100)]
    private int firstHitHapticMs = 18;

    [Tooltip("Вибрация при втором ударе.")]
    [SerializeField, Range(5, 120)]
    private int secondHitHapticMs = 28;

    [Tooltip("Вибрация при полном разрушении.")]
    [SerializeField, Range(5, 200)]
    private int breakHapticMs = 90;

    // ============================================================
    // REWARD
    // ============================================================

    [Header("REWARD")]

    [SerializeField]
    private GoalRevealFromBox rewardReveal;

    [SerializeField]
    private bool revealRewardOnBreak = true;

    [SerializeField]
    private float rewardRevealDelay = 0.05f;

    [Header("REWARD SAFE DETACH")]

    [SerializeField]
    private bool detachRewardBeforeDestroy = true;

    [SerializeField]
    private float rewardDetachDelayAfterReveal = 0.65f;

    [SerializeField]
    private HeartPulse rewardHeartPulse;

    // ============================================================
    // EFFECTS
    // ============================================================

    [Header("EFFECTS")]

    [SerializeField]
    private GameObject breakEffect;

    [SerializeField]
    private float breakEffectLifetime = 2f;

    // ============================================================
    // AUDIO
    // ============================================================

    [Header("AUDIO")]

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip firstHitSound;

    [SerializeField]
    private AudioClip secondHitSound;

    [SerializeField]
    private AudioClip breakSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float firstHitVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField]
    private float secondHitVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField]
    private float breakVolume = 1f;

    // ============================================================
    // BOX SPRITES
    // ============================================================

    [Header("BOX SPRITES")]

    [SerializeField]
    private Sprite normalSprite;

    [SerializeField]
    private Sprite crackedSprite;

    [SerializeField]
    private Sprite crackedSprite2;

    [SerializeField]
    private Sprite brokenSprite;

    [SerializeField]
    private float brokenSpriteDuration = 0.22f;

    // ============================================================
    // HIT EFFECT
    // ============================================================

    [Header("HIT EFFECT")]

    [SerializeField]
    private float hitScaleMultiplier = 0.9f;

    [SerializeField]
    private float hitEffectDuration = 0.08f;

    // ============================================================
    // FIRST HIT SHAKE
    // ============================================================

    [Header("FIRST HIT SHAKE")]

    [SerializeField]
    private float firstHitShakeDuration = 0.20f;

    [SerializeField]
    private float firstHitShakeAmountX = 0.035f;

    [SerializeField]
    private float firstHitShakeAmountY = 0.008f;

    [SerializeField]
    private float firstHitShakeSpeed = 28f;

    // ============================================================
    // SECOND HIT SHAKE
    // ============================================================

    [Header("SECOND HIT SHAKE")]

    [SerializeField]
    private float secondHitShakeDuration = 0.25f;

    [SerializeField]
    private float secondHitShakeAmountX = 0.045f;

    [SerializeField]
    private float secondHitShakeAmountY = 0.010f;

    [SerializeField]
    private float secondHitShakeSpeed = 30f;

    // ============================================================
    // FINAL BREAK SHAKE
    // ============================================================

    [Header("FINAL BREAK SHAKE")]

    [SerializeField]
    private float finalShakeDuration = 0.45f;

    [SerializeField]
    private float finalShakeAmountX = 0.06f;

    [SerializeField]
    private float finalShakeAmountY = 0.01f;

    [SerializeField]
    private float finalShakeSpeed = 35f;

    // ============================================================
    // BROKEN PIECES
    // ============================================================

    [Header("BROKEN PIECES")]

    [SerializeField]
    private GameObject[] woodChips;

    [SerializeField]
    private float chipFallDuration = 0.22f;

    [SerializeField]
    private float chipExtraFallDuration = 0.26f;

    [SerializeField]
    private float chipExtraDropDistance = 0.18f;

    [SerializeField]
    private float chipHorizontalSpread = 0.06f;

    [SerializeField]
    private float chipStayDuration = 0.55f;

    [SerializeField]
    private float chipFadeDuration = 0.25f;

    [SerializeField]
    private float chipEndRotMin = 55f;

    [SerializeField]
    private float chipEndRotMax = 125f;

    [SerializeField]
    private float chipGroundLift = 0.045f;

    // ============================================================
    // PRIVATE
    // ============================================================

    private int hits;

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

    public bool IsBroken => isBreaking;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
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
                GetComponentInChildren<
                    GoalRevealFromBox
                >(true);
        }

        if (rewardHeartPulse == null &&
            rewardReveal != null)
        {
            rewardHeartPulse =
                rewardReveal
                    .GetComponent<HeartPulse>();
        }

        Transform background =
            transform.Find(
                "Box_Background"
            );

        if (background != null)
        {
            boxBackgroundRenderer =
                background
                    .GetComponent<SpriteRenderer>();
        }

        originalLocalScale =
            transform.localScale;

        originalLocalPosition =
            transform.localPosition;
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        ResetBoxState();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        UpdateHitEffect();
    }

    // ============================================================
    // RESET
    // ============================================================

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
            rewardHeartPulse.enabled =
                false;
        }

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled =
                true;

            boxSpriteRenderer.color =
                Color.white;

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
            boxCollider.enabled =
                true;
        }

        if (rewardReveal != null)
        {
            rewardReveal.HideGoalImmediate();
        }
    }

    // ============================================================
    // LEG ATTACK
    // ============================================================

    /*
     * Теперь средний ящик получает удар
     * ТОЛЬКО от LegAttackButton.
     *
     * Никакого тапа по самому ящику.
     * Никакой мышки по ящику.
     */
    public void ReceiveKick(
        int damage
    )
    {
        if (isBreaking ||
            isBusy)
        {
            return;
        }

        if (damage <= 0)
            return;

        isBusy = true;

        StartCoroutine(
            ReceiveKickRoutine(
                damage
            )
        );
    }

    // ============================================================
    // RECEIVE KICK
    // ============================================================

    private IEnumerator ReceiveKickRoutine(
        int damage
    )
    {
        if (isBreaking)
        {
            isBusy = false;
            yield break;
        }

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

        Debug.Log(
            "Middle box hit: " +
            hits +
            " / " +
            hitsToBreak
        );

        PlayHitEffect();

        if (hits <
            hitsToBreak)
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

    // ============================================================
    // HAPTICS
    // ============================================================

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

    // ============================================================
    // HIT EFFECT
    // ============================================================

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

    // ============================================================
    // AUDIO
    // ============================================================

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

    // ============================================================
    // DAMAGE STAGES
    // ============================================================

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

    // ============================================================
    // FINAL BREAK
    // ============================================================

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

    // ============================================================
    // REWARD
    // ============================================================

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
            rewardHeartPulse.enabled =
                false;
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
                rewardHeartPulse
                    .transform
                    .localScale
            );

            rewardHeartPulse.enabled =
                true;
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

        rewardDetached =
            true;
    }

    // ============================================================
    // SHAKE
    // ============================================================

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
            timer +=
                Time.deltaTime;

            float x =
                Mathf.Sin(
                    timer *
                    speed
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

    // ============================================================
    // FINISH BREAK
    // ============================================================

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

    // ============================================================
    // BREAK EFFECT
    // ============================================================

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
            effect.GetComponentsInChildren<
                ParticleSystem
            >(true);

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

    // ============================================================
    // BROKEN PIECES
    // ============================================================

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

    // ============================================================
    // CHIP ANIMATION
    // ============================================================

    private IEnumerator AnimateChip(
        GameObject chip,
        Vector3 startPosition,
        Vector3 endPosition,
        int index
    )
    {
        if (chip == null)
            yield break;

        SpriteRenderer chipRenderer =
            chip.GetComponent<SpriteRenderer>();

        Collider2D chipCollider =
            chip.GetComponent<Collider2D>();

        Rigidbody2D chipRigidbody =
            chip.GetComponent<Rigidbody2D>();

        Color startColor =
            Color.white;

        if (chipRenderer != null)
        {
            startColor =
                chipRenderer.color;
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

        float safeFallDuration =
            Mathf.Max(
                0.01f,
                chipFallDuration
            );

        while (timer <
               safeFallDuration)
        {
            if (chip == null)
                yield break;

            timer +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    safeFallDuration
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

        float safeExtraFallDuration =
            Mathf.Max(
                0.01f,
                chipExtraFallDuration
            );

        while (fallTimer <
               safeExtraFallDuration)
        {
            if (chip == null)
                yield break;

            fallTimer +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    fallTimer /
                    safeExtraFallDuration
                );

            float curvedProgress =
                progress *
                progress;

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

        if (chipStayDuration > 0f)
        {
            yield return new WaitForSeconds(
                chipStayDuration
            );
        }

        float fadeTimer = 0f;

        float safeFadeDuration =
            Mathf.Max(
                0.01f,
                chipFadeDuration
            );

        while (fadeTimer <
               safeFadeDuration)
        {
            if (chip == null)
                yield break;

            fadeTimer +=
                Time.deltaTime;

            float progress =
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
                        progress
                    );

                chipRenderer.color =
                    color;
            }

            yield return null;
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

        Destroy(chip);
    }
}