using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInputController : MonoBehaviour
{
    [Header("NAME INPUT")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("CONFIRM BUTTON")]
    [SerializeField] private Button confirmNameButton;

    [Header("TUTORIAL TEXT")]
    [SerializeField] private TMP_Text tutorialText;

    [Header("TUTORIAL CONTROLLER")]
    [Tooltip("Контроллер текста и стрелок обучения.")]
    [SerializeField] private TutorialTextController tutorialTextController;

    [Header("AFTER NAME CONFIRM")]
    [TextArea(2, 4)]
    [SerializeField] private string textAfterName = "{name}, рад знакомству!";

    [Header("SAVE SETTINGS")]
    [SerializeField] private string playerNameSaveKey = "PlayerName";

    [Header("CONFIRM SOUND")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField, Range(0f, 1f)] private float confirmSoundVolume = 1f;

    [Header("CONFIRM HAPTICS")]
    [SerializeField] private bool useConfirmHaptics = true;

    [Header("NAME INPUT DIMMING")]
    [Tooltip("Image самой рамки TutorialFrame.")]
    [SerializeField] private Image tutorialFrameImage;

    [Tooltip("Image Хранителя PuzzleKeeper.")]
    [SerializeField] private Image puzzleKeeperImage;

    [Tooltip("Дополнительная картинка Хранителя, например KeeperPuzzleIcon.")]
    [SerializeField] private Image keeperPuzzleIconImage;

    [Tooltip("Насколько затемнять рамку и Хранителя во время ввода имени. 0 = чёрный, 1 = без затемнения.")]
    [SerializeField, Range(0f, 1f)] private float nameInputBrightness = 0.35f;

    private bool nameConfirmed = false;
    private bool nameInputVisible = false;

    private Color originalFrameColor = Color.white;
    private Color originalKeeperColor = Color.white;
    private Color originalKeeperPuzzleIconColor = Color.white;

    private bool originalColorsSaved = false;

    private void Awake()
    {
        if (confirmNameButton != null)
        {
            confirmNameButton.onClick.RemoveListener(ConfirmName);
            confirmNameButton.onClick.AddListener(ConfirmName);
        }

        SaveOriginalColors();
    }

    private void Start()
    {
        nameConfirmed = false;

        // ВАЖНО:
        // поле имени больше НЕ включаем здесь автоматически.
        // Им полностью управляет TutorialTextController.
        HideNameInput();
    }

    private void SaveOriginalColors()
    {
        if (tutorialFrameImage != null)
            originalFrameColor = tutorialFrameImage.color;

        if (puzzleKeeperImage != null)
            originalKeeperColor = puzzleKeeperImage.color;

        if (keeperPuzzleIconImage != null)
            originalKeeperPuzzleIconColor = keeperPuzzleIconImage.color;

        originalColorsSaved = true;
    }

    public void ShowNameInput()
    {
        if (nameConfirmed)
            return;

        if (nameInputVisible)
            return;

        nameInputVisible = true;

        if (nameInputField != null)
        {
            nameInputField.gameObject.SetActive(true);
            nameInputField.text = "";
            nameInputField.ActivateInputField();
        }

        if (confirmNameButton != null)
            confirmNameButton.gameObject.SetActive(true);

        ApplyNameInputDimming();
    }

    public void HideNameInput()
    {
        nameInputVisible = false;

        if (nameInputField != null)
        {
            nameInputField.DeactivateInputField();
            nameInputField.gameObject.SetActive(false);
        }

        if (confirmNameButton != null)
            confirmNameButton.gameObject.SetActive(false);

        RestoreOriginalColors();
    }

    private void ApplyNameInputDimming()
    {
        if (!originalColorsSaved)
            SaveOriginalColors();

        if (tutorialFrameImage != null)
        {
            tutorialFrameImage.color =
                GetDimmedColor(
                    originalFrameColor,
                    nameInputBrightness
                );
        }

        if (puzzleKeeperImage != null)
        {
            puzzleKeeperImage.color =
                GetDimmedColor(
                    originalKeeperColor,
                    nameInputBrightness
                );
        }

        if (keeperPuzzleIconImage != null)
        {
            keeperPuzzleIconImage.color =
                GetDimmedColor(
                    originalKeeperPuzzleIconColor,
                    nameInputBrightness
                );
        }
    }

    private Color GetDimmedColor(
        Color originalColor,
        float brightness
    )
    {
        brightness = Mathf.Clamp01(brightness);

        return new Color(
            originalColor.r * brightness,
            originalColor.g * brightness,
            originalColor.b * brightness,
            originalColor.a
        );
    }

    private void RestoreOriginalColors()
    {
        if (!originalColorsSaved)
            return;

        if (tutorialFrameImage != null)
            tutorialFrameImage.color = originalFrameColor;

        if (puzzleKeeperImage != null)
            puzzleKeeperImage.color = originalKeeperColor;

        if (keeperPuzzleIconImage != null)
        {
            keeperPuzzleIconImage.color =
                originalKeeperPuzzleIconColor;
        }
    }

    public void ConfirmName()
    {
        if (nameConfirmed)
            return;

        if (nameInputField == null)
        {
            Debug.LogWarning(
                "PlayerNameInputController: Name Input Field не назначен в Inspector."
            );

            return;
        }

        string playerName =
            nameInputField.text.Trim();

        // Пустое имя подтверждать нельзя.
        if (string.IsNullOrWhiteSpace(playerName))
        {
            nameInputField.ActivateInputField();
            return;
        }

        nameConfirmed = true;

        // Сохраняем имя игрока.
        PlayerPrefs.SetString(
            playerNameSaveKey,
            playerName
        );

        PlayerPrefs.Save();

        // Звук подтверждения.
        if (audioSource != null &&
            confirmSound != null)
        {
            audioSource.PlayOneShot(
                confirmSound,
                confirmSoundVolume
            );
        }

        // Короткая вибрация.
        if (useConfirmHaptics)
        {
            MicroHaptics.TinyClick();
        }

        // Сначала убираем поле и возвращаем
        // нормальную яркость рамки и Хранителя.
        HideNameInput();

        // Передаём ЧИСТОЕ имя главному
        // контроллеру диалога.
        if (tutorialTextController != null)
        {
            tutorialTextController.ConfirmPlayerName(
                playerName
            );
        }
        else if (tutorialText != null)
        {
            // Запасной вариант.
            string finalText =
                textAfterName.Replace(
                    "{name}",
                    playerName
                );

            tutorialText.text = finalText;
        }
    }

    public string GetPlayerName()
    {
        if (nameInputField != null &&
            !string.IsNullOrWhiteSpace(nameInputField.text))
        {
            return nameInputField.text.Trim();
        }

        return PlayerPrefs.GetString(
            playerNameSaveKey,
            ""
        );
    }

    public bool IsNameConfirmed()
    {
        return nameConfirmed;
    }

    public bool IsNameInputVisible()
    {
        return nameInputVisible;
    }

    private void OnDestroy()
    {
        if (confirmNameButton != null)
        {
            confirmNameButton.onClick.RemoveListener(
                ConfirmName
            );
        }

        RestoreOriginalColors();
    }
}