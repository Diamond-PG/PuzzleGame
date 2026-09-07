using System.Collections;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    // ============================================================
    // BOX SETTINGS
    // ============================================================

    [Header("BOX SETTINGS")]

    [Tooltip("Сколько ударов нужно для разрушения ящика.")]
    [SerializeField, Min(1)]
    private int hitsToBreak = 2;

    // ============================================================
    // PLAYER
    // ============================================================

    [Header("PLAYER")]

    [SerializeField]
    private string playerTag = "Player";

    [SerializeField]
    private Transform player;

    [SerializeField]
    private Collider2D playerCollider;

    // ============================================================
    // SIDE KICK
    // ============================================================

    [Header("SIDE KICK")]

    [Tooltip(
        "Если включено, обычный удар ногой не повреждает ящик, " +
        "когда игрок стоит сверху на нём."
    )]
    [SerializeField]
    private bool blockKickFromAbove = true;

    [Tooltip(
        "Допуск определения, что ноги игрока находятся на верхней поверхности ящика."
    )]
    [SerializeField, Min(0f)]
    private float playerAboveTolerance = 0.12f;

    // ============================================================
    // TOP LANDING HIT
    // ============================================================

    [Header("TOP LANDING HIT")]

    [Tooltip(
        "Разрешить разбивать ящик приземлением сверху после прыжка."
    )]
    [SerializeField]
    private bool enableTopLandingHit = true;

    [Tooltip(
        "Сколько урона получает ящик при одном приземлении сверху."
    )]
    [SerializeField, Min(1)]
    private int topLandingDamage = 1;

    [Tooltip(
        "Минимальная вертикальная скорость столкновения, " +
        "чтобы обычное вставание на ящик не считалось ударом."
    )]
    [SerializeField, Min(0f)]
    private float minimumTopImpactSpeed = 0.8f;

    [Tooltip(
        "Допуск по высоте для определения контакта именно с верхом ящика."
    )]
    [SerializeField, Min(0f)]
    private float topContactTolerance = 0.12f;

    [Tooltip(
        "Минимальная пауза между ударами сверху. " +
        "Защищает от двойного срабатывания одного приземления."
    )]
    [SerializeField, Min(0f)]
    private float topHitCooldown = 0.15f;

    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("HAPTICS")]

    [SerializeField]
    private bool useHaptics = true;

    [Tooltip("Вибрация при обычном ударе по ящику.")]
    [SerializeField, Range(5, 100)]
    private int firstHitHapticMs = 18;

    [Tooltip("Вибрация непосредственно в момент разрушения ящика.")]
    [SerializeField, Range(5, 200)]
    private int breakHapticMs = 75;

    // ============================================================
    // EFFECTS
    // ============================================================

    [Header("EFFECTS")]

    [SerializeField]
    private GameObject breakEffect;

    [SerializeField]
    private float breakEffectLifetime = 2f;

    // ============================================================
    // AUDIO
    // ============================================================

    [Header("AUDIO")]

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip firstHitSound;

    [SerializeField]
    private AudioClip breakSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float firstHitVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField]
    private float breakVolume = 1f;

    // ============================================================
    // BOX SPRITES
    // ============================================================

    [Header("BOX SPRITES")]

    [SerializeField]
    private Sprite normalSprite;

    [SerializeField]
    private Sprite crackedSprite;

    [SerializeField]
    private Sprite brokenSprite;

    [SerializeField]
    private float brokenSpriteDuration = 0.22f;

    // ============================================================
    // HIT EFFECT
    // ============================================================

    [Header("HIT EFFECT")]

    [SerializeField]
    private float hitScaleMultiplier = 0.9f;

    [SerializeField]
    private float hitEffectDuration = 0.08f;

    // ============================================================
    // FIRST HIT SHAKE
    // ============================================================

    [Header("FIRST HIT SHAKE")]

    [SerializeField]
    private float firstHitShakeDuration = 0.20f;

    [SerializeField]
    private float firstHitShakeAmountX = 0.035f;

    [SerializeField]
    private float firstHitShakeAmountY = 0.008f;

    [SerializeField]
    private float firstHitShakeSpeed = 28f;

    // ============================================================
    // FINAL BREAK SHAKE
    // ============================================================

    [Header("FINAL BREAK SHAKE")]

    [SerializeField]
    private float finalShakeDuration = 0.45f;

    [SerializeField]
    private float finalShakeAmountX = 0.06f;

    [SerializeField]
    private float finalShakeAmountY = 0.01f;

    [SerializeField]
    private float finalShakeSpeed = 35f;

    // ============================================================
    // BROKEN PIECES
    // ============================================================

    [Header("BROKEN PIECES")]

    [SerializeField]
    private GameObject[] woodChips;

    [SerializeField]
    private float chipFallDuration = 0.22f;

    [SerializeField]
    private float chipExtraFallDuration = 0.26f;

    [SerializeField]
    private float chipExtraDropDistance = 0.18f;

    [SerializeField]
    private float chipHorizontalSpread = 0.06f;

    [SerializeField]
    private float chipStayDuration = 0.55f;

    [SerializeField]
    private float chipFadeDuration = 0.25f;

    [SerializeField]
    private float chipEndRotMin = 55f;

    [SerializeField]
    private float chipEndRotMax = 125f;

    [SerializeField]
    private float chipGroundLift = 0.045f;

    // ============================================================
    // GOAL REVEAL
    // ============================================================

    [Header("GOAL REVEAL")]

    [SerializeField]
    private GoalRevealFromBox goalReveal;

    [SerializeField]
    private float goalDetachDelay = 0.25f;

    [SerializeField]
    private bool goalDebugLogs = false;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugHits = false;

    // ============================================================
    // PRIVATE
    // ============================================================

    private int hits;

    private SpriteRenderer boxSpriteRenderer;
    private SpriteRenderer boxBackgroundRenderer;
    private Collider2D boxCollider;

    private Vector3 originalLocalScale;
    private Vector3 originalLocalPosition;

    private float hitEffectTimer;
    private float nextTopHitTime;

    private bool isPlayingHitEffect;
    private bool isBreaking;
    private bool isBusy;

    public bool IsBroken => isBreaking;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        boxSpriteRenderer =
            GetComponent<SpriteRenderer>();

        boxCollider =
            GetComponent<Collider2D>();

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        Transform background =
            transform.Find("Box_Background");

        if (background != null)
        {
            boxBackgroundRenderer =
                background.GetComponent<SpriteRenderer>();
        }

        FindPlayer();

        originalLocalScale =
            transform.localScale;

        originalLocalPosition =
            transform.localPosition;
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        ResetBoxState();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        UpdateHitEffect();

        if (player == null)
        {
            FindPlayer();
        }
    }

    // ============================================================
    // FIND PLAYER
    // ============================================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject == null)
            return;

        player =
            playerObject.transform;

        if (playerCollider == null)
        {
            playerCollider =
                playerObject.GetComponent<Collider2D>();

            if (playerCollider == null)
            {
                playerCollider =
                    playerObject.GetComponentInChildren<Collider2D>();
            }
        }
    }

    // ============================================================
    // RESET
    // ============================================================

    private void ResetBoxState()
    {
        StopAllCoroutines();

        hits = 0;

        isBreaking = false;
        isBusy = false;
        isPlayingHitEffect = false;

        hitEffectTimer = 0f;
        nextTopHitTime = 0f;

        transform.localScale =
            originalLocalScale;

        transform.localPosition =
            originalLocalPosition;

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled =
                true;

            boxSpriteRenderer.color =
                Color.white;

            if (normalSprite != null)
            {
                boxSpriteRenderer.sprite =
                    normalSprite;
            }
        }

        if (boxBackgroundRenderer != null)
        {
            boxBackgroundRenderer.enabled =
                true;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled =
                true;
        }

        if (goalReveal != null)
        {
            goalReveal.HideGoalImmediate();

            if (goalReveal.transform.parent != transform)
            {
                goalReveal.transform.SetParent(
                    transform,
                    true
                );
            }
        }
    }

    // ============================================================
    // LEG ATTACK
    // ============================================================

    public void ReceiveKick(int damage)
    {
        if (isBreaking ||
            isBusy ||
            damage <= 0)
        {
            return;
        }

        /*
         * Если игрок стоит НА ящике,
         * обычная кнопка ноги ящик не повреждает.
         */
        if (blockKickFromAbove &&
            PlayerIsStandingAboveBox())
        {
            if (debugHits)
            {
                Debug.Log(
                    "[BOX] Kick blocked: player is above box.",
                    this
                );
            }

            return;
        }

        ReceiveHit(damage);
    }

    // ============================================================
    // GENERIC HIT
    // ============================================================

    /*
     * Универсальный метод нанесения урона.
     *
     * Его могут использовать:
     * - нога;
     * - приземление сверху;
     * - меч;
     * - палка;
     * - топор;
     * - любое будущее оружие.
     */
    public void ReceiveHit(int damage)
    {
        if (isBreaking ||
            isBusy ||
            damage <= 0)
        {
            return;
        }

        isBusy = true;

        StartCoroutine(
            ReceiveHitRoutine(damage)
        );
    }

    // ============================================================
    // PLAYER ABOVE CHECK
    // ============================================================

    private bool PlayerIsStandingAboveBox()
    {
        if (boxCollider == null)
            return false;

        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
            return false;

        float boxTop =
            boxCollider.bounds.max.y;

        if (playerCollider != null)
        {
            float playerBottom =
                playerCollider.bounds.min.y;

            bool playerFeetAreAtTop =
                playerBottom >=
                boxTop -
                playerAboveTolerance;

            bool playerCenterIsAbove =
                playerCollider.bounds.center.y >
                boxCollider.bounds.center.y;

            return
                playerFeetAreAtTop &&
                playerCenterIsAbove;
        }

        return
            player.position.y >
            boxCollider.bounds.center.y;
    }

    // ============================================================
    // TOP LANDING COLLISION
    // ============================================================

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (!enableTopLandingHit ||
            isBreaking ||
            isBusy)
        {
            return;
        }

        if (Time.time <
            nextTopHitTime)
        {
            return;
        }

        if (!IsPlayerCollision(collision))
        {
            return;
        }

        /*
         * ВАЖНО:
         * Сначала проверяем, что игрок действительно
         * находится НАД ящиком.
         *
         * Это предотвращает ложный удар,
         * когда игрок прыгает рядом и касается
         * боковой стенки ящика.
         */
        if (!CollisionIsOnTop(collision))
        {
            return;
        }

        float verticalImpactSpeed =
            Mathf.Abs(
                collision.relativeVelocity.y
            );

        if (verticalImpactSpeed <
            minimumTopImpactSpeed)
        {
            if (debugHits)
            {
                Debug.Log(
                    "[BOX] Top contact ignored. Impact speed = " +
                    verticalImpactSpeed.ToString("F2"),
                    this
                );
            }

            return;
        }

        nextTopHitTime =
            Time.time +
            topHitCooldown;

        if (debugHits)
        {
            Debug.Log(
                "[BOX] TRUE TOP LANDING. Damage = " +
                topLandingDamage +
                ", speed = " +
                verticalImpactSpeed.ToString("F2"),
                this
            );
        }

        ReceiveHit(
            topLandingDamage
        );
    }

    // ============================================================
    // IS PLAYER COLLISION
    // ============================================================

    private bool IsPlayerCollision(
        Collision2D collision
    )
    {
        if (collision == null ||
            collision.collider == null)
        {
            return false;
        }

        Transform hitTransform =
            collision.collider.transform;

        if (hitTransform.CompareTag(playerTag))
        {
            return true;
        }

        Transform root =
            hitTransform.root;

        if (root != null &&
            root.CompareTag(playerTag))
        {
            return true;
        }

        if (player != null)
        {
            return
                hitTransform == player ||
                hitTransform.IsChildOf(player);
        }

        return false;
    }

    // ============================================================
    // IS COLLISION REALLY ON TOP
    // ============================================================

    private bool CollisionIsOnTop(
        Collision2D collision
    )
    {
        if (boxCollider == null ||
            collision == null ||
            collision.collider == null)
        {
            return false;
        }

        Collider2D otherCollider =
            collision.collider;

        /*
         * Ноги игрока должны находиться
         * примерно на уровне верхней поверхности ящика.
         *
         * Если игрок ударился в БОК ящика,
         * его нижняя граница будет слишком низко,
         * и такой контакт будет отклонён.
         */
        float boxTop =
            boxCollider.bounds.max.y;

        float playerBottom =
            otherCollider.bounds.min.y;

        if (playerBottom <
            boxTop -
            topContactTolerance)
        {
            return false;
        }

        /*
         * Центр игрока обязательно должен
         * находиться выше центра ящика.
         */
        if (otherCollider.bounds.center.y <=
            boxCollider.bounds.center.y)
        {
            return false;
        }

        /*
         * Дополнительно проверяем нормаль контакта.
         *
         * Для настоящего приземления игрока
         * на верх ящика нормаль со стороны ящика
         * должна в основном смотреть вниз.
         *
         * Боковые контакты сюда не проходят.
         */
        for (int i = 0;
             i < collision.contactCount;
             i++)
        {
            ContactPoint2D contact =
                collision.GetContact(i);

            bool contactNearTop =
                contact.point.y >=
                boxTop -
                topContactTolerance;

            bool topNormal =
                contact.normal.y < -0.5f;

            if (contactNearTop &&
                topNormal)
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // RECEIVE HIT ROUTINE
    // ============================================================

    private IEnumerator ReceiveHitRoutine(
        int damage
    )
    {
        if (isBreaking)
        {
            isBusy = false;
            yield break;
        }

        hits +=
            Mathf.Max(
                1,
                damage
            );

        hits =
            Mathf.Min(
                hits,
                hitsToBreak
            );

        if (debugHits)
        {
            Debug.Log(
                "[BOX] Hit: " +
                hits +
                " / " +
                hitsToBreak,
                this
            );
        }

        PlayHitEffect();

        if (hits <
            hitsToBreak)
        {
            PlayFirstHitHaptic();
            PlayFirstHitSound();

            yield return StartCoroutine(
                FirstHitSequence()
            );
        }
        else
        {
            yield return StartCoroutine(
                FinalBreakSequence()
            );
        }
    }

    // ============================================================
    // HAPTICS
    // ============================================================

    private void PlayFirstHitHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            firstHitHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }

    private void PlayBreakHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            breakHapticMs,
            MicroHaptics.IOSHapticStyle.Heavy
        );
    }

    // ============================================================
    // HIT EFFECT
    // ============================================================

    private void PlayHitEffect()
    {
        transform.localScale =
            originalLocalScale *
            hitScaleMultiplier;

        hitEffectTimer =
            hitEffectDuration;

        isPlayingHitEffect =
            true;
    }

    private void UpdateHitEffect()
    {
        if (!isPlayingHitEffect)
            return;

        hitEffectTimer -=
            Time.deltaTime;

        if (hitEffectTimer <= 0f)
        {
            transform.localScale =
                originalLocalScale;

            isPlayingHitEffect =
                false;
        }
    }

    // ============================================================
    // AUDIO
    // ============================================================

    private void PlayFirstHitSound()
    {
        if (audioSource == null ||
            firstHitSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            firstHitSound,
            firstHitVolume
        );
    }

    private void PlayBreakSound()
    {
        if (audioSource == null ||
            breakSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            breakSound,
            breakVolume
        );
    }

    // ============================================================
    // FIRST HIT
    // ============================================================

    private IEnumerator FirstHitSequence()
    {
        yield return StartCoroutine(
            ShakeBox(
                firstHitShakeDuration,
                firstHitShakeAmountX,
                firstHitShakeAmountY,
                firstHitShakeSpeed
            )
        );

        if (boxSpriteRenderer != null &&
            crackedSprite != null)
        {
            boxSpriteRenderer.sprite =
                crackedSprite;
        }

        transform.localPosition =
            originalLocalPosition;

        transform.localScale =
            originalLocalScale;

        isBusy = false;
    }

    // ============================================================
    // FINAL BREAK
    // ============================================================

    private IEnumerator FinalBreakSequence()
    {
        isBreaking = true;

        /*
         * КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ:
         *
         * Ящик уже получил последний удар,
         * поэтому физически его больше нет.
         *
         * Отключаем Collider СРАЗУ.
         *
         * Если Player стоял сверху,
         * он сразу начинает падать,
         * не ожидая окончания shake-анимации.
         */
        if (boxCollider != null)
        {
            boxCollider.enabled =
                false;
        }

        yield return StartCoroutine(
            ShakeBox(
                finalShakeDuration,
                finalShakeAmountX,
                finalShakeAmountY,
                finalShakeSpeed
            )
        );

        transform.localPosition =
            originalLocalPosition;

        transform.localScale =
            originalLocalScale;

        if (boxBackgroundRenderer != null)
        {
            boxBackgroundRenderer.enabled =
                false;
        }

        if (boxSpriteRenderer != null)
        {
            if (brokenSprite != null)
            {
                boxSpriteRenderer.sprite =
                    brokenSprite;
            }
            else if (crackedSprite != null)
            {
                boxSpriteRenderer.sprite =
                    crackedSprite;
            }
        }

        PlayBreakHaptic();
        PlayBreakSound();

        SpawnBreakEffect();
        SpawnBrokenPieces();

        RevealGoalFromBox();

        yield return new WaitForSeconds(
            brokenSpriteDuration
        );

        HideAndFinishBreak();
    }

    // ============================================================
    // GOAL REVEAL
    // ============================================================

    private void RevealGoalFromBox()
    {
        if (goalReveal == null)
        {
            if (goalDebugLogs)
            {
                Debug.LogWarning(
                    "GoalRevealFromBox не назначен в BreakableBox."
                );
            }

            return;
        }

        goalReveal.RevealGoal();

        StartCoroutine(
            DetachGoalAfterDelay()
        );
    }

    private IEnumerator DetachGoalAfterDelay()
    {
        yield return new WaitForSeconds(
            goalDetachDelay
        );

        if (goalReveal != null)
        {
            goalReveal.transform.SetParent(
                null,
                true
            );

            if (goalDebugLogs)
            {
                Debug.Log(
                    "Goal отсоединён от Regular box."
                );
            }
        }
    }

    // ============================================================
    // SHAKE
    // ============================================================

    private IEnumerator ShakeBox(
        float duration,
        float amountX,
        float amountY,
        float speed
    )
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer +=
                Time.deltaTime;

            float x =
                Mathf.Sin(
                    timer *
                    speed
                ) *
                amountX;

            float y =
                Mathf.Cos(
                    timer *
                    speed *
                    0.5f
                ) *
                amountY;

            transform.localPosition =
                originalLocalPosition +
                new Vector3(
                    x,
                    y,
                    0f
                );

            yield return null;
        }

        transform.localPosition =
            originalLocalPosition;
    }

    // ============================================================
    // FINISH BREAK
    // ============================================================

    private void HideAndFinishBreak()
    {
        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled =
                false;
        }

        if (boxBackgroundRenderer != null)
        {
            boxBackgroundRenderer.enabled =
                false;
        }

        /*
         * Collider уже отключён
         * в самом начале FinalBreakSequence.
         * Здесь оставляем страховочную проверку.
         */
        if (boxCollider != null)
        {
            boxCollider.enabled =
                false;
        }

        if (debugHits)
        {
            Debug.Log(
                "[BOX] Box broken!",
                this
            );
        }

        float totalChipTime =
            chipFallDuration +
            chipExtraFallDuration +
            chipStayDuration +
            chipFadeDuration +
            0.15f;

        Destroy(
            gameObject,
            totalChipTime
        );
    }

    // ============================================================
    // BREAK EFFECT
    // ============================================================

    private void SpawnBreakEffect()
    {
        if (breakEffect == null)
            return;

        GameObject effect =
            Instantiate(
                breakEffect,
                transform.position,
                Quaternion.identity
            );

        ParticleSystem[] particleSystems =
            effect.GetComponentsInChildren<ParticleSystem>(
                true
            );

        for (int i = 0;
             i < particleSystems.Length;
             i++)
        {
            particleSystems[i].Clear();
            particleSystems[i].Play();
        }

        Destroy(
            effect,
            breakEffectLifetime
        );
    }

    // ============================================================
    // BROKEN PIECES
    // ============================================================

    private void SpawnBrokenPieces()
    {
        if (woodChips == null ||
            woodChips.Length == 0)
        {
            return;
        }

        int count =
            Mathf.Min(
                woodChips.Length,
                6
            );

        Vector3[] startOffsets =
        {
            new Vector3(-0.16f,  0.08f, 0f),
            new Vector3( 0.00f,  0.09f, 0f),
            new Vector3( 0.16f,  0.08f, 0f),
            new Vector3(-0.14f, -0.02f, 0f),
            new Vector3( 0.02f, -0.03f, 0f),
            new Vector3( 0.15f, -0.01f, 0f)
        };

        Vector3[] endOffsets =
        {
            new Vector3(-0.24f, -0.10f, 0f),
            new Vector3( 0.00f, -0.12f, 0f),
            new Vector3( 0.24f, -0.10f, 0f),
            new Vector3(-0.18f, -0.22f, 0f),
            new Vector3( 0.02f, -0.24f, 0f),
            new Vector3( 0.19f, -0.21f, 0f)
        };

        for (int i = 0;
             i < count;
             i++)
        {
            GameObject prefab =
                woodChips[i];

            if (prefab == null)
                continue;

            Vector3 startPosition =
                transform.position +
                startOffsets[i];

            Vector3 endPosition =
                transform.position +
                endOffsets[i];

            Quaternion startRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Random.Range(
                        -10f,
                        10f
                    )
                );

            GameObject chip =
                Instantiate(
                    prefab,
                    startPosition,
                    startRotation
                );

            Rigidbody2D chipRigidbody =
                chip.GetComponent<Rigidbody2D>();

            if (chipRigidbody != null)
            {
                chipRigidbody.simulated =
                    false;
            }

            Collider2D chipCollider =
                chip.GetComponent<Collider2D>();

            if (chipCollider != null)
            {
                chipCollider.enabled =
                    false;
            }

            StartCoroutine(
                AnimateChip(
                    chip,
                    startPosition,
                    endPosition,
                    i
                )
            );
        }
    }

    // ============================================================
    // CHIP ANIMATION
    // ============================================================

    private IEnumerator AnimateChip(
        GameObject chip,
        Vector3 startPosition,
        Vector3 endPosition,
        int index
    )
    {
        if (chip == null)
            yield break;

        SpriteRenderer spriteRenderer =
            chip.GetComponent<SpriteRenderer>();

        Collider2D chipCollider =
            chip.GetComponent<Collider2D>();

        Rigidbody2D chipRigidbody =
            chip.GetComponent<Rigidbody2D>();

        Color startColor =
            Color.white;

        if (spriteRenderer != null)
        {
            startColor =
                spriteRenderer.color;
        }

        float startRotation =
            chip.transform.eulerAngles.z;

        float middleRotation =
            startRotation +
            Random.Range(
                -18f,
                18f
            );

        float endRotation =
            index % 2 == 0
                ? -Random.Range(
                    chipEndRotMin,
                    chipEndRotMax
                )
                : Random.Range(
                    chipEndRotMin,
                    chipEndRotMax
                );

        float timer = 0f;

        while (timer <
               chipFallDuration)
        {
            if (chip == null)
                yield break;

            timer +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    Mathf.Max(
                        0.01f,
                        chipFallDuration
                    )
                );

            chip.transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress
                );

            chip.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        startRotation,
                        middleRotation,
                        progress
                    )
                );

            yield return null;
        }

        Vector3 extraFallStart =
            chip.transform.position;

        Vector3 extraFallEnd =
            extraFallStart +
            new Vector3(
                Random.Range(
                    -chipHorizontalSpread,
                    chipHorizontalSpread
                ),
                -chipExtraDropDistance +
                    chipGroundLift,
                0f
            );

        float fallTimer = 0f;

        while (fallTimer <
               chipExtraFallDuration)
        {
            if (chip == null)
                yield break;

            fallTimer +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    fallTimer /
                    Mathf.Max(
                        0.01f,
                        chipExtraFallDuration
                    )
                );

            float curvedProgress =
                progress *
                progress;

            chip.transform.position =
                Vector3.Lerp(
                    extraFallStart,
                    extraFallEnd,
                    curvedProgress
                );

            chip.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        middleRotation,
                        endRotation,
                        progress
                    )
                );

            yield return null;
        }

        chip.transform.position =
            extraFallEnd;

        chip.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                endRotation
            );

        if (chipStayDuration > 0f)
        {
            yield return new WaitForSeconds(
                chipStayDuration
            );
        }

        float fadeTimer = 0f;

        while (fadeTimer <
               chipFadeDuration)
        {
            if (chip == null)
                yield break;

            fadeTimer +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    fadeTimer /
                    Mathf.Max(
                        0.01f,
                        chipFadeDuration
                    )
                );

            if (spriteRenderer != null)
            {
                Color color =
                    startColor;

                color.a =
                    Mathf.Lerp(
                        startColor.a,
                        0f,
                        progress
                    );

                spriteRenderer.color =
                    color;
            }

            yield return null;
        }

        if (chipCollider != null)
        {
            chipCollider.enabled =
                false;
        }

        if (chipRigidbody != null)
        {
            chipRigidbody.simulated =
                false;
        }

        Destroy(chip);
    }
}