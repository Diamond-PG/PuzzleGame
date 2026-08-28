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

    [Tooltip(
        "Как далеко удар ногой достаёт перед игроком."
    )]
    [SerializeField]
    private float attackDistance = 0.85f;

    [Tooltip(
        "Ширина области удара."
    )]
    [SerializeField]
    private float attackWidth = 0.75f;

    [Tooltip(
        "Высота области удара."
    )]
    [SerializeField]
    private float attackHeight = 0.85f;

    [Tooltip(
        "Смещение центра зоны удара по высоте " +
        "относительно Player."
    )]
    [SerializeField]
    private float attackVerticalOffset = 0f;

    // ============================================================
    // DAMAGE
    // ============================================================

    [Header("DAMAGE")]

    [Tooltip(
        "Сколько урона наносит один удар ногой."
    )]
    [SerializeField, Min(1)]
    private int kickDamage = 1;

    [Tooltip(
        "Через сколько секунд после нажатия " +
        "реально применяется удар. " +
        "Нужно для совпадения со спрайтом ноги."
    )]
    [SerializeField]
    private float impactDelay = 0.08f;

    // ============================================================
    // DETECTION
    // ============================================================

    [Header("DETECTION")]

    [Tooltip(
        "Какие слои вообще могут получать удар. " +
        "Пока можно оставить Everything. " +
        "Позже сделаем отдельный слой KickTarget."
    )]
    [SerializeField]
    private LayerMask hittableLayers = ~0;

    [Tooltip(
        "Не позволяет одному объекту получить " +
        "несколько ударов от разных Collider2D " +
        "за одно нажатие."
    )]
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
         * OnClick в Inspector можно оставить пустым.
         *
         * Скрипт сам подключает кнопку ноги.
         */
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

        if (player != null &&
            playerKick == null)
        {
            playerKick =
                player.GetComponent<PlayerKick>();
        }
    }

    // ============================================================
    // BUTTON PRESSED
    // ============================================================

    public void OnLegButtonPressed()
    {
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

        /*
         * Сначала запускаем сам визуальный удар игрока.
         *
         * PlayerKick уже знает,
         * куда в последний раз смотрел Player.
         */
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

        /*
         * Ждём момента,
         * когда нога на спрайте реально долетает
         * до объекта.
         */
        if (impactDelay > 0f)
        {
            yield return new WaitForSeconds(
                impactDelay
            );
        }

        PerformKickHit();

        /*
         * Отдельный большой cooldown здесь
         * не нужен.
         *
         * PlayerKick уже сам контролирует
         * Kick Cooldown.
         */
        attackBusy = false;
    }

    // ============================================================
    // PERFORM HIT
    // ============================================================

    private void PerformKickHit()
    {
        if (player == null ||
            playerKick == null)
        {
            return;
        }

        float direction =
            playerKick.FacingRight
                ? 1f
                : -1f;

        /*
         * Центр зоны находится ПЕРЕД игроком.
         */
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

        System.Collections.Generic.HashSet<
            GameObject
        > alreadyHit =
            new System.Collections.Generic.HashSet<
                GameObject
            >();

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            /*
             * Игрок самого себя ногой не бьёт.
             */
            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
            {
                continue;
            }

            /*
             * Ищем корневой объект,
             * которому принадлежит Collider.
             *
             * Это важно для:
             * - GuardEnemy;
             * - SkeletonEnemy;
             * - ящиков;
             * - будущих врагов.
             */
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
             * Универсальная команда.
             *
             * Любой объект, который должен
             * реагировать на удар ногой,
             * будет иметь метод:
             *
             * ReceiveKick(int damage)
             *
             * Поэтому эта кнопка будет работать
             * и со стражником, и со скелетом,
             * и с ящиком, и с будущими объектами.
             */
            target.SendMessage(
                "ReceiveKick",
                kickDamage,
                SendMessageOptions.DontRequireReceiver
            );

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

        /*
         * Проверяем сам объект
         * и несколько родителей.
         *
         * Это пригодится, например,
         * если Collider находится
         * на дочернем объекте врага.
         */
        while (current != null)
        {
            /*
             * GuardEnemy.
             */
            if (current.GetComponent<GuardEnemy>() != null)
            {
                return current.gameObject;
            }

            /*
             * Другие объекты специально
             * не привязываем здесь жёстко
             * к названию класса.
             *
             * Если у Skeleton или Box
             * ReceiveKick находится
             * именно на этом объекте,
             * SendMessage сработает.
             */

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

        /*
         * Если специального объекта не нашли,
         * возвращаем объект Collider.
         *
         * Если у него нет ReceiveKick,
         * ничего страшного:
         * DontRequireReceiver не выдаст ошибку.
         */
        return hit.gameObject;
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