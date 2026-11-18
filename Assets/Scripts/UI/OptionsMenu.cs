
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject firstSelected;

    [Header("Main Menu")]
    [SerializeField] private GameObject menuButtonsRoot;

    [Header("Freeze Main Menu")]
    [SerializeField] private bool pauseMenuTime = true;

    [Tooltip("Дополнительные скрипты анимаций, которые используют unscaledDeltaTime. Не добавляйте сюда UIManager или OptionsMenu.")]
    [SerializeField]
    private MonoBehaviour[] additionalVisualScriptsToPause;

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusic;

    [Range(0f, 1f)]
    [SerializeField]
    private float musicVolumeWhenOptionsOpen = 0.25f;

    [SerializeField]
    private bool pauseMusicInsteadOfLowering = false;

    [Header("Vibration UI")]
    [SerializeField] private GameObject vibrationOnGlow;
    [SerializeField] private GameObject vibrationOffGlow;

    [Header("Vibration Safety")]
    [Range(0f, 1f)]
    [SerializeField]
    private float vibrationClickCooldown = 0.15f;

    private bool isOpen;

    private float musicVolumeBefore;
    private bool musicWasPlaying;

    private float timeScaleBefore = 1f;
    private bool timeScaleWasChanged;

    private float lastVibrationClickTime = -999f;

    private CanvasGroup menuButtonsCanvasGroup;
    private bool previousMenuInteractable;
    private bool previousMenuBlocksRaycasts;
    private bool menuInputWasChanged;

    private Animator[] cachedAnimators;

    private readonly Dictionary<Animator, float>
        animatorSpeedsBefore = new Dictionary<Animator, float>();

    private bool[] additionalScriptsEnabledBefore;

    private void Awake()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);

            Image panelImage = optionsPanel.GetComponent<Image>();

            if (panelImage != null)
                panelImage.raycastTarget = true;
        }

        if (menuButtonsRoot != null)
        {
            cachedAnimators =
                menuButtonsRoot.GetComponentsInChildren<Animator>(true);

            menuButtonsCanvasGroup =
                menuButtonsRoot.GetComponent<CanvasGroup>();
        }

        if (menuMusic != null)
            musicVolumeBefore = menuMusic.volume;

        AutoFindVibrationGlowsIfMissing();
        UpdateVibrationVisuals(GetVibrationEnabled());

        isOpen = false;
    }

    // =====================================================
    // OPTIONS
    // =====================================================

    public void OpenOptions()
    {
        if (isOpen || optionsPanel == null)
            return;

        isOpen = true;

        optionsPanel.SetActive(true);

        CanvasGroup panelGroup =
            optionsPanel.GetComponent<CanvasGroup>();

        if (panelGroup != null)
        {
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        }

        FreezeMainMenu();
        HandleMusic(true);

        UpdateVibrationVisuals(GetVibrationEnabled());

        if (firstSelected != null &&
            EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    public void CloseOptions()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        RestoreMainMenu();
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
    // FREEZE MAIN MENU
    // =====================================================

    private void FreezeMainMenu()
    {
        // Block buttons behind Options.
        if (menuButtonsRoot != null)
        {
            if (menuButtonsCanvasGroup == null)
            {
                menuButtonsCanvasGroup =
                    menuButtonsRoot.AddComponent<CanvasGroup>();
            }

            previousMenuInteractable =
                menuButtonsCanvasGroup.interactable;

            previousMenuBlocksRaycasts =
                menuButtonsCanvasGroup.blocksRaycasts;

            menuButtonsCanvasGroup.interactable = false;
            menuButtonsCanvasGroup.blocksRaycasts = false;

            menuInputWasChanged = true;
        }

        // Stop Animator animations, including those
        // configured to use unscaled time.
        animatorSpeedsBefore.Clear();

        if (cachedAnimators != null)
        {
            foreach (Animator animator in cachedAnimators)
            {
                if (animator == null)
                    continue;

                animatorSpeedsBefore[animator] = animator.speed;
                animator.speed = 0f;
            }
        }

        // Stop explicitly assigned custom visual scripts.
        if (additionalVisualScriptsToPause != null)
        {
            additionalScriptsEnabledBefore =
                new bool[additionalVisualScriptsToPause.Length];

            for (int i = 0;
                 i < additionalVisualScriptsToPause.Length;
                 i++)
            {
                MonoBehaviour script =
                    additionalVisualScriptsToPause[i];

                if (script == null || script == this)
                    continue;

                additionalScriptsEnabledBefore[i] =
                    script.enabled;

                script.enabled = false;
            }
        }

        // Pause animations driven by normal Time.deltaTime.
        if (pauseMenuTime)
        {
            timeScaleBefore = Time.timeScale;
            Time.timeScale = 0f;
            timeScaleWasChanged = true;
        }
    }

    private void RestoreMainMenu()
    {
        if (timeScaleWasChanged)
        {
            Time.timeScale = timeScaleBefore;
            timeScaleWasChanged = false;
        }

        foreach (KeyValuePair<Animator, float> entry
                 in animatorSpeedsBefore)
        {
            if (entry.Key != null)
                entry.Key.speed = entry.Value;
        }

        animatorSpeedsBefore.Clear();

        if (additionalVisualScriptsToPause != null &&
            additionalScriptsEnabledBefore != null)
        {
            int count = Mathf.Min(
                additionalVisualScriptsToPause.Length,
                additionalScriptsEnabledBefore.Length);

            for (int i = 0; i < count; i++)
            {
                MonoBehaviour script =
                    additionalVisualScriptsToPause[i];

                if (script == null || script == this)
                    continue;

                script.enabled =
                    additionalScriptsEnabledBefore[i];
            }
        }

        additionalScriptsEnabledBefore = null;

        if (menuInputWasChanged &&
            menuButtonsCanvasGroup != null)
        {
            menuButtonsCanvasGroup.interactable =
                previousMenuInteractable;

            menuButtonsCanvasGroup.blocksRaycasts =
                previousMenuBlocksRaycasts;
        }

        menuInputWasChanged = false;
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
            musicWasPlaying = menuMusic.isPlaying;

            if (pauseMusicInsteadOfLowering)
            {
                if (musicWasPlaying)
                    menuMusic.Pause();
            }
            else
            {
                menuMusic.volume =
                    musicVolumeWhenOptionsOpen;
            }
        }
        else
        {
            if (pauseMusicInsteadOfLowering)
            {
                if (musicWasPlaying)
                    menuMusic.UnPause();
            }
            else
            {
                menuMusic.volume = musicVolumeBefore;
            }
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

        if (!wasEnabled)
            MicroHaptics.TinyClick();
    }

    public void SetVibrationOff()
    {
        if (!VibrationCooldownPassed())
            return;

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

    // =====================================================
    // AUTO FIND VIBRATION GLOWS
    // =====================================================

    private void AutoFindVibrationGlowsIfMissing()
    {
        if (optionsPanel == null)
            return;

        Transform windowRoot =
            optionsPanel.transform.Find("OptionsWindowRoot");

        if (windowRoot == null)
            return;

        if (vibrationOnGlow == null)
        {
            Transform found =
                windowRoot.Find("VibrationOnGlow");

            if (found != null)
                vibrationOnGlow = found.gameObject;
        }

        if (vibrationOffGlow == null)
        {
            Transform found =
                windowRoot.Find("VibrationOffGlow");

            if (found != null)
                vibrationOffGlow = found.gameObject;
        }
    }

    // =====================================================
    // SAFETY
    // =====================================================

    private void OnDisable()
    {
        if (!isOpen)
            return;

        isOpen = false;

        RestoreMainMenu();
        HandleMusic(false);
    }

    private void OnDestroy()
    {
        if (timeScaleWasChanged ||
            menuInputWasChanged ||
            animatorSpeedsBefore.Count > 0)
        {
            RestoreMainMenu();
        }
    }
}
