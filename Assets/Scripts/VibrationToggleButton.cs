using UnityEngine;
using TMPro;

public class VibrationToggleButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text label;          // Текст "ON/OFF" (если есть)
    [SerializeField] private string onText = "ON";
    [SerializeField] private string offText = "OFF";

    private void Start()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    public void ToggleVibration()
    {
        bool enabledNow = MicroHaptics.IsEnabled();
        MicroHaptics.SetEnabled(!enabledNow);

        // маленький клик (чтобы было приятно) — но только если включили
        if (!enabledNow)
            MicroHaptics.TinyClick();

        Apply();
    }

    private void Apply()
    {
        bool enabled = MicroHaptics.IsEnabled();

        if (label != null)
            label.text = enabled ? onText : offText;
    }
}