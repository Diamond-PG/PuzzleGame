using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelLaunchMode
{
    None,
    NewGame,
    Continue,
    GalleryReplay
}

public static class LevelLaunchSession
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string FirstLevelPath = "Assets/Scenes/Level1.unity";

    public static LevelLaunchMode Mode { get; private set; }
    public static int ReplayLevel { get; private set; }
    public static bool IsLoading { get; private set; }

    private static string[] replayCatalog;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        Reset();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    internal static void BeginStory(bool newGame)
    {
        Reset();
        Mode = newGame ? LevelLaunchMode.NewGame : LevelLaunchMode.Continue;
        TutorialProgressManager.RestoreLegacyMilestones();
    }

    internal static void ResetForNewGame()
    {
        GameInstallationSave.MarkNewGameStarted();
        TutorialProgressManager.ClearSavedProgress();
        LevelProgressSave.ResetAllProgress();
    }

    private static string LevelPath(int level) => $"Assets/Scenes/Level{level}.unity";

    internal static bool SupportsReplay(int level, string scenePath) =>
        string.Equals(scenePath, LevelPath(level), StringComparison.Ordinal);

    internal static bool TryStartReplay(int level, string[] scenePaths)
    {
        if (IsLoading || Mode != LevelLaunchMode.None ||
            !TryLoadReplayLevel(level, scenePaths))
            return false;

        if (Mode == LevelLaunchMode.GalleryReplay)
            replayCatalog = (string[])scenePaths.Clone();
        return true;
    }

    internal static void AdvanceReplayOrReturnToMenu()
    {
        if (Mode != LevelLaunchMode.GalleryReplay || IsLoading)
            return;

        if (TryLoadReplayLevel(ReplayLevel + 1, replayCatalog))
            return;

        BeginLoad(GetLoadableSceneIndex(MainMenuPath));
    }

    private static bool TryLoadReplayLevel(int level, string[] scenePaths)
    {
        if (scenePaths == null || level < 1 || level > scenePaths.Length)
            return false;

        string path = scenePaths[level - 1];
        bool started = GameInstallationSave.HasCurrentInstallSave();
        if (!SupportsReplay(level, path) || !LevelProgressSave.IsLevelUnlocked(
                level, started, LevelProgressSave.GetBestStars))
            return false;

        int index = GetLoadableSceneIndex(path);
        if (index < 0)
            return false;

        var previousMode = Mode;
        int previousLevel = ReplayLevel;
        Mode = LevelLaunchMode.GalleryReplay;
        ReplayLevel = level;
        if (path == FirstLevelPath)
            TutorialProgressManager.RestoreLegacyMilestones();

        if (BeginLoad(index))
            return true;

        Mode = previousMode;
        ReplayLevel = previousLevel;
        return false;
    }

    internal static void AdvanceFromStoryLevel(string currentScenePath)
    {
        if (TryGetLevelNumber(currentScenePath, out int currentLevel) &&
            SupportsReplay(currentLevel, currentScenePath) &&
            LevelProgressSave.IsLevelUnlocked(currentLevel + 1,
                GameInstallationSave.HasCurrentInstallSave(), LevelProgressSave.GetBestStars) &&
            BeginLoad(GetLoadableSceneIndex(LevelPath(currentLevel + 1))))
                return;

        BeginLoad(GetLoadableSceneIndex(MainMenuPath));
    }

    internal static bool TryGetLevelNumber(string scenePath, out int level)
    {
        level = 0;
        string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        return name.StartsWith("Level", StringComparison.Ordinal) &&
            int.TryParse(name.Substring("Level".Length), out level);
    }

    internal static int GetLoadableSceneIndex(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return -1;

        int index = SceneUtility.GetBuildIndexByScenePath(path);
        return index >= 0 && Application.CanStreamedLevelBeLoaded(index) ? index : -1;
    }

    private static bool BeginLoad(int index)
    {
        if (index < 0 || IsLoading)
            return false;

        IsLoading = true;
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;
        try
        {
            if (SceneManager.LoadSceneAsync(index, LoadSceneMode.Single) != null)
                return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        Time.timeScale = previousTimeScale;
        IsLoading = false;
        return false;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single)
            return;

        IsLoading = false;
        if (scene.path == MainMenuPath)
            Reset();
    }

    private static void Reset()
    {
        Mode = LevelLaunchMode.None;
        ReplayLevel = 0;
        IsLoading = false;
        replayCatalog = null;
    }
}
