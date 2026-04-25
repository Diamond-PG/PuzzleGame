using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

#if UNITY_LOCALIZATION
using UnityEngine.Localization.Settings; // just to be safe in some setups
#endif

/// <summary>
/// Startup locale selector that reads locale code from PlayerPrefs ("locale_code").
/// If value not found or locale missing, returns null so Unity falls back to Project Locale Identifier.
/// </summary>
[System.Serializable]
public class PlayerPrefsLocaleSelector : IStartupLocaleSelector
{
    private const string PREF_LOCALE = "locale_code";

    // Higher priority than CommandLine? NO.
    // We want CommandLine to override if used, so give this a lower priority number.
    // In Unity Localization, higher Priority usually wins. We'll set it to 0 and CommandLine is typically higher.
    public int Priority => 0;

    public Locale GetStartupLocale(ILocalesProvider availableLocales)
    {
        if (availableLocales == null) return null;

        string savedCode = PlayerPrefs.GetString(PREF_LOCALE, "");
        if (string.IsNullOrEmpty(savedCode))
            return null; // fallback to Project Locale Identifier

        var locales = availableLocales.Locales;
        if (locales == null) return null;

        for (int i = 0; i < locales.Count; i++)
        {
            var l = locales[i];
            if (l == null) continue;

            if (string.Equals(l.Identifier.Code, savedCode, System.StringComparison.OrdinalIgnoreCase))
                return l;
        }

        return null; // fallback
    }
}