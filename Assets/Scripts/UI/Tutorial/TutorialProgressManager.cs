using UnityEngine;

/// <summary>
/// Stores persistent tutorial milestones and the exact in-progress tutorial state.
/// This class only stores data. Scene objects restore themselves from it in later steps.
/// Existing PlayerPrefs keys are intentionally preserved.
/// </summary>
public static class TutorialProgressManager
{
    internal const string LegacyPuzzleFinaleKey = "TutorialPuzzleFinale_Completed";
    internal const string PuzzleFinaleKey = "TutorialPuzzleFinale_Completed_V2";
    private const string PuzzleFinalePageKey = Prefix + "PuzzleFinalePage";
    private const string PuzzleOwnedAtNewGameKey = Prefix + "PuzzleOwnedAtNewGame";
    private const string TutorialHeartPickedKey = Prefix + "TutorialHeartPicked";

    internal static void RestoreLegacyMilestones()
    {
        if (!GameInstallationSave.HasCurrentInstallSave())
            return;

        // Older Gallery runs could save objectives but discard tutorial pages.
        bool finished = LevelProgressSave.HasObjective(1, LevelProgressSave.Objective.Finish);
        bool pickedHeart = LevelProgressSave.HasObjective(1, LevelProgressSave.Objective.TutorialHeart);
        bool finaleComplete = IsPuzzleFinaleComplete(PuzzleFinaleKey);
        if (!DoorBroken && (HasSavedLeftTutorialStage || finaleComplete || pickedHeart || finished))
            MarkDoorBroken();

        // Keep any exact stage. For runs with no stage at all, the saved
        // tutorial heart proves the box was reached, but not that its dialogue ended.
        if (pickedHeart && !HasSavedLeftTutorialStage)
            SaveLeftTutorialStage(TutorialHeartLesson.StageBoxUnlocked);

        if (finaleComplete || (finished && LevelProgressSave.GetBestStars(1) == 3))
        {
            if (!ClimbCompleted)
                CompleteClimb();
            if (SavedLeftTutorialStage < TutorialHeartLesson.StageHeartCompleted)
                SaveLeftTutorialStage(TutorialHeartLesson.StageHeartCompleted);
            MarkPuzzleFinaleComplete(PuzzleFinaleKey);
        }
        else if (SavedPuzzleFinalePage < 0 && !PuzzleOwnedAtNewGame &&
                 PuzzlePiecePickup.IsPieceCollected(1, 1))
        {
            // Older builds kept the piece but could lose the Gallery dialogue.
            // The farewell closes an unknown state without replaying the pickup.
            SavePuzzleFinalePage(TutorialPuzzleFinale.LastPage);
        }
    }

    // ============================================================
    // 01 - EXISTING SAVE KEYS (DO NOT RENAME)
    // ============================================================

    private const string Prefix = "ROLP_Tutorial_V1_";
    internal static bool IsPuzzleFinaleComplete(string key) =>
        PlayerPrefs.GetInt(key, 0) == 1 ||
        (key == PuzzleFinaleKey && PlayerPrefs.GetInt(LegacyPuzzleFinaleKey, 0) == 1);

    internal static void MarkPuzzleFinaleComplete(string key)
    {
        if (PlayerPrefs.GetInt(key, 0) == 1 && !PlayerPrefs.HasKey(PuzzleFinalePageKey))
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.DeleteKey(PuzzleFinalePageKey);
        PlayerPrefs.Save();
    }

    internal static int SavedPuzzleFinalePage => PlayerPrefs.GetInt(PuzzleFinalePageKey, -1);
    internal static bool PuzzleOwnedAtNewGame => PlayerPrefs.GetInt(PuzzleOwnedAtNewGameKey, 0) == 1;
    internal static bool TutorialHeartPicked => PlayerPrefs.GetInt(TutorialHeartPickedKey, 0) == 1;
    internal static void MarkTutorialHeartPicked() => SaveFlag(TutorialHeartPickedKey);

    internal static void SavePuzzleFinalePage(int page)
    {
        PlayerPrefs.SetInt(PuzzleFinalePageKey, page);
        PlayerPrefs.Save();
    }

    internal static void ResetClimbMilestones()
    {
        PlayerPrefs.DeleteKey(ClimbCompletedKey);
        PlayerPrefs.DeleteKey(LadderReachedKey);
        PlayerPrefs.Save();
    }

    private const string NameKey = Prefix + "PlayerName";
    private const string MovementKey = Prefix + "Movement";
    private const string JumpKey = Prefix + "Jump";
    private const string KickKey = Prefix + "Kick";
    private const string DoorKey = Prefix + "DoorLesson";

    private const string DoorBrokenKey = Prefix + "DoorBroken";
    private const string LadderReachedKey = Prefix + "LadderReached";
    private const string ClimbCompletedKey = Prefix + "ClimbCompleted";

    // ============================================================
    // 02 - NEW EXACT-PROGRESS KEYS
    // ============================================================

    // Main/right tutorial frame: pages tutorial_00 ... tutorial_22.
    // -1 means there is no exact saved page yet.
    private const string MainTutorialPageKey = Prefix + "MainPage";

    // Left tutorial frame (starting at tutorial_23).
    // -1 = never saved; 1 = greeting; 2 = ladder question;
    // 3 = grab rungs; 4 = free movement to ladder.
    // Higher values can be used for future lessons.
    private const string LeftTutorialStageKey = Prefix + "LeftStage";

    // Remembers the individual LEFT and RIGHT actions even if the
    // player exits before completing the two-direction practice.
    private const string MovementLeftUsedKey = Prefix + "MovementLeftUsed";
    private const string MovementRightUsedKey = Prefix + "MovementRightUsed";

    // Number of hits on the prison door before it is destroyed.
    private const string DoorHitsKey = Prefix + "DoorHits";

    // ============================================================
    // 03 - EXISTING PLAYER NAME
    // ============================================================

    public static string PlayerName => PlayerPrefs.GetString(NameKey, "");

    public static bool HasPlayerName =>
        !string.IsNullOrWhiteSpace(PlayerName);

    // ============================================================
    // 04 - EXISTING TUTORIAL PROGRESS
    // ============================================================

    public static bool MovementCompleted =>
        PlayerPrefs.GetInt(MovementKey, 0) == 1;

    public static bool JumpCompleted =>
        PlayerPrefs.GetInt(JumpKey, 0) == 1;

    public static bool KickCompleted =>
        PlayerPrefs.GetInt(KickKey, 0) == 1;

    public static bool DoorLessonReached =>
        PlayerPrefs.GetInt(DoorKey, 0) == 1;

    public static bool DoorBroken =>
        PlayerPrefs.GetInt(DoorBrokenKey, 0) == 1;

    public static bool LadderReached =>
        PlayerPrefs.GetInt(LadderReachedKey, 0) == 1;

    public static bool ClimbCompleted =>
        PlayerPrefs.GetInt(ClimbCompletedKey, 0) == 1;

    public static bool IntroCompleted =>
        DoorBroken;

    // ============================================================
    // 05 - NEW EXACT-PROGRESS READERS
    // ============================================================

    public static bool HasSavedMainTutorialPage =>
        PlayerPrefs.HasKey(MainTutorialPageKey);

    public static int SavedMainTutorialPage =>
        PlayerPrefs.GetInt(MainTutorialPageKey, -1);

    public static bool HasSavedLeftTutorialStage =>
        PlayerPrefs.HasKey(LeftTutorialStageKey);

    public static int SavedLeftTutorialStage =>
        PlayerPrefs.GetInt(LeftTutorialStageKey, -1);

    public static bool MovementLeftUsed =>
        PlayerPrefs.GetInt(MovementLeftUsedKey, 0) == 1;

    public static bool MovementRightUsed =>
        PlayerPrefs.GetInt(MovementRightUsedKey, 0) == 1;

    public static int SavedDoorHits =>
        Mathf.Max(0, PlayerPrefs.GetInt(DoorHitsKey, 0));

    // ============================================================
    // 06 - EXISTING NAME SAVING
    // ============================================================

    public static void SavePlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        string trimmed = name.Trim();

        if (PlayerName == trimmed)
            return;

        PlayerPrefs.SetString(NameKey, trimmed);
        PlayerPrefs.Save();
    }

    // ============================================================
    // 07 - EXISTING LESSON PROGRESS
    // ============================================================

    public static void CompleteMovement()
    {
        // If the overall lesson is completed, both directions
        // are necessarily completed as well.
        PlayerPrefs.SetInt(MovementLeftUsedKey, 1);
        PlayerPrefs.SetInt(MovementRightUsedKey, 1);
        PlayerPrefs.SetInt(MovementKey, 1);
        PlayerPrefs.Save();
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

    public static void MarkDoorBroken()
    {
        PlayerPrefs.SetInt(DoorKey, 1);
        PlayerPrefs.SetInt(DoorBrokenKey, 1);

        // The door is permanently broken in this playthrough;
        // partial hit count is no longer relevant.
        PlayerPrefs.DeleteKey(DoorHitsKey);
        PlayerPrefs.Save();
    }

    public static void MarkLadderReached()
    {
        SaveFlag(LadderReachedKey);
    }

    public static void CompleteClimb()
    {
        SaveFlag(ClimbCompletedKey);
    }

    // ============================================================
    // 08 - SAVE EXACT MAIN / RIGHT TUTORIAL PAGE
    // ============================================================

    public static void SaveMainTutorialPage(int pageIndex)
    {
        if (pageIndex < 0)
            return;

        if (SavedMainTutorialPage == pageIndex)
            return;

        PlayerPrefs.SetInt(MainTutorialPageKey, pageIndex);
        PlayerPrefs.Save();
    }

    public static void ClearMainTutorialPage()
    {
        if (!HasSavedMainTutorialPage)
            return;

        PlayerPrefs.DeleteKey(MainTutorialPageKey);
        PlayerPrefs.Save();
    }

    // ============================================================
    // 09 - SAVE EXACT LEFT TUTORIAL STAGE
    // ============================================================

    public static void SaveLeftTutorialStage(int stage)
    {
        if (stage < 0)
            return;

        if (SavedLeftTutorialStage == stage)
            return;

        PlayerPrefs.SetInt(LeftTutorialStageKey, stage);
        PlayerPrefs.Save();
    }

    public static void ClearLeftTutorialStage()
    {
        if (!HasSavedLeftTutorialStage)
            return;

        PlayerPrefs.DeleteKey(LeftTutorialStageKey);
        PlayerPrefs.Save();
    }

    // ============================================================
    // 10 - SAVE PARTIAL LEFT / RIGHT BUTTON PRACTICE
    // ============================================================

    public static void MarkMovementLeftUsed()
    {
        SaveFlag(MovementLeftUsedKey);
    }

    public static void MarkMovementRightUsed()
    {
        SaveFlag(MovementRightUsedKey);
    }

    // ============================================================
    // 11 - SAVE PARTIAL DOOR DAMAGE
    // ============================================================

    public static void SaveDoorHits(int hits)
    {
        if (DoorBroken)
            return;

        int safeHits = Mathf.Max(0, hits);

        if (SavedDoorHits == safeHits)
            return;

        PlayerPrefs.SetInt(DoorHitsKey, safeHits);
        PlayerPrefs.Save();
    }

    // ============================================================
    // 12 - SAVE FLAG HELPER
    // ============================================================

    private static void SaveFlag(string key)
    {
        if (PlayerPrefs.GetInt(key, 0) == 1)
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    // ============================================================
    // 13 - RESET ALL TUTORIAL PROGRESS
    // ============================================================

    public static void ResetAllProgress()
    {
        ClearSavedProgress();
        PlayerPrefs.Save();

        Debug.Log("[Tutorial] Tutorial progress reset.");
    }

    internal static void ClearSavedProgress()
    {
        // Original keys.
        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.DeleteKey(PlayerNameInputController.DefaultNameSaveKey);
        PlayerPrefs.DeleteKey(MovementKey);
        PlayerPrefs.DeleteKey(JumpKey);
        PlayerPrefs.DeleteKey(KickKey);
        PlayerPrefs.DeleteKey(DoorKey);
        PlayerPrefs.DeleteKey(DoorBrokenKey);
        PlayerPrefs.DeleteKey(LadderReachedKey);
        PlayerPrefs.DeleteKey(ClimbCompletedKey);

        // New exact-progress keys.
        PlayerPrefs.DeleteKey(MainTutorialPageKey);
        PlayerPrefs.DeleteKey(LeftTutorialStageKey);
        PlayerPrefs.DeleteKey(MovementLeftUsedKey);
        PlayerPrefs.DeleteKey(MovementRightUsedKey);
        PlayerPrefs.DeleteKey(DoorHitsKey);
        PlayerPrefs.DeleteKey(TutorialHeartPickedKey);
        PlayerPrefs.DeleteKey(PuzzleFinalePageKey);
        PlayerPrefs.DeleteKey(PuzzleFinaleKey);
        PlayerPrefs.DeleteKey(LegacyPuzzleFinaleKey);
        PlayerPrefs.DeleteKey(PuzzleOwnedAtNewGameKey);
    }

    // ============================================================
    // 14 - UNITY EDITOR TEST COMMAND (UNCHANGED)
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
            "[Tutorial] Ready to test the first introduction."
        );
    }

#endif
}
