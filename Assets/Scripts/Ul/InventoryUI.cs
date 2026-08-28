using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("SLOTS")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotTemplate;

    [Tooltip("Уже существующие слоты на сцене.")]
    [SerializeField] private List<GameObject> existingSlots =
        new List<GameObject>();

    [Header("ITEM CHILD")]
    [SerializeField] private string itemChildName = "Item";

    [Header("DEBUG")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<GameObject> runtimeSlots =
        new List<GameObject>();

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
    // GET / CREATE FREE SLOT
    // ============================================================

    public GameObject GetOrCreateFreeSlot()
    {
        GameObject freeSlot =
            FindFreeSlot();

        if (freeSlot != null)
        {
            PrepareSlotForUse(freeSlot);
            return freeSlot;
        }

        GameObject newSlot =
            CreateSlot();

        if (newSlot != null)
        {
            PrepareSlotForUse(newSlot);
        }

        return newSlot;
    }

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

        Canvas.ForceUpdateCanvases();

        RectTransform parentRect =
            slotsParent as RectTransform;

        if (parentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                parentRect
            );
        }

        RectTransform slotRect =
            slot.GetComponent<RectTransform>();

        if (slotRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                slotRect
            );
        }

        Canvas.ForceUpdateCanvases();
    }

    // ============================================================
    // OLD VERSION - WITHOUT GLOW
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
    // UNIVERSAL ITEM + GLOW
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

        if (!slot.activeSelf)
        {
            slot.SetActive(true);
        }

        Transform itemTransform =
            FindItemTransform(slot);

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

        // ========================================================
        // ITEM
        // ========================================================

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
                new Vector2(0.5f, 0.5f);

            itemRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            itemRect.pivot =
                new Vector2(0.5f, 0.5f);

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

        // ========================================================
        // GLOW
        // ========================================================

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
            DisableGlow(slot);
        }

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
    // CREATE / UPDATE GLOW
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
            slot.transform.Find("ItemGlow");

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
            glowObject.GetComponent<RectTransform>();

        Image glowImage =
            glowObject.GetComponent<Image>();

        InventoryItemGlow glowPulse =
            glowObject.GetComponent<InventoryItemGlow>();

        glowRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        glowRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        glowRect.pivot =
            new Vector2(0.5f, 0.5f);

        /*
         * ВАЖНО:
         * glowOffset считается в локальном
         * направлении самого оружия.
         *
         * То есть Y двигает glow вдоль меча,
         * даже если меч повернут по диагонали.
         */
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

        /*
         * X и Y glow можно
         * растягивать независимо.
         */
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

        glowObject.transform.SetSiblingIndex(
            Mathf.Max(
                0,
                itemIndex
            )
        );

        itemTransform.SetSiblingIndex(
            glowObject.transform
                .GetSiblingIndex() + 1
        );

        glowObject.SetActive(true);
    }

    // ============================================================
    // DISABLE GLOW
    // ============================================================

    private void DisableGlow(
        GameObject slot
    )
    {
        if (slot == null)
            return;

        Transform glow =
            slot.transform.Find("ItemGlow");

        if (glow != null)
        {
            glow.gameObject.SetActive(false);
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

        return slot.GetComponent<RectTransform>();
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

            Transform item =
                FindItemTransform(slot);

            if (item == null)
                continue;

            if (!item.gameObject.activeSelf)
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

        slot.SetActive(true);

        Transform item =
            FindItemTransform(slot);

        if (item != null)
        {
            item.gameObject.SetActive(false);
        }

        runtimeSlots.Add(slot);

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

        foreach (Transform child in slot.transform)
        {
            if (child == null)
                continue;

            if (child.name.StartsWith("Item_"))
            {
                return child;
            }
        }

        return null;
    }
}