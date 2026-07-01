using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotTemplate;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private void Awake()
    {
        if (slotsParent == null)
            slotsParent = transform;

        if (slotTemplate != null)
            slotTemplate.SetActive(false);

        if (debugLogs)
            Debug.Log("[INVENTORY UI] Ready.", this);
    }

    public GameObject CreateSlot()
    {
        if (slotTemplate == null)
        {
            Debug.LogError("[INVENTORY UI] SlotTemplate missing!", this);
            return null;
        }

        GameObject slot = Instantiate(slotTemplate, slotsParent);

        slot.name = "Slot";
        slot.SetActive(true);

        return slot;
    }

    public void AddItem(Sprite itemSprite)
    {
        GameObject slot = CreateSlot();

        if (slot == null)
            return;

        Image image = slot.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = itemSprite;
            image.color = Color.white;
        }

        if (debugLogs)
            Debug.Log("[INVENTORY UI] Item added.", this);
    }
}