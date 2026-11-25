using UnityEngine;

internal sealed class CombatImpactVisuals
{
    public const int MaxParticles = 12;

    private const int PoolSize = 8;
    private const int TileSize = 48;
    private readonly Texture2D atlas;
    private readonly Sprite core;
    private readonly Sprite halo;
    private readonly Sprite streak;
    private readonly Impact[] pool = new Impact[PoolSize];
    private readonly System.Random random = new();
    private int nextSlot;
    private string sortingLayer;
    private int sortingOrder;

    public CombatImpactVisuals(Transform parent, Material material)
    {
        atlas = CreateAtlas();
        core = CreateSprite(0);
        halo = CreateSprite(1);
        streak = CreateSprite(2);

        for (int i = 0; i < pool.Length; ++i)
            pool[i] = new Impact(parent, material, core, halo, streak);
    }

    public void SetSorting(string layerName, int order)
    {
        if (sortingLayer == layerName && sortingOrder == order)
            return;

        sortingLayer = layerName;
        sortingOrder = order;
        int layerId = SortingLayer.NameToID(layerName);
        foreach (Impact impact in pool)
            impact.SetSorting(layerId, order);
    }

    public void Play(CombatImpactProfile profile, CombatContact contact)
    {
        int slot = nextSlot;
        for (int i = 0; i < pool.Length; ++i)
        {
            int candidate = (nextSlot + i) % pool.Length;
            if (!pool[candidate].IsActive)
            {
                slot = candidate;
                break;
            }
        }

        pool[slot].Play(profile, contact, random);
        nextSlot = (slot + 1) % pool.Length;
    }

    public void Advance(float deltaTime)
    {
        foreach (Impact impact in pool)
            if (impact.IsActive)
                impact.Advance(deltaTime);
    }

    public void Clear()
    {
        foreach (Impact impact in pool)
            if (impact.IsActive)
                impact.Hide();
    }

    public void Dispose()
    {
        foreach (Impact impact in pool)
            impact.Dispose();
        Object.Destroy(core);
        Object.Destroy(halo);
        Object.Destroy(streak);
        Object.Destroy(atlas);
    }

    private Sprite CreateSprite(int tile)
    {
        Sprite sprite = Sprite.Create(atlas, new Rect(tile * TileSize, 0, TileSize, TileSize),
            Vector2.one * 0.5f, TileSize, 0, SpriteMeshType.FullRect);
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    private static Texture2D CreateAtlas()
    {
        const int width = TileSize * 3;
        var pixels = new Color32[width * TileSize];
        for (int row = 1; row < TileSize - 1; ++row)
        {
            float y = Mathf.Abs((row + 0.5f) / TileSize * 2f - 1f);
            for (int column = 1; column < TileSize - 1; ++column)
            {
                float signedX = (column + 0.5f) / TileSize * 2f - 1f;
                float x = Mathf.Abs(signedX);
                float radiusSquared = x * x + y * y;

                // Tapered rays over a compact, rounded core.
                float rayX = 1f - x - y * 5.4f;
                float rayY = 1f - y * 1.25f - x * 5.8f;
                float radius = Mathf.Sqrt(radiusSquared);
                float flash = Mathf.Max(0.72f - radius * 1.64f, Mathf.Max(rayX, rayY));
                float glow = Mathf.Max(0f, 1f - radius);
                float taperedLine = (1f - Mathf.Pow(x, 2.25f)) - y * (0.91f + x * 0.40f);

                int pixel = row * width + column;
                pixels[pixel] = WhiteWithAlpha(flash * 2f);
                pixels[pixel + TileSize] = WhiteWithAlpha(glow * glow);
                pixels[pixel + 2 * TileSize] = WhiteWithAlpha(taperedLine * Mathf.Lerp(1.35f, 0.85f, column / (float)TileSize));
            }
        }

        var texture = new Texture2D(width, TileSize, TextureFormat.RGBA32, false)
        {
            name = "Combat Impact Atlas",
            hideFlags = HideFlags.DontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Color32 WhiteWithAlpha(float alpha)
        => new(255, 255, 255, (byte)(Mathf.Clamp01(alpha) * 255f));

    private sealed class Impact
    {
        private struct Spark
        {
            public SpriteRenderer renderer;
            public Vector2 velocity;
            public float size;
        }

        private readonly Transform root;
        private readonly SpriteRenderer core;
        private readonly SpriteRenderer halo;
        private readonly SpriteRenderer lineUpper;
        private readonly SpriteRenderer lineLower;
        private readonly Spark[] sparks = new Spark[MaxParticles];
        private CombatImpactProfile profile;
        private int usedSparks;
        private int startFrame;
        private float age;
        private float direction;

        public bool IsActive => root.gameObject.activeSelf;

        public Impact(Transform parent, Material material, Sprite flash, Sprite glow, Sprite line)
        {
            root = new GameObject("Impact").transform;
            root.SetParent(parent, false);
            root.gameObject.SetActive(false);

            halo = Add("Glow", glow);
            lineUpper = Add("Upper Streak", line);
            lineLower = Add("Lower Streak", line);
            for (int i = 0; i < sparks.Length; ++i)
                sparks[i].renderer = Add("Spark", line);
            core = Add("Flash", flash);

            SpriteRenderer Add(string name, Sprite sprite)
            {
                var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(root, false);
                renderer.sprite = sprite;
                if (material != null)
                    renderer.sharedMaterial = material;
                return renderer;
            }
        }

        public void SetSorting(int layer, int order)
        {
            Set(halo, order);
            Set(lineUpper, order + 1);
            Set(lineLower, order + 1);
            foreach (Spark spark in sparks)
                Set(spark.renderer, order + 1);
            Set(core, order + 2);

            void Set(SpriteRenderer renderer, int rendererOrder)
            {
                renderer.sortingLayerID = layer;
                renderer.sortingOrder = rendererOrder;
            }
        }

        public void Play(CombatImpactProfile settings, CombatContact contact, System.Random random)
        {
            profile = settings;
            age = 0f;
            direction = contact.Direction;
            startFrame = Time.frameCount;
            root.position = contact.Position;
            core.flipX = direction < 0f;
            core.transform.localRotation = Quaternion.Euler(0f, 0f, direction * 18f);

            usedSparks = Mathf.Clamp(profile.particleCount, 0, MaxParticles);
            for (int i = 0; i < sparks.Length; ++i)
            {
                ref Spark spark = ref sparks[i];
                spark.renderer.enabled = i < usedSparks;
                if (i >= usedSparks)
                    continue;

                float progress = (i + 0.2f + (float)random.NextDouble() * 0.6f) / usedSparks;
                float angle = Mathf.Lerp(-profile.particleSpread, profile.particleSpread, progress) * Mathf.Deg2Rad;
                Vector2 path = new Vector2(Mathf.Cos(angle) * direction, Mathf.Sin(angle));
                float speed = profile.particleSpeed * Mathf.Lerp(0.74f, 1.25f, (float)random.NextDouble());
                spark.velocity = path * speed;
                spark.size = profile.particleSize * Mathf.Lerp(0.72f, 1.16f, (float)random.NextDouble());
                spark.renderer.transform.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(path.y, path.x) * Mathf.Rad2Deg);
            }

            ApplyFrame(0f);
            root.gameObject.SetActive(true);
        }

        public void Advance(float deltaTime)
        {
            if (Time.frameCount == startFrame)
                return;

            age += deltaTime;
            if (age >= profile.duration)
                Hide();
            else
                ApplyFrame(age / profile.duration);
        }

        public void Hide() => root.gameObject.SetActive(false);

        public void Dispose()
        {
            if (root != null)
                Object.Destroy(root.gameObject);
        }

        private void ApplyFrame(float progress)
        {
            float fade = 1f - progress;
            float strength = profile.brightness;
            float fastFade = Mathf.Max(0f, 1f - progress * 1.65f);
            float flashScale = profile.flashSize * Mathf.Lerp(1f, 1.38f, progress);
            core.transform.localScale = new Vector3(flashScale * 1.27f, flashScale, 1f);
            core.color = Opacity(Color.Lerp(profile.flashColor, Color.white, 0.64f), strength * fastFade * fastFade);
            halo.transform.localScale = new Vector3(flashScale * 1.85f, flashScale * 1.44f, 1f);
            halo.color = Opacity(profile.flashColor, strength * 0.62f * fade * fade);
            SetLine(lineUpper, 31f, 1f, progress, fade);
            SetLine(lineLower, -36f, 0.75f, progress, fade);

            float distance = age * (1f - progress * 0.32f);
            for (int i = 0; i < usedSparks; ++i)
            {
                ref Spark spark = ref sparks[i];
                float size = spark.size * Mathf.Lerp(1f, 0.45f, progress);
                Transform particle = spark.renderer.transform;
                particle.localPosition = spark.velocity * (distance + 0.023f) + Vector2.down * (age * age * 1.55f);
                particle.localScale = new Vector3(size * (1.6f + profile.lineLength), size * 0.53f, 1f);
                spark.renderer.color = Opacity(profile.particleColor, Mathf.Min(1f, fade * 1.5f));
            }
        }

        private void SetLine(SpriteRenderer line, float degrees, float lengthMultiplier, float progress, float fade)
        {
            float angle = degrees * Mathf.Deg2Rad;
            Vector2 axis = new Vector2(direction * Mathf.Cos(angle), Mathf.Sin(angle));
            float length = profile.flashSize * profile.lineLength * lengthMultiplier;
            Transform visual = line.transform;
            visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg);
            visual.localPosition = axis * (length * (0.34f + progress * 0.31f));
            visual.localScale = new Vector3(length, profile.flashSize * 0.09f * fade, 1f);
            line.color = Opacity(profile.flashColor, fade * fade * 0.85f * profile.brightness);
        }

        private static Color Opacity(Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
            return color;
        }
    }
}
