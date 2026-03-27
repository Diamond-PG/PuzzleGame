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
    [SerializeField] private TMP_Dropdown dropdown;          // твой TMP_Dropdown
    [SerializeField] private TMP_Text languageValueLabel;    // LanguageValue (TMP) - можно null

    [Header("Dropdown index -> Locale code (Identifier.Code)")]
    // ДОЛЖНО совпадать с порядком опций в Dropdown
    [SerializeField] private string[] dropdownLocaleCodes = { "en", "ru", "de", "es", "fr", "zh" };

    private const string PREF_LOCALE = "selected-locale";

    private bool _initialized;
    private bool _ignoreDropdownCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(InitRoutine());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // При смене сцен UI может пересоздаваться — перепривязываем
        TryBindUI();
        RefreshUI();
    }

    private IEnumerator InitRoutine()
    {
        yield return LocalizationSettings.InitializationOperation;

        Debug.Log($"[LanguageManager] Init in scene='{SceneManager.GetActiveScene().name}'. AvailableLocales: {DumpLocales()}");

        // 1) применяем сохранённый язык (если есть)
        var saved = PlayerPrefs.GetString(PREF_LOCALE, "");
        if (!string.IsNullOrEmpty(saved))
        {
            var savedLocale = FindLocale(saved);
            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;
                Debug.Log($"[LanguageManager] Applied saved locale '{saved}'.");
            }
            else
            {
                Debug.LogWarning($"[LanguageManager] Saved locale '{saved}' not found. Keeping default.");
            }
        }
        else
        {
            Debug.Log("[LanguageManager] No saved locale key yet.");
        }

        _initialized = true;

        TryBindUI();
        RefreshUI();

        // На всякий случай подписка на изменение локали (если кто-то меняет её извне)
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        Debug.Log($"[LanguageManager] Init done. Selected='{GetSelectedCode()}', Saved='{PlayerPrefs.GetString(PREF_LOCALE, "")}'");
    }

    private void OnSelectedLocaleChanged(Locale obj)
    {
        RefreshUI();
    }

    private void TryBindUI()
    {
        // Если не назначено руками — найдём в сцене
        if (dropdown == null)
        {
            dropdown = Object.FindFirstObjectByType<TMP_Dropdown>(FindObjectsInactive.Include);
        }

        if (languageValueLabel == null)
        {
            // Пытаемся найти именно объект с именем LanguageValue (как у тебя в иерархии)
            var allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTexts)
            {
                if (t != null && t.name == "LanguageValue") { languageValueLabel = t; break; }
            }
        }

        // Привязка listener-а ТОЛЬКО кодом
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        if (_ignoreDropdownCallback) return;

        // КЛЮЧЕВАЯ ДИАГНОСТИКА: кто дернул dropdown?
        Debug.Log($"[LanguageManager] Dropdown changed -> index={index}, option='{GetOptionText(index)}' | Stack:\n{System.Environment.StackTrace}");

        SetLanguageByIndex(index);
    }

    public void SetLanguageByIndex(int index)
    {
        StartCoroutine(SetLanguageByIndexRoutine(index));
    }

    private IEnumerator SetLanguageByIndexRoutine(int index)
    {
        if (!_initialized)
            yield return LocalizationSettings.InitializationOperation;

        if (dropdownLocaleCodes == null || dropdownLocaleCodes.Length == 0)
        {
            Debug.LogWarning("[LanguageManager] dropdownLocaleCodes is EMPTY!");
            yield break;
        }

        index = Mathf.Clamp(index, 0, dropdownLocaleCodes.Length - 1);
        var code = dropdownLocaleCodes[index];

        var locale = FindLocale(code);
        if (locale == null)
        {
            Debug.LogWarning($"[LanguageManager] Locale '{code}' not found. Available: {DumpLocales()}");
            yield break;
        }

        LocalizationSettings.SelectedLocale = locale;

        PlayerPrefs.SetString(PREF_LOCALE, locale.Identifier.Code);
        PlayerPrefs.Save();

        RefreshUI();

        Debug.Log($"[LanguageManager] Applied locale='{locale.Identifier.Code}' by dropdown index={index}. Saved.");
    }

    private void RefreshUI()
    {
        // label
        if (languageValueLabel != null)
        {
            languageValueLabel.text = LocalizationSettings.SelectedLocale != null
                ? LocalizationSettings.SelectedLocale.LocaleName
                : "NULL";
        }

        // dropdown sync (без триггера события)
        if (dropdown != null)
        {
            int idx = GetDropdownIndexForCurrentLocale();

            _ignoreDropdownCallback = true;
            dropdown.SetValueWithoutNotify(idx);
            dropdown.RefreshShownValue();
            _ignoreDropdownCallback = false;

            Debug.Log($"[LanguageManager] UI Refresh. Selected='{GetSelectedCode()}', dropdownIndex={idx}");
        }
        else
        {
            Debug.LogWarning("[LanguageManager] UI Refresh: dropdown is NULL (not found / not assigned).");
        }
    }

    private Locale FindLocale(string code)
    {
        var locales = LocalizationSettings.AvailableLocales?.Locales;
        if (locales == null) return null;

        // exact
        var exact = locales.FirstOrDefault(l => l != null && l.Identifier.Code == code);
        if (exact != null) return exact;

        // safe fallback: "zh" matches "zh-CN", "fr" matches "fr-FR" etc
        return locales.FirstOrDefault(l =>
            l != null &&
            (l.Identifier.Code == code || l.Identifier.Code.StartsWith(code + "-")));
    }

    private int GetDropdownIndexForCurrentLocale()
    {
        string code = GetSelectedCode();
        if (string.IsNullOrEmpty(code)) return 0;

        int idx = System.Array.FindIndex(dropdownLocaleCodes, c =>
            code == c || code.StartsWith(c + "-"));

        return idx >= 0 ? idx : 0;
    }

    private string GetSelectedCode()
    {
        return LocalizationSettings.SelectedLocale != null
            ? LocalizationSettings.SelectedLocale.Identifier.Code
            : "";
    }

    private string DumpLocales()
    {
        var locales = LocalizationSettings.AvailableLocales?.Locales;
        if (locales == null) return "NULL";

        return string.Join(", ", locales
            .Where(l => l != null)
            .Select(l => $"{l.Identifier.Code}({l.LocaleName})"));
    }

    private string GetOptionText(int index)
    {
        if (dropdown == null) return "NULL_DROPDOWN";
        if (dropdown.options == null || dropdown.options.Count == 0) return "NO_OPTIONS";
        if (index < 0 || index >= dropdown.options.Count) return "OUT_OF_RANGE";
        return dropdown.options[index]?.text ?? "NULL_TEXT";
    }
}