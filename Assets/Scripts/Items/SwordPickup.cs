using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class SwordPickup : MonoBehaviour, IHandInteractable
{
    [Header("PLAYER")]
    [SerializeField] private Transform player;
    [SerializeField] private float pickupDistance = 1f;

    [Header("INVENTORY")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Sprite inventorySprite;

    [Header("INVENTORY ICON LOOK")]
    [SerializeField] private Vector2 inventoryIconSize =
        new Vector2(115f, 115f);

    [SerializeField] private float inventoryRotationZ = 0f;

    [SerializeField] private Vector2 inventoryIconOffset =
        Vector2.zero;

    [SerializeField, Min(0.1f)]
    private float inventoryIconScale = 1.3f;

    // ============================================================
    // INVENTORY GLOW
    // ============================================================

    [Header("INVENTORY GLOW")]

    [Tooltip("Soft glow для меча внутри инвентаря.")]
    [SerializeField] private Sprite inventoryGlowSprite;

    [SerializeField] private Color inventoryGlowColor =
        new Color(
            0.45f,
            0.85f,
            1f,
            0.80f
        );

    [SerializeField] private Vector2 inventoryGlowSize =
        new Vector2(
            155f,
            210f
        );

    [Tooltip("Смещение glow относительно меча. Y двигает вдоль направления меча.")]
    [SerializeField] private Vector2 inventoryGlowOffset =
        Vector2.zero;

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
    // WORLD SWORD
    // ============================================================

    [Header("WORLD SWORD")]
    [SerializeField] private SpriteRenderer swordRenderer;
    [SerializeField] private Rigidbody2D swordRigidbody;
    [SerializeField] private Collider2D swordCollider;

    [Header("WORLD GLOW")]
    [SerializeField] private GameObject weaponGlow;

    // ============================================================
    // AUDIO
    // ============================================================

    [Header("AUDIO")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("Звук в момент подбора меча.")]
    [SerializeField] private AudioClip pickupClip;

    [Range(0f, 1f)]
    [SerializeField] private float pickupVolume = 1f;

    // ============================================================
    // FLY TO INVENTORY
    // ============================================================

    [Header("FLY TO INVENTORY")]
    [SerializeField] private float flyDuration = 0.45f;
    [SerializeField] private float flyArcHeight = 80f;

    [SerializeField] private Vector2 flyIconSize =
        new Vector2(
            100f,
            100f
        );

    [SerializeField] private float endScale = 1f;

    // ============================================================
    // FLY GLOW
    // ============================================================

    [Header("FLY GLOW")]

    [Tooltip("Soft glow, который летит вместе с мечом.")]
    [SerializeField] private Sprite flyGlowSprite;

    [SerializeField] private Color flyGlowColor =
        new Color(
            0.45f,
            0.85f,
            1f,
            0.85f
        );

    [SerializeField] private Vector2 flyGlowSize =
        new Vector2(
            150f,
            150f
        );

    [SerializeField] private float flyGlowPulseSpeed =
        5f;

    [SerializeField] private float flyGlowPulseAmount =
        0.12f;

    // ============================================================
    // FLY TRAIL
    // ============================================================

    [Header("FLY TRAIL")]

    [SerializeField] private bool useFlyTrail = true;

    [SerializeField] private float trailSpawnInterval =
        0.045f;

    [SerializeField] private float trailLifetime =
        0.20f;

    [SerializeField, Range(0f, 1f)]
    private float trailStartAlpha =
        0.32f;

    [SerializeField] private Vector2 trailGlowSize =
        new Vector2(
            115f,
            115f
        );

    [SerializeField] private float trailStartScale =
        0.90f;

    [SerializeField] private float trailEndScale =
        0.45f;

    // ============================================================
    // FLASH
    // ============================================================

    [Header("PICKUP FLASH")]
    [SerializeField] private float startFlashDuration =
        0.10f;

    [SerializeField] private float startFlashScale =
        1.35f;

    [Header("ARRIVAL FLASH")]
    [SerializeField] private float arrivalFlashDuration =
        0.12f;

    [SerializeField] private float arrivalFlashScale =
        1.35f;

    [Header("DEBUG")]
    [SerializeField] private bool debugLogs = true;

    private Camera mainCamera;
    private bool pickupBusy;

    private class TrailGhost
    {
        public GameObject gameObject;
        public RectTransform rect;
        public Image image;
        public float age;
        public Color baseColor;
    }

    private readonly List<TrailGhost> trailGhosts =
        new List<TrailGhost>();

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        mainCamera = Camera.main;

        if (swordCollider == null)
            swordCollider = GetComponent<Collider2D>();

        if (swordRigidbody == null)
            swordRigidbody = GetComponent<Rigidbody2D>();

        if (swordRenderer == null)
            swordRenderer = GetComponent<SpriteRenderer>();

        if (weaponGlow == null)
        {
            Transform glow =
                transform.Find("WeaponGlow");

            if (glow != null)
                weaponGlow = glow.gameObject;
        }

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        FindPlayer();

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }
    }

    // ============================================================
    // FIND PLAYER
    // ============================================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // ============================================================
    // HAND INTERACTION
    // ============================================================

    public bool CanHandInteract
    {
        get
        {
            if (pickupBusy)
                return false;

            if (swordCollider == null ||
                !swordCollider.enabled)
            {
                return false;
            }

            if (player == null)
            {
                FindPlayer();
            }

            if (player == null)
                return false;

            float distance =
                Vector2.Distance(
                    player.position,
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
                "[SWORD PICKUP] Picked up with HAND button.",
                this
            );
        }

        StartPickup();
    }

    // ============================================================
    // START PICKUP
    // ============================================================

    private void StartPickup()
    {
        if (pickupBusy)
            return;

        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI == null)
        {
            Debug.LogError(
                "[SWORD PICKUP] InventoryUI missing!",
                this
            );

            return;
        }

        Sprite finalSprite =
            inventorySprite;

        if (finalSprite == null &&
            swordRenderer != null)
        {
            finalSprite =
                swordRenderer.sprite;
        }

        if (finalSprite == null)
        {
            Debug.LogError(
                "[SWORD PICKUP] Sword sprite missing!",
                this
            );

            return;
        }

        GameObject targetSlot =
            inventoryUI.GetOrCreateFreeSlot();

        if (targetSlot == null)
            return;

        RectTransform targetRect =
            inventoryUI.GetSlotRect(
                targetSlot
            );

        if (targetRect == null)
            return;

        pickupBusy = true;

        PlayPickupSound();

        if (swordCollider != null)
            swordCollider.enabled = false;

        if (swordRigidbody != null)
        {
            swordRigidbody.linearVelocity =
                Vector2.zero;

            swordRigidbody.angularVelocity =
                0f;

            swordRigidbody.simulated =
                false;
        }

        StartCoroutine(
            PickupSequenceRoutine(
                targetSlot,
                targetRect,
                finalSprite
            )
        );
    }

    // ============================================================
    // SOUND
    // ============================================================

    private void PlayPickupSound()
    {
        if (pickupClip == null)
            return;

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(
                pickupClip,
                pickupVolume
            );
        }
        else
        {
            AudioSource.PlayClipAtPoint(
                pickupClip,
                transform.position,
                pickupVolume
            );
        }
    }

    // ============================================================
    // PICKUP SEQUENCE
    // ============================================================

    private IEnumerator PickupSequenceRoutine(
        GameObject targetSlot,
        RectTransform targetRect,
        Sprite itemSprite
    )
    {
        if (weaponGlow != null)
        {
            weaponGlow.SetActive(true);

            Vector3 originalScale =
                weaponGlow.transform.localScale;

            float timer = 0f;

            while (timer < startFlashDuration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer /
                        Mathf.Max(
                            0.01f,
                            startFlashDuration
                        )
                    );

                float scale =
                    Mathf.Lerp(
                        1f,
                        startFlashScale,
                        t
                    );

                weaponGlow.transform.localScale =
                    originalScale *
                    scale;

                yield return null;
            }

            weaponGlow.transform.localScale =
                originalScale;

            weaponGlow.SetActive(false);
        }

        yield return FlyToInventoryRoutine(
            targetSlot,
            targetRect,
            itemSprite
        );
    }

    // ============================================================
    // FLY TO INVENTORY
    // ============================================================

    private IEnumerator FlyToInventoryRoutine(
        GameObject targetSlot,
        RectTransform targetRect,
        Sprite itemSprite
    )
    {
        Canvas canvas =
            inventoryUI.GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            PutSwordIntoSlot(
                targetSlot,
                itemSprite
            );

            Destroy(gameObject);

            yield break;
        }

        RectTransform canvasRect =
            canvas.transform as RectTransform;

        Camera uiCamera =
            canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

        Vector2 startScreen =
            mainCamera.WorldToScreenPoint(
                transform.position
            );

        Vector2 targetScreen =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                targetRect.position
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            startScreen,
            uiCamera,
            out Vector2 startLocal
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            targetScreen,
            uiCamera,
            out Vector2 targetLocal
        );

        GameObject flyRoot =
            new GameObject(
                "Sword_Fly_To_Inventory",
                typeof(RectTransform)
            );

        flyRoot.transform.SetParent(
            canvas.transform,
            false
        );

        RectTransform flyRootRect =
            flyRoot.GetComponent<RectTransform>();

        flyRootRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        flyRootRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        flyRootRect.pivot =
            new Vector2(0.5f, 0.5f);

        flyRootRect.anchoredPosition =
            startLocal;

        RectTransform glowRect = null;
        Image glowImage = null;

        if (flyGlowSprite != null)
        {
            GameObject glowObject =
                new GameObject(
                    "Glow",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            glowObject.transform.SetParent(
                flyRoot.transform,
                false
            );

            glowRect =
                glowObject.GetComponent<RectTransform>();

            glowImage =
                glowObject.GetComponent<Image>();

            glowRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            glowRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            glowRect.pivot =
                new Vector2(0.5f, 0.5f);

            glowRect.anchoredPosition =
                Vector2.zero;

            glowRect.sizeDelta =
                flyGlowSize;

            glowImage.sprite =
                flyGlowSprite;

            glowImage.color =
                flyGlowColor;

            glowImage.raycastTarget =
                false;
        }

        GameObject swordFlyObject =
            new GameObject(
                "Sword",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        swordFlyObject.transform.SetParent(
            flyRoot.transform,
            false
        );

        RectTransform swordFlyRect =
            swordFlyObject.GetComponent<RectTransform>();

        Image swordFlyImage =
            swordFlyObject.GetComponent<Image>();

        swordFlyRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        swordFlyRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        swordFlyRect.pivot =
            new Vector2(0.5f, 0.5f);

        swordFlyRect.anchoredPosition =
            Vector2.zero;

        swordFlyRect.sizeDelta =
            flyIconSize;

        swordFlyImage.sprite =
            itemSprite;

        swordFlyImage.color =
            Color.white;

        swordFlyImage.preserveAspect =
            true;

        swordFlyImage.raycastTarget =
            false;

        if (swordRenderer != null)
            swordRenderer.enabled = false;

        float timer = 0f;
        float trailTimer = 0f;

        float duration =
            Mathf.Max(
                0.05f,
                flyDuration
            );

        while (timer < duration)
        {
            timer +=
                Time.unscaledDeltaTime;

            trailTimer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    duration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            Vector2 position =
                Vector2.Lerp(
                    startLocal,
                    targetLocal,
                    smoothT
                );

            position.y +=
                Mathf.Sin(
                    smoothT *
                    Mathf.PI
                ) *
                flyArcHeight;

            flyRootRect.anchoredPosition =
                position;

            float rootScale =
                Mathf.Lerp(
                    1f,
                    endScale,
                    smoothT
                );

            flyRootRect.localScale =
                Vector3.one *
                rootScale;

            if (glowRect != null)
            {
                float pulse =
                    1f +
                    Mathf.Sin(
                        Time.unscaledTime *
                        flyGlowPulseSpeed
                    ) *
                    flyGlowPulseAmount;

                glowRect.localScale =
                    Vector3.one *
                    pulse;
            }

            if (useFlyTrail &&
                flyGlowSprite != null &&
                trailTimer >= trailSpawnInterval)
            {
                trailTimer = 0f;

                CreateTrailGhost(
                    canvas,
                    position
                );
            }

            UpdateTrailGhosts();

            yield return null;
        }

        // ========================================================
        // ARRIVAL FLASH
        // ========================================================

        if (glowRect != null &&
            glowImage != null)
        {
            float flashTimer = 0f;

            Color startColor =
                glowImage.color;

            while (flashTimer <
                   arrivalFlashDuration)
            {
                flashTimer +=
                    Time.unscaledDeltaTime;

                UpdateTrailGhosts();

                float t =
                    Mathf.Clamp01(
                        flashTimer /
                        Mathf.Max(
                            0.01f,
                            arrivalFlashDuration
                        )
                    );

                float scale =
                    Mathf.Lerp(
                        1f,
                        arrivalFlashScale,
                        t
                    );

                glowRect.localScale =
                    Vector3.one *
                    scale;

                Color c =
                    startColor;

                c.a =
                    Mathf.Lerp(
                        startColor.a,
                        0f,
                        t
                    );

                glowImage.color =
                    c;

                yield return null;
            }
        }

        ClearTrailGhosts();

        PutSwordIntoSlot(
            targetSlot,
            itemSprite
        );

        if (debugLogs)
        {
            Debug.Log(
                "[SWORD PICKUP] Sword added to inventory.",
                this
            );
        }

        Destroy(flyRoot);
        Destroy(gameObject);
    }

    // ============================================================
    // CREATE TRAIL
    // ============================================================

    private void CreateTrailGhost(
        Canvas canvas,
        Vector2 anchoredPosition
    )
    {
        GameObject ghostObject =
            new GameObject(
                "Sword_Trail",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

        ghostObject.transform.SetParent(
            canvas.transform,
            false
        );

        RectTransform rect =
            ghostObject.GetComponent<RectTransform>();

        Image image =
            ghostObject.GetComponent<Image>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            anchoredPosition;

        rect.sizeDelta =
            trailGlowSize;

        rect.localScale =
            Vector3.one *
            trailStartScale;

        Color color =
            flyGlowColor;

        color.a =
            trailStartAlpha;

        image.sprite =
            flyGlowSprite;

        image.color =
            color;

        image.raycastTarget =
            false;

        TrailGhost ghost =
            new TrailGhost
            {
                gameObject = ghostObject,
                rect = rect,
                image = image,
                age = 0f,
                baseColor = color
            };

        trailGhosts.Add(
            ghost
        );
    }

    // ============================================================
    // UPDATE TRAIL
    // ============================================================

    private void UpdateTrailGhosts()
    {
        for (int i = trailGhosts.Count - 1;
             i >= 0;
             i--)
        {
            TrailGhost ghost =
                trailGhosts[i];

            if (ghost == null ||
                ghost.gameObject == null)
            {
                trailGhosts.RemoveAt(i);
                continue;
            }

            ghost.age +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    ghost.age /
                    Mathf.Max(
                        0.01f,
                        trailLifetime
                    )
                );

            Color color =
                ghost.baseColor;

            color.a =
                Mathf.Lerp(
                    trailStartAlpha,
                    0f,
                    t
                );

            ghost.image.color =
                color;

            float scale =
                Mathf.Lerp(
                    trailStartScale,
                    trailEndScale,
                    t
                );

            ghost.rect.localScale =
                Vector3.one *
                scale;

            if (t >= 1f)
            {
                Destroy(
                    ghost.gameObject
                );

                trailGhosts.RemoveAt(i);
            }
        }
    }

    // ============================================================
    // CLEAR TRAIL
    // ============================================================

    private void ClearTrailGhosts()
    {
        foreach (TrailGhost ghost
                 in trailGhosts)
        {
            if (ghost != null &&
                ghost.gameObject != null)
            {
                Destroy(
                    ghost.gameObject
                );
            }
        }

        trailGhosts.Clear();
    }

    // ============================================================
    // PUT INTO INVENTORY
    // ============================================================

    private void PutSwordIntoSlot(
        GameObject targetSlot,
        Sprite itemSprite
    )
    {
        inventoryUI.ShowItemInSlot(
            targetSlot,
            itemSprite,

            inventoryIconSize,
            inventoryRotationZ,
            inventoryIconOffset,
            inventoryIconScale,

            inventoryGlowSprite,
            inventoryGlowColor,
            inventoryGlowSize,
            inventoryGlowOffset,

            inventoryGlowPulseSpeed,
            inventoryGlowScaleAmount,
            inventoryGlowMinAlpha,
            inventoryGlowMaxAlpha
        );
    }
}