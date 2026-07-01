using System.Collections;
using UnityEngine;

public class PickupFlyEffect : MonoBehaviour
{
    [Header("Fly To UI Animation")]
    [SerializeField] private float flyDuration = 0.35f;
    [SerializeField] private float flyArcHeight = 0.6f;
    [SerializeField] private float endScaleMultiplier = 0.35f;

    [Header("Golden Spark Trail")]
    [SerializeField] private bool useGoldenSparkTrail = true;
    [SerializeField] private int sparksPerSecond = 160;
    [SerializeField] private float sparkLifetime = 1.2f;
    [SerializeField] private float sparkSize = 0.1f;
    [SerializeField] private float sparkSpread = 0.16f;
    [SerializeField] private float sparkDrift = 0.28f;
    [SerializeField] private float sparkRotationSpeed = 360f;
    [SerializeField] private Color sparkColor = new Color(1f, 0.72f, 0.12f, 1f);
    [SerializeField] private int sparkSortingOrderOffset = 10;

    [Header("End Burst")]
    [SerializeField] private int endBurstCount = 28;
    [SerializeField] private float endBurstSize = 0.22f;
    [SerializeField] private float endBurstLifetime = 0.7f;
    [SerializeField] private float endBurstSpread = 0.45f;

    private Camera mainCamera;
    private SpriteRenderer sourceRenderer;
    private Sprite sparkSprite;
    private float sparkTimer;

    private void Awake()
    {
        mainCamera = Camera.main;
        sourceRenderer = GetComponentInChildren<SpriteRenderer>();
        sparkSprite = CreateWhiteSprite();
    }

    public IEnumerator FlyToUI(GameObject targetUI, System.Action onArrived)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 startWorldPos = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 targetWorldPos = startWorldPos;

        RectTransform targetRect = targetUI != null ? targetUI.GetComponent<RectTransform>() : null;
        Canvas canvas = targetUI != null ? targetUI.GetComponentInParent<Canvas>() : null;

        if (mainCamera != null && targetRect != null && canvas != null)
        {
            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                targetRect.position
            );

            screenPoint.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            targetWorldPos = mainCamera.ScreenToWorldPoint(screenPoint);
            targetWorldPos.z = transform.position.z;
        }

        float time = 0f;
        sparkTimer = 0f;
        Vector3 endScale = startScale * endScaleMultiplier;

        while (time < flyDuration)
        {
            float t = time / flyDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = Vector3.Lerp(startWorldPos, targetWorldPos, smoothT);
            pos.y += Mathf.Sin(smoothT * Mathf.PI) * flyArcHeight;

            transform.position = pos;
            transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

            SpawnSparkTrail();

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetWorldPos;
        transform.localScale = endScale;

        SpawnEndBurst();

        onArrived?.Invoke();
    }

    private void SpawnSparkTrail()
    {
        if (!useGoldenSparkTrail || sparksPerSecond <= 0)
            return;

        sparkTimer += Time.deltaTime;
        float interval = 1f / sparksPerSecond;

        while (sparkTimer >= interval)
        {
            sparkTimer -= interval;
            SpawnStarSpark(transform.position, sparkSize, sparkLifetime, sparkSpread, sparkDrift);
        }
    }

    private void SpawnEndBurst()
    {
        if (!useGoldenSparkTrail)
            return;

        for (int i = 0; i < endBurstCount; i++)
            SpawnStarSpark(transform.position, endBurstSize, endBurstLifetime, endBurstSpread, endBurstSpread);
    }

    private void SpawnStarSpark(Vector3 centerPos, float size, float lifetime, float spread, float driftAmount)
    {
        GameObject sparkRoot = new GameObject("Pickup_Golden_Star_Spark");

        sparkRoot.transform.position = centerPos + new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        );

        sparkRoot.transform.localScale = Vector3.one * Random.Range(0.75f, 1.25f);
        sparkRoot.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        SpriteRenderer lineA = CreateSparkLine(sparkRoot.transform, size, size * 0.22f, 0f);
        SpriteRenderer lineB = CreateSparkLine(sparkRoot.transform, size, size * 0.22f, 90f);

        Vector3 drift = new Vector3(
            Random.Range(-driftAmount, driftAmount),
            Random.Range(-driftAmount, driftAmount),
            0f
        );

        SparkAnimator animator = sparkRoot.AddComponent<SparkAnimator>();
        animator.Init(lineA, lineB, lifetime, drift, sparkRotationSpeed);
    }

    private SpriteRenderer CreateSparkLine(Transform parent, float length, float width, float zRotation)
    {
        GameObject line = new GameObject("Spark_Line");
        line.transform.SetParent(parent);
        line.transform.localPosition = Vector3.zero;
        line.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        line.transform.localScale = new Vector3(length, width, 1f);

        SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
        sr.sprite = sparkSprite;
        sr.color = sparkColor;

        if (sourceRenderer != null)
        {
            sr.sortingLayerID = sourceRenderer.sortingLayerID;
            sr.sortingOrder = sourceRenderer.sortingOrder + sparkSortingOrderOffset;
        }

        return sr;
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(8, 8);
        Color[] pixels = new Color[8 * 8];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            8f
        );
    }
}

public class SparkAnimator : MonoBehaviour
{
    private SpriteRenderer lineA;
    private SpriteRenderer lineB;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 startScale;
    private Vector3 endScale;

    private float lifetime;
    private float rotationSpeed;
    private float timer;

    public void Init(SpriteRenderer a, SpriteRenderer b, float life, Vector3 drift, float rotSpeed)
    {
        lineA = a;
        lineB = b;
        lifetime = Mathf.Max(0.01f, life);
        rotationSpeed = rotSpeed;

        startPos = transform.position;
        endPos = startPos + drift;

        startScale = transform.localScale;
        endScale = startScale * 0.2f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / lifetime);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        transform.position = Vector3.Lerp(startPos, endPos, smoothT);
        transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        float alpha = Mathf.Lerp(1f, 0f, smoothT);

        SetAlpha(lineA, alpha);
        SetAlpha(lineB, alpha);

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}