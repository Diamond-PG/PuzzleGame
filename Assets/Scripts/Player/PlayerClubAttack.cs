using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClubAttack : MonoBehaviour
{
    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("REFERENCES")]

    [SerializeField]
    private PlayerVisual playerVisual;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private AudioSource clubAudioSource;

    [SerializeField]
    private WeaponButtonDurabilityUI weaponDurabilityUI;


    // ============================================================
    // ATTACK TIMING
    // ============================================================

    [Header("CLUB ATTACK TIMING")]

    [Tooltip("Сколько показывается кадр замаха дубинки.")]
    [SerializeField, Min(0.01f)]
    private float swingDuration = 0.15f;

    [Tooltip("Сколько показывается кадр самого удара.")]
    [SerializeField, Min(0.01f)]
    private float strikeDuration = 0.18f;

    [Tooltip("Пауза после завершения удара.")]
    [SerializeField, Min(0f)]
    private float attackCooldown = 0.15f;


    // ============================================================
    // DAMAGE
    // ============================================================

    [Header("CLUB DAMAGE")]

    [Tooltip(
        "Урон дубинки по Guard. " +
        "При здоровье Guard = 4 значение 0.8 даёт примерно 5 ударов."
    )]
    [SerializeField, Min(0.01f)]
    private float guardDamage = 0.80f;

    [Tooltip(
        "Урон дубинки по Skeleton. " +
        "При здоровье Skeleton = 3 значение 0.8 даёт 4 удара."
    )]
    [SerializeField, Min(0.01f)]
    private float skeletonDamage = 0.80f;

    [Tooltip(
        "Урон дубинки по обычному ящику. " +
        "При прочности 2 значение 0.8 даёт 3 удара."
    )]
    [SerializeField, Min(0.01f)]
    private float regularBoxDamage = 0.80f;

    [Tooltip(
        "Урон дубинки по среднему ящику. " +
        "При прочности 3 значение 0.8 даёт 4 удара."
    )]
    [SerializeField, Min(0.01f)]
    private float middleBoxDamage = 0.80f;

    [Tooltip(
        "Урон дубинки по тяжёлому ящику. " +
        "При прочности 4 значение 0.8 даёт 5 ударов."
    )]
    [SerializeField, Min(0.01f)]
    private float hardBoxDamage = 0.80f;


    // ============================================================
    // DURABILITY
    // ============================================================

    [Header("CLUB DURABILITY")]

    [Tooltip(
        "Если включено, прочность дубинки тратится " +
        "только при настоящем попадании."
    )]
    [SerializeField]
    private bool consumeDurabilityOnRealHit = true;


    // ============================================================
    // AUDIO
    // ============================================================

    [Header("CLUB AUDIO - SWING")]

    [Tooltip(
        "Звук замаха дубинкой. " +
        "Пока можно оставить None."
    )]
    [SerializeField]
    private AudioClip clubSwingClip;

    [SerializeField, Range(0f, 1f)]
    private float clubSwingVolume = 1f;


    [Header("CLUB AUDIO - IMPACT")]

    [Tooltip(
        "Звук настоящего попадания дубинкой. " +
        "Пока можно оставить None."
    )]
    [SerializeField]
    private AudioClip clubImpactClip;

    [SerializeField, Range(0f, 1f)]
    private float clubImpactVolume = 1f;


    // ============================================================
    // ENEMY HITBOX
    // ============================================================

    [Header("ENEMY STRIKE HITBOX")]

    [Tooltip(
        "Размер зоны попадания дубинки по врагам."
    )]
    [SerializeField]
    private Vector2 enemyStrikeBoxSize =
        new Vector2(
            1.15f,
            1.0f
        );

    [Tooltip(
        "Расстояние зоны удара от центра Player."
    )]
    [SerializeField]
    private Vector2 enemyStrikeBoxOffset =
        new Vector2(
            0.72f,
            0f
        );


    // ============================================================
    // BOX HITBOX
    // ============================================================

    [Header("BOX STRIKE HITBOX")]

    [Tooltip(
        "Размер зоны попадания дубинки по ящикам."
    )]
    [SerializeField]
    private Vector2 boxStrikeBoxSize =
        new Vector2(
            0.70f,
            0.80f
        );

    [Tooltip(
        "Расстояние зоны удара по ящикам."
    )]
    [SerializeField]
    private Vector2 boxStrikeBoxOffset =
        new Vector2(
            0.48f,
            0f
        );


    // ============================================================
    // LAYERS
    // ============================================================

    [Header("HITTABLE LAYERS")]

    [SerializeField]
    private LayerMask hittableLayers = ~0;


    // ============================================================
    // MOVEMENT
    // ============================================================

    [Header("MOVEMENT DURING ATTACK")]

    [Tooltip(
        "Если включено, движение Player блокируется " +
        "на время удара дубинкой."
    )]
    [SerializeField]
    private bool lockPlayerDuringAttack = false;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = false;


    // ============================================================
    // STATE
    // ============================================================

    private bool attackRunning;

    private Coroutine attackRoutine;


    // ============================================================
    // PUBLIC
    // ============================================================

    public bool IsAttacking =>
        attackRunning;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (playerVisual == null)
        {
            playerVisual =
                GetComponent<PlayerVisual>();
        }


        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }


        if (clubAudioSource == null)
        {
            clubAudioSource =
                GetComponent<AudioSource>();
        }


        if (weaponDurabilityUI == null)
        {
            weaponDurabilityUI =
                FindFirstObjectByType<
                    WeaponButtonDurabilityUI
                >();
        }
    }


    // ============================================================
    // BUTTON
    // ============================================================

    public void OnWeaponButtonPressed()
    {
        TryAttack();
    }


    // ============================================================
    // TRY ATTACK
    // ============================================================

    public void TryAttack()
    {
        if (attackRunning)
        {
            return;
        }


        if (playerVisual == null)
        {
            return;
        }


        /*
         * Очень важно:
         *
         * Этот скрипт реагирует ТОЛЬКО,
         * если сейчас реально экипирована Club.
         *
         * Поэтому PlayerSwordAttack и
         * PlayerClubAttack могут оба висеть
         * на WeaponButton.
         */
        if (!playerVisual.IsClubEquipped)
        {
            return;
        }


        if (playerVisual.IsKicking ||
            playerVisual.IsCelebrating ||
            playerVisual.IsClimbing ||
            playerVisual.IsClubAttacking ||
            playerVisual.IsSwordAttacking)
        {
            return;
        }


        attackRoutine =
            StartCoroutine(
                AttackRoutine()
            );
    }


    // ============================================================
    // ATTACK ROUTINE
    // ============================================================

    private IEnumerator AttackRoutine()
    {
        attackRunning =
            true;


        bool attackRight =
            playerVisual.ClubFacingRight;


        bool weaponBroke =
            false;


        // --------------------------------------------------------
        // SWING
        // --------------------------------------------------------

        if (attackRight)
        {
            playerVisual.PlayClubSwingRight();
        }
        else
        {
            playerVisual.PlayClubSwingLeft();
        }


        /*
         * Например, если нужный Club Swing Sprite
         * не назначен, атаку дальше не продолжаем.
         */
        if (!playerVisual.IsClubAttacking)
        {
            attackRunning =
                false;

            attackRoutine =
                null;

            yield break;
        }


        if (lockPlayerDuringAttack &&
            playerController != null)
        {
            playerController.SetActionLock(
                true
            );
        }


        PlayClubSwingSound();


        yield return new WaitForSeconds(
            swingDuration
        );


        // --------------------------------------------------------
        // STRIKE
        // --------------------------------------------------------

        if (attackRight)
        {
            playerVisual.PlayClubStrikeRight();
        }
        else
        {
            playerVisual.PlayClubStrikeLeft();
        }


        bool somethingWasHit =
            ApplyClubHit(
                attackRight
            );


        if (somethingWasHit)
        {
            PlayClubImpactSound();


            weaponBroke =
                ConsumeClubDurability();
        }


        /*
         * Если дубинка сломалась именно этим ударом,
         * Strike-спрайт всё равно сначала полностью
         * показывается.
         */
        yield return new WaitForSeconds(
            strikeDuration
        );


        // --------------------------------------------------------
        // END STRIKE VISUAL
        // --------------------------------------------------------

        playerVisual.EndClubAttackVisual();


        // --------------------------------------------------------
        // BREAK WEAPON AFTER FINAL STRIKE
        // --------------------------------------------------------

        if (weaponBroke &&
            weaponDurabilityUI != null)
        {
            weaponDurabilityUI
                .BreakWeaponNow();
        }


        if (lockPlayerDuringAttack &&
            playerController != null)
        {
            playerController.SetActionLock(
                false
            );
        }


        // --------------------------------------------------------
        // COOLDOWN
        // --------------------------------------------------------

        if (attackCooldown > 0f)
        {
            yield return new WaitForSeconds(
                attackCooldown
            );
        }


        attackRunning =
            false;


        attackRoutine =
            null;
    }


    // ============================================================
    // DURABILITY
    // ============================================================

    private bool ConsumeClubDurability()
    {
        if (!consumeDurabilityOnRealHit)
        {
            return false;
        }


        if (weaponDurabilityUI == null)
        {
            weaponDurabilityUI =
                FindFirstObjectByType<
                    WeaponButtonDurabilityUI
                >();
        }


        if (weaponDurabilityUI == null)
        {
            return false;
        }


        float before =
            weaponDurabilityUI
                .GetWeaponDurability01();


        bool broke =
            weaponDurabilityUI
                .ConsumeWeaponDurabilityHit();


        float after =
            weaponDurabilityUI
                .GetWeaponDurability01();


        DebugMessage(
            "Club durability: " +
            Mathf.RoundToInt(
                before * 100f
            ) +
            "% -> " +
            Mathf.RoundToInt(
                after * 100f
            ) +
            "%"
        );


        return broke;
    }


    // ============================================================
    // APPLY HIT
    // ============================================================

    private bool ApplyClubHit(
        bool attackRight
    )
    {
        bool somethingWasHit =
            false;


        if (ApplyEnemyHits(
                attackRight))
        {
            somethingWasHit =
                true;
        }


        if (ApplyBoxHits(
                attackRight))
        {
            somethingWasHit =
                true;
        }


        return somethingWasHit;
    }


    // ============================================================
    // ENEMIES
    // ============================================================

    private bool ApplyEnemyHits(
        bool attackRight
    )
    {
        Vector2 center =
            CalculateHitboxCenter(
                attackRight,
                enemyStrikeBoxOffset
            );


        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                center,
                enemyStrikeBoxSize,
                0f,
                hittableLayers
            );


        if (hits == null ||
            hits.Length == 0)
        {
            return false;
        }


        HashSet<GuardEnemy> hitGuards =
            new HashSet<GuardEnemy>();


        HashSet<SkeletonEnemy> hitSkeletons =
            new HashSet<SkeletonEnemy>();


        bool enemyHit =
            false;


        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                continue;
            }


            GuardEnemy guard =
                hit.GetComponentInParent<
                    GuardEnemy
                >();


            if (guard != null &&
                !guard.IsDead &&
                hitGuards.Add(
                    guard))
            {
                /*
                 * Универсальный урон оружия.
                 *
                 * После обновления GuardEnemy
                 * этим же методом смогут пользоваться
                 * дубинка, топор, копьё и т.д.
                 */
                guard.ReceiveWeaponHit(
                    guardDamage
                );


                enemyHit =
                    true;
            }


            SkeletonEnemy skeleton =
                hit.GetComponentInParent<
                    SkeletonEnemy
                >();


            if (skeleton != null &&
                !skeleton.IsDead &&
                hitSkeletons.Add(
                    skeleton))
            {
                skeleton.ReceiveWeaponHit(
                    skeletonDamage
                );


                enemyHit =
                    true;
            }
        }


        return enemyHit;
    }


    // ============================================================
    // BOXES
    // ============================================================

    private bool ApplyBoxHits(
        bool attackRight
    )
    {
        Vector2 center =
            CalculateHitboxCenter(
                attackRight,
                boxStrikeBoxOffset
            );


        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                center,
                boxStrikeBoxSize,
                0f,
                hittableLayers
            );


        if (hits == null ||
            hits.Length == 0)
        {
            return false;
        }


        HashSet<BreakableBox> hitRegularBoxes =
            new HashSet<BreakableBox>();


        HashSet<BreakableMiddleBox> hitMiddleBoxes =
            new HashSet<BreakableMiddleBox>();


        HashSet<BreakableHardBox> hitHardBoxes =
            new HashSet<BreakableHardBox>();


        bool boxHit =
            false;


        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                continue;
            }


            BreakableBox regularBox =
                hit.GetComponentInParent<
                    BreakableBox
                >();


            if (regularBox != null &&
                !regularBox.IsBroken &&
                hitRegularBoxes.Add(
                    regularBox))
            {
                regularBox.ReceiveWeaponHit(
                    regularBoxDamage
                );


                boxHit =
                    true;
            }


            BreakableMiddleBox middleBox =
                hit.GetComponentInParent<
                    BreakableMiddleBox
                >();


            if (middleBox != null &&
                !middleBox.IsBroken &&
                hitMiddleBoxes.Add(
                    middleBox))
            {
                middleBox.ReceiveWeaponHit(
                    middleBoxDamage
                );


                boxHit =
                    true;
            }


            BreakableHardBox hardBox =
                hit.GetComponentInParent<
                    BreakableHardBox
                >();


            if (hardBox != null &&
                !hardBox.IsBroken &&
                hitHardBoxes.Add(
                    hardBox))
            {
                hardBox.ReceiveWeaponHit(
                    hardBoxDamage
                );


                boxHit =
                    true;
            }
        }


        return boxHit;
    }


    // ============================================================
    // HITBOX CENTER
    // ============================================================

    private Vector2 CalculateHitboxCenter(
        bool attackRight,
        Vector2 hitboxOffset
    )
    {
        float direction =
            attackRight
                ? 1f
                : -1f;


        Vector2 directionalOffset =
            new Vector2(
                Mathf.Abs(
                    hitboxOffset.x
                ) * direction,
                hitboxOffset.y
            );


        return
            (Vector2)transform.position +
            directionalOffset;
    }


    // ============================================================
    // AUDIO
    // ============================================================

    private void PlayClubSwingSound()
    {
        if (clubAudioSource == null ||
            clubSwingClip == null)
        {
            return;
        }


        clubAudioSource.PlayOneShot(
            clubSwingClip,
            clubSwingVolume
        );
    }


    private void PlayClubImpactSound()
    {
        if (clubAudioSource == null ||
            clubImpactClip == null)
        {
            return;
        }


        clubAudioSource.PlayOneShot(
            clubImpactClip,
            clubImpactVolume
        );
    }


    // ============================================================
    // CANCEL
    // ============================================================

    public void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(
                attackRoutine
            );


            attackRoutine =
                null;
        }


        attackRunning =
            false;


        if (playerVisual != null &&
            playerVisual.IsClubAttacking)
        {
            playerVisual
                .CancelClubAttackVisual();
        }


        if (lockPlayerDuringAttack &&
            playerController != null)
        {
            playerController.SetActionLock(
                false
            );
        }
    }


    // ============================================================
    // DISABLE
    // ============================================================

    private void OnDisable()
    {
        CancelAttack();
    }


    // ============================================================
    // DEBUG
    // ============================================================

    private void DebugMessage(
        string message
    )
    {
        if (!debugLogs)
        {
            return;
        }


        Debug.Log(
            "[PlayerClubAttack] " +
            message,
            this
        );
    }


    // ============================================================
    // GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        DrawHitboxGizmos(
            enemyStrikeBoxSize,
            enemyStrikeBoxOffset
        );


        DrawHitboxGizmos(
            boxStrikeBoxSize,
            boxStrikeBoxOffset
        );
    }


    private void DrawHitboxGizmos(
        Vector2 hitboxSize,
        Vector2 hitboxOffset
    )
    {
        Vector3 basePosition =
            transform.position;


        float distanceX =
            Mathf.Abs(
                hitboxOffset.x
            );


        Vector3 rightCenter =
            basePosition +
            new Vector3(
                distanceX,
                hitboxOffset.y,
                0f
            );


        Vector3 leftCenter =
            basePosition +
            new Vector3(
                -distanceX,
                hitboxOffset.y,
                0f
            );


        Vector3 size =
            new Vector3(
                hitboxSize.x,
                hitboxSize.y,
                0f
            );


        Gizmos.DrawWireCube(
            rightCenter,
            size
        );


        Gizmos.DrawWireCube(
            leftCenter,
            size
        );
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        swingDuration =
            Mathf.Max(
                0.01f,
                swingDuration
            );


        strikeDuration =
            Mathf.Max(
                0.01f,
                strikeDuration
            );


        attackCooldown =
            Mathf.Max(
                0f,
                attackCooldown
            );


        guardDamage =
            Mathf.Max(
                0.01f,
                guardDamage
            );


        skeletonDamage =
            Mathf.Max(
                0.01f,
                skeletonDamage
            );


        regularBoxDamage =
            Mathf.Max(
                0.01f,
                regularBoxDamage
            );


        middleBoxDamage =
            Mathf.Max(
                0.01f,
                middleBoxDamage
            );


        hardBoxDamage =
            Mathf.Max(
                0.01f,
                hardBoxDamage
            );


        enemyStrikeBoxSize.x =
            Mathf.Max(
                0.01f,
                enemyStrikeBoxSize.x
            );


        enemyStrikeBoxSize.y =
            Mathf.Max(
                0.01f,
                enemyStrikeBoxSize.y
            );


        boxStrikeBoxSize.x =
            Mathf.Max(
                0.01f,
                boxStrikeBoxSize.x
            );


        boxStrikeBoxSize.y =
            Mathf.Max(
                0.01f,
                boxStrikeBoxSize.y
            );


        clubSwingVolume =
            Mathf.Clamp01(
                clubSwingVolume
            );


        clubImpactVolume =
            Mathf.Clamp01(
                clubImpactVolume
            );
    }
}