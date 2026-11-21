using System;
using UnityEngine;

public static class LevelProgressSave
{
    private const string KeyPrefix = "ROLP_LevelStars_V1_";
    private const string ObjectivePrefix = "ROLP_LevelStarObjectives_V1_";
    private const string HighestLevelKey = "ROLP_LevelStars_V1_HighestLevel";

    internal enum Objective { TutorialHeart = 1, CollectionPiece = 2, Finish = 4 }

    public static event Action BestStarsChanged;

    public static int GetBestStars(int level)
    {
        return level > 0 && GameInstallationSave.HasCurrentInstallSave()
            ? Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + level, 0), 0, 3)
            : 0;
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
        RememberLevel(level);
        PlayerPrefs.Save();
        BestStarsChanged?.Invoke();
        return true;
    }

    internal static bool IsLevelUnlocked(int level, bool started, Func<int, int> bestStars)
    {
        return started && (level == 1 || (level > 1 && bestStars(level - 1) == 3));
    }

    internal static bool HasObjective(int level, Objective objective) =>
        GameInstallationSave.HasCurrentInstallSave() &&
        (PlayerPrefs.GetInt(ObjectivePrefix + level, 0) & (int)objective) != 0;

    internal static void RecordObjective(int level, Objective objective)
    {
        if (level < 1 || !GameInstallationSave.HasCurrentInstallSave())
            return;

        string key = ObjectivePrefix + level;
        int previous = PlayerPrefs.GetInt(key, 0) & 7;
        int next = previous | (int)objective;
        if (next == previous)
            return;

        PlayerPrefs.SetInt(key, next);
        RememberLevel(level);
        int stars = (next & 1) + ((next >> 1) & 1) + ((next >> 2) & 1);
        if (!RecordBestStars(level, stars))
            PlayerPrefs.Save();
    }

    internal static void ResetAllProgress()
    {
        int lastLevel = Mathf.Max(10, PlayerPrefs.GetInt(HighestLevelKey, 0));
        for (int level = 1; level <= lastLevel; level++)
        {
            PlayerPrefs.DeleteKey(KeyPrefix + level);
            PlayerPrefs.DeleteKey(ObjectivePrefix + level);
        }

        PlayerPrefs.DeleteKey(HighestLevelKey);
        GameInstallationSave.ClearStartedGame();
        PlayerPrefs.Save();
        BestStarsChanged?.Invoke();
    }

    internal static void RetainCollectedPuzzleObjective(int level)
    {
        // New Game clears the displayed result. Keep the objective behind the
        // permanent pickup so the next new objective can still reach three stars.
        PlayerPrefs.SetInt(ObjectivePrefix + level, (int)Objective.CollectionPiece);
        PlayerPrefs.Save();
    }

    private static void RememberLevel(int level)
    {
        if (level > PlayerPrefs.GetInt(HighestLevelKey, 0))
            PlayerPrefs.SetInt(HighestLevelKey, level);
    }

    internal static int MergeBestStars(int previous, int result)
    {
        return Mathf.Max(Mathf.Clamp(previous, 0, 3), Mathf.Clamp(result, 0, 3));
    }
}
