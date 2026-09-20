using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSwordAttack : MonoBehaviour
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
    private AudioSource swordAudioSource;

    [SerializeField]
    private WeaponButtonDurabilityUI weaponDurabilityUI;


    // ============================================================
    // ATTACK TIMING
    // ============================================================

    [Header("ATTACK TIMING")]

    [SerializeField, Min(0.01f)]
    private float swingDuration = 0.15f;

    [SerializeField, Min(0.01f)]
    private float strikeDuration = 0.18f;

    [SerializeField, Min(0f)]
    private float attackCooldown = 0.15f;


    // ============================================================
    // DAMAGE
    // ============================================================

    [Header("SWORD DAMAGE")]

    [SerializeField, Min(1)]
    private int swordDamage = 1;


    // ============================================================
    // DURABILITY
    // ============================================================

    [Header("SWORD DURABILITY")]

    [SerializeField]
    private bool consumeDurabilityOnRealHit = true;


    // ============================================================
    // AUDIO
    // ============================================================

    [Header("SWORD AUDIO - SWING")]

    [SerializeField]
    private AudioClip swordSwingClip;

    [SerializeField, Range(0f, 1f)]
    private float swordSwingVolume = 1f;


    [Header("SWORD AUDIO - IMPACT")]

    [SerializeField]
    private AudioClip swordImpactClip;

    [SerializeField, Range(0f, 1f)]
    private float swordImpactVolume = 1f;


    // ============================================================
    // ENEMY HITBOX
    // ============================================================

    [Header("ENEMY STRIKE HITBOX")]

    [FormerlySerializedAs("strikeBoxSize")]
    [SerializeField]
    private Vector2 enemyStrikeBoxSize =
        new Vector2(
            1.15f,
            1.0f
        );

    [FormerlySerializedAs("strikeBoxOffset")]
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

    [SerializeField]
    private Vector2 boxStrikeBoxSize =
        new Vector2(
            0.70f,
            0.80f
        );

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

        if (swordAudioSource == null)
        {
            swordAudioSource =
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
            return;

        if (playerVisual == null)
            return;

        if (!playerVisual.IsSwordEquipped)
            return;

        if (playerVisual.IsKicking ||
            playerVisual.IsCelebrating ||
            playerVisual.IsClimbing ||
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
            playerVisual.SwordFacingRight;

        bool weaponBroke =
            false;


        // --------------------------------------------------------
        // SWING
        // --------------------------------------------------------

        if (attackRight)
        {
            playerVisual.PlaySwordSwingRight();
        }
        else
        {
            playerVisual.PlaySwordSwingLeft();
        }

        if (!playerVisual.IsSwordAttacking)
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

        PlaySwordSwingSound();

        yield return new WaitForSeconds(
            swingDuration
        );


        // --------------------------------------------------------
        // STRIKE
        // --------------------------------------------------------

        if (attackRight)
        {
            playerVisual.PlaySwordStrikeRight();
        }
        else
        {
            playerVisual.PlaySwordStrikeLeft();
        }

        bool somethingWasHit =
            ApplySwordHit(
                attackRight
            );

        if (somethingWasHit)
        {
            PlaySwordImpactSound();

            weaponBroke =
                ConsumeSwordDurability();
        }


        /*
         * КЛЮЧЕВО:
         * даже если это 20-й удар,
         * сначала полностью показываем
         * финальный Strike.
         */
        yield return new WaitForSeconds(
            strikeDuration
        );


        // --------------------------------------------------------
        // END STRIKE VISUAL
        // --------------------------------------------------------

        playerVisual.EndSwordAttackVisual();


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

    private bool ConsumeSwordDurability()
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
            "Sword durability: " +
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

    private bool ApplySwordHit(
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
                continue;


            GuardEnemy guard =
                hit.GetComponentInParent<
                    GuardEnemy
                >();

            if (guard != null &&
                !guard.IsDead &&
                hitGuards.Add(
                    guard))
            {
                guard.ReceiveSwordHit(
                    swordDamage
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
                skeleton.ReceiveSwordHit(
                    swordDamage
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
                continue;


            BreakableBox regularBox =
                hit.GetComponentInParent<
                    BreakableBox
                >();

            if (regularBox != null &&
                !regularBox.IsBroken &&
                hitRegularBoxes.Add(
                    regularBox))
            {
                regularBox.ReceiveHit(
                    swordDamage
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
                middleBox.ReceiveHit(
                    swordDamage
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
                hardBox.ReceiveHit(
                    swordDamage
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

    private void PlaySwordSwingSound()
    {
        if (swordAudioSource == null ||
            swordSwingClip == null)
        {
            return;
        }

        swordAudioSource.PlayOneShot(
            swordSwingClip,
            swordSwingVolume
        );
    }

    private void PlaySwordImpactSound()
    {
        if (swordAudioSource == null ||
            swordImpactClip == null)
        {
            return;
        }

        swordAudioSource.PlayOneShot(
            swordImpactClip,
            swordImpactVolume
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
            playerVisual.IsSwordAttacking)
        {
            playerVisual
                .CancelSwordAttackVisual();
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
            return;

        Debug.Log(
            "[PlayerSwordAttack] " +
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

        swordDamage =
            Mathf.Max(
                1,
                swordDamage
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

        swordSwingVolume =
            Mathf.Clamp01(
                swordSwingVolume
            );

        swordImpactVolume =
            Mathf.Clamp01(
                swordImpactVolume
            );
    }
}