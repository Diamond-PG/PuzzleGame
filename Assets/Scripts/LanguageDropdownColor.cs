using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LanguageDropdownColor : MonoBehaviour
{
    [Serializable]
    public class LocaleColor
    {
        public string localeCode;   // "en", "ru", "de", "fr", "zh" ...
        public Color color = Color.white;
    }

    [Header("Optional (если не заполнено — возьмём из TMP_Dropdown автоматически)")]
    public TMP_Text selectedLabel;      // Dropdown caption text (Label)
    public TMP_Text itemLabelPrefab;    // Dropdown item text (Item Label in template)

    [Header("Colors per locale code (first match wins). Uses StartsWith, so 'zh' matches 'zh-CN'")]
    public List<LocaleColor> colors = new List<LocaleColor>();

    private TMP_Dropdown dropdown;
    private bool initialized;

    private int lastColoredListInstanceId = -1;
    private int lastTryFrame = -1;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();

        if (dropdown != null)
        {
            if (selectedLabel == null)
                selectedLabel = dropdown.captionText;

            if (itemLabelPrefab == null)
                itemLabelPrefab = dropdown.itemText;
        }
    }

    private void OnEnable()
    {
        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnDropdownChanged);

        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        StartCoroutine(InitAndRefresh());
    }

    private void OnDisable()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);

        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private IEnumerator InitAndRefresh()
    {
        AsyncOperationHandle initOp = LocalizationSettings.InitializationOperation;
        if (initOp.IsValid() && !initOp.IsDone)
            yield return initOp;

        initialized = true;

        // Дадим локализации обновить тексты, и только потом красим
        yield return null;
        yield return null;

        RefreshCaptionOnly();
    }

    private void OnDropdownChanged(int index)
    {
        if (!initialized) return;
        StartCoroutine(DelayedCaptionRefresh());
    }

    private void OnSelectedLocaleChanged(Locale _)
    {
        if (!initialized) return;
        StartCoroutine(DelayedCaptionRefresh());
    }

    private IEnumerator DelayedCaptionRefresh()
    {
        yield return null;
        yield return null;
        RefreshCaptionOnly();
    }

    private void LateUpdate()
    {
        if (!initialized) return;

        if (Time.frameCount == lastTryFrame) return;
        lastTryFrame = Time.frameCount;

        TryColorOpenedDropdownList();
    }

    private void RefreshCaptionOnly()
    {
        if (LocalizationSettings.SelectedLocale == null) return;

        string selectedCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        Color selectedColor = GetColorFor(selectedCode);

        if (selectedLabel != null)
            selectedLabel.color = selectedColor;

        if (itemLabelPrefab != null)
            itemLabelPrefab.color = selectedColor;

        if (dropdown != null)
            dropdown.RefreshShownValue();
    }

    private void TryColorOpenedDropdownList()
    {
        var list = FindOpenedDropdownList();
        if (list == null) return;

        int id = list.GetInstanceID();
        if (id == lastColoredListInstanceId) return;

        var content = list.transform.Find("Viewport/Content");
        if (content == null) return;

        var toggles = content.GetComponentsInChildren<Toggle>(true);
        if (toggles == null || toggles.Length == 0) return;

        var locales = LocalizationSettings.AvailableLocales?.Locales;
        if (locales == null || locales.Count == 0) return;

        int count = Mathf.Min(toggles.Length, locales.Count);

        for (int i = 0; i < count; i++)
        {
            var t = toggles[i];
            if (t == null) continue;

            TMP_Text label = t.GetComponentInChildren<TMP_Text>(true);
            if (label == null) continue;

            string code = locales[i].Identifier.Code;
            label.color = GetColorFor(code);
        }

        lastColoredListInstanceId = id;
    }

    private GameObject FindOpenedDropdownList()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            Transform found = FindChildDeep(parentCanvas.transform, "Dropdown List");
            if (found != null && found.gameObject.activeInHierarchy)
                return found.gameObject;
        }

        // ✅ Новый API (без obsolete warning)
        Transform[] all = UnityEngine.Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < all.Length; i++)
        {
            var tr = all[i];
            if (tr == null) continue;
            if (!tr.gameObject.activeInHierarchy) continue;
            if (!string.Equals(tr.name, "Dropdown List", StringComparison.Ordinal)) continue;

            if (tr.Find("Viewport/Content") != null)
                return tr.gameObject;
        }

        return null;
    }

    private static Transform FindChildDeep(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            var found = FindChildDeep(child, name);
            if (found != null) return found;
        }

        return null;
    }

    private Color GetColorFor(string code)
    {
        if (string.IsNullOrEmpty(code))
            return Color.white;

        foreach (var entry in colors)
        {
            if (entry == null || string.IsNullOrEmpty(entry.localeCode))
                continue;

            if (code.StartsWith(entry.localeCode, StringComparison.OrdinalIgnoreCase))
                return entry.color;
        }

        return Color.white;
    }
}