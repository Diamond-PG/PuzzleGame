using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InventoryWeaponSlot : MonoBehaviour
{
    // ============================================================
    // WEAPON
    // ============================================================

    [Header("WEAPON")]

    [SerializeField]
    private Sprite weaponIcon;

    [SerializeField]
    private string weaponId = "Sword";

    // ============================================================
    // SLOT
    // ============================================================

    [Header("SLOT")]

    [SerializeField]
    private Image slotItemImage;

    // ============================================================
    // WEAPON BUTTON
    // ============================================================

    [Header("WEAPON BUTTON")]

    [SerializeField]
    private Button weaponButton;

    [SerializeField]
    private Image weaponButtonIcon;

    [SerializeField]
    private Vector2 weaponButtonIconSize =
        new Vector2(190f, 190f);

    [SerializeField]
    private float weaponButtonIconRotation = 0f;

    [SerializeField]
    private Vector2 weaponButtonIconOffset =
        Vector2.zero;

    // ============================================================
    // SYSTEM
    // ============================================================

    [Header("SYSTEM")]

    [SerializeField]
    private InventoryUI inventoryUI;

    [SerializeField]
    private UnequipWeaponButton unequipButton;

    // ============================================================
    // SLOT BUTTON
    // ============================================================

    [Header("SLOT BUTTON")]

    [SerializeField]
    private Button slotButton;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = true;

    // ============================================================
    // PUBLIC
    // ============================================================

    public string WeaponId => weaponId;

    public Sprite WeaponIcon
    {
        get
        {
            if (weaponIcon != null)
                return weaponIcon;

            if (slotItemImage != null)
                return slotItemImage.sprite;

            return null;
        }
    }

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        RefreshReferences();
        ConnectButton();
    }

    // ============================================================
    // REFRESH REFERENCES
    // ============================================================

    private void RefreshReferences()
    {
        if (slotButton == null)
        {
            slotButton =
                GetComponent<Button>();
        }

        if (slotItemImage == null)
        {
            FindSlotItemImage();
        }

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (unequipButton == null)
        {
            unequipButton =
                FindFirstObjectByType<UnequipWeaponButton>();
        }
    }

    // ============================================================
    // CONNECT BUTTON
    // ============================================================

    private void ConnectButton()
    {
        if (slotButton == null)
            return;

        slotButton.onClick.RemoveListener(
            EquipWeapon
        );

        slotButton.onClick.AddListener(
            EquipWeapon
        );
    }

    // ============================================================
    // FIND SLOT ITEM
    // ============================================================

    private void FindSlotItemImage()
    {
        Image[] images =
            GetComponentsInChildren<Image>(
                true
            );

        foreach (Image image in images)
        {
            if (image == null)
                continue;

            if (image.transform == transform)
                continue;

            if (image.name.StartsWith("Item"))
            {
                slotItemImage = image;
                return;
            }
        }
    }

    // ============================================================
    // SETUP WEAPON
    // ============================================================

    public void SetupWeapon(
        Sprite newWeaponIcon,
        string newWeaponId,
        Button newWeaponButton,
        Image newWeaponButtonIcon,
        Vector2 newButtonIconSize,
        float newButtonIconRotation,
        Vector2 newButtonIconOffset,
        InventoryUI newInventoryUI,
        UnequipWeaponButton newUnequipButton
    )
    {
        weaponIcon =
            newWeaponIcon;

        weaponId =
            newWeaponId;

        weaponButton =
            newWeaponButton;

        weaponButtonIcon =
            newWeaponButtonIcon;

        weaponButtonIconSize =
            newButtonIconSize;

        weaponButtonIconRotation =
            newButtonIconRotation;

        weaponButtonIconOffset =
            newButtonIconOffset;

        inventoryUI =
            newInventoryUI;

        unequipButton =
            newUnequipButton;

        slotButton =
            GetComponent<Button>();

        FindSlotItemImage();
        ConnectButton();

        if (debugLogs)
        {
            Debug.Log(
                "[WEAPON SLOT] Configured as: " +
                weaponId,
                this
            );
        }
    }

    // ============================================================
    // EQUIP
    // ============================================================

    public void EquipWeapon()
    {
        RefreshReferences();

        if (inventoryUI == null)
        {
            Debug.LogError(
                "[WEAPON SLOT] InventoryUI not found!",
                this
            );

            return;
        }

        if (slotItemImage == null)
        {
            FindSlotItemImage();
        }

        if (slotItemImage == null ||
            !slotItemImage.gameObject.activeSelf ||
            slotItemImage.sprite == null)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[WEAPON SLOT] Slot is empty.",
                    this
                );
            }

            return;
        }

        Sprite sprite =
            WeaponIcon;

        if (sprite == null)
        {
            Debug.LogWarning(
                "[WEAPON SLOT] Weapon sprite missing.",
                this
            );

            return;
        }

        if (weaponButton == null ||
            weaponButtonIcon == null)
        {
            Debug.LogWarning(
                "[WEAPON SLOT] WeaponButton references missing.",
                this
            );

            return;
        }

        bool success =
            inventoryUI.EquipWeaponFromSlot(
                gameObject,
                sprite,
                weaponId,
                weaponButton,
                weaponButtonIcon,
                weaponButtonIconSize,
                weaponButtonIconRotation,
                weaponButtonIconOffset
            );

        if (!success)
            return;

        if (unequipButton == null)
        {
            unequipButton =
                FindFirstObjectByType<UnequipWeaponButton>();
        }

        if (unequipButton != null)
        {
            unequipButton.SetWeaponEquipped(
                true
            );
        }

        if (debugLogs)
        {
            Debug.Log(
                "[WEAPON SLOT] Equipped: " +
                weaponId,
                this
            );
        }
    }

    // ============================================================
    // CLEAR WEAPON DATA
    // ============================================================

    public void ClearWeaponData()
    {
        weaponIcon = null;
        weaponId = "";

        if (debugLogs)
        {
            Debug.Log(
                "[WEAPON SLOT] Weapon data cleared.",
                this
            );
        }
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(
                EquipWeapon
            );
        }
    }
}