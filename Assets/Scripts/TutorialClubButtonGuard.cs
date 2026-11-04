
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Protects WeaponButton and UnequipButton during
/// selected Puzzle Keeper tutorial pages.
///
/// Tutorial 38: both buttons protected.
/// Tutorial 39: both buttons protected.
/// Tutorial 40: unequip practice is allowed.
/// Tutorial 41: inventory practice is allowed.
/// Tutorial 42-44: both buttons protected.
///
/// After the full Keeper training ends (saved stage 35),
/// restores the small UnequipButton raycast once.
///
/// Does not change inventory data, weapon durability,
/// equipment state, tutorial progress or saved data.
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

    [Tooltip("Small UnequipButton in ActionControls.")]
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
        "Keep the existing Inspector value."
    )]
    [SerializeField, Min(0)]
    private int firstProtectedStage = 25;

    [Tooltip(
        "Saved stage 28 is tutorial 44. " +
        "Stage 29 releases this tutorial protection."
    )]
    [SerializeField, Min(0)]
    private int lastProtectedStage = 28;

    [Tooltip("Require a visible Keeper frame.")]
    [SerializeField]
    private bool requireVisibleKeeper = true;

    [Tooltip(
        "Require the gold Next button during " +
        "protected explanation pages."
    )]
    [SerializeField]
    private bool requireGoldNextButton = true;


    // ============================================================
    // 03B - EARLIER CLUB EXPLANATIONS
    // ============================================================

    [Header("03B - ADDITIONAL PROTECTION (38-39)")]

    [Tooltip(
        "Protect WeaponButton and UnequipButton " +
        "during tutorials 38 and 39. " +
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

    // Full Keeper training ends after tutorial 48.
    private const int StageFullTrainingCompleted = 35;


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

    // Prevents continuous changes after the training.
    private bool postTutorialUnequipRestored;


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
            Mathf.Max(
                0,
                firstProtectedStage
            );

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
                transform.Find(
                    "TutorialNextButton_Left"
                );

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
                        durability
                            .GetComponentInParent<Button>();
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
                weaponButton
                    .GetComponent<CanvasGroup>();

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
                unequipButton
                    .GetComponent<CanvasGroup>();

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
        {
            return false;
        }

        int stage =
            TutorialProgressManager
                .SavedLeftTutorialStage;

        // Tutorials 38 and 39:
        // explanation only, no weapon button input.
        bool earlierExplanation =
            protectTutorial38And39 &&
            (
                stage == StageTutorial38 ||
                stage == StageTutorial39
            );

        // Tutorials 42-44:
        // explanation and protected box sequence.
        bool finalClubExplanation =
            stage >= firstProtectedStage &&
            stage <= lastProtectedStage;

        if (!earlierExplanation &&
            !finalClubExplanation)
        {
            return false;
        }

        // Tutorial 40 must allow the real
        // UnequipButton interaction.
        if (stage == StageTutorial40)
        {
            return false;
        }

        if (requireVisibleKeeper)
        {
            if (keeperFrame == null)
            {
                return false;
            }

            if (!keeperFrame.gameObject
                .activeInHierarchy)
            {
                return false;
            }

            if (keeperFrame.alpha <= 0.01f)
            {
                return false;
            }
        }

        if (requireGoldNextButton)
        {
            if (goldNextButton == null)
            {
                return false;
            }

            if (!goldNextButton.gameObject
                .activeInHierarchy)
            {
                return false;
            }

            if (!goldNextButton.interactable)
            {
                return false;
            }
        }

        return true;
    }


    // ============================================================
    // UPDATE BUTTON PROTECTION
    // ============================================================

    private void UpdateProtection()
    {
        bool shouldProtect =
            ShouldProtectButtons();

        if (!shouldProtect)
        {
            if (protectionActive ||
                weaponStateCaptured ||
                unequipStateCaptured)
            {
                ReleaseProtection();
            }

            RestoreUnequipAfterFullTraining();

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
                    TutorialProgressManager
                        .SavedLeftTutorialStage,
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
        {
            return;
        }

        if (!weaponStateCaptured)
        {
            weaponOriginalRaycasts =
                weaponGroup.blocksRaycasts;

            weaponStateCaptured = true;

            // If another component had already
            // blocked this button, do not claim
            // ownership of that block.
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
        {
            return;
        }

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
    // RELEASE TUTORIAL PROTECTION
    // ============================================================

    private void ReleaseProtection()
    {
        // Keep the previous ownership-based
        // release behavior unchanged.

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

        bool wasActive =
            protectionActive;

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


    // ============================================================
    // RESTORE AFTER COMPLETED KEEPER TRAINING
    // ============================================================

    private void RestoreUnequipAfterFullTraining()
    {
        if (postTutorialUnequipRestored)
        {
            return;
        }

        // Never perform this recovery while
        // the player is still in the tutorial.
        int savedStage =
            TutorialProgressManager
                .SavedLeftTutorialStage;

        if (savedStage <
            StageFullTrainingCompleted)
        {
            return;
        }

        if (unequipButton == null ||
            unequipGroup == null)
        {
            ResolveReferences();
        }

        if (unequipButton == null ||
            unequipGroup == null)
        {
            return;
        }

        // Wait until the previous Keeper frame
        // has finished fading out.
        if (keeperFrame != null &&
            keeperFrame.gameObject.activeInHierarchy &&
            keeperFrame.alpha > 0.01f)
        {
            return;
        }

        // Recover the button once after
        // tutorial 48 is fully completed.
        //
        // This is not a per-frame override,
        // so other gameplay UI systems remain
        // free to manage the button afterward.
        if (!unequipGroup.blocksRaycasts)
        {
            unequipGroup.blocksRaycasts = true;

            if (debugLogs)
            {
                Debug.Log(
                    "[CLUB BUTTON GUARD] " +
                    "UnequipButton raycasts restored " +
                    "after completed Keeper training. " +
                    "Stage: " + savedStage,
                    this
                );
            }
        }

        postTutorialUnequipRestored = true;
    }
}
