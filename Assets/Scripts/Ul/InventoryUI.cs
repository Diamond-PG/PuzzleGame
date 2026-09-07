using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // ============================================================
    // SLOTS
    // ============================================================

    [Header("SLOTS")]

    [SerializeField]
    private Transform slotsParent;

    [SerializeField]
    private GameObject slotTemplate;

    [Tooltip("Уже существующие слоты на сцене.")]
    [SerializeField]
    private List<GameObject> existingSlots =
        new List<GameObject>();

    // ============================================================
    // ITEM CHILD
    // ============================================================

    [Header("ITEM CHILD")]

    [SerializeField]
    private string itemChildName = "Item";

    // ============================================================
    // EMPTY SLOT LOOK
    // ============================================================

    [Header("EMPTY SLOT LOOK")]

    [Tooltip(
        "Прозрачность единственной пустой ячейки."
    )]
    [SerializeField, Range(0f, 1f)]
    private float emptySlotAlpha = 0.4f;

    [Tooltip(
        "Прозрачность занятой ячейки."
    )]
    [SerializeField, Range(0f, 1f)]
    private float occupiedSlotAlpha = 1f;

    // ============================================================
    // WEAPON SYSTEM
    // ============================================================

    [Header("WEAPON SYSTEM")]

    [Tooltip("Большая круглая кнопка оружия.")]
    [SerializeField]
    private Button weaponButton;

    [Tooltip("WeaponIcon внутри большого круга.")]
    [SerializeField]
    private Image weaponButtonIcon;

    [Tooltip("Маленькая кнопка возврата оружия.")]
    [SerializeField]
    private UnequipWeaponButton unequipButton;

    // ============================================================
    // BIG WEAPON GLOW
    // ============================================================

    [Header("BIG WEAPON GLOW")]

    [Tooltip(
        "Размер голубого свечения оружия " +
        "в большом круге."
    )]
    [SerializeField, Range(0.30f, 1.50f)]
    private float bigWeaponGlowScale = 0.82f;

    // ============================================================
    // WEAPON MOVE SOUND
    // ============================================================

    [Header("WEAPON MOVE SOUND")]

    [Tooltip(
        "AudioSource на InventoryPanel. " +
        "Если оставить пустым, скрипт попробует найти его сам."
    )]
    [SerializeField]
    private AudioSource weaponMoveAudioSource;

    [Tooltip(
        "Один и тот же звук используется " +
        "при перемещении оружия туда и обратно."
    )]
    [SerializeField]
    private AudioClip weaponMoveClip;

    [Tooltip("Громкость звука перемещения оружия.")]
    [SerializeField, Range(0f, 1f)]
    private float weaponMoveVolume = 1f;

    [Tooltip(
        "Можно быстро отключить звук, " +
        "не удаляя AudioClip."
    )]
    [SerializeField]
    private bool playWeaponMoveSound = true;

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
    // CURRENT WEAPON
    // ============================================================

    private bool weaponEquipped;

    private Sprite currentWeaponSprite;
    private string currentWeaponId;

    private Vector2 currentWeaponButtonIconSize;
    private float currentWeaponButtonIconRotation;
    private Vector2 currentWeaponButtonIconOffset;

    // ============================================================
    // INVENTORY LOOK BACKUP
    // ============================================================

    private Vector2 currentInventoryIconSize;
    private float currentInventoryIconRotation;
    private Vector2 currentInventoryIconOffset;
    private float currentInventoryIconScale = 1f;

    private Sprite currentGlowSprite;
    private Color currentGlowColor = Color.white;

    private Vector2 currentGlowSize;
    private Vector2 currentGlowOffset;

    private float currentGlowPulseSpeed = 1.4f;
    private float currentGlowScaleAmount = 0.08f;
    private float currentGlowMinAlpha = 0.55f;
    private float currentGlowMaxAlpha = 0.85f;

    // ============================================================
    // BIG WEAPON BUTTON GLOW
    // ============================================================

    private GameObject weaponGlowObject;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (slotsParent == null)
        {
            slotsParent = transform;
        }

        if (slotTemplate != null)
        {
            slotTemplate.SetActive(false);
        }

        runtimeSlots.Clear();

        foreach (GameObject slot in existingSlots)
        {
            if (slot == null)
                continue;

            runtimeSlots.Add(slot);
        }

        FindWeaponReferences();

        if (weaponMoveAudioSource == null)
        {
            weaponMoveAudioSource =
                GetComponent<AudioSource>();
        }

        weaponEquipped = false;

        if (weaponButton != null)
        {
            weaponButton.interactable = false;
        }

        if (weaponButtonIcon != null)
        {
            weaponButtonIcon.sprite = null;
            weaponButtonIcon.enabled = true;
            weaponButtonIcon.gameObject.SetActive(false);
        }

        DisableWeaponButtonGlow();

        if (unequipButton != null)
        {
            unequipButton.SetWeaponEquipped(false);
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
    // PLAY WEAPON MOVE SOUND
    // ============================================================

    private void PlayWeaponMoveSound()
    {
        if (!playWeaponMoveSound)
            return;

        if (weaponMoveClip == null)
            return;

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

                foreach (Image image in images)
                {
                    if (image == null)
                        continue;

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
    // INVENTORY SLOT VISIBILITY
    // ============================================================

    private void RefreshInventorySlots()
    {
        int occupiedCount = 0;

        foreach (GameObject slot in runtimeSlots)
        {
            if (slot == null)
                continue;

            if (IsSlotOccupied(slot))
            {
                occupiedCount++;
            }
        }

        bool emptyPlaceholderShown = false;

        foreach (GameObject slot in runtimeSlots)
        {
            if (slot == null)
                continue;

            bool occupied =
                IsSlotOccupied(slot);

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

                emptyPlaceholderShown = true;
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
    // IS SLOT OCCUPIED
    // ============================================================

    private bool IsSlotOccupied(
        GameObject slot
    )
    {
        if (slot == null)
            return false;

        Transform item =
            FindItemTransform(
                slot
            );

        if (item == null)
            return false;

        if (!item.gameObject.activeSelf)
            return false;

        Image itemImage =
            item.GetComponent<Image>();

        if (itemImage == null)
            return false;

        return itemImage.sprite != null;
    }

    // ============================================================
    // SLOT VISIBILITY
    // ============================================================

    private void SetSlotVisible(
        GameObject slot,
        bool visible
    )
    {
        if (slot == null)
            return;

        if (visible)
        {
            if (!slot.activeSelf)
            {
                slot.SetActive(true);
            }
        }
        else
        {
            if (slot.activeSelf)
            {
                slot.SetActive(false);
            }
        }
    }

    // ============================================================
    // SLOT GRAPHIC ALPHA
    // ============================================================

    private void SetSlotGraphicAlpha(
        GameObject slot,
        float alpha
    )
    {
        if (slot == null)
            return;

        float safeAlpha =
            Mathf.Clamp01(alpha);

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

        foreach (Image image in images)
        {
            if (image == null)
                continue;

            if (itemTransform != null)
            {
                if (image.transform ==
                    itemTransform)
                {
                    continue;
                }

                if (image.transform.IsChildOf(
                        itemTransform))
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
                        glowTransform))
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
    // GET / CREATE FREE SLOT
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
            return;

        if (!slot.activeSelf)
        {
            slot.SetActive(true);
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
            slot.GetComponent<
                RectTransform
            >();

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
    // SHOW ITEM WITHOUT GLOW
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

            1.4f,
            0.08f,
            0.55f,
            0.85f
        );
    }

    // ============================================================
    // SHOW ITEM + GLOW
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
            itemTransform.GetComponent<Image>();

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
            itemTransform as RectTransform;

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
    // CONFIGURE WEAPON SLOT
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
        if (slot == null)
            return;

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
            unequipButton
        );

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Weapon slot configured: " +
                slot.name +
                " -> " +
                weaponId,
                this
            );
        }
    }

    // ============================================================
    // EQUIP WEAPON
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
            return false;

        Image itemImage =
            itemTransform.GetComponent<Image>();

        if (itemImage == null ||
            !itemTransform.gameObject.activeSelf)
        {
            return false;
        }

        RectTransform itemRect =
            itemTransform as RectTransform;

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

        Transform glowTransform =
            sourceSlot.transform.Find(
                "ItemGlow"
            );

        if (glowTransform != null)
        {
            Image glowImage =
                glowTransform.GetComponent<Image>();

            RectTransform glowRect =
                glowTransform as RectTransform;

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
        }

        currentWeaponSprite =
            weaponSprite;

        currentWeaponId =
            weaponId;

        currentWeaponButtonIconSize =
            buttonIconSize;

        currentWeaponButtonIconRotation =
            buttonIconRotation;

        currentWeaponButtonIconOffset =
            buttonIconOffset;

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

        weaponButtonIcon
            .gameObject
            .SetActive(
                true
            );

        weaponButtonIcon.enabled =
            true;

        weaponButtonIcon.sprite =
            currentWeaponSprite;

        weaponButtonIcon.color =
            new Color(
                1f,
                1f,
                1f,
                1f
            );

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
                currentInventoryIconRotation
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

        itemTransform
            .gameObject
            .SetActive(
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

        /*
         * ЗВУК:
         * ячейка -> большой круг.
         */
        PlayWeaponMoveSound();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Equipped weapon: " +
                currentWeaponId,
                this
            );
        }

        return true;
    }

    // ============================================================
    // UNEQUIP CURRENT WEAPON
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

                currentGlowPulseSpeed,
                currentGlowScaleAmount,
                currentGlowMinAlpha,
                currentGlowMaxAlpha
            );

        if (!placed)
            return false;

        ConfigureWeaponSlot(
            targetSlot,

            currentWeaponSprite,
            currentWeaponId,

            currentWeaponButtonIconSize,
            currentWeaponButtonIconRotation,
            currentWeaponButtonIconOffset
        );

        if (weaponButtonIcon != null)
        {
            weaponButtonIcon.sprite =
                null;

            weaponButtonIcon
                .gameObject
                .SetActive(
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

        /*
         * ЗВУК:
         * большой круг -> свободная ячейка.
         */
        PlayWeaponMoveSound();

        if (debugLogs)
        {
            Debug.Log(
                "[INVENTORY UI] Weapon returned to free slot: " +
                targetSlot.name,
                this
            );
        }

        ClearCurrentWeaponData();

        RefreshInventorySlots();

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

        currentWeaponButtonIconSize =
            Vector2.zero;

        currentWeaponButtonIconRotation =
            0f;

        currentWeaponButtonIconOffset =
            Vector2.zero;

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
    }

    // ============================================================
    // CREATE / UPDATE BIG WEAPON GLOW
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

            weaponGlowObject
                .transform
                .SetParent(
                    weaponButton.transform,
                    false
                );
        }

        RectTransform glowRect =
            weaponGlowObject
                .GetComponent<
                    RectTransform
                >();

        Image glowImage =
            weaponGlowObject
                .GetComponent<Image>();

        InventoryItemGlow glowPulse =
            weaponGlowObject
                .GetComponent<
                    InventoryItemGlow
                >();

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

        if (currentInventoryIconSize.x > 0.01f &&
            currentInventoryIconSize.y > 0.01f)
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

        Vector2 scaledGlowOffset =
            currentGlowOffset *
            sizeMultiplier *
            bigWeaponGlowScale;

        Vector2 rotatedGlowOffset =
            Quaternion.Euler(
                0f,
                0f,
                currentInventoryIconRotation
            ) * scaledGlowOffset;

        glowRect.anchoredPosition =
            currentWeaponButtonIconOffset +
            rotatedGlowOffset;

        glowRect.sizeDelta =
            currentGlowSize *
            sizeMultiplier *
            bigWeaponGlowScale;

        glowRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                currentInventoryIconRotation
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

        weaponGlowObject
            .SetActive(
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
    // CREATE / UPDATE INVENTORY GLOW
    // ============================================================

    private void CreateOrUpdateGlow(
        GameObject slot,
        Transform itemTransform,
        Sprite glowSprite,
        Color glowColor,
        Vector2 glowSize,
        Vector2 glowOffset,
        float rotationZ,
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

            glowObject
                .transform
                .SetParent(
                    slot.transform,
                    false
                );
        }

        RectTransform glowRect =
            glowObject
                .GetComponent<
                    RectTransform
                >();

        Image glowImage =
            glowObject
                .GetComponent<Image>();

        InventoryItemGlow glowPulse =
            glowObject
                .GetComponent<
                    InventoryItemGlow
                >();

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
            ) * glowOffset;

        glowRect.anchoredPosition =
            rotatedOffset;

        glowRect.sizeDelta =
            glowSize;

        glowRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                rotationZ
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
            itemTransform.GetSiblingIndex();

        glowObject
            .transform
            .SetSiblingIndex(
                Mathf.Max(
                    0,
                    itemIndex
                )
            );

        itemTransform
            .SetSiblingIndex(
                glowObject
                    .transform
                    .GetSiblingIndex() + 1
            );

        glowObject.SetActive(
            true
        );
    }

    // ============================================================
    // DISABLE INVENTORY GLOW
    // ============================================================

    private void DisableGlow(
        GameObject slot
    )
    {
        if (slot == null)
            return;

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
    // SLOT RECT
    // ============================================================

    public RectTransform GetSlotRect(
        GameObject slot
    )
    {
        if (slot == null)
            return null;

        return slot.GetComponent<
            RectTransform
        >();
    }

    // ============================================================
    // FIND FREE SLOT
    // ============================================================

    private GameObject FindFreeSlot()
    {
        foreach (GameObject slot in runtimeSlots)
        {
            if (slot == null)
                continue;

            if (!IsSlotOccupied(slot))
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
    // FIND ITEM CHILD
    // ============================================================

    private Transform FindItemTransform(
        GameObject slot
    )
    {
        if (slot == null)
            return null;

        Transform exact =
            slot.transform.Find(
                itemChildName
            );

        if (exact != null)
            return exact;

        foreach (Transform child
                 in slot.transform)
        {
            if (child == null)
                continue;

            if (child.name.StartsWith(
                    "Item_"))
            {
                return child;
            }
        }

        return null;
    }
}