using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerVisual : MonoBehaviour
{
    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("REFERENCES")]

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private PlayerJump playerJump;

    [SerializeField]
    private PrisonBreakDoor prisonBreakDoor;


    // ============================================================
    // NORMAL IDLE
    // ============================================================

    [Header("NORMAL IDLE")]

    [FormerlySerializedAs("idleSprite")]
    [SerializeField]
    private Sprite idleSprite;

    [FormerlySerializedAs("blinkSprite")]
    [SerializeField]
    private Sprite idleBlinkSprite;


    // ============================================================
    // WALK LEFT
    // ============================================================

    [Header("WALK LEFT")]

    [FormerlySerializedAs("lookLeftSprite")]
    [SerializeField]
    private Sprite walkLeftSprite;

    [SerializeField]
    private Sprite walkLeftBlinkSprite;


    // ============================================================
    // WALK RIGHT
    // ============================================================

    [Header("WALK RIGHT")]

    [FormerlySerializedAs("lookRightSprite")]
    [SerializeField]
    private Sprite walkRightSprite;

    [SerializeField]
    private Sprite walkRightBlinkSprite;


    // ============================================================
    // JUMP / FALL
    // ============================================================

    [Header("JUMP / FALL")]

    [FormerlySerializedAs("lookUpSprite")]
    [SerializeField]
    private Sprite jumpSprite;

    [FormerlySerializedAs("lookDownSprite")]
    [SerializeField]
    private Sprite fallSprite;

    [SerializeField, Min(0f)]
    private float fallVisualDelay = 0.08f;

    [SerializeField]
    private float fallVelocityThreshold = -0.10f;


    // ============================================================
    // HURT
    // ============================================================

    [Header("HURT")]

    [FormerlySerializedAs("hurtSprite")]
    [SerializeField]
    private Sprite hurtSprite;

    [SerializeField]
    private float hurtDuration = 0.45f;


    // ============================================================
    // KICK
    // ============================================================

    [Header("KICK SPRITES")]

    [SerializeField]
    private Sprite kickRightSprite;

    [SerializeField]
    private Sprite kickLeftSprite;


    // ============================================================
    // SWORD - JUMP / FALL
    // ============================================================

    [Header("SWORD - JUMP / FALL")]

    [Tooltip("Игрок прыгает с экипированным мечом.")]
    [SerializeField]
    private Sprite swordJumpSprite;

    [Tooltip("Игрок падает с экипированным мечом.")]
    [SerializeField]
    private Sprite swordFallSprite;


    // ============================================================
    // SWORD - HURT
    // ============================================================

    [Header("SWORD - HURT")]

    [SerializeField]
    private Sprite swordHurtLeftSprite;

    [SerializeField]
    private Sprite swordHurtRightSprite;


    // ============================================================
    // SWORD - KICK
    // ============================================================

    [Header("SWORD - KICK")]

    [SerializeField]
    private Sprite swordKickRightSprite;

    [SerializeField]
    private Sprite swordKickLeftSprite;


    // ============================================================
    // SWORD - IDLE LEFT
    // ============================================================

    [Header("SWORD - IDLE LEFT")]

    [FormerlySerializedAs("swordIdleSprite")]
    [SerializeField]
    private Sprite swordIdleLeftSprite;

    [SerializeField]
    private Sprite swordIdleLeftBlinkSprite;


    // ============================================================
    // SWORD - IDLE RIGHT
    // ============================================================

    [Header("SWORD - IDLE RIGHT")]

    [SerializeField]
    private Sprite swordIdleRightSprite;

    [SerializeField]
    private Sprite swordIdleRightBlinkSprite;


    // ============================================================
    // SWORD - WALK LEFT
    // ============================================================

    [Header("SWORD - WALK LEFT")]

    [SerializeField]
    private Sprite swordWalkLeftSprite;

    [SerializeField]
    private Sprite swordWalkLeftBlinkSprite;


    // ============================================================
    // SWORD - WALK RIGHT
    // ============================================================

    [Header("SWORD - WALK RIGHT")]

    [SerializeField]
    private Sprite swordWalkRightSprite;

    [SerializeField]
    private Sprite swordWalkRightBlinkSprite;


    // ============================================================
    // SWORD - ATTACK
    // ============================================================

    [Header("SWORD - ATTACK")]

    [SerializeField]
    private Sprite swordSwingLeftSprite;

    [SerializeField]
    private Sprite swordSwingRightSprite;

    [SerializeField]
    private Sprite swordStrikeLeftSprite;

    [SerializeField]
    private Sprite swordStrikeRightSprite;


    // ============================================================
    // CLUB - JUMP / FALL
    // ============================================================

    [Header("CLUB - JUMP / FALL")]

    [Tooltip(
        "Игрок прыгает с дубинкой. " +
        "Пока спрайта нет, можно оставить None."
    )]
    [SerializeField]
    private Sprite clubJumpSprite;

    [Tooltip(
        "Игрок падает с дубинкой. " +
        "Пока спрайта нет, можно оставить None."
    )]
    [SerializeField]
    private Sprite clubFallSprite;


    // ============================================================
    // CLUB - HURT
    // ============================================================

    [Header("CLUB - HURT")]

    [SerializeField]
    private Sprite clubHurtLeftSprite;

    [SerializeField]
    private Sprite clubHurtRightSprite;


    // ============================================================
    // CLUB - KICK
    // ============================================================

    [Header("CLUB - KICK")]

    [SerializeField]
    private Sprite clubKickRightSprite;

    [SerializeField]
    private Sprite clubKickLeftSprite;


    // ============================================================
    // CLUB - IDLE LEFT
    // ============================================================

    [Header("CLUB - IDLE LEFT")]

    [Tooltip(
        "Стоит с дубинкой слева. " +
        "Сюда ставим один из двух готовых спрайтов."
    )]
    [SerializeField]
    private Sprite clubIdleLeftSprite;

    [Tooltip(
        "Моргание стоя с дубинкой слева. " +
        "Пока можно оставить None."
    )]
    [SerializeField]
    private Sprite clubIdleLeftBlinkSprite;


    // ============================================================
    // CLUB - IDLE RIGHT
    // ============================================================

    [Header("CLUB - IDLE RIGHT")]

    [Tooltip(
        "Стоит с дубинкой справа. " +
        "Сюда ставим второй готовый спрайт."
    )]
    [SerializeField]
    private Sprite clubIdleRightSprite;

    [Tooltip(
        "Моргание стоя с дубинкой справа. " +
        "Пока можно оставить None."
    )]
    [SerializeField]
    private Sprite clubIdleRightBlinkSprite;


    // ============================================================
    // CLUB - WALK LEFT
    // ============================================================

    [Header("CLUB - WALK LEFT")]

    [SerializeField]
    private Sprite clubWalkLeftSprite;

    [SerializeField]
    private Sprite clubWalkLeftBlinkSprite;


    // ============================================================
    // CLUB - WALK RIGHT
    // ============================================================

    [Header("CLUB - WALK RIGHT")]

    [SerializeField]
    private Sprite clubWalkRightSprite;

    [SerializeField]
    private Sprite clubWalkRightBlinkSprite;


    // ============================================================
    // CLUB - ATTACK
    // ============================================================

    [Header("CLUB - ATTACK")]

    [Tooltip(
        "Первый кадр замаха дубинки влево."
    )]
    [SerializeField]
    private Sprite clubSwingLeftSprite;

    [Tooltip(
        "Первый кадр замаха дубинки вправо."
    )]
    [SerializeField]
    private Sprite clubSwingRightSprite;

    [Tooltip(
        "Кадр попадания / удара дубинкой влево."
    )]
    [SerializeField]
    private Sprite clubStrikeLeftSprite;

    [Tooltip(
        "Кадр попадания / удара дубинкой вправо."
    )]
    [SerializeField]
    private Sprite clubStrikeRightSprite;


    // ============================================================
    // PRISON
    // ============================================================

    [Header("PRISON BEFORE DOOR BREAK")]

    [SerializeField]
    private Sprite prisonSadSprite;

    [SerializeField]
    private Sprite prisonSadBlinkSprite;


    // ============================================================
    // DOOR BREAK
    // ============================================================

    [Header("DOOR BREAK REACTION")]

    [SerializeField]
    private Sprite doorBreakReactionSprite;

    [SerializeField, Min(0f)]
    private float doorBreakReactionDuration = 0.85f;


    // ============================================================
    // CLIMB UP
    // ============================================================

    [Header("CLIMB UP")]

    [SerializeField]
    private Sprite climbUpLeftSprite;

    [SerializeField]
    private Sprite climbUpRightSprite;

    [SerializeField]
    private Sprite climbUpLeftBlinkSprite;

    [SerializeField]
    private Sprite climbUpRightBlinkSprite;


    // ============================================================
    // CLIMB DOWN
    // ============================================================

    [Header("CLIMB DOWN")]

    [SerializeField]
    private Sprite climbDownLeftSprite;

    [SerializeField]
    private Sprite climbDownRightSprite;

    [SerializeField]
    private Sprite climbDownLeftBlinkSprite;

    [SerializeField]
    private Sprite climbDownRightBlinkSprite;


    // ============================================================
    // MOVEMENT
    // ============================================================

    [Header("MOVEMENT SETTINGS")]

    [SerializeField]
    private float movementThreshold = 0.05f;


    // ============================================================
    // BLINK
    // ============================================================

    [Header("BLINK SETTINGS")]

    [SerializeField]
    private float blinkInterval = 2.2f;

    [SerializeField]
    private float blinkDuration = 0.1f;

    [SerializeField]
    private Vector2 blinkRandomDelay =
        new Vector2(
            0.1f,
            0.5f
        );


    // ============================================================
    // CLIMB EXIT
    // ============================================================

    [Header("CLIMB SETTINGS")]

    [SerializeField, Min(0f)]
    private float climbExitVisualGrace = 0.10f;


    // ============================================================
    // STATE
    // ============================================================

    private enum VisualState
    {
        Idle,
        WalkLeft,
        WalkRight,
        Jump,
        Fall,
        ClimbUp,
        ClimbDown
    }


    private VisualState currentState =
        VisualState.Idle;


    private bool isBlinking;
    private bool isHurt;
    private bool isKicking;
    private bool isClimbing;
    private bool isCelebrating;


    // ============================================================
    // SWORD STATE
    // ============================================================

    private bool swordEquipped;
    private bool swordFacingRight;
    private bool isSwordAttacking;

    private Sprite activeSwordAttackSprite;


    // ============================================================
    // CLUB STATE
    // ============================================================

    private bool clubEquipped;
    private bool clubFacingRight;
    private bool isClubAttacking;

    private Sprite activeClubAttackSprite;


    // ============================================================
    // OTHER STATE
    // ============================================================

    private bool climbHookOnRight;
    private float climbVertical;

    private bool prisonWasLocked;

    private float nextBlinkTime;
    private float climbExitGraceUntil;

    private float ungroundedSince =
        -1f;

    private Sprite activeKickSprite;

    private Coroutine blinkRoutine;
    private Coroutine hurtRoutine;
    private Coroutine celebrationRoutine;


    // ============================================================
    // PUBLIC
    // ============================================================

    public bool IsKicking =>
        isKicking;

    public bool IsCelebrating =>
        isCelebrating;

    public bool IsClimbing =>
        isClimbing;

    public bool IsSwordEquipped =>
        swordEquipped;

    public bool IsSwordAttacking =>
        isSwordAttacking;

    public bool SwordFacingRight =>
        swordFacingRight;

    public bool IsClubEquipped =>
        clubEquipped;

    public bool IsClubAttacking =>
        isClubAttacking;

    public bool ClubFacingRight =>
        clubFacingRight;

    public bool GameplayActionsLocked =>
        isCelebrating;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        if (playerController == null)
        {
            playerController =
                GetComponent<PlayerController>();
        }

        if (playerJump == null)
        {
            playerJump =
                GetComponent<PlayerJump>();
        }

        if (prisonBreakDoor == null)
        {
            prisonBreakDoor =
                FindFirstObjectByType<
                    PrisonBreakDoor
                >();
        }
    }


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        currentState =
            VisualState.Idle;

        swordEquipped =
            false;

        clubEquipped =
            false;

        swordFacingRight =
            false;

        clubFacingRight =
            false;

        prisonWasLocked =
            IsPrisonLocked();

        if (prisonWasLocked)
        {
            SetSprite(
                prisonSadSprite
            );
        }
        else
        {
            SetSprite(
                idleSprite
            );
        }

        ScheduleBlink();
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        UpdatePrisonDoorState();

        if (isKicking)
            return;

        if (isCelebrating)
            return;

        if (isHurt)
            return;

        if (IsAnyWeaponAttacking())
            return;

        if (IsPrisonLocked())
        {
            currentState =
                VisualState.Idle;

            if (!isBlinking)
            {
                SetSprite(
                    prisonSadSprite
                );
            }

            HandleBlink();
            return;
        }

        if (isClimbing)
        {
            UpdateClimbVisual();
            HandleBlink();

            return;
        }

        if (Time.time <
            climbExitGraceUntil)
        {
            return;
        }

        UpdateNormalState();
        HandleBlink();
    }


    // ============================================================
    // LATE UPDATE
    // ============================================================

    private void LateUpdate()
    {
        if (isKicking &&
            spriteRenderer != null &&
            activeKickSprite != null)
        {
            spriteRenderer.sprite =
                activeKickSprite;

            return;
        }

        if (isCelebrating &&
            spriteRenderer != null &&
            doorBreakReactionSprite != null)
        {
            spriteRenderer.sprite =
                doorBreakReactionSprite;

            return;
        }

        if (isSwordAttacking &&
            spriteRenderer != null &&
            activeSwordAttackSprite != null)
        {
            spriteRenderer.sprite =
                activeSwordAttackSprite;

            return;
        }

        if (isClubAttacking &&
            spriteRenderer != null &&
            activeClubAttackSprite != null)
        {
            spriteRenderer.sprite =
                activeClubAttackSprite;
        }
    }


    // ============================================================
    // GENERIC WEAPON EQUIPMENT
    // ============================================================

    public void SetEquippedWeapon(
        string weaponId
    )
    {
        string normalizedId =
            string.IsNullOrWhiteSpace(
                weaponId
            )
                ? ""
                : weaponId
                    .Trim()
                    .ToLowerInvariant();


        bool newSwordEquipped =
            normalizedId ==
            "sword";


        bool newClubEquipped =
            normalizedId ==
            "club";


        if (swordEquipped ==
                newSwordEquipped &&
            clubEquipped ==
                newClubEquipped)
        {
            return;
        }


        if (!newSwordEquipped &&
            isSwordAttacking)
        {
            ClearSwordAttackState();
        }


        if (!newClubEquipped &&
            isClubAttacking)
        {
            ClearClubAttackState();
        }


        swordEquipped =
            newSwordEquipped;


        clubEquipped =
            newClubEquipped;


        StopBlinkRoutine();


        if (isKicking ||
            isHurt ||
            isCelebrating ||
            IsPrisonLocked() ||
            isClimbing ||
            Time.time <
                climbExitGraceUntil)
        {
            ScheduleBlink();
            return;
        }


        UpdateNormalState();

        ScheduleBlink();
    }


    // ============================================================
    // SWORD EQUIPMENT
    // ============================================================

    /*
     * Старый метод сохраняем полностью
     * для совместимости с уже рабочими
     * скриптами меча.
     */
    public void SetSwordEquipped(
        bool equipped
    )
    {
        if (equipped)
        {
            SetEquippedWeapon(
                "Sword"
            );
        }
        else
        {
            SetEquippedWeapon(
                null
            );
        }
    }


    // ============================================================
    // CLUB EQUIPMENT
    // ============================================================

    public void SetClubEquipped(
        bool equipped
    )
    {
        if (equipped)
        {
            SetEquippedWeapon(
                "Club"
            );
        }
        else
        {
            SetEquippedWeapon(
                null
            );
        }
    }


    // ============================================================
    // SWORD ATTACK VISUAL
    // ============================================================

    public void PlaySwordSwingLeft()
    {
        PlaySwordSwing(
            false
        );
    }


    public void PlaySwordSwingRight()
    {
        PlaySwordSwing(
            true
        );
    }


    public void PlaySwordSwing(
        bool attackRight
    )
    {
        CancelHurtForAction();

        if (!CanShowSwordAttack())
        {
            return;
        }


        swordFacingRight =
            attackRight;


        Sprite target =
            attackRight
                ? swordSwingRightSprite
                : swordSwingLeftSprite;


        if (target == null)
        {
            return;
        }


        StopBlinkRoutine();


        isSwordAttacking =
            true;


        activeSwordAttackSprite =
            target;


        SetSprite(
            target
        );
    }


    public void PlaySwordStrikeLeft()
    {
        PlaySwordStrike(
            false
        );
    }


    public void PlaySwordStrikeRight()
    {
        PlaySwordStrike(
            true
        );
    }


    public void PlaySwordStrike(
        bool attackRight
    )
    {
        CancelHurtForAction();

        if (!CanShowSwordAttack())
        {
            return;
        }


        swordFacingRight =
            attackRight;


        Sprite target =
            attackRight
                ? swordStrikeRightSprite
                : swordStrikeLeftSprite;


        if (target == null)
        {
            return;
        }


        StopBlinkRoutine();


        isSwordAttacking =
            true;


        activeSwordAttackSprite =
            target;


        SetSprite(
            target
        );
    }


    public void EndSwordAttackVisual()
    {
        if (!isSwordAttacking)
        {
            return;
        }


        ClearSwordAttackState();


        if (!isKicking &&
            !isHurt &&
            !isCelebrating &&
            !IsPrisonLocked() &&
            !isClimbing)
        {
            RestoreCurrentSprite();

            ScheduleBlink();
        }
    }


    public void CancelSwordAttackVisual()
    {
        EndSwordAttackVisual();
    }


    private bool CanShowSwordAttack()
    {
        if (spriteRenderer == null)
        {
            return false;
        }


        if (!swordEquipped)
        {
            return false;
        }


        if (isKicking ||
            isHurt ||
            isCelebrating ||
            IsPrisonLocked() ||
            isClimbing ||
            Time.time <
                climbExitGraceUntil)
        {
            return false;
        }


        return true;
    }


    private void ClearSwordAttackState()
    {
        isSwordAttacking =
            false;


        activeSwordAttackSprite =
            null;
    }


    // ============================================================
    // CLUB ATTACK VISUAL
    // ============================================================

    /*
     * Эти методы уже готовы заранее.
     *
     * Когда позже сделаем PlayerClubAttack,
     * он сможет сразу вызывать их,
     * и PlayerVisual снова переделывать
     * не придётся.
     */

    public void PlayClubSwingLeft()
    {
        PlayClubSwing(
            false
        );
    }


    public void PlayClubSwingRight()
    {
        PlayClubSwing(
            true
        );
    }


    public void PlayClubSwing(
        bool attackRight
    )
    {
        CancelHurtForAction();


        if (!CanShowClubAttack())
        {
            return;
        }


        clubFacingRight =
            attackRight;


        Sprite target =
            attackRight
                ? clubSwingRightSprite
                : clubSwingLeftSprite;


        if (target == null)
        {
            return;
        }


        StopBlinkRoutine();


        isClubAttacking =
            true;


        activeClubAttackSprite =
            target;


        SetSprite(
            target
        );
    }


    public void PlayClubStrikeLeft()
    {
        PlayClubStrike(
            false
        );
    }


    public void PlayClubStrikeRight()
    {
        PlayClubStrike(
            true
        );
    }


    public void PlayClubStrike(
        bool attackRight
    )
    {
        CancelHurtForAction();


        if (!CanShowClubAttack())
        {
            return;
        }


        clubFacingRight =
            attackRight;


        Sprite target =
            attackRight
                ? clubStrikeRightSprite
                : clubStrikeLeftSprite;


        if (target == null)
        {
            return;
        }


        StopBlinkRoutine();


        isClubAttacking =
            true;


        activeClubAttackSprite =
            target;


        SetSprite(
            target
        );
    }


    public void EndClubAttackVisual()
    {
        if (!isClubAttacking)
        {
            return;
        }


        ClearClubAttackState();


        if (!isKicking &&
            !isHurt &&
            !isCelebrating &&
            !IsPrisonLocked() &&
            !isClimbing)
        {
            RestoreCurrentSprite();

            ScheduleBlink();
        }
    }


    public void CancelClubAttackVisual()
    {
        EndClubAttackVisual();
    }


    private bool CanShowClubAttack()
    {
        if (spriteRenderer == null)
        {
            return false;
        }


        if (!clubEquipped)
        {
            return false;
        }


        if (isKicking ||
            isHurt ||
            isCelebrating ||
            IsPrisonLocked() ||
            isClimbing ||
            Time.time <
                climbExitGraceUntil)
        {
            return false;
        }


        return true;
    }


    private void ClearClubAttackState()
    {
        isClubAttacking =
            false;


        activeClubAttackSprite =
            null;
    }


    private bool IsAnyWeaponAttacking()
    {
        return
            isSwordAttacking ||
            isClubAttacking;
    }


    // ============================================================
    // CANCEL HURT FOR PLAYER ACTION
    // ============================================================

    private void CancelHurtForAction()
    {
        if (!isHurt &&
            hurtRoutine == null)
        {
            return;
        }


        if (hurtRoutine != null)
        {
            StopCoroutine(
                hurtRoutine
            );


            hurtRoutine =
                null;
        }


        isHurt =
            false;
    }


    // ============================================================
    // PRISON
    // ============================================================

    private bool IsPrisonLocked()
    {
        return
            prisonBreakDoor != null &&
            !prisonBreakDoor.IsBroken;
    }


    private void UpdatePrisonDoorState()
    {
        bool lockedNow =
            IsPrisonLocked();


        if (prisonWasLocked &&
            !lockedNow)
        {
            StartDoorBreakReaction();
        }


        prisonWasLocked =
            lockedNow;
    }


    // ============================================================
    // NORMAL STATE
    // ============================================================

    private void UpdateNormalState()
    {
        bool grounded =
            playerJump != null
                ? playerJump.IsGrounded()
                : true;


        if (playerJump != null &&
            playerJump.IsJumpInProgress)
        {
            ungroundedSince =
                -1f;


            if (rb != null &&
                rb.linearVelocity.y <
                fallVelocityThreshold)
            {
                currentState =
                    VisualState.Fall;
            }
            else
            {
                currentState =
                    VisualState.Jump;
            }


            if (!isBlinking)
            {
                ApplyCurrentSprite();
            }


            return;
        }


        if (!grounded)
        {
            if (ungroundedSince < 0f)
            {
                ungroundedSince =
                    Time.time;
            }


            bool delayPassed =
                Time.time -
                ungroundedSince >=
                fallVisualDelay;


            bool movingDown =
                rb == null ||
                rb.linearVelocity.y <
                fallVelocityThreshold;


            if (delayPassed &&
                movingDown)
            {
                currentState =
                    VisualState.Fall;


                if (!isBlinking)
                {
                    ApplyCurrentSprite();
                }
            }


            return;
        }


        ungroundedSince =
            -1f;


        float velocityX =
            rb != null
                ? rb.linearVelocity.x
                : 0f;


        if (velocityX >
            movementThreshold)
        {
            swordFacingRight =
                true;


            clubFacingRight =
                true;


            currentState =
                VisualState.WalkRight;


            if (!isBlinking)
            {
                ApplyCurrentSprite();
            }


            return;
        }


        if (velocityX <
            -movementThreshold)
        {
            swordFacingRight =
                false;


            clubFacingRight =
                false;


            currentState =
                VisualState.WalkLeft;


            if (!isBlinking)
            {
                ApplyCurrentSprite();
            }


            return;
        }


        currentState =
            VisualState.Idle;


        if (!isBlinking)
        {
            ApplyCurrentSprite();
        }
    }


    // ============================================================
    // APPLY CURRENT
    // ============================================================

    private void ApplyCurrentSprite()
    {
        Sprite target =
            idleSprite;


        switch (currentState)
        {
            case VisualState.Idle:

                if (clubEquipped)
                {
                    Sprite clubTarget =
                        GetClubIdleSprite(
                            false
                        );


                    target =
                        clubTarget != null
                            ? clubTarget
                            : idleSprite;
                }
                else if (swordEquipped)
                {
                    Sprite swordTarget =
                        GetSwordIdleSprite(
                            false
                        );


                    target =
                        swordTarget != null
                            ? swordTarget
                            : idleSprite;
                }
                else
                {
                    target =
                        idleSprite;
                }

                break;


            case VisualState.WalkLeft:

                if (clubEquipped &&
                    clubWalkLeftSprite != null)
                {
                    target =
                        clubWalkLeftSprite;
                }
                else if (swordEquipped &&
                         swordWalkLeftSprite != null)
                {
                    target =
                        swordWalkLeftSprite;
                }
                else
                {
                    target =
                        walkLeftSprite;
                }

                break;


            case VisualState.WalkRight:

                if (clubEquipped &&
                    clubWalkRightSprite != null)
                {
                    target =
                        clubWalkRightSprite;
                }
                else if (swordEquipped &&
                         swordWalkRightSprite != null)
                {
                    target =
                        swordWalkRightSprite;
                }
                else
                {
                    target =
                        walkRightSprite;
                }

                break;


            case VisualState.Jump:

                if (clubEquipped &&
                    clubJumpSprite != null)
                {
                    target =
                        clubJumpSprite;
                }
                else if (swordEquipped &&
                         swordJumpSprite != null)
                {
                    target =
                        swordJumpSprite;
                }
                else
                {
                    target =
                        jumpSprite;
                }

                break;


            case VisualState.Fall:

                if (clubEquipped &&
                    clubFallSprite != null)
                {
                    target =
                        clubFallSprite;
                }
                else if (swordEquipped &&
                         swordFallSprite != null)
                {
                    target =
                        swordFallSprite;
                }
                else
                {
                    target =
                        fallSprite;
                }

                break;


            case VisualState.ClimbUp:

                target =
                    climbHookOnRight
                        ? climbUpRightSprite
                        : climbUpLeftSprite;

                break;


            case VisualState.ClimbDown:

                target =
                    climbHookOnRight
                        ? climbDownRightSprite
                        : climbDownLeftSprite;

                break;
        }


        SetSprite(
            target
        );
    }


    // ============================================================
    // SWORD IDLE HELPERS
    // ============================================================

    private Sprite GetSwordIdleSprite(
        bool blink
    )
    {
        Sprite preferred;
        Sprite opposite;


        if (swordFacingRight)
        {
            preferred =
                blink
                    ? swordIdleRightBlinkSprite
                    : swordIdleRightSprite;


            opposite =
                blink
                    ? swordIdleLeftBlinkSprite
                    : swordIdleLeftSprite;
        }
        else
        {
            preferred =
                blink
                    ? swordIdleLeftBlinkSprite
                    : swordIdleLeftSprite;


            opposite =
                blink
                    ? swordIdleRightBlinkSprite
                    : swordIdleRightSprite;
        }


        if (preferred != null)
        {
            return preferred;
        }


        return opposite;
    }


    private Sprite GetSwordHurtSprite()
    {
        Sprite preferred =
            swordFacingRight
                ? swordHurtRightSprite
                : swordHurtLeftSprite;


        if (preferred != null)
        {
            return preferred;
        }


        Sprite opposite =
            swordFacingRight
                ? swordHurtLeftSprite
                : swordHurtRightSprite;


        return opposite;
    }


    // ============================================================
    // CLUB IDLE HELPERS
    // ============================================================

    private Sprite GetClubIdleSprite(
        bool blink
    )
    {
        Sprite preferred;
        Sprite opposite;


        if (clubFacingRight)
        {
            preferred =
                blink
                    ? clubIdleRightBlinkSprite
                    : clubIdleRightSprite;


            opposite =
                blink
                    ? clubIdleLeftBlinkSprite
                    : clubIdleLeftSprite;
        }
        else
        {
            preferred =
                blink
                    ? clubIdleLeftBlinkSprite
                    : clubIdleLeftSprite;


            opposite =
                blink
                    ? clubIdleRightBlinkSprite
                    : clubIdleRightSprite;
        }


        if (preferred != null)
        {
            return preferred;
        }


        return opposite;
    }


    private Sprite GetClubHurtSprite()
    {
        Sprite preferred =
            clubFacingRight
                ? clubHurtRightSprite
                : clubHurtLeftSprite;


        if (preferred != null)
        {
            return preferred;
        }


        Sprite opposite =
            clubFacingRight
                ? clubHurtLeftSprite
                : clubHurtRightSprite;


        return opposite;
    }


    // ============================================================
    // BLINK
    // ============================================================

    private void HandleBlink()
    {
        if (isBlinking ||
            isKicking ||
            isHurt ||
            isCelebrating ||
            IsAnyWeaponAttacking())
        {
            return;
        }


        if (Time.time <
            nextBlinkTime)
        {
            return;
        }


        Sprite blinkSprite =
            GetCurrentBlinkSprite();


        if (blinkSprite == null)
        {
            ScheduleBlink();

            return;
        }


        blinkRoutine =
            StartCoroutine(
                BlinkRoutine(
                    blinkSprite
                )
            );
    }


    private IEnumerator BlinkRoutine(
        Sprite blinkSprite
    )
    {
        isBlinking =
            true;


        SetSprite(
            blinkSprite
        );


        yield return new WaitForSeconds(
            blinkDuration
        );


        isBlinking =
            false;


        if (!isKicking &&
            !isHurt &&
            !isCelebrating &&
            !IsAnyWeaponAttacking())
        {
            RestoreCurrentSprite();
        }


        ScheduleBlink();


        blinkRoutine =
            null;
    }


    private Sprite GetCurrentBlinkSprite()
    {
        if (IsPrisonLocked())
        {
            return
                prisonSadBlinkSprite;
        }


        if (isClimbing)
        {
            if (climbVertical > 0.1f)
            {
                return
                    climbHookOnRight
                        ? climbUpRightBlinkSprite
                        : climbUpLeftBlinkSprite;
            }


            if (climbVertical < -0.1f)
            {
                return
                    climbHookOnRight
                        ? climbDownRightBlinkSprite
                        : climbDownLeftBlinkSprite;
            }


            return null;
        }


        if (clubEquipped)
        {
            switch (currentState)
            {
                case VisualState.Idle:
                    return
                        GetClubIdleSprite(
                            true
                        );


                case VisualState.WalkLeft:
                    return
                        clubWalkLeftBlinkSprite;


                case VisualState.WalkRight:
                    return
                        clubWalkRightBlinkSprite;
            }


            return null;
        }


        if (swordEquipped)
        {
            switch (currentState)
            {
                case VisualState.Idle:
                    return
                        GetSwordIdleSprite(
                            true
                        );


                case VisualState.WalkLeft:
                    return
                        swordWalkLeftBlinkSprite;


                case VisualState.WalkRight:
                    return
                        swordWalkRightBlinkSprite;
            }


            return null;
        }


        switch (currentState)
        {
            case VisualState.Idle:
                return
                    idleBlinkSprite;


            case VisualState.WalkLeft:
                return
                    walkLeftBlinkSprite;


            case VisualState.WalkRight:
                return
                    walkRightBlinkSprite;
        }


        return null;
    }


    private void ScheduleBlink()
    {
        float minimum =
            Mathf.Min(
                blinkRandomDelay.x,
                blinkRandomDelay.y
            );


        float maximum =
            Mathf.Max(
                blinkRandomDelay.x,
                blinkRandomDelay.y
            );


        nextBlinkTime =
            Time.time +
            blinkInterval +
            Random.Range(
                minimum,
                maximum
            );
    }


    private void StopBlinkRoutine()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(
                blinkRoutine
            );


            blinkRoutine =
                null;
        }


        isBlinking =
            false;
    }


    // ============================================================
    // KICK
    // ============================================================

    public void PlayKickRight()
    {
        PlayKick(
            true
        );
    }


    public void PlayKickLeft()
    {
        PlayKick(
            false
        );
    }


    public void PlayKick(
        bool kickRight
    )
    {
        if (spriteRenderer == null ||
            isCelebrating ||
            IsAnyWeaponAttacking())
        {
            return;
        }


        CancelHurtForAction();


        StopBlinkRoutine();


        isKicking =
            true;


        Sprite normalKickSprite =
            kickRight
                ? kickRightSprite
                : kickLeftSprite;


        Sprite swordKickSprite =
            kickRight
                ? swordKickRightSprite
                : swordKickLeftSprite;


        Sprite clubKickSprite =
            kickRight
                ? clubKickRightSprite
                : clubKickLeftSprite;


        if (clubEquipped &&
            clubKickSprite != null)
        {
            activeKickSprite =
                clubKickSprite;
        }
        else if (swordEquipped &&
                 swordKickSprite != null)
        {
            activeKickSprite =
                swordKickSprite;
        }
        else
        {
            activeKickSprite =
                normalKickSprite;
        }


        SetSprite(
            activeKickSprite
        );
    }


    public void EndKick()
    {
        if (!isKicking)
        {
            return;
        }


        isKicking =
            false;


        activeKickSprite =
            null;


        if (!isCelebrating)
        {
            RestoreCurrentSprite();

            ScheduleBlink();
        }
    }


    // ============================================================
    // JUMP
    // ============================================================

    public void PlayJumpLookUp()
    {
        if (isKicking ||
            isHurt ||
            isCelebrating ||
            IsAnyWeaponAttacking() ||
            IsPrisonLocked() ||
            isClimbing)
        {
            return;
        }


        currentState =
            VisualState.Jump;


        StopBlinkRoutine();


        ApplyCurrentSprite();
    }


    // ============================================================
    // HURT
    // ============================================================

    public void PlayHurtVisual()
    {
        if (isKicking ||
            isCelebrating)
        {
            return;
        }


        if (isSwordAttacking)
        {
            ClearSwordAttackState();
        }


        if (isClubAttacking)
        {
            ClearClubAttackState();
        }


        if (hurtRoutine != null)
        {
            StopCoroutine(
                hurtRoutine
            );
        }


        hurtRoutine =
            StartCoroutine(
                HurtRoutine()
            );
    }


    private IEnumerator HurtRoutine()
    {
        isHurt =
            true;


        StopBlinkRoutine();


        Sprite target =
            hurtSprite;


        if (clubEquipped)
        {
            Sprite clubHurt =
                GetClubHurtSprite();


            if (clubHurt != null)
            {
                target =
                    clubHurt;
            }
        }
        else if (swordEquipped)
        {
            Sprite swordHurt =
                GetSwordHurtSprite();


            if (swordHurt != null)
            {
                target =
                    swordHurt;
            }
        }


        SetSprite(
            target
        );


        yield return new WaitForSeconds(
            hurtDuration
        );


        isHurt =
            false;


        if (!isKicking &&
            !isCelebrating)
        {
            RestoreCurrentSprite();

            ScheduleBlink();
        }


        hurtRoutine =
            null;
    }


    // ============================================================
    // CLIMB
    // ============================================================

    public void SetClimbLook(
        float vertical
    )
    {
        SetClimbLook(
            vertical,
            climbHookOnRight
        );
    }


    public void SetClimbLook(
        float vertical,
        bool hookOnRight
    )
    {
        if (isKicking ||
            isHurt ||
            isCelebrating ||
            IsAnyWeaponAttacking() ||
            IsPrisonLocked())
        {
            return;
        }


        climbExitGraceUntil =
            0f;


        bool justStarted =
            !isClimbing;


        isClimbing =
            true;


        climbHookOnRight =
            hookOnRight;


        climbVertical =
            vertical;


        if (justStarted)
        {
            StopBlinkRoutine();
        }


        if (vertical > 0.1f)
        {
            currentState =
                VisualState.ClimbUp;
        }
        else if (vertical < -0.1f)
        {
            currentState =
                VisualState.ClimbDown;
        }


        if (Mathf.Abs(vertical) >
            0.1f &&
            !isBlinking)
        {
            ApplyCurrentSprite();
        }
    }


    private void UpdateClimbVisual()
    {
        if (Mathf.Abs(climbVertical) <=
            0.1f)
        {
            return;
        }


        if (!isBlinking)
        {
            ApplyCurrentSprite();
        }
    }


    public void ClearClimbLook()
    {
        if (!isClimbing)
        {
            return;
        }


        isClimbing =
            false;


        climbVertical =
            0f;


        climbExitGraceUntil =
            Time.time +
            climbExitVisualGrace;


        ScheduleBlink();
    }


    // ============================================================
    // DOOR BREAK
    // ============================================================

    private void StartDoorBreakReaction()
    {
        if (celebrationRoutine != null)
        {
            StopCoroutine(
                celebrationRoutine
            );
        }


        if (isSwordAttacking)
        {
            ClearSwordAttackState();
        }


        if (isClubAttacking)
        {
            ClearClubAttackState();
        }


        celebrationRoutine =
            StartCoroutine(
                DoorBreakReactionRoutine()
            );
    }


    private IEnumerator DoorBreakReactionRoutine()
    {
        isCelebrating =
            true;


        if (playerController != null)
        {
            playerController.SetActionLock(
                true
            );
        }


        StopBlinkRoutine();


        while (isKicking)
        {
            yield return null;
        }


        SetSprite(
            doorBreakReactionSprite
        );


        yield return new WaitForSeconds(
            doorBreakReactionDuration
        );


        isCelebrating =
            false;


        if (playerController != null)
        {
            playerController.SetActionLock(
                false
            );
        }


        celebrationRoutine =
            null;


        currentState =
            VisualState.Idle;


        ungroundedSince =
            -1f;


        RestoreCurrentSprite();

        ScheduleBlink();
    }


    // ============================================================
    // RESTORE
    // ============================================================

    private void RestoreCurrentSprite()
    {
        if (isKicking)
        {
            SetSprite(
                activeKickSprite
            );

            return;
        }


        if (isCelebrating)
        {
            SetSprite(
                doorBreakReactionSprite
            );

            return;
        }


        if (isHurt)
        {
            Sprite target =
                hurtSprite;


            if (clubEquipped)
            {
                Sprite clubHurt =
                    GetClubHurtSprite();


                if (clubHurt != null)
                {
                    target =
                        clubHurt;
                }
            }
            else if (swordEquipped)
            {
                Sprite swordHurt =
                    GetSwordHurtSprite();


                if (swordHurt != null)
                {
                    target =
                        swordHurt;
                }
            }


            SetSprite(
                target
            );


            return;
        }


        if (isSwordAttacking)
        {
            SetSprite(
                activeSwordAttackSprite
            );

            return;
        }


        if (isClubAttacking)
        {
            SetSprite(
                activeClubAttackSprite
            );

            return;
        }


        if (IsPrisonLocked())
        {
            SetSprite(
                prisonSadSprite
            );

            return;
        }


        if (isClimbing ||
            Time.time <
            climbExitGraceUntil)
        {
            return;
        }


        UpdateNormalState();
    }


    // ============================================================
    // SET SPRITE
    // ============================================================

    private void SetSprite(
        Sprite sprite
    )
    {
        if (spriteRenderer == null ||
            sprite == null)
        {
            return;
        }


        if (spriteRenderer.sprite !=
            sprite)
        {
            spriteRenderer.sprite =
                sprite;
        }
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        movementThreshold =
            Mathf.Max(
                0.001f,
                movementThreshold
            );


        blinkInterval =
            Mathf.Max(
                0.1f,
                blinkInterval
            );


        blinkDuration =
            Mathf.Max(
                0.01f,
                blinkDuration
            );


        hurtDuration =
            Mathf.Max(
                0.01f,
                hurtDuration
            );


        fallVisualDelay =
            Mathf.Max(
                0f,
                fallVisualDelay
            );


        climbExitVisualGrace =
            Mathf.Max(
                0f,
                climbExitVisualGrace
            );


        doorBreakReactionDuration =
            Mathf.Max(
                0f,
                doorBreakReactionDuration
            );
    }
}