using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EnemyHealthBar))]
public partial class GuardEnemy : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("VISUAL")]
    [Tooltip("Use a child SpriteRenderer to align unarmed poses independently of the guard's physics.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("SPRITES - IDLE / LOOK")]
    [SerializeField] private Sprite idleFrontSprite;
    [SerializeField] private Sprite blinkSprite;
    [SerializeField] private Sprite lookLeftSprite;
    [SerializeField] private Sprite lookRightSprite;

    [Header("SPRITES - WALK")]
    [SerializeField] private Sprite walkRightSprite;
    [SerializeField] private Sprite walkLeftSprite;

    [Header("SPRITES - CHASE")]
    [SerializeField] private Sprite chaseLeftSprite;
    [SerializeField] private Sprite chaseRightSprite;

    [Header("SPRITES - ATTACK")]
    [SerializeField] private Sprite attackLeftSprite;
    [SerializeField] private Sprite attackRightSprite;

    [Header("DEATH SPRITES")]
    [SerializeField] private Sprite deathWhenPlayerLeftSprite;
    [SerializeField] private Sprite deathWhenPlayerRightSprite;


    // ============================================================
    // PATROL
    // ============================================================

    [Header("PATROL")]
    [SerializeField] private float patrolSpeed = 1.15f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float patrolEdgeTolerance = 0.08f;

    [Header("PATROL PAUSE")]
    [SerializeField] private float pauseBeforeBlink = 1f;
    [SerializeField] private float blinkDuration = 0.18f;
    [SerializeField] private float pauseAfterBlink = 0.30f;

    [Header("OBSTACLE DETECTION")]
    [SerializeField] private float obstacleCheckDistance = 0.05f;


    // ============================================================
    // PLAYER DETECTION
    // ============================================================

    [Header("PLAYER DETECTION")]
    [SerializeField] private float detectionDistance = 1.7f;
    [SerializeField] private float detectionHeight = 0.35f;
    [SerializeField] private float losePlayerDistance = 6f;

    [Header("CHASE")]
    [SerializeField] private float chaseSpeed = 1.65f;


    // ============================================================
    // GUARD HEALTH
    // ============================================================

    [Header("GUARD HEALTH")]
    [SerializeField, Min(1)] private int maxHealth = 4;


    // ============================================================
    // HIT BLINK
    // ============================================================

    [Header("HIT BLINK")]
    [SerializeField, Min(1)] private int hitBlinkCount = 3;
    [SerializeField] private float hitBlinkInterval = 0.12f;


    // ============================================================
    // HIT KNOCKBACK
    // ============================================================

    [Header("HIT KNOCKBACK")]
    [SerializeField] private float knockbackForce = 2.1f;
    [SerializeField] private float knockbackUpForce = 3f;
    [SerializeField] private float knockbackDuration = 0.18f;


    // ============================================================
    // GUARD ATTACK
    // ============================================================

    [Header("GUARD ATTACK")]
    [SerializeField, Min(1)] private int damageToPlayer = 1;
    [SerializeField] private float attackDistance = 0.85f;
    [SerializeField] private float attackHeight = 1.1f;
    [SerializeField] private float attackImpactDelay = 0.16f;
    [SerializeField] private float attackSpriteDuration = 0.35f;
    [SerializeField] private float attackCooldown = 1.15f;


    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("HAPTICS")]
    [SerializeField] private bool useHaptics = true;

    [SerializeField, Range(5, 100)]
    private int playerHitsGuardHapticMs = 20;

    [Tooltip(
        "Вибрация при настоящем попадании мечом по Guard."
    )]
    [SerializeField, Range(5, 150)]
    private int playerSwordHitsGuardHapticMs = 30;

    [SerializeField, Range(5, 150)]
    private int guardHitsPlayerHapticMs = 40;


    // ============================================================
    // AUDIO
    // ============================================================

    [Header("AUDIO - OPTIONAL")]
    [SerializeField] private AudioSource sfxSource;
    private bool CanPlaySfx => sfxSource != null && sfxSource.isActiveAndEnabled;

    [Header("AUDIO - DETECT")]
    [SerializeField] private AudioClip detectClip;

    [Range(0f, 1f)]
    [SerializeField] private float detectVolume = 1f;

    [Header("AUDIO - CHASE")]
    [SerializeField] private AudioClip chaseClip;

    [Range(0f, 1f)]
    [SerializeField] private float chaseVolume = 1f;

    [SerializeField, Min(0f)]
    private float chaseVoiceDelay = 0.18f;


    // ============================================================
    // PLAYER KICK IMPACT
    // ============================================================

    [Header("AUDIO - PLAYER KICK IMPACT")]

    [SerializeField]
    private AudioClip playerKickImpactClip;

    [SerializeField, Range(0f, 1f)]
    private float playerKickImpactVolume = 0.85f;


    // ============================================================
    // PLAYER SWORD IMPACT
    // ============================================================

    [Header("AUDIO - PLAYER SWORD IMPACT")]

    [SerializeField]
    private AudioClip playerSwordImpactClip;

    [SerializeField, Range(0f, 1f)]
    private float playerSwordImpactVolume = 0.9f;

    [Header("AUDIO - HURT")]
    [SerializeField] private AudioClip hurtClip;

    [Range(0f, 1f)]
    [SerializeField] private float hurtVolume = 1f;

    [Header("AUDIO - SWING")]
    [SerializeField] private AudioClip swingClip;

    [Range(0f, 1f)]
    [SerializeField] private float swingVolume = 1f;

    [Header("AUDIO - DEATH")]
    [SerializeField] private AudioClip deathClip;

    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 1f;


    // ============================================================
    // WEAPON DROP
    // ============================================================

    [Header("WEAPON DROP")]
    [SerializeField] private GameObject weaponObject;

    [SerializeField]
    private Vector2 weaponSpawnOffset =
        new Vector2(0f, 0.18f);

    [SerializeField] private float weaponDropDistance = 0.65f;
    [SerializeField] private float weaponDropDuration = 0.38f;
    [SerializeField] private float weaponDropArcHeight = 0.10f;
    [SerializeField] private float weaponLandingRotation = 90f;
    [SerializeField] private float weaponFloorGap = 0.015f;
    [SerializeField] private bool weaponColliderBecomesTrigger = true;


    // ============================================================
    // PRIVATE
    // ============================================================

    private Rigidbody2D rb;
    private Collider2D bodyCollider;

    private float startX;
    private float leftPatrolX;
    private float rightPatrolX;

    /*
     * Был int.
     *
     * Теперь float, чтобы дубинка, нога,
     * топор, копьё и другое оружие
     * могли наносить разный дробный урон.
     */
    private float currentHealth;
    private EnemyHealthBar healthBar;

    private bool movingRight;
    private bool chasingPlayer;
    private bool patrolPaused;
    private bool attackBusy;
    private bool hitBlinking;
    private bool isKnockedBack;
    private bool isDead;
    private bool weaponDropped;

    private bool detectedPlayerNaturally;
    private bool detectionVoicePlayed;
    private bool chaseVoicePlayed;

    private Coroutine patrolPauseCoroutine;
    private Coroutine hitBlinkCoroutine;
    private Coroutine knockbackCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine chaseVoiceCoroutine;

    public bool IsDead => isDead;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody2D>();

        bodyCollider =
            GetComponent<Collider2D>();

        currentHealth =
            maxHealth;
        healthBar = GetComponent<EnemyHealthBar>();

        if (spriteRenderer == null)
        {
            Transform visual =
                transform.Find("Visual");

            if (visual != null)
            {
                spriteRenderer =
                    visual.GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer =
                    GetComponentInChildren<SpriteRenderer>();
            }
        }

        if (weaponObject == null)
        {
            Transform weapon =
                transform.Find("Weapon");

            if (weapon != null)
            {
                weaponObject =
                    weapon.gameObject;
            }
        }

        CaptureVisualAndWeapon();

        if (sfxSource == null)
        {
            sfxSource =
                GetComponent<AudioSource>();
        }

        FindPlayerLinks();
    }


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        startX =
            transform.position.x;

        leftPatrolX =
            startX -
            patrolDistance;

        rightPatrolX =
            startX +
            patrolDistance;

        SetIdleFrontSprite();
    }


    // ============================================================
    // FIXED UPDATE
    // ============================================================

    private void FixedUpdate()
    {
        if (!CanBeTargeted)
            return;

        if (player == null)
        {
            FindPlayerLinks();

            if (player == null)
                return;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            StopAllCombatAfterPlayerDeath();
            return;
        }

        CheckPlayer();

        if (isKnockedBack)
            return;

        if (hitBlinking)
        {
            StopHorizontalMovement();
            return;
        }

        if (attackBusy)
        {
            StopHorizontalMovement();
            return;
        }

        if (chasingPlayer)
        {
            CancelPatrolPause();
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }


    // ============================================================
    // PLAYER LINKS
    // ============================================================

    private void FindPlayerLinks()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag(
                    "Player"
                );

            if (playerObject != null)
            {
                player =
                    playerObject.transform;
            }
        }

        if (player == null)
            return;

        if (playerHealth == null)
        {
            playerHealth =
                player.GetComponent<PlayerHealth>();
        }
    }


    // ============================================================
    // PLAYER HITS
    // ============================================================

    public void ReceiveKick(int damage) => ReceiveKickDamage(damage);

    public void ReceiveKickDamage(float damage) => ReceiveWeaponHit(damage, GuardHitType.Kick);

    public void ReceiveSwordHit(int damage) => ReceiveWeaponHit(damage, GuardHitType.Sword);

    public void ReceiveWeaponHit(float damage) => ReceiveWeaponHit(damage, GuardHitType.Unspecified);

    public void ReceiveWeaponHit(float damage, GuardHitType hitType)
        => ReceiveWeaponHit(damage, hitType, null);

    internal void ReceiveWeaponHit(float damage, GuardHitType hitType, CombatContact? contact)
    {
        if (!CanBeTargeted || hitBlinking || isKnockedBack || damage <= 0f)
            return;

        float previousHealth = currentHealth;
        currentHealth =
            Mathf.Max(
                0f,
                currentHealth -
                damage
            );
        if (currentHealth < previousHealth)
            CombatImpactEffects.Report(this, hitType, contact, player, bodyCollider);
        if (healthBar != null)
            healthBar.ShowDamage(currentHealth / maxHealth, spriteRenderer);


        // --------------------------------------------------------
        // HAPTICS + IMPACT
        // --------------------------------------------------------

        if (hitType ==
            GuardHitType.Kick)
        {
            PlayPlayerHitsGuardHaptic();
            PlayPlayerKickImpactSound();
        }
        else if (hitType ==
                 GuardHitType.Sword)
        {
            PlayPlayerSwordHitsGuardHaptic();
            PlayPlayerSwordImpactSound();
        }
        else
        {
            /*
             * Для дубинки / топора / копья
             * используем ударную вибрацию.
             *
             * Отдельный звук самого оружия
             * проигрывает его Attack-скрипт.
             */
            PlayPlayerSwordHitsGuardHaptic();
        }


        // --------------------------------------------------------
        // HURT VOICE
        // --------------------------------------------------------

        if (CanPlaySfx &&
            hurtClip != null)
        {
            sfxSource.PlayOneShot(
                hurtClip,
                hurtVolume
            );
        }


        // --------------------------------------------------------
        // DEATH
        // --------------------------------------------------------

        if (currentHealth <=
            0.0001f)
        {
            Fall(enableKnockout && (hitType == GuardHitType.Kick || hitType == GuardHitType.Club));
            return;
        }


        // --------------------------------------------------------
        // AGGRO
        // --------------------------------------------------------

        detectedPlayerNaturally =
            false;

        if (chaseVoiceCoroutine != null)
        {
            StopCoroutine(
                chaseVoiceCoroutine
            );

            chaseVoiceCoroutine =
                null;
        }

        AggroAndFacePlayerAfterHit();

        CancelPatrolPause();

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine =
                null;

            attackBusy =
                false;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );
        }

        knockbackCoroutine =
            StartCoroutine(
                KnockbackRoutine()
            );

        if (hitBlinkCoroutine != null)
        {
            StopCoroutine(
                hitBlinkCoroutine
            );
        }

        hitBlinkCoroutine =
            StartCoroutine(
                HitBlinkRoutine()
            );
    }


    // ============================================================
    // PLAYER KICK IMPACT SOUND
    // ============================================================

    private void PlayPlayerKickImpactSound()
    {
        if (!CanPlaySfx ||
            playerKickImpactClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            playerKickImpactClip,
            playerKickImpactVolume
        );
    }


    // ============================================================
    // PLAYER SWORD IMPACT SOUND
    // ============================================================

    private void PlayPlayerSwordImpactSound()
    {
        if (!CanPlaySfx ||
            playerSwordImpactClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            playerSwordImpactClip,
            playerSwordImpactVolume
        );
    }


    // ============================================================
    // AGGRO AFTER HIT
    // ============================================================

    private void AggroAndFacePlayerAfterHit()
    {
        if (player == null)
        {
            FindPlayerLinks();
        }

        if (player == null)
            return;

        chasingPlayer = true;
        patrolPaused = false;

        if (player.position.x <
            transform.position.x)
        {
            movingRight = false;
            SetChaseLeftSprite();
        }
        else
        {
            movingRight = true;
            SetChaseRightSprite();
        }
    }


    // ============================================================
    // KNOCKBACK
    // ============================================================

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        float direction;

        if (player != null)
        {
            direction =
                transform.position.x >=
                player.position.x
                    ? 1f
                    : -1f;
        }
        else
        {
            direction =
                movingRight
                    ? -1f
                    : 1f;
        }

        if (rb != null)
        {
            rb.linearVelocity =
                new Vector2(
                    direction *
                    knockbackForce,
                    knockbackUpForce
                );
        }

        yield return new WaitForSeconds(
            knockbackDuration
        );

        StopHorizontalMovement();

        isKnockedBack = false;
        knockbackCoroutine = null;
    }


    // ============================================================
    // HIT BLINK
    // ============================================================

    private IEnumerator HitBlinkRoutine()
    {
        hitBlinking = true;

        int toggles =
            Mathf.Max(
                1,
                hitBlinkCount
            ) * 2;

        for (int i = 0;
             i < toggles;
             i++)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled =
                    !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(
                hitBlinkInterval
            );
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled =
                true;
        }

        hitBlinking = false;
        hitBlinkCoroutine = null;

        if (chasingPlayer)
        {
            UpdateChaseSprite();
        }
        else
        {
            UpdateWalkingSprite();
        }
    }


    // ============================================================
    // DETECTION
    // ============================================================

    private void CheckPlayer()
    {
        if (player == null)
            return;

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return;
        }

        float differenceX =
            player.position.x -
            transform.position.x;

        float horizontalDistance =
            Mathf.Abs(
                differenceX
            );

        float verticalDistance =
            Mathf.Abs(
                player.position.y -
                transform.position.y
            );

        bool playerIsInFront =
            movingRight
                ? differenceX > 0f
                : differenceX < 0f;

        if (!chasingPlayer)
        {
            if (playerIsInFront &&
                horizontalDistance <=
                    ActiveDetectionRange &&
                verticalDistance <=
                    detectionHeight &&
                HasClearLineOfSightToPlayer())
            {
                detectedPlayerNaturally = true;
                chasingPlayer = true;

                CancelPatrolPause();
                StopHorizontalMovement();

                UpdateChaseSprite();

                PlayNaturalDetectionVoices();
            }
        }
        else
        {
            if (horizontalDistance >
                    losePlayerDistance ||
                verticalDistance >
                    detectionHeight *
                    1.5f ||
                !HasClearLineOfSightToPlayer())
            {
                StopChasingPlayer();
            }
        }
    }


    // ============================================================
    // DETECT + CHASE VOICES
    // ============================================================

    private void PlayNaturalDetectionVoices()
    {
        if (!detectedPlayerNaturally)
            return;

        if (!detectionVoicePlayed)
        {
            PlayDetectSound();

            detectionVoicePlayed = true;
        }

        if (!chaseVoicePlayed &&
            chaseClip != null)
        {
            if (chaseVoiceCoroutine != null)
            {
                StopCoroutine(
                    chaseVoiceCoroutine
                );
            }

            chaseVoiceCoroutine =
                StartCoroutine(
                    ChaseVoiceAfterDelayRoutine()
                );
        }
    }


    private void PlayDetectSound()
    {
        if (!CanPlaySfx ||
            detectClip == null)
        {
            return;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return;
        }

        sfxSource.PlayOneShot(
            detectClip,
            detectVolume
        );
    }


    private IEnumerator ChaseVoiceAfterDelayRoutine()
    {
        if (chaseVoiceDelay > 0f)
        {
            yield return new WaitForSeconds(
                chaseVoiceDelay
            );
        }

        if (!CanBeTargeted ||
            !chasingPlayer ||
            !detectedPlayerNaturally ||
            player == null ||
            (playerHealth != null &&
             playerHealth.IsDead))
        {
            chaseVoiceCoroutine = null;
            yield break;
        }

        if (CanPlaySfx &&
            chaseClip != null)
        {
            sfxSource.PlayOneShot(
                chaseClip,
                chaseVolume
            );

            chaseVoicePlayed = true;
        }

        chaseVoiceCoroutine = null;
    }


    // ============================================================
    // CHASE
    // ============================================================

    private void ChasePlayer()
    {
        if (player == null)
            return;

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            StopAllCombatAfterPlayerDeath();
            return;
        }

        float differenceX =
            player.position.x -
            transform.position.x;

        float distanceX =
            Mathf.Abs(
                differenceX
            );

        float distanceY =
            Mathf.Abs(
                player.position.y -
                transform.position.y
            );

        if (differenceX < 0f)
        {
            movingRight = false;
            SetChaseLeftSprite();
        }
        else
        {
            movingRight = true;
            SetChaseRightSprite();
        }

        if (!HasClearLineOfSightToPlayer())
        {
            StopChasingPlayer();
            return;
        }

        if (distanceX <=
                ActiveAttackRange &&
            distanceY <=
                attackHeight)
        {
            StopHorizontalMovement();
            StartGuardAttack();
            return;
        }

        float direction =
            Mathf.Sign(
                differenceX
            );

        rb.linearVelocity =
            new Vector2(
                direction *
                ActiveChaseSpeed,
                rb.linearVelocity.y
            );
    }


    // ============================================================
    // ATTACK
    // ============================================================

    private void StartGuardAttack()
    {
        if (attackBusy ||
            !CanBeTargeted ||
            hitBlinking ||
            isKnockedBack)
        {
            return;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return;
        }

        attackCoroutine =
            StartCoroutine(
                GuardAttackRoutine()
            );
    }


    private IEnumerator GuardAttackRoutine()
    {
        attackBusy = true;
        float spriteDuration = isUnarmed ? unarmedCombat.attackAnimationDuration : attackSpriteDuration;
        float impactDelay = isUnarmed ? Mathf.Min(unarmedCombat.attackHitDelay, spriteDuration) : attackImpactDelay;
        float cooldown = isUnarmed ? unarmedCombat.attackCooldown : attackCooldown;

        StopHorizontalMovement();
        UpdateAttackSprite();

        if (CanPlaySfx &&
            swingClip != null &&
            (playerHealth == null ||
             !playerHealth.IsDead))
        {
            sfxSource.PlayOneShot(
                swingClip,
                swingVolume
            );
        }

        if (impactDelay > 0f)
        {
            yield return new WaitForSeconds(
                impactDelay
            );
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            FinishAttackImmediately();
            yield break;
        }

        if (CanBeTargeted &&
            !isKnockedBack &&
            !hitBlinking &&
            PlayerStillInAttackRange())
        {
            bool playerCanTakeDamage =
                playerHealth != null &&
                !playerHealth.IsDead &&
                !playerHealth.IsInvulnerable;

            if (playerCanTakeDamage)
            {
                playerHealth.TakeCombatDamage(
                    isUnarmed ? unarmedCombat.attackDamage : damageToPlayer,
                    isUnarmed ? GuardHitType.Fist : GuardHitType.Sword,
                    transform
                );

                PlayGuardHitsPlayerHaptic();
            }
        }

        float remainingSpriteTime =
            Mathf.Max(
                0f,
                spriteDuration -
                impactDelay
            );

        if (remainingSpriteTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingSpriteTime
            );
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            FinishAttackImmediately();
            yield break;
        }

        if (isUnarmed)
            SetIdleFrontSprite();
        else
            UpdateChaseSprite();

        float remainingCooldown =
            Mathf.Max(
                0f,
                cooldown - spriteDuration
            );

        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(
                remainingCooldown
            );
        }

        attackBusy = false;
        attackCoroutine = null;
    }


    private bool PlayerStillInAttackRange()
    {
        if (player == null)
            return false;

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return false;
        }

        float distanceX =
            Mathf.Abs(
                player.position.x -
                transform.position.x
            );

        float distanceY =
            Mathf.Abs(
                player.position.y -
                transform.position.y
            );

        return
            (!isUnarmed || movingRight == (player.position.x >= transform.position.x)) &&
            distanceX <=
                ActiveAttackRange &&
            distanceY <=
                attackHeight &&
            HasClearLineOfSightToPlayer();
    }


    private void FinishAttackImmediately()
    {
        attackBusy = false;
        attackCoroutine = null;
        chasingPlayer = false;

        StopHorizontalMovement();
        SetIdleFrontSprite();
    }


    private void StopAllCombatAfterPlayerDeath()
    {
        chasingPlayer = false;

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine = null;
        }

        if (chaseVoiceCoroutine != null)
        {
            StopCoroutine(
                chaseVoiceCoroutine
            );

            chaseVoiceCoroutine = null;
        }

        attackBusy = false;

        StopHorizontalMovement();

        if (!hitBlinking &&
            !isKnockedBack)
        {
            SetIdleFrontSprite();
        }
    }


    // ============================================================
    // PATROL
    // ============================================================

    private void Patrol()
    {
        if (patrolPaused)
        {
            StopHorizontalMovement();
            return;
        }

        if (ObstacleAhead())
        {
            StartPatrolPause();
            return;
        }

        if (movingRight)
        {
            rb.linearVelocity =
                new Vector2(
                    ActivePatrolSpeed,
                    rb.linearVelocity.y
                );

            SetWalkRightSprite();

            if (transform.position.x >=
                rightPatrolX -
                patrolEdgeTolerance)
            {
                StartPatrolPause();
            }
        }
        else
        {
            rb.linearVelocity =
                new Vector2(
                    -ActivePatrolSpeed,
                    rb.linearVelocity.y
                );

            SetWalkLeftSprite();

            if (transform.position.x <=
                leftPatrolX +
                patrolEdgeTolerance)
            {
                StartPatrolPause();
            }
        }
    }


    private void StartPatrolPause()
    {
        if (patrolPaused ||
            chasingPlayer ||
            hitBlinking ||
            isKnockedBack ||
            !CanBeTargeted)
        {
            return;
        }

        patrolPauseCoroutine =
            StartCoroutine(
                PatrolPauseRoutine()
            );
    }


    private IEnumerator PatrolPauseRoutine()
    {
        patrolPaused = true;

        StopHorizontalMovement();
        SetIdleFrontSprite();

        yield return new WaitForSeconds(
            isUnarmed ? wakeUpAnimation.patrolPauseBeforeBlink : pauseBeforeBlink
        );

        SetBlinkSprite();

        yield return new WaitForSeconds(
            isUnarmed ? wakeUpAnimation.patrolBlinkDuration : blinkDuration
        );

        SetIdleFrontSprite();

        yield return new WaitForSeconds(
            isUnarmed ? wakeUpAnimation.patrolPauseAfterBlink : pauseAfterBlink
        );

        movingRight =
            !movingRight;

        if (movingRight)
        {
            SetWalkRightSprite();
        }
        else
        {
            SetWalkLeftSprite();
        }

        patrolPaused = false;
        patrolPauseCoroutine = null;
    }


    private void CancelPatrolPause()
    {
        if (patrolPauseCoroutine != null)
        {
            StopCoroutine(
                patrolPauseCoroutine
            );

            patrolPauseCoroutine = null;
        }

        patrolPaused = false;
    }


    private void StopChasingPlayer()
    {
        chasingPlayer = false;

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine = null;
        }

        if (chaseVoiceCoroutine != null)
        {
            StopCoroutine(
                chaseVoiceCoroutine
            );

            chaseVoiceCoroutine = null;
        }

        attackBusy = false;

        StopHorizontalMovement();

        movingRight =
            transform.position.x <
            startX;

        if (movingRight)
        {
            SetLookRightSprite();
        }
        else
        {
            SetLookLeftSprite();
        }
    }


    // ============================================================
    // LINE OF SIGHT
    // ============================================================

    private bool HasClearLineOfSightToPlayer()
    {
        if (player == null ||
            bodyCollider == null)
        {
            return false;
        }

        Vector2 origin =
            bodyCollider.bounds.center;

        Vector2 direction =
            (Vector2)player.position -
            origin;

        float distance =
            direction.magnitude;

        if (distance <= 0.01f)
            return true;

        direction.Normalize();

        RaycastHit2D[] hits =
            Physics2D.RaycastAll(
                origin,
                direction,
                distance
            );

        float closestDistance =
            Mathf.Infinity;

        Transform closestTransform =
            null;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider ==
                bodyCollider)
            {
                continue;
            }

            if (hit.collider.isTrigger)
                continue;

            if (hit.distance <
                closestDistance)
            {
                closestDistance =
                    hit.distance;

                closestTransform =
                    hit.transform;
            }
        }

        if (closestTransform == null)
            return true;

        return IsPlayerTransform(
            closestTransform
        );
    }


    // ============================================================
    // OBSTACLE DETECTION
    // ============================================================

    private bool ObstacleAhead()
    {
        if (bodyCollider == null)
            return false;

        Bounds bounds =
            bodyCollider.bounds;

        float direction =
            movingRight
                ? 1f
                : -1f;

        float originX =
            direction > 0f
                ? bounds.max.x + 0.01f
                : bounds.min.x - 0.01f;

        float height =
            bounds.size.y;

        float lowY =
            bounds.min.y +
            height *
            0.20f;

        float middleY =
            bounds.center.y;

        float highY =
            bounds.min.y +
            height *
            0.80f;

        return
            ObstacleRay(
                originX,
                lowY,
                direction
            ) ||
            ObstacleRay(
                originX,
                middleY,
                direction
            ) ||
            ObstacleRay(
                originX,
                highY,
                direction
            );
    }


    private bool ObstacleRay(
        float originX,
        float originY,
        float direction
    )
    {
        RaycastHit2D[] hits =
            Physics2D.RaycastAll(
                new Vector2(
                    originX,
                    originY
                ),
                Vector2.right *
                direction,
                obstacleCheckDistance
            );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider ==
                bodyCollider)
            {
                continue;
            }

            if (hit.collider.isTrigger)
                continue;

            if (IsPlayerTransform(
                    hit.transform))
            {
                continue;
            }

            return true;
        }

        return false;
    }


    // ============================================================
    // COLLISION
    // ============================================================

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (!CanBeTargeted ||
            isKnockedBack)
        {
            return;
        }

        if (IsPlayerTransform(
                collision.transform))
        {
            return;
        }

        foreach (ContactPoint2D contact
                 in collision.contacts)
        {
            if (Mathf.Abs(
                    contact.normal.x) >
                0.5f)
            {
                if (chasingPlayer)
                {
                    StopChasingPlayer();
                }
                else
                {
                    StartPatrolPause();
                }

                break;
            }
        }
    }


    // ============================================================
    // FALL
    // ============================================================

    private void Fall(bool knockedOut)
    {
        if (!CanBeTargeted)
            return;

        float floorY =
            bodyCollider != null
                ? bodyCollider.bounds.min.y
                : transform.position.y;

        isDead = !knockedOut;
        isKnockedOut = knockedOut;
        healthBar.ResetHidden(0f);

        bool playerIsLeft =
            player != null &&
            player.position.x <
            transform.position.x;

        CancelPatrolPause();

        if (hitBlinkCoroutine != null)
        {
            StopCoroutine(
                hitBlinkCoroutine
            );
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );
        }

        if (chaseVoiceCoroutine != null)
        {
            StopCoroutine(
                chaseVoiceCoroutine
            );
        }

        hitBlinkCoroutine = null;
        knockbackCoroutine = null;
        attackCoroutine = null;
        chaseVoiceCoroutine = null;

        chasingPlayer = false;
        attackBusy = false;
        hitBlinking = false;
        isKnockedBack = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled =
                true;

            SetGuardSprite(playerIsLeft ? deathWhenPlayerLeftSprite : deathWhenPlayerRightSprite);
        }
        if (bodyCollider != null)
        {
            bodyCollider.enabled =
                false;
        }

        if (rb != null)
        {
            rb.simulated =
                false;
        }

        if (!knockedOut && CanPlaySfx && deathClip != null)
        {
            sfxSource.PlayOneShot(
                deathClip,
                deathVolume
            );
        }

        if (knockedOut)
        {
            wakeAt = Time.time + knockoutDuration;
            knockoutStars.Show(transform, playerIsLeft == mirrorStarsWhenPlayerLeft);
        }

        if (!isUnarmed && !weaponDropped && weaponObject != null)
            StartCoroutine(DropWeaponRoutine(playerIsLeft, floorY));
    }
    private IEnumerator DropWeaponRoutine(
        bool playerIsLeft,
        float floorY
    )
    {
        if (weaponDropped ||
            weaponObject == null)
        {
            yield break;
        }

        weaponDropped = true;
        weaponDropInProgress = true;

        weaponObject.transform.SetParent(
            null,
            true
        );

        weaponObject.SetActive(
            true
        );

        Collider2D weaponCollider =
            weaponObject.GetComponent<Collider2D>();

        Rigidbody2D weaponRb =
            weaponObject.GetComponent<Rigidbody2D>();

        if (weaponRb != null)
        {
            weaponRb.linearVelocity =
                Vector2.zero;

            weaponRb.angularVelocity =
                0f;

            weaponRb.simulated =
                false;
        }

        if (weaponCollider != null)
        {
            weaponCollider.enabled =
                false;
        }

        Vector3 startPosition =
            transform.position +
            new Vector3(
                weaponSpawnOffset.x,
                weaponSpawnOffset.y,
                0f
            );

        weaponObject.transform.position =
            startPosition;

        float dropDirection;

        if (player != null)
        {
            float difference =
                player.position.x -
                transform.position.x;

            if (Mathf.Abs(
                    difference) >
                0.01f)
            {
                dropDirection =
                    Mathf.Sign(
                        difference
                    );
            }
            else
            {
                dropDirection =
                    playerIsLeft
                        ? -1f
                        : 1f;
            }
        }
        else
        {
            dropDirection =
                playerIsLeft
                    ? -1f
                    : 1f;
        }

        float targetX =
            transform.position.x +
            dropDirection *
            weaponDropDistance;

        float targetY =
            CalculateWeaponLandingY(
                floorY
            );

        Vector3 endPosition =
            new Vector3(
                targetX,
                targetY,
                startPosition.z
            );

        Quaternion startRotation =
            weaponObject.transform.rotation;

        Quaternion endRotation =
            Quaternion.Euler(
                0f,
                0f,
                weaponLandingRotation
            );

        float safeDuration =
            Mathf.Max(
                0.05f,
                weaponDropDuration
            );

        float timer = 0f;

        while (timer <
               safeDuration && weaponObject != null)
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    safeDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    smoothT
                );

            position.y +=
                Mathf.Sin(
                    smoothT *
                    Mathf.PI
                ) *
                weaponDropArcHeight;

            weaponObject.transform.position =
                position;

            weaponObject.transform.rotation =
                Quaternion.Lerp(
                    startRotation,
                    endRotation,
                    smoothT
                );

            yield return null;
        }

        if (weaponObject == null)
        {
            weaponDropInProgress = false;
            yield break;
        }

        weaponObject.transform.position =
            endPosition;

        weaponObject.transform.rotation =
            endRotation;

        if (weaponRb != null)
        {
            weaponRb.linearVelocity =
                Vector2.zero;

            weaponRb.angularVelocity =
                0f;

            weaponRb.simulated =
                false;
        }

        if (weaponCollider != null)
        {
            weaponCollider.enabled =
                true;

            if (weaponColliderBecomesTrigger)
            {
                weaponCollider.isTrigger =
                    true;
            }
        }

        weaponDropInProgress = false;
    }


    // ============================================================
    // WEAPON LANDING HEIGHT
    // ============================================================

    private float CalculateWeaponLandingY(
        float floorY
    )
    {
        if (weaponObject == null)
        {
            return
                floorY +
                weaponFloorGap;
        }

        SpriteRenderer weaponRenderer =
            weaponObject.GetComponent<SpriteRenderer>();

        if (weaponRenderer == null)
        {
            weaponRenderer =
                weaponObject
                    .GetComponentInChildren<SpriteRenderer>();
        }

        if (weaponRenderer == null)
        {
            return
                floorY +
                weaponFloorGap;
        }

        Vector3 savedPosition =
            weaponObject.transform.position;

        Quaternion savedRotation =
            weaponObject.transform.rotation;

        weaponObject.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                weaponLandingRotation
            );

        Physics2D.SyncTransforms();

        Bounds visualBounds =
            weaponRenderer.bounds;

        float pivotToBottom =
            weaponObject.transform.position.y -
            visualBounds.min.y;

        weaponObject.transform.position =
            savedPosition;

        weaponObject.transform.rotation =
            savedRotation;

        Physics2D.SyncTransforms();

        return
            floorY +
            pivotToBottom +
            weaponFloorGap;
    }


    // ============================================================
    // HAPTICS
    // ============================================================

    private void PlayPlayerHitsGuardHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            playerHitsGuardHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }


    private void PlayPlayerSwordHitsGuardHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            playerSwordHitsGuardHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }


    private void PlayGuardHitsPlayerHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            guardHitsPlayerHapticMs,
            MicroHaptics.IOSHapticStyle.Heavy
        );
    }


    // ============================================================
    // MOVEMENT / SPRITES
    // ============================================================

    private void StopHorizontalMovement()
    {
        if (rb == null)
            return;

        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );
    }


    private void UpdateWalkingSprite()
    {
        if (movingRight)
        {
            SetWalkRightSprite();
        }
        else
        {
            SetWalkLeftSprite();
        }
    }


    private void UpdateChaseSprite()
    {
        if (player == null)
            return;

        if (player.position.x <
            transform.position.x)
        {
            movingRight = false;
            SetChaseLeftSprite();
        }
        else
        {
            movingRight = true;
            SetChaseRightSprite();
        }
    }


    private void UpdateAttackSprite()
    {
        if (player == null)
            return;

        if (player.position.x <
            transform.position.x)
        {
            movingRight = false;
            SetAttackLeftSprite();
        }
        else
        {
            movingRight = true;
            SetAttackRightSprite();
        }
    }


    private void SetIdleFrontSprite() => SetGuardSprite(idleFrontSprite, unarmedSprites.angry);
    private void SetBlinkSprite() => SetGuardSprite(blinkSprite, unarmedSprites.angryBlink);
    private void SetLookLeftSprite() => SetGuardSprite(lookLeftSprite, unarmedSprites.angry);
    private void SetLookRightSprite() => SetGuardSprite(lookRightSprite, unarmedSprites.angry);
    private void SetWalkRightSprite() => SetGuardSprite(walkRightSprite, unarmedSprites.walkRight);
    private void SetWalkLeftSprite() => SetGuardSprite(walkLeftSprite, unarmedSprites.walkLeft);
    private void SetChaseLeftSprite() => SetGuardSprite(chaseLeftSprite, unarmedSprites.runLeft);
    private void SetChaseRightSprite() => SetGuardSprite(chaseRightSprite, unarmedSprites.runRight);
    private void SetAttackLeftSprite() => SetGuardSprite(attackLeftSprite, unarmedSprites.attackLeft);
    private void SetAttackRightSprite() => SetGuardSprite(attackRightSprite, unarmedSprites.attackRight);
    private bool IsPlayerTransform(
        Transform target
    )
    {
        if (player == null ||
            target == null)
        {
            return false;
        }

        return
            target == player ||
            target.IsChildOf(player) ||
            player.IsChildOf(target);
    }
}
