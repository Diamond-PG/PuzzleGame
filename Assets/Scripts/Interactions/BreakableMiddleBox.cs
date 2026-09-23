using System.Collections;
using UnityEngine;

public class BreakableMiddleBox : MonoBehaviour
{
    [Header("BOX SETTINGS")]
    [SerializeField, Min(1)]
    private int hitsToBreak = 3;


    [Header("PLAYER")]
    [SerializeField]
    private string playerTag = "Player";

    [SerializeField]
    private Transform player;

    [SerializeField]
    private Collider2D playerCollider;


    [Header("SIDE KICK")]
    [SerializeField]
    private bool blockKickFromAbove = true;

    [SerializeField, Min(0f)]
    private float playerAboveTolerance = 0.12f;


    [Header("TOP LANDING HIT")]
    [SerializeField]
    private bool enableTopLandingHit = true;

    [SerializeField, Min(1)]
    private int topLandingDamage = 1;

    [SerializeField, Min(0f)]
    private float minimumTopImpactSpeed = 0.8f;

    [SerializeField, Min(0f)]
    private float topContactTolerance = 0.12f;

    [SerializeField, Range(0f, 1f)]
    private float minimumTopContactNormalY = 0.65f;

    [SerializeField, Min(0f)]
    private float minimumHorizontalOverlap = 0.05f;

    [SerializeField, Min(0f)]
    private float topHitCooldown = 0.15f;


    [Header("HAPTICS")]
    [SerializeField]
    private bool useHaptics = true;

    [SerializeField, Range(5, 100)]
    private int firstHitHapticMs = 18;

    [SerializeField, Range(5, 120)]
    private int secondHitHapticMs = 28;

    [SerializeField, Range(5, 200)]
    private int breakHapticMs = 90;


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


    [Header("EFFECTS")]
    [SerializeField]
    private GameObject breakEffect;

    [SerializeField]
    private float breakEffectLifetime = 2f;


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


    [Header("HIT EFFECT")]
    [SerializeField]
    private float hitScaleMultiplier = 0.9f;

    [SerializeField]
    private float hitEffectDuration = 0.08f;


    [Header("FIRST HIT SHAKE")]
    [SerializeField]
    private float firstHitShakeDuration = 0.20f;

    [SerializeField]
    private float firstHitShakeAmountX = 0.035f;

    [SerializeField]
    private float firstHitShakeAmountY = 0.008f;

    [SerializeField]
    private float firstHitShakeSpeed = 28f;


    [Header("SECOND HIT SHAKE")]
    [SerializeField]
    private float secondHitShakeDuration = 0.25f;

    [SerializeField]
    private float secondHitShakeAmountX = 0.045f;

    [SerializeField]
    private float secondHitShakeAmountY = 0.010f;

    [SerializeField]
    private float secondHitShakeSpeed = 30f;


    [Header("FINAL BREAK SHAKE")]
    [SerializeField]
    private float finalShakeDuration = 0.45f;

    [SerializeField]
    private float finalShakeAmountX = 0.06f;

    [SerializeField]
    private float finalShakeAmountY = 0.01f;

    [SerializeField]
    private float finalShakeSpeed = 35f;


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


    [Header("DEBUG")]
    [SerializeField]
    private bool debugHits = false;


    // ============================================================
    // PRIVATE
    // ============================================================

    private float damageTaken;

    private SpriteRenderer boxSpriteRenderer;
    private SpriteRenderer boxBackgroundRenderer;
    private Collider2D boxCollider;

    private Vector3 originalLocalScale;
    private Vector3 originalLocalPosition;

    private float hitEffectTimer;
    private float nextTopHitTime;

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
                rewardReveal.GetComponent<HeartPulse>();
        }

        Transform background =
            transform.Find(
                "Box_Background"
            );

        if (background != null)
        {
            boxBackgroundRenderer =
                background.GetComponent<SpriteRenderer>();
        }

        FindPlayer();

        originalLocalScale =
            transform.localScale;

        originalLocalPosition =
            transform.localPosition;
    }


    private void Start()
    {
        ResetBoxState();
    }


    private void Update()
    {
        UpdateHitEffect();

        if (player == null)
        {
            FindPlayer();
        }
    }


    // ============================================================
    // FIND PLAYER
    // ============================================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                playerTag
            );

        if (playerObject == null)
            return;

        player =
            playerObject.transform;

        if (playerCollider == null)
        {
            playerCollider =
                playerObject.GetComponent<Collider2D>();

            if (playerCollider == null)
            {
                playerCollider =
                    playerObject
                        .GetComponentInChildren<Collider2D>();
            }
        }
    }


    // ============================================================
    // RESET
    // ============================================================

    private void ResetBoxState()
    {
        StopAllCoroutines();

        damageTaken = 0f;

        isBreaking = false;
        isBusy = false;
        isPlayingHitEffect = false;
        rewardDetached = false;

        hitEffectTimer = 0f;
        nextTopHitTime = 0f;

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
    // KICK LEGACY
    // ============================================================

    public void ReceiveKick(
        int damage
    )
    {
        ReceiveKickDamage(
            damage
        );
    }


    // ============================================================
    // KICK BALANCED
    // ============================================================

    public void ReceiveKickDamage(
        float damage
    )
    {
        if (isBreaking ||
            isBusy ||
            damage <= 0f)
        {
            return;
        }

        if (blockKickFromAbove &&
            PlayerIsStandingAboveBox())
        {
            return;
        }

        ReceiveDamage(
            damage
        );
    }


    // ============================================================
    // LEGACY GENERIC HIT
    // ============================================================

    public void ReceiveHit(
        int damage
    )
    {
        ReceiveDamage(
            damage
        );
    }


    // ============================================================
    // GENERIC WEAPON
    // ============================================================

    public void ReceiveWeaponHit(
        float damage
    )
    {
        ReceiveDamage(
            damage
        );
    }


    // ============================================================
    // RECEIVE DAMAGE
    // ============================================================

    private void ReceiveDamage(
        float damage
    )
    {
        if (isBreaking ||
            isBusy ||
            damage <= 0f)
        {
            return;
        }

        isBusy = true;

        StartCoroutine(
            ReceiveHitRoutine(
                damage
            )
        );
    }


    // ============================================================
    // PLAYER ABOVE
    // ============================================================

    private bool PlayerIsStandingAboveBox()
    {
        if (boxCollider == null)
            return false;

        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
            return false;

        float boxTop =
            boxCollider.bounds.max.y;

        if (playerCollider != null)
        {
            float playerBottom =
                playerCollider.bounds.min.y;

            bool playerFeetAreAtTop =
                playerBottom >=
                boxTop -
                playerAboveTolerance;

            bool playerCenterIsAbove =
                playerCollider.bounds.center.y >
                boxCollider.bounds.center.y;

            bool horizontalOverlap =
                GetHorizontalOverlap() >
                0f;

            return
                playerFeetAreAtTop &&
                playerCenterIsAbove &&
                horizontalOverlap;
        }

        return
            player.position.y >
            boxCollider.bounds.center.y;
    }


    // ============================================================
    // TOP LANDING
    // ============================================================

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (!enableTopLandingHit ||
            isBreaking ||
            isBusy)
        {
            return;
        }

        if (Time.time <
            nextTopHitTime)
        {
            return;
        }

        if (!IsPlayerCollision(
                collision))
        {
            return;
        }

        if (!CollisionIsRealTopLanding(
                collision))
        {
            return;
        }

        float verticalImpactSpeed =
            Mathf.Abs(
                collision.relativeVelocity.y
            );

        if (verticalImpactSpeed <
            minimumTopImpactSpeed)
        {
            return;
        }

        nextTopHitTime =
            Time.time +
            topHitCooldown;

        ReceiveHit(
            topLandingDamage
        );
    }


    private bool CollisionIsRealTopLanding(
        Collision2D collision
    )
    {
        if (boxCollider == null ||
            collision == null ||
            collision.collider == null)
        {
            return false;
        }

        if (playerCollider == null)
        {
            playerCollider =
                collision.collider;
        }

        Bounds currentPlayerBounds =
            playerCollider != null
                ? playerCollider.bounds
                : collision.collider.bounds;

        Bounds boxBounds =
            boxCollider.bounds;

        if (currentPlayerBounds.center.y <=
            boxBounds.center.y)
        {
            return false;
        }

        float horizontalOverlap =
            Mathf.Min(
                currentPlayerBounds.max.x,
                boxBounds.max.x
            ) -
            Mathf.Max(
                currentPlayerBounds.min.x,
                boxBounds.min.x
            );

        if (horizontalOverlap <
            minimumHorizontalOverlap)
        {
            return false;
        }

        float boxTop =
            boxBounds.max.y;

        float playerBottom =
            currentPlayerBounds.min.y;

        if (playerBottom <
            boxTop -
            topContactTolerance)
        {
            return false;
        }

        for (int i = 0;
             i < collision.contactCount;
             i++)
        {
            ContactPoint2D contact =
                collision.GetContact(i);

            bool contactNearTop =
                contact.point.y >=
                boxTop -
                topContactTolerance;

            bool contactIsVertical =
                Mathf.Abs(
                    contact.normal.y
                ) >=
                minimumTopContactNormalY;

            if (contactNearTop &&
                contactIsVertical)
            {
                return true;
            }
        }

        return false;
    }


    private float GetHorizontalOverlap()
    {
        if (playerCollider == null ||
            boxCollider == null)
        {
            return 0f;
        }

        Bounds playerBounds =
            playerCollider.bounds;

        Bounds boxBounds =
            boxCollider.bounds;

        return
            Mathf.Min(
                playerBounds.max.x,
                boxBounds.max.x
            ) -
            Mathf.Max(
                playerBounds.min.x,
                boxBounds.min.x
            );
    }


    private bool IsPlayerCollision(
        Collision2D collision
    )
    {
        if (collision == null ||
            collision.collider == null)
        {
            return false;
        }

        Transform hitTransform =
            collision.collider.transform;

        if (hitTransform.CompareTag(
                playerTag))
        {
            return true;
        }

        Transform root =
            hitTransform.root;

        if (root != null &&
            root.CompareTag(
                playerTag))
        {
            return true;
        }

        if (player != null)
        {
            return
                hitTransform == player ||
                hitTransform.IsChildOf(player);
        }

        return false;
    }


    // ============================================================
    // DAMAGE
    // ============================================================

    private IEnumerator ReceiveHitRoutine(
        float damage
    )
    {
        if (isBreaking)
        {
            isBusy = false;
            yield break;
        }

        damageTaken +=
            Mathf.Max(
                0.01f,
                damage
            );

        damageTaken =
            Mathf.Min(
                damageTaken,
                hitsToBreak
            );

        if (debugHits)
        {
            Debug.Log(
                "[MIDDLE BOX] Damage: " +
                damageTaken.ToString("F2") +
                " / " +
                hitsToBreak,
                this
            );
        }

        PlayHitEffect();

        bool finalHit =
            damageTaken >=
            hitsToBreak -
            0.0001f;

        if (!finalHit)
        {
            int stage =
                Mathf.Clamp(
                    Mathf.CeilToInt(
                        damageTaken
                    ),
                    1,
                    2
                );

            PlayHitHapticByStage(
                stage
            );

            PlayHitSoundByStage(
                stage
            );

            yield return StartCoroutine(
                HitSequence(
                    stage
                )
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

    private void PlayHitHapticByStage(
        int stage
    )
    {
        if (!useHaptics)
            return;

        if (stage <= 1)
        {
            MicroHaptics.Pulse(
                firstHitHapticMs,
                MicroHaptics.IOSHapticStyle.Light
            );
        }
        else
        {
            MicroHaptics.Pulse(
                secondHitHapticMs,
                MicroHaptics.IOSHapticStyle.Medium
            );
        }
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
    // EFFECT
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

    private void PlayHitSoundByStage(
        int stage
    )
    {
        if (audioSource == null)
            return;

        if (stage <= 1)
        {
            if (firstHitSound != null)
            {
                audioSource.PlayOneShot(
                    firstHitSound,
                    firstHitVolume
                );
            }
        }
        else
        {
            if (secondHitSound != null)
            {
                audioSource.PlayOneShot(
                    secondHitSound,
                    secondHitVolume
                );
            }
        }
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
    // DAMAGE STAGE
    // ============================================================

    private IEnumerator HitSequence(
        int stage
    )
    {
        if (stage <= 1)
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
        else
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

        ApplyDamageSprite(
            stage
        );

        transform.localPosition =
            originalLocalPosition;

        transform.localScale =
            originalLocalScale;

        isBusy = false;
    }


    private void ApplyDamageSprite(
        int stage
    )
    {
        if (boxSpriteRenderer == null)
            return;

        if (stage <= 1)
        {
            if (crackedSprite != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite;
            }
        }
        else
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

        if (boxCollider != null)
        {
            boxCollider.enabled =
                false;
        }

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
    // FINISH
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
    // EFFECT
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
    // CHIPS
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

        float safeFallDuration =
            Mathf.Max(
                0.01f,
                chipFallDuration
            );

        float timer = 0f;

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

        float safeExtraFallDuration =
            Mathf.Max(
                0.01f,
                chipExtraFallDuration
            );

        float fallTimer = 0f;

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

        Destroy(
            chip
        );
    }
}