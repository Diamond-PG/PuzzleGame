using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BonusHeartPickup : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private HeartsUI heartsUI;
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Объект HeartsContainer с пятью обычными сердцами.")]
    [SerializeField] private GameObject regularHeartsTargetUI;

    [Tooltip("Бонусная рамка, внутри которой отображается x1, x2 и далее.")]
    [SerializeField] private GameObject bonusHeartBadgeUI;

    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float pickupDistance = 0.8f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 1f;

    [Header("Haptics")]
    [SerializeField] private bool usePickupHaptics = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D pickupCollider;
    [SerializeField] private HeartPulse heartPulse;

    [Header("Fly To UI Animation")]
    [SerializeField] private bool animateToUI = true;
    [SerializeField] private float flyDuration = 0.35f;
    [SerializeField] private float flyArcHeight = 0.6f;
    [SerializeField] private float endScaleMultiplier = 0.35f;

    [Header("Golden Spark Trail")]
    [SerializeField] private bool useGoldenSparkTrail = true;
    [SerializeField] private int sparksPerSecond = 120;
    [SerializeField] private float sparkLifetime = 0.7f;
    [SerializeField] private float sparkSize = 0.12f;
    [SerializeField] private float sparkSpread = 0.12f;
    [SerializeField] private float sparkDrift = 0.18f;
    [SerializeField] private float sparkRotationSpeed = 280f;
    [SerializeField] private Color sparkColor =
        new Color(1f, 0.72f, 0.12f, 1f);

    [SerializeField] private int sparkSortingOrderOffset = 5;

    [Header("End Burst")]
    [SerializeField] private int endBurstCount = 18;
    [SerializeField] private float endBurstSize = 0.16f;
    [SerializeField] private float endBurstLifetime = 0.55f;
    [SerializeField] private float endBurstSpread = 0.35f;

    private Camera mainCamera;
    private bool pickedUp;
    private bool shouldRestoreRegularHeart;

    private float sparkTimer;
    private Sprite sparkSprite;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (heartsUI == null)
            heartsUI = Object.FindFirstObjectByType<HeartsUI>();

        if (playerHealth == null)
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();

        if (regularHeartsTargetUI == null && heartsUI != null)
        {
            Transform heartsContainer =
                heartsUI.transform.Find("HeartsContainer");

            if (heartsContainer != null)
            {
                regularHeartsTargetUI =
                    heartsContainer.gameObject;
            }
            else
            {
                regularHeartsTargetUI =
                    heartsUI.gameObject;
            }
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (pickupCollider == null)
            pickupCollider = GetComponent<Collider2D>();

        if (heartPulse == null)
            heartPulse = GetComponent<HeartPulse>();

        sparkSprite = CreateWhiteSprite();
    }

    private void Update()
    {
        if (pickedUp)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPickup(
                Mouse.current.position.ReadValue()
            );
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryPickup(
                Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }
    }

    private void TryPickup(Vector2 screenPos)
    {
        if (mainCamera == null)
            return;

        Vector3 worldPos =
            mainCamera.ScreenToWorldPoint(screenPos);

        Vector2 point2D =
            new Vector2(worldPos.x, worldPos.y);

        Collider2D hit =
            Physics2D.OverlapPoint(point2D);

        if (hit == null)
            return;

        if (hit.gameObject != gameObject)
            return;

        GameObject player =
            GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogWarning(
                "Player не найден! Проверь Tag = Player."
            );

            return;
        }

        float distance = Vector2.Distance(
            player.transform.position,
            transform.position
        );

        if (distance > pickupDistance)
        {
            Debug.Log(
                "Слишком далеко от сердечка"
            );

            return;
        }

        if (playerHealth == null)
            playerHealth = player.GetComponent<PlayerHealth>();

        Pickup();
    }

    private void Pickup()
    {
        if (pickedUp)
            return;

        pickedUp = true;

        /*
         * Запоминаем состояние здоровья именно в момент подбора.
         *
         * Если хотя бы одного обычного сердца не хватает —
         * восстанавливаем обычное сердце.
         *
         * Если обычные сердца уже заполнены —
         * добавляем бонус x1.
         */
        shouldRestoreRegularHeart =
            playerHealth != null &&
            playerHealth.HasMissingHearts;

        if (pickupCollider != null)
            pickupCollider.enabled = false;

        if (heartPulse != null)
            heartPulse.enabled = false;

        if (usePickupHaptics)
            MicroHaptics.TinyClick();

        if (audioSource != null &&
            pickupSound != null)
        {
            audioSource.PlayOneShot(
                pickupSound,
                pickupVolume
            );
        }

        GameObject targetUI =
            shouldRestoreRegularHeart
                ? regularHeartsTargetUI
                : bonusHeartBadgeUI;

        if (animateToUI && targetUI != null)
        {
            StartCoroutine(
                AnimateHeartToUI(targetUI)
            );
        }
        else
        {
            FinishPickupInstant();
        }
    }

    private IEnumerator AnimateHeartToUI(
        GameObject targetUI
    )
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 startWorldPos =
            transform.position;

        Vector3 startScale =
            transform.localScale;

        Vector3 targetWorldPos =
            startWorldPos;

        RectTransform targetRect =
            targetUI.GetComponent<RectTransform>();

        Canvas canvas =
            targetUI.GetComponentInParent<Canvas>();

        if (mainCamera != null &&
            targetRect != null &&
            canvas != null)
        {
            Camera uiCamera = null;

            if (canvas.renderMode !=
                RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = canvas.worldCamera;

                if (uiCamera == null)
                    uiCamera = mainCamera;
            }

            Vector3 screenPoint =
                RectTransformUtility.WorldToScreenPoint(
                    uiCamera,
                    targetRect.position
                );

            screenPoint.z = Mathf.Abs(
                mainCamera.transform.position.z -
                transform.position.z
            );

            targetWorldPos =
                mainCamera.ScreenToWorldPoint(screenPoint);

            targetWorldPos.z =
                transform.position.z;
        }

        float safeFlyDuration =
            Mathf.Max(0.01f, flyDuration);

        float time = 0f;
        sparkTimer = 0f;

        Vector3 endScale =
            startScale * endScaleMultiplier;

        while (time < safeFlyDuration)
        {
            float t =
                time / safeFlyDuration;

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            Vector3 position =
                Vector3.Lerp(
                    startWorldPos,
                    targetWorldPos,
                    smoothT
                );

            position.y +=
                Mathf.Sin(smoothT * Mathf.PI) *
                flyArcHeight;

            transform.position = position;

            transform.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    smoothT
                );

            SpawnSparkTrail();

            time += Time.deltaTime;

            yield return null;
        }

        transform.position =
            targetWorldPos;

        transform.localScale =
            endScale;

        SpawnEndBurst();
        FinishPickupInstant();
    }

    private void SpawnSparkTrail()
    {
        if (!useGoldenSparkTrail)
            return;

        if (sparksPerSecond <= 0)
            return;

        sparkTimer += Time.deltaTime;

        float interval =
            1f / sparksPerSecond;

        while (sparkTimer >= interval)
        {
            sparkTimer -= interval;

            SpawnStarSpark(
                transform.position,
                sparkSize,
                sparkLifetime,
                sparkSpread,
                sparkDrift
            );
        }
    }

    private void SpawnEndBurst()
    {
        if (!useGoldenSparkTrail)
            return;

        for (int i = 0; i < endBurstCount; i++)
        {
            SpawnStarSpark(
                transform.position,
                endBurstSize,
                endBurstLifetime,
                endBurstSpread,
                endBurstSpread
            );
        }
    }

    private void SpawnStarSpark(
        Vector3 centerPos,
        float size,
        float lifetime,
        float spread,
        float driftAmount
    )
    {
        GameObject sparkRoot =
            new GameObject(
                "Heart_Golden_Star_Spark"
            );

        sparkRoot.transform.position =
            centerPos +
            new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            );

        sparkRoot.transform.localScale =
            Vector3.one *
            Random.Range(0.75f, 1.25f);

        sparkRoot.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                Random.Range(0f, 360f)
            );

        SpriteRenderer lineA =
            CreateSparkLine(
                sparkRoot.transform,
                size,
                size * 0.22f,
                0f
            );

        SpriteRenderer lineB =
            CreateSparkLine(
                sparkRoot.transform,
                size,
                size * 0.22f,
                90f
            );

        StartCoroutine(
            AnimateSpark(
                sparkRoot,
                lineA,
                lineB,
                lifetime,
                driftAmount
            )
        );
    }

    private SpriteRenderer CreateSparkLine(
        Transform parent,
        float length,
        float width,
        float zRotation
    )
    {
        GameObject line =
            new GameObject("Spark_Line");

        line.transform.SetParent(parent);

        line.transform.localPosition =
            Vector3.zero;

        line.transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                zRotation
            );

        line.transform.localScale =
            new Vector3(
                length,
                width,
                1f
            );

        SpriteRenderer sprite =
            line.AddComponent<SpriteRenderer>();

        sprite.sprite = sparkSprite;
        sprite.color = sparkColor;

        if (spriteRenderer != null)
        {
            sprite.sortingLayerID =
                spriteRenderer.sortingLayerID;

            sprite.sortingOrder =
                spriteRenderer.sortingOrder +
                sparkSortingOrderOffset;
        }

        return sprite;
    }

    private IEnumerator AnimateSpark(
        GameObject sparkRoot,
        SpriteRenderer lineA,
        SpriteRenderer lineB,
        float lifetime,
        float driftAmount
    )
    {
        if (sparkRoot == null)
            yield break;

        float safeLifetime =
            Mathf.Max(0.01f, lifetime);

        Vector3 startPos =
            sparkRoot.transform.position;

        Vector3 endPos =
            startPos +
            new Vector3(
                Random.Range(
                    -driftAmount,
                    driftAmount
                ),
                Random.Range(
                    -driftAmount,
                    driftAmount
                ),
                0f
            );

        Vector3 startScale =
            sparkRoot.transform.localScale;

        Vector3 endScale =
            startScale * 0.2f;

        float timer = 0f;

        while (timer < safeLifetime)
        {
            if (sparkRoot == null)
                yield break;

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / safeLifetime
                );

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            sparkRoot.transform.position =
                Vector3.Lerp(
                    startPos,
                    endPos,
                    smoothT
                );

            sparkRoot.transform.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    smoothT
                );

            sparkRoot.transform.Rotate(
                0f,
                0f,
                sparkRotationSpeed *
                Time.deltaTime
            );

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    smoothT
                );

            SetRendererAlpha(
                lineA,
                alpha
            );

            SetRendererAlpha(
                lineB,
                alpha
            );

            yield return null;
        }

        Destroy(sparkRoot);
    }

    private void SetRendererAlpha(
        SpriteRenderer sprite,
        float alpha
    )
    {
        if (sprite == null)
            return;

        Color color = sprite.color;
        color.a = alpha;
        sprite.color = color;
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture =
            new Texture2D(8, 8);

        Color[] pixels =
            new Color[8 * 8];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(
                0,
                0,
                texture.width,
                texture.height
            ),
            new Vector2(0.5f, 0.5f),
            8f
        );
    }

    private void FinishPickupInstant()
    {
        bool regularHeartWasRestored = false;

        if (shouldRestoreRegularHeart &&
            playerHealth != null)
        {
            regularHeartWasRestored =
                playerHealth.TryRestoreHeart();
        }

        /*
         * Бонус добавляется только в двух случаях:
         *
         * 1. В момент подбора обычные сердца уже были заполнены.
         * 2. По какой-либо причине обычное сердце восстановить не удалось.
         */
        if (!regularHeartWasRestored &&
            heartsUI != null)
        {
            heartsUI.AddBonusHeart();
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        float waitTime = 0.05f;

        if (pickupSound != null)
        {
            waitTime =
                pickupSound.length + 0.05f;
        }

        StartCoroutine(
            DisableAfterSound(waitTime)
        );
    }

    private IEnumerator DisableAfterSound(
        float delay
    )
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(
                delay
            );
        }

        gameObject.SetActive(false);
    }
}