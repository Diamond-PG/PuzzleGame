using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // ============================================================
    // SLOTS
    // ============================================================

    [Header("SLOTS")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotTemplate;

    [Tooltip("Уже существующие слоты на сцене.")]
    [SerializeField]
    private List<GameObject> existingSlots =
        new List<GameObject>();


    // ============================================================
    // ITEM CHILD
    // ============================================================

    [Header("ITEM CHILD")]
    [SerializeField] private string itemChildName = "Item";


    // ============================================================
    // EMPTY SLOT LOOK
    // ============================================================

    [Header("EMPTY SLOT LOOK")]

    [SerializeField, Range(0f, 1f)]
    private float emptySlotAlpha = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float occupiedSlotAlpha = 1f;


    // ============================================================
    // WEAPON SYSTEM
    // ============================================================

    [Header("WEAPON SYSTEM")]

    [SerializeField]
    private Button weaponButton;

    [SerializeField]
    private Image weaponButtonIcon;

    [SerializeField]
    private UnequipWeaponButton unequipButton;


    // ============================================================
    // PLAYER VISUAL
    // ============================================================

    [Header("PLAYER VISUAL")]

    [Tooltip("PlayerVisual на объекте Player.")]
    [SerializeField]
    private PlayerVisual playerVisual;


    // ============================================================
    // BIG WEAPON GLOW
    // ============================================================

    [Header("BIG WEAPON GLOW")]

    /*
     * Это старый ОБЩИЙ множитель.
     *
     * Его не убираем и не меняем,
     * чтобы меч и уже настроенные оружия
     * выглядели точно так же, как раньше.
     */
    [SerializeField, Range(0.30f, 1.50f)]
    private float bigWeaponGlowScale = 0.82f;


    // ============================================================
    // WEAPON MOVE SOUND
    // ============================================================

    [Header("WEAPON MOVE SOUND")]

    [SerializeField]
    private AudioSource weaponMoveAudioSource;

    [SerializeField]
    private AudioClip weaponMoveClip;

    [SerializeField, Range(0f, 1f)]
    private float weaponMoveVolume = 1f;

    [SerializeField]
    private bool playWeaponMoveSound = true;


    // ============================================================
    // WEAPON MOVE HAPTICS
    // ============================================================

    [Header("WEAPON MOVE HAPTICS")]

    [SerializeField]
    private bool useWeaponMoveHaptics = true;

    [SerializeField, Range(5, 100)]
    private int weaponMoveHapticMs = 18;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = true;


    // ============================================================
    // RUNTIME SLOTS
    // ============================================================

    private readonly List<GameObject> runtimeSlots =
        new List<GameObject>();


    // ============================================================
    // EQUIPPED WEAPON STATE
    // ============================================================

    private bool weaponEquipped;

    private Sprite currentWeaponSprite;

    private string currentWeaponId;

    private float currentWeaponDurability01 = 1f;


    // ============================================================
    // BIG WEAPON ICON DATA
    // ============================================================

    private Vector2 currentWeaponButtonIconSize;

    private float currentWeaponButtonIconRotation;

    private Vector2 currentWeaponButtonIconOffset;

    /*
     * Индивидуальный множитель
     * большого glow именно текущего оружия.
     *
     * Sword по старой системе = 1.
     * Club можно сделать, например, 1.15.
     */
    private float currentWeaponButtonGlowScale = 1f;


    // ============================================================
    // INVENTORY ICON DATA
    // ============================================================

    private Vector2 currentInventoryIconSize;

    private float currentInventoryIconRotation;

    private Vector2 currentInventoryIconOffset;

    private float currentInventoryIconScale = 1f;


    // ============================================================
    // GLOW DATA
    // ============================================================

    private Sprite currentGlowSprite;

    private Color currentGlowColor =
        Color.white;

    private Vector2 currentGlowSize;

    private Vector2 currentGlowOffset;

    private float currentGlowRotationOffset;

    private float currentGlowPulseSpeed = 1.4f;

    private float currentGlowScaleAmount = 0.08f;

    private float currentGlowMinAlpha = 0.55f;

    private float currentGlowMaxAlpha = 0.85f;

    private GameObject weaponGlowObject;


    // ============================================================
    // PUBLIC STATE
    // ============================================================

    public bool IsWeaponEquipped =>
        weaponEquipped;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (slotsParent == null)
        {
            slotsParent =
                transform;
        }

        if (slotTemplate != null)
        {
            slotTemplate.SetActive(
                false
            );
        }

        runtimeSlots.Clear();

        foreach (GameObject slot
                 in existingSlots)
        {
            if (slot != null)
            {
                runtimeSlots.Add(
                    slot
                );
            }
        }

        FindWeaponReferences();

        FindPlayerVisual();

        if (weaponMoveAudioSource == null)
        {
            weaponMoveAudioSource =
                GetComponent<AudioSource>();
        }

        weaponEquipped =
            false;

        currentWeaponDurability01 =
            1f;

        currentWeaponButtonGlowScale =
            1f;

        if (weaponButton != null)
        {
            weaponButton.interactable =
                false;
        }

        if (weaponButtonIcon != null)
        {
            weaponButtonIcon.sprite =
                null;

            weaponButtonIcon.enabled =
                true;

            weaponButtonIcon.gameObject.SetActive(
                false
            );
        }

        DisableWeaponButtonGlow();

        if (unequipButton != null)
        {
            unequipButton.SetWeaponEquipped(
                false
            );
        }

        if (playerVisual != null)
        {
            playerVisual.SetSwordEquipped(
                false
            );
        }

        RefreshInventorySlots();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Ready. Slots: " +
                runtimeSlots.Count,
                this
            );
        }
    }


    // ============================================================
    // LATE UPDATE
    // ============================================================

    private void LateUpdate()
    {
        RefreshInventorySlots();
    }


    // ============================================================
    // PLAYER VISUAL
    // ============================================================

    private void FindPlayerVisual()
    {
        if (playerVisual != null)
        {
            return;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject != null)
        {
            playerVisual =
                playerObject.GetComponent<PlayerVisual>();
        }

        if (playerVisual == null)
        {
            playerVisual =
                FindFirstObjectByType<PlayerVisual>();
        }
    }


    private void SetPlayerSwordEquipped(
        bool equipped
    )
    {
        if (playerVisual == null)
        {
            FindPlayerVisual();
        }

        if (playerVisual == null)
        {
            Debug.LogWarning(
                "[INVENTORY UI] PlayerVisual not found.",
                this
            );

            return;
        }

        playerVisual.SetSwordEquipped(
            equipped
        );
    }


    // ============================================================
    // WEAPON MOVE SOUND
    // ============================================================

    private void PlayWeaponMoveSound()
    {
        if (!playWeaponMoveSound ||
            weaponMoveClip == null)
        {
            return;
        }

        if (weaponMoveAudioSource == null)
        {
            weaponMoveAudioSource =
                GetComponent<AudioSource>();
        }

        if (weaponMoveAudioSource != null)
        {
            weaponMoveAudioSource.PlayOneShot(
                weaponMoveClip,
                weaponMoveVolume
            );
        }
        else
        {
            Debug.LogWarning(
                "[INVENTORY UI] Weapon move AudioSource missing!",
                this
            );
        }
    }


    // ============================================================
    // WEAPON MOVE HAPTIC
    // ============================================================

    private void PlayWeaponMoveHaptic()
    {
        if (!useWeaponMoveHaptics)
        {
            return;
        }

        MicroHaptics.Pulse(
            weaponMoveHapticMs,
            MicroHaptics.IOSHapticStyle.Light
        );
    }


    // ============================================================
    // FIND WEAPON REFERENCES
    // ============================================================

    private void FindWeaponReferences()
    {
        if (unequipButton == null)
        {
            unequipButton =
                FindFirstObjectByType<
                    UnequipWeaponButton
                >();
        }

        if (weaponButton != null &&
            weaponButtonIcon == null)
        {
            Transform iconTransform =
                weaponButton.transform.Find(
                    "WeaponIcon"
                );

            if (iconTransform != null)
            {
                weaponButtonIcon =
                    iconTransform.GetComponent<Image>();
            }

            if (weaponButtonIcon == null)
            {
                Image[] images =
                    weaponButton
                        .GetComponentsInChildren<Image>(
                            true
                        );

                foreach (Image image
                         in images)
                {
                    if (image == null)
                    {
                        continue;
                    }

                    if (image.gameObject ==
                        weaponButton.gameObject)
                    {
                        continue;
                    }

                    if (image.gameObject.name ==
                        "WeaponGlow")
                    {
                        continue;
                    }

                    weaponButtonIcon =
                        image;

                    break;
                }
            }
        }
    }


    // ============================================================
    // REFRESH INVENTORY
    // ============================================================

    private void RefreshInventorySlots()
    {
        int occupiedCount =
            0;

        foreach (GameObject slot
                 in runtimeSlots)
        {
            if (slot != null &&
                IsSlotOccupied(
                    slot
                ))
            {
                occupiedCount++;
            }
        }

        bool emptyPlaceholderShown =
            false;

        foreach (GameObject slot
                 in runtimeSlots)
        {
            if (slot == null)
            {
                continue;
            }

            bool occupied =
                IsSlotOccupied(
                    slot
                );

            if (occupied)
            {
                SetSlotVisible(
                    slot,
                    true
                );

                SetSlotGraphicAlpha(
                    slot,
                    occupiedSlotAlpha
                );

                continue;
            }

            if (occupiedCount > 0)
            {
                SetSlotVisible(
                    slot,
                    false
                );

                continue;
            }

            if (!emptyPlaceholderShown)
            {
                SetSlotVisible(
                    slot,
                    true
                );

                SetSlotGraphicAlpha(
                    slot,
                    emptySlotAlpha
                );

                emptyPlaceholderShown =
                    true;
            }
            else
            {
                SetSlotVisible(
                    slot,
                    false
                );
            }
        }
    }


    // ============================================================
    // SLOT OCCUPIED
    // ============================================================

    private bool IsSlotOccupied(
        GameObject slot
    )
    {
        if (slot == null)
        {
            return false;
        }

        Transform item =
            FindItemTransform(
                slot
            );

        if (item == null ||
            !item.gameObject.activeSelf)
        {
            return false;
        }

        Image itemImage =
            item.GetComponent<Image>();

        return
            itemImage != null &&
            itemImage.sprite != null;
    }


    // ============================================================
    // SLOT VISIBLE
    // ============================================================

    private void SetSlotVisible(
        GameObject slot,
        bool visible
    )
    {
        if (slot == null)
        {
            return;
        }

        if (slot.activeSelf !=
            visible)
        {
            slot.SetActive(
                visible
            );
        }
    }


    // ============================================================
    // SLOT ALPHA
    // ============================================================

    private void SetSlotGraphicAlpha(
        GameObject slot,
        float alpha
    )
    {
        if (slot == null)
        {
            return;
        }

        float safeAlpha =
            Mathf.Clamp01(
                alpha
            );

        Transform itemTransform =
            FindItemTransform(
                slot
            );

        Transform glowTransform =
            slot.transform.Find(
                "ItemGlow"
            );

        Image[] images =
            slot.GetComponentsInChildren<Image>(
                true
            );

        foreach (Image image
                 in images)
        {
            if (image == null)
            {
                continue;
            }

            if (itemTransform != null)
            {
                if (image.transform ==
                    itemTransform)
                {
                    continue;
                }

                if (image.transform.IsChildOf(
                        itemTransform
                    ))
                {
                    continue;
                }
            }

            if (glowTransform != null)
            {
                if (image.transform ==
                    glowTransform)
                {
                    continue;
                }

                if (image.transform.IsChildOf(
                        glowTransform
                    ))
                {
                    continue;
                }
            }

            Color color =
                image.color;

            color.a =
                safeAlpha;

            image.color =
                color;
        }
    }


    // ============================================================
    // GET OR CREATE FREE SLOT
    // ============================================================

    public GameObject GetOrCreateFreeSlot()
    {
        GameObject freeSlot =
            FindFreeSlot();

        if (freeSlot != null)
        {
            PrepareSlotForUse(
                freeSlot
            );

            return freeSlot;
        }

        GameObject newSlot =
            CreateSlot();

        if (newSlot != null)
        {
            PrepareSlotForUse(
                newSlot
            );
        }

        return newSlot;
    }


    // ============================================================
    // PREPARE SLOT
    // ============================================================

    private void PrepareSlotForUse(
        GameObject slot
    )
    {
        if (slot == null)
        {
            return;
        }

        if (!slot.activeSelf)
        {
            slot.SetActive(
                true
            );
        }

        SetSlotGraphicAlpha(
            slot,
            emptySlotAlpha
        );

        Canvas.ForceUpdateCanvases();

        RectTransform parentRect =
            slotsParent as RectTransform;

        if (parentRect != null)
        {
            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    parentRect
                );
        }

        RectTransform slotRect =
            slot.GetComponent<RectTransform>();

        if (slotRect != null)
        {
            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    slotRect
                );
        }

        Canvas.ForceUpdateCanvases();
    }


    // ============================================================
    // SHOW ITEM - SIMPLE
    // ============================================================

    public bool ShowItemInSlot(
        GameObject slot,
        Sprite itemSprite,
        Vector2 iconSize,
        float rotationZ,
        Vector2 iconOffset,
        float iconScale
    )
    {
        return ShowItemInSlot(
            slot,
            itemSprite,
            iconSize,
            rotationZ,
            iconOffset,
            iconScale,

            null,
            Color.white,
            Vector2.zero,
            Vector2.zero,

            0f,

            1.4f,
            0.08f,
            0.55f,
            0.85f
        );
    }


    // ============================================================
    // SHOW ITEM - OLD GLOW VERSION
    // ============================================================

    public bool ShowItemInSlot(
        GameObject slot,
        Sprite itemSprite,
        Vector2 iconSize,
        float rotationZ,
        Vector2 iconOffset,
        float iconScale,

        Sprite glowSprite,
        Color glowColor,
        Vector2 glowSize,
        Vector2 glowOffset,

        float glowPulseSpeed,
        float glowScaleAmount,
        float glowMinAlpha,
        float glowMaxAlpha
    )
    {
        return ShowItemInSlot(
            slot,
            itemSprite,
            iconSize,
            rotationZ,
            iconOffset,
            iconScale,

            glowSprite,
            glowColor,
            glowSize,
            glowOffset,

            0f,

            glowPulseSpeed,
            glowScaleAmount,
            glowMinAlpha,
            glowMaxAlpha
        );
    }


    // ============================================================
    // SHOW ITEM - INDEPENDENT GLOW ROTATION
    // ============================================================

    public bool ShowItemInSlot(
        GameObject slot,
        Sprite itemSprite,
        Vector2 iconSize,
        float rotationZ,
        Vector2 iconOffset,
        float iconScale,

        Sprite glowSprite,
        Color glowColor,
        Vector2 glowSize,
        Vector2 glowOffset,

        float glowRotationOffset,

        float glowPulseSpeed,
        float glowScaleAmount,
        float glowMinAlpha,
        float glowMaxAlpha
    )
    {
        if (slot == null ||
            itemSprite == null)
        {
            return false;
        }

        PrepareSlotForUse(
            slot
        );

        Transform itemTransform =
            FindItemTransform(
                slot
            );

        if (itemTransform == null)
        {
            Debug.LogError(
                "[INVENTORY UI] Item child not found in " +
                slot.name,
                slot
            );

            return false;
        }

        Image itemImage =
            itemTransform
                .GetComponent<Image>();

        if (itemImage == null)
        {
            Debug.LogError(
                "[INVENTORY UI] Image missing on " +
                itemTransform.name,
                itemTransform
            );

            return false;
        }

        RectTransform itemRect =
            itemTransform
                as RectTransform;

        itemImage.sprite =
            itemSprite;

        itemImage.color =
            Color.white;

        itemImage.preserveAspect =
            true;

        itemImage.raycastTarget =
            false;

        if (itemRect != null)
        {
            itemRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            itemRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            itemRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            itemRect.anchoredPosition =
                iconOffset;

            itemRect.sizeDelta =
                iconSize;

            itemRect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotationZ
                );

            float safeScale =
                Mathf.Max(
                    0.01f,
                    iconScale
                );

            itemRect.localScale =
                new Vector3(
                    safeScale,
                    safeScale,
                    1f
                );
        }

        itemTransform.gameObject.SetActive(
            true
        );

        if (glowSprite != null)
        {
            CreateOrUpdateGlow(
                slot,
                itemTransform,

                glowSprite,
                glowColor,
                glowSize,
                glowOffset,

                rotationZ,
                glowRotationOffset,

                glowPulseSpeed,
                glowScaleAmount,
                glowMinAlpha,
                glowMaxAlpha
            );
        }
        else
        {
            DisableGlow(
                slot
            );
        }

        SetSlotVisible(
            slot,
            true
        );

        SetSlotGraphicAlpha(
            slot,
            occupiedSlotAlpha
        );

        Canvas.ForceUpdateCanvases();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Item placed into " +
                slot.name,
                this
            );
        }

        return true;
    }


    // ============================================================
    // GENERIC ITEM ACCESS
    // ============================================================

    public Transform GetItemTransform(
        GameObject slot
    )
    {
        return FindItemTransform(
            slot
        );
    }


    // ============================================================
    // REMOVE ITEM
    // ============================================================

    public void RemoveItemFromSlot(
        GameObject slot
    )
    {
        if (slot == null)
        {
            return;
        }

        Transform itemTransform =
            FindItemTransform(
                slot
            );

        if (itemTransform != null)
        {
            Image itemImage =
                itemTransform.GetComponent<Image>();

            if (itemImage != null)
            {
                itemImage.sprite =
                    null;

                itemImage.color =
                    Color.white;
            }

            itemTransform.gameObject.SetActive(
                false
            );
        }

        DisableGlow(
            slot
        );

        InventoryWeaponSlot weaponSlot =
            slot.GetComponent<
                InventoryWeaponSlot
            >();

        if (weaponSlot != null)
        {
            weaponSlot.ClearWeaponData();
        }

        RefreshInventorySlots();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Slot cleared: " +
                slot.name,
                this
            );
        }
    }


    // ============================================================
    // CONFIGURE WEAPON SLOT - NEW WEAPON
    // ============================================================

    public void ConfigureWeaponSlot(
        GameObject slot,
        Sprite weaponSprite,
        string weaponId,
        Vector2 buttonIconSize,
        float buttonIconRotation,
        Vector2 buttonIconOffset
    )
    {
        ConfigureWeaponSlot(
            slot,
            weaponSprite,
            weaponId,
            buttonIconSize,
            buttonIconRotation,
            buttonIconOffset,
            1f,
            1f
        );
    }


    // ============================================================
    // CONFIGURE WEAPON SLOT - WITH DURABILITY
    // ============================================================

    public void ConfigureWeaponSlot(
        GameObject slot,
        Sprite weaponSprite,
        string weaponId,
        Vector2 buttonIconSize,
        float buttonIconRotation,
        Vector2 buttonIconOffset,
        float weaponDurability01
    )
    {
        ConfigureWeaponSlot(
            slot,
            weaponSprite,
            weaponId,
            buttonIconSize,
            buttonIconRotation,
            buttonIconOffset,
            weaponDurability01,
            1f
        );
    }


    // ============================================================
    // CONFIGURE WEAPON SLOT - DURABILITY + BIG GLOW SCALE
    // ============================================================

    public void ConfigureWeaponSlot(
        GameObject slot,
        Sprite weaponSprite,
        string weaponId,
        Vector2 buttonIconSize,
        float buttonIconRotation,
        Vector2 buttonIconOffset,
        float weaponDurability01,
        float weaponButtonGlowScale
    )
    {
        if (slot == null)
        {
            return;
        }

        FindWeaponReferences();

        InventoryWeaponSlot weaponSlot =
            slot.GetComponent<
                InventoryWeaponSlot
            >();

        if (weaponSlot == null)
        {
            weaponSlot =
                slot.AddComponent<
                    InventoryWeaponSlot
                >();
        }

        weaponSlot.SetupWeapon(
            weaponSprite,
            weaponId,

            weaponButton,
            weaponButtonIcon,

            buttonIconSize,
            buttonIconRotation,
            buttonIconOffset,

            this,
            unequipButton,

            Mathf.Clamp01(
                weaponDurability01
            ),

            Mathf.Max(
                0.1f,
                weaponButtonGlowScale
            )
        );

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Weapon slot configured: " +
                slot.name +
                " -> " +
                weaponId +
                " | Durability = " +
                Mathf.RoundToInt(
                    weaponDurability01 *
                    100f
                ) +
                "% | Big Glow Scale = " +
                weaponButtonGlowScale,
                this
            );
        }
    }


    // ============================================================
    // EQUIP WEAPON - LEGACY VERSION
    // ============================================================

    public bool EquipWeaponFromSlot(
        GameObject sourceSlot,
        Sprite weaponSprite,
        string weaponId,
        Button sourceWeaponButton,
        Image sourceWeaponButtonIcon,
        Vector2 buttonIconSize,
        float buttonIconRotation,
        Vector2 buttonIconOffset
    )
    {
        return EquipWeaponFromSlot(
            sourceSlot,
            weaponSprite,
            weaponId,

            sourceWeaponButton,
            sourceWeaponButtonIcon,

            buttonIconSize,
            buttonIconRotation,
            buttonIconOffset,

            1f,
            1f
        );
    }


    // ============================================================
    // EQUIP WEAPON - WITH DURABILITY
    // ============================================================

    public bool EquipWeaponFromSlot(
        GameObject sourceSlot,
        Sprite weaponSprite,
        string weaponId,
        Button sourceWeaponButton,
        Image sourceWeaponButtonIcon,
        Vector2 buttonIconSize,
        float buttonIconRotation,
        Vector2 buttonIconOffset,
        float weaponDurability01
    )
    {
        return EquipWeaponFromSlot(
            sourceSlot,
            weaponSprite,
            weaponId,

            sourceWeaponButton,
            sourceWeaponButtonIcon,

            buttonIconSize,
            buttonIconRotation,
            buttonIconOffset,

            weaponDurability01,
            1f
        );
    }


    // ============================================================
    // EQUIP WEAPON - DURABILITY + BIG GLOW SCALE
    // ============================================================

    public bool EquipWeaponFromSlot(
        GameObject sourceSlot,
        Sprite weaponSprite,
        string weaponId,
        Button sourceWeaponButton,
        Image sourceWeaponButtonIcon,
        Vector2 buttonIconSize,
        float buttonIconRotation,
        Vector2 buttonIconOffset,
        float weaponDurability01,
        float weaponButtonGlowScale
    )
    {
        if (sourceSlot == null ||
            weaponSprite == null)
        {
            return false;
        }

        if (weaponEquipped)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[INVENTORY UI] Weapon already equipped.",
                    this
                );
            }

            return false;
        }

        Transform itemTransform =
            FindItemTransform(
                sourceSlot
            );

        if (itemTransform == null)
        {
            return false;
        }

        Image itemImage =
            itemTransform
                .GetComponent<Image>();

        if (itemImage == null ||
            !itemTransform.gameObject.activeSelf)
        {
            return false;
        }


        // ========================================================
        // SAVE INVENTORY ICON LOOK
        // ========================================================

        RectTransform itemRect =
            itemTransform
                as RectTransform;

        if (itemRect != null)
        {
            currentInventoryIconSize =
                itemRect.sizeDelta;

            currentInventoryIconRotation =
                itemRect.localEulerAngles.z;

            currentInventoryIconOffset =
                itemRect.anchoredPosition;

            currentInventoryIconScale =
                itemRect.localScale.x;
        }


        // ========================================================
        // SAVE GLOW LOOK
        // ========================================================

        Transform glowTransform =
            sourceSlot.transform.Find(
                "ItemGlow"
            );

        if (glowTransform != null)
        {
            Image glowImage =
                glowTransform
                    .GetComponent<Image>();

            RectTransform glowRect =
                glowTransform
                    as RectTransform;

            if (glowImage != null)
            {
                currentGlowSprite =
                    glowImage.sprite;

                currentGlowColor =
                    glowImage.color;
            }

            if (glowRect != null)
            {
                currentGlowSize =
                    glowRect.sizeDelta;

                Quaternion inverseRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        -currentInventoryIconRotation
                    );

                currentGlowOffset =
                    inverseRotation *
                    glowRect.anchoredPosition;

                currentGlowRotationOffset =
                    Mathf.DeltaAngle(
                        currentInventoryIconRotation,
                        glowRect.localEulerAngles.z
                    );
            }
        }
        else
        {
            currentGlowSprite =
                null;

            currentGlowSize =
                Vector2.zero;

            currentGlowOffset =
                Vector2.zero;

            currentGlowRotationOffset =
                0f;
        }


        // ========================================================
        // SAVE WEAPON DATA
        // ========================================================

        currentWeaponSprite =
            weaponSprite;

        currentWeaponId =
            weaponId;

        currentWeaponDurability01 =
            Mathf.Clamp01(
                weaponDurability01
            );

        currentWeaponButtonIconSize =
            buttonIconSize;

        currentWeaponButtonIconRotation =
            buttonIconRotation;

        currentWeaponButtonIconOffset =
            buttonIconOffset;

        currentWeaponButtonGlowScale =
            Mathf.Max(
                0.1f,
                weaponButtonGlowScale
            );


        // ========================================================
        // FIND BIG WEAPON REFERENCES
        // ========================================================

        if (weaponButton == null)
        {
            weaponButton =
                sourceWeaponButton;
        }

        if (weaponButtonIcon == null)
        {
            weaponButtonIcon =
                sourceWeaponButtonIcon;
        }

        FindWeaponReferences();

        if (weaponButton == null ||
            weaponButtonIcon == null)
        {
            Debug.LogError(
                "[INVENTORY UI] WeaponButton references missing!",
                this
            );

            ClearCurrentWeaponData();

            return false;
        }


        // ========================================================
        // BIG WEAPON ICON
        // ========================================================

        weaponButtonIcon.gameObject.SetActive(
            true
        );

        weaponButtonIcon.enabled =
            true;

        weaponButtonIcon.sprite =
            currentWeaponSprite;

        weaponButtonIcon.color =
            Color.white;

        weaponButtonIcon.preserveAspect =
            true;

        weaponButtonIcon.raycastTarget =
            false;

        RectTransform bigIconRect =
            weaponButtonIcon.rectTransform;

        bigIconRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        bigIconRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        bigIconRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        bigIconRect.anchoredPosition =
            currentWeaponButtonIconOffset;

        bigIconRect.sizeDelta =
            currentWeaponButtonIconSize;

        bigIconRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                currentWeaponButtonIconRotation
            );

        bigIconRect.localScale =
            Vector3.one;


        CreateOrUpdateWeaponButtonGlow();


        weaponButtonIcon
            .transform
            .SetAsLastSibling();


        weaponButton.interactable =
            true;


        weaponEquipped =
            true;


        // ========================================================
        // REMOVE ITEM FROM SOURCE SLOT
        // ========================================================

        itemTransform.gameObject.SetActive(
            false
        );

        DisableGlow(
            sourceSlot
        );

        InventoryWeaponSlot oldWeaponSlot =
            sourceSlot.GetComponent<
                InventoryWeaponSlot
            >();

        if (oldWeaponSlot != null)
        {
            oldWeaponSlot.ClearWeaponData();
        }

        RefreshInventorySlots();


        // ========================================================
        // UNEQUIP BUTTON
        // ========================================================

        if (unequipButton == null)
        {
            unequipButton =
                FindFirstObjectByType<
                    UnequipWeaponButton
                >();
        }

        if (unequipButton != null)
        {
            unequipButton.SetWeaponEquipped(
                true
            );
        }


        // ========================================================
        // PLAYER VISUAL
        // ========================================================

        SetPlayerSwordEquipped(
            true
        );

        PlayWeaponMoveSound();

        PlayWeaponMoveHaptic();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Equipped weapon: " +
                currentWeaponId +
                " | Durability = " +
                Mathf.RoundToInt(
                    currentWeaponDurability01 *
                    100f
                ) +
                "% | Big Glow Scale = " +
                currentWeaponButtonGlowScale,
                this
            );
        }

        return true;
    }


    // ============================================================
    // EQUIPPED WEAPON DURABILITY
    // ============================================================

    public float GetEquippedWeaponDurability01()
    {
        return
            Mathf.Clamp01(
                currentWeaponDurability01
            );
    }


    public void SetEquippedWeaponDurability01(
        float value
    )
    {
        currentWeaponDurability01 =
            Mathf.Clamp01(
                value
            );
    }


    // ============================================================
    // UNEQUIP WEAPON
    // ============================================================

    public bool UnequipCurrentWeapon()
    {
        if (!weaponEquipped ||
            currentWeaponSprite == null)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[INVENTORY UI] Nothing to unequip.",
                    this
                );
            }

            return false;
        }

        GameObject targetSlot =
            GetOrCreateFreeSlot();

        if (targetSlot == null)
        {
            Debug.LogWarning(
                "[INVENTORY UI] No free slot for weapon!",
                this
            );

            return false;
        }

        bool placed =
            ShowItemInSlot(
                targetSlot,
                currentWeaponSprite,

                currentInventoryIconSize,
                currentInventoryIconRotation,
                currentInventoryIconOffset,
                currentInventoryIconScale,

                currentGlowSprite,
                currentGlowColor,
                currentGlowSize,
                currentGlowOffset,

                currentGlowRotationOffset,

                currentGlowPulseSpeed,
                currentGlowScaleAmount,
                currentGlowMinAlpha,
                currentGlowMaxAlpha
            );

        if (!placed)
        {
            return false;
        }

        ConfigureWeaponSlot(
            targetSlot,
            currentWeaponSprite,
            currentWeaponId,

            currentWeaponButtonIconSize,
            currentWeaponButtonIconRotation,
            currentWeaponButtonIconOffset,

            currentWeaponDurability01,
            currentWeaponButtonGlowScale
        );


        // ========================================================
        // CLEAR BIG ICON
        // ========================================================

        if (weaponButtonIcon != null)
        {
            weaponButtonIcon.sprite =
                null;

            weaponButtonIcon.gameObject.SetActive(
                false
            );
        }

        DisableWeaponButtonGlow();

        if (weaponButton != null)
        {
            weaponButton.interactable =
                false;
        }

        if (unequipButton != null)
        {
            unequipButton.SetWeaponEquipped(
                false
            );
        }

        SetPlayerSwordEquipped(
            false
        );

        PlayWeaponMoveSound();

        PlayWeaponMoveHaptic();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Weapon returned to free slot: " +
                targetSlot.name +
                " | Durability = " +
                Mathf.RoundToInt(
                    currentWeaponDurability01 *
                    100f
                ) +
                "%",
                this
            );
        }

        ClearCurrentWeaponData();

        RefreshInventorySlots();

        return true;
    }


    // ============================================================
    // BREAK EQUIPPED WEAPON
    // ============================================================

    public bool BreakEquippedWeapon()
    {
        if (!weaponEquipped)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[INVENTORY UI] Break ignored: no weapon equipped.",
                    this
                );
            }

            return false;
        }

        if (weaponButtonIcon != null)
        {
            weaponButtonIcon.sprite =
                null;

            weaponButtonIcon.enabled =
                false;

            weaponButtonIcon.gameObject.SetActive(
                false
            );
        }

        DisableWeaponButtonGlow();

        if (weaponButton != null)
        {
            weaponButton.interactable =
                false;
        }

        if (unequipButton != null)
        {
            unequipButton.SetWeaponEquipped(
                false
            );
        }

        SetPlayerSwordEquipped(
            false
        );

        ClearCurrentWeaponData();

        RefreshInventorySlots();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] WEAPON BROKEN AND DESTROYED.",
                this
            );
        }

        return true;
    }


    // ============================================================
    // CLEAR CURRENT WEAPON DATA
    // ============================================================

    private void ClearCurrentWeaponData()
    {
        weaponEquipped =
            false;

        currentWeaponSprite =
            null;

        currentWeaponId =
            "";

        currentWeaponDurability01 =
            1f;

        currentWeaponButtonIconSize =
            Vector2.zero;

        currentWeaponButtonIconRotation =
            0f;

        currentWeaponButtonIconOffset =
            Vector2.zero;

        currentWeaponButtonGlowScale =
            1f;

        currentInventoryIconSize =
            Vector2.zero;

        currentInventoryIconRotation =
            0f;

        currentInventoryIconOffset =
            Vector2.zero;

        currentInventoryIconScale =
            1f;

        currentGlowSprite =
            null;

        currentGlowColor =
            Color.white;

        currentGlowSize =
            Vector2.zero;

        currentGlowOffset =
            Vector2.zero;

        currentGlowRotationOffset =
            0f;
    }


    // ============================================================
    // BIG WEAPON GLOW
    // ============================================================

    private void CreateOrUpdateWeaponButtonGlow()
    {
        if (weaponButton == null ||
            weaponButtonIcon == null)
        {
            return;
        }

        if (currentGlowSprite == null)
        {
            DisableWeaponButtonGlow();

            return;
        }

        Transform existingGlow =
            weaponButton.transform.Find(
                "WeaponGlow"
            );

        if (existingGlow != null)
        {
            weaponGlowObject =
                existingGlow.gameObject;
        }
        else
        {
            weaponGlowObject =
                new GameObject(
                    "WeaponGlow",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(InventoryItemGlow)
                );

            weaponGlowObject.transform.SetParent(
                weaponButton.transform,
                false
            );
        }

        RectTransform glowRect =
            weaponGlowObject
                .GetComponent<RectTransform>();

        Image glowImage =
            weaponGlowObject
                .GetComponent<Image>();

        InventoryItemGlow glowPulse =
            weaponGlowObject
                .GetComponent<InventoryItemGlow>();

        glowRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        glowRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        glowRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        float sizeMultiplier =
            1f;


        if (currentInventoryIconSize.x >
                0.01f &&
            currentInventoryIconSize.y >
                0.01f)
        {
            float xMultiplier =
                currentWeaponButtonIconSize.x /
                currentInventoryIconSize.x;


            float yMultiplier =
                currentWeaponButtonIconSize.y /
                currentInventoryIconSize.y;


            sizeMultiplier =
                Mathf.Max(
                    xMultiplier,
                    yMultiplier
                );
        }


        /*
         * Старое смещение НЕ меняем.
         *
         * Новый индивидуальный параметр
         * меняет только РАЗМЕР glow,
         * чтобы дубинка не уезжала
         * в сторону при увеличении.
         */
        Vector2 scaledGlowOffset =
            currentGlowOffset *
            sizeMultiplier *
            bigWeaponGlowScale;


        Vector2 rotatedGlowOffset =
            Quaternion.Euler(
                0f,
                0f,
                currentWeaponButtonIconRotation
            ) *
            scaledGlowOffset;


        glowRect.anchoredPosition =
            currentWeaponButtonIconOffset +
            rotatedGlowOffset;


        /*
         * ВОТ ЗДЕСЬ применяется
         * индивидуальный размер.
         *
         * Для старых оружий:
         * currentWeaponButtonGlowScale = 1.
         *
         * Поэтому меч вообще
         * визуально не изменится.
         */
        glowRect.sizeDelta =
            currentGlowSize *
            sizeMultiplier *
            bigWeaponGlowScale *
            currentWeaponButtonGlowScale;


        glowRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                currentWeaponButtonIconRotation +
                currentGlowRotationOffset
            );


        glowRect.localScale =
            Vector3.one;


        glowImage.sprite =
            currentGlowSprite;


        glowImage.color =
            currentGlowColor;


        glowImage.preserveAspect =
            false;


        glowImage.raycastTarget =
            false;


        if (glowPulse != null)
        {
            glowPulse.Setup(
                currentGlowPulseSpeed,
                currentGlowScaleAmount,
                currentGlowMinAlpha,
                currentGlowMaxAlpha
            );
        }


        weaponGlowObject.SetActive(
            true
        );


        int iconIndex =
            weaponButtonIcon
                .transform
                .GetSiblingIndex();


        weaponGlowObject
            .transform
            .SetSiblingIndex(
                Mathf.Max(
                    0,
                    iconIndex
                )
            );


        weaponButtonIcon
            .transform
            .SetAsLastSibling();
    }


    // ============================================================
    // DISABLE BIG WEAPON GLOW
    // ============================================================

    private void DisableWeaponButtonGlow()
    {
        if (weaponGlowObject == null &&
            weaponButton != null)
        {
            Transform glow =
                weaponButton.transform.Find(
                    "WeaponGlow"
                );


            if (glow != null)
            {
                weaponGlowObject =
                    glow.gameObject;
            }
        }


        if (weaponGlowObject != null)
        {
            weaponGlowObject.SetActive(
                false
            );
        }
    }


    // ============================================================
    // CREATE / UPDATE SLOT GLOW
    // ============================================================

    private void CreateOrUpdateGlow(
        GameObject slot,
        Transform itemTransform,

        Sprite glowSprite,
        Color glowColor,
        Vector2 glowSize,
        Vector2 glowOffset,

        float rotationZ,
        float glowRotationOffset,

        float glowPulseSpeed,
        float glowScaleAmount,
        float glowMinAlpha,
        float glowMaxAlpha
    )
    {
        Transform existingGlow =
            slot.transform.Find(
                "ItemGlow"
            );


        GameObject glowObject;


        if (existingGlow != null)
        {
            glowObject =
                existingGlow.gameObject;
        }
        else
        {
            glowObject =
                new GameObject(
                    "ItemGlow",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(InventoryItemGlow)
                );


            glowObject.transform.SetParent(
                slot.transform,
                false
            );
        }


        RectTransform glowRect =
            glowObject
                .GetComponent<RectTransform>();


        Image glowImage =
            glowObject
                .GetComponent<Image>();


        InventoryItemGlow glowPulse =
            glowObject
                .GetComponent<InventoryItemGlow>();


        glowRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        glowRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        glowRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        Vector2 rotatedOffset =
            Quaternion.Euler(
                0f,
                0f,
                rotationZ
            ) *
            glowOffset;


        glowRect.anchoredPosition =
            rotatedOffset;


        glowRect.sizeDelta =
            glowSize;


        glowRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                rotationZ +
                glowRotationOffset
            );


        glowRect.localScale =
            Vector3.one;


        glowImage.sprite =
            glowSprite;


        glowImage.color =
            glowColor;


        glowImage.preserveAspect =
            false;


        glowImage.raycastTarget =
            false;


        if (glowPulse != null)
        {
            glowPulse.Setup(
                glowPulseSpeed,
                glowScaleAmount,
                glowMinAlpha,
                glowMaxAlpha
            );
        }


        int itemIndex =
            itemTransform
                .GetSiblingIndex();


        glowObject.transform.SetSiblingIndex(
            Mathf.Max(
                0,
                itemIndex
            )
        );


        itemTransform.SetSiblingIndex(
            glowObject
                .transform
                .GetSiblingIndex() + 1
        );


        glowObject.SetActive(
            true
        );
    }


    // ============================================================
    // DISABLE SLOT GLOW
    // ============================================================

    private void DisableGlow(
        GameObject slot
    )
    {
        if (slot == null)
        {
            return;
        }


        Transform glow =
            slot.transform.Find(
                "ItemGlow"
            );


        if (glow != null)
        {
            glow.gameObject.SetActive(
                false
            );
        }
    }


    // ============================================================
    // GET SLOT RECT
    // ============================================================

    public RectTransform GetSlotRect(
        GameObject slot
    )
    {
        if (slot == null)
        {
            return null;
        }


        return
            slot.GetComponent<
                RectTransform
            >();
    }


    // ============================================================
    // FIND FREE SLOT
    // ============================================================

    private GameObject FindFreeSlot()
    {
        foreach (GameObject slot
                 in runtimeSlots)
        {
            if (slot == null)
            {
                continue;
            }


            if (!IsSlotOccupied(
                    slot
                ))
            {
                return slot;
            }
        }


        return null;
    }


    // ============================================================
    // CREATE SLOT
    // ============================================================

    private GameObject CreateSlot()
    {
        if (slotTemplate == null)
        {
            Debug.LogError(
                "[INVENTORY UI] SlotTemplate missing!",
                this
            );


            return null;
        }


        GameObject slot =
            Instantiate(
                slotTemplate,
                slotsParent
            );


        slot.name =
            "Slot_" +
            (runtimeSlots.Count + 1);


        slot.SetActive(
            true
        );


        Transform item =
            FindItemTransform(
                slot
            );


        if (item != null)
        {
            item.gameObject.SetActive(
                false
            );
        }


        runtimeSlots.Add(
            slot
        );


        SetSlotGraphicAlpha(
            slot,
            emptySlotAlpha
        );


        return slot;
    }


    // ============================================================
    // FIND ITEM TRANSFORM
    // ============================================================

    private Transform FindItemTransform(
        GameObject slot
    )
    {
        if (slot == null)
        {
            return null;
        }


        Transform exact =
            slot.transform.Find(
                itemChildName
            );


        if (exact != null)
        {
            return exact;
        }


        foreach (Transform child
                 in slot.transform)
        {
            if (child == null)
            {
                continue;
            }


            if (child.name.StartsWith(
                    "Item_"
                ))
            {
                return child;
            }
        }


        return null;
    }
}