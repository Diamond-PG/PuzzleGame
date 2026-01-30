using System.Runtime.InteropServices;
using UnityEngine;

public class MicroHaptics : MonoBehaviour
{
    private const string PREF_KEY = "HAPTICS_ENABLED";

    [Header("Enable")]
    [SerializeField] private bool enableHaptics = true;

    [Header("Anti-spam")]
    [SerializeField, Range(0.02f, 0.3f)]
    private float minInterval = 0.06f;

    [Header("Android Pulse (ms)")]
    [Tooltip("Длительность клика вибрации в миллисекундах (Android).")]
#pragma warning disable CS0414
    [SerializeField, Range(5, 100)]
    private int tinyClickMs = 25;
#pragma warning restore CS0414

    [Header("iOS Haptics")]
    [Tooltip("Тип хаптика для iOS. Selection — самый короткий и приятный для UI.")]
#pragma warning disable CS0414
    [SerializeField] private IOSHapticStyle iosStyle = IOSHapticStyle.Selection;

    [Tooltip("Если iOS-плагин не подключён, использовать Handheld.Vibrate() как запасной вариант.")]
    [SerializeField] private bool iosFallbackToHandheldVibrate = true;
#pragma warning restore CS0414

    private float lastTime;
    private static MicroHaptics instance;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject vibrator;
    private int sdkInt;
#endif

    public enum IOSHapticStyle
    {
        Selection = 0,
        Light = 1,
        Medium = 2,
        Heavy = 3
    }

    private void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Загружаем настройку игрока (по умолчанию ВКЛ)
        enableHaptics = PlayerPrefs.GetInt(PREF_KEY, 1) == 1;

#if UNITY_ANDROID && !UNITY_EDITOR
        TryInitAndroidVibrator();
#endif
    }

    // Можно вызывать откуда угодно (кнопки, пауза, меню и т.д.)
    public static void TinyClick()
    {
        if (instance == null) return;
        instance.PlayTiny();
    }

    // Для UI Toggle: узнать, включена ли вибрация сейчас
    public static bool IsEnabled()
    {
        return instance != null && instance.enableHaptics;
    }

    // Для UI Toggle: включить/выключить и сохранить
    public static void SetEnabled(bool enabled)
    {
        if (instance == null) return;

        instance.enableHaptics = enabled;
        PlayerPrefs.SetInt(PREF_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void PlayTiny()
    {
        if (!enableHaptics) return;

        // Антиспам
        if (Time.unscaledTime - lastTime < minInterval) return;
        lastTime = Time.unscaledTime;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrateMs(tinyClickMs);

#elif UNITY_IOS && !UNITY_EDITOR
        // На iOS делаем хаптики через UIImpactFeedbackGenerator (через плагин)
        bool ok = IOS_Haptic((int)iosStyle);
        if (!ok && iosFallbackToHandheldVibrate)
            Handheld.Vibrate();

#else
        // Другие платформы: дефолт
        Handheld.Vibrate();
#endif
    }

    // ================= ANDROID =================
#if UNITY_ANDROID && !UNITY_EDITOR
    private void TryInitAndroidVibrator()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using (var contextClass = new AndroidJavaClass("android.content.Context"))
                {
                    string vibratorService = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
                    vibrator = activity.Call<AndroidJavaObject>("getSystemService", vibratorService);
                }
            }

            using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                sdkInt = versionClass.GetStatic<int>("SDK_INT");
            }
        }
        catch
        {
            vibrator = null;
            sdkInt = 0;
        }
    }

    private void AndroidVibrateMs(int ms)
    {
        if (ms <= 0) return;

        if (vibrator == null)
        {
            Handheld.Vibrate();
            return;
        }

        try
        {
            if (sdkInt >= 26)
            {
                using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    int defaultAmplitude = vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE");
                    var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createOneShot", (long)ms, defaultAmplitude);

                    vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                vibrator.Call("vibrate", (long)ms);
            }
        }
        catch
        {
            Handheld.Vibrate();
        }
    }
#endif

    // ================= iOS =================
#if UNITY_IOS && !UNITY_EDITOR
    // Важно: вернёт false, если плагин не подключён/не сработал
    [DllImport("__Internal")]
    private static extern bool _MicroHaptics_Haptic(int style);

    private static bool IOS_Haptic(int style)
    {
        try { return _MicroHaptics_Haptic(style); }
        catch { return false; }
    }
#else
    private static bool IOS_Haptic(int style) => false;
#endif
}