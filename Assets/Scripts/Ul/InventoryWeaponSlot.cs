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
    // DURABILITY
    // ============================================================

    [Header("WEAPON DURABILITY")]

    [Tooltip(
        "Прочность именно ЭТОГО экземпляра оружия. " +
        "1 = 100%, 0 = сломано."
    )]
    [SerializeField, Range(0f, 1f)]
    private float weaponDurability01 = 1f;


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

    [Tooltip(
        "Индивидуальный масштаб подсветки оружия " +
        "внутри большого WeaponButton. " +
        "1 = обычный размер."
    )]
    [SerializeField, Min(0.1f)]
    private float weaponButtonGlowScale = 1f;


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
    // STATE
    // ============================================================

    private bool weaponConfigured;


    // ============================================================
    // PUBLIC
    // ============================================================

    public string WeaponId =>
        weaponId;

    public float WeaponDurability01 =>
        Mathf.Clamp01(
            weaponDurability01
        );

    public bool IsConfiguredAsWeapon =>
        weaponConfigured;


    public Sprite WeaponIcon
    {
        get
        {
            if (weaponIcon != null)
            {
                return weaponIcon;
            }

            if (slotItemImage != null)
            {
                return slotItemImage.sprite;
            }

            return null;
        }
    }


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        RefreshReferences();

        weaponDurability01 =
            Mathf.Clamp01(
                weaponDurability01
            );

        weaponButtonGlowScale =
            Mathf.Max(
                0.1f,
                weaponButtonGlowScale
            );

        weaponConfigured =
            weaponIcon != null &&
            !string.IsNullOrEmpty(
                weaponId
            );

        if (weaponConfigured)
        {
            ConnectButton();
        }
        else
        {
            DisconnectButton();
        }
    }


    // ============================================================
    // REFERENCES
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
    // BUTTON
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


    private void DisconnectButton()
    {
        if (slotButton == null)
            return;

        slotButton.onClick.RemoveListener(
            EquipWeapon
        );
    }


    // ============================================================
    // FIND ITEM
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

            if (image.transform ==
                transform)
            {
                continue;
            }

            if (image.name.StartsWith(
                    "Item"))
            {
                slotItemImage =
                    image;

                return;
            }
        }
    }


    // ============================================================
    // OLD SETUP - DEFAULT 100%
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
        SetupWeapon(
            newWeaponIcon,
            newWeaponId,
            newWeaponButton,
            newWeaponButtonIcon,
            newButtonIconSize,
            newButtonIconRotation,
            newButtonIconOffset,
            newInventoryUI,
            newUnequipButton,
            1f,
            1f
        );
    }


    // ============================================================
    // SETUP WITH DURABILITY
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
        UnequipWeaponButton newUnequipButton,
        float newWeaponDurability01
    )
    {
        SetupWeapon(
            newWeaponIcon,
            newWeaponId,
            newWeaponButton,
            newWeaponButtonIcon,
            newButtonIconSize,
            newButtonIconRotation,
            newButtonIconOffset,
            newInventoryUI,
            newUnequipButton,
            newWeaponDurability01,
            1f
        );
    }


    // ============================================================
    // SETUP WITH DURABILITY + BIG GLOW SCALE
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
        UnequipWeaponButton newUnequipButton,
        float newWeaponDurability01,
        float newWeaponButtonGlowScale
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

        weaponButtonGlowScale =
            Mathf.Max(
                0.1f,
                newWeaponButtonGlowScale
            );

        inventoryUI =
            newInventoryUI;

        unequipButton =
            newUnequipButton;

        weaponDurability01 =
            Mathf.Clamp01(
                newWeaponDurability01
            );

        slotButton =
            GetComponent<Button>();

        FindSlotItemImage();

        weaponConfigured =
            true;

        ConnectButton();

        if (debugLogs)
        {
            Debug.Log(
                "[WEAPON SLOT] Configured: " +
                weaponId +
                " | Durability = " +
                Mathf.RoundToInt(
                    weaponDurability01 * 100f
                ) +
                "% | Big Glow Scale = " +
                weaponButtonGlowScale,
                this
            );
        }
    }


    // ============================================================
    // SET DURABILITY
    // ============================================================

    public void SetWeaponDurability01(
        float value
    )
    {
        weaponDurability01 =
            Mathf.Clamp01(
                value
            );
    }


    // ============================================================
    // EQUIP
    // ============================================================

    public void EquipWeapon()
    {
        if (!weaponConfigured)
        {
            return;
        }

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
                weaponButtonIconOffset,
                weaponDurability01,
                weaponButtonGlowScale
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
                weaponId +
                " | Durability = " +
                Mathf.RoundToInt(
                    weaponDurability01 * 100f
                ) +
                "%",
                this
            );
        }
    }


    // ============================================================
    // CLEAR
    // ============================================================

    public void ClearWeaponData()
    {
        weaponConfigured =
            false;

        weaponIcon =
            null;

        weaponId =
            "";

        weaponDurability01 =
            1f;

        weaponButtonGlowScale =
            1f;

        RefreshReferences();

        DisconnectButton();

        if (debugLogs)
        {
            Debug.Log(
                "[WEAPON SLOT] Weapon data cleared.",
                this
            );
        }
    }


    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        DisconnectButton();
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        weaponDurability01 =
            Mathf.Clamp01(
                weaponDurability01
            );

        weaponButtonGlowScale =
            Mathf.Max(
                0.1f,
                weaponButtonGlowScale
            );
    }
}