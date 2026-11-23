
using System;
using System.IO;
using UnityEngine;

public static class GameInstallationSave
{
    private const string StartedKey =
        "ROLP_GameStarted_V1";

    private const string SaveInstallKey =
        "ROLP_SaveInstallId_V1";

    private const string InstallFile =
        "rolp_install_id_v1.txt";

    private const string CollectionLegacyOwner = "ROLP_CollectionLegacyOwner_V1";
    private const string CollectionRunKey = "ROLP_CollectionRun_V1";

    private static string cachedInstallId;
    private static bool installIdResolved;

    // Unique identifier for this installation.
    private static string GetInstallId()
    {
        if (installIdResolved)
            return cachedInstallId;

        installIdResolved = true;

        try
        {
            string path = GetInstallFilePath();

            if (File.Exists(path))
            {
                string existing =
                    File.ReadAllText(path).Trim();

                if (!string.IsNullOrEmpty(existing))
                {
                    cachedInstallId = existing;
                    return cachedInstallId;
                }
            }

            string newId =
                Guid.NewGuid().ToString("N");

            string directory =
                Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, newId);

            cachedInstallId = newId;

            Debug.Log(
                "[GameInstallationSave] " +
                "New installation identifier created."
            );

            return cachedInstallId;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[GameInstallationSave] " +
                "Cannot access installation identifier: " +
                exception.Message
            );

            return null;
        }
    }

    private static string GetInstallFilePath()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer =
            new AndroidJavaClass(
                "com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>(
                    "currentActivity"))
            {
                using (AndroidJavaObject directory =
                    activity.Call<AndroidJavaObject>(
                        "getNoBackupFilesDir"))
                {
                    string path =
                        directory.Call<string>(
                            "getAbsolutePath");

                    return Path.Combine(
                        path, InstallFile);
                }
            }
        }
#else
        // Editor and other platforms:
        // persistent across ordinary game sessions.
        return Path.Combine(
            Application.persistentDataPath,
            InstallFile
        );
#endif
    }

    // Called only when New Game starts.
    internal static void MarkNewGameStarted()
    {
        string installId = GetInstallId();

        if (string.IsNullOrEmpty(installId))
            return;

        ResolveCollectionInstallId();
        PlayerPrefs.SetString(CollectionRunKey, Guid.NewGuid().ToString("N"));

        PlayerPrefs.SetString(
            SaveInstallKey, installId);

        PlayerPrefs.SetInt(StartedKey, 1);

        Debug.Log(
            "[GameInstallationSave] " +
            "New game registered."
        );
    }

    private static string ResolveCollectionInstallId()
    {
        string id = GetInstallId();
        if (!string.IsNullOrEmpty(id) && !PlayerPrefs.HasKey(CollectionLegacyOwner))
        {
            // Decide before New Game can claim restored preferences for a new installation.
            PlayerPrefs.SetString(CollectionLegacyOwner, HasCurrentInstallSave() ? id : "");
            PlayerPrefs.Save();
        }

        return id;
    }

    internal static int GetCollectionInt(string key)
    {
        string id = ResolveCollectionInstallId();
        if (string.IsNullOrEmpty(id))
            return 0;

        string run = PlayerPrefs.GetString(CollectionRunKey, "");
        string ownedKey = GetOwnedCollectionKey(key, id, run);
        if (PlayerPrefs.HasKey(ownedKey))
            return PlayerPrefs.GetInt(ownedKey, 0);

        // Only existing saves can fall back to keys written before run IDs existed.
        return string.IsNullOrEmpty(run) && PlayerPrefs.GetString(CollectionLegacyOwner, "") == id
            ? PlayerPrefs.GetInt(key, 0)
            : 0;
    }

    internal static bool SetCollectionInt(string key, int value)
    {
        string id = ResolveCollectionInstallId();
        if (string.IsNullOrEmpty(id))
            return false;

        string run = PlayerPrefs.GetString(CollectionRunKey, "");
        PlayerPrefs.SetInt(GetOwnedCollectionKey(key, id, run), value);
        PlayerPrefs.Save();
        return true;
    }

    private static string GetOwnedCollectionKey(string key, string installId, string run)
    {
        string installKey = key + "_Install_" + installId;
        return string.IsNullOrEmpty(run) ? installKey : installKey + "_Run_" + run;
    }

    // Checks whether the save belongs to
    // this specific installation.
    public static bool HasCurrentInstallSave()
    {
        string installId = GetInstallId();

        if (string.IsNullOrEmpty(installId))
            return false;

        if (PlayerPrefs.GetInt(StartedKey, 0) != 1)
            return false;

        return PlayerPrefs.GetString(
            SaveInstallKey, "") == installId;
    }

    // Also requires real tutorial progress.
    public static bool CanContinue(
        int minimumSavedTutorialPage = 1)
    {
        if (!HasCurrentInstallSave())
            return false;

        return
            TutorialProgressManager.SavedMainTutorialPage
                >= minimumSavedTutorialPage ||

            TutorialProgressManager.HasPlayerName ||

            TutorialProgressManager.HasSavedLeftTutorialStage ||

            TutorialProgressManager.MovementLeftUsed ||
            TutorialProgressManager.MovementRightUsed ||

            TutorialProgressManager.MovementCompleted ||
            TutorialProgressManager.JumpCompleted ||
            TutorialProgressManager.KickCompleted ||

            TutorialProgressManager.DoorLessonReached ||
            TutorialProgressManager.DoorBroken ||

            TutorialProgressManager.LadderReached ||
            TutorialProgressManager.ClimbCompleted ||

            TutorialProgressManager.SavedDoorHits > 0;
    }
}
