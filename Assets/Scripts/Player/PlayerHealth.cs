using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Hearts")]
    [SerializeField] private int maxHearts = 5;

    public int Hearts => hearts;
    public int MaxHearts => maxHearts;
    public bool HasMissingHearts => hearts < maxHearts;
    public bool IsDead => isDead;
    public bool IsInvulnerable => invulnerable;

    [Header("Invulnerability")]
    [SerializeField] private float invulnTime = 0.8f;

    [Header("Links")]
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private HeartsUI heartsUI;
    [SerializeField] private PlayerVisual playerVisual;

    [Header("Movement Links")]
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private ClimbHook climbHook;

    [Header("Blink Player")]
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkInterval = 0.15f;

    [Header("Sound")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip damageClip;
    [SerializeField, Range(0f, 1f)] private float damageVolume = 1f;

    [Header("Death Sound")]
    [SerializeField] private AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;

    [Header("Hit Flash")]
    [SerializeField] private bool useHitFlash = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;

    [Header("Hit Effect (Particles)")]
    [SerializeField] private ParticleSystem hitEffectPrefab;
    [SerializeField] private Vector3 hitEffectOffset = Vector3.zero;
    [SerializeField] private bool effectFollowsPlayer = true;
    [SerializeField] private bool destroyEffectAfterPlay = true;

    [Header("Hit Effect Amount")]
    [SerializeField, Min(1)] private int hitEffectSpawnCount = 2;
    [SerializeField, Min(0f)] private float hitEffectSpawnDelay = 0.03f;

    [Header("Hit Effect Spread")]
    [SerializeField] private Vector2 hitEffectRandomOffsetX =
        new Vector2(-0.15f, 0.15f);

    [SerializeField] private Vector2 hitEffectRandomOffsetY =
        new Vector2(-0.10f, 0.18f);

    [SerializeField] private Vector2 hitEffectRandomOffsetZ =
        new Vector2(0f, 0f);

    [Header("Hit Effect Rotation")]
    [SerializeField] private bool randomizeHitEffectRotation = true;

    [SerializeField] private Vector2 hitEffectRandomRotationZ =
        new Vector2(-25f, 25f);

    [Header("Hit Effect Scale")]
    [SerializeField] private bool randomizeHitEffectScale = true;

    [SerializeField] private Vector2 hitEffectRandomScale =
        new Vector2(0.9f, 1.25f);

    [Header("Death Effect (Particles)")]
    [SerializeField] private ParticleSystem deathEffectPrefab;
    [SerializeField] private Vector3 deathEffectOffset = Vector3.zero;
    [SerializeField] private bool deathEffectFollowsPlayer = false;
    [SerializeField] private bool destroyDeathEffectAfterPlay = true;

    [Header("Death Settings")]
    [SerializeField] private float deathRestartDelay = 0.6f;
    [SerializeField] private bool hidePlayerOnDeath = true;
    [SerializeField] private bool disableMovementOnDeath = true;

    [Tooltip(
        "Полностью отключает физику Rigidbody2D после смерти, " +
        "чтобы невидимый игрок не мог продолжать падать, " +
        "прыгать или взаимодействовать с объектами."
    )]
    [SerializeField] private bool disablePhysicsOnDeath = true;

    private int hearts;
    private bool invulnerable;
    private bool isDead;

    private Color originalColor;

    private Coroutine restartRoutine;
    private Coroutine invulnRoutine;
    private Coroutine playerBlinkRoutine;
    private Coroutine hitFlashRoutine;
    private Coroutine hitEffectRoutine;

    private void Awake()
    {
        hearts = maxHearts;

        if (playerRenderer == null)
            playerRenderer = GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
            originalColor = playerRenderer.color;

        if (heartsUI == null)
            heartsUI = Object.FindFirstObjectByType<HeartsUI>();

        if (heartsUI != null)
        {
            heartsUI.SetHearts(hearts);
            heartsUI.SetBonusHearts(0);
        }

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (playerVisual == null)
            playerVisual = GetComponent<PlayerVisual>();

        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody2D>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();

        if (climbHook == null)
            climbHook = GetComponent<ClimbHook>();
    }

    /// <summary>
    /// Восстанавливает обычные сердца,
    /// но не превышает maxHearts.
    /// </summary>
    public bool TryRestoreHeart(int amount = 1)
    {
        if (isDead)
            return false;

        if (amount <= 0)
            return false;

        if (hearts >= maxHearts)
            return false;

        int previousHearts = hearts;

        hearts = Mathf.Clamp(
            hearts + amount,
            0,
            maxHearts
        );

        bool heartWasRestored =
            hearts > previousHearts;

        if (heartWasRestored &&
            heartsUI != null)
        {
            heartsUI.SetHearts(hearts);
            heartsUI.PlayRegularHeartPickupPop();
        }

        return heartWasRestored;
    }

    /// <summary>
    /// Обычный урон от ловушек и других объектов.
    /// Использует стандартный damageClip.
    /// </summary>
    public void TakeDamage(int amount = 1)
    {
        ApplyDamage(
            amount,
            true
        );
    }

    /// <summary>
    /// Урон от падения.
    /// Стандартный damageClip не воспроизводится,
    /// потому что звук приземления с Ow
    /// запускает PlayerLanding.
    /// </summary>
    public void TakeFallDamage(int amount)
    {
        ApplyDamage(
            amount,
            false
        );
    }

    private void ApplyDamage(
        int amount,
        bool playDamageSound
    )
    {
        if (isDead)
            return;

        if (invulnerable)
            return;

        if (amount <= 0)
            return;

        int remainingDamage = amount;
        int usedBonusHearts = 0;

        /*
         * Сначала по одной единице расходуются
         * все доступные бонусные сердца.
         */
        if (heartsUI != null)
        {
            while (remainingDamage > 0 &&
                   heartsUI.TryUseBonusHeart())
            {
                usedBonusHearts++;
                remainingDamage--;
            }
        }

        int previousHearts = hearts;
        int lostRegularHearts = 0;

        /*
         * Оставшийся урон снимается
         * с обычных сердец.
         */
        if (remainingDamage > 0)
        {
            hearts = Mathf.Max(
                0,
                hearts - remainingDamage
            );

            lostRegularHearts =
                previousHearts - hearts;
        }

        /*
         * Если по какой-то причине ничего не снялось,
         * обратная связь не запускается.
         */
        if (usedBonusHearts <= 0 &&
            lostRegularHearts <= 0)
        {
            return;
        }

        if (CameraShake2D.Instance != null)
            CameraShake2D.Instance.ShakeDefault();

        /*
         * Обновляем и анимируем обычные сердца.
         */
        if (lostRegularHearts > 0 &&
            heartsUI != null)
        {
            heartsUI.SetHearts(hearts);

            int firstLostIndex =
                maxHearts - previousHearts;

            heartsUI.BlinkAndHideMultiple(
                firstLostIndex,
                lostRegularHearts
            );
        }

        PlayDamageFeedback(
            playDamageSound
        );

        if (hearts <= 0)
        {
            Die();
            return;
        }

        invulnerable = true;

        if (invulnRoutine != null)
            StopCoroutine(invulnRoutine);

        invulnRoutine = StartCoroutine(
            InvulnTimer(invulnTime)
        );
    }

    private void PlayDamageFeedback(
        bool playDamageSound
    )
    {
        if (playDamageSound &&
            sfxSource != null &&
            damageClip != null)
        {
            sfxSource.PlayOneShot(
                damageClip,
                damageVolume
            );
        }

        if (playerVisual != null)
            playerVisual.PlayHurtVisual();

        if (hitEffectRoutine != null)
            StopCoroutine(hitEffectRoutine);

        hitEffectRoutine = StartCoroutine(
            SpawnHitEffectRoutine()
        );

        if (useHitFlash &&
            playerRenderer != null)
        {
            if (hitFlashRoutine != null)
                StopCoroutine(hitFlashRoutine);

            hitFlashRoutine = StartCoroutine(
                HitFlash()
            );
        }

        if (playerBlinkRoutine != null)
            StopCoroutine(playerBlinkRoutine);

        playerBlinkRoutine = StartCoroutine(
            BlinkPlayer()
        );
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        invulnerable = true;

        if (invulnRoutine != null)
            StopCoroutine(invulnRoutine);

        if (playerBlinkRoutine != null)
            StopCoroutine(playerBlinkRoutine);

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);

        if (hitEffectRoutine != null)
            StopCoroutine(hitEffectRoutine);

        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
            playerRenderer.color = originalColor;
        }

        if (disableMovementOnDeath)
            DisablePlayerAfterDeath();

        if (sfxSource != null &&
            deathClip != null)
        {
            sfxSource.PlayOneShot(
                deathClip,
                deathVolume
            );
        }

        SpawnDeathEffect();

        if (hidePlayerOnDeath)
            StartCoroutine(HidePlayerNextFrame());

        float wait = deathRestartDelay;

        if (deathClip != null)
        {
            wait = Mathf.Max(
                wait,
                deathClip.length
            );
        }

        if (restartRoutine != null)
            StopCoroutine(restartRoutine);

        restartRoutine = StartCoroutine(
            RestartAfterDeath(wait)
        );
    }

    private void DisablePlayerAfterDeath()
    {
        /*
         * Сначала корректно блокируем движение
         * через сам PlayerController.
         */
        if (playerController != null)
        {
            playerController.LockMovement(true);
            playerController.enabled = false;
        }

        /*
         * Отдельно отключаем прыжок.
         */
        if (playerJump != null)
            playerJump.enabled = false;

        /*
         * Отдельно отключаем цепляние и лазание.
         */
        if (climbHook != null)
            climbHook.enabled = false;

        /*
         * Полностью останавливаем Rigidbody2D.
         */
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;

            playerRigidbody.angularVelocity =
                0f;

            if (disablePhysicsOnDeath)
                playerRigidbody.simulated = false;
        }
    }

    private IEnumerator HidePlayerNextFrame()
    {
        yield return null;

        if (playerRenderer != null)
            playerRenderer.enabled = false;
    }

    private IEnumerator RestartAfterDeath(
        float delay
    )
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .buildIndex
        );
    }

    private IEnumerator InvulnTimer(
        float duration
    )
    {
        yield return new WaitForSeconds(duration);

        invulnerable = false;
        invulnRoutine = null;
    }

    private IEnumerator BlinkPlayer()
    {
        if (playerRenderer == null)
            yield break;

        int toggles =
            blinkCount * 2;

        for (int i = 0;
             i < toggles;
             i++)
        {
            playerRenderer.enabled =
                !playerRenderer.enabled;

            yield return new WaitForSeconds(
                blinkInterval
            );
        }

        playerRenderer.enabled = true;
        playerBlinkRoutine = null;
    }

    private IEnumerator HitFlash()
    {
        if (playerRenderer == null)
            yield break;

        Color colorBeforeFlash =
            playerRenderer.color;

        playerRenderer.color =
            flashColor;

        yield return new WaitForSeconds(
            flashDuration
        );

        playerRenderer.color =
            colorBeforeFlash;

        hitFlashRoutine = null;
    }

    private IEnumerator SpawnHitEffectRoutine()
    {
        if (hitEffectPrefab == null)
            yield break;

        int spawnCount =
            Mathf.Max(
                1,
                hitEffectSpawnCount
            );

        for (int i = 0;
             i < spawnCount;
             i++)
        {
            SpawnSingleHitEffect();

            if (i < spawnCount - 1 &&
                hitEffectSpawnDelay > 0f)
            {
                yield return new WaitForSeconds(
                    hitEffectSpawnDelay
                );
            }
        }

        hitEffectRoutine = null;
    }

    private void SpawnSingleHitEffect()
    {
        Vector3 randomOffset =
            new Vector3(
                Random.Range(
                    hitEffectRandomOffsetX.x,
                    hitEffectRandomOffsetX.y
                ),
                Random.Range(
                    hitEffectRandomOffsetY.x,
                    hitEffectRandomOffsetY.y
                ),
                Random.Range(
                    hitEffectRandomOffsetZ.x,
                    hitEffectRandomOffsetZ.y
                )
            );

        Vector3 spawnPosition =
            transform.position +
            hitEffectOffset +
            randomOffset;

        Quaternion spawnRotation =
            Quaternion.identity;

        if (randomizeHitEffectRotation)
        {
            float randomZ =
                Random.Range(
                    hitEffectRandomRotationZ.x,
                    hitEffectRandomRotationZ.y
                );

            spawnRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    randomZ
                );
        }

        ParticleSystem effect;

        if (effectFollowsPlayer)
        {
            effect = Instantiate(
                hitEffectPrefab,
                spawnPosition,
                spawnRotation,
                transform
            );

            effect.transform.localPosition =
                hitEffectOffset +
                randomOffset;

            effect.transform.localRotation =
                spawnRotation;
        }
        else
        {
            effect = Instantiate(
                hitEffectPrefab,
                spawnPosition,
                spawnRotation
            );
        }

        if (randomizeHitEffectScale)
        {
            float randomScale =
                Random.Range(
                    hitEffectRandomScale.x,
                    hitEffectRandomScale.y
                );

            effect.transform.localScale =
                Vector3.one * randomScale;
        }

        effect.Play();

        if (destroyEffectAfterPlay)
        {
            float destroyDelay =
                effect.main.duration +
                effect.main
                    .startLifetime
                    .constantMax +
                0.2f;

            Destroy(
                effect.gameObject,
                destroyDelay
            );
        }
    }

    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null)
            return;

        Vector3 spawnPosition =
            transform.position +
            deathEffectOffset;

        ParticleSystem effect;

        if (deathEffectFollowsPlayer)
        {
            effect = Instantiate(
                deathEffectPrefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

            effect.transform.localPosition =
                deathEffectOffset;
        }
        else
        {
            effect = Instantiate(
                deathEffectPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }

        effect.Play();

        if (destroyDeathEffectAfterPlay)
        {
            float destroyDelay =
                effect.main.duration +
                effect.main
                    .startLifetime
                    .constantMax +
                0.2f;

            Destroy(
                effect.gameObject,
                destroyDelay
            );
        }
    }
}