using System;
using UnityEngine;

public static class LevelProgressSave
{
    private const string KeyPrefix = "ROLP_LevelStars_V1_";

    public static event Action BestStarsChanged;

    public static int GetBestStars(int level)
    {
        return level > 0 ? Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + level, 0), 0, 3) : 0;
    }

    // Call only when gameplay awards a result. Repeated or worse results do not write.
    public static bool RecordBestStars(int level, int stars)
    {
        if (level < 1 || stars < 0 || stars > 3 || !GameInstallationSave.HasCurrentInstallSave())
            return false;

        var previous = GetBestStars(level);
        var best = MergeBestStars(previous, stars);
        if (best == previous)
            return false;

        PlayerPrefs.SetInt(KeyPrefix + level, best);
        PlayerPrefs.Save();
        BestStarsChanged?.Invoke();
        return true;
    }

    internal static bool IsLevelUnlocked(int level, bool started, Func<int, int> bestStars)
    {
        return started && level > 0 && (level == 1 || bestStars(level - 1) >= 2);
    }

    internal static int MergeBestStars(int previous, int result)
    {
        return Mathf.Max(Mathf.Clamp(previous, 0, 3), Mathf.Clamp(result, 0, 3));
    }
}
