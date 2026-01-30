 using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private CanvasGroup dimOverlay;   // объект DimOverlay (на нем CanvasGroup)
    [SerializeField] private GameObject firstSelected; // можно оставить пустым

    [Header("Menu Buttons Root (freeze animations)")]
    [SerializeField] private GameObject menuButtonsRoot; // Canvas или объект-родитель кнопок (Play/Options/Quit)

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusic;    // MenuMusic -> AudioSource
    [Range(0f, 1f)][SerializeField] private float musicVolumeWhenOptionsOpen = 0.25f;
    [SerializeField] private bool pauseMusicInsteadOfLowering = false;

    [Header("Overlay")]
    [Range(0f, 1f)][SerializeField] private float dimAlpha = 0.55f;

    private bool isOpen;
    private float musicVolumeBefore;

    private Animator[] cachedAnimators;

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

        // кэшируем аниматоры на кнопках/меню, чтобы их замораживать
        if (menuButtonsRoot != null)
            cachedAnimators = menuButtonsRoot.GetComponentsInChildren<Animator>(true);

        if (menuMusic != null)
            musicVolumeBefore = menuMusic.volume;

        isOpen = false;
    }

    public void OpenOptions()
    {
        if (isOpen) return;
        isOpen = true;

        // 1) затемнение + блок кликов по меню под ним
        ShowDim(true);

        // 2) показываем панель опций
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        // 3) заморозка анимаций кнопок (и вообще всего меню, что анимируется через Animator)
        FreezeMenu(true);

        // 4) музыка: пауза или приглушение
        HandleMusic(true);

        // 5) фокус на первой кнопке внутри Options (для клавы/геймпада)
        if (firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void CloseOptions()
    {
        if (!isOpen) return;
        isOpen = false;

        // 1) скрываем панель
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // 2) убираем затемнение
        ShowDim(false);

        // 3) размораживаем меню
        FreezeMenu(false);

        // 4) возвращаем музыку
        HandleMusic(false);

        // 5) сброс выделения
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ToggleOptions()
    {
        if (isOpen) CloseOptions();
        else OpenOptions();
    }

    private void ShowDim(bool show)
    {
        if (dimOverlay == null) return;

        dimOverlay.gameObject.SetActive(true);
        dimOverlay.alpha = show ? dimAlpha : 0f;

        // важно: когда открыт Options — блокируем клики по меню снизу
        dimOverlay.interactable = show;
        dimOverlay.blocksRaycasts = show;

        if (!show)
            dimOverlay.gameObject.SetActive(false);
    }

    private void FreezeMenu(bool freeze)
    {
        if (cachedAnimators == null) return;

        foreach (var a in cachedAnimators)
        {
            if (a == null) continue;

            // Останавливаем именно анимации меню-кнопок
            // (OptionsPanel можно не трогать — он отдельно)
            a.enabled = !freeze;
        }
    }

    private void HandleMusic(bool opening)
    {
        if (menuMusic == null) return;

        if (opening)
        {
            musicVolumeBefore = menuMusic.volume;

            if (pauseMusicInsteadOfLowering)
            {
                menuMusic.Pause();
            }
            else
            {
                menuMusic.volume = musicVolumeWhenOptionsOpen;
            }
        }
        else
        {
            if (pauseMusicInsteadOfLowering)
            {
                menuMusic.UnPause();
            }
            else
            {
                menuMusic.volume = musicVolumeBefore;
            }
        }
    }
}