using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class VictoryFireworks : MaskableGraphic
{
    [Serializable]
    private sealed class Salute
    {
        [Tooltip("Start and burst positions in the normalized frame rectangle.")]
        public Vector2 launch;
        public Vector2 burst;
        [Min(0f)] public float delay;
        [Min(1f)] public float rocketSpeed = 520f;
        [Min(1f)] public float burstSize = 174f;
        [Range(4, 64)] public int sparkCount = 54;
        [Min(0.1f)] public float duration = 1.26f;
        [Min(0f)] public float fallAcceleration = 110f;
        public Color rocketColor = new(1f, 0.78f, 0.36f);
    }

    [SerializeField] private Salute left = new()
    {
        launch = new Vector2(0.19f, 0.28f), burst = new Vector2(0.105f, 0.66f)
    };
    [SerializeField] private Salute right = new()
    {
        launch = new Vector2(0.81f, 0.28f), burst = new Vector2(0.895f, 0.67f), delay = 0.12f,
        rocketColor = new Color(0.78f, 0.93f, 1f)
    };
    [SerializeField] private Color gold = new(1f, 0.75f, 0.29f);
    [SerializeField] private Color silver = new(0.76f, 0.88f, 1f);
    [SerializeField] private Color white = new(1f, 0.99f, 0.92f);
    [SerializeField] private Color emerald = new(0.28f, 1f, 0.63f);
    [SerializeField, Min(0.1f)] private float launchInterval = 2.2f;

    private struct Spark
    {
        public Vector2 velocity;
        public float lifetime;
        public float size;
        public Color color;
    }

    private Spark[] leftSparks;
    private Spark[] rightSparks;
    private float elapsed;
    private bool playing;

    public void Play()
    {
        if (playing)
            return;

        playing = true;
        elapsed = 0f;
        leftSparks = CreateSparks(left, 206, gold);
        rightSparks = CreateSparks(right, 417, silver);
        SetVerticesDirty();
    }

    public void Stop()
    {
        playing = false;
        elapsed = 0f;
        SetVerticesDirty();
    }

    private void Update()
    {
        if (!playing || PauseManager.IsMenuPaused)
            return;

        // Let both bursts finish before starting the next pair at any frame size.
        float cycleDuration = Mathf.Max(launchInterval, Mathf.Max(EndTime(left), EndTime(right)));
        elapsed = (elapsed + Time.unscaledDeltaTime) % cycleDuration;
        SetVerticesDirty();
    }

    protected override void OnDisable()
    {
        Stop();
        base.OnDisable();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (!playing)
            return;

        DrawSalute(mesh, left, leftSparks);
        DrawSalute(mesh, right, rightSparks);
    }

    private Spark[] CreateSparks(Salute salute, int seed, Color primary)
    {
        var random = new System.Random(seed);
        var sparks = new Spark[salute.sparkCount];
        for (int i = 0; i < sparks.Length; i++)
        {
            float angle = 2f * Mathf.PI * (i + (float)random.NextDouble() * 0.85f) / sparks.Length;
            float speed = Mathf.Lerp(0.48f, 1.38f, (float)random.NextDouble()) * salute.burstSize;
            float tone = (float)random.NextDouble();
            Color sparkColor = tone < 0.15f ? emerald : tone < 0.31f ? white : primary;
            sparks[i] = new Spark
            {
                velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                lifetime = salute.duration * Mathf.Lerp(0.59f, 1f, (float)random.NextDouble()),
                size = Mathf.Lerp(2.1f, 5.7f, (float)random.NextDouble()),
                color = sparkColor
            };
        }

        return sparks;
    }

    private void DrawSalute(VertexHelper mesh, Salute salute, Spark[] sparks)
    {
        float time = elapsed - salute.delay;
        if (time < 0f)
            return;

        Vector2 start = FramePoint(salute.launch);
        Vector2 end = FramePoint(salute.burst);
        float flight = FlightTime(salute);
        if (time < flight)
        {
            float progress = time / flight;
            Vector2 head = RocketPosition(start, end, progress);
            Vector2 tail = RocketPosition(start, end, Mathf.Max(0f, progress - 0.15f));
            AddStreak(mesh, tail, head, 8f, WithAlpha(salute.rocketColor, 0.55f));
            AddStreak(mesh, Vector2.Lerp(tail, head, 0.38f), head, 3.2f, salute.rocketColor);
            AddStar(mesh, head, 17f, WithAlpha(salute.rocketColor, 0.44f));
            AddStar(mesh, head, 8.5f, white);
            return;
        }

        time -= flight;
        if (time < 0.22f)
        {
            float flash = 1f - time / 0.22f;
            AddStar(mesh, end, 42f - 14f * flash, WithAlpha(salute.rocketColor, flash * 0.70f));
            AddStar(mesh, end, 16f, WithAlpha(white, flash));
        }

        foreach (Spark spark in sparks)
        {
            float progress = time / spark.lifetime;
            if (progress >= 1f)
                continue;

            float fade = 1f - progress * progress;
            Vector2 head = SparkPosition(end, spark.velocity, salute.fallAcceleration, time);
            Vector2 tail = SparkPosition(end, spark.velocity, salute.fallAcceleration, Mathf.Max(0f, time - 0.075f));
            AddStreak(mesh, tail, head, spark.size * 0.60f, WithAlpha(spark.color, fade * 0.82f));
            AddStar(mesh, head, spark.size * (1f + 0.50f * fade), WithAlpha(spark.color, fade));
        }
    }

    private Vector2 FramePoint(Vector2 normalized) => rectTransform.rect.min + Vector2.Scale(rectTransform.rect.size, normalized);

    private float FlightTime(Salute salute) => Mathf.Max(0.01f, Vector2.Distance(FramePoint(salute.launch), FramePoint(salute.burst)) / salute.rocketSpeed);

    private float EndTime(Salute salute) => salute.delay + FlightTime(salute) + salute.duration;

    private static Vector2 RocketPosition(Vector2 start, Vector2 end, float progress) =>
        Vector2.Lerp(start, end, progress) + Vector2.up * (Mathf.Sin(progress * Mathf.PI) * 39f);

    private static Vector2 SparkPosition(Vector2 origin, Vector2 velocity, float gravity, float time) =>
        origin + velocity * (time / (1f + 0.70f * time)) + Vector2.down * (0.5f * gravity * time * time);

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a *= alpha;
        return color;
    }

    private static void AddStar(VertexHelper mesh, Vector2 center, float size, Color color)
    {
        int vertex = mesh.currentVertCount;
        Color edge = WithAlpha(color, 0f);
        mesh.AddVert(center, color, Vector2.zero);
        mesh.AddVert(center + Vector2.up * size, edge, Vector2.zero);
        mesh.AddVert(center + Vector2.right * size * 0.60f, edge, Vector2.zero);
        mesh.AddVert(center + Vector2.down * size, edge, Vector2.zero);
        mesh.AddVert(center + Vector2.left * size * 0.60f, edge, Vector2.zero);
        for (int i = 0; i < 4; i++)
            mesh.AddTriangle(vertex, vertex + i + 1, vertex + (i + 1) % 4 + 1);
    }

    private static void AddStreak(VertexHelper mesh, Vector2 from, Vector2 to, float thickness, Color color)
    {
        Vector2 normal = Vector2.Perpendicular((to - from).normalized) * (0.5f * thickness);
        int vertex = mesh.currentVertCount;
        mesh.AddVert(from, WithAlpha(color, 0f), Vector2.zero);
        mesh.AddVert(to + normal, color, Vector2.zero);
        mesh.AddVert(to - normal, color, Vector2.zero);
        mesh.AddTriangle(vertex, vertex + 1, vertex + 2);
    }
}
