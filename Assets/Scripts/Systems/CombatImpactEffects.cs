using System;
using UnityEngine;

[Serializable]
internal sealed class CombatImpactProfile
{
    [ColorUsage(true)] public Color flashColor;
    [Min(0f)] public float flashSize;
    [Range(0f, 2f)] public float brightness;
    [Range(0.05f, 0.5f)] public float duration;
    [Range(0, CombatImpactVisuals.MaxParticles)] public int particleCount;
    public Color particleColor;
    [Min(0f)] public float particleSize;
    [Min(0f)] public float particleSpeed;
    [Range(5f, 115f)] public float particleSpread;
    [Range(0f, 2f)] public float lineLength;
    [Min(0f)] public float shakeStrength;
    [Min(0f)] public float shakeDuration;
    [Range(0f, 0.12f)] public float hitStopDuration;

    public CombatImpactProfile(Color flash, float size, float intensity, float lifetime,
        int sparks, Color sparkColor, float sparkSize, float speed, float spread, float lines,
        float shake, float shakeTime, float stopTime)
    {
        flashColor = flash;
        flashSize = size;
        brightness = intensity;
        duration = lifetime;
        particleCount = sparks;
        particleColor = sparkColor;
        particleSize = sparkSize;
        particleSpeed = speed;
        particleSpread = spread;
        lineLength = lines;
        shakeStrength = shake;
        shakeDuration = shakeTime;
        hitStopDuration = stopTime;
    }
}

[DisallowMultipleComponent]
public sealed class CombatImpactEffects : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private bool enableHitEffects = true;
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private bool enableHitStop = true;

    [Header("Rendering")]
    [SerializeField] private CameraShake2D worldCameraShake;
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int orderInLayer = 100;

    [Header("Impact Profiles")]
    [SerializeField] private CombatImpactProfile kick = new(
        new Color(1f, 0.94f, 0.68f), 0.55f, 1.5f, 0.3f,
        8, new Color(1f, 0.77f, 0.32f), 0.07f, 1.6f, 63f, 1.2f, 0.08f, 0.18f, 0.035f);
    [SerializeField] private CombatImpactProfile fist = new(
        new Color(1f, 0.88f, 0.57f), 0.45f, 1.3f, 0.25f,
        6, new Color(1f, 0.75f, 0.39f), 0.06f, 1.4f, 47f, 0.9f, 0.06f, 0.15f, 0.025f);
    [SerializeField] private CombatImpactProfile club = new(
        new Color(1f, 0.69f, 0.29f), 0.7f, 1.8f, 0.35f,
        12, new Color(1f, 0.49f, 0.15f), 0.09f, 2f, 81f, 1.4f, 0.12f, 0.22f, 0.05f);
    [SerializeField] private CombatImpactProfile sword = new(
        new Color(0.86f, 0.94f, 1f), 0.65f, 1.8f, 0.3f,
        10, new Color(0.77f, 0.91f, 1f), 0.08f, 2.2f, 35f, 1.6f, 0.14f, 0.24f, 0.05f);
    [SerializeField] private CombatImpactProfile skeletonStrike = new(
        new Color(1f, 0.91f, 0.73f), 0.55f, 1.5f, 0.3f,
        8, new Color(0.95f, 0.81f, 0.59f), 0.07f, 1.6f, 54f, 1.2f, 0.08f, 0.18f, 0.035f);
    [SerializeField] private CombatImpactProfile axe = new(
        new Color(1f, 0.89f, 0.72f), 0.75f, 1.8f, 0.35f,
        12, new Color(1f, 0.61f, 0.28f), 0.09f, 2.2f, 68f, 1.5f, 0.15f, 0.24f, 0.05f);
    [SerializeField] private CombatImpactProfile spear = new(
        new Color(0.90f, 0.98f, 1f), 0.6f, 1.7f, 0.3f,
        9, new Color(0.80f, 0.93f, 1f), 0.07f, 2.3f, 24f, 1.8f, 0.12f, 0.22f, 0.04f);

    private static CombatImpactEffects instance;
    private readonly CombatHitStop hitStop = new();
    private CombatImpactVisuals visuals;
    private float shakeAge;
    private float shakeStrength;
    private float shakeDuration;
    private float shakeDirection;

    internal static CombatImpactEffects ForScene(Component target)
    {
        return instance != null && instance.gameObject.scene == target.gameObject.scene
            ? instance : null;
    }

    internal static void Report(Component target, GuardHitType type, CombatContact? contact,
        Transform attacker, Collider2D body)
    {
        var effects = ForScene(target);
        if (effects != null)
            effects.Play(type, contact ?? CombatContact.Between(attacker, target, body));
    }

    private void Awake()
    {
        if (worldCameraShake == null && Camera.main != null)
            Camera.main.TryGetComponent(out worldCameraShake);
        if (enableHitEffects)
            CreateVisuals();
    }

    private void OnEnable()
    {
        instance = this;
        if (enableHitStop)
            hitStop.Prepare();
    }

    private void OnDisable()
    {
        if (instance == this)
            instance = null;

        hitStop.Uninstall();
        visuals?.Clear();
        ClearCameraShake();
    }

    private void OnDestroy() => visuals?.Dispose();

    internal void Play(GuardHitType type, CombatContact contact)
    {
        CombatImpactProfile profile = GetProfile(type);
        if (profile == null)
            return;

        if (enableHitEffects)
        {
            if (visuals == null)
                CreateVisuals();
            visuals.Play(profile, contact);
        }

        if (enableCameraShake && profile.shakeStrength > 0f && profile.shakeDuration > 0f &&
            Time.timeScale > 0f && !PauseManager.IsMenuPaused)
        {
            float strengthRemaining = shakeDuration > 0f
                ? shakeStrength * (1f - shakeAge / shakeDuration)
                : 0f;
            if (profile.shakeStrength >= strengthRemaining)
            {
                shakeAge = 0f;
                shakeStrength = profile.shakeStrength;
                shakeDuration = profile.shakeDuration;
                shakeDirection = contact.Direction;
            }
        }

        if (enableHitStop)
            hitStop.Request(profile.hitStopDuration);
    }

    private void LateUpdate()
    {
        bool paused = Time.timeScale <= 0f || PauseManager.IsMenuPaused;
        if (!enableHitStop || paused)
            hitStop.Cancel();
        else
            hitStop.Prepare();

        if (!enableHitEffects)
            visuals?.Clear();
        else if (visuals != null)
        {
            visuals.SetSorting(sortingLayer, orderInLayer);
            if (!paused)
                visuals.Advance(Time.unscaledDeltaTime);
        }

        if (!enableCameraShake || paused)
        {
            ClearCameraShake();
            return;
        }

        if (shakeDuration <= 0f || worldCameraShake == null)
            return;

        float damping = 1f - shakeAge / shakeDuration;
        float strength = shakeStrength * damping * damping;
        const float tau = 2f * Mathf.PI;
        worldCameraShake.SetCombatOffset(new Vector2(
            Mathf.Cos(shakeAge * tau * 21f) * strength * shakeDirection,
            Mathf.Sin(shakeAge * tau * 27f) * strength * 0.43f));
        shakeAge += Time.unscaledDeltaTime;
        if (shakeAge >= shakeDuration)
            ClearCameraShake();
    }

    private void ClearCameraShake()
    {
        if (shakeDuration <= 0f)
            return;

        shakeDuration = 0f;
        if (worldCameraShake != null)
            worldCameraShake.SetCombatOffset(Vector2.zero);
    }

    private void CreateVisuals()
    {
        visuals = new CombatImpactVisuals(transform, spriteMaterial);
        visuals.SetSorting(sortingLayer, orderInLayer);
    }

    private CombatImpactProfile GetProfile(GuardHitType type) => type switch
    {
        GuardHitType.Kick => kick,
        GuardHitType.Fist => fist,
        GuardHitType.Club => club,
        GuardHitType.Sword => sword,
        GuardHitType.SkeletonStrike => skeletonStrike,
        GuardHitType.Axe => axe,
        GuardHitType.Spear => spear,
        _ => null
    };
}
