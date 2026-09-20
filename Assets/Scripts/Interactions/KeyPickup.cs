using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyPickup : MonoBehaviour, IHandInteractable
{
    // ============================================================
    // INVENTORY
    // ============================================================

    [Header("INVENTORY")]

    [SerializeField]
    private InventoryUI inventoryUI;

    [Tooltip(
        "Старый Item_Key используется только как источник " +
        "спрайта и размера ключа."
    )]
    [SerializeField]
    private GameObject keyIconUI;

    [Tooltip(
        "DoorUnlock, который должен срабатывать " +
        "при нажатии на ключ в инвентаре."
    )]
    [SerializeField]
    private DoorUnlock doorUnlock;


    // ============================================================
    // KEY ICON IN SLOT
    // ============================================================

    [Header("KEY ICON IN SLOT")]

    [Tooltip(
        "Смещение ключа внутри любой динамической ячейки. " +
        "0 / 0 = строго по центру."
    )]
    [SerializeField]
    private Vector2 inventoryKeyIconOffset =
        Vector2.zero;

    [Tooltip(
        "Дополнительный поворот ключа внутри ячейки."
    )]
    [SerializeField]
    private float inventoryKeyIconRotationOffset =
        0f;

    [Tooltip(
        "Дополнительный масштаб ключа внутри ячейки."
    )]
    [SerializeField, Min(0.01f)]
    private float inventoryKeyIconScaleMultiplier =
        1f;


    // ============================================================
    // INVENTORY GLOW
    // ============================================================

    [Header("INVENTORY GLOW")]

    [SerializeField]
    private Sprite inventoryGlowSprite;

    [SerializeField]
    private Color inventoryGlowColor =
        new Color(
            1.00f,
            0.75f,
            0.15f,
            0.85f
        );

    [SerializeField]
    private Vector2 inventoryGlowSize =
        new Vector2(
            175f,
            190f
        );

    [SerializeField]
    private Vector2 inventoryGlowOffset =
        new Vector2(
            -10f,
            -12f
        );

    [SerializeField]
    private float inventoryGlowRotationOffset =
        0f;

    [SerializeField]
    private float inventoryGlowPulseSpeed =
        1.4f;

    [SerializeField]
    private float inventoryGlowScaleAmount =
        0.12f;

    [SerializeField, Range(0f, 1f)]
    private float inventoryGlowMinAlpha =
        0.50f;

    [SerializeField, Range(0f, 1f)]
    private float inventoryGlowMaxAlpha =
        0.85f;


    // ============================================================
    // OPTIONAL VISUALS
    // ============================================================

    [Header("OPTIONAL VISUALS")]

    [SerializeField]
    private GameObject keyGlowObject;

    [SerializeField]
    private MonoBehaviour keyPulseScript;


    // ============================================================
    // PLAYER
    // ============================================================

    [Header("PLAYER")]

    [SerializeField]
    private string playerTag = "Player";

    [SerializeField]
    private float pickupDistance = 1.2f;


    // ============================================================
    // SOUND
    // ============================================================

    [Header("PICKUP SOUND")]

    [SerializeField]
    private AudioSource pickupAudioSource;

    [SerializeField]
    private bool playPickupSound = true;


    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("PICKUP HAPTICS")]

    [SerializeField]
    private bool usePickupHaptics = true;


    // ============================================================
    // FLY
    // ============================================================

    [Header("FLY TO UI ANIMATION")]

    [SerializeField]
    private bool animateToUI = true;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = true;


    // ============================================================
    // PUBLIC KEY STATE
    // ============================================================

    public static bool HasKey
    {
        get;
        private set;
    }

    private static KeyPickup currentInstance;


    // ============================================================
    // PRIVATE
    // ============================================================

    private bool pickedUp;

    private Transform playerTransform;
    private Collider2D keyCollider;

    private SpriteRenderer[] spriteRenderers;

    private PickupFlyEffect flyEffect;


    // ============================================================
    // CACHED KEY VISUAL
    // ============================================================

    private Sprite cachedKeySprite;

    private Vector2 cachedKeyIconSize =
        new Vector2(
            100f,
            100f
        );

    private float cachedKeyIconRotation;

    private float cachedKeyIconScale =
        1f;

    private bool keyVisualCached;


    // ============================================================
    // CURRENT INVENTORY SLOT
    // ============================================================

    private GameObject currentInventorySlot;

    private Transform currentInventoryItemTransform;

    private Image currentInventoryItemImage;

    private GameObject currentInventoryGlowObject;

    private Button currentInventorySlotButton;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        currentInstance =
            this;

        keyCollider =
            GetComponent<Collider2D>();

        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>();

        flyEffect =
            GetComponent<PickupFlyEffect>();

        if (pickupAudioSource == null)
        {
            pickupAudioSource =
                GetComponent<AudioSource>();
        }

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (doorUnlock == null)
        {
            doorUnlock =
                FindFirstObjectByType<DoorUnlock>();
        }

        CacheKeyInventoryVisual();

        /*
         * Старый Item_Key больше не является
         * настоящей активной иконкой инвентаря.
         */
        if (keyIconUI != null)
        {
            keyIconUI.SetActive(
                false
            );
        }

        FindPlayer();

        HasKey =
            false;
    }


    // ============================================================
    // CACHE KEY VISUAL
    // ============================================================

    private void CacheKeyInventoryVisual()
    {
        keyVisualCached =
            false;

        if (keyIconUI == null)
        {
            Debug.LogError(
                "[KEY] KeyIconUI is missing.",
                this
            );

            return;
        }

        Image keyImage =
            keyIconUI.GetComponent<Image>();

        RectTransform keyRect =
            keyIconUI.GetComponent<RectTransform>();

        if (keyImage == null ||
            keyImage.sprite == null)
        {
            Debug.LogError(
                "[KEY] KeyIconUI has no key sprite.",
                keyIconUI
            );

            return;
        }

        cachedKeySprite =
            keyImage.sprite;

        if (keyRect != null)
        {
            /*
             * Размер старого ключа сохраняем,
             * но его СТАРУЮ ПОЗИЦИЮ
             * специально не используем.
             */
            cachedKeyIconSize =
                keyRect.sizeDelta;

            cachedKeyIconRotation =
                keyRect.localEulerAngles.z;

            cachedKeyIconScale =
                Mathf.Max(
                    0.01f,
                    keyRect.localScale.x
                );
        }

        keyVisualCached =
            true;
    }


    // ============================================================
    // FIND PLAYER
    // ============================================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                playerTag
            );

        if (playerObject != null)
        {
            playerTransform =
                playerObject.transform;
        }
    }


    // ============================================================
    // HAND INTERACTION
    // ============================================================

    public bool CanHandInteract
    {
        get
        {
            if (pickedUp)
            {
                return false;
            }

            if (keyCollider == null ||
                !keyCollider.enabled)
            {
                return false;
            }

            if (playerTransform == null)
            {
                FindPlayer();
            }

            if (playerTransform == null)
            {
                return false;
            }

            float distance =
                Vector2.Distance(
                    playerTransform.position,
                    transform.position
                );

            return
                distance <=
                pickupDistance;
        }
    }


    public void HandInteract()
    {
        if (!CanHandInteract)
        {
            return;
        }

        PickupKey();
    }


    // ============================================================
    // PICKUP KEY
    // ============================================================

    private void PickupKey()
    {
        if (pickedUp)
        {
            return;
        }

        if (!PrepareInventorySlot())
        {
            return;
        }

        pickedUp =
            true;

        HasKey =
            true;

        if (keyCollider != null)
        {
            keyCollider.enabled =
                false;
        }

        if (usePickupHaptics)
        {
            MicroHaptics.TinyClick();
        }

        if (playPickupSound &&
            pickupAudioSource != null &&
            pickupAudioSource.clip != null)
        {
            pickupAudioSource.PlayOneShot(
                pickupAudioSource.clip
            );
        }

        if (keyGlowObject != null)
        {
            keyGlowObject.SetActive(
                false
            );
        }

        if (keyPulseScript != null)
        {
            keyPulseScript.enabled =
                false;
        }

        if (animateToUI &&
            flyEffect != null &&
            currentInventoryItemTransform != null)
        {
            StartCoroutine(
                flyEffect.FlyToUI(
                    currentInventoryItemTransform.gameObject,
                    FinishPickupInstant
                )
            );
        }
        else
        {
            FinishPickupInstant();
        }
    }


    // ============================================================
    // PREPARE INVENTORY SLOT
    // ============================================================

    private bool PrepareInventorySlot()
    {
        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI == null)
        {
            Debug.LogError(
                "[KEY] InventoryUI not found.",
                this
            );

            return false;
        }

        if (!keyVisualCached)
        {
            CacheKeyInventoryVisual();
        }

        if (!keyVisualCached ||
            cachedKeySprite == null)
        {
            return false;
        }


        // --------------------------------------------------------
        // FIRST FREE SLOT
        // --------------------------------------------------------

        currentInventorySlot =
            inventoryUI.GetOrCreateFreeSlot();

        if (currentInventorySlot == null)
        {
            return false;
        }


        // --------------------------------------------------------
        // REMOVE OLD WEAPON BEHAVIOUR
        // --------------------------------------------------------

        /*
         * Ячейка могла секунду назад
         * содержать меч.
         *
         * Теперь она должна стать
         * полностью нейтральной.
         */
        InventoryWeaponSlot oldWeaponSlot =
            currentInventorySlot
                .GetComponent<InventoryWeaponSlot>();

        if (oldWeaponSlot != null)
        {
            oldWeaponSlot.ClearWeaponData();
        }


        // --------------------------------------------------------
        // PLACE KEY
        // --------------------------------------------------------

        float finalRotation =
            cachedKeyIconRotation +
            inventoryKeyIconRotationOffset;

        float finalScale =
            cachedKeyIconScale *
            Mathf.Max(
                0.01f,
                inventoryKeyIconScaleMultiplier
            );

        bool placed =
            inventoryUI.ShowItemInSlot(
                currentInventorySlot,
                cachedKeySprite,
                cachedKeyIconSize,
                finalRotation,

                /*
                 * Ключ теперь позиционируется
                 * относительно ЦЕНТРА
                 * новой динамической ячейки.
                 */
                inventoryKeyIconOffset,

                finalScale,
                inventoryGlowSprite,
                inventoryGlowColor,
                inventoryGlowSize,
                inventoryGlowOffset,
                inventoryGlowPulseSpeed,
                inventoryGlowScaleAmount,
                inventoryGlowMinAlpha,
                inventoryGlowMaxAlpha
            );

        if (!placed)
        {
            currentInventorySlot =
                null;

            return false;
        }


        // --------------------------------------------------------
        // REAL ITEM
        // --------------------------------------------------------

        currentInventoryItemTransform =
            inventoryUI.GetItemTransform(
                currentInventorySlot
            );

        if (currentInventoryItemTransform != null)
        {
            currentInventoryItemImage =
                currentInventoryItemTransform
                    .GetComponent<Image>();
        }


        // --------------------------------------------------------
        // REAL GLOW
        // --------------------------------------------------------

        Transform glowTransform =
            currentInventorySlot
                .transform
                .Find(
                    "ItemGlow"
                );

        if (glowTransform != null)
        {
            currentInventoryGlowObject =
                glowTransform.gameObject;

            RectTransform glowRect =
                glowTransform as RectTransform;

            if (glowRect != null)
            {
                glowRect.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        finalRotation +
                        inventoryGlowRotationOffset
                    );
            }
        }


        // --------------------------------------------------------
        // KEY CLICK BEHAVIOUR
        // --------------------------------------------------------

        ConfigureKeySlotButton();


        // --------------------------------------------------------
        // HIDE FINAL ICON WHILE FLYING
        // --------------------------------------------------------

        if (animateToUI)
        {
            SetInventoryItemVisible(
                false
            );

            if (currentInventoryGlowObject != null)
            {
                currentInventoryGlowObject.SetActive(
                    false
                );
            }
        }

        if (debugLogs)
        {
            Debug.Log(
                "[KEY] Key placed into: " +
                currentInventorySlot.name,
                this
            );
        }

        return true;
    }


    // ============================================================
    // CONFIGURE KEY SLOT BUTTON
    // ============================================================

    private void ConfigureKeySlotButton()
    {
        if (currentInventorySlot == null)
        {
            return;
        }

        currentInventorySlotButton =
            currentInventorySlot
                .GetComponent<Button>();

        if (currentInventorySlotButton == null)
        {
            Debug.LogWarning(
                "[KEY] Inventory slot has no Button.",
                currentInventorySlot
            );

            return;
        }

        /*
         * Удаляем только НАШ старый ключевой listener,
         * если по какой-то причине он уже был.
         */
        currentInventorySlotButton
            .onClick
            .RemoveListener(
                OnInventoryKeyPressed
            );

        currentInventorySlotButton
            .onClick
            .AddListener(
                OnInventoryKeyPressed
            );
    }


    // ============================================================
    // KEY PRESSED IN INVENTORY
    // ============================================================

    private void OnInventoryKeyPressed()
    {
        if (!HasKey)
        {
            return;
        }

        if (currentInventorySlot == null)
        {
            return;
        }

        if (doorUnlock == null)
        {
            doorUnlock =
                FindFirstObjectByType<DoorUnlock>();
        }

        if (doorUnlock == null)
        {
            Debug.LogWarning(
                "[KEY] DoorUnlock not found.",
                this
            );

            return;
        }

        /*
         * САМОЕ ГЛАВНОЕ:
         *
         * ключ НИКУДА не экипируется.
         *
         * Нажатие просто говорит двери:
         * "Попробуй открыться этим ключом".
         */
        doorUnlock.TryOpenDoorWithKey();
    }


    // ============================================================
    // FINISH PICKUP
    // ============================================================

    private void FinishPickupInstant()
    {
        SetInventoryItemVisible(
            true
        );

        if (currentInventoryGlowObject != null)
        {
            currentInventoryGlowObject.SetActive(
                true
            );
        }

        HideWorldKeyVisuals();

        float waitTime =
            0f;

        if (playPickupSound &&
            pickupAudioSource != null &&
            pickupAudioSource.clip != null)
        {
            waitTime =
                pickupAudioSource.clip.length;
        }

        StartCoroutine(
            DisableAfterSound(
                waitTime
            )
        );
    }


    // ============================================================
    // ITEM VISIBILITY
    // ============================================================

    private void SetInventoryItemVisible(
        bool visible
    )
    {
        if (currentInventoryItemImage == null)
        {
            return;
        }

        Color color =
            currentInventoryItemImage.color;

        color.a =
            visible
                ? 1f
                : 0f;

        currentInventoryItemImage.color =
            color;
    }


    // ============================================================
    // REMOVE KEY
    // ============================================================

    private void ClearKeyInventoryVisuals()
    {
        /*
         * Сначала удаляем кнопочное поведение ключа.
         */
        if (currentInventorySlotButton != null)
        {
            currentInventorySlotButton
                .onClick
                .RemoveListener(
                    OnInventoryKeyPressed
                );
        }

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI != null &&
            currentInventorySlot != null)
        {
            inventoryUI.RemoveItemFromSlot(
                currentInventorySlot
            );
        }

        currentInventorySlot =
            null;

        currentInventoryItemTransform =
            null;

        currentInventoryItemImage =
            null;

        currentInventoryGlowObject =
            null;

        currentInventorySlotButton =
            null;
    }


    // ============================================================
    // HIDE WORLD KEY
    // ============================================================

    private void HideWorldKeyVisuals()
    {
        if (spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer sr
                 in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled =
                    false;
            }
        }
    }


    // ============================================================
    // DISABLE AFTER SOUND
    // ============================================================

    private IEnumerator DisableAfterSound(
        float delay
    )
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(
                delay
            );
        }

        gameObject.SetActive(
            false
        );
    }


    // ============================================================
    // PUBLIC KEY STATE
    // ============================================================

    public static bool PlayerHasKey()
    {
        return HasKey;
    }


    public static void ConsumeKey()
    {
        HasKey =
            false;

        if (currentInstance != null)
        {
            currentInstance
                .ClearKeyInventoryVisuals();
        }
    }


    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        if (currentInventorySlotButton != null)
        {
            currentInventorySlotButton
                .onClick
                .RemoveListener(
                    OnInventoryKeyPressed
                );
        }

        if (currentInstance == this)
        {
            currentInstance =
                null;
        }
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        pickupDistance =
            Mathf.Max(
                0f,
                pickupDistance
            );

        inventoryKeyIconScaleMultiplier =
            Mathf.Max(
                0.01f,
                inventoryKeyIconScaleMultiplier
            );

        inventoryGlowPulseSpeed =
            Mathf.Max(
                0f,
                inventoryGlowPulseSpeed
            );

        inventoryGlowScaleAmount =
            Mathf.Max(
                0f,
                inventoryGlowScaleAmount
            );

        inventoryGlowMinAlpha =
            Mathf.Clamp01(
                inventoryGlowMinAlpha
            );

        inventoryGlowMaxAlpha =
            Mathf.Clamp01(
                inventoryGlowMaxAlpha
            );

        if (inventoryGlowMaxAlpha <
            inventoryGlowMinAlpha)
        {
            inventoryGlowMaxAlpha =
                inventoryGlowMinAlpha;
        }
    }
}