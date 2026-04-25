using UnityEngine;
using TMPro;

public class SoundToggleButton : MonoBehaviour
{
    private const string SOUND_KEY = "SoundEnabled";

    [Header("UI")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private string onText = "ON";
    [SerializeField] private string offText = "OFF";

    [Header("Optional: Menu Music")]
    [SerializeField] private AudioSource menuMusic;

    [Header("Glow")]
    [SerializeField] private GameObject offGlow;
    [SerializeField] private GameObject onGlow;

    [Header("Pulse (Animator on Glow objects)")]
    [SerializeField] private Animator offGlowAnimator;
    [SerializeField] private Animator onGlowAnimator;

    private void Start()
    {
        Apply(LoadEnabled());
    }

    // Нажатие "ON"
    public void SetOn()
    {
        SaveEnabled(true);
        Apply(true);
    }

    // Нажатие "OFF"
    public void SetOff()
    {
        SaveEnabled(false);
        Apply(false);
    }

    // Если хочешь одной кнопкой (оставила)
    public void ToggleSound()
    {
        bool enabled = !LoadEnabled();
        SaveEnabled(enabled);
        Apply(enabled);
    }

    private bool LoadEnabled()
    {
        return PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
    }

    private void SaveEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SOUND_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void Apply(bool enabled)
    {
        // Глобально звук
        AudioListener.volume = enabled ? 1f : 0f;

        // Музыка меню
        if (menuMusic != null)
        {
            if (enabled)
            {
                if (!menuMusic.isPlaying) menuMusic.Play();
                else menuMusic.UnPause();
            }
            else
            {
                menuMusic.Pause();
            }
        }

        // Текст ON/OFF (если используешь)
        if (label != null)
            label.text = enabled ? onText : offText;

        // Glow включаем/выключаем
        if (offGlow != null) offGlow.SetActive(!enabled);
        if (onGlow != null) onGlow.SetActive(enabled);

        // Пульс только у активного glow
        if (offGlowAnimator != null) offGlowAnimator.enabled = !enabled;
        if (onGlowAnimator != null) onGlowAnimator.enabled = enabled;
    }
}