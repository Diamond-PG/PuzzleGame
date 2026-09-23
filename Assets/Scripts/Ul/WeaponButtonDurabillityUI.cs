using UnityEngine;
using UnityEngine.UI;

public class WeaponButtonDurabilityUI : MonoBehaviour
{
    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("REFERENCES")]

    [SerializeField]
    private InventoryUI inventoryUI;

    [SerializeField]
    private Image frameImage;

    [SerializeField]
    private Image durabilityOverlayTop;

    [SerializeField]
    private Image weaponIcon;

    [SerializeField]
    private Image weaponDurabilityFill;

    [SerializeField]
    private Image armorDurabilityFill;

    [SerializeField]
    private Button weaponButton;


    // ============================================================
    // AUTOMATIC WEAPON STATE
    // ============================================================

    [Header("AUTOMATIC WEAPON STATE")]

    [SerializeField]
    private bool readWeaponStateFromInventory = true;


    // ============================================================
    // FRAME VISIBILITY
    // ============================================================

    [Header("FRAME VISIBILITY")]

    [SerializeField, Range(0f, 1f)]
    private float emptyFrameAlpha = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float activeFrameAlpha = 1f;


    // ============================================================
    // DURABILITY VISIBILITY
    // ============================================================

    [Header("DURABILITY VISIBILITY")]

    [SerializeField]
    private bool hideWeaponDurabilityWhenEmpty = true;

    [SerializeField]
    private bool hideArmorDurabilityWhenEmpty = true;

    [SerializeField]
    private bool hideWeaponIconWhenNoWeapon = true;

    [SerializeField]
    private bool disableButtonWhenNoWeapon = true;


    // ============================================================
    // STATE
    // ============================================================

    [Header("STATE / TEST IN INSPECTOR")]

    [SerializeField]
    private bool hasWeapon = false;

    [SerializeField]
    private bool hasArmor = false;


    // ============================================================
    // DURABILITY
    // ============================================================

    [Header("DURABILITY IN INSPECTOR")]

    [SerializeField, Range(0f, 1f)]
    private float weaponDurability01 = 1f;

    [SerializeField, Range(0f, 1f)]
    private float armorDurability01 = 1f;


    // ============================================================
    // WEAPON DURABILITY SETTINGS
    // ============================================================

    [Header("WEAPON DURABILITY SETTINGS")]

    [Tooltip(
        "Сколько успешных попаданий выдерживает меч."
    )]
    [SerializeField, Min(1)]
    private int weaponMaxSuccessfulHits = 20;

    [SerializeField]
    private bool useWeaponDurability = true;


    // ============================================================
    // CONSTANTS
    // ============================================================

    private const float HALF_RING_MAX_FILL = 0.5f;
    private const float EPSILON = 0.0001f;


    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        CacheReferences();

        SyncWeaponStateFromInventory(
            true
        );

        RefreshUI();
    }


    private void OnEnable()
    {
        CacheReferences();

        SyncWeaponStateFromInventory(
            true
        );

        RefreshUI();
    }


    private void LateUpdate()
    {
        SyncWeaponStateFromInventory(
            false
        );
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        CacheReferences();

        weaponDurability01 =
            Mathf.Clamp01(
                weaponDurability01
            );

        armorDurability01 =
            Mathf.Clamp01(
                armorDurability01
            );

        emptyFrameAlpha =
            Mathf.Clamp01(
                emptyFrameAlpha
            );

        activeFrameAlpha =
            Mathf.Clamp01(
                activeFrameAlpha
            );

        weaponMaxSuccessfulHits =
            Mathf.Max(
                1,
                weaponMaxSuccessfulHits
            );

        /*
         * Благодаря этому даже если во время Play Mode
         * вручную двинуть прочность в Inspector,
         * текущее оружие запомнит это значение.
         */
        PushCurrentDurabilityToInventory();

        RefreshUI();
    }

#endif


    // ============================================================
    // REFERENCES
    // ============================================================

    private void CacheReferences()
    {
        if (frameImage == null)
        {
            frameImage =
                GetComponent<Image>();
        }

        if (weaponButton == null)
        {
            weaponButton =
                GetComponent<Button>();
        }

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }
    }


    // ============================================================
    // INVENTORY SYNC
    // ============================================================

    private void SyncWeaponStateFromInventory(
        bool forceRefresh
    )
    {
        if (!readWeaponStateFromInventory)
            return;

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI == null)
            return;

        bool previousState =
            hasWeapon;

        bool inventoryState =
            inventoryUI.IsWeaponEquipped;

        /*
         * Новый экземпляр оружия только что
         * попал в большой круг.
         *
         * Загружаем ЕГО собственную прочность.
         */
        if (inventoryState &&
            (!previousState ||
             forceRefresh))
        {
            weaponDurability01 =
                inventoryUI
                    .GetEquippedWeaponDurability01();
        }

        hasWeapon =
            inventoryState;

        /*
         * Пока оружие экипировано,
         * значение UI считается текущей
         * рабочей прочностью этого экземпляра.
         *
         * Синхронизируем её обратно
         * в InventoryUI.
         */
        if (hasWeapon)
        {
            inventoryUI
                .SetEquippedWeaponDurability01(
                    weaponDurability01
                );
        }

        if (forceRefresh ||
            previousState != hasWeapon)
        {
            RefreshUI();
        }
    }


    private void PushCurrentDurabilityToInventory()
    {
        if (inventoryUI == null)
            return;

        if (!inventoryUI.IsWeaponEquipped)
            return;

        inventoryUI
            .SetEquippedWeaponDurability01(
                weaponDurability01
            );
    }


    // ============================================================
    // WEAPON STATE
    // ============================================================

    public void SetHasWeapon(
        bool value
    )
    {
        hasWeapon =
            value;

        RefreshUI();
    }


    public bool HasWeapon()
    {
        return hasWeapon;
    }


    // ============================================================
    // WEAPON DURABILITY
    // ============================================================

    public void SetWeaponDurability01(
        float value
    )
    {
        weaponDurability01 =
            Mathf.Clamp01(
                value
            );

        PushCurrentDurabilityToInventory();

        RefreshUI();
    }


    public float GetWeaponDurability01()
    {
        return weaponDurability01;
    }


    public int GetWeaponMaxSuccessfulHits()
    {
        return weaponMaxSuccessfulHits;
    }


    public int GetWeaponApproximateHitsRemaining()
    {
        return
            Mathf.CeilToInt(
                weaponDurability01 *
                weaponMaxSuccessfulHits
            );
    }


    // ============================================================
    // CONSUME DURABILITY
    // ============================================================

    public bool ConsumeWeaponDurabilityHit()
    {
        if (!useWeaponDurability)
            return false;

        if (!hasWeapon)
            return false;

        if (weaponDurability01 <=
            EPSILON)
        {
            weaponDurability01 =
                0f;

            PushCurrentDurabilityToInventory();

            RefreshUI();

            return true;
        }

        float loss =
            1f /
            Mathf.Max(
                1,
                weaponMaxSuccessfulHits
            );

        weaponDurability01 =
            Mathf.Clamp01(
                weaponDurability01 -
                loss
            );

        if (weaponDurability01 <=
            EPSILON)
        {
            weaponDurability01 =
                0f;
        }

        /*
         * Сразу записываем остаток
         * именно в текущий экземпляр оружия.
         */
        PushCurrentDurabilityToInventory();

        RefreshUI();

        return
            weaponDurability01 <=
            EPSILON;
    }


    // ============================================================
    // BREAK WEAPON NOW
    // ============================================================

    public void BreakWeaponNow()
    {
        weaponDurability01 =
            0f;

        PushCurrentDurabilityToInventory();

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI != null)
        {
            inventoryUI.BreakEquippedWeapon();
        }

        hasWeapon =
            false;

        RefreshUI();
    }


    // ============================================================
    // MANUAL RESET
    // ============================================================

    public void ResetWeaponDurability()
    {
        weaponDurability01 =
            1f;

        PushCurrentDurabilityToInventory();

        RefreshUI();
    }


    // ============================================================
    // ARMOR
    // ============================================================

    public void SetHasArmor(
        bool value
    )
    {
        hasArmor =
            value;

        RefreshUI();
    }


    public bool HasArmor()
    {
        return hasArmor;
    }


    public void SetArmorDurability01(
        float value
    )
    {
        armorDurability01 =
            Mathf.Clamp01(
                value
            );

        RefreshUI();
    }


    public float GetArmorDurability01()
    {
        return armorDurability01;
    }


    // ============================================================
    // REFRESH
    // ============================================================

    public void RefreshUI()
    {
        RefreshFrame();
        RefreshWeapon();
        RefreshArmor();
        RefreshButton();
    }


    // ============================================================
    // FRAME
    // ============================================================

    private void RefreshFrame()
    {
        bool anythingEquipped =
            hasWeapon ||
            hasArmor;

        float targetAlpha =
            anythingEquipped
                ? activeFrameAlpha
                : emptyFrameAlpha;

        SetImageAlpha(
            frameImage,
            targetAlpha
        );

        SetImageAlpha(
            durabilityOverlayTop,
            targetAlpha
        );
    }


    // ============================================================
    // WEAPON UI
    // ============================================================

    private void RefreshWeapon()
    {
        bool showDurability =
            hasWeapon &&
            weaponDurability01 >
            EPSILON;

        if (!hideWeaponDurabilityWhenEmpty)
        {
            showDurability =
                hasWeapon;
        }

        if (weaponDurabilityFill != null)
        {
            weaponDurabilityFill.type =
                Image.Type.Filled;

            weaponDurabilityFill.fillAmount =
                weaponDurability01 *
                HALF_RING_MAX_FILL;

            weaponDurabilityFill.enabled =
                showDurability;
        }

        if (weaponIcon != null)
        {
            bool showIcon =
                hasWeapon;

            if (hideWeaponIconWhenNoWeapon &&
                !hasWeapon)
            {
                showIcon =
                    false;
            }

            if (weaponIcon.gameObject.activeSelf)
            {
                weaponIcon.enabled =
                    showIcon;
            }
        }
    }


    // ============================================================
    // ARMOR UI
    // ============================================================

    private void RefreshArmor()
    {
        bool showDurability =
            hasArmor &&
            armorDurability01 >
            EPSILON;

        if (!hideArmorDurabilityWhenEmpty)
        {
            showDurability =
                hasArmor;
        }

        if (armorDurabilityFill != null)
        {
            armorDurabilityFill.type =
                Image.Type.Filled;

            armorDurabilityFill.fillAmount =
                armorDurability01 *
                HALF_RING_MAX_FILL;

            armorDurabilityFill.enabled =
                showDurability;
        }
    }


    // ============================================================
    // BUTTON
    // ============================================================

    private void RefreshButton()
    {
        if (weaponButton == null)
            return;

        if (disableButtonWhenNoWeapon)
        {
            weaponButton.interactable =
                hasWeapon;
        }
    }


    // ============================================================
    // IMAGE ALPHA
    // ============================================================

    private void SetImageAlpha(
        Image image,
        float alpha
    )
    {
        if (image == null)
            return;

        image.enabled =
            true;

        Color color =
            image.color;

        color.a =
            Mathf.Clamp01(
                alpha
            );

        image.color =
            color;
    }
}