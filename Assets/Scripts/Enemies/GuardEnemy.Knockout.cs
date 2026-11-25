using System;
using System.Collections;
using UnityEngine;

public partial class GuardEnemy
{
    [Serializable]
    private sealed class UnarmedSprites
    {
        public Sprite idle;
        public Sprite idleBlink;
        public Sprite angry;
        public Sprite angryBlink;
        public Sprite lookLeft;
        public Sprite lookRight;
        public Sprite walkRight;
        public Sprite walkLeft;
        public Sprite runRight;
        public Sprite runLeft;
        public Sprite attackRight;
        public Sprite attackLeft;
    }

    [Serializable]
    private sealed class WakeUpAnimation
    {
        [Min(0f)] public float idle = 0.6f;
        [Min(0f)] public float idleBlink = 0.15f;
        [Min(0f)] public float idleAfterBlink = 0.4f;
        [Min(0f)] public float angry = 0.7f;
        [Min(0f)] public float angryBlink = 0.15f;
        [Min(0f)] public float angryAfterBlink = 0.4f;
        [Min(0f)] public float lookLeft = 0.6f;
        [Min(0f)] public float lookRight = 0.6f;
        [Min(0f)] public float patrolPauseBeforeBlink = 1f;
        [Min(0f)] public float patrolBlinkDuration = 0.18f;
        [Min(0f)] public float patrolPauseAfterBlink = 0.3f;
    }

    [Serializable]
    private sealed class UnarmedCombat
    {
        [Min(0f)] public float walkSpeed = 1.15f;
        [Min(0f)] public float runSpeed = 1.65f;
        [Min(0f)] public float detectionRange = 2.5f;
        [Min(0f)] public float attackRange = 0.62f;
        [Min(1)] public int attackDamage = 1;
        [Min(0f)] public float attackCooldown = 1.65f;
        [Min(0.01f)] public float attackAnimationDuration = 0.35f;
        [Min(0f)] public float attackHitDelay = 0.18f;
    }

    [Serializable]
    private struct SpriteAlignment
    {
        public Sprite sprite;
        [Min(0.01f)] public float scale;
        [Tooltip("Offset in the Visual transform's parent space.")]
        public Vector2 offset;
    }

    [Header("Knockout Settings")]
    [SerializeField] private bool enableKnockout = true;
    [SerializeField] private bool enableWakeUp;
    [SerializeField, Min(0f)] private float knockoutDuration = 30f;
    [SerializeField, Range(1f, 100f)] private float healthRestoreOnWakeUp = 100f;

    [Header("Unarmed Sprites")]
    [SerializeField] private UnarmedSprites unarmedSprites = new();

    [Header("Wake Up Animation")]
    [SerializeField] private WakeUpAnimation wakeUpAnimation = new();

    [Header("Unarmed Combat")]
    [SerializeField] private UnarmedCombat unarmedCombat = new();

    [Header("Knockout Stars")]
    [SerializeField] private GuardKnockoutStars knockoutStars = new();
    [Tooltip("Mirrors Offset X when the downed sprite has its head on the opposite side.")]
    [SerializeField] private bool mirrorStarsWhenPlayerLeft;

    [Header("Visual Alignment")]
    [SerializeField, Min(0.01f)] private float unarmedVisualScale = 1f;
    [SerializeField] private Vector2 unarmedVisualOffset;
    [SerializeField] private SpriteAlignment[] unarmedSpriteAlignment = Array.Empty<SpriteAlignment>();

    private bool isKnockedOut;
    private bool isWakingUp;
    private bool isUnarmed;
    private bool playedDisarmReaction;
    private bool weaponDropInProgress;
    private float wakeAt;
    private Transform alignedVisual;
    private Vector3 visualBasePosition;
    private Vector3 visualBaseScale;
    private Sprite currentUnarmedSprite;
    private Transform weaponParent;
    private Vector3 weaponLocalPosition;
    private Quaternion weaponLocalRotation;
    private Vector3 weaponLocalScale;
    private SwordPickup swordPickup;
    private ClubPickup clubPickup;

    public bool IsKnockedOut => isKnockedOut;
    public bool IsUnarmed => isUnarmed;
    public bool CanBeTargeted => !isDead && !isKnockedOut && !isWakingUp;

    private float ActivePatrolSpeed => isUnarmed ? unarmedCombat.walkSpeed : patrolSpeed;
    private float ActiveChaseSpeed => isUnarmed ? unarmedCombat.runSpeed : chaseSpeed;
    private float ActiveDetectionRange => isUnarmed ? unarmedCombat.detectionRange : detectionDistance;
    private float ActiveAttackRange => isUnarmed ? unarmedCombat.attackRange : attackDistance;

    private void CaptureVisualAndWeapon()
    {
        if (spriteRenderer != null && spriteRenderer.transform != transform &&
            spriteRenderer.transform.IsChildOf(transform))
        {
            alignedVisual = spriteRenderer.transform;
            visualBasePosition = alignedVisual.localPosition;
            visualBaseScale = alignedVisual.localScale;
        }

        if (weaponObject == null)
            return;

        Transform weapon = weaponObject.transform;
        weaponParent = weapon.parent;
        weaponLocalPosition = weapon.localPosition;
        weaponLocalRotation = weapon.localRotation;
        weaponLocalScale = weapon.localScale;
        swordPickup = weaponObject.GetComponent<SwordPickup>();
        clubPickup = weaponObject.GetComponent<ClubPickup>();
        weaponObject.SetActive(false);
    }

    private void Update()
    {
        if (isKnockedOut && enableWakeUp && Time.timeScale > 0f &&
            Time.time >= wakeAt && !weaponDropInProgress)
        {
            BeginWakeUp();
        }
    }

    private void LateUpdate()
    {
        if (isKnockedOut && Time.timeScale > 0f)
            knockoutStars.Advance(Time.deltaTime);
    }

    private void BeginWakeUp()
    {
        isKnockedOut = false;
        knockoutStars.Hide();
        isUnarmed = !RecoverDroppedWeapon();
        currentHealth = maxHealth * Mathf.Clamp(healthRestoreOnWakeUp, 1f, 100f) / 100f;
        healthBar.ResetHidden(currentHealth / maxHealth);

        movingRight = transform.position.x < startX;
        detectedPlayerNaturally = false;
        detectionVoicePlayed = false;
        chaseVoicePlayed = false;

        if (isUnarmed && !playedDisarmReaction)
        {
            isWakingUp = true;
            StartCoroutine(DisarmReaction());
        }
        else
        {
            FinishWakeUp();
        }
    }

    private bool RecoverDroppedWeapon()
    {
        if (isUnarmed || !weaponDropped || weaponObject == null || !weaponObject.activeInHierarchy ||
            (swordPickup != null && swordPickup.IsPickupInProgress) ||
            (clubPickup != null && clubPickup.IsPickupInProgress))
        {
            return false;
        }

        weaponObject.SetActive(false);
        Transform weapon = weaponObject.transform;
        weapon.SetParent(weaponParent, false);
        weapon.localPosition = weaponLocalPosition;
        weapon.localRotation = weaponLocalRotation;
        weapon.localScale = weaponLocalScale;
        weaponDropped = false;
        return true;
    }

    private IEnumerator DisarmReaction()
    {
        yield return ShowWakeUpPose(unarmedSprites.idle, wakeUpAnimation.idle);
        yield return ShowWakeUpPose(unarmedSprites.idleBlink, wakeUpAnimation.idleBlink);
        yield return ShowWakeUpPose(unarmedSprites.idle, wakeUpAnimation.idleAfterBlink);
        yield return ShowWakeUpPose(unarmedSprites.angry, wakeUpAnimation.angry);
        yield return ShowWakeUpPose(unarmedSprites.angryBlink, wakeUpAnimation.angryBlink);
        yield return ShowWakeUpPose(unarmedSprites.angry, wakeUpAnimation.angryAfterBlink);
        yield return ShowWakeUpPose(unarmedSprites.lookLeft, wakeUpAnimation.lookLeft);
        yield return ShowWakeUpPose(unarmedSprites.lookRight, wakeUpAnimation.lookRight);
        playedDisarmReaction = true;
        FinishWakeUp();
    }

    private IEnumerator ShowWakeUpPose(Sprite sprite, float duration)
    {
        SetGuardSprite(idleFrontSprite, sprite);
        if (duration > 0f)
            yield return new WaitForSeconds(duration);
    }

    private void FinishWakeUp()
    {
        isWakingUp = false;
        rb.simulated = true;
        bodyCollider.enabled = true;
        SetIdleFrontSprite();
    }

    private void SetGuardSprite(Sprite armedSprite, Sprite unarmedSprite = null)
    {
        if (spriteRenderer == null)
            return;

        if (!isUnarmed || unarmedSprite == null)
        {
            if (armedSprite == null)
                return;

            if (currentUnarmedSprite != null && alignedVisual != null)
            {
                alignedVisual.localPosition = visualBasePosition;
                alignedVisual.localScale = visualBaseScale;
            }

            currentUnarmedSprite = null;
            spriteRenderer.sprite = armedSprite;
            return;
        }

        if (currentUnarmedSprite == unarmedSprite)
            return;

        if (alignedVisual != null)
        {
            float scale = unarmedVisualScale;
            Vector2 offset = unarmedVisualOffset;
            foreach (SpriteAlignment alignment in unarmedSpriteAlignment)
            {
                if (alignment.sprite != unarmedSprite)
                    continue;

                scale *= alignment.scale;
                offset += alignment.offset;
                break;
            }

            alignedVisual.localPosition = visualBasePosition + (Vector3)offset;
            alignedVisual.localScale = Vector3.Scale(visualBaseScale, new Vector3(scale, scale, 1f));
        }
        spriteRenderer.sprite = unarmedSprite;
        currentUnarmedSprite = unarmedSprite;
    }
}
