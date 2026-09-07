using System.Collections;
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

    [Tooltip(
        "Если Player не назначен вручную, " +
        "скрипт найдёт объект с Tag = Player."
    )]
    [SerializeField]
    private string playerTag = "Player";

    // ============================================================
    // ATTACK ZONE
    // ============================================================

    [Header("ATTACK ZONE")]

    [SerializeField]
    private float attackDistance = 0.85f;

    [SerializeField]
    private float attackWidth = 0.75f;

    [SerializeField]
    private float attackHeight = 0.85f;

    [SerializeField]
    private float attackVerticalOffset = 0f;

    // ============================================================
    // DAMAGE
    // ============================================================

    [Header("DAMAGE")]

    [SerializeField, Min(1)]
    private int kickDamage = 1;

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

    [Tooltip(
        "AudioSource для звука реального попадания ногой по врагу."
    )]
    [SerializeField]
    private AudioSource kickImpactAudioSource;

    [Tooltip(
        "Звук удара ноги по Guard / Skeleton."
    )]
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

        /*
         * Если AudioSource для попадания
         * не назначен вручную,
         * пробуем взять AudioSource с Player.
         */
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

        if (player != null)
        {
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

            if (kickImpactAudioSource == null)
            {
                kickImpactAudioSource =
                    player.GetComponent<AudioSource>();
            }
        }
    }

    // ============================================================
    // DEAD CHECK
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

    // ============================================================
    // BUTTON PRESSED
    // ============================================================

    public void OnLegButtonPressed()
    {
        if (PlayerIsDead())
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[LEG ATTACK] Игрок мёртв. Удар запрещён.",
                    this
                );
            }

            return;
        }

        if (attackBusy)
            return;

        if (player == null ||
            playerKick == null ||
            playerHealth == null)
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

        if (PlayerIsDead())
            return;

        bool kickStarted =
            playerKick.Kick();

        if (!kickStarted)
            return;

        /*
         * Старая вибрация самого удара.
         * Оставляем как есть.
         */
        if (!PlayerIsDead() &&
            useKickHaptics)
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
    // ATTACK ROUTINE
    // ============================================================

    private IEnumerator AttackRoutine()
    {
        attackBusy = true;

        if (impactDelay > 0f)
        {
            float timer = 0f;

            while (timer <
                   impactDelay)
            {
                if (PlayerIsDead())
                {
                    attackBusy = false;
                    attackRoutine = null;
                    yield break;
                }

                timer +=
                    Time.deltaTime;

                yield return null;
            }
        }

        if (PlayerIsDead())
        {
            attackBusy = false;
            attackRoutine = null;
            yield break;
        }

        PerformKickHit();

        attackBusy = false;
        attackRoutine = null;
    }

    // ============================================================
    // PERFORM HIT
    // ============================================================

    private void PerformKickHit()
    {
        if (PlayerIsDead())
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

        if (hits == null ||
            hits.Length == 0)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[LEG ATTACK] Удар в воздух.",
                    this
                );
            }

            return;
        }

        System.Collections.Generic.HashSet<GameObject>
            alreadyHit =
                new System.Collections.Generic.HashSet<GameObject>();

        /*
         * За одно нажатие звук попадания
         * по телу проигрываем максимум один раз.
         */
        bool enemyImpactSoundPlayed =
            false;

        foreach (Collider2D hit in hits)
        {
            if (PlayerIsDead())
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

            if (hitEachObjectOnlyOnce &&
                alreadyHit.Contains(target))
            {
                continue;
            }

            if (hitEachObjectOnlyOnce)
            {
                alreadyHit.Add(target);
            }

            /*
             * Проверяем:
             * это живой враг или другой объект?
             *
             * Только Guard / Skeleton
             * получают отдельный звук удара по телу.
             */
            bool isEnemyTarget =
                IsEnemyTarget(
                    target
                );

            target.SendMessage(
                "ReceiveKick",
                kickDamage,
                SendMessageOptions.DontRequireReceiver
            );

            /*
             * Ящики сюда НЕ проходят.
             * Поэтому у них остаются
             * только собственные звуки.
             */
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
                    target.name,
                    target
                );
            }
        }
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

        /*
         * GuardEnemy проверяем напрямую.
         */
        GuardEnemy guard =
            target.GetComponent<GuardEnemy>();

        if (guard != null)
        {
            return !guard.IsDead;
        }

        /*
         * SkeletonEnemy проверяем по имени класса.
         *
         * Так LegAttackButton не зависит
         * жёстко от реализации SkeletonEnemy.
         */
        MonoBehaviour[] behaviours =
            target.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour
                 in behaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour.GetType().Name ==
                "SkeletonEnemy")
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // KICK IMPACT SOUND
    // ============================================================

    private void PlayKickImpactSound()
    {
        if (PlayerIsDead())
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
            /*
             * Запасной вариант,
             * если AudioSource не назначен.
             */
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
            if (current.GetComponent<GuardEnemy>() != null)
            {
                return current.gameObject;
            }

            MonoBehaviour[] behaviours =
                current.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour behaviour
                     in behaviours)
            {
                if (behaviour == null)
                    continue;

                System.Reflection.MethodInfo method =
                    behaviour
                        .GetType()
                        .GetMethod(
                            "ReceiveKick",
                            new System.Type[]
                            {
                                typeof(int)
                            }
                        );

                if (method != null)
                {
                    return current.gameObject;
                }
            }

            current =
                current.parent;
        }

        return hit.gameObject;
    }

    // ============================================================
    // DISABLE SAFETY
    // ============================================================

    private void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(
                attackRoutine
            );

            attackRoutine = null;
        }

        attackBusy = false;
    }

    // ============================================================
    // CLEANUP
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
    // DEBUG ATTACK ZONE
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (!drawAttackZone)
            return;

        Transform targetPlayer =
            player;

        if (targetPlayer == null)
            return;

        bool right = true;

        if (playerKick != null)
        {
            right =
                playerKick.FacingRight;
        }

        float direction =
            right ? 1f : -1f;

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
}