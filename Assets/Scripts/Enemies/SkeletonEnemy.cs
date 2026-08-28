using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SkeletonEnemy : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("VISUAL")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("SPRITES")]
    [SerializeField] private Sprite patrolLeftSprite;
    [SerializeField] private Sprite patrolRightSprite;
    [SerializeField] private Sprite attackLeftSprite;
    [SerializeField] private Sprite attackRightSprite;
    [SerializeField] private Sprite blinkSprite;

    [Header("DEATH SPRITES")]
    [Tooltip("Skeleton 6. Используется, когда игрок находится справа от скелета.")]
    [SerializeField] private Sprite deathFromRightSprite;

    [Tooltip("Skeleton 7. Используется, когда игрок находится слева от скелета.")]
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
    [SerializeField, Min(1)] private int maxHealth = 3;

    [Header("HIT BLINK")]
    [SerializeField, Min(1)] private int hitBlinkCount = 3;
    [SerializeField] private float hitBlinkInterval = 0.12f;

    [Header("HIT KNOCKBACK")]
    [SerializeField] private float knockbackForce = 2.2f;
    [SerializeField] private float knockbackUpForce = 3.2f;
    [SerializeField] private float knockbackDuration = 0.18f;

    [Header("SKELETON ATTACK")]
    [SerializeField, Min(1)] private int damageToPlayer = 1;
    [SerializeField] private float attackDistance = 0.8f;
    [SerializeField] private float attackHeight = 1.2f;
    [SerializeField] private float attackImpactDelay = 0.15f;
    [SerializeField] private float attackCooldown = 1.1f;

    [Header("HAPTICS")]
    [SerializeField] private bool useHaptics = true;

    [SerializeField, Range(5, 100)]
    private int playerHitsSkeletonHapticMs = 18;

    [SerializeField, Range(5, 150)]
    private int skeletonHitsPlayerHapticMs = 35;

    [Header("AUDIO")]
    [SerializeField] private AudioSource sfxSource;

    [Header("AUDIO - DETECT")]
    [SerializeField] private AudioClip detectClip;

    [Range(0f, 1f)]
    [SerializeField] private float detectVolume = 1f;

    [Header("AUDIO - HURT")]
    [SerializeField] private AudioClip skeletonHurtClip;

    [Range(0f, 1f)]
    [SerializeField] private float skeletonHurtVolume = 1f;

    [Header("AUDIO - SWING")]
    [SerializeField] private AudioClip swingClip;

    [Range(0f, 1f)]
    [SerializeField] private float swingVolume = 1f;

    [Header("AUDIO - DEATH")]
    [SerializeField] private AudioClip deathClip;

    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 1f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;

    private float startX;
    private float leftPatrolX;
    private float rightPatrolX;

    private int currentHealth;

    private bool movingRight;
    private bool chasingPlayer;
    private bool patrolPaused;
    private bool attackBusy;
    private bool hitBlinking;
    private bool isKnockedBack;
    private bool isDead;

    private Coroutine patrolPauseCoroutine;
    private Coroutine hitBlinkCoroutine;
    private Coroutine knockbackCoroutine;
    private Coroutine attackCoroutine;

    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

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
                GameObject.FindGameObjectWithTag("Player");

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

    public void ReceiveKick(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        if (hitBlinking ||
            isKnockedBack)
        {
            return;
        }

        ReceivePlayerHit(damage);
    }

    private void ReceivePlayerHit(int damage)
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

        PlayPlayerHitsSkeletonHaptic();

        if (sfxSource != null &&
            skeletonHurtClip != null)
        {
            sfxSource.PlayOneShot(
                skeletonHurtClip,
                skeletonHurtVolume
            );
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        AggroAndFacePlayerAfterHit();

        CancelPatrolPause();

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            attackBusy = false;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine =
            StartCoroutine(
                KnockbackRoutine()
            );

        if (hitBlinkCoroutine != null)
        {
            StopCoroutine(hitBlinkCoroutine);
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
            SetAttackSpriteLeft();
        }
        else
        {
            movingRight = true;
            SetAttackSpriteRight();
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

                UpdateAttackSprite();
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
            Mathf.Sign(differenceX);

        rb.linearVelocity =
            new Vector2(
                direction * chaseSpeed,
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

        if (playerHealth != null &&
            playerHealth.IsDead)
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
        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            attackBusy = false;
            attackCoroutine = null;
            yield break;
        }

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
            attackBusy = false;
            attackCoroutine = null;
            StopHorizontalMovement();
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

                PlaySkeletonHitsPlayerHaptic();
            }
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            attackBusy = false;
            attackCoroutine = null;
            chasingPlayer = false;

            StopHorizontalMovement();

            yield break;
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

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            attackBusy = false;
            attackCoroutine = null;
            chasingPlayer = false;

            StopHorizontalMovement();

            yield break;
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
            distanceX <=
                attackDistance &&
            distanceY <=
                attackHeight &&
            HasClearLineOfSightToPlayer();
    }

    private void StopAllCombatAfterPlayerDeath()
    {
        chasingPlayer = false;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackBusy = false;

        StopHorizontalMovement();

        if (!hitBlinking &&
            !isKnockedBack)
        {
            UpdatePatrolSprite();
        }
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

        movingRight = !movingRight;

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
            StopCoroutine(attackCoroutine);
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

        if (playerHealth != null &&
            playerHealth.IsDead)
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

    private void Die()
    {
        if (isDead)
            return;

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

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

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

        if (sfxSource != null &&
            deathClip != null)
        {
            sfxSource.PlayOneShot(
                deathClip,
                deathVolume
            );
        }
    }

    private void PlayPlayerHitsSkeletonHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            playerHitsSkeletonHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }

    private void PlaySkeletonHitsPlayerHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            skeletonHitsPlayerHapticMs,
            MicroHaptics.IOSHapticStyle.Heavy
        );
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
}