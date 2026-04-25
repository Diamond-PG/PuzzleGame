using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BreakableMiddleBox : MonoBehaviour
{
    [Header("Box Settings")]
    public int hitsToBreak = 3;
    public Transform player;
    public float interactDistance = 1.5f;

    [Header("Effects")]
    public GameObject breakEffect;
    public float breakEffectLifetime = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip firstHitSound;
    public AudioClip secondHitSound;
    public AudioClip breakSound;
    [Range(0f, 1f)] public float firstHitVolume = 1f;
    [Range(0f, 1f)] public float secondHitVolume = 1f;
    [Range(0f, 1f)] public float breakVolume = 1f;

    [Header("Box Sprites")]
    public Sprite normalSprite;
    public Sprite crackedSprite;
    public Sprite crackedSprite2;
    public Sprite brokenSprite;
    public float brokenSpriteDuration = 0.22f;

    [Header("Hit Effect")]
    public float hitScaleMultiplier = 0.9f;
    public float hitEffectDuration = 0.08f;

    [Header("First Hit Shake")]
    public float firstHitShakeDuration = 0.20f;
    public float firstHitShakeAmountX = 0.035f;
    public float firstHitShakeAmountY = 0.008f;
    public float firstHitShakeSpeed = 28f;

    [Header("Second Hit Shake")]
    public float secondHitShakeDuration = 0.25f;
    public float secondHitShakeAmountX = 0.045f;
    public float secondHitShakeAmountY = 0.010f;
    public float secondHitShakeSpeed = 30f;

    [Header("Final Break Shake")]
    public float finalShakeDuration = 0.45f;
    public float finalShakeAmountX = 0.06f;
    public float finalShakeAmountY = 0.01f;
    public float finalShakeSpeed = 35f;

    [Header("Broken Pieces")]
    public GameObject[] woodChips;

    [Tooltip("Время первого осыпания щепки")]
    public float chipFallDuration = 0.22f;

    [Tooltip("Время дополнительного падения щепки вниз")]
    public float chipExtraFallDuration = 0.26f;

    [Tooltip("Насколько ещё щепка опускается вниз после первого падения")]
    public float chipExtraDropDistance = 0.18f;

    [Tooltip("Разброс по X при финальном падении")]
    public float chipHorizontalSpread = 0.06f;

    [Tooltip("Сколько щепки лежат на полу до исчезновения")]
    public float chipStayDuration = 0.55f;

    [Tooltip("Сколько времени щепки плавно исчезают")]
    public float chipFadeDuration = 0.25f;

    [Tooltip("Минимальный финальный угол, когда щепка ложится плашмя")]
    public float chipEndRotMin = 55f;

    [Tooltip("Максимальный финальный угол, когда щепка ложится плашмя")]
    public float chipEndRotMax = 125f;

    [Tooltip("Небольшой подъём щепок, чтобы не проваливались визуально в пол")]
    public float chipGroundLift = 0.045f;

    private int hits = 0;
    private Camera mainCamera;

    private SpriteRenderer boxSpriteRenderer;
    private SpriteRenderer boxBackgroundRenderer;
    private Collider2D boxCollider;

    private Vector3 originalLocalScale;
    private Vector3 originalLocalPosition;

    private float hitEffectTimer = 0f;
    private bool isPlayingHitEffect = false;
    private bool isBreaking = false;
    private bool isBusy = false;

    private void Awake()
    {
        mainCamera = Camera.main;

        boxSpriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        Transform bg = transform.Find("Box_Background");
        if (bg != null)
            boxBackgroundRenderer = bg.GetComponent<SpriteRenderer>();

        originalLocalScale = transform.localScale;
        originalLocalPosition = transform.localPosition;
    }

    private void Start()
    {
        ResetBoxState();
    }

    private void OnEnable()
    {
        ResetBoxState();
    }

    private void ResetBoxState()
    {
        StopAllCoroutines();

        hits = 0;
        isBreaking = false;
        isBusy = false;
        isPlayingHitEffect = false;
        hitEffectTimer = 0f;

        transform.localScale = originalLocalScale;
        transform.localPosition = originalLocalPosition;

        if (boxSpriteRenderer != null)
        {
            boxSpriteRenderer.enabled = true;
            boxSpriteRenderer.color = Color.white;

            if (normalSprite != null)
                boxSpriteRenderer.sprite = normalSprite;
        }

        if (boxBackgroundRenderer != null)
            boxBackgroundRenderer.enabled = true;

        if (boxCollider != null)
            boxCollider.enabled = true;
    }

    private void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        UpdateHitEffect();

        if (isBreaking || isBusy || mainCamera == null || boxCollider == null)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            TryHitBox(screenPos);
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            TryHitBox(touchPos);
        }
    }

    private void TryHitBox(Vector2 screenPos)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 point2D = new Vector2(worldPos.x, worldPos.y);

        Collider2D hit = Physics2D.OverlapPoint(point2D);

        if (hit != boxCollider)
            return;

        if (player == null)
        {
            Debug.LogWarning("Player не назначен в BreakableMiddleBox!");
            return;
        }

        float distance = Vector2.Distance(player.position, transform.position);

        if (distance > interactDistance)
        {
            Debug.Log("Слишком далеко от среднего ящика");
            return;
        }

        hits++;
        Debug.Log("Middle box hit: " + hits + " / " + hitsToBreak);

        PlayHitEffect();

        if (hits < hitsToBreak)
        {
            if (hits == 1)
                PlayFirstHitSound();
            else if (hits == 2)
                PlaySecondHitSound();

            StartCoroutine(HitSequence());
        }
        else
        {
            StartCoroutine(FinalBreakSequence());
        }
    }

    private void PlayHitEffect()
    {
        transform.localScale = originalLocalScale * hitScaleMultiplier;
        hitEffectTimer = hitEffectDuration;
        isPlayingHitEffect = true;
    }

    private void UpdateHitEffect()
    {
        if (!isPlayingHitEffect)
            return;

        hitEffectTimer -= Time.deltaTime;

        if (hitEffectTimer <= 0f)
        {
            transform.localScale = originalLocalScale;
            isPlayingHitEffect = false;
        }
    }

    private void PlayFirstHitSound()
    {
        if (audioSource == null || firstHitSound == null)
            return;

        audioSource.PlayOneShot(firstHitSound, firstHitVolume);
    }

    private void PlaySecondHitSound()
    {
        if (audioSource == null || secondHitSound == null)
            return;

        audioSource.PlayOneShot(secondHitSound, secondHitVolume);
    }

    private void PlayBreakSound()
    {
        if (audioSource == null || breakSound == null)
            return;

        audioSource.PlayOneShot(breakSound, breakVolume);
    }

    private IEnumerator HitSequence()
    {
        isBusy = true;

        if (hits == 1)
        {
            yield return StartCoroutine(ShakeBox(
                firstHitShakeDuration,
                firstHitShakeAmountX,
                firstHitShakeAmountY,
                firstHitShakeSpeed
            ));
        }
        else if (hits == 2)
        {
            yield return StartCoroutine(ShakeBox(
                secondHitShakeDuration,
                secondHitShakeAmountX,
                secondHitShakeAmountY,
                secondHitShakeSpeed
            ));
        }

        ApplyDamageSprite();

        transform.localPosition = originalLocalPosition;
        transform.localScale = originalLocalScale;

        isBusy = false;
    }

    private void ApplyDamageSprite()
    {
        if (boxSpriteRenderer == null)
            return;

        if (hits == 1)
        {
            if (crackedSprite != null)
                boxSpriteRenderer.sprite = crackedSprite;
        }
        else if (hits == 2)
        {
            if (crackedSprite2 != null)
                boxSpriteRenderer.sprite = crackedSprite2;
            else if (crackedSprite != null)
                boxSpriteRenderer.sprite = crackedSprite;
        }
    }

    private IEnumerator FinalBreakSequence()
    {
        isBusy = true;
        isBreaking = true;

        yield return StartCoroutine(ShakeBox(
            finalShakeDuration,
            finalShakeAmountX,
            finalShakeAmountY,
            finalShakeSpeed
        ));

        transform.localPosition = originalLocalPosition;
        transform.localScale = originalLocalScale;

        if (boxBackgroundRenderer != null)
            boxBackgroundRenderer.enabled = false;

        if (boxSpriteRenderer != null)
        {
            if (brokenSprite != null)
                boxSpriteRenderer.sprite = brokenSprite;
            else if (crackedSprite2 != null)
                boxSpriteRenderer.sprite = crackedSprite2;
            else if (crackedSprite != null)
                boxSpriteRenderer.sprite = crackedSprite;
        }

        PlayBreakSound();
        SpawnBreakEffect();
        SpawnBrokenPieces();

        yield return new WaitForSeconds(brokenSpriteDuration);

        HideAndFinishBreak();
    }

    private IEnumerator ShakeBox(float duration, float amountX, float amountY, float speed)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float x = Mathf.Sin(timer * speed) * amountX;
            float y = Mathf.Cos(timer * speed * 0.5f) * amountY;

            transform.localPosition = originalLocalPosition + new Vector3(x, y, 0f);
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
    }

    private void HideAndFinishBreak()
    {
        if (boxSpriteRenderer != null)
            boxSpriteRenderer.enabled = false;

        if (boxBackgroundRenderer != null)
            boxBackgroundRenderer.enabled = false;

        if (boxCollider != null)
            boxCollider.enabled = false;

        Debug.Log("Middle box broken!");

        float totalChipTime = chipFallDuration + chipExtraFallDuration + chipStayDuration + chipFadeDuration + 0.15f;
        Destroy(gameObject, totalChipTime);
    }

    private void SpawnBreakEffect()
    {
        if (breakEffect == null)
            return;

        GameObject effect = Instantiate(breakEffect, transform.position, Quaternion.identity);

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Clear();
            particleSystems[i].Play();
        }

        Destroy(effect, breakEffectLifetime);
    }

    private void SpawnBrokenPieces()
    {
        if (woodChips == null || woodChips.Length == 0)
            return;

        int count = Mathf.Min(woodChips.Length, 6);

        Vector3[] startOffsets = new Vector3[]
        {
            new Vector3(-0.16f,  0.08f, 0f),
            new Vector3( 0.00f,  0.09f, 0f),
            new Vector3( 0.16f,  0.08f, 0f),
            new Vector3(-0.14f, -0.02f, 0f),
            new Vector3( 0.02f, -0.03f, 0f),
            new Vector3( 0.15f, -0.01f, 0f)
        };

        Vector3[] endOffsets = new Vector3[]
        {
            new Vector3(-0.24f, -0.10f, 0f),
            new Vector3( 0.00f, -0.12f, 0f),
            new Vector3( 0.24f, -0.10f, 0f),
            new Vector3(-0.18f, -0.22f, 0f),
            new Vector3( 0.02f, -0.24f, 0f),
            new Vector3( 0.19f, -0.21f, 0f)
        };

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = woodChips[i];
            if (prefab == null)
                continue;

            Vector3 startPos = transform.position + startOffsets[i];
            Vector3 endPos = transform.position + endOffsets[i];
            Quaternion startRot = Quaternion.Euler(0f, 0f, Random.Range(-10f, 10f));

            GameObject chip = Instantiate(prefab, startPos, startRot);

            Rigidbody2D rb = chip.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.simulated = false;

            Collider2D col = chip.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;

            StartCoroutine(AnimateChip(chip, startPos, endPos, i));
        }
    }

    private IEnumerator AnimateChip(GameObject chip, Vector3 startPos, Vector3 endPos, int index)
    {
        if (chip == null)
            yield break;

        SpriteRenderer sr = chip.GetComponent<SpriteRenderer>();
        Collider2D col = chip.GetComponent<Collider2D>();
        Rigidbody2D rb = chip.GetComponent<Rigidbody2D>();

        Color startColor = Color.white;
        if (sr != null)
            startColor = sr.color;

        float startRot = chip.transform.eulerAngles.z;
        float midRot = startRot + Random.Range(-18f, 18f);

        float endRot = (index % 2 == 0)
            ? -Random.Range(chipEndRotMin, chipEndRotMax)
            : Random.Range(chipEndRotMin, chipEndRotMax);

        float timer = 0f;

        while (timer < chipFallDuration)
        {
            if (chip == null)
                yield break;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / chipFallDuration);

            chip.transform.position = Vector3.Lerp(startPos, endPos, t);
            chip.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRot, midRot, t));

            yield return null;
        }

        Vector3 fallStartPos = chip.transform.position;
        Vector3 fallEndPos = fallStartPos + new Vector3(
            Random.Range(-chipHorizontalSpread, chipHorizontalSpread),
            -chipExtraDropDistance + chipGroundLift,
            0f
        );

        float fallTimer = 0f;

        while (fallTimer < chipExtraFallDuration)
        {
            if (chip == null)
                yield break;

            fallTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fallTimer / chipExtraFallDuration);
            float curvedT = t * t;

            chip.transform.position = Vector3.Lerp(fallStartPos, fallEndPos, curvedT);
            chip.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(midRot, endRot, t));

            yield return null;
        }

        chip.transform.position = fallEndPos;
        chip.transform.rotation = Quaternion.Euler(0f, 0f, endRot);

        float stayTimer = 0f;

        while (stayTimer < chipStayDuration)
        {
            if (chip == null)
                yield break;

            stayTimer += Time.deltaTime;
            yield return null;
        }

        float fadeTimer = 0f;

        while (fadeTimer < chipFadeDuration)
        {
            if (chip == null)
                yield break;

            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / chipFadeDuration);

            if (sr != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            sr.enabled = false;
        }

        if (col != null)
            col.enabled = false;

        if (rb != null)
            rb.simulated = false;

        chip.SetActive(false);
        Destroy(chip);
    }
}