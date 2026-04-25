using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private CanvasGroup dimOverlay;     // объект DimOverlay (на нем CanvasGroup)
    [SerializeField] private GameObject firstSelected;   // можно оставить пустым

    [Header("Menu Buttons Root (freeze animations)")]
    [SerializeField] private GameObject menuButtonsRoot; // Canvas или объект-родитель кнопок меню

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusic;      // MenuMusic -> AudioSource
    [Range(0f, 1f)] [SerializeField] private float musicVolumeWhenOptionsOpen = 0.25f;
    [SerializeField] private bool pauseMusicInsteadOfLowering = false;

    [Header("Overlay")]
    [Range(0f, 1f)] [SerializeField] private float dimAlpha = 0.55f;

    [Header("Vibration UI (Glow)")]
    [SerializeField] private GameObject vibrationOnGlow;   // OnGlow
    [SerializeField] private GameObject vibrationOffGlow;  // OffGlow

    [Header("Vibration Safety")]
    [Tooltip("Защита от двойного клика/двойного OnClick в одном кадре")]
    [SerializeField] private float vibrationClickCooldown = 0.15f;

    // -------------------------

    private bool isOpen;
    private float musicVolumeBefore;
    private Animator[] cachedAnimators;

    private float lastVibrationClickTime = -999f;

    private void Awake()
    {
        // закрываем всё при старте (чтобы не перекрывало клики)
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (dimOverlay != null)
        {
            dimOverlay.alpha = 0f;
            dimOverlay.interactable = false;
            dimOverlay.blocksRaycasts = false;
            dimOverlay.gameObject.SetActive(false);
        }

        // кешируем аниматоры на кнопках/меню, чтобы их замораживать
        if (menuButtonsRoot != null)
            cachedAnimators = menuButtonsRoot.GetComponentsInChildren<Animator>(true);

        if (menuMusic != null)
            musicVolumeBefore = menuMusic.volume;

        isOpen = false;

        // если glow не назначены — попробуем найти автоматически внутри OptionsPanel
        AutoFindVibrationGlowsIfMissing();

        // синхронизация визуала вибрации с MicroHaptics
        UpdateVibrationVisuals(GetVibrationEnabled());
    }

    // -------------------------
    // Options open/close
    // -------------------------

    public void OpenOptions()
    {
        if (isOpen) return;
        isOpen = true;

        ShowDim(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        FreezeMenu(true);
        HandleMusic(true);

        if (firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected);

        // обновим визуал вибрации при открытии
        UpdateVibrationVisuals(GetVibrationEnabled());
    }

    public void CloseOptions()
    {
        if (!isOpen) return;
        isOpen = false;

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        ShowDim(false);
        FreezeMenu(false);
        HandleMusic(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ToggleOptions()
    {
        if (isOpen) CloseOptions();
        else OpenOptions();
    }

    // -------------------------
    // Dim Overlay
    // -------------------------

    private void ShowDim(bool show)
    {
        if (dimOverlay == null) return;

        dimOverlay.gameObject.SetActive(true);
        dimOverlay.alpha = show ? dimAlpha : 0f;

        // когда открыт Options — блокируем клики по меню снизу
        dimOverlay.interactable = show;
        dimOverlay.blocksRaycasts = show;

        if (!show)
            dimOverlay.gameObject.SetActive(false);
    }

    // -------------------------
    // Freeze menu animations
    // -------------------------

    private void FreezeMenu(bool freeze)
    {
        if (cachedAnimators == null) return;

        foreach (var a in cachedAnimators)
        {
            if (a == null) continue;
            a.enabled = !freeze;
        }
    }

    // -------------------------
    // Music
    // -------------------------

    private void HandleMusic(bool opening)
    {
        if (menuMusic == null) return;

        if (opening)
        {
            musicVolumeBefore = menuMusic.volume;

            if (pauseMusicInsteadOfLowering)
                menuMusic.Pause();
            else
                menuMusic.volume = musicVolumeWhenOptionsOpen;
        }
        else
        {
            if (pauseMusicInsteadOfLowering)
                menuMusic.UnPause();
            else
                menuMusic.volume = musicVolumeBefore;
        }
    }

    // -------------------------
    // VIBRATION (через MicroHaptics)
    // -------------------------

    // НЕ вешай это на OnHitbox/OffHitbox. Это общий тумблер.
    public void ToggleVibration()
    {
        if (!VibrationCooldownPassed()) return;

        bool enabledNow = MicroHaptics.IsEnabled();
        bool newValue = !enabledNow;

        MicroHaptics.SetEnabled(newValue);
        UpdateVibrationVisuals(newValue);

        // приятный клик только когда включили
        if (newValue) MicroHaptics.TinyClick();
    }

    // ВЕШАЙ ЭТО НА OnHitbox
    public void SetVibrationOn()
    {
        if (!VibrationCooldownPassed()) return;

        MicroHaptics.SetEnabled(true);
        UpdateVibrationVisuals(true);

        // приятный клик при включении
        MicroHaptics.TinyClick();
    }

    // ВЕШАЙ ЭТО НА OffHitbox
    public void SetVibrationOff()
    {
        if (!VibrationCooldownPassed()) return;

        MicroHaptics.SetEnabled(false);
        UpdateVibrationVisuals(false);
    }

    private bool GetVibrationEnabled()
    {
        return MicroHaptics.IsEnabled();
    }

    private void UpdateVibrationVisuals(bool isOn)
    {
        if (vibrationOnGlow != null) vibrationOnGlow.SetActive(isOn);
        if (vibrationOffGlow != null) vibrationOffGlow.SetActive(!isOn);
    }

    // -------------------------
    // Helpers
    // -------------------------

    private bool VibrationCooldownPassed()
    {
        if (Time.unscaledTime - lastVibrationClickTime < vibrationClickCooldown)
            return false;

        lastVibrationClickTime = Time.unscaledTime;
        return true;
    }

    private void AutoFindVibrationGlowsIfMissing()
    {
        if (optionsPanel == null) return;

        if (vibrationOnGlow == null)
        {
            var t = optionsPanel.transform.Find("OnGlow");
            if (t != null) vibrationOnGlow = t.gameObject;
        }

        if (vibrationOffGlow == null)
        {
            var t = optionsPanel.transform.Find("OffGlow");
            if (t != null) vibrationOffGlow = t.gameObject;
        }
    }
}