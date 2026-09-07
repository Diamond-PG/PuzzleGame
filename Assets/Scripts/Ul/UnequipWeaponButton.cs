using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UnequipWeaponButton : MonoBehaviour
{
    // ============================================================
    // BUTTON
    // ============================================================

    [Header("BUTTON")]
    [SerializeField] private Button unequipButton;

    // ============================================================
    // INVENTORY
    // ============================================================

    [Header("INVENTORY")]
    [SerializeField] private InventoryUI inventoryUI;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]
    [SerializeField] private bool debugLogs = false;

    // ============================================================
    // STATE
    // ============================================================

    private bool weaponEquipped;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (unequipButton == null)
        {
            unequipButton =
                GetComponent<Button>();
        }

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(
                UseUnequip
            );

            unequipButton.onClick.AddListener(
                UseUnequip
            );

            // В начале оружие не экипировано,
            // поэтому кнопка тусклая и не нажимается.
            unequipButton.interactable = false;
        }
        else
        {
            Debug.LogError(
                "[UNEQUIP BUTTON] Button component not found!",
                this
            );
        }

        weaponEquipped = false;
    }

    // ============================================================
    // PUBLIC STATE
    // ============================================================

    /// <summary>
    /// Вызывается системой оружия каждый раз,
    /// когда оружие появляется в большом WeaponButton
    /// или возвращается обратно в инвентарь.
    /// </summary>
    public void SetWeaponEquipped(
        bool equipped
    )
    {
        weaponEquipped = equipped;

        if (unequipButton != null)
        {
            /*
             * Точно такой же принцип,
             * как у HandInteractionButton:
             *
             * false = Disabled Color
             *         кнопка тусклая
             *
             * true  = Normal Color
             *         кнопка яркая
             */
            unequipButton.interactable =
                weaponEquipped;
        }

        if (debugLogs)
        {
            Debug.Log(
                "[UNEQUIP BUTTON] Weapon equipped = " +
                weaponEquipped,
                this
            );
        }
    }

    // ============================================================
    // USE UNEQUIP
    // ============================================================

    private void UseUnequip()
    {
        if (!weaponEquipped)
            return;

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI == null)
        {
            Debug.LogError(
                "[UNEQUIP BUTTON] InventoryUI not found!",
                this
            );

            return;
        }

        if (debugLogs)
        {
            Debug.Log(
                "[UNEQUIP BUTTON] Unequip pressed.",
                this
            );
        }

        /*
         * Этот настоящий метод мы сейчас
         * добавим в InventoryUI.
         *
         * Никакого SendMessage больше нет.
         */
        bool success =
            inventoryUI.UnequipCurrentWeapon();

        /*
         * Если оружие успешно вернулось
         * из большого круга в инвентарь,
         * кнопка сразу снова становится тусклой.
         */
        if (success)
        {
            SetWeaponEquipped(false);
        }
    }

    // ============================================================
    // PUBLIC CHECK
    // ============================================================

    public bool IsWeaponEquipped()
    {
        return weaponEquipped;
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(
                UseUnequip
            );
        }
    }
}