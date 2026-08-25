using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SkeletonEnemy : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerKick playerKick;

    [Header("VISUAL")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("SPRITES")]
    [SerializeField] private Sprite patrolLeftSprite;
    [SerializeField] private Sprite patrolRightSprite;
    [SerializeField] private Sprite attackLeftSprite;
    [SerializeField] private Sprite attackRightSprite;
    [SerializeField] private Sprite blinkSprite;

    [Header("DEATH SPRITES")]
    [Tooltip(
        "Skeleton 6. Используется, когда игрок находится справа от скелета."
    )]
    [SerializeField] private Sprite deathFromRightSprite;

    [Tooltip(
        "Skeleton 7. Используется, когда игрок находится слева от скелета."
    )]
    [SerializeField] private Sprite deathFromLeftSprite;

    [Header("PATROL")]
    [SerializeField] private float patrolSpeed = 1.2f;
    [SerializeField] private float patrolDistance = 1.5f;
    [SerializeField] private float patrolEdgeTolerance = 0.08f;

    [Header("PATROL PAUSE")]
    [SerializeField] private float pauseBeforeBlink = 1.2f;
    [SerializeField] private float blinkDuration = 0.18f;
    [SerializeField] private float pauseAfterBlink = 0.35f;

    [Header("OBSTACLE DETECTION")]
    [SerializeField] private float obstacleCheckDistance = 0.05f;

    [Header("PLAYER DETECTION")]
    [SerializeField] private float detectionDistance = 1.5f;
    [SerializeField] private float detectionHeight = 0.35f;
    [SerializeField] private float losePlayerDistance = 6f;

    [Header("CHASE")]
    [SerializeField] private float chaseSpeed = 1.8f;

    [Header("SKELETON HEALTH")]
    [SerializeField, Min(1)] private int maxHealth = 2;

    [SerializeField] private float playerHitDistance = 1.5f;
    [SerializeField] private float playerKickImpactDelay = 0.08f;

    [Header("HIT BLINK")]
    [SerializeField, Min(1)] private int hitBlinkCount = 3;
    [SerializeField] private float hitBlinkInterval = 0.12f;

    [Header("HIT KNOCKBACK")]
    [Tooltip("Сила отскока скелета назад после удара.")]
    [SerializeField] private float knockbackForce = 2.2f;

    [Tooltip("Сколько времени длится отскок.")]
    [SerializeField] private float knockbackDuration = 0.12f;

    [Header("SKELETON ATTACK")]
    [SerializeField, Min(1)] private int damageToPlayer = 1;

    [SerializeField] private float attackDistance = 0.8f;
    [SerializeField] private float attackHeight = 1.2f;
    [SerializeField] private float attackImpactDelay = 0.15f;
    [SerializeField] private float attackCooldown = 1.1f;

    [Header("AUDIO - OPTIONAL")]
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip skeletonHitClip;

    [Range(0f, 1f)]
    [SerializeField] private float skeletonHitVolume = 1f;

    [SerializeField] private AudioClip attackImpactClip;

    [Range(0f, 1f)]
    [SerializeField] private float attackImpactVolume = 1f;

    [SerializeField] private AudioClip deathClip;

    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 1f;

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

    private Coroutine patrolPauseCoroutine;
    private Coroutine hitBlinkCoroutine;
    private Coroutine knockbackCoroutine;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        mainCamera = Camera.main;

        currentHealth = maxHealth;

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
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

        UpdatePatrolSprite();
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

        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                new Vector2(
                    worldPosition.x,
                    worldPosition.y
                )
            );

        bool clickedSkeleton =
            false;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit == bodyCollider ||
                hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                clickedSkeleton = true;
                break;
            }
        }

        if (!clickedSkeleton)
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

        if (sfxSource != null &&
            skeletonHitClip != null)
        {
            sfxSource.PlayOneShot(
                skeletonHitClip,
                skeletonHitVolume
            );
        }

        /*
         * Если это смертельный удар —
         * сразу переходим в состояние смерти.
         *
         * Здесь же будет определено,
         * с какой стороны находится игрок.
         */
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        CancelPatrolPause();

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine = null;
            attackBusy = false;
        }

        /*
         * Отскок после обычного,
         * не смертельного удара.
         */
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

        /*
         * Моргание после попадания.
         */
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

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        float direction;

        /*
         * Игрок слева —
         * скелет отлетает вправо.
         *
         * Игрок справа —
         * скелет отлетает влево.
         */
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
                    rb.linearVelocity.y
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
            spriteRenderer.enabled =
                true;
        }

        hitBlinking = false;
        hitBlinkCoroutine = null;

        if (chasingPlayer)
        {
            UpdateAttackSprite();
        }
        else
        {
            UpdatePatrolSprite();
        }
    }

    private void CheckPlayer()
    {
        if (player == null)
            return;

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
                detectionDistance &&
                verticalDistance <=
                detectionHeight &&
                HasClearLineOfSightToPlayer())
            {
                chasingPlayer = true;

                CancelPatrolPause();
                StopHorizontalMovement();

                UpdateAttackSprite();
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

    private void ChasePlayer()
    {
        if (player == null)
            return;

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
            SetAttackSpriteLeft();
        }
        else
        {
            movingRight = true;
            SetAttackSpriteRight();
        }

        if (!HasClearLineOfSightToPlayer())
        {
            StopChasingPlayer();
            return;
        }

        if (distanceX <=
                attackDistance &&
            distanceY <=
                attackHeight)
        {
            StopHorizontalMovement();

            StartSkeletonAttack();
            return;
        }

        float direction =
            Mathf.Sign(
                differenceX
            );

        rb.linearVelocity =
            new Vector2(
                direction *
                chaseSpeed,
                rb.linearVelocity.y
            );
    }

    private void StartSkeletonAttack()
    {
        if (attackBusy ||
            isDead ||
            hitBlinking ||
            isKnockedBack)
        {
            return;
        }

        attackCoroutine =
            StartCoroutine(
                SkeletonAttackRoutine()
            );
    }

    private IEnumerator SkeletonAttackRoutine()
    {
        attackBusy = true;

        StopHorizontalMovement();
        UpdateAttackSprite();

        if (attackImpactDelay > 0f)
        {
            yield return new WaitForSeconds(
                attackImpactDelay
            );
        }

        if (!isDead &&
            !isKnockedBack &&
            !hitBlinking &&
            PlayerStillInAttackRange())
        {
            if (sfxSource != null &&
                attackImpactClip != null)
            {
                sfxSource.PlayOneShot(
                    attackImpactClip,
                    attackImpactVolume
                );
            }

            if (playerHealth != null &&
                !playerHealth.IsDead)
            {
                playerHealth.TakeDamage(
                    damageToPlayer
                );
            }
        }

        float remainingCooldown =
            Mathf.Max(
                0f,
                attackCooldown -
                attackImpactDelay
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
            distanceX <=
                attackDistance &&
            distanceY <=
                attackHeight &&
            HasClearLineOfSightToPlayer();
    }

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

            SetPatrolSpriteRight();

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

            SetPatrolSpriteLeft();

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

        yield return new WaitForSeconds(
            pauseBeforeBlink
        );

        if (spriteRenderer != null &&
            blinkSprite != null)
        {
            spriteRenderer.sprite =
                blinkSprite;
        }

        yield return new WaitForSeconds(
            blinkDuration
        );

        UpdatePatrolSprite();

        yield return new WaitForSeconds(
            pauseAfterBlink
        );

        movingRight =
            !movingRight;

        UpdatePatrolSprite();

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

        UpdatePatrolSprite();
    }

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

        foreach (RaycastHit2D hit
                 in hits)
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
                ? bounds.max.x +
                    0.01f
                : bounds.min.x -
                    0.01f;

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

        foreach (RaycastHit2D hit
                 in hits)
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

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        /*
         * Самое важное:
         * определяем сторону игрока
         * ДО отключения всей логики.
         */
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

            hitBlinkCoroutine = null;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );

            knockbackCoroutine = null;
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine = null;
        }

        chasingPlayer = false;
        attackBusy = false;
        hitBlinking = false;
        isKnockedBack = false;

        /*
         * Останавливаем физику.
         */
        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        /*
         * Ставим нужный лежачий спрайт.
         *
         * Игрок слева -> Skeleton 7.
         * Игрок справа -> Skeleton 6.
         */
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled =
                true;

            if (playerIsLeft)
            {
                if (deathFromLeftSprite != null)
                {
                    spriteRenderer.sprite =
                        deathFromLeftSprite;
                }
            }
            else
            {
                if (deathFromRightSprite != null)
                {
                    spriteRenderer.sprite =
                        deathFromRightSprite;
                }
            }
        }

        /*
         * После смерти скелет больше
         * не должен блокировать игрока.
         */
        if (bodyCollider != null)
        {
            bodyCollider.enabled =
                false;
        }

        /*
         * Полностью отключаем физическое
         * движение после установки спрайта.
         */
        if (rb != null)
        {
            rb.simulated =
                false;
        }

        /*
         * Звук смерти пока оставляем,
         * если потом его назначим.
         */
        if (sfxSource != null &&
            deathClip != null)
        {
            sfxSource.PlayOneShot(
                deathClip,
                deathVolume
            );
        }

        /*
         * ВАЖНО:
         * Destroy(gameObject) здесь НЕТ.
         *
         * Разваленный скелет остаётся
         * лежать на уровне.
         */
    }

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

    private void UpdatePatrolSprite()
    {
        if (movingRight)
        {
            SetPatrolSpriteRight();
        }
        else
        {
            SetPatrolSpriteLeft();
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
            SetAttackSpriteLeft();
        }
        else
        {
            movingRight = true;
            SetAttackSpriteRight();
        }
    }

    private void SetPatrolSpriteLeft()
    {
        if (spriteRenderer != null &&
            patrolLeftSprite != null)
        {
            spriteRenderer.sprite =
                patrolLeftSprite;
        }
    }

    private void SetPatrolSpriteRight()
    {
        if (spriteRenderer != null &&
            patrolRightSprite != null)
        {
            spriteRenderer.sprite =
                patrolRightSprite;
        }
    }

    private void SetAttackSpriteLeft()
    {
        if (spriteRenderer != null &&
            attackLeftSprite != null)
        {
            spriteRenderer.sprite =
                attackLeftSprite;
        }
    }

    private void SetAttackSpriteRight()
    {
        if (spriteRenderer != null &&
            attackRightSprite != null)
        {
            spriteRenderer.sprite =
                attackRightSprite;
        }
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

        return
            position.x >= 0f &&
            position.y >= 0f &&
            position.x <= Screen.width &&
            position.y <= Screen.height;
    }
}