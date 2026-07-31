using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private CanvasGroup dimOverlay;
    [SerializeField] private GameObject firstSelected;

    [Header("Menu Buttons Root (freeze animations)")]
    [SerializeField] private GameObject menuButtonsRoot;

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusic;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolumeWhenOptionsOpen = 0.25f;

    [SerializeField] private bool pauseMusicInsteadOfLowering = false;

    [Header("Overlay")]
    [Range(0f, 1f)]
    [SerializeField] private float dimAlpha = 0.55f;

    [Header("Vibration UI (Glow)")]
    [SerializeField] private GameObject vibrationOnGlow;
    [SerializeField] private GameObject vibrationOffGlow;

    [Header("Vibration Safety")]
    [Tooltip("Защита от двойного клика или двойного OnClick.")]
    [SerializeField] private float vibrationClickCooldown = 0.15f;

    private bool isOpen;
    private float musicVolumeBefore;
    private Animator[] cachedAnimators;

    private float lastVibrationClickTime = -999f;

    private void Awake()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (dimOverlay != null)
        {
            dimOverlay.alpha = 0f;
            dimOverlay.interactable = false;
            dimOverlay.blocksRaycasts = false;
            dimOverlay.gameObject.SetActive(false);
        }

        if (menuButtonsRoot != null)
        {
            cachedAnimators =
                menuButtonsRoot.GetComponentsInChildren<Animator>(true);
        }

        if (menuMusic != null)
            musicVolumeBefore = menuMusic.volume;

        isOpen = false;

        AutoFindVibrationGlowsIfMissing();
        UpdateVibrationVisuals(GetVibrationEnabled());
    }

    // =====================================================
    // OPTIONS OPEN / CLOSE
    // =====================================================

    public void OpenOptions()
    {
        if (isOpen)
            return;

        isOpen = true;

        ShowDim(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        FreezeMenu(true);
        HandleMusic(true);

        if (firstSelected != null &&
            EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        UpdateVibrationVisuals(GetVibrationEnabled());
    }

    public void CloseOptions()
    {
        if (!isOpen)
            return;

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
        if (isOpen)
            CloseOptions();
        else
            OpenOptions();
    }

    // =====================================================
    // DIM OVERLAY
    // =====================================================

    private void ShowDim(bool show)
    {
        if (dimOverlay == null)
            return;

        dimOverlay.gameObject.SetActive(true);
        dimOverlay.alpha = show ? dimAlpha : 0f;
        dimOverlay.interactable = show;
        dimOverlay.blocksRaycasts = show;

        if (!show)
            dimOverlay.gameObject.SetActive(false);
    }

    // =====================================================
    // FREEZE MENU ANIMATIONS
    // =====================================================

    private void FreezeMenu(bool freeze)
    {
        if (cachedAnimators == null)
            return;

        foreach (Animator animator in cachedAnimators)
        {
            if (animator == null)
                continue;

            animator.enabled = !freeze;
        }
    }

    // =====================================================
    // MUSIC
    // =====================================================

    private void HandleMusic(bool opening)
    {
        if (menuMusic == null)
            return;

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

    // =====================================================
    // VIBRATION
    // =====================================================

    public void ToggleVibration()
    {
        if (!VibrationCooldownPassed())
            return;

        bool enabledBefore = MicroHaptics.IsEnabled();
        bool newValue = !enabledBefore;

        /*
         * Если вибрацию выключаем, UIHapticsOnPress уже успевает
         * дать импульс на PointerDown до выполнения этого метода.
         *
         * Если вибрацию включаем из выключенного состояния,
         * PointerDown не мог дать импульс, поэтому после включения
         * даём один тестовый TinyClick.
         */
        MicroHaptics.SetEnabled(newValue);
        UpdateVibrationVisuals(newValue);

        if (newValue && !enabledBefore)
            MicroHaptics.TinyClick();
    }

    public void SetVibrationOn()
    {
        if (!VibrationCooldownPassed())
            return;

        bool wasEnabled = MicroHaptics.IsEnabled();

        MicroHaptics.SetEnabled(true);
        UpdateVibrationVisuals(true);

        /*
         * Даём тестовый импульс только тогда, когда вибрация
         * действительно была выключена.
         *
         * Если она уже была включена, импульс уже пришёл
         * от UIHapticsOnPress.
         */
        if (!wasEnabled)
            MicroHaptics.TinyClick();
    }

    public void SetVibrationOff()
    {
        if (!VibrationCooldownPassed())
            return;

        /*
         * UIHapticsOnPress срабатывает раньше OnClick,
         * поэтому пользователь сначала почувствует нажатие,
         * а затем вибрация отключится.
         */
        MicroHaptics.SetEnabled(false);
        UpdateVibrationVisuals(false);
    }

    private bool GetVibrationEnabled()
    {
        return MicroHaptics.IsEnabled();
    }

    private void UpdateVibrationVisuals(bool isOn)
    {
        if (vibrationOnGlow != null)
            vibrationOnGlow.SetActive(isOn);

        if (vibrationOffGlow != null)
            vibrationOffGlow.SetActive(!isOn);
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private bool VibrationCooldownPassed()
    {
        if (Time.unscaledTime - lastVibrationClickTime <
            vibrationClickCooldown)
        {
            return false;
        }

        lastVibrationClickTime = Time.unscaledTime;
        return true;
    }

    private void AutoFindVibrationGlowsIfMissing()
    {
        if (optionsPanel == null)
            return;

        if (vibrationOnGlow == null)
        {
            Transform onGlow =
                optionsPanel.transform.Find("OnGlow");

            if (onGlow != null)
                vibrationOnGlow = onGlow.gameObject;
        }

        if (vibrationOffGlow == null)
        {
            Transform offGlow =
                optionsPanel.transform.Find("OffGlow");

            if (offGlow != null)
                vibrationOffGlow = offGlow.gameObject;
        }
    }
}