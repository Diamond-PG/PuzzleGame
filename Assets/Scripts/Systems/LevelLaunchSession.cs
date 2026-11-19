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
    private const string ReviewedReplayScene = "Assets/Scenes/Level2.unity";

    public static LevelLaunchMode Mode { get; private set; }
    public static int ReplayLevel { get; private set; }
    public static int ReplayBestStars { get; private set; }
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
    }

    internal static bool SupportsReplay(string scenePath)
    {
        // Level1 currently writes to the same tutorial state as Continue.
        // Add other scenes only after checking their state, spawn and exit paths.
        return string.Equals(scenePath, ReviewedReplayScene, StringComparison.Ordinal);
    }

    internal static bool TryStartReplay(int level, string[] scenePaths)
    {
        if (IsLoading || Mode != LevelLaunchMode.None ||
            !TryLoadReplayLevel(level, scenePaths))
            return false;

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
        if (!SupportsReplay(path) || !LevelProgressSave.IsLevelUnlocked(
                level, GameInstallationSave.HasCurrentInstallSave(), LevelProgressSave.GetBestStars))
            return false;

        int index = GetLoadableSceneIndex(path);
        if (index < 0)
            return false;

        var previousMode = Mode;
        int previousLevel = ReplayLevel;
        int previousStars = ReplayBestStars;
        Mode = LevelLaunchMode.GalleryReplay;
        ReplayLevel = level;
        ReplayBestStars = LevelProgressSave.GetBestStars(level);

        if (BeginLoad(index))
            return true;

        Mode = previousMode;
        ReplayLevel = previousLevel;
        ReplayBestStars = previousStars;
        return false;
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
        ReplayBestStars = 0;
        IsLoading = false;
        replayCatalog = null;
    }
}
