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
    // WEAPON BUTTON ICON
    // ============================================================

    [Header("WEAPON BUTTON ICON")]

    [SerializeField]
    private Button weaponButton;

    [SerializeField]
    private Image weaponButtonIcon;

    [SerializeField]
    private Vector2 weaponButtonIconSize =
        new Vector2(
            190f,
            190f
        );

    [SerializeField]
    private float weaponButtonIconRotation = 0f;

    [SerializeField]
    private Vector2 weaponButtonIconOffset =
        Vector2.zero;

    [Tooltip(
        "Дополнительный Scale самого оружия " +
        "в большом WeaponButton."
    )]
    [SerializeField, Min(0.01f)]
    private float weaponButtonIconScale = 1f;


    // ============================================================
    // WEAPON BUTTON GLOW
    // ============================================================

    [Header("WEAPON BUTTON GLOW")]

    [Tooltip(
        "Общий индивидуальный Scale подсветки " +
        "этого оружия в большом WeaponButton."
    )]
    [SerializeField, Min(0.01f)]
    private float weaponButtonGlowScale = 1f;


    [Tooltip(
        "Отдельное изменение размера glow по X и Y. " +
        "X = ширина, Y = высота. " +
        "1 / 1 = без изменений."
    )]
    [SerializeField]
    private Vector2 weaponButtonGlowSizeScale =
        Vector2.one;


    [Tooltip(
        "Независимое X/Y смещение ТОЛЬКО glow " +
        "в большом WeaponButton."
    )]
    [SerializeField]
    private Vector2 weaponButtonGlowOffset =
        Vector2.zero;


    [Tooltip(
        "Дополнительный Z-поворот ТОЛЬКО glow " +
        "в большом WeaponButton."
    )]
    [SerializeField]
    private float weaponButtonGlowRotationOffset = 0f;


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


        weaponButtonIconScale =
            Mathf.Max(
                0.01f,
                weaponButtonIconScale
            );


        weaponButtonGlowScale =
            Mathf.Max(
                0.01f,
                weaponButtonGlowScale
            );


        ClampGlowSizeScale();


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
        {
            return;
        }


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
        {
            return;
        }


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


        foreach (Image image
                 in images)
        {
            if (image == null)
            {
                continue;
            }


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

    /*
     * Старую сигнатуру оставляем,
     * чтобы существующие pickup-скрипты,
     * включая SwordPickup,
     * продолжили работать.
     */
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

            1f,

            1f,
            Vector2.one,
            Vector2.zero,
            0f
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

            1f,

            1f,
            Vector2.one,
            Vector2.zero,
            0f
        );
    }


    // ============================================================
    // SETUP WITH DURABILITY + OLD BIG GLOW SCALE
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

            1f,

            newWeaponButtonGlowScale,
            Vector2.one,
            Vector2.zero,
            0f
        );
    }


    // ============================================================
    // SETUP WITH FULL VISUAL DATA
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

        float newWeaponButtonIconScale,

        float newWeaponButtonGlowScale,
        Vector2 newWeaponButtonGlowSizeScale,
        Vector2 newWeaponButtonGlowOffset,
        float newWeaponButtonGlowRotationOffset
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


        weaponButtonIconScale =
            Mathf.Max(
                0.01f,
                newWeaponButtonIconScale
            );


        weaponButtonGlowScale =
            Mathf.Max(
                0.01f,
                newWeaponButtonGlowScale
            );


        weaponButtonGlowSizeScale =
            new Vector2(
                Mathf.Max(
                    0.01f,
                    newWeaponButtonGlowSizeScale.x
                ),
                Mathf.Max(
                    0.01f,
                    newWeaponButtonGlowSizeScale.y
                )
            );


        weaponButtonGlowOffset =
            newWeaponButtonGlowOffset;


        weaponButtonGlowRotationOffset =
            newWeaponButtonGlowRotationOffset;


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
                    weaponDurability01 *
                    100f
                ) +
                "% | Icon Scale = " +
                weaponButtonIconScale +
                " | Big Glow Scale = " +
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


        /*
         * Передаём вместе с оружием:
         *
         * - прочность этого экземпляра;
         * - размер/угол/X/Y самого оружия;
         * - Scale самого оружия;
         * - Scale glow;
         * - X/Y размер glow;
         * - X/Y позицию glow;
         * - Z glow.
         */
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

                weaponButtonIconScale,

                weaponButtonGlowScale,
                weaponButtonGlowSizeScale,
                weaponButtonGlowOffset,
                weaponButtonGlowRotationOffset
            );


        if (!success)
        {
            return;
        }


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
                    weaponDurability01 *
                    100f
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


        weaponButtonIconScale =
            1f;


        weaponButtonGlowScale =
            1f;


        weaponButtonGlowSizeScale =
            Vector2.one;


        weaponButtonGlowOffset =
            Vector2.zero;


        weaponButtonGlowRotationOffset =
            0f;


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
    // CLAMP GLOW SIZE SCALE
    // ============================================================

    private void ClampGlowSizeScale()
    {
        weaponButtonGlowSizeScale.x =
            Mathf.Max(
                0.01f,
                weaponButtonGlowSizeScale.x
            );


        weaponButtonGlowSizeScale.y =
            Mathf.Max(
                0.01f,
                weaponButtonGlowSizeScale.y
            );
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


        weaponButtonIconScale =
            Mathf.Max(
                0.01f,
                weaponButtonIconScale
            );


        weaponButtonGlowScale =
            Mathf.Max(
                0.01f,
                weaponButtonGlowScale
            );


        ClampGlowSizeScale();
    }
}