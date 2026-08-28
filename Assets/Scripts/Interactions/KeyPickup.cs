using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyPickup : MonoBehaviour, IHandInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject keyIconUI;

    // ============================================================
    // INVENTORY GLOW
    // ============================================================

    [Header("INVENTORY GLOW")]

    [Tooltip("Наш Soft glow для ключа в инвентаре.")]
    [SerializeField] private Sprite inventoryGlowSprite;

    [Tooltip("Цвет свечения ключа.")]
    [SerializeField] private Color inventoryGlowColor =
        new Color(
            1.00f,
            0.75f,
            0.15f,
            0.85f
        );

    [Tooltip("Размер свечения.")]
    [SerializeField] private Vector2 inventoryGlowSize =
        new Vector2(
            155f,
            190f
        );

    [Tooltip("Смещение свечения относительно ключа.")]
    [SerializeField] private Vector2 inventoryGlowOffset =
        Vector2.zero;

    [Tooltip("Дополнительный поворот свечения.")]
    [SerializeField] private float inventoryGlowRotationOffset =
        0f;

    [SerializeField] private float inventoryGlowPulseSpeed =
        1.4f;

    [SerializeField] private float inventoryGlowScaleAmount =
        0.08f;

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
    [SerializeField] private GameObject keyGlowObject;
    [SerializeField] private MonoBehaviour keyPulseScript;

    // ============================================================
    // PLAYER
    // ============================================================

    [Header("PLAYER")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip(
        "Расстояние, на котором ключ можно подобрать кнопкой руки."
    )]
    [SerializeField] private float pickupDistance = 1.2f;

    // ============================================================
    // SOUND
    // ============================================================

    [Header("PICKUP SOUND")]
    [SerializeField] private AudioSource pickupAudioSource;
    [SerializeField] private bool playPickupSound = true;

    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("PICKUP HAPTICS")]
    [SerializeField] private bool usePickupHaptics = true;

    // ============================================================
    // FLY
    // ============================================================

    [Header("FLY TO UI ANIMATION")]
    [SerializeField] private bool animateToUI = true;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]
    [SerializeField] private bool debugLogs = true;

    // ============================================================
    // PUBLIC KEY STATE
    // ============================================================

    public static bool HasKey { get; private set; }

    /*
     * Сохраняем ссылку на текущий ключ.
     * Это позволяет статическому ConsumeKey()
     * убрать не только HasKey,
     * но и UI ключа + его glow.
     */
    private static KeyPickup currentInstance;

    // ============================================================
    // PRIVATE
    // ============================================================

    private bool pickedUp;

    private Transform playerTransform;
    private Collider2D keyCollider;

    private SpriteRenderer[] spriteRenderers;

    private PickupFlyEffect flyEffect;

    private GameObject createdInventoryGlow;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        currentInstance = this;

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

        FindPlayer();

        HasKey = false;

        if (keyIconUI != null)
        {
            keyIconUI.SetActive(false);
        }

        RemoveInventoryGlow();

        if (debugLogs)
        {
            Debug.Log(
                $"[KEY] Awake. keyIconUI=" +
                $"{(keyIconUI != null ? "OK" : "NULL")}",
                this
            );
        }
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
                return false;

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
                return false;

            float distance =
                Vector2.Distance(
                    playerTransform.position,
                    transform.position
                );

            return distance <= pickupDistance;
        }
    }

    public void HandInteract()
    {
        if (!CanHandInteract)
            return;

        if (debugLogs)
        {
            Debug.Log(
                "[KEY] Picked up with HAND button.",
                this
            );
        }

        PickupKey();
    }

    // ============================================================
    // PICKUP KEY
    // ============================================================

    private void PickupKey()
    {
        if (pickedUp)
            return;

        pickedUp = true;
        HasKey = true;

        if (debugLogs)
        {
            Debug.Log(
                "[KEY] Key picked up successfully.",
                this
            );
        }

        if (keyCollider != null)
        {
            keyCollider.enabled = false;
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
            keyGlowObject.SetActive(false);
        }

        if (keyPulseScript != null)
        {
            keyPulseScript.enabled = false;
        }

        if (animateToUI &&
            keyIconUI != null &&
            flyEffect != null)
        {
            StartCoroutine(
                flyEffect.FlyToUI(
                    keyIconUI,
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
    // FINISH PICKUP
    // ============================================================

    private void FinishPickupInstant()
    {
        if (keyIconUI != null)
        {
            keyIconUI.SetActive(true);

            CreateInventoryGlow();

            if (debugLogs)
            {
                Debug.Log(
                    "[KEY] KeyIcon UI enabled.",
                    this
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[KEY] keyIconUI is NULL. " +
                "Assign KeyIcon in Inspector.",
                this
            );
        }

        HideWorldKeyVisuals();

        float waitTime = 0f;

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
    // CREATE INVENTORY GLOW
    // ============================================================

    private void CreateInventoryGlow()
    {
        if (keyIconUI == null)
            return;

        if (inventoryGlowSprite == null)
            return;

        Transform parent =
            keyIconUI.transform.parent;

        if (parent == null)
            return;

        /*
         * Если старый glow почему-то существует,
         * сначала полностью его убираем.
         */
        RemoveInventoryGlow();

        createdInventoryGlow =
            new GameObject(
                "KeyItemGlow",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(InventoryItemGlow)
            );

        createdInventoryGlow.transform.SetParent(
            parent,
            false
        );

        RectTransform glowRect =
            createdInventoryGlow
                .GetComponent<RectTransform>();

        Image glowImage =
            createdInventoryGlow
                .GetComponent<Image>();

        InventoryItemGlow glowPulse =
            createdInventoryGlow
                .GetComponent<InventoryItemGlow>();

        RectTransform keyRect =
            keyIconUI
                .GetComponent<RectTransform>();

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

        glowRect.anchoredPosition =
            inventoryGlowOffset;

        glowRect.sizeDelta =
            inventoryGlowSize;

        float keyRotation = 0f;

        if (keyRect != null)
        {
            keyRotation =
                keyRect.localEulerAngles.z;
        }

        glowRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                keyRotation +
                inventoryGlowRotationOffset
            );

        glowRect.localScale =
            Vector3.one;

        glowImage.sprite =
            inventoryGlowSprite;

        glowImage.color =
            inventoryGlowColor;

        glowImage.preserveAspect =
            false;

        glowImage.raycastTarget =
            false;

        if (glowPulse != null)
        {
            glowPulse.Setup(
                inventoryGlowPulseSpeed,
                inventoryGlowScaleAmount,
                inventoryGlowMinAlpha,
                inventoryGlowMaxAlpha
            );
        }

        int keyIndex =
            keyIconUI
                .transform
                .GetSiblingIndex();

        createdInventoryGlow
            .transform
            .SetSiblingIndex(
                Mathf.Max(
                    0,
                    keyIndex
                )
            );

        keyIconUI
            .transform
            .SetSiblingIndex(
                createdInventoryGlow
                    .transform
                    .GetSiblingIndex() + 1
            );

        createdInventoryGlow.SetActive(true);
    }

    // ============================================================
    // REMOVE INVENTORY GLOW
    // ============================================================

    private void RemoveInventoryGlow()
    {
        /*
         * Сначала используем сохранённую ссылку.
         */
        if (createdInventoryGlow != null)
        {
            /*
             * SetActive(false) убирает glow
             * сразу в этом же кадре.
             */
            createdInventoryGlow.SetActive(false);

            Destroy(
                createdInventoryGlow
            );

            createdInventoryGlow = null;
        }

        if (keyIconUI == null)
            return;

        Transform parent =
            keyIconUI.transform.parent;

        if (parent == null)
            return;

        /*
         * Дополнительная защита:
         * если ссылка потерялась,
         * ищем glow по имени.
         */
        Transform oldGlow =
            parent.Find(
                "KeyItemGlow"
            );

        if (oldGlow != null)
        {
            oldGlow.gameObject.SetActive(false);

            Destroy(
                oldGlow.gameObject
            );
        }
    }

    // ============================================================
    // CLEAR KEY INVENTORY VISUALS
    // ============================================================

    private void ClearKeyInventoryVisuals()
    {
        /*
         * Убираем сам ключ из ячейки.
         */
        if (keyIconUI != null)
        {
            keyIconUI.SetActive(false);
        }

        /*
         * И одновременно убираем
         * золотое свечение.
         */
        RemoveInventoryGlow();

        if (debugLogs)
        {
            Debug.Log(
                "[KEY] Key icon and inventory glow removed.",
                this
            );
        }
    }

    // ============================================================
    // HIDE WORLD KEY
    // ============================================================

    private void HideWorldKeyVisuals()
    {
        if (spriteRenderers == null)
            return;

        foreach (SpriteRenderer sr
                 in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = false;
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

        /*
         * Сам объект ключа выключается,
         * но currentInstance остаётся доступен,
         * поэтому ConsumeKey всё ещё сможет
         * убрать UI и glow.
         */
        gameObject.SetActive(false);
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
        HasKey = false;

        /*
         * ВАЖНО:
         * теперь при расходовании ключа
         * исчезает не только иконка,
         * но и его золотой glow.
         */
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
        if (currentInstance == this)
        {
            currentInstance = null;
        }
    }
}