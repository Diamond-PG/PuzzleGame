using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Tutorial after the climbing lesson:
/// 31 - hand icon question (gold Next; hand cannot be pressed).
/// 32 - pick up the club (no gold Next; original hand interaction works).
/// After the club has FINISHED flying into the inventory:
/// 33 - congratulations; Keeper celebrates (gold Next).
/// 34 - combat with a weapon; Keeper presents with an open palm (gold Next).
/// 35 - weapon durability; spotlight on the BIG weapon circle (gold Next).
/// 36 - watch the red bar; three visual-only flashes (gold Next after flashes).
/// 37 - tap the club in the inventory (real inventory click, NO gold Next).
/// On all other explanation pages, inventory clicks are blocked so the club
/// cannot be equipped before the Keeper instructs the player.
/// 38 - the club is now equipped in the big WeaponButton (gold Next).
/// 39 - explain the UnequipButton (gold Next; button protected).
/// 40 - tap the real UnequipButton; club returns to inventory.
/// 41 - tap the club in inventory again to re-equip it.
/// 42 - invite the player to try the club (gold Next; then unpause).
///
/// All stage values are saved using TutorialProgressManager.SaveLeftTutorialStage.
/// This component lives on the EXISTING TutorialFrame_Left.
/// It never changes the player's actual weapon durability or equips the club.
/// </summary>
[DisallowMultipleComponent]
public sealed class TutorialClubLesson : MonoBehaviour
{
    // Do not change existing persisted stages 0-15.
    private const int StageFinishedClimbLesson = 10;
    private const int StageClubQuestion = 13;
    private const int StageClubHandInstruction = 14;
    private const int StageClubCollected = 15;
    private const int StageWeaponCongratulations = 16;
    private const int StageWeaponCombat = 17;
    private const int StageWeaponDurability = 18;
    private const int StageWeaponRedBar = 19;
    // Former StageClubLessonFinished was 20. Keep that value as the first
    // NEW stage so existing saves at 20 can continue the added lesson.
    private const int StageClubEquipInstruction = 20;
    private const int StageClubEquipped = 21;
    // Keep persisted values 0-21 exactly as before.
    // Old 22 was an unfinished planned page 39; it now begins the
    // four-page unequip/re-equip lesson.
    private const int StageUnequipExplanation = 22;
    private const int StageUnequipPractice = 23;
    private const int StageClubReequip = 24;
    private const int StageClubTryWeapon = 25;
    private const int StageClubLessonFinished = 26;

    [Header("01 - CLUB AND PLAYER")]
    [SerializeField] private ClubPickup clubPickup;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TutorialClimbLesson climbLesson;

    [Header("02 - EXISTING KEEPER UI")]
    [SerializeField] private TMP_Text leftTutorialText;
    [SerializeField] private CanvasGroup leftFrameCanvasGroup;
    [SerializeField] private Button goldNextButton;
    [SerializeField] private TutorialArrowPulse goldNextPulse;
    [SerializeField] private GameObject tutorialDimmer;

    [Header("03 - EXISTING HAND BUTTON")]
    [Tooltip("ActionButton_Hand, NOT ActionButton_Leg.")]
    [SerializeField] private Button handButton;
    [SerializeField] private RectTransform handButtonGraphic;

    [Header("04 - HAND SPOTLIGHT")]
    [SerializeField, Range(64, 512)] private int textureResolution = 256;
    [SerializeField, Range(0f, 0.08f)] private float windowPadding = 0.012f;
    [SerializeField, Range(0.35f, 1.25f)] private float handSpotlightScale = 0.8f;
    [SerializeField, Range(0.001f, 0.08f)] private float windowSoftness = 0.012f;

    [Header("05 - LOCALIZATION (TUTORIAL 31 AND 32)")]
    [SerializeField] private string localizationTable = "Tutorial";
    [FormerlySerializedAs("handInstructionKey")]
    [SerializeField] private string handQuestionKey = "tutorial_31";
    [SerializeField, TextArea(2, 4)]
    private string handQuestionFallback = "Видишь значок руки?";
    [SerializeField] private string handUseKey = "tutorial_32";
    [SerializeField, TextArea(2, 4)]
    private string handUseFallback = "Нажми на него, чтобы взять дубинку.";

    // The existing 31/32 Inspector fields and their tuned values MUST stay unchanged.
    [Header("06 - KEEPER POSE FOR 31-32 (EXISTING)")]
    [SerializeField] private Image keeperImage;
    [SerializeField] private Sprite handInstructionKeeperSprite;
    [SerializeField] private RectTransform keeperPuzzleIcon;
    [SerializeField] private Vector2 keeperPosition = new Vector2(134.8f, -221f);
    [SerializeField] private Vector2 keeperSize = new Vector2(230f, 230f);
    [SerializeField] private float keeperRotationZ;
    [SerializeField] private Vector2 keeperScale = Vector2.one;
    [SerializeField] private Vector2 puzzlePosition = new Vector2(6.9f, -63.8f);
    [SerializeField] private Vector2 puzzleSize = new Vector2(27f, 27f);
    [SerializeField] private float puzzleRotationZ = 20f;
    [SerializeField] private Vector2 puzzleScale = Vector2.one;

    [Header("07 - NEW LOCALIZATION (TUTORIAL 33-36)")]
    [SerializeField] private string congratulationsKey = "tutorial_33";
    [SerializeField, TextArea(2, 4)]
    private string congratulationsFallback = "Отлично! Теперь ты можешь сражаться.";
    [SerializeField] private string combatKey = "tutorial_34";
    [SerializeField, TextArea(2, 4)]
    private string combatFallback = "Не только ногами, но и оружием!";
    [SerializeField] private string durabilityKey = "tutorial_35";
    [SerializeField, TextArea(2, 4)]
    private string durabilityFallback = "Но помни: у каждого оружия есть запас прочности.";
    [SerializeField] private string redBarKey = "tutorial_36";
    [SerializeField, TextArea(2, 4)]
    private string redBarFallback = "Следи за красной полоской!";

    [System.Serializable]
    private class IndependentKeeperPose
    {
        [Tooltip("Select an existing Guardian sprite. None keeps the current pose.")]
        public Sprite keeperSprite;
        [Header("KEEPER RECT TRANSFORM")]
        public Vector2 keeperPosition = new Vector2(134.8f, -221f);
        public Vector2 keeperSize = new Vector2(230f, 230f);
        public float keeperRotationZ;
        public Vector2 keeperScale = Vector2.one;
        [Header("BLUE PUZZLE MEDALLION - INDEPENDENT")]
        public Vector2 puzzlePosition = new Vector2(6.9f, -63.8f);
        public Vector2 puzzleSize = new Vector2(27f, 27f);
        public float puzzleRotationZ = 20f;
        public Vector2 puzzleScale = Vector2.one;
    }

    [Header("07B - LOCALIZATION (TUTORIAL 37-38 AND 42)")]
    [SerializeField] private string equipInstructionKey = "tutorial_37";
    [SerializeField, TextArea(2, 4)]
    private string equipInstructionFallback = "Теперь нажми на дубинку в инвентаре.";
    [SerializeField] private string equippedSuccessKey = "tutorial_38";
    [SerializeField, TextArea(2, 4)]
    private string equippedSuccessFallback = "Отлично! Теперь дубинка готова к бою!";
    // Keep existing saved Inspector value when upgrading from old tutorial_39.
    [FormerlySerializedAs("tryWeaponKey")]
    [SerializeField] private string finalTryWeaponKey = "tutorial_42";
    [FormerlySerializedAs("tryWeaponFallback")]
    [SerializeField, TextArea(2, 4)]
    private string finalTryWeaponFallback = "А теперь попробуй применить её в деле!";

    [Header("07C - LOCALIZATION (TUTORIAL 39-41)")]
    [SerializeField] private string unequipExplanationKey = "tutorial_39";
    [SerializeField, TextArea(2, 4)]
    private string unequipExplanationFallback = "Эта кнопка возвращает оружие в инвентарь.";
    [SerializeField] private string unequipPracticeKey = "tutorial_40";
    [SerializeField, TextArea(2, 4)]
    private string unequipPracticeFallback = "Попробуй нажать на неё!";
    [SerializeField] private string reEquipKey = "tutorial_41";
    [SerializeField, TextArea(2, 4)]
    private string reEquipFallback = "А теперь снова выбери дубинку.";

    [Header("08 - HAPPY KEEPER (TUTORIAL 33)")]
    [Tooltip("Guardian celebrating with both fists raised; own medallion settings.")]
    [SerializeField] private IndependentKeeperPose happyKeeperPose =
        new IndependentKeeperPose();

    [Header("09 - OPEN PALM KEEPER (TUTORIAL 34-36)")]
    [Tooltip("Guardian presenting with an open hand; own medallion settings.")]
    [SerializeField] private IndependentKeeperPose openPalmKeeperPose =
        new IndependentKeeperPose();

    [Header("10 - BIG WEAPON CIRCLE SPOTLIGHT (35-36)")]
    [Tooltip("WeaponButton RectTransform, NOT the inventory slot.")]
    [SerializeField] private RectTransform weaponButtonGraphic;
    [SerializeField, Range(0.35f, 1.5f)] private float weaponSpotlightScale = 0.95f;
    [Tooltip("WeaponButtonDurabilityUI on WeaponButton, for finding its circle.")]
    [SerializeField] private WeaponButtonDurabilityUI weaponDurabilityUI;

    [Header("10B - INVENTORY CLUB SLOT SPOTLIGHT (TUTORIAL 37)")]
    [Tooltip("InventoryPanel object with InventoryUI. Leave empty for automatic lookup.")]
    [SerializeField] private InventoryUI inventoryUI;
    [Tooltip("OPTIONAL: the little inventory slot containing the club. Leave empty to auto-find the occupied weapon slot.")]
    [SerializeField] private RectTransform clubInventorySlotGraphic;
    [SerializeField, Range(0.35f, 1.5f)] private float clubSlotSpotlightScale = 0.95f;

    [Header("10C - UNEQUIP BUTTON SPOTLIGHT (TUTORIAL 39-40)")]
    [Tooltip("The existing Canvas/ActionControls/UnequipButton with Button component.")]
    [SerializeField] private Button unequipButton;
    [Tooltip("RectTransform of the same existing UnequipButton. Auto-resolved if omitted.")]
    [SerializeField] private RectTransform unequipButtonGraphic;
    [SerializeField, Range(0.35f, 1.5f)] private float unequipSpotlightScale = 0.95f;

    [Header("11 - RED DURABILITY BAR DEMO (TUTORIAL 36)")]
    [Tooltip("The REAL WeaponDurabilityFill image below WeaponButton.")]
    [SerializeField] private Image weaponDurabilityFill;
    [Tooltip("Show the bar ONLY as a temporary visual preview when the club is still in an inventory slot.")]
    [SerializeField] private bool previewBarWhenUnequipped = true;
    [SerializeField, Range(1, 10)] private int tutorialFlashCount = 3;
    [SerializeField, Min(0.02f)] private float flashVisibleSeconds = 0.22f;
    [SerializeField, Min(0.02f)] private float flashHiddenSeconds = 0.18f;

    private enum Phase
    {
        Waiting, Question, Instruction, PickupAnimation,
        Congratulations, Combat, Durability, RedBar,
        EquipInstruction, EquippedSuccess,
        UnequipExplanation, UnequipPractice, ReequipInstruction,
        TryWeapon, Finished
    }

    private Phase phase = Phase.Waiting;
    private Image dimmerImage;
    private Sprite originalDimmerSprite;
    private Image.Type originalDimmerType;
    private bool originalPreserveAspect;
    private bool originalRaycastTarget;
    private Texture2D maskTexture;
    private Sprite maskSprite;
    private Color32[] maskPixels;

    private float previousTimeScale = 1f;
    private bool pausedByThis;
    private bool movementLockedByThis;
    private bool waitingForRepeatPickup;
    private int resumePostPickupStage = StageWeaponCongratulations;

    private Coroutine localizationCoroutine;
    private int localizationVersion;
    private string localizedHandQuestion;
    private string localizedHandUse;
    private string localizedCongratulations;
    private string localizedCombat;
    private string localizedDurability;
    private string localizedRedBar;
    private string localizedEquipInstruction;
    private string localizedEquippedSuccess;
    private string localizedTryWeapon;
    private string localizedUnequipExplanation;
    private string localizedUnequipPractice;
    private string localizedReequip;
    private RectTransform resolvedClubInventorySlot;
    private int postEquipResumeStage = StageClubEquipped;
    private bool unequipRaycastStateCaptured;
    private bool originalUnequipBlocksRaycasts = true;
    private CanvasGroup unequipRaycastGroup;

    private CanvasGroup handRaycastGroup;
    private bool originalHandBlocksRaycasts = true;
    private bool handRaycastStateCaptured;

    // Inventory items remain visible throughout the explanation, but the
    // player may equip a weapon ONLY on tutorial_37 and tutorial_41.
    // Lock the InventoryPanel's UI raycasts rather than changing Button colors
    // or replacing InventoryWeaponSlot's existing listeners.
    private CanvasGroup inventoryRaycastGroup;
    private bool inventoryRaycastStateCaptured;
    private bool originalInventoryBlocksRaycasts = true;

    // Disable hits on the LARGE attack button only while Keeper pages are
    // paused. Time.timeScale = 0 does not block Unity UI pointer events.
    private CanvasGroup weaponAttackRaycastGroup;
    private bool weaponAttackRaycastStateCaptured;
    private bool originalWeaponAttackBlocksRaycasts = true;

    private Coroutine flashCoroutine;
    private bool flashStateCaptured;
    private Color originalBarColor;
    private bool originalBarEnabled;
    private float originalBarFill;
    private Image.Type originalBarType;

    private bool keeperBaselineCaptured;
    private Sprite baselineKeeperSprite;
    private Vector2 baselineKeeperPosition;
    private Vector2 baselineKeeperSize;
    private Quaternion baselineKeeperRotation;
    private Vector3 baselineKeeperScale;
    private Vector2 baselinePuzzlePosition;
    private Vector2 baselinePuzzleSize;
    private Quaternion baselinePuzzleRotation;
    private Vector3 baselinePuzzleScale;

    private void Awake()
    {
        ResolveReferences();
        localizedHandQuestion = handQuestionFallback;
        localizedHandUse = handUseFallback;
        localizedCongratulations = congratulationsFallback;
        localizedCombat = combatFallback;
        localizedDurability = durabilityFallback;
        localizedRedBar = redBarFallback;
        localizedEquipInstruction = equipInstructionFallback;
        localizedEquippedSuccess = equippedSuccessFallback;
        // Unity's serialized value may come from the old final page (39).
        // The new final lesson is tutorial_42, with the same original text.
        if (finalTryWeaponKey == "tutorial_39")
            finalTryWeaponKey = "tutorial_42";
        localizedTryWeapon = finalTryWeaponFallback;
        localizedUnequipExplanation = unequipExplanationFallback;
        localizedUnequipPractice = unequipPracticeFallback;
        localizedReequip = reEquipFallback;

        if (dimmerImage != null)
        {
            originalDimmerSprite = dimmerImage.sprite;
            originalDimmerType = dimmerImage.type;
            originalPreserveAspect = dimmerImage.preserveAspect;
            originalRaycastTarget = dimmerImage.raycastTarget;
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        BeginLocalizationLoad();
    }

    private void Start()
    {
        ResolveReferences();
        // Original HandInteractionButton listener is registered during Awake.
        BindButtonListeners();

        int saved = TutorialProgressManager.SavedLeftTutorialStage;
        phase = saved >= StageClubLessonFinished ? Phase.Finished : Phase.Waiting;
        resumePostPickupStage = Mathf.Clamp(saved, StageWeaponCongratulations, StageClubTryWeapon);

        // After a scene reload the actual club may need to be picked up
        // again. Keep inventory locked until we reach the saved instruction.
        if (saved >= StageClubCollected && saved < StageClubLessonFinished)
            SetInventoryRaycastsEnabled(false);

        // Stop the old climbing lesson from hiding the shared Keeper frame.
        // Leave the actual climbing and movement mechanics unchanged.
        if (saved >= StageClubQuestion && saved < StageClubLessonFinished &&
            climbLesson != null && climbLesson.enabled)
            climbLesson.enabled = false;
    }

    private void BindButtonListeners()
    {
        if (goldNextButton != null)
        {
            goldNextButton.onClick.RemoveListener(OnGoldNextClicked);
            goldNextButton.onClick.AddListener(OnGoldNextClicked);
        }
        if (handButton != null)
        {
            handButton.onClick.RemoveListener(OnHandPressed);
            handButton.onClick.AddListener(OnHandPressed);
        }
    }

    private void Update()
    {
        if (phase == Phase.Finished) return;

        // Existing InventoryUI owns all real equipping/unequipping.
        // Observe only the final state, never call its methods here.
        if (inventoryUI != null)
        {
            if (phase == Phase.EquipInstruction && inventoryUI.IsClubEquipped)
            {
                switch (postEquipResumeStage)
                {
                    case StageUnequipExplanation: ShowUnequipExplanationPage(); break;
                    case StageUnequipPractice: ShowUnequipPracticePage(); break;
                    case StageClubTryWeapon: ShowTryWeaponPage(); break;
                    default: ShowEquippedSuccessPage(); break;
                }
                return;
            }
            if (phase == Phase.UnequipPractice && !inventoryUI.IsClubEquipped)
            {
                ShowReequipPage();
                return;
            }
            if (phase == Phase.ReequipInstruction && inventoryUI.IsClubEquipped)
            {
                ShowTryWeaponPage();
                return;
            }
        }

        if (phase == Phase.Waiting)
        {
            int saved = TutorialProgressManager.SavedLeftTutorialStage;
            if (!TutorialProgressManager.DoorBroken ||
                saved < StageFinishedClimbLesson ||
                saved >= StageClubLessonFinished)
                return;

            // On a new run, physical items reset, but the saved Keeper page
            // must remain. Let the player pick up the respawned club, then
            // resume precisely the saved post-pickup page (33, 34, 35 or 36).
            if (saved >= StageClubCollected)
            {
                resumePostPickupStage = saved == StageClubCollected
                    ? StageWeaponCongratulations
                    : Mathf.Clamp(saved, StageWeaponCongratulations, StageClubTryWeapon);

                if (clubPickup == null)
                {
                    ShowSavedPostPickupPage();
                    return;
                }

                if (clubPickup.CanHandInteract && handButton != null &&
                    handButton.interactable)
                    waitingForRepeatPickup = true;

                if (waitingForRepeatPickup && !clubPickup.CanHandInteract)
                    phase = Phase.PickupAnimation;

                return;
            }

            if (clubPickup == null || handButton == null ||
                !handButton.interactable || !clubPickup.CanHandInteract)
                return;

            StartHandLesson(saved == StageClubHandInstruction);
        }
        else if (phase == Phase.Question || phase == Phase.Instruction)
        {
            // The original pickup can run during Time.timeScale = 0.
            // Release our own pause once its collider becomes unavailable.
            if (clubPickup != null && !clubPickup.CanHandInteract)
                BeginPickupAnimation();
        }
        else if (phase == Phase.PickupAnimation)
        {
            // Unity destroys the world club only at the END of its flight.
            // Do not show tutorial_33 until the inventory animation finishes.
            if (clubPickup == null)
            {
                TutorialProgressManager.SaveLeftTutorialStage(StageClubCollected);
                ShowSavedPostPickupPage();
            }
        }
    }

    // ============================================================
    // 31-32: EXPLAIN THE HAND BUTTON
    // ============================================================

    private void StartHandLesson(bool resumePage32)
    {
        ResolveReferences();
        if (leftFrameCanvasGroup == null || leftTutorialText == null ||
            tutorialDimmer == null || dimmerImage == null ||
            handButton == null || playerController == null ||
            goldNextButton == null)
        {
            Debug.LogWarning("[CLUB TUTORIAL] Assign Keeper text, dimmer, gold Next, hand button and player.", this);
            return;
        }

        if (climbLesson != null && climbLesson.enabled)
            climbLesson.enabled = false;

        CaptureKeeperBaseline();
        ApplyHandInstructionKeeperPose();
        ShowKeeperFrame();
        ShowHandSpotlight();
        // Prevent inventory weapon selection even during the pickup flight.
        SetInventoryRaycastsEnabled(false);
        PauseAndLockPlayer();

        if (resumePage32) ShowUseHandPage();
        else ShowQuestionPage();
    }

    private void ShowQuestionPage()
    {
        phase = Phase.Question;
        TutorialProgressManager.SaveLeftTutorialStage(StageClubQuestion);
        SetHandRaycastsEnabled(false);
        SetKeeperMessage(localizedHandQuestion);
        SetGoldNextVisible(true);
    }

    private void ShowUseHandPage()
    {
        phase = Phase.Instruction;
        TutorialProgressManager.SaveLeftTutorialStage(StageClubHandInstruction);
        SetKeeperMessage(localizedHandUse);
        SetGoldNextVisible(false);
        SetHandRaycastsEnabled(true);
    }

    private void BeginPickupAnimation()
    {
        if (phase == Phase.PickupAnimation || phase == Phase.Finished) return;
        phase = Phase.PickupAnimation;
        SetHandRaycastsEnabled(true);
        SetGoldNextVisible(false);
        HideKeeperFrame();
        RestoreDimmer();
        ReleasePlayerAndTime();
    }

    // ============================================================
    // 33-36: AFTER THE CLUB ARRIVES IN INVENTORY
    // ============================================================

    private void ShowSavedPostPickupPage()
    {
        ResolveReferences();
        if (leftFrameCanvasGroup == null || leftTutorialText == null ||
            tutorialDimmer == null || dimmerImage == null ||
            goldNextButton == null || playerController == null)
        {
            Debug.LogWarning("[CLUB TUTORIAL] Missing Keeper UI references for tutorial_33-36.", this);
            phase = Phase.Waiting;
            return;
        }

        if (climbLesson != null && climbLesson.enabled)
            climbLesson.enabled = false;

        CaptureKeeperBaseline();
        SetHandRaycastsEnabled(false);
        // Pages 33-36 are explanations, not an invitation to equip.
        SetInventoryRaycastsEnabled(false);
        PauseAndLockPlayer();
        ShowKeeperFrame();

        switch (resumePostPickupStage)
        {
            case StageWeaponCombat: ShowCombatPage(); break;
            case StageWeaponDurability: ShowDurabilityPage(); break;
            case StageWeaponRedBar: ShowRedBarPage(); break;
            case StageClubEquipInstruction:
                ShowEquipInstructionPage();
                break;
            case StageClubEquipped:
            case StageUnequipExplanation:
            case StageUnequipPractice:
                // Level reloads reset actual inventory/equipment. If club is
                // not equipped, temporarily repeat page 37 without rolling
                // BACK the saved tutorial page. Resume the exact dialogue
                // after the player taps the real inventory slot.
                if (inventoryUI != null && inventoryUI.IsClubEquipped)
                {
                    if (resumePostPickupStage == StageClubEquipped)
                        ShowEquippedSuccessPage();
                    else if (resumePostPickupStage == StageUnequipExplanation)
                        ShowUnequipExplanationPage();
                    else
                        ShowUnequipPracticePage();
                }
                else
                {
                    postEquipResumeStage = resumePostPickupStage;
                    ShowEquipInstructionPage(true);
                }
                break;
            case StageClubReequip:
                // At this point the club should be in the inventory, so we
                // can immediately repeat the tap-to-equip exercise.
                if (inventoryUI != null && inventoryUI.IsClubEquipped)
                    ShowTryWeaponPage();
                else
                    ShowReequipPage();
                break;
            case StageClubTryWeapon:
                if (inventoryUI != null && inventoryUI.IsClubEquipped)
                    ShowTryWeaponPage();
                else
                    ShowReequipPage(true); // Preserve saved page 42.
                break;
            default:
                ShowCongratulationsPage();
                break;
        }
    }

    private void ShowCongratulationsPage()
    {
        StopBarFlashAndRestore();
        phase = Phase.Congratulations;
        TutorialProgressManager.SaveLeftTutorialStage(StageWeaponCongratulations);
        ApplyIndependentKeeperPose(happyKeeperPose);
        SetKeeperMessage(localizedCongratulations);
        ShowPlainDimmer();
        SetGoldNextVisible(true);
    }

    private void ShowCombatPage()
    {
        StopBarFlashAndRestore();
        phase = Phase.Combat;
        TutorialProgressManager.SaveLeftTutorialStage(StageWeaponCombat);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedCombat);
        ShowPlainDimmer();
        SetGoldNextVisible(true);
    }

    private void ShowDurabilityPage()
    {
        StopBarFlashAndRestore();
        phase = Phase.Durability;
        TutorialProgressManager.SaveLeftTutorialStage(StageWeaponDurability);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedDurability);
        ShowWeaponSpotlight();
        SetGoldNextVisible(true);
    }

    private void ShowRedBarPage()
    {
        StopBarFlashAndRestore();
        phase = Phase.RedBar;
        TutorialProgressManager.SaveLeftTutorialStage(StageWeaponRedBar);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedRedBar);
        ShowWeaponSpotlight();
        SetGoldNextVisible(false);
        StartBarFlash();
    }

    // ============================================================
    // 37-42: EQUIP / UNEQUIP / RE-EQUIP (REAL INPUT)
    // ============================================================

    private void ShowEquipInstructionPage(bool resumingLaterPage = false)
    {
        StopBarFlashAndRestore();
        ResolveReferences();
        // Tutorial 37: now (and only now) the club may be selected.
        SetInventoryRaycastsEnabled(true);
        phase = Phase.EquipInstruction;
        if (!resumingLaterPage)
        {
            postEquipResumeStage = StageClubEquipped;
            TutorialProgressManager.SaveLeftTutorialStage(StageClubEquipInstruction);
        }
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedEquipInstruction);
        if (inventoryUI == null)
            Debug.LogWarning("[CLUB TUTORIAL] InventoryUI reference is missing. Assign InventoryPanel in Inspector.", this);
        resolvedClubInventorySlot = FindOccupiedClubSlot();
        if (resolvedClubInventorySlot != null)
            ShowSpotlightFor(resolvedClubInventorySlot, clubSlotSpotlightScale);
        else
        {
            ShowPlainDimmer();
            Debug.LogWarning("[CLUB TUTORIAL] Could not find an occupied slot with weapon ID Club for tutorial_37.", this);
        }
        SetGoldNextVisible(false); // The only way forward is tapping the club.
    }

    private void ShowEquippedSuccessPage()
    {
        StopBarFlashAndRestore();
        SetInventoryRaycastsEnabled(false);
        phase = Phase.EquippedSuccess;
        TutorialProgressManager.SaveLeftTutorialStage(StageClubEquipped);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedEquippedSuccess);
        ShowWeaponSpotlight(); // Club is now in the BIG WeaponButton.
        SetGoldNextVisible(true);
    }

    // 39: explain existing UnequipButton, show its spotlight but block
    // early taps so the player reads the explanation and presses gold Next.
    private void ShowUnequipExplanationPage()
    {
        StopBarFlashAndRestore();
        SetInventoryRaycastsEnabled(false);
        phase = Phase.UnequipExplanation;
        TutorialProgressManager.SaveLeftTutorialStage(StageUnequipExplanation);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedUnequipExplanation);
        SetUnequipRaycastsEnabled(false);
        ShowUnequipSpotlight();
        SetGoldNextVisible(true);
    }

    // 40: golden arrow disappears, original button can be pressed.
    // Existing UnequipWeaponButton.UseUnequip executes normally; we wait
    // for the resulting InventoryUI.IsClubEquipped == false in Update().
    private void ShowUnequipPracticePage()
    {
        // Tutorial 40 uses a SEPARATE UnequipButton, not inventory slots.
        SetInventoryRaycastsEnabled(false);
        phase = Phase.UnequipPractice;
        TutorialProgressManager.SaveLeftTutorialStage(StageUnequipPractice);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedUnequipPractice);
        SetUnequipRaycastsEnabled(true);
        ShowUnequipSpotlight();
        SetGoldNextVisible(false);
    }

    // 41: the player taps the ACTUAL occupied inventory slot again.
    private void ShowReequipPage(bool resumingFinalPage = false)
    {
        // Tutorial 41: the club can be equipped again.
        SetInventoryRaycastsEnabled(true);
        phase = Phase.ReequipInstruction;
        if (!resumingFinalPage)
            TutorialProgressManager.SaveLeftTutorialStage(StageClubReequip);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedReequip);
        SetUnequipRaycastsEnabled(true);
        resolvedClubInventorySlot = FindOccupiedClubSlot();
        if (resolvedClubInventorySlot != null)
            ShowSpotlightFor(resolvedClubInventorySlot, clubSlotSpotlightScale);
        else
        {
            ShowPlainDimmer();
            Debug.LogWarning("[CLUB TUTORIAL] Could not locate the club slot for tutorial_41.", this);
        }
        SetGoldNextVisible(false);
    }

    // 42: unfreeze only after pressing the final gold Next arrow.
    private void ShowTryWeaponPage()
    {
        StopBarFlashAndRestore();
        SetInventoryRaycastsEnabled(false);
        phase = Phase.TryWeapon;
        TutorialProgressManager.SaveLeftTutorialStage(StageClubTryWeapon);
        ApplyIndependentKeeperPose(openPalmKeeperPose);
        SetKeeperMessage(localizedTryWeapon);
        ShowPlainDimmer();
        SetGoldNextVisible(true);
    }

    private void ShowUnequipSpotlight()
    {
        ResolveReferences();
        if (unequipButtonGraphic == null)
            Debug.LogWarning("[CLUB TUTORIAL] Assign UnequipButton to section 10C for tutorial_39/40.", this);
        ShowSpotlightFor(unequipButtonGraphic, unequipSpotlightScale);
    }

    private void SetUnequipRaycastsEnabled(bool enabled)
    {
        ResolveReferences();
        if (unequipButton == null) return;
        if (unequipRaycastGroup == null)
        {
            unequipRaycastGroup = unequipButton.GetComponent<CanvasGroup>();
            if (unequipRaycastGroup == null)
                unequipRaycastGroup = unequipButton.gameObject.AddComponent<CanvasGroup>();
        }
        if (!unequipRaycastStateCaptured)
        {
            originalUnequipBlocksRaycasts = unequipRaycastGroup.blocksRaycasts;
            unequipRaycastStateCaptured = true;
        }
        // Don't alter the Button.interactable state. That is owned by the
        // existing UnequipWeaponButton component and the weapon system.
        unequipRaycastGroup.blocksRaycasts = enabled && originalUnequipBlocksRaycasts;
    }

    private void RestoreUnequipRaycasts()
    {
        if (unequipRaycastGroup != null && unequipRaycastStateCaptured)
            unequipRaycastGroup.blocksRaycasts = originalUnequipBlocksRaycasts;
        unequipRaycastStateCaptured = false;
    }

    private RectTransform FindOccupiedClubSlot()
    {
        ResolveReferences();

        // InventoryUI knows which *actual* slot holds weapon ID "Club".
        // Handles both dynamic Slot_N and slots whose item child is
        // Item_Sword / Item_Club instead of exactly "Item".
        // Never highlight Sword or Key by mistake.
        if (inventoryUI != null)
        {
            RectTransform clubSlot = inventoryUI.FindOccupiedWeaponSlot("Club");
            if (clubSlot != null)
                return clubSlot;
        }

        // Optional manual fallback, only if the player deliberately set it.
        // Normally keep this Inspector field None to follow moving weapons.
        if (clubInventorySlotGraphic != null &&
            clubInventorySlotGraphic.gameObject.activeInHierarchy)
            return clubInventorySlotGraphic;

        return null;
    }

    private void FinishClubLesson()
    {
        if (phase == Phase.Finished) return;
        phase = Phase.Finished;
        TutorialProgressManager.SaveLeftTutorialStage(StageClubLessonFinished);
        StopBarFlashAndRestore();
        RestoreHandRaycasts();
        RestoreUnequipRaycasts();
        RestoreInventoryRaycasts();
        SetGoldNextVisible(false);
        HideKeeperFrame();
        RestoreDimmer();
        ReleasePlayerAndTime();
        RestoreKeeperBaseline();
    }

    // ============================================================
    // SHARED BUTTONS AND FRAME
    // ============================================================

    private void SetKeeperMessage(string message)
    {
        if (leftTutorialText == null) return;
        leftTutorialText.text = message ?? "";
        leftTutorialText.gameObject.SetActive(true);
        leftTutorialText.ForceMeshUpdate();
        ShowKeeperFrame();
    }

    private void ShowKeeperFrame()
    {
        if (leftFrameCanvasGroup != null)
            leftFrameCanvasGroup.alpha = 1f;
        if (leftTutorialText != null)
            leftTutorialText.gameObject.SetActive(true);
    }

    private void HideKeeperFrame()
    {
        if (leftFrameCanvasGroup != null)
        {
            leftFrameCanvasGroup.alpha = 0f;
            leftFrameCanvasGroup.interactable = false;
            leftFrameCanvasGroup.blocksRaycasts = false;
        }
        if (leftTutorialText != null) leftTutorialText.text = "";
    }

    private void SetGoldNextVisible(bool visible)
    {
        if (leftFrameCanvasGroup != null)
        {
            leftFrameCanvasGroup.interactable = visible;
            leftFrameCanvasGroup.blocksRaycasts = visible;
        }
        if (goldNextButton != null)
        {
            goldNextButton.gameObject.SetActive(visible);
            goldNextButton.interactable = visible;
        }
        if (goldNextPulse != null) goldNextPulse.SetPulseActive(visible);
    }

    private void OnGoldNextClicked()
    {
        switch (phase)
        {
            case Phase.Question: ShowUseHandPage(); break;
            case Phase.Congratulations: ShowCombatPage(); break;
            case Phase.Combat: ShowDurabilityPage(); break;
            case Phase.Durability: ShowRedBarPage(); break;
            case Phase.RedBar:
                if (flashCoroutine == null) ShowEquipInstructionPage();
                break;
            case Phase.EquippedSuccess: ShowUnequipExplanationPage(); break;
            case Phase.UnequipExplanation: ShowUnequipPracticePage(); break;
            case Phase.TryWeapon: FinishClubLesson(); break;
        }
    }

    private void OnHandPressed()
    {
        // The ORIGINAL HandInteractionButton.UseHand listener runs first.
        // We never call ClubPickup.HandInteract twice.
        if (phase == Phase.Instruction)
            BeginPickupAnimation();
        else if (phase == Phase.Waiting && waitingForRepeatPickup &&
                 clubPickup != null && !clubPickup.CanHandInteract)
            BeginPickupAnimation();
    }

    private void SetHandRaycastsEnabled(bool enabled)
    {
        if (handButton == null) return;
        if (handRaycastGroup == null)
        {
            handRaycastGroup = handButton.GetComponent<CanvasGroup>();
            if (handRaycastGroup == null)
                handRaycastGroup = handButton.gameObject.AddComponent<CanvasGroup>();
        }
        if (!handRaycastStateCaptured)
        {
            originalHandBlocksRaycasts = handRaycastGroup.blocksRaycasts;
            handRaycastStateCaptured = true;
        }
        handRaycastGroup.blocksRaycasts = enabled && originalHandBlocksRaycasts;
    }

    private void RestoreHandRaycasts()
    {
        if (handRaycastGroup != null && handRaycastStateCaptured)
            handRaycastGroup.blocksRaycasts = originalHandBlocksRaycasts;
    }

    // ============================================================
    // INVENTORY TOUCH LOCK (ALLOW ONLY ON PAGES 37 AND 41)
    // ============================================================

    private void SetInventoryRaycastsEnabled(bool enabled)
    {
        ResolveReferences();
        if (inventoryUI == null)
        {
            Debug.LogWarning(
                "[CLUB TUTORIAL] InventoryUI missing: cannot protect inventory clicks.",
                this);
            return;
        }

        if (inventoryRaycastGroup == null)
        {
            inventoryRaycastGroup = inventoryUI.GetComponent<CanvasGroup>();
            if (inventoryRaycastGroup == null)
                inventoryRaycastGroup = inventoryUI.gameObject.AddComponent<CanvasGroup>();
        }

        if (!inventoryRaycastStateCaptured)
        {
            originalInventoryBlocksRaycasts = inventoryRaycastGroup.blocksRaycasts;
            inventoryRaycastStateCaptured = true;
        }

        // Do not dim inventory icons or change the real weapon-slot Buttons.
        // UI raycasts are enabled only when the lesson explicitly requests it.
        inventoryRaycastGroup.blocksRaycasts =
            enabled && originalInventoryBlocksRaycasts;
    }

    private void RestoreInventoryRaycasts()
    {
        if (inventoryRaycastGroup != null && inventoryRaycastStateCaptured)
            inventoryRaycastGroup.blocksRaycasts = originalInventoryBlocksRaycasts;
        inventoryRaycastStateCaptured = false;
    }

    // ============================================================
    // ATTACK BUTTON TOUCH LOCK (INDEPENDENT OF INVENTORY / UNEQUIP)
    // ============================================================

    private void SetWeaponAttackRaycastsEnabled(bool enabled)
    {
        ResolveReferences();
        if (weaponButtonGraphic == null) return;
        if (weaponAttackRaycastGroup == null)
        {
            weaponAttackRaycastGroup = weaponButtonGraphic.GetComponent<CanvasGroup>();
            if (weaponAttackRaycastGroup == null)
                weaponAttackRaycastGroup = weaponButtonGraphic.gameObject.AddComponent<CanvasGroup>();
        }
        if (!weaponAttackRaycastStateCaptured)
        {
            originalWeaponAttackBlocksRaycasts = weaponAttackRaycastGroup.blocksRaycasts;
            weaponAttackRaycastStateCaptured = true;
        }
        weaponAttackRaycastGroup.blocksRaycasts = enabled && originalWeaponAttackBlocksRaycasts;
    }

    private void RestoreWeaponAttackRaycasts()
    {
        if (weaponAttackRaycastGroup != null && weaponAttackRaycastStateCaptured)
            weaponAttackRaycastGroup.blocksRaycasts = originalWeaponAttackBlocksRaycasts;
        weaponAttackRaycastStateCaptured = false;
    }

    // ============================================================
    // PAUSE AND MOVEMENT LOCK
    // ============================================================

    private void PauseAndLockPlayer()
    {
        // Keep inventory slots and UnequipButton usable in their practice
        // pages. Block ONLY the big WeaponButton (attack/swing), which must
        // never start an attack while the tutorial has frozen gameplay.
        SetWeaponAttackRaycastsEnabled(false);
        if (!pausedByThis)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            pausedByThis = true;
        }
        if (!movementLockedByThis && playerController != null)
        {
            playerController.SetHorizontalMovementLock(true);
            movementLockedByThis = true;
        }
    }

    private void ReleasePlayerAndTime()
    {
        RestoreWeaponAttackRaycasts();
        if (movementLockedByThis && playerController != null)
            playerController.SetHorizontalMovementLock(false);
        movementLockedByThis = false;
        if (pausedByThis) Time.timeScale = previousTimeScale;
        pausedByThis = false;
    }

    // ============================================================
    // REFERENCE LOOKUP - DOES NOT CHANGE INSPECTOR-TUNED SETTINGS
    // ============================================================

    private void ResolveReferences()
    {
        if (climbLesson == null) climbLesson = GetComponent<TutorialClimbLesson>();
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
        if (leftFrameCanvasGroup == null)
            leftFrameCanvasGroup = GetComponent<CanvasGroup>();
        if (leftTutorialText == null)
        {
            Transform t = transform.Find("TutorialText_Left");
            if (t != null) leftTutorialText = t.GetComponent<TMP_Text>();
        }
        if (goldNextButton == null)
        {
            Transform t = transform.Find("TutorialNextButton_Left");
            if (t != null) goldNextButton = t.GetComponent<Button>();
        }
        if (goldNextPulse == null && goldNextButton != null)
            goldNextPulse = goldNextButton.GetComponent<TutorialArrowPulse>();
        if (tutorialDimmer == null && transform.parent != null)
        {
            Transform t = transform.parent.Find("TutorialDimmer");
            if (t != null) tutorialDimmer = t.gameObject;
        }
        if (dimmerImage == null && tutorialDimmer != null)
            dimmerImage = tutorialDimmer.GetComponent<Image>();
        if (handButton != null && handButtonGraphic == null)
            handButtonGraphic = handButton.GetComponent<RectTransform>();
        if (keeperImage == null)
        {
            Transform t = transform.Find("PuzzleKeeper_Left");
            if (t != null) keeperImage = t.GetComponent<Image>();
        }
        if (keeperPuzzleIcon == null && keeperImage != null)
        {
            Transform t = keeperImage.transform.Find("KeeperPuzzleIcon");
            if (t != null) keeperPuzzleIcon = t.GetComponent<RectTransform>();
        }
        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (unequipButton == null)
        {
            UnequipWeaponButton component = FindFirstObjectByType<UnequipWeaponButton>();
            if (component != null)
                unequipButton = component.GetComponent<Button>();
        }
        if (unequipButtonGraphic == null && unequipButton != null)
            unequipButtonGraphic = unequipButton.GetComponent<RectTransform>();
        if (weaponDurabilityUI == null)
            weaponDurabilityUI = FindFirstObjectByType<WeaponButtonDurabilityUI>();
        if (weaponButtonGraphic == null && weaponDurabilityUI != null)
            weaponButtonGraphic = weaponDurabilityUI.GetComponent<RectTransform>();
        if (weaponDurabilityFill == null && weaponButtonGraphic != null)
        {
            Image[] images = weaponButtonGraphic.GetComponentsInChildren<Image>(true);
            foreach (Image candidate in images)
            {
                if (candidate != null && candidate.name == "WeaponDurabilityFill")
                {
                    weaponDurabilityFill = candidate;
                    break;
                }
            }
        }
    }

    // ============================================================
    // INDEPENDENT KEEPER POSES AND MEDALLIONS
    // ============================================================

    private void CaptureKeeperBaseline()
    {
        if (keeperBaselineCaptured || keeperImage == null) return;
        RectTransform r = keeperImage.rectTransform;
        baselineKeeperSprite = keeperImage.sprite;
        baselineKeeperPosition = r.anchoredPosition;
        baselineKeeperSize = r.sizeDelta;
        baselineKeeperRotation = r.localRotation;
        baselineKeeperScale = r.localScale;
        if (keeperPuzzleIcon != null)
        {
            baselinePuzzlePosition = keeperPuzzleIcon.anchoredPosition;
            baselinePuzzleSize = keeperPuzzleIcon.sizeDelta;
            baselinePuzzleRotation = keeperPuzzleIcon.localRotation;
            baselinePuzzleScale = keeperPuzzleIcon.localScale;
        }
        keeperBaselineCaptured = true;
    }

    private void ApplyHandInstructionKeeperPose()
    {
        if (keeperImage == null || handInstructionKeeperSprite == null) return;
        CaptureKeeperBaseline();
        keeperImage.sprite = handInstructionKeeperSprite;
        RectTransform r = keeperImage.rectTransform;
        r.anchoredPosition = keeperPosition;
        r.sizeDelta = keeperSize;
        r.localRotation = Quaternion.Euler(0f, 0f, keeperRotationZ);
        r.localScale = new Vector3(keeperScale.x, keeperScale.y, r.localScale.z);
        if (keeperPuzzleIcon == null) return;
        keeperPuzzleIcon.anchoredPosition = puzzlePosition;
        keeperPuzzleIcon.sizeDelta = puzzleSize;
        keeperPuzzleIcon.localRotation = Quaternion.Euler(0f, 0f, puzzleRotationZ);
        keeperPuzzleIcon.localScale = new Vector3(puzzleScale.x, puzzleScale.y,
            keeperPuzzleIcon.localScale.z);
    }

    private void ApplyIndependentKeeperPose(IndependentKeeperPose pose)
    {
        // Empty sprite is a safe fallback while new art is not yet assigned.
        if (pose == null || pose.keeperSprite == null || keeperImage == null) return;
        CaptureKeeperBaseline();
        keeperImage.sprite = pose.keeperSprite;
        RectTransform r = keeperImage.rectTransform;
        r.anchoredPosition = pose.keeperPosition;
        r.sizeDelta = pose.keeperSize;
        r.localRotation = Quaternion.Euler(0f, 0f, pose.keeperRotationZ);
        r.localScale = new Vector3(pose.keeperScale.x, pose.keeperScale.y, r.localScale.z);
        if (keeperPuzzleIcon == null) return;
        keeperPuzzleIcon.anchoredPosition = pose.puzzlePosition;
        keeperPuzzleIcon.sizeDelta = pose.puzzleSize;
        keeperPuzzleIcon.localRotation = Quaternion.Euler(0f, 0f, pose.puzzleRotationZ);
        keeperPuzzleIcon.localScale = new Vector3(
            pose.puzzleScale.x, pose.puzzleScale.y, keeperPuzzleIcon.localScale.z);
    }

    private void RestoreKeeperBaseline()
    {
        if (!keeperBaselineCaptured || keeperImage == null) return;
        keeperImage.sprite = baselineKeeperSprite;
        RectTransform r = keeperImage.rectTransform;
        r.anchoredPosition = baselineKeeperPosition;
        r.sizeDelta = baselineKeeperSize;
        r.localRotation = baselineKeeperRotation;
        r.localScale = baselineKeeperScale;
        if (keeperPuzzleIcon != null)
        {
            keeperPuzzleIcon.anchoredPosition = baselinePuzzlePosition;
            keeperPuzzleIcon.sizeDelta = baselinePuzzleSize;
            keeperPuzzleIcon.localRotation = baselinePuzzleRotation;
            keeperPuzzleIcon.localScale = baselinePuzzleScale;
        }
    }

    // ============================================================
    // SPOTLIGHTS - REUSE THE EXISTING TUTORIAL DIMMER
    // ============================================================

    private void ShowHandSpotlight()
    {
        ShowSpotlightFor(handButtonGraphic, handSpotlightScale);
    }

    private void ShowWeaponSpotlight()
    {
        ResolveReferences();
        ShowSpotlightFor(weaponButtonGraphic, weaponSpotlightScale);
    }

    private void ShowPlainDimmer()
    {
        if (tutorialDimmer == null || dimmerImage == null) return;
        RestoreDimmerImageOnly();
        tutorialDimmer.SetActive(true);
        dimmerImage.raycastTarget = false;
    }

    private void ShowSpotlightFor(RectTransform target, float scale)
    {
        if (tutorialDimmer == null || dimmerImage == null) return;
        if (target == null || Screen.width <= 0 || Screen.height <= 0)
        {
            ShowPlainDimmer();
            return;
        }

        Canvas c = target.GetComponentInParent<Canvas>();
        Camera uiCamera = c != null && c.renderMode != RenderMode.ScreenSpaceOverlay
            ? (c.worldCamera != null ? c.worldCamera : Camera.main) : null;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        float xMin = float.PositiveInfinity, xMax = float.NegativeInfinity;
        float yMin = float.PositiveInfinity, yMax = float.NegativeInfinity;
        foreach (Vector3 corner in corners)
        {
            Vector2 p = RectTransformUtility.WorldToScreenPoint(uiCamera, corner);
            xMin = Mathf.Min(xMin, p.x / Screen.width);
            xMax = Mathf.Max(xMax, p.x / Screen.width);
            yMin = Mathf.Min(yMin, p.y / Screen.height);
            yMax = Mathf.Max(yMax, p.y / Screen.height);
        }
        float aspect = (float)Screen.width / Screen.height;
        Vector2 center = new Vector2((xMin + xMax) * 0.5f * aspect,
            (yMin + yMax) * 0.5f);
        float radius = (Mathf.Max((xMax - xMin) * aspect, yMax - yMin) * 0.5f
            + windowPadding) * scale;

        EnsureMaskTexture();
        int n = maskTexture.width;
        for (int y = 0; y < n; y++)
        {
            float ny = (float)y / (n - 1);
            for (int x = 0; x < n; x++)
            {
                float nx = (float)x / (n - 1);
                float d = Vector2.Distance(new Vector2(nx * aspect, ny), center) - radius;
                float a = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01(d / Mathf.Max(0.001f, windowSoftness)));
                maskPixels[y * n + x] =
                    new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
            }
        }
        maskTexture.SetPixels32(maskPixels);
        maskTexture.Apply(false, false);
        dimmerImage.sprite = maskSprite;
        dimmerImage.type = Image.Type.Simple;
        dimmerImage.preserveAspect = false;
        dimmerImage.raycastTarget = false;
        tutorialDimmer.SetActive(true);
    }

    private void EnsureMaskTexture()
    {
        int n = Mathf.Clamp(textureResolution, 64, 512);
        if (maskTexture != null && maskTexture.width == n) return;
        if (maskSprite != null) Destroy(maskSprite);
        if (maskTexture != null) Destroy(maskTexture);
        maskTexture = new Texture2D(n, n, TextureFormat.RGBA32, false);
        maskTexture.wrapMode = TextureWrapMode.Clamp;
        maskTexture.filterMode = FilterMode.Bilinear;
        maskPixels = new Color32[n * n];
        maskSprite = Sprite.Create(maskTexture, new Rect(0, 0, n, n),
            new Vector2(.5f, .5f), 100f);
        maskSprite.name = "TutorialClubSpotlight_Runtime";
    }

    private void RestoreDimmerImageOnly()
    {
        if (dimmerImage == null) return;
        dimmerImage.sprite = originalDimmerSprite;
        dimmerImage.type = originalDimmerType;
        dimmerImage.preserveAspect = originalPreserveAspect;
        dimmerImage.raycastTarget = originalRaycastTarget;
    }

    private void RestoreDimmer()
    {
        if (dimmerImage == null) return;
        RestoreDimmerImageOnly();
        if (tutorialDimmer != null) tutorialDimmer.SetActive(false);
    }

    // ============================================================
    // RED DURABILITY BAR - PREVIEW WITHOUT ALTERING GAMEPLAY DURABILITY
    // ============================================================

    private void StartBarFlash()
    {
        ResolveReferences();
        if (weaponDurabilityFill == null)
        {
            Debug.LogWarning("[CLUB TUTORIAL] Assign WeaponDurabilityFill for the 3 flashes.", this);
            SetGoldNextVisible(true); // Do not trap the player in a paused lesson.
            return;
        }
        flashCoroutine = StartCoroutine(FlashBarRoutine());
    }

    private IEnumerator FlashBarRoutine()
    {
        originalBarColor = weaponDurabilityFill.color;
        originalBarEnabled = weaponDurabilityFill.enabled;
        originalBarFill = weaponDurabilityFill.fillAmount;
        originalBarType = weaponDurabilityFill.type;
        flashStateCaptured = true;

        // ClubPickup only puts the club in a normal inventory slot.
        // A temporarily visible red half-ring is a VISUAL DEMO ONLY;
        // it never equips the club, changes inventory, or spends durability.
        bool preview = !originalBarEnabled && previewBarWhenUnequipped;
        if (preview)
        {
            weaponDurabilityFill.type = Image.Type.Filled;
            weaponDurabilityFill.fillAmount = 0.5f;
        }

        int count = Mathf.Max(1, tutorialFlashCount);
        float on = Mathf.Max(0.02f, flashVisibleSeconds);
        float off = Mathf.Max(0.02f, flashHiddenSeconds);
        for (int i = 0; i < count; i++)
        {
            weaponDurabilityFill.enabled = true;
            Color c = originalBarColor;
            c.a = preview ? 1f : originalBarColor.a;
            weaponDurabilityFill.color = c;
            yield return new WaitForSecondsRealtime(on);

            c.a = 0f;
            weaponDurabilityFill.color = c;
            yield return new WaitForSecondsRealtime(off);
        }
        RestoreBarVisual();
        flashCoroutine = null;
        if (phase == Phase.RedBar)
            SetGoldNextVisible(true);
    }

    private void StopBarFlashAndRestore()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        RestoreBarVisual();
    }

    private void RestoreBarVisual()
    {
        if (!flashStateCaptured || weaponDurabilityFill == null) return;
        weaponDurabilityFill.color = originalBarColor;
        weaponDurabilityFill.enabled = originalBarEnabled;
        weaponDurabilityFill.fillAmount = originalBarFill;
        weaponDurabilityFill.type = originalBarType;
        flashStateCaptured = false;
        if (weaponDurabilityUI != null)
            weaponDurabilityUI.RefreshUI();
    }

    // ============================================================
    // LOCALIZATION - ALL PAGES 31-42
    // ============================================================

    private void OnLocaleChanged(Locale locale)
    {
        BeginLocalizationLoad();
    }

    private void BeginLocalizationLoad()
    {
        localizationVersion++;
        if (localizationCoroutine != null) StopCoroutine(localizationCoroutine);
        localizationCoroutine = StartCoroutine(LoadLocalizedText(localizationVersion));
    }

    private IEnumerator LoadLocalizedText(int version)
    {
        string[] keys = {
            handQuestionKey, handUseKey, congratulationsKey,
            combatKey, durabilityKey, redBarKey,
            equipInstructionKey, equippedSuccessKey,
            unequipExplanationKey, unequipPracticeKey, reEquipKey,
            finalTryWeaponKey
        };
        string[] fallback = {
            handQuestionFallback, handUseFallback, congratulationsFallback,
            combatFallback, durabilityFallback, redBarFallback,
            equipInstructionFallback, equippedSuccessFallback,
            unequipExplanationFallback, unequipPracticeFallback, reEquipFallback,
            finalTryWeaponFallback
        };
        string[] result = (string[])fallback.Clone();

        for (int i = 0; i < keys.Length; i++)
        {
            var request = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
                localizationTable, keys[i]);
            yield return request;
            if (version != localizationVersion) yield break;
            if (request.Status == AsyncOperationStatus.Succeeded &&
                !string.IsNullOrWhiteSpace(request.Result))
                result[i] = request.Result;
        }

        localizedHandQuestion = result[0];
        localizedHandUse = result[1];
        localizedCongratulations = result[2];
        localizedCombat = result[3];
        localizedDurability = result[4];
        localizedRedBar = result[5];
        localizedEquipInstruction = result[6];
        localizedEquippedSuccess = result[7];
        localizedUnequipExplanation = result[8];
        localizedUnequipPractice = result[9];
        localizedReequip = result[10];
        localizedTryWeapon = result[11];
        localizationCoroutine = null;

        switch (phase)
        {
            case Phase.Question: SetKeeperMessage(localizedHandQuestion); break;
            case Phase.Instruction: SetKeeperMessage(localizedHandUse); break;
            case Phase.Congratulations: SetKeeperMessage(localizedCongratulations); break;
            case Phase.Combat: SetKeeperMessage(localizedCombat); break;
            case Phase.Durability: SetKeeperMessage(localizedDurability); break;
            case Phase.RedBar: SetKeeperMessage(localizedRedBar); break;
            case Phase.EquipInstruction: SetKeeperMessage(localizedEquipInstruction); break;
            case Phase.EquippedSuccess: SetKeeperMessage(localizedEquippedSuccess); break;
            case Phase.UnequipExplanation: SetKeeperMessage(localizedUnequipExplanation); break;
            case Phase.UnequipPractice: SetKeeperMessage(localizedUnequipPractice); break;
            case Phase.ReequipInstruction: SetKeeperMessage(localizedReequip); break;
            case Phase.TryWeapon: SetKeeperMessage(localizedTryWeapon); break;
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        if (goldNextButton != null)
            goldNextButton.onClick.RemoveListener(OnGoldNextClicked);
        if (handButton != null)
            handButton.onClick.RemoveListener(OnHandPressed);
        localizationVersion++;
        if (localizationCoroutine != null) StopCoroutine(localizationCoroutine);
        localizationCoroutine = null;
        StopBarFlashAndRestore();
        RestoreHandRaycasts();
        RestoreUnequipRaycasts();
        RestoreInventoryRaycasts();
        RestoreWeaponAttackRaycasts();

        if (phase != Phase.Waiting && phase != Phase.Finished)
        {
            SetGoldNextVisible(false);
            HideKeeperFrame();
            RestoreDimmer();
            ReleasePlayerAndTime();
        }
        RestoreKeeperBaseline();
    }

    private void OnDestroy()
    {
        if (maskSprite != null) Destroy(maskSprite);
        if (maskTexture != null) Destroy(maskTexture);
    }

    private void OnValidate()
    {
        textureResolution = Mathf.Clamp(textureResolution, 64, 512);
        handSpotlightScale = Mathf.Clamp(handSpotlightScale, 0.35f, 1.25f);
        weaponSpotlightScale = Mathf.Clamp(weaponSpotlightScale, 0.35f, 1.5f);
        clubSlotSpotlightScale = Mathf.Clamp(clubSlotSpotlightScale, 0.35f, 1.5f);
        unequipSpotlightScale = Mathf.Clamp(unequipSpotlightScale, 0.35f, 1.5f);
        if (finalTryWeaponKey == "tutorial_39")
            finalTryWeaponKey = "tutorial_42";
        windowSoftness = Mathf.Max(0.001f, windowSoftness);
        tutorialFlashCount = Mathf.Clamp(tutorialFlashCount, 1, 10);
        flashVisibleSeconds = Mathf.Max(0.02f, flashVisibleSeconds);
        flashHiddenSeconds = Mathf.Max(0.02f, flashHiddenSeconds);
    }
}
