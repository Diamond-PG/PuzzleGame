
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Protects the large WeaponButton and small UnequipButton
/// during selected Keeper explanation pages.
///
/// Tutorial 38 - Club is ready: both buttons blocked.
/// Tutorial 39 - Explain unequip: both buttons blocked.
/// Tutorial 40 - Practice unequip: guard releases both buttons.
/// Tutorial 41 - Re-equip using inventory: no guard lock.
/// Tutorial 42 - Try the club: both buttons blocked.
/// Tutorial 43 - Look at first box: both buttons blocked.
/// Tutorial 44 - Break box with club: both buttons blocked.
///
/// Protection is applied only while the gold Next
/// button and the Keeper frame are visible.
///
/// Does not alter the inventory, equipped weapon,
/// weapon durability, tutorial stages or saved progress.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(200)]
public sealed class TutorialClubButtonGuard : MonoBehaviour
{
    // ============================================================
    // 01 - BUTTONS TO PROTECT
    // ============================================================

    [Header("01 - BUTTONS TO PROTECT")]

    [Tooltip("Large WeaponButton in ActionControls.")]
    [SerializeField]
    private Button weaponButton;

    [Tooltip("Small UnequipButton.")]
    [SerializeField]
    private Button unequipButton;


    // ============================================================
    // 02 - EXISTING TUTORIAL FRAME
    // ============================================================

    [Header("02 - EXISTING TUTORIAL FRAME")]

    [Tooltip("CanvasGroup on TutorialFrame_Left.")]
    [SerializeField]
    private CanvasGroup keeperFrame;

    [Tooltip("Existing TutorialNextButton_Left.")]
    [SerializeField]
    private Button goldNextButton;


    // ============================================================
    // 03 - TUTORIAL PROTECTION
    // ============================================================

    [Header("03 - TUTORIAL PROTECTION")]

    [Tooltip("Master switch for button protection.")]
    [SerializeField]
    private bool enableProtection = true;

    [Tooltip(
        "Saved stage 25 begins tutorial 42. " +
        "Existing Inspector value remains unchanged."
    )]
    [SerializeField, Min(0)]
    private int firstProtectedStage = 25;

    [Tooltip(
        "Saved stage 28 is tutorial 44. " +
        "Stage 29 releases tutorial protection."
    )]
    [SerializeField, Min(0)]
    private int lastProtectedStage = 28;

    [Tooltip("Require a visible Keeper frame.")]
    [SerializeField]
    private bool requireVisibleKeeper = true;

    [Tooltip(
        "Only block on explanation pages that show " +
        "the gold Next button."
    )]
    [SerializeField]
    private bool requireGoldNextButton = true;


    // ============================================================
    // 03B - EARLIER CLUB EXPLANATIONS
    // ============================================================

    [Header("03B - ADDITIONAL PROTECTION (38-39)")]

    [Tooltip(
        "Block WeaponButton and UnequipButton on " +
        "tutorial 38 and tutorial 39. " +
        "Tutorial 40 remains interactive."
    )]
    [SerializeField]
    private bool protectTutorial38And39 = true;


    // ============================================================
    // 04 - DEBUG
    // ============================================================

    [Header("04 - DEBUG")]

    [SerializeField]
    private bool debugLogs = false;


    // ============================================================
    // SAVED TUTORIAL STAGES
    // ============================================================

    private const int StageTutorial38 = 21;
    private const int StageTutorial39 = 22;
    private const int StageTutorial40 = 23;


    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private CanvasGroup weaponGroup;
    private CanvasGroup unequipGroup;

    private bool weaponStateCaptured;
    private bool unequipStateCaptured;

    private bool weaponOriginalRaycasts = true;
    private bool unequipOriginalRaycasts = true;

    private bool weaponGuardOwnsBlock;
    private bool unequipGuardOwnsBlock;

    private bool protectionActive;


    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
        UpdateProtection();
    }

    private void LateUpdate()
    {
        UpdateProtection();
    }

    private void OnDisable()
    {
        ReleaseProtection();
    }

    private void OnDestroy()
    {
        ReleaseProtection();
    }

    private void OnValidate()
    {
        firstProtectedStage =
            Mathf.Max(0, firstProtectedStage);

        lastProtectedStage =
            Mathf.Max(
                firstProtectedStage,
                lastProtectedStage
            );
    }


    // ============================================================
    // FIND EXISTING REFERENCES
    // ============================================================

    private void ResolveReferences()
    {
        if (keeperFrame == null)
        {
            keeperFrame =
                GetComponent<CanvasGroup>();
        }

        if (goldNextButton == null)
        {
            Transform next =
                transform.Find("TutorialNextButton_Left");

            if (next != null)
            {
                goldNextButton =
                    next.GetComponent<Button>();
            }
        }

        if (weaponButton == null)
        {
            WeaponButtonDurabilityUI durability =
                FindFirstObjectByType<
                    WeaponButtonDurabilityUI
                >();

            if (durability != null)
            {
                weaponButton =
                    durability.GetComponent<Button>();

                if (weaponButton == null)
                {
                    weaponButton =
                        durability.GetComponentInParent<Button>();
                }
            }
        }

        if (unequipButton == null)
        {
            UnequipWeaponButton unequip =
                FindFirstObjectByType<
                    UnequipWeaponButton
                >();

            if (unequip != null)
            {
                unequipButton =
                    unequip.GetComponent<Button>();
            }
        }

        if (weaponButton != null &&
            weaponGroup == null)
        {
            weaponGroup =
                weaponButton.GetComponent<CanvasGroup>();

            if (weaponGroup == null)
            {
                weaponGroup =
                    weaponButton.gameObject
                        .AddComponent<CanvasGroup>();
            }
        }

        if (unequipButton != null &&
            unequipGroup == null)
        {
            unequipGroup =
                unequipButton.GetComponent<CanvasGroup>();

            if (unequipGroup == null)
            {
                unequipGroup =
                    unequipButton.gameObject
                        .AddComponent<CanvasGroup>();
            }
        }
    }


    // ============================================================
    // DETERMINE WHETHER TO BLOCK
    // ============================================================

    private bool ShouldProtectButtons()
    {
        if (!enableProtection)
            return false;

        int stage =
            TutorialProgressManager.SavedLeftTutorialStage;

        // Tutorial 38 and 39 are explanation pages.
        // The player must NOT use either weapon button.
        bool earlierExplanation =
            protectTutorial38And39 &&
            (stage == StageTutorial38 ||
             stage == StageTutorial39);

        // Tutorial 42-44 also require button protection.
        bool finalClubExplanation =
            stage >= firstProtectedStage &&
            stage <= lastProtectedStage;

        if (!earlierExplanation &&
            !finalClubExplanation)
        {
            return false;
        }

        // Tutorial 40 is an interactive practice lesson.
        // Never block its UnequipButton here.
        if (stage == StageTutorial40)
            return false;

        if (requireVisibleKeeper)
        {
            if (keeperFrame == null)
                return false;

            if (!keeperFrame.gameObject.activeInHierarchy)
                return false;

            if (keeperFrame.alpha <= 0.01f)
                return false;
        }

        // This prevents the guard from interfering with
        // pages where the player is explicitly asked to
        // equip or unequip using real UI buttons.
        if (requireGoldNextButton)
        {
            if (goldNextButton == null)
                return false;

            if (!goldNextButton.gameObject.activeInHierarchy)
                return false;

            if (!goldNextButton.interactable)
                return false;
        }

        return true;
    }


    // ============================================================
    // UPDATE BUTTON PROTECTION
    // ============================================================

    private void UpdateProtection()
    {
        bool shouldProtect = ShouldProtectButtons();

        if (!shouldProtect)
        {
            if (protectionActive ||
                weaponStateCaptured ||
                unequipStateCaptured)
            {
                ReleaseProtection();
            }

            return;
        }

        if (weaponGroup == null ||
            unequipGroup == null)
        {
            ResolveReferences();
        }

        ProtectWeaponButton();
        ProtectUnequipButton();

        if (!protectionActive)
        {
            protectionActive = true;

            if (debugLogs)
            {
                Debug.Log(
                    "[CLUB BUTTON GUARD] " +
                    "Weapon and Unequip buttons protected. " +
                    "Saved tutorial stage: " +
                    TutorialProgressManager.SavedLeftTutorialStage,
                    this
                );
            }
        }
    }


    // ============================================================
    // LARGE WEAPON BUTTON
    // ============================================================

    private void ProtectWeaponButton()
    {
        if (weaponGroup == null)
            return;

        if (!weaponStateCaptured)
        {
            weaponOriginalRaycasts =
                weaponGroup.blocksRaycasts;

            weaponStateCaptured = true;

            // Another tutorial component might already
            // own a blocked state.
            weaponGuardOwnsBlock =
                weaponOriginalRaycasts;
        }

        weaponGroup.blocksRaycasts = false;
    }


    // ============================================================
    // SMALL UNEQUIP BUTTON
    // ============================================================

    private void ProtectUnequipButton()
    {
        if (unequipGroup == null)
            return;

        if (!unequipStateCaptured)
        {
            unequipOriginalRaycasts =
                unequipGroup.blocksRaycasts;

            unequipStateCaptured = true;

            unequipGuardOwnsBlock =
                unequipOriginalRaycasts;
        }

        unequipGroup.blocksRaycasts = false;
    }


    // ============================================================
    // RELEASE PROTECTION
    // ============================================================

    private void ReleaseProtection()
    {
        // Only undo blocks that this component owns.
        // Other tutorial systems retain control of
        // their own CanvasGroup settings.

        if (weaponGroup != null &&
            weaponStateCaptured &&
            weaponGuardOwnsBlock)
        {
            weaponGroup.blocksRaycasts = true;
        }

        if (unequipGroup != null &&
            unequipStateCaptured &&
            unequipGuardOwnsBlock)
        {
            unequipGroup.blocksRaycasts = true;
        }

        bool wasActive = protectionActive;

        weaponStateCaptured = false;
        unequipStateCaptured = false;

        weaponGuardOwnsBlock = false;
        unequipGuardOwnsBlock = false;

        protectionActive = false;

        if (debugLogs && wasActive)
        {
            Debug.Log(
                "[CLUB BUTTON GUARD] " +
                "Protection released.",
                this
            );
        }
    }
}
