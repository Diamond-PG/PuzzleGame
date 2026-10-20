using UnityEngine;

public static class TutorialProgressManager
{
    // ============================================================
    // 01 - SAVE KEYS
    // ============================================================

    private const string Prefix = "ROLP_Tutorial_V1_";

    private const string NameKey =
        Prefix + "PlayerName";

    private const string MovementKey =
        Prefix + "Movement";

    private const string JumpKey =
        Prefix + "Jump";

    private const string KickKey =
        Prefix + "Kick";

    private const string DoorKey =
        Prefix + "DoorLesson";

    // ============================================================
    // 02 - NEW PROGRESS KEYS
    // ============================================================

    private const string DoorBrokenKey =
        Prefix + "DoorBroken";

    private const string LadderReachedKey =
        Prefix + "LadderReached";

    private const string ClimbCompletedKey =
        Prefix + "ClimbCompleted";

    // ============================================================
    // 03 - SAVED PLAYER NAME
    // ============================================================

    public static string PlayerName =>
        PlayerPrefs.GetString(NameKey, "");

    public static bool HasPlayerName =>
        !string.IsNullOrWhiteSpace(PlayerName);

    // ============================================================
    // 04 - ORIGINAL TUTORIAL PROGRESS
    // ============================================================

    public static bool MovementCompleted =>
        PlayerPrefs.GetInt(MovementKey, 0) == 1;

    public static bool JumpCompleted =>
        PlayerPrefs.GetInt(JumpKey, 0) == 1;

    public static bool KickCompleted =>
        PlayerPrefs.GetInt(KickKey, 0) == 1;

    public static bool DoorLessonReached =>
        PlayerPrefs.GetInt(DoorKey, 0) == 1;

    // ============================================================
    // 05 - NEW TUTORIAL PROGRESS
    // ============================================================

    public static bool DoorBroken =>
        PlayerPrefs.GetInt(DoorBrokenKey, 0) == 1;

    public static bool LadderReached =>
        PlayerPrefs.GetInt(LadderReachedKey, 0) == 1;

    public static bool ClimbCompleted =>
        PlayerPrefs.GetInt(ClimbCompletedKey, 0) == 1;

    // ============================================================
    // 06 - INTRO COMPLETION
    // ============================================================

    // Introduction is completed only when
    // the prison door has actually been destroyed.
    //
    // Reaching the door lesson is not enough.

    public static bool IntroCompleted =>
        MovementCompleted &&
        JumpCompleted &&
        KickCompleted &&
        DoorBroken;

    // ============================================================
    // 07 - SAVE PLAYER NAME
    // ============================================================

    public static void SavePlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        PlayerPrefs.SetString(
            NameKey,
            name.Trim()
        );

        PlayerPrefs.Save();
    }

    // ============================================================
    // 08 - ORIGINAL LESSON PROGRESS
    // ============================================================

    public static void CompleteMovement()
    {
        SaveFlag(MovementKey);
    }

    public static void CompleteJump()
    {
        SaveFlag(JumpKey);
    }

    public static void CompleteKick()
    {
        SaveFlag(KickKey);
    }

    public static void ReachDoorLesson()
    {
        SaveFlag(DoorKey);
    }

    // ============================================================
    // 09 - PRISON DOOR DESTROYED
    // ============================================================

    public static void MarkDoorBroken()
    {
        ReachDoorLesson();
        SaveFlag(DoorBrokenKey);
    }

    // ============================================================
    // 10 - LADDER REACHED
    // ============================================================

    public static void MarkLadderReached()
    {
        SaveFlag(LadderReachedKey);
    }

    // ============================================================
    // 11 - CLIMBING COMPLETED
    // ============================================================

    public static void CompleteClimb()
    {
        SaveFlag(ClimbCompletedKey);
    }

    // ============================================================
    // 12 - SAVE HELPER
    // ============================================================

    private static void SaveFlag(string key)
    {
        if (PlayerPrefs.GetInt(key, 0) == 1)
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    // ============================================================
    // 13 - RESET TUTORIAL PROGRESS
    // ============================================================

    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.DeleteKey(MovementKey);
        PlayerPrefs.DeleteKey(JumpKey);
        PlayerPrefs.DeleteKey(KickKey);
        PlayerPrefs.DeleteKey(DoorKey);

        PlayerPrefs.DeleteKey(DoorBrokenKey);
        PlayerPrefs.DeleteKey(LadderReachedKey);
        PlayerPrefs.DeleteKey(ClimbCompletedKey);

        PlayerPrefs.Save();

        Debug.Log(
            "[Tutorial] Tutorial progress reset. " +
            "Other game saves were not changed."
        );
    }

    // ============================================================
    // 14 - UNITY EDITOR TEST COMMAND
    // ============================================================

#if UNITY_EDITOR

    [UnityEditor.MenuItem(
        "Tools/Realm of the Lost Puzzle/Reset Tutorial Progress"
    )]
    private static void ResetTutorialForTesting()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "[Tutorial] Stop Play Mode before resetting."
            );

            return;
        }

        bool confirmed =
            UnityEditor.EditorUtility.DisplayDialog(
                "Reset Tutorial Progress",

                "Reset the saved player name " +
                "and all tutorial lessons?\n\n" +
                "Other PlayerPrefs will remain untouched.",

                "Reset Tutorial",
                "Cancel"
            );

        if (!confirmed)
            return;

        ResetAllProgress();

        Debug.Log(
            "[Tutorial] Ready to test " +
            "the first introduction."
        );
    }

#endif
}