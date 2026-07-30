using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MicroHaptics : MonoBehaviour
{
    // Один ключ на весь проект — ВЕЗДЕ должен быть такой же.
    private const string PREF_KEY = "HAPTICS_ENABLED";

    [Header("Enable")]
    [SerializeField] private bool enableHaptics = true;

    [Header("Anti-spam")]
    [SerializeField, Range(0.02f, 0.3f)]
    private float minInterval = 0.06f;

#pragma warning disable CS0414

    [Header("Default Android Pulse (ms)")]
    [Tooltip("Стандартная длительность короткой вибрации на Android.")]
    [SerializeField, Range(5, 100)]
    private int tinyClickMs = 25;

    [Header("Default iOS Haptics")]
    [Tooltip("Стандартный тип вибрации для обычного короткого нажатия.")]
    [SerializeField]
    private IOSHapticStyle iosStyle = IOSHapticStyle.Selection;

    [Tooltip("Если iOS-плагин не подключён, использовать Handheld.Vibrate().")]
    [SerializeField]
    private bool iosFallbackToHandheldVibrate = true;

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
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        enableHaptics = PlayerPrefs.GetInt(PREF_KEY, 1) == 1;

#if UNITY_ANDROID && !UNITY_EDITOR
        TryInitAndroidVibrator();
#endif
    }

    /// <summary>
    /// Стандартный короткий импульс.
    /// Можно продолжать использовать для кнопок и других элементов UI.
    /// </summary>
    public static void TinyClick()
    {
        if (instance == null)
            return;

        instance.PlayPulse(
            instance.tinyClickMs,
            instance.iosStyle
        );
    }

    /// <summary>
    /// Настраиваемый импульс.
    /// Используется для пружинок и других игровых объектов.
    /// </summary>
    public static void Pulse(
        int androidDurationMs,
        IOSHapticStyle iosHapticStyle
    )
    {
        if (instance == null)
            return;

        instance.PlayPulse(
            androidDurationMs,
            iosHapticStyle
        );
    }

    public static bool IsEnabled()
    {
        return PlayerPrefs.GetInt(PREF_KEY, 1) == 1;
    }

    public static void SetEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(PREF_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (instance != null)
            instance.enableHaptics = enabled;
    }

    private void PlayPulse(
        int androidDurationMs,
        IOSHapticStyle selectedIOSStyle
    )
    {
        if (!enableHaptics)
            return;

        if (Time.unscaledTime - lastTime < minInterval)
            return;

        lastTime = Time.unscaledTime;

        int safeDuration = Mathf.Clamp(androidDurationMs, 5, 200);

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidVibrateMs(safeDuration);

#elif UNITY_IOS && !UNITY_EDITOR
        bool success = IOS_Haptic((int)selectedIOSStyle);

        if (!success && iosFallbackToHandheldVibrate)
            Handheld.Vibrate();

#else
        // В Unity Editor реальную мобильную вибрацию нормально
        // проверить нельзя. Проверяем на телефоне.
#endif
    }

    // ================= ANDROID =================

#if UNITY_ANDROID && !UNITY_EDITOR
    private void TryInitAndroidVibrator()
    {
        try
        {
            using (var unityPlayer =
                   new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity =
                    unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using (var contextClass =
                       new AndroidJavaClass("android.content.Context"))
                {
                    string vibratorService =
                        contextClass.GetStatic<string>("VIBRATOR_SERVICE");

                    vibrator =
                        activity.Call<AndroidJavaObject>(
                            "getSystemService",
                            vibratorService
                        );
                }
            }

            using (var versionClass =
                   new AndroidJavaClass("android.os.Build$VERSION"))
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

    private void AndroidVibrateMs(int milliseconds)
    {
        if (milliseconds <= 0)
            return;

        if (vibrator == null)
        {
            Handheld.Vibrate();
            return;
        }

        try
        {
            if (sdkInt >= 26)
            {
                using (var vibrationEffectClass =
                       new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    int defaultAmplitude =
                        vibrationEffectClass.GetStatic<int>(
                            "DEFAULT_AMPLITUDE"
                        );

                    var effect =
                        vibrationEffectClass.CallStatic<AndroidJavaObject>(
                            "createOneShot",
                            (long)milliseconds,
                            defaultAmplitude
                        );

                    vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                vibrator.Call("vibrate", (long)milliseconds);
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
    [DllImport("__Internal")]
    private static extern bool _MicroHaptics_Haptic(int style);

    private static bool IOS_Haptic(int style)
    {
        try
        {
            return _MicroHaptics_Haptic(style);
        }
        catch
        {
            return false;
        }
    }
#else
    private static bool IOS_Haptic(int style)
    {
        return false;
    }
#endif
}