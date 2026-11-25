using System;
using UnityEngine;

[Serializable]
internal sealed class GuardKnockoutStars
{
    [SerializeField] private bool enableEffect = true;
    [SerializeField, Range(1, 8)] private int starCount = 3;
    [SerializeField, Min(0.01f)] private float size = 0.11f;
    [SerializeField] private Color color = new(0.95f, 0.76f, 0.34f, 0.93f);
    [SerializeField, Min(0f), Tooltip("Full orbits per second.")]
    private float rotationSpeed = 0.7f;
    [SerializeField, Min(0f)] private float orbitWidth = 0.29f;
    [SerializeField, Min(0f)] private float orbitHeight = 0.09f;
    [SerializeField] private float offsetX = 0.18f;
    [SerializeField] private float offsetY = 0.095f;
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int orderInLayer = 5;

    private const float Tau = 2f * Mathf.PI;
    private static Sprite sharedStar;
    private Transform root;
    private SpriteRenderer[] stars;
    private float phase;

    public void Show(Transform parent, bool mirrorOffset)
    {
        if (!enableEffect)
            return;

        if (root == null)
        {
            root = new GameObject("Knockout Stars").transform;
            root.SetParent(parent, false);
            CreateStars(Mathf.Clamp(starCount, 1, 8));
        }

        root.localPosition = new Vector3(mirrorOffset ? -offsetX : offsetX, offsetY, 0f);
        phase = 0f;
        root.gameObject.SetActive(true);
        Advance(0f);
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }

    public void Advance(float deltaTime)
    {
        if (root == null || !root.gameObject.activeSelf)
            return;

        phase = Mathf.Repeat(phase + rotationSpeed * Tau * deltaTime, Tau);
        for (int i = 0; i < stars.Length; i++)
        {
            float angle = phase + i * Tau / stars.Length;
            float depth = Mathf.Sin(angle);
            Transform star = stars[i].transform;
            star.localPosition = new Vector3(Mathf.Cos(angle) * orbitWidth * 0.5f, depth * orbitHeight * 0.5f, 0f);
            star.localScale = Vector3.one * (size * (1f - depth * 0.14f));
        }
    }

    private void CreateStars(int count)
    {
        if (sharedStar == null)
            sharedStar = CreateStarSprite();

        stars = new SpriteRenderer[count];
        int sortingLayerId = SortingLayer.NameToID(sortingLayer);
        for (int i = 0; i < count; i++)
        {
            var star = new GameObject("Star").AddComponent<SpriteRenderer>();
            star.transform.SetParent(root, false);
            star.sprite = sharedStar;
            star.color = color;
            star.sortingLayerID = sortingLayerId;
            star.sortingOrder = orderInLayer;
            stars[i] = star;
        }
    }

    private static Sprite CreateStarSprite()
    {
        const int resolution = 32;
        const int samples = 2;
        var points = new Vector2[10];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = Mathf.PI * 0.5f + i * Tau / points.Length;
            float radius = i % 2 == 0 ? 0.47f : 0.215f;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        var pixels = new Color32[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int coverage = 0;
                for (int dy = 0; dy < samples; dy++)
                {
                    for (int dx = 0; dx < samples; dx++)
                    {
                        float sampleX = (x + (dx + 0.5f) / samples) / resolution - 0.5f;
                        float sampleY = (y + (dy + 0.5f) / samples) / resolution - 0.5f;
                        if (ContainsPoint(points, new Vector2(sampleX, sampleY)))
                            coverage++;
                    }
                }

                byte shade = (byte)Mathf.Lerp(180f, 255f, y / (resolution - 1f));
                pixels[y * resolution + x] = new Color32(shade, shade, shade, (byte)(coverage * 255 / (samples * samples)));
            }
        }

        var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
        {
            name = "Knockout Star Texture",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), Vector2.one * 0.5f,
            resolution, 0, SpriteMeshType.FullRect);
        sprite.name = "Knockout Star";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static bool ContainsPoint(Vector2[] points, Vector2 point)
    {
        bool inside = false;
        for (int i = 0, previous = points.Length - 1; i < points.Length; previous = i++)
        {
            Vector2 a = points[previous];
            Vector2 b = points[i];
            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
