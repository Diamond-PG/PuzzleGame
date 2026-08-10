using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrisonBreakDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform doorVisual;
    [SerializeField] private SpriteRenderer doorSpriteRenderer;
    [SerializeField] private Collider2D doorCollider;

    [SerializeField] private Transform player;
    [SerializeField] private PlayerKick playerKick;

    [Header("Background Behind Door")]
    [Tooltip("Чёрный фон за решёткой. Видим у целой и повреждённой двери, скрываем после разрушения.")]
    [SerializeField] private GameObject blackBackground;

    [Header("Door Sprites")]
    [SerializeField] private Sprite intactSprite;
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private Sprite brokenSprite;

    [Header("Door Settings")]
    [SerializeField] private int hitsToBreak = 8;
    [SerializeField] private int damagedSpriteHit = 4;
    [SerializeField] private float interactDistance = 1.6f;

    [Tooltip("Через сколько секунд после начала удара ногой дверь получает удар.")]
    [SerializeField] private float kickImpactDelay = 0.08f;

    [Header("Cartoon Punch Animation")]
    [Tooltip("Общая длительность короткого мультяшного выпирания.")]
    [SerializeField] private float punchDuration = 0.16f;

    [Tooltip("Насколько дверь расширяется по X при первом ударе.")]
    [SerializeField] private float minPunchScaleX = 1.035f;

    [Tooltip("Насколько дверь расширяется по X ближе к последним ударам.")]
    [SerializeField] private float maxPunchScaleX = 1.10f;

    [Tooltip("Насколько дверь слегка сжимается по Y.")]
    [SerializeField] private float minPunchScaleY = 0.985f;

    [Tooltip("Сжатие по Y на сильных ударах.")]
    [SerializeField] private float maxPunchScaleY = 0.95f;

    [Tooltip("Небольшой толчок двери от игрока.")]
    [SerializeField] private float maxPunchMoveX = 0.045f;

    [Header("Final Break")]
    [Tooltip("Насколько сильнее дверь выпирает на последнем ударе.")]
    [SerializeField] private float finalPunchScaleX = 1.14f;

    [Tooltip("Сжатие двери по Y на последнем ударе.")]
    [SerializeField] private float finalPunchScaleY = 0.92f;

    [SerializeField] private float finalPunchDuration = 0.20f;

    [Tooltip("Небольшая пауза перед переключением на сломанную дверь.")]
    [SerializeField] private float finalBreakPause = 0.035f;

    [Header("Break Effect")]
    [Tooltip("Пыль/дым при окончательном разрушении.")]
    [SerializeField] private GameObject breakEffect;

    [SerializeField] private float breakEffectLifetime = 2f;

    [Header("Wood Chips")]
    [Tooltip("Небольшие щепки, которые появляются при финальном ударе.")]
    [SerializeField] private GameObject[] woodChips;

    [SerializeField] private int woodChipCount = 5;

    [SerializeField] private float chipForceX = 1.2f;
    [SerializeField] private float chipForceY = 1.5f;
    [SerializeField] private float chipTorque = 120f;
    [SerializeField] private float chipLifetime = 1.2f;

    [Header("Haptics")]
    [SerializeField] private bool useHaptics = true;

    [SerializeField, Range(5, 120)]
    private int normalHitHapticMs = 22;

    [SerializeField, Range(5, 200)]
    private int finalHitHapticMs = 100;

    [Header("Optional Door Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip breakSound;

    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float breakVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Camera mainCamera;

    private int hits;
    private bool isBusy;
    private bool isBroken;

    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRotation;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (doorVisual == null)
            doorVisual = transform;

        if (doorSpriteRenderer == null && doorVisual != null)
            doorSpriteRenderer = doorVisual.GetComponent<SpriteRenderer>();

        if (doorSpriteRenderer == null)
            doorSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorCollider == null && doorVisual != null)
            doorCollider = doorVisual.GetComponent<Collider2D>();

        if (playerKick == null && player != null)
            playerKick = player.GetComponent<PlayerKick>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (doorVisual != null)
        {
            originalLocalPosition = doorVisual.localPosition;
            originalLocalScale = doorVisual.localScale;
            originalLocalRotation = doorVisual.localRotation;
        }
    }

    private void Start()
    {
        ResetDoorState();
    }

    private void OnEnable()
    {
        ResetDoorState();
    }

    private void OnValidate()
    {
        if (hitsToBreak < 1)
            hitsToBreak = 1;

        if (damagedSpriteHit < 1)
            damagedSpriteHit = 1;

        if (damagedSpriteHit >= hitsToBreak)
            damagedSpriteHit = hitsToBreak - 1;

        if (woodChipCount < 0)
            woodChipCount = 0;
    }

    private void ResetDoorState()
    {
        StopAllCoroutines();

        hits = 0;
        isBusy = false;
        isBroken = false;

        if (doorVisual != null)
        {
            doorVisual.localPosition = originalLocalPosition;
            doorVisual.localScale = originalLocalScale;
            doorVisual.localRotation = originalLocalRotation;
        }

        if (doorSpriteRenderer != null)
        {
            doorSpriteRenderer.enabled = true;
            doorSpriteRenderer.color = Color.white;

            if (intactSprite != null)
                doorSpriteRenderer.sprite = intactSprite;
        }

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (blackBackground != null)
            blackBackground.SetActive(true);
    }

    private void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (isBroken ||
            isBusy ||
            mainCamera == null ||
            doorCollider == null)
        {
            return;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryHitDoor(
                Mouse.current.position.ReadValue()
            );
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryHitDoor(
                Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }
    }

    private void TryHitDoor(Vector2 screenPosition)
    {
        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(
                        mainCamera.transform.position.z
                    )
                )
            );

        Vector2 point2D =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        Collider2D hitCollider =
            Physics2D.OverlapPoint(point2D);

        if (hitCollider == null)
            return;

        if (hitCollider != doorCollider)
            return;

        if (player == null)
        {
            Debug.LogWarning(
                "Player не назначен в PrisonBreakDoor!"
            );

            return;
        }

        if (playerKick == null)
        {
            playerKick =
                player.GetComponent<PlayerKick>();
        }

        if (playerKick == null)
        {
            Debug.LogWarning(
                "PlayerKick не найден на Player!"
            );

            return;
        }

        float distance =
            Vector2.Distance(
                player.position,
                doorVisual.position
            );

        if (distance > interactDistance)
        {
            if (debugLogs)
                Debug.Log("Слишком далеко от двери.");

            return;
        }

        bool kickStarted =
            playerKick.KickToward(
                doorVisual.position
            );

        if (!kickStarted)
            return;

        StartCoroutine(
            HitDoorSequence()
        );
    }

    private IEnumerator HitDoorSequence()
    {
        isBusy = true;

        yield return new WaitForSeconds(
            kickImpactDelay
        );

        hits++;

        if (debugLogs)
        {
            Debug.Log(
                "Door hit: " +
                hits +
                " / " +
                hitsToBreak
            );
        }

        if (hits < hitsToBreak)
        {
            PlayHitHaptic();
            PlayHitSound();

            float hitProgress =
                Mathf.InverseLerp(
                    1f,
                    hitsToBreak - 1f,
                    hits
                );

            float scaleX =
                Mathf.Lerp(
                    minPunchScaleX,
                    maxPunchScaleX,
                    hitProgress
                );

            float scaleY =
                Mathf.Lerp(
                    minPunchScaleY,
                    maxPunchScaleY,
                    hitProgress
                );

            yield return StartCoroutine(
                CartoonPunch(
                    punchDuration,
                    scaleX,
                    scaleY,
                    maxPunchMoveX
                )
            );

            if (hits >= damagedSpriteHit &&
                damagedSprite != null &&
                doorSpriteRenderer != null)
            {
                doorSpriteRenderer.sprite =
                    damagedSprite;
            }
        }
        else
        {
            PlayBreakHaptic();
            PlayBreakSound();

            yield return StartCoroutine(
                CartoonPunch(
                    finalPunchDuration,
                    finalPunchScaleX,
                    finalPunchScaleY,
                    maxPunchMoveX * 1.5f
                )
            );

            if (finalBreakPause > 0f)
            {
                yield return new WaitForSeconds(
                    finalBreakPause
                );
            }

            if (doorSpriteRenderer != null &&
                brokenSprite != null)
            {
                doorSpriteRenderer.sprite =
                    brokenSprite;
            }

            if (blackBackground != null)
                blackBackground.SetActive(false);

            SpawnBreakEffect();
            SpawnWoodChips();

            if (doorCollider != null)
                doorCollider.enabled = false;

            isBroken = true;

            if (debugLogs)
            {
                Debug.Log(
                    "Door broken. Проход открыт."
                );
            }
        }

        if (doorVisual != null)
        {
            doorVisual.localPosition =
                originalLocalPosition;

            doorVisual.localScale =
                originalLocalScale;

            doorVisual.localRotation =
                originalLocalRotation;
        }

        isBusy = false;
    }

    private IEnumerator CartoonPunch(
        float duration,
        float targetScaleX,
        float targetScaleY,
        float moveX
    )
    {
        if (doorVisual == null)
            yield break;

        float timer = 0f;

        float direction = 1f;

        if (player != null)
        {
            direction =
                player.position.x <=
                doorVisual.position.x
                    ? 1f
                    : -1f;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            float punch =
                Mathf.Sin(
                    t * Mathf.PI
                );

            float currentScaleX =
                Mathf.Lerp(
                    1f,
                    targetScaleX,
                    punch
                );

            float currentScaleY =
                Mathf.Lerp(
                    1f,
                    targetScaleY,
                    punch
                );

            doorVisual.localScale =
                new Vector3(
                    originalLocalScale.x *
                    currentScaleX,

                    originalLocalScale.y *
                    currentScaleY,

                    originalLocalScale.z
                );

            doorVisual.localPosition =
                originalLocalPosition +
                new Vector3(
                    direction *
                    moveX *
                    punch,
                    0f,
                    0f
                );

            yield return null;
        }

        doorVisual.localScale =
            originalLocalScale;

        doorVisual.localPosition =
            originalLocalPosition;
    }

    private void SpawnBreakEffect()
    {
        if (breakEffect == null)
            return;

        GameObject effect =
            Instantiate(
                breakEffect,
                doorVisual.position,
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

    private void SpawnWoodChips()
    {
        if (woodChips == null ||
            woodChips.Length == 0)
        {
            return;
        }

        int count =
            Mathf.Min(
                woodChipCount,
                woodChips.Length
            );

        for (int i = 0;
             i < count;
             i++)
        {
            GameObject prefab =
                woodChips[i];

            if (prefab == null)
                continue;

            Vector3 spawnPosition =
                doorVisual.position +
                new Vector3(
                    Random.Range(
                        -0.18f,
                        0.18f
                    ),
                    Random.Range(
                        -0.18f,
                        0.18f
                    ),
                    0f
                );

            GameObject chip =
                Instantiate(
                    prefab,
                    spawnPosition,
                    Quaternion.Euler(
                        0f,
                        0f,
                        Random.Range(
                            -25f,
                            25f
                        )
                    )
                );

            Rigidbody2D chipRb =
                chip.GetComponent<Rigidbody2D>();

            if (chipRb != null)
            {
                float direction =
                    Random.value < 0.5f
                        ? -1f
                        : 1f;

                chipRb.linearVelocity =
                    new Vector2(
                        direction *
                        Random.Range(
                            chipForceX * 0.5f,
                            chipForceX
                        ),
                        Random.Range(
                            chipForceY * 0.5f,
                            chipForceY
                        )
                    );

                chipRb.angularVelocity =
                    Random.Range(
                        -chipTorque,
                        chipTorque
                    );
            }

            Destroy(
                chip,
                chipLifetime
            );
        }
    }

    private void PlayHitHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            normalHitHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }

    private void PlayBreakHaptic()
    {
        if (!useHaptics)
            return;

        MicroHaptics.Pulse(
            finalHitHapticMs,
            MicroHaptics.IOSHapticStyle.Heavy
        );
    }

    private void PlayHitSound()
    {
        if (audioSource == null ||
            hitSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            hitSound,
            hitVolume
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
}