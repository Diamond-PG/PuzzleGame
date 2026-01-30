using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Hearts")]
    [SerializeField] private int maxHearts = 5;
    public int Hearts => hearts;

    [Header("Invulnerability")]
    [SerializeField] private float invulnTime = 0.8f;

    [Header("Links")]
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private HeartsUI heartsUI;

    [Header("Blink Player")]
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkInterval = 0.15f;

    // ===== Sound =====
    [Header("Sound")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip damageClip;
    [SerializeField, Range(0f, 1f)] private float damageVolume = 1f;

    [Header("Death Sound")]
    [SerializeField] private AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;

    // ===== Hit Flash =====
    [Header("Hit Flash")]
    [SerializeField] private bool useHitFlash = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;
    private Color _originalColor;

    // ===== Hit Effect (Particles) =====
    [Header("Hit Effect (Particles)")]
    [SerializeField] private ParticleSystem hitEffectPrefab;
    [SerializeField] private Vector3 hitEffectOffset = Vector3.zero;
    [SerializeField] private bool effectFollowsPlayer = true;
    [SerializeField] private bool destroyEffectAfterPlay = true;

    // ===== Death Effect (Particles) =====
    [Header("Death Effect (Particles)")]
    [SerializeField] private ParticleSystem deathEffectPrefab;
    [SerializeField] private Vector3 deathEffectOffset = Vector3.zero;
    [SerializeField] private bool deathEffectFollowsPlayer = false;
    [SerializeField] private bool destroyDeathEffectAfterPlay = true;

    // ===== Death Settings =====
    [Header("Death Settings")]
    [SerializeField] private float deathRestartDelay = 0.6f;          // минимум пауза перед рестартом
    [SerializeField] private bool hidePlayerOnDeath = true;           // спрятать игрока при смерти (после запуска эффекта)
    [SerializeField] private bool disableMovementOnDeath = true;      // отключить PlayerController на смерть

    private int hearts;
    private bool invulnerable;

    private bool isDead;
    private Coroutine restartRoutine;

    private Coroutine invulnRoutine;
    private Coroutine playerBlinkRoutine;
    private Coroutine hitFlashRoutine;

    private PlayerController playerController;

    private void Awake()
    {
        hearts = maxHearts;

        if (playerRenderer == null)
            playerRenderer = GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
            _originalColor = playerRenderer.color;

        if (heartsUI == null)
            heartsUI = Object.FindFirstObjectByType<HeartsUI>();

        if (heartsUI != null)
            heartsUI.SetHearts(hearts);

        // auto-pick AudioSource if not set
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        // optional: movement script
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead) return;
        if (invulnerable) return;

        // CAMERA SHAKE
        if (CameraShake2D.Instance != null)
            CameraShake2D.Instance.ShakeDefault();

        int prev = hearts;
        hearts = Mathf.Max(0, hearts - amount);
        int lostIndex = prev - 1;

        // UI update + blink heart
        if (heartsUI != null)
        {
            heartsUI.SetHearts(hearts);
            heartsUI.BlinkAndHide(lostIndex);
        }

        // damage sound
        if (sfxSource != null && damageClip != null)
            sfxSource.PlayOneShot(damageClip, damageVolume);

        // hit particles
        SpawnHitEffect();

        // hit flash
        if (useHitFlash && playerRenderer != null)
        {
            if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
            hitFlashRoutine = StartCoroutine(HitFlash());
        }

        // blink player
        if (playerBlinkRoutine != null) StopCoroutine(playerBlinkRoutine);
        playerBlinkRoutine = StartCoroutine(BlinkPlayer());

        // death
        if (hearts <= 0)
        {
            Die();
            return;
        }

        // invulnerability window
        invulnerable = true;
        if (invulnRoutine != null) StopCoroutine(invulnRoutine);
        invulnRoutine = StartCoroutine(InvulnTimer(invulnTime));
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        invulnerable = true;

        // stop routines
        if (invulnRoutine != null) StopCoroutine(invulnRoutine);
        if (playerBlinkRoutine != null) StopCoroutine(playerBlinkRoutine);
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);

        // restore renderer
        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
            playerRenderer.color = _originalColor;
        }

        // disable movement (optional)
        if (disableMovementOnDeath && playerController != null)
            playerController.enabled = false;

        // play death sound
        if (sfxSource != null && deathClip != null)
            sfxSource.PlayOneShot(deathClip, deathVolume);

        // spawn death particles
        SpawnDeathEffect();

        // hide player (optional) - делаем чуть позже, чтобы эффект успел появиться
        if (hidePlayerOnDeath)
            StartCoroutine(HidePlayerNextFrame());

        // restart with delay, but ensure deathClip can finish
        float wait = deathRestartDelay;
        if (deathClip != null) wait = Mathf.Max(wait, deathClip.length);

        if (restartRoutine != null) StopCoroutine(restartRoutine);
        restartRoutine = StartCoroutine(RestartAfterDeath(wait));
    }

    private IEnumerator HidePlayerNextFrame()
    {
        // 1 кадр — чтобы Particles успели заспавниться в позиции игрока
        yield return null;

        if (playerRenderer != null)
            playerRenderer.enabled = false;
    }

    private IEnumerator RestartAfterDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator InvulnTimer(float t)
    {
        yield return new WaitForSeconds(t);
        invulnerable = false;
    }

    private IEnumerator BlinkPlayer()
    {
        if (playerRenderer == null) yield break;

        int toggles = blinkCount * 2;
        for (int i = 0; i < toggles; i++)
        {
            playerRenderer.enabled = !playerRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        playerRenderer.enabled = true;
    }

    private IEnumerator HitFlash()
    {
        if (playerRenderer == null) yield break;

        Color before = playerRenderer.color;

        playerRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        playerRenderer.color = before;
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab == null) return;

        Vector3 spawnPos = transform.position + hitEffectOffset;

        ParticleSystem fx;
        if (effectFollowsPlayer)
        {
            fx = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity, transform);
            fx.transform.localPosition = hitEffectOffset;
        }
        else
        {
            fx = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);
        }

        fx.Play();

        if (destroyEffectAfterPlay)
        {
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax + 0.2f);
        }
    }

    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null) return;

        Vector3 spawnPos = transform.position + deathEffectOffset;

        ParticleSystem fx;
        if (deathEffectFollowsPlayer)
        {
            fx = Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity, transform);
            fx.transform.localPosition = deathEffectOffset;
        }
        else
        {
            fx = Instantiate(deathEffectPrefab, spawnPos, Quaternion.identity);
        }

        fx.Play();

        if (destroyDeathEffectAfterPlay)
        {
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax + 0.2f);
        }
    }
}