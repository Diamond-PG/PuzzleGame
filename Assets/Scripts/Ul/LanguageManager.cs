using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    [Header("UI (can be null - will auto-find)")]
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private TMP_Text languageValueLabel;

    [Header("Dropdown index -> Locale code")]
    [SerializeField]
    private string[] dropdownLocaleCodes =
    {
        "en",
        "ru",
        "de",
        "es",
        "fr",
        "zh"
    };

    [Header("Language Selection Haptics")]
    [Tooltip("Короткая вибрация при выборе конкретного языка.")]
    [SerializeField] private bool useLanguageSelectionHaptics = true;

    private const string PREF_LOCALE = "selected-locale";

    private bool initialized;
    private bool ignoreDropdownCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(InitRoutine());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        LocalizationSettings.SelectedLocaleChanged -=
            OnSelectedLocaleChanged;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        TryBindUI();
        RefreshUI();
    }

    private IEnumerator InitRoutine()
    {
        yield return LocalizationSettings.InitializationOperation;

        Debug.Log(
            "[LanguageManager] Init in scene='" +
            SceneManager.GetActiveScene().name +
            "'. AvailableLocales: " +
            DumpLocales()
        );

        string saved =
            PlayerPrefs.GetString(
                PREF_LOCALE,
                ""
            );

        if (!string.IsNullOrEmpty(saved))
        {
            Locale savedLocale =
                FindLocale(saved);

            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale =
                    savedLocale;

                Debug.Log(
                    "[LanguageManager] Applied saved locale '" +
                    saved +
                    "'."
                );
            }
            else
            {
                Debug.LogWarning(
                    "[LanguageManager] Saved locale '" +
                    saved +
                    "' not found. Keeping default."
                );
            }
        }
        else
        {
            Debug.Log(
                "[LanguageManager] No saved locale key yet."
            );
        }

        initialized = true;

        TryBindUI();
        RefreshUI();

        LocalizationSettings.SelectedLocaleChanged -=
            OnSelectedLocaleChanged;

        LocalizationSettings.SelectedLocaleChanged +=
            OnSelectedLocaleChanged;

        Debug.Log(
            "[LanguageManager] Init done. Selected='" +
            GetSelectedCode() +
            "', Saved='" +
            PlayerPrefs.GetString(PREF_LOCALE, "") +
            "'"
        );
    }

    private void OnSelectedLocaleChanged(Locale locale)
    {
        RefreshUI();
    }

    private void TryBindUI()
    {
        if (dropdown == null)
        {
            dropdown =
                Object.FindFirstObjectByType<TMP_Dropdown>(
                    FindObjectsInactive.Include
                );
        }

        if (languageValueLabel == null)
        {
            TMP_Text[] allTexts =
                Object.FindObjectsByType<TMP_Text>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (TMP_Text textObject in allTexts)
            {
                if (textObject != null &&
                    textObject.name == "LanguageValue")
                {
                    languageValueLabel = textObject;
                    break;
                }
            }
        }

        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(
                OnDropdownValueChanged
            );

            dropdown.onValueChanged.AddListener(
                OnDropdownValueChanged
            );
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        if (ignoreDropdownCallback)
            return;

        Debug.Log(
            "[LanguageManager] Dropdown changed -> index=" +
            index +
            ", option='" +
            GetOptionText(index) +
            "'"
        );

        /*
         * Срабатывает именно в момент нажатия
         * на конкретный язык в выпадающем списке.
         */
        PlayLanguageSelectionHaptic();

        SetLanguageByIndex(index);
    }

    private void PlayLanguageSelectionHaptic()
    {
        if (!useLanguageSelectionHaptics)
            return;

        /*
         * Такая же короткая вибрация,
         * как у кнопок главного меню.
         */
        MicroHaptics.TinyClick();
    }

    public void SetLanguageByIndex(int index)
    {
        StartCoroutine(
            SetLanguageByIndexRoutine(index)
        );
    }

    private IEnumerator SetLanguageByIndexRoutine(int index)
    {
        if (!initialized)
            yield return LocalizationSettings.InitializationOperation;

        if (dropdownLocaleCodes == null ||
            dropdownLocaleCodes.Length == 0)
        {
            Debug.LogWarning(
                "[LanguageManager] dropdownLocaleCodes is EMPTY!"
            );

            yield break;
        }

        index =
            Mathf.Clamp(
                index,
                0,
                dropdownLocaleCodes.Length - 1
            );

        string code =
            dropdownLocaleCodes[index];

        Locale locale =
            FindLocale(code);

        if (locale == null)
        {
            Debug.LogWarning(
                "[LanguageManager] Locale '" +
                code +
                "' not found. Available: " +
                DumpLocales()
            );

            yield break;
        }

        LocalizationSettings.SelectedLocale =
            locale;

        PlayerPrefs.SetString(
            PREF_LOCALE,
            locale.Identifier.Code
        );

        PlayerPrefs.Save();

        RefreshUI();

        Debug.Log(
            "[LanguageManager] Applied locale='" +
            locale.Identifier.Code +
            "' by dropdown index=" +
            index +
            ". Saved."
        );
    }

    private void RefreshUI()
    {
        if (languageValueLabel != null)
        {
            languageValueLabel.text =
                LocalizationSettings.SelectedLocale != null
                    ? LocalizationSettings.SelectedLocale.LocaleName
                    : "NULL";
        }

        if (dropdown != null)
        {
            int index =
                GetDropdownIndexForCurrentLocale();

            ignoreDropdownCallback = true;

            dropdown.SetValueWithoutNotify(index);
            dropdown.RefreshShownValue();

            ignoreDropdownCallback = false;

            Debug.Log(
                "[LanguageManager] UI Refresh. Selected='" +
                GetSelectedCode() +
                "', dropdownIndex=" +
                index
            );
        }
        else
        {
            Debug.LogWarning(
                "[LanguageManager] UI Refresh: dropdown is NULL."
            );
        }
    }

    private Locale FindLocale(string code)
    {
        var locales =
            LocalizationSettings.AvailableLocales?.Locales;

        if (locales == null)
            return null;

        Locale exact =
            locales.FirstOrDefault(
                locale =>
                    locale != null &&
                    locale.Identifier.Code == code
            );

        if (exact != null)
            return exact;

        return locales.FirstOrDefault(
            locale =>
                locale != null &&
                (
                    locale.Identifier.Code == code ||
                    locale.Identifier.Code.StartsWith(code + "-")
                )
        );
    }

    private int GetDropdownIndexForCurrentLocale()
    {
        string code =
            GetSelectedCode();

        if (string.IsNullOrEmpty(code))
            return 0;

        int index =
            System.Array.FindIndex(
                dropdownLocaleCodes,
                localeCode =>
                    code == localeCode ||
                    code.StartsWith(localeCode + "-")
            );

        return index >= 0 ? index : 0;
    }

    private string GetSelectedCode()
    {
        return LocalizationSettings.SelectedLocale != null
            ? LocalizationSettings.SelectedLocale.Identifier.Code
            : "";
    }

    private string DumpLocales()
    {
        var locales =
            LocalizationSettings.AvailableLocales?.Locales;

        if (locales == null)
            return "NULL";

        return string.Join(
            ", ",
            locales
                .Where(locale => locale != null)
                .Select(
                    locale =>
                        locale.Identifier.Code +
                        "(" +
                        locale.LocaleName +
                        ")"
                )
        );
    }

    private string GetOptionText(int index)
    {
        if (dropdown == null)
            return "NULL_DROPDOWN";

        if (dropdown.options == null ||
            dropdown.options.Count == 0)
        {
            return "NO_OPTIONS";
        }

        if (index < 0 ||
            index >= dropdown.options.Count)
        {
            return "OUT_OF_RANGE";
        }

        return dropdown.options[index]?.text ??
               "NULL_TEXT";
    }
}