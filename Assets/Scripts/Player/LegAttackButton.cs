using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LegAttackButton : MonoBehaviour
{
    // ============================================================
    // PLAYER
    // ============================================================

    [Header("PLAYER")]

    [SerializeField]
    private Transform player;

    [SerializeField]
    private PlayerKick playerKick;

    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private PlayerVisual playerVisual;

    [SerializeField]
    private Collider2D playerCollider;

    [SerializeField]
    private string playerTag = "Player";


    // ============================================================
    // ATTACK ZONE
    // ============================================================

    [Header("ATTACK ZONE")]

    [Tooltip(
        "Насколько центр зоны удара смещён " +
        "в сторону пинка."
    )]
    [SerializeField]
    private float attackDistance = 0.85f;

    [Tooltip(
        "Ширина зоны пинка."
    )]
    [SerializeField]
    private float attackWidth = 0.75f;

    [Tooltip(
        "Высота зоны пинка."
    )]
    [SerializeField]
    private float attackHeight = 0.85f;

    [Tooltip(
        "Вертикальное смещение зоны пинка."
    )]
    [SerializeField]
    private float attackVerticalOffset = 0f;

    [Tooltip(
        "Минимальное расстояние центра цели " +
        "от центра Player по X, чтобы цель считалась " +
        "на правильной стороне. " +
        "Защищает от попадания по врагу за спиной."
    )]
    [SerializeField, Min(0f)]
    private float minimumTargetSideDistance = 0.03f;


    // ============================================================
    // BOX COMBO ASSIST
    // ============================================================

    [Header("BOX COMBO ASSIST")]

    [Tooltip(
        "Даёт небольшой дополнительный допуск " +
        "ТОЛЬКО для повторного удара по тому же ящику. " +
        "На Guard / Skeleton не влияет."
    )]
    [SerializeField]
    private bool useBoxComboAssist = true;

    [Tooltip(
        "Дополнительное расстояние только для ящика, " +
        "который был успешно ударен предыдущим ударом."
    )]
    [SerializeField, Min(0f)]
    private float boxComboExtraReach = 0.30f;

    [Tooltip(
        "Сколько секунд помнить последний ударенный ящик."
    )]
    [SerializeField, Min(0f)]
    private float boxComboMemoryTime = 0.9f;

    [Tooltip(
        "Допустимая разница по высоте между Player и ящиком."
    )]
    [SerializeField, Min(0f)]
    private float boxComboMaxVerticalDifference = 0.75f;


    // ============================================================
    // LEGACY DAMAGE
    // ============================================================

    [Header("LEGACY KICK DAMAGE")]

    [Tooltip(
        "Старое целочисленное значение удара. " +
        "Оставлено для совместимости со старыми объектами, " +
        "которые ещё не используют дробный баланс."
    )]
    [SerializeField, Min(1)]
    private int kickDamage = 1;


    // ============================================================
    // BALANCED KICK DAMAGE
    // ============================================================

    [Header("KICK DAMAGE - GUARD")]

    [Tooltip(
        "Урон ногой по Guard. " +
        "При здоровье Guard = 4 значение 0.67 даёт 6 ударов."
    )]
    [SerializeField, Min(0.01f)]
    private float guardKickDamage = 0.67f;


    [Header("KICK DAMAGE - SKELETON")]

    [Tooltip(
        "Урон ногой по Skeleton. " +
        "При здоровье Skeleton = 3 значение 0.60 даёт 5 ударов."
    )]
    [SerializeField, Min(0.01f)]
    private float skeletonKickDamage = 0.60f;


    [Header("KICK DAMAGE - REGULAR BOX")]

    [Tooltip(
        "Урон ногой по обычному ящику. " +
        "При прочности 2 значение 0.50 даёт 4 удара."
    )]
    [SerializeField, Min(0.01f)]
    private float regularBoxKickDamage = 0.50f;


    [Header("KICK DAMAGE - MIDDLE BOX")]

    [Tooltip(
        "Урон ногой по среднему ящику. " +
        "При прочности 3 значение 0.60 даёт 5 ударов."
    )]
    [SerializeField, Min(0.01f)]
    private float middleBoxKickDamage = 0.60f;


    [Header("KICK DAMAGE - HARD BOX")]

    [Tooltip(
        "Урон ногой по тяжёлому ящику. " +
        "При прочности 4 значение 0.67 даёт 6 ударов."
    )]
    [SerializeField, Min(0.01f)]
    private float hardBoxKickDamage = 0.67f;


    // ============================================================
    // DAMAGE TIMING
    // ============================================================

    [Header("DAMAGE TIMING")]

    [SerializeField]
    private float impactDelay = 0.08f;


    // ============================================================
    // DETECTION
    // ============================================================

    [Header("DETECTION")]

    [SerializeField]
    private LayerMask hittableLayers = ~0;

    [SerializeField]
    private bool hitEachObjectOnlyOnce = true;


    // ============================================================
    // BUTTON
    // ============================================================

    [Header("BUTTON")]

    [SerializeField]
    private Button legButton;


    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("HAPTICS")]

    [SerializeField]
    private bool useKickHaptics = true;

    [SerializeField, Range(5, 100)]
    private int kickHapticMs = 18;


    // ============================================================
    // KICK IMPACT AUDIO
    // ============================================================

    [Header("KICK IMPACT AUDIO")]

    [SerializeField]
    private AudioSource kickImpactAudioSource;

    [SerializeField]
    private AudioClip kickImpactClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float kickImpactVolume = 1f;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = false;

    [SerializeField]
    private bool drawAttackZone = true;


    // ============================================================
    // PRIVATE
    // ============================================================

    private bool attackBusy;

    private Coroutine attackRoutine;

    private GameObject rememberedBox;

    private float rememberedBoxHitTime;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (legButton == null)
        {
            legButton =
                GetComponent<Button>();
        }

        FindPlayer();

        if (kickImpactAudioSource == null &&
            player != null)
        {
            kickImpactAudioSource =
                player.GetComponent<AudioSource>();
        }

        if (legButton != null)
        {
            legButton.onClick.RemoveListener(
                OnLegButtonPressed
            );

            legButton.onClick.AddListener(
                OnLegButtonPressed
            );
        }
    }


    // ============================================================
    // FIND PLAYER
    // ============================================================

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag(
                    playerTag
                );

            if (playerObject != null)
            {
                player =
                    playerObject.transform;
            }
        }

        if (player == null)
            return;

        if (playerKick == null)
        {
            playerKick =
                player.GetComponent<PlayerKick>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                player.GetComponent<PlayerHealth>();
        }

        if (playerController == null)
        {
            playerController =
                player.GetComponent<PlayerController>();
        }

        if (playerVisual == null)
        {
            playerVisual =
                player.GetComponent<PlayerVisual>();
        }

        if (playerCollider == null)
        {
            playerCollider =
                player.GetComponent<Collider2D>();

            if (playerCollider == null)
            {
                playerCollider =
                    player.GetComponentInChildren<
                        Collider2D
                    >();
            }
        }

        if (kickImpactAudioSource == null)
        {
            kickImpactAudioSource =
                player.GetComponent<AudioSource>();
        }
    }


    // ============================================================
    // CHECKS
    // ============================================================

    private bool PlayerIsDead()
    {
        if (playerHealth == null)
        {
            FindPlayer();
        }

        return
            playerHealth != null &&
            playerHealth.IsDead;
    }


    private bool GameplayActionsLocked()
    {
        if (PlayerIsDead())
            return true;

        if (playerVisual != null &&
            playerVisual.GameplayActionsLocked)
        {
            return true;
        }

        if (playerController != null &&
            playerController.IsActionLocked)
        {
            return true;
        }

        return false;
    }


    // ============================================================
    // BUTTON
    // ============================================================

    public void OnLegButtonPressed()
    {
        if (GameplayActionsLocked())
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[LEG ATTACK] Action locked.",
                    this
                );
            }

            return;
        }

        if (attackBusy)
            return;

        if (player == null ||
            playerKick == null)
        {
            FindPlayer();
        }

        if (player == null ||
            playerKick == null)
        {
            Debug.LogWarning(
                "[LEG ATTACK] Player или PlayerKick не найден.",
                this
            );

            return;
        }

        bool kickStarted =
            playerKick.Kick();

        if (!kickStarted)
            return;

        if (useKickHaptics)
        {
            MicroHaptics.Pulse(
                kickHapticMs,
                MicroHaptics.IOSHapticStyle.Light
            );
        }

        attackRoutine =
            StartCoroutine(
                AttackRoutine()
            );
    }


    // ============================================================
    // ROUTINE
    // ============================================================

    private IEnumerator AttackRoutine()
    {
        attackBusy =
            true;

        if (impactDelay > 0f)
        {
            float timer =
                0f;

            while (timer <
                   impactDelay)
            {
                if (GameplayActionsLocked())
                {
                    attackBusy =
                        false;

                    attackRoutine =
                        null;

                    yield break;
                }

                timer +=
                    Time.deltaTime;

                yield return null;
            }
        }

        if (!GameplayActionsLocked())
        {
            PerformKickHit();
        }

        attackBusy =
            false;

        attackRoutine =
            null;
    }


    // ============================================================
    // PERFORM HIT
    // ============================================================

    private void PerformKickHit()
    {
        if (GameplayActionsLocked())
            return;

        if (player == null ||
            playerKick == null)
        {
            return;
        }

        float direction =
            playerKick.FacingRight
                ? 1f
                : -1f;

        Vector2 attackCenter =
            new Vector2(
                player.position.x +
                direction *
                attackDistance,

                player.position.y +
                attackVerticalOffset
            );

        Vector2 attackSize =
            new Vector2(
                attackWidth,
                attackHeight
            );

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                attackCenter,
                attackSize,
                0f,
                hittableLayers
            );

        HashSet<GameObject>
            alreadyHit =
                new HashSet<GameObject>();

        bool anyRealTargetHit =
            false;

        bool enemyImpactSoundPlayed =
            false;

        if (hits != null)
        {
            foreach (Collider2D hit in hits)
            {
                if (GameplayActionsLocked())
                    return;

                if (hit == null)
                    continue;

                if (hit.transform == player ||
                    hit.transform.IsChildOf(player))
                {
                    continue;
                }

                GameObject target =
                    FindKickTargetObject(
                        hit
                    );

                if (target == null)
                    continue;

                if (!IsTargetOnKickSide(
                        target,
                        direction))
                {
                    if (debugLogs)
                    {
                        Debug.Log(
                            "[LEG ATTACK] Target ignored: " +
                            target.name +
                            " is on the WRONG SIDE.",
                            target
                        );
                    }

                    continue;
                }

                if (hitEachObjectOnlyOnce &&
                    alreadyHit.Contains(
                        target))
                {
                    continue;
                }

                if (hitEachObjectOnlyOnce)
                {
                    alreadyHit.Add(
                        target
                    );
                }

                bool hasKickReceiver =
                    HasReceiveKick(
                        target
                    );

                if (!hasKickReceiver)
                    continue;

                bool isEnemyTarget =
                    IsEnemyTarget(
                        target
                    );

                bool isBoxTarget =
                    IsBreakableBoxTarget(
                        target
                    );

                bool damageDelivered =
                    ApplyBalancedKickDamage(
                        target
                    );

                if (!damageDelivered)
                {
                    continue;
                }

                anyRealTargetHit =
                    true;

                if (isBoxTarget)
                {
                    RememberBox(
                        target
                    );
                }

                if (isEnemyTarget &&
                    !enemyImpactSoundPlayed)
                {
                    PlayKickImpactSound();

                    enemyImpactSoundPlayed =
                        true;
                }

                if (debugLogs)
                {
                    Debug.Log(
                        "[LEG ATTACK] Kick -> " +
                        target.name +
                        " | Damage = " +
                        GetBalancedKickDamage(
                            target
                        ).ToString("F2"),
                        target
                    );
                }
            }
        }

        if (!anyRealTargetHit)
        {
            TryHitRememberedBox(
                direction,
                alreadyHit
            );
        }
    }


    // ============================================================
    // BALANCED KICK DAMAGE
    // ============================================================

    private bool ApplyBalancedKickDamage(
        GameObject target
    )
    {
        if (target == null)
        {
            return false;
        }

        float balancedDamage =
            GetBalancedKickDamage(
                target
            );

        /*
         * Новый метод:
         *
         * ReceiveKickDamage(float)
         *
         * Его мы сейчас добавим в Guard,
         * Skeleton и все три типа ящиков.
         *
         * Пока объект ещё старый,
         * автоматически используется старый
         * ReceiveKick(int).
         */
        if (TryInvokeFloatMethod(
                target,
                "ReceiveKickDamage",
                balancedDamage))
        {
            return true;
        }

        /*
         * Старый безопасный fallback.
         *
         * Благодаря этому текущая система ноги
         * не ломается между этапами переделки.
         */
        if (HasReceiveKick(
                target))
        {
            target.SendMessage(
                "ReceiveKick",
                kickDamage,
                SendMessageOptions.DontRequireReceiver
            );

            return true;
        }

        return false;
    }


    // ============================================================
    // BALANCED DAMAGE VALUE
    // ============================================================

    private float GetBalancedKickDamage(
        GameObject target
    )
    {
        if (target == null)
        {
            return kickDamage;
        }


        GuardEnemy guard =
            target.GetComponent<GuardEnemy>();

        if (guard != null)
        {
            return
                guardKickDamage;
        }


        SkeletonEnemy skeleton =
            target.GetComponent<SkeletonEnemy>();

        if (skeleton != null)
        {
            return
                skeletonKickDamage;
        }


        BreakableHardBox hardBox =
            target.GetComponent<
                BreakableHardBox
            >();

        if (hardBox != null)
        {
            return
                hardBoxKickDamage;
        }


        BreakableMiddleBox middleBox =
            target.GetComponent<
                BreakableMiddleBox
            >();

        if (middleBox != null)
        {
            return
                middleBoxKickDamage;
        }


        BreakableBox regularBox =
            target.GetComponent<
                BreakableBox
            >();

        if (regularBox != null)
        {
            return
                regularBoxKickDamage;
        }


        return
            kickDamage;
    }


    // ============================================================
    // INVOKE FLOAT DAMAGE METHOD
    // ============================================================

    private bool TryInvokeFloatMethod(
        GameObject target,
        string methodName,
        float damage
    )
    {
        if (target == null ||
            string.IsNullOrEmpty(
                methodName))
        {
            return false;
        }


        MonoBehaviour[] behaviours =
            target.GetComponents<
                MonoBehaviour
            >();


        foreach (MonoBehaviour behaviour
                 in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }


            MethodInfo method =
                behaviour
                    .GetType()
                    .GetMethod(
                        methodName,
                        new System.Type[]
                        {
                            typeof(float)
                        }
                    );


            if (method == null)
            {
                continue;
            }


            method.Invoke(
                behaviour,
                new object[]
                {
                    damage
                }
            );


            return true;
        }


        return false;
    }


    // ============================================================
    // STRICT KICK SIDE CHECK
    // ============================================================

    private bool IsTargetOnKickSide(
        GameObject target,
        float direction
    )
    {
        if (target == null ||
            player == null)
        {
            return false;
        }

        float playerCenterX =
            GetPlayerCenterX();

        float targetCenterX =
            GetTargetCenterX(
                target
            );

        float relativeX =
            targetCenterX -
            playerCenterX;

        if (direction > 0f)
        {
            return
                relativeX >
                minimumTargetSideDistance;
        }

        return
            relativeX <
            -minimumTargetSideDistance;
    }


    // ============================================================
    // PLAYER CENTER
    // ============================================================

    private float GetPlayerCenterX()
    {
        if (playerCollider != null)
        {
            return
                playerCollider.bounds.center.x;
        }

        if (player != null)
        {
            return
                player.position.x;
        }

        return
            transform.position.x;
    }


    // ============================================================
    // TARGET CENTER
    // ============================================================

    private float GetTargetCenterX(
        GameObject target
    )
    {
        if (target == null)
        {
            return 0f;
        }

        Collider2D targetCollider =
            target.GetComponent<Collider2D>();

        if (targetCollider == null)
        {
            targetCollider =
                target.GetComponentInChildren<
                    Collider2D
                >();
        }

        if (targetCollider != null)
        {
            return
                targetCollider.bounds.center.x;
        }

        return
            target.transform.position.x;
    }


    // ============================================================
    // BOX COMBO
    // ============================================================

    private void RememberBox(
        GameObject box
    )
    {
        if (!useBoxComboAssist ||
            box == null)
        {
            return;
        }

        rememberedBox =
            box;

        rememberedBoxHitTime =
            Time.time;
    }


    private void TryHitRememberedBox(
        float direction,
        HashSet<GameObject> alreadyHit
    )
    {
        if (!useBoxComboAssist)
            return;

        if (rememberedBox == null)
            return;

        if (Time.time -
            rememberedBoxHitTime >
            boxComboMemoryTime)
        {
            rememberedBox =
                null;

            return;
        }

        if (!IsBreakableBoxTarget(
                rememberedBox))
        {
            rememberedBox =
                null;

            return;
        }

        if (alreadyHit != null &&
            alreadyHit.Contains(
                rememberedBox))
        {
            return;
        }

        if (!IsTargetOnKickSide(
                rememberedBox,
                direction))
        {
            return;
        }

        Collider2D boxCollider =
            rememberedBox
                .GetComponent<Collider2D>();

        if (boxCollider == null)
        {
            boxCollider =
                rememberedBox
                    .GetComponentInChildren<
                        Collider2D
                    >();
        }

        if (boxCollider == null)
            return;

        float verticalDifference =
            Mathf.Abs(
                boxCollider.bounds.center.y -
                (
                    player.position.y +
                    attackVerticalOffset
                )
            );

        if (verticalDifference >
            boxComboMaxVerticalDifference)
        {
            return;
        }

        float nearEdgeDistance;

        float playerCenterX =
            GetPlayerCenterX();

        if (direction > 0f)
        {
            nearEdgeDistance =
                boxCollider.bounds.min.x -
                playerCenterX;
        }
        else
        {
            nearEdgeDistance =
                playerCenterX -
                boxCollider.bounds.max.x;
        }

        float normalMaximumReach =
            attackDistance +
            attackWidth *
            0.5f;

        float allowedReach =
            normalMaximumReach +
            boxComboExtraReach;

        if (nearEdgeDistance >
            allowedReach)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[LEG ATTACK] " +
                    "Remembered box too far. Distance = " +
                    nearEdgeDistance.ToString("F2"),
                    this
                );
            }

            return;
        }

        bool damageDelivered =
            ApplyBalancedKickDamage(
                rememberedBox
            );

        if (!damageDelivered)
        {
            return;
        }

        rememberedBoxHitTime =
            Time.time;

        if (debugLogs)
        {
            Debug.Log(
                "[LEG ATTACK] BOX COMBO ASSIST -> " +
                rememberedBox.name +
                " | Damage = " +
                GetBalancedKickDamage(
                    rememberedBox
                ).ToString("F2") +
                " | Distance = " +
                nearEdgeDistance.ToString("F2"),
                rememberedBox
            );
        }
    }


    // ============================================================
    // RECEIVE KICK CHECK
    // ============================================================

    private bool HasReceiveKick(
        GameObject target
    )
    {
        if (target == null)
            return false;

        MonoBehaviour[] behaviours =
            target.GetComponents<
                MonoBehaviour
            >();

        foreach (MonoBehaviour behaviour
                 in behaviours)
        {
            if (behaviour == null)
                continue;


            MethodInfo balancedMethod =
                behaviour
                    .GetType()
                    .GetMethod(
                        "ReceiveKickDamage",
                        new System.Type[]
                        {
                            typeof(float)
                        }
                    );

            if (balancedMethod != null)
            {
                return true;
            }


            MethodInfo legacyMethod =
                behaviour
                    .GetType()
                    .GetMethod(
                        "ReceiveKick",
                        new System.Type[]
                        {
                            typeof(int)
                        }
                    );

            if (legacyMethod != null)
            {
                return true;
            }
        }

        return false;
    }


    // ============================================================
    // BOX CHECK
    // ============================================================

    private bool IsBreakableBoxTarget(
        GameObject target
    )
    {
        if (target == null)
            return false;

        MonoBehaviour[] behaviours =
            target.GetComponents<
                MonoBehaviour
            >();

        foreach (MonoBehaviour behaviour
                 in behaviours)
        {
            if (behaviour == null)
                continue;

            string typeName =
                behaviour
                    .GetType()
                    .Name;

            if (typeName.Contains(
                    "Breakable") &&
                typeName.Contains(
                    "Box"))
            {
                return true;
            }
        }

        return false;
    }


    // ============================================================
    // ENEMY CHECK
    // ============================================================

    private bool IsEnemyTarget(
        GameObject target
    )
    {
        if (target == null)
            return false;


        GuardEnemy guard =
            target.GetComponent<
                GuardEnemy
            >();

        if (guard != null)
        {
            return
                !guard.IsDead;
        }


        SkeletonEnemy skeleton =
            target.GetComponent<
                SkeletonEnemy
            >();

        if (skeleton != null)
        {
            return
                !skeleton.IsDead;
        }


        return false;
    }


    // ============================================================
    // IMPACT AUDIO
    // ============================================================

    private void PlayKickImpactSound()
    {
        if (GameplayActionsLocked())
            return;

        if (kickImpactClip == null)
            return;

        if (kickImpactAudioSource != null)
        {
            kickImpactAudioSource.PlayOneShot(
                kickImpactClip,
                kickImpactVolume
            );
        }
        else if (player != null)
        {
            AudioSource.PlayClipAtPoint(
                kickImpactClip,
                player.position,
                kickImpactVolume
            );
        }
    }


    // ============================================================
    // FIND TARGET ROOT
    // ============================================================

    private GameObject FindKickTargetObject(
        Collider2D hit
    )
    {
        if (hit == null)
            return null;

        Transform current =
            hit.transform;

        while (current != null)
        {
            if (current.GetComponent<
                    GuardEnemy
                >() != null)
            {
                return
                    current.gameObject;
            }


            if (current.GetComponent<
                    SkeletonEnemy
                >() != null)
            {
                return
                    current.gameObject;
            }


            MonoBehaviour[] behaviours =
                current.GetComponents<
                    MonoBehaviour
                >();

            foreach (MonoBehaviour behaviour
                     in behaviours)
            {
                if (behaviour == null)
                    continue;


                MethodInfo balancedMethod =
                    behaviour
                        .GetType()
                        .GetMethod(
                            "ReceiveKickDamage",
                            new System.Type[]
                            {
                                typeof(float)
                            }
                        );

                if (balancedMethod != null)
                {
                    return
                        current.gameObject;
                }


                MethodInfo legacyMethod =
                    behaviour
                        .GetType()
                        .GetMethod(
                            "ReceiveKick",
                            new System.Type[]
                            {
                                typeof(int)
                            }
                        );

                if (legacyMethod != null)
                {
                    return
                        current.gameObject;
                }
            }

            current =
                current.parent;
        }

        return
            hit.gameObject;
    }


    // ============================================================
    // DISABLE
    // ============================================================

    private void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(
                attackRoutine
            );

            attackRoutine =
                null;
        }

        attackBusy =
            false;
    }


    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        if (legButton != null)
        {
            legButton.onClick.RemoveListener(
                OnLegButtonPressed
            );
        }
    }


    // ============================================================
    // GIZMO
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (!drawAttackZone)
            return;

        Transform targetPlayer =
            player;

        if (targetPlayer == null)
            return;

        bool right =
            true;

        if (playerKick != null)
        {
            right =
                playerKick.FacingRight;
        }

        float direction =
            right
                ? 1f
                : -1f;

        Vector3 center =
            new Vector3(
                targetPlayer.position.x +
                direction *
                attackDistance,

                targetPlayer.position.y +
                attackVerticalOffset,

                targetPlayer.position.z
            );

        Gizmos.DrawWireCube(
            center,
            new Vector3(
                attackWidth,
                attackHeight,
                0.01f
            )
        );
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        attackDistance =
            Mathf.Max(
                0f,
                attackDistance
            );

        attackWidth =
            Mathf.Max(
                0.01f,
                attackWidth
            );

        attackHeight =
            Mathf.Max(
                0.01f,
                attackHeight
            );

        minimumTargetSideDistance =
            Mathf.Max(
                0f,
                minimumTargetSideDistance
            );

        kickDamage =
            Mathf.Max(
                1,
                kickDamage
            );

        guardKickDamage =
            Mathf.Max(
                0.01f,
                guardKickDamage
            );

        skeletonKickDamage =
            Mathf.Max(
                0.01f,
                skeletonKickDamage
            );

        regularBoxKickDamage =
            Mathf.Max(
                0.01f,
                regularBoxKickDamage
            );

        middleBoxKickDamage =
            Mathf.Max(
                0.01f,
                middleBoxKickDamage
            );

        hardBoxKickDamage =
            Mathf.Max(
                0.01f,
                hardBoxKickDamage
            );

        impactDelay =
            Mathf.Max(
                0f,
                impactDelay
            );

        boxComboExtraReach =
            Mathf.Max(
                0f,
                boxComboExtraReach
            );

        boxComboMemoryTime =
            Mathf.Max(
                0f,
                boxComboMemoryTime
            );

        boxComboMaxVerticalDifference =
            Mathf.Max(
                0f,
                boxComboMaxVerticalDifference
            );

        kickImpactVolume =
            Mathf.Clamp01(
                kickImpactVolume
            );
    }
}