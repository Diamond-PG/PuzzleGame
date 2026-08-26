using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GuardEnemy : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerKick playerKick;

    [Header("VISUAL")]
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

    [Header("PLAYER DETECTION")]
    [SerializeField] private float detectionDistance = 1.7f;
    [SerializeField] private float detectionHeight = 0.35f;
    [SerializeField] private float losePlayerDistance = 6f;

    [Header("CHASE")]
    [SerializeField] private float chaseSpeed = 1.65f;

    [Header("GUARD HEALTH")]
    [SerializeField, Min(1)] private int maxHealth = 4;
    [SerializeField] private float playerHitDistance = 0.8f;
    [SerializeField] private float playerKickImpactDelay = 0.08f;

    [Header("HIT BLINK")]
    [SerializeField, Min(1)] private int hitBlinkCount = 3;
    [SerializeField] private float hitBlinkInterval = 0.12f;

    [Header("HIT KNOCKBACK")]
    [SerializeField] private float knockbackForce = 2.1f;
    [SerializeField] private float knockbackUpForce = 3f;
    [SerializeField] private float knockbackDuration = 0.18f;

    [Header("GUARD ATTACK")]
    [SerializeField, Min(1)] private int damageToPlayer = 1;
    [SerializeField] private float attackDistance = 0.85f;
    [SerializeField] private float attackHeight = 1.1f;
    [SerializeField] private float attackImpactDelay = 0.16f;
    [SerializeField] private float attackSpriteDuration = 0.35f;
    [SerializeField] private float attackCooldown = 1.15f;

    [Header("HAPTICS")]
    [SerializeField] private bool useHaptics = true;

    [SerializeField, Range(5, 100)]
    private int playerHitsGuardHapticMs = 20;

    [SerializeField, Range(5, 150)]
    private int guardHitsPlayerHapticMs = 40;

    [Header("AUDIO - OPTIONAL")]
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip detectClip;

    [Range(0f, 1f)]
    [SerializeField] private float detectVolume = 1f;

    [SerializeField] private AudioClip hurtClip;

    [Range(0f, 1f)]
    [SerializeField] private float hurtVolume = 1f;

    [SerializeField] private AudioClip swingClip;

    [Range(0f, 1f)]
    [SerializeField] private float swingVolume = 1f;

    [SerializeField] private AudioClip deathClip;

    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 1f;

    // ============================================================
    // WEAPON DROP
    // ============================================================

    [Header("WEAPON DROP")]
    [SerializeField] private GameObject weaponObject;

    [Tooltip("Откуда начинается падение меча относительно Guard.")]
    [SerializeField] private Vector2 weaponSpawnOffset =
        new Vector2(0f, 0.18f);

    [Tooltip(
        "Насколько меч падает в сторону игрока. " +
        "Если Guard у левой стены - меч идёт вправо. " +
        "Если Guard у правой стены - меч идёт влево."
    )]
    [SerializeField] private float weaponDropDistance = 0.65f;

    [Tooltip("Длительность короткого падения меча.")]
    [SerializeField] private float weaponDropDuration = 0.38f;

    [Tooltip(
        "Небольшая дуга падения. " +
        "Это НЕ физический прыжок."
    )]
    [SerializeField] private float weaponDropArcHeight = 0.10f;

    [Tooltip(
        "Конечный угол меча. " +
        "Для нашего Sword оставляем 90."
    )]
    [SerializeField] private float weaponLandingRotation = 90f;

    [Tooltip(
        "Маленький зазор между нижней частью картинки меча и полом."
    )]
    [SerializeField] private float weaponFloorGap = 0.015f;

    [Tooltip(
        "После падения Collider меча становится Trigger, " +
        "чтобы он не мешал игроку и позже его можно было подобрать."
    )]
    [SerializeField] private bool weaponColliderBecomesTrigger = true;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private Camera mainCamera;

    private float startX;
    private float leftPatrolX;
    private float rightPatrolX;

    private int currentHealth;

    private bool movingRight;
    private bool chasingPlayer;
    private bool patrolPaused;
    private bool playerHitBusy;
    private bool attackBusy;
    private bool hitBlinking;
    private bool isKnockedBack;
    private bool isDead;
    private bool weaponDropped;

    private Coroutine patrolPauseCoroutine;
    private Coroutine hitBlinkCoroutine;
    private Coroutine knockbackCoroutine;
    private Coroutine attackCoroutine;

    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        mainCamera = Camera.main;
        currentHealth = maxHealth;

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

        if (weaponObject != null)
        {
            weaponObject.SetActive(false);
        }

        if (sfxSource == null)
        {
            sfxSource =
                GetComponent<AudioSource>();
        }

        FindPlayerLinks();
    }

    private void Start()
    {
        startX = transform.position.x;

        leftPatrolX =
            startX - patrolDistance;

        rightPatrolX =
            startX + patrolDistance;

        SetIdleFrontSprite();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (player == null)
            FindPlayerLinks();

        if (mainCamera == null ||
            playerHitBusy ||
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

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryRequestPlayerHit(
                Mouse.current.position.ReadValue()
            );
        }

        if (Touchscreen.current != null &&
            Touchscreen.current
                .primaryTouch
                .press
                .wasPressedThisFrame)
        {
            TryRequestPlayerHit(
                Touchscreen.current
                    .primaryTouch
                    .position
                    .ReadValue()
            );
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
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

        if (playerKick == null)
        {
            playerKick =
                player.GetComponent<PlayerKick>();
        }
    }

    // ============================================================
    // PLAYER HITS GUARD
    // ============================================================

    private void TryRequestPlayerHit(
        Vector2 screenPosition
    )
    {
        if (isDead ||
            hitBlinking ||
            isKnockedBack ||
            player == null ||
            bodyCollider == null ||
            mainCamera == null)
        {
            return;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return;
        }

        if (!IsValidScreenPosition(
                screenPosition))
        {
            return;
        }

        float cameraDistance =
            Mathf.Abs(
                transform.position.z -
                mainCamera.transform.position.z
            );

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    cameraDistance
                )
            );

        if (!IsFiniteVector3(
                worldPosition))
        {
            return;
        }

        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                new Vector2(
                    worldPosition.x,
                    worldPosition.y
                )
            );

        bool clickedGuard = false;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit == bodyCollider ||
                hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                clickedGuard = true;
                break;
            }
        }

        if (!clickedGuard)
            return;

        float distance =
            Vector2.Distance(
                player.position,
                transform.position
            );

        if (distance >
            playerHitDistance)
        {
            return;
        }

        if (playerKick == null)
        {
            playerKick =
                player.GetComponent<PlayerKick>();
        }

        if (playerKick == null)
            return;

        bool kickStarted =
            playerKick.KickToward(
                transform.position
            );

        if (!kickStarted)
            return;

        playerHitBusy = true;

        StartCoroutine(
            PlayerKickImpactRoutine()
        );
    }

    private IEnumerator PlayerKickImpactRoutine()
    {
        if (playerKickImpactDelay > 0f)
        {
            yield return new WaitForSeconds(
                playerKickImpactDelay
            );
        }

        if (!isDead)
        {
            ReceivePlayerHit(1);
        }

        playerHitBusy = false;
    }

    private void ReceivePlayerHit(
        int damage
    )
    {
        if (isDead ||
            hitBlinking ||
            isKnockedBack ||
            damage <= 0)
        {
            return;
        }

        currentHealth =
            Mathf.Max(
                0,
                currentHealth - damage
            );

        PlayPlayerHitsGuardHaptic();

        if (sfxSource != null &&
            hurtClip != null)
        {
            sfxSource.PlayOneShot(
                hurtClip,
                hurtVolume
            );
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        /*
         * Если Guard ударили со спины,
         * сразу разворачиваемся к игроку.
         */
        AggroAndFacePlayerAfterHit();

        CancelPatrolPause();

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine = null;
            attackBusy = false;
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
            spriteRenderer.enabled = true;
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
            Mathf.Abs(differenceX);

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
                    detectionDistance &&
                verticalDistance <=
                    detectionHeight &&
                HasClearLineOfSightToPlayer())
            {
                chasingPlayer = true;

                CancelPatrolPause();
                StopHorizontalMovement();

                UpdateChaseSprite();
                PlayDetectSound();
            }
        }
        else
        {
            if (horizontalDistance >
                    losePlayerDistance ||
                verticalDistance >
                    detectionHeight * 1.5f ||
                !HasClearLineOfSightToPlayer())
            {
                StopChasingPlayer();
            }
        }
    }

    private void PlayDetectSound()
    {
        if (sfxSource == null ||
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
            Mathf.Abs(differenceX);

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

        if (distanceX <= attackDistance &&
            distanceY <= attackHeight)
        {
            StopHorizontalMovement();
            StartGuardAttack();
            return;
        }

        float direction =
            Mathf.Sign(differenceX);

        rb.linearVelocity =
            new Vector2(
                direction * chaseSpeed,
                rb.linearVelocity.y
            );
    }

    // ============================================================
    // ATTACK
    // ============================================================

    private void StartGuardAttack()
    {
        if (attackBusy ||
            isDead ||
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

        StopHorizontalMovement();
        UpdateAttackSprite();

        if (sfxSource != null &&
            swingClip != null &&
            (playerHealth == null ||
             !playerHealth.IsDead))
        {
            sfxSource.PlayOneShot(
                swingClip,
                swingVolume
            );
        }

        if (attackImpactDelay > 0f)
        {
            yield return new WaitForSeconds(
                attackImpactDelay
            );
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            FinishAttackImmediately();
            yield break;
        }

        if (!isDead &&
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
                playerHealth.TakeDamage(
                    damageToPlayer
                );

                PlayGuardHitsPlayerHaptic();
            }
        }

        float remainingSpriteTime =
            Mathf.Max(
                0f,
                attackSpriteDuration -
                attackImpactDelay
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

        UpdateChaseSprite();

        float remainingCooldown =
            Mathf.Max(
                0f,
                attackCooldown -
                attackSpriteDuration
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
            distanceX <= attackDistance &&
            distanceY <= attackHeight &&
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
                    patrolSpeed,
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
                    -patrolSpeed,
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
            isDead)
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

        if (movingRight)
            SetLookRightSprite();
        else
            SetLookLeftSprite();

        yield return new WaitForSeconds(
            pauseBeforeBlink
        );

        SetBlinkSprite();

        yield return new WaitForSeconds(
            blinkDuration
        );

        if (movingRight)
            SetLookRightSprite();
        else
            SetLookLeftSprite();

        yield return new WaitForSeconds(
            pauseAfterBlink
        );

        movingRight = !movingRight;

        if (movingRight)
            SetLookRightSprite();
        else
            SetLookLeftSprite();

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

        attackBusy = false;

        StopHorizontalMovement();

        movingRight =
            transform.position.x <
            startX;

        if (movingRight)
            SetLookRightSprite();
        else
            SetLookLeftSprite();
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
    // OBSTACLES
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
            height * 0.20f;

        float middleY =
            bounds.center.y;

        float highY =
            bounds.min.y +
            height * 0.80f;

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

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (isDead ||
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
    // DEATH
    // ============================================================

    private void Die()
    {
        if (isDead)
            return;

        /*
         * ВАЖНО.
         *
         * Сохраняем уровень пола ДО того,
         * как отключим Collider стражника.
         *
         * Guard уже стоит на настоящем полу,
         * поэтому bounds.min.y — это именно
         * уровень поверхности под его ногами.
         */
        float floorY =
            bodyCollider != null
                ? bodyCollider.bounds.min.y
                : transform.position.y;

        isDead = true;

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

        hitBlinkCoroutine = null;
        knockbackCoroutine = null;
        attackCoroutine = null;

        chasingPlayer = false;
        attackBusy = false;
        hitBlinking = false;
        isKnockedBack = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity = 0f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            if (playerIsLeft)
            {
                if (deathWhenPlayerLeftSprite != null)
                {
                    spriteRenderer.sprite =
                        deathWhenPlayerLeftSprite;
                }
            }
            else
            {
                if (deathWhenPlayerRightSprite != null)
                {
                    spriteRenderer.sprite =
                        deathWhenPlayerRightSprite;
                }
            }
        }

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (rb != null)
        {
            rb.simulated = false;
        }

        if (sfxSource != null &&
            deathClip != null)
        {
            sfxSource.PlayOneShot(
                deathClip,
                deathVolume
            );
        }

        /*
         * Теперь запускаем полностью
         * контролируемое падение меча.
         */
        StartCoroutine(
            DropWeaponRoutine(
                playerIsLeft,
                floorY
            )
        );
    }

    // ============================================================
    // CONTROLLED WEAPON DROP
    // ============================================================

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

        /*
         * Отделяем Weapon от Guard.
         */
        weaponObject.transform.SetParent(
            null,
            true
        );

        weaponObject.SetActive(true);

        Collider2D weaponCollider =
            weaponObject.GetComponent<Collider2D>();

        Rigidbody2D weaponRb =
            weaponObject.GetComponent<Rigidbody2D>();

        /*
         * ВАЖНО:
         *
         * Никакой физики падения.
         * Rigidbody не имеет права толкать,
         * бросать или выталкивать меч.
         */
        if (weaponRb != null)
        {
            weaponRb.linearVelocity =
                Vector2.zero;

            weaponRb.angularVelocity =
                0f;

            weaponRb.simulated = false;
        }

        /*
         * Во время анимации Collider выключен,
         * поэтому ни пол, ни стены не могут
         * физически вытолкнуть меч.
         */
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
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

        /*
         * Если Player находится слева,
         * значит Guard обычно погиб возле
         * ПРАВОЙ стены -> меч идёт ВЛЕВО.
         *
         * Если Player находится справа,
         * Guard погиб возле ЛЕВОЙ стены ->
         * меч идёт ВПРАВО.
         *
         * То есть меч падает НА СТОРОНУ PLAYER.
         */
        float dropDirection;

        if (player != null)
        {
            float difference =
                player.position.x -
                transform.position.x;

            if (Mathf.Abs(difference) >
                0.01f)
            {
                dropDirection =
                    Mathf.Sign(difference);
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

        /*
         * Вычисляем правильный Y НЕ ПО COLLIDER,
         * а по настоящей видимой картинке Sword.
         *
         * Поэтому Rotation 90 больше никогда
         * не сможет изменить высоту пола.
         */
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
            safeDuration)
        {
            timer += Time.deltaTime;

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

            /*
             * Только маленькая визуальная дуга.
             * Это Transform-анимация,
             * а НЕ физический прыжок.
             */
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

        /*
         * ЖЁСТКО ставим финальную точку.
         *
         * После этой строки меч уже
         * ничего не может изменить.
         */
        weaponObject.transform.position =
            endPosition;

        weaponObject.transform.rotation =
            endRotation;

        /*
         * Rigidbody остаётся выключенным.
         *
         * То есть:
         * - не провалится;
         * - не подпрыгнет;
         * - не уедет;
         * - не залетит в стену.
         */
        if (weaponRb != null)
        {
            weaponRb.linearVelocity =
                Vector2.zero;

            weaponRb.angularVelocity =
                0f;

            weaponRb.simulated = false;
        }

        /*
         * Collider снова включаем,
         * но делаем Trigger.
         *
         * Позже на нём удобно сделаем
         * подбор меча.
         */
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;

            if (weaponColliderBecomesTrigger)
            {
                weaponCollider.isTrigger = true;
            }
        }
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
            weaponObject.GetComponent<
                SpriteRenderer
            >();

        if (weaponRenderer == null)
        {
            weaponRenderer =
                weaponObject
                    .GetComponentInChildren<
                        SpriteRenderer
                    >();
        }

        if (weaponRenderer == null)
        {
            return
                floorY +
                weaponFloorGap;
        }

        /*
         * Временно ставим Sword в угол 90,
         * чтобы узнать настоящую нижнюю точку
         * именно ЛЕЖАЩЕЙ картинки.
         *
         * Это происходит мгновенно внутри
         * одного кадра и игрок этого
         * промежуточного действия не увидит.
         */
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

        /*
         * Расстояние от Pivot объекта
         * до самого низа видимого Sprite.
         */
        float pivotToBottom =
            weaponObject.transform.position.y -
            visualBounds.min.y;

        /*
         * Возвращаем исходную позу
         * перед началом анимации.
         */
        weaponObject.transform.position =
            savedPosition;

        weaponObject.transform.rotation =
            savedRotation;

        Physics2D.SyncTransforms();

        /*
         * Теперь гарантированно:
         *
         * НИЗ ВИДИМОГО МЕЧА =
         * ПОВЕРХНОСТЬ ПОЛА + МАЛЕНЬКИЙ ЗАЗОР.
         */
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
    // SPRITES
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
            SetWalkRightSprite();
        else
            SetWalkLeftSprite();
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

    private void SetIdleFrontSprite()
    {
        if (spriteRenderer != null &&
            idleFrontSprite != null)
        {
            spriteRenderer.sprite =
                idleFrontSprite;
        }
    }

    private void SetBlinkSprite()
    {
        if (spriteRenderer != null &&
            blinkSprite != null)
        {
            spriteRenderer.sprite =
                blinkSprite;
        }
    }

    private void SetLookLeftSprite()
    {
        if (spriteRenderer != null &&
            lookLeftSprite != null)
        {
            spriteRenderer.sprite =
                lookLeftSprite;
        }
    }

    private void SetLookRightSprite()
    {
        if (spriteRenderer != null &&
            lookRightSprite != null)
        {
            spriteRenderer.sprite =
                lookRightSprite;
        }
    }

    private void SetWalkRightSprite()
    {
        if (spriteRenderer != null &&
            walkRightSprite != null)
        {
            spriteRenderer.sprite =
                walkRightSprite;
        }
    }

    private void SetWalkLeftSprite()
    {
        if (spriteRenderer != null &&
            walkLeftSprite != null)
        {
            spriteRenderer.sprite =
                walkLeftSprite;
        }
    }

    private void SetChaseLeftSprite()
    {
        if (spriteRenderer != null &&
            chaseLeftSprite != null)
        {
            spriteRenderer.sprite =
                chaseLeftSprite;
        }
    }

    private void SetChaseRightSprite()
    {
        if (spriteRenderer != null &&
            chaseRightSprite != null)
        {
            spriteRenderer.sprite =
                chaseRightSprite;
        }
    }

    private void SetAttackLeftSprite()
    {
        if (spriteRenderer != null &&
            attackLeftSprite != null)
        {
            spriteRenderer.sprite =
                attackLeftSprite;
        }
    }

    private void SetAttackRightSprite()
    {
        if (spriteRenderer != null &&
            attackRightSprite != null)
        {
            spriteRenderer.sprite =
                attackRightSprite;
        }
    }

    // ============================================================
    // HELPERS
    // ============================================================

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

    private bool IsValidScreenPosition(
        Vector2 position
    )
    {
        if (!IsFinite(position.x) ||
            !IsFinite(position.y))
        {
            return false;
        }

        return
            position.x >= 0f &&
            position.y >= 0f &&
            position.x <= Screen.width &&
            position.y <= Screen.height;
    }

    private bool IsFiniteVector3(
        Vector3 value
    )
    {
        return
            IsFinite(value.x) &&
            IsFinite(value.y) &&
            IsFinite(value.z);
    }

    private bool IsFinite(
        float value
    )
    {
        return
            !float.IsNaN(value) &&
            !float.IsInfinity(value);
    }
}