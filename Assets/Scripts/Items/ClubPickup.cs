using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class ClubPickup : MonoBehaviour, IHandInteractable
{
    // ============================================================
    // PLAYER
    // ============================================================

    [Header("PLAYER")]

    [SerializeField]
    private Transform player;

    [Tooltip(
        "Расстояние, на котором дубинку можно подобрать кнопкой руки."
    )]
    [SerializeField]
    private float pickupDistance = 1f;


    // ============================================================
    // INVENTORY
    // ============================================================

    [Header("INVENTORY")]

    [SerializeField]
    private InventoryUI inventoryUI;

    [Tooltip(
        "Спрайт дубинки, который будет показан в инвентаре."
    )]
    [SerializeField]
    private Sprite inventorySprite;


    // ============================================================
    // INVENTORY ICON LOOK
    // ============================================================

    [Header("INVENTORY ICON LOOK")]

    [Tooltip(
        "Размер дубинки внутри обычной ячейки инвентаря."
    )]
    [SerializeField]
    private Vector2 inventoryIconSize =
        new Vector2(
            115f,
            115f
        );

    [Tooltip(
        "Поворот дубинки внутри обычной ячейки."
    )]
    [SerializeField]
    private float inventoryRotationZ = -45f;

    [Tooltip(
        "X/Y смещение дубинки внутри обычной ячейки."
    )]
    [SerializeField]
    private Vector2 inventoryIconOffset =
        Vector2.zero;

    [Tooltip(
        "Дополнительный Scale дубинки внутри обычной ячейки."
    )]
    [SerializeField, Min(0.1f)]
    private float inventoryIconScale = 1.3f;


    // ============================================================
    // WEAPON BUTTON ICON
    // ============================================================

    [Header("WEAPON BUTTON ICON")]

    [Tooltip(
        "Тип оружия. Для этой дубинки должен быть Club."
    )]
    [SerializeField]
    private string weaponId = "Club";

    [Tooltip(
        "Размер дубинки внутри большого круглого WeaponButton."
    )]
    [SerializeField]
    private Vector2 weaponButtonIconSize =
        new Vector2(
            190f,
            190f
        );

    [Tooltip(
        "Z-поворот дубинки внутри большого WeaponButton."
    )]
    [SerializeField]
    private float weaponButtonIconRotation = 0f;

    [Tooltip(
        "X/Y смещение самой дубинки " +
        "внутри большого WeaponButton."
    )]
    [SerializeField]
    private Vector2 weaponButtonIconOffset =
        Vector2.zero;

    [Tooltip(
        "Дополнительный Scale самой дубинки " +
        "в большом WeaponButton. " +
        "1 = без дополнительного масштаба."
    )]
    [SerializeField, Min(0.1f)]
    private float weaponButtonIconScale = 1f;


    // ============================================================
    // WEAPON BUTTON GLOW
    // ============================================================

    [Header("WEAPON BUTTON GLOW")]

    [Tooltip(
        "Общий индивидуальный Scale оранжевого glow " +
        "дубинки в большом WeaponButton. " +
        "Твоё текущее настроенное значение может быть 1.5."
    )]
    [SerializeField, Range(0.1f, 3f)]
    private float weaponButtonGlowScale = 1.15f;


    [Tooltip(
        "Отдельное изменение размера большого glow. " +
        "X = ширина, Y = высота. " +
        "1 / 1 = без дополнительного растяжения."
    )]
    [SerializeField]
    private Vector2 weaponButtonGlowSizeScale =
        Vector2.one;


    [Tooltip(
        "X/Y смещение ТОЛЬКО оранжевой подсветки " +
        "дубинки в большом WeaponButton. " +
        "Сама дубинка при этом остаётся на месте."
    )]
    [SerializeField]
    private Vector2 weaponButtonGlowOffset =
        Vector2.zero;


    [Tooltip(
        "Дополнительный Z-поворот ТОЛЬКО glow " +
        "дубинки в большом WeaponButton."
    )]
    [SerializeField]
    private float weaponButtonGlowRotationOffset = 0f;


    // ============================================================
    // INVENTORY GLOW
    // ============================================================

    [Header("INVENTORY GLOW")]

    [Tooltip(
        "Soft glow для дубинки в обычной ячейке."
    )]
    [SerializeField]
    private Sprite inventoryGlowSprite;

    [Tooltip(
        "Цвет свечения дубинки в инвентаре."
    )]
    [SerializeField]
    private Color inventoryGlowColor =
        new Color(
            1.00f,
            0.55f,
            0.18f,
            0.85f
        );

    [Tooltip(
        "Размер glow в обычной ячейке."
    )]
    [SerializeField]
    private Vector2 inventoryGlowSize =
        new Vector2(
            155f,
            190f
        );

    [Tooltip(
        "X/Y смещение glow в обычной ячейке."
    )]
    [SerializeField]
    private Vector2 inventoryGlowOffset =
        Vector2.zero;

    [Tooltip(
        "Дополнительный Z-поворот ТОЛЬКО подсветки " +
        "дубинки в обычной ячейке. " +
        "Не меняет поворот самой дубинки."
    )]
    [SerializeField]
    private float inventoryGlowRotationOffset = 0f;

    [SerializeField]
    private float inventoryGlowPulseSpeed =
        1.4f;

    [SerializeField]
    private float inventoryGlowScaleAmount =
        0.08f;

    [SerializeField, Range(0f, 1f)]
    private float inventoryGlowMinAlpha =
        0.50f;

    [SerializeField, Range(0f, 1f)]
    private float inventoryGlowMaxAlpha =
        0.85f;


    // ============================================================
    // WORLD CLUB
    // ============================================================

    [Header("WORLD CLUB")]

    [SerializeField]
    private SpriteRenderer clubRenderer;

    [SerializeField]
    private Rigidbody2D clubRigidbody;

    [SerializeField]
    private Collider2D clubCollider;


    // ============================================================
    // WORLD GLOW
    // ============================================================

    [Header("WORLD GLOW")]

    [SerializeField]
    private GameObject weaponGlow;


    // ============================================================
    // AUDIO
    // ============================================================

    [Header("AUDIO")]

    [SerializeField]
    private AudioSource sfxSource;

    [Tooltip(
        "Звук в момент успешного подбора дубинки."
    )]
    [SerializeField]
    private AudioClip pickupClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float pickupVolume = 0.6f;


    // ============================================================
    // PICKUP HAPTICS
    // ============================================================

    [Header("PICKUP HAPTICS")]

    [SerializeField]
    private bool usePickupHaptics = true;

    [SerializeField, Range(5, 100)]
    private int androidPickupHapticMs = 22;

    [SerializeField]
    private MicroHaptics.IOSHapticStyle
        iosPickupHapticStyle =
            MicroHaptics.IOSHapticStyle.Medium;


    // ============================================================
    // FLY TO INVENTORY
    // ============================================================

    [Header("FLY TO INVENTORY")]

    [SerializeField]
    private float flyDuration = 0.45f;

    [SerializeField]
    private float flyArcHeight = 80f;

    [SerializeField]
    private Vector2 flyIconSize =
        new Vector2(
            100f,
            100f
        );

    [SerializeField]
    private float endScale = 1f;


    // ============================================================
    // FLY GLOW
    // ============================================================

    [Header("FLY GLOW")]

    [SerializeField]
    private Sprite flyGlowSprite;

    [SerializeField]
    private Color flyGlowColor =
        new Color(
            1.00f,
            0.55f,
            0.18f,
            0.95f
        );

    [SerializeField]
    private Vector2 flyGlowSize =
        new Vector2(
            150f,
            150f
        );

    [SerializeField]
    private float flyGlowPulseSpeed =
        5f;

    [SerializeField]
    private float flyGlowPulseAmount =
        0.12f;


    // ============================================================
    // FLY TRAIL
    // ============================================================

    [Header("FLY TRAIL")]

    [SerializeField]
    private bool useFlyTrail = true;

    [Tooltip(
        "Чем меньше значение, тем плотнее шлейф."
    )]
    [SerializeField]
    private float trailSpawnInterval =
        0.045f;

    [SerializeField]
    private float trailLifetime =
        0.20f;

    [SerializeField, Range(0f, 1f)]
    private float trailStartAlpha =
        0.32f;

    [SerializeField]
    private Vector2 trailGlowSize =
        new Vector2(
            160f,
            160f
        );

    [SerializeField]
    private float trailStartScale =
        0.90f;

    [SerializeField]
    private float trailEndScale =
        0.45f;


    // ============================================================
    // EXTRA TRAIL
    // ============================================================

    [Header("EXTRA TRAIL LAYER")]

    [SerializeField]
    private bool useInnerTrail = true;

    [SerializeField]
    private Vector2 innerTrailSize =
        new Vector2(
            125f,
            125f
        );

    [SerializeField, Range(0f, 1f)]
    private float innerTrailAlpha =
        0.72f;

    [SerializeField]
    private float innerTrailStartScale =
        0.85f;

    [SerializeField]
    private float innerTrailEndScale =
        0.20f;


    // ============================================================
    // FLASH
    // ============================================================

    [Header("PICKUP FLASH")]

    [SerializeField]
    private float startFlashDuration =
        0.10f;

    [SerializeField]
    private float startFlashScale =
        1.35f;


    [Header("ARRIVAL FLASH")]

    [SerializeField]
    private float arrivalFlashDuration =
        0.12f;

    [SerializeField]
    private float arrivalFlashScale =
        1.35f;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = true;


    // ============================================================
    // PRIVATE
    // ============================================================

    private Camera mainCamera;

    private bool pickupBusy;


    // ============================================================
    // TRAIL DATA
    // ============================================================

    private class TrailGhost
    {
        public GameObject gameObject;

        public RectTransform rect;

        public Image image;

        public float age;

        public Color baseColor;

        public float startScale;

        public float endScale;
    }


    private readonly List<TrailGhost> trailGhosts =
        new List<TrailGhost>();


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        mainCamera =
            Camera.main;


        if (clubCollider == null)
        {
            clubCollider =
                GetComponent<Collider2D>();
        }


        if (clubRigidbody == null)
        {
            clubRigidbody =
                GetComponent<Rigidbody2D>();
        }


        if (clubRenderer == null)
        {
            clubRenderer =
                GetComponent<SpriteRenderer>();
        }


        if (weaponGlow == null)
        {
            Transform glow =
                transform.Find(
                    "WeaponGlow"
                );


            if (glow != null)
            {
                weaponGlow =
                    glow.gameObject;
            }
        }


        if (sfxSource == null)
        {
            sfxSource =
                GetComponent<AudioSource>();
        }


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
            GameObject.FindGameObjectWithTag(
                "Player"
            );


        if (playerObject != null)
        {
            player =
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
            if (pickupBusy)
            {
                return false;
            }


            if (clubCollider == null ||
                !clubCollider.enabled)
            {
                return false;
            }


            if (player == null)
            {
                FindPlayer();
            }


            if (player == null)
            {
                return false;
            }


            float distance =
                Vector2.Distance(
                    player.position,
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


        if (debugLogs)
        {
            Debug.Log(
                "[CLUB PICKUP] Picked up with HAND button.",
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
        {
            return;
        }


        if (inventoryUI == null)
        {
            inventoryUI =
                FindFirstObjectByType<InventoryUI>();
        }


        if (inventoryUI == null)
        {
            Debug.LogError(
                "[CLUB PICKUP] InventoryUI missing!",
                this
            );

            return;
        }


        Sprite finalSprite =
            inventorySprite;


        if (finalSprite == null &&
            clubRenderer != null)
        {
            finalSprite =
                clubRenderer.sprite;
        }


        if (finalSprite == null)
        {
            Debug.LogError(
                "[CLUB PICKUP] Club sprite missing!",
                this
            );

            return;
        }


        GameObject targetSlot =
            inventoryUI.GetOrCreateFreeSlot();


        if (targetSlot == null)
        {
            return;
        }


        RectTransform targetRect =
            inventoryUI.GetSlotRect(
                targetSlot
            );


        if (targetRect == null)
        {
            return;
        }


        pickupBusy =
            true;


        PlayPickupHaptic();

        PlayPickupSound();


        if (clubCollider != null)
        {
            clubCollider.enabled =
                false;
        }


        if (clubRigidbody != null)
        {
            clubRigidbody.linearVelocity =
                Vector2.zero;

            clubRigidbody.angularVelocity =
                0f;

            clubRigidbody.simulated =
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
    // HAPTIC
    // ============================================================

    private void PlayPickupHaptic()
    {
        if (!usePickupHaptics)
        {
            return;
        }


        MicroHaptics.Pulse(
            androidPickupHapticMs,
            iosPickupHapticStyle
        );
    }


    // ============================================================
    // SOUND
    // ============================================================

    private void PlayPickupSound()
    {
        if (pickupClip == null)
        {
            return;
        }


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
            weaponGlow.SetActive(
                true
            );


            Vector3 originalScale =
                weaponGlow.transform.localScale;


            float timer =
                0f;


            while (timer <
                   startFlashDuration)
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


                float punch =
                    Mathf.Sin(
                        t *
                        Mathf.PI *
                        0.5f
                    );


                float scale =
                    Mathf.Lerp(
                        1f,
                        startFlashScale,
                        punch
                    );


                weaponGlow.transform.localScale =
                    originalScale *
                    scale;


                yield return null;
            }


            weaponGlow.transform.localScale =
                originalScale;


            weaponGlow.SetActive(
                false
            );
        }


        yield return
            FlyToInventoryRoutine(
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
            PutClubIntoSlot(
                targetSlot,
                itemSprite
            );


            Destroy(
                gameObject
            );


            yield break;
        }


        if (mainCamera == null)
        {
            mainCamera =
                Camera.main;
        }


        RectTransform canvasRect =
            canvas.transform
                as RectTransform;


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


        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                startScreen,
                uiCamera,
                out Vector2 startLocal
            );


        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                targetScreen,
                uiCamera,
                out Vector2 targetLocal
            );


        GameObject flyRoot =
            new GameObject(
                "Club_Fly_To_Inventory",
                typeof(RectTransform)
            );


        flyRoot.transform.SetParent(
            canvas.transform,
            false
        );


        RectTransform flyRootRect =
            flyRoot.GetComponent<RectTransform>();


        flyRootRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        flyRootRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        flyRootRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        flyRootRect.anchoredPosition =
            startLocal;


        RectTransform glowRect =
            null;


        Image glowImage =
            null;


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


        GameObject clubFlyObject =
            new GameObject(
                "Club",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );


        clubFlyObject.transform.SetParent(
            flyRoot.transform,
            false
        );


        RectTransform clubFlyRect =
            clubFlyObject
                .GetComponent<RectTransform>();


        Image clubFlyImage =
            clubFlyObject
                .GetComponent<Image>();


        clubFlyRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        clubFlyRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        clubFlyRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        clubFlyRect.anchoredPosition =
            Vector2.zero;


        clubFlyRect.sizeDelta =
            flyIconSize;


        clubFlyImage.sprite =
            itemSprite;


        clubFlyImage.color =
            Color.white;


        clubFlyImage.preserveAspect =
            true;


        clubFlyImage.raycastTarget =
            false;


        if (clubRenderer != null)
        {
            clubRenderer.enabled =
                false;
        }


        float timer =
            0f;


        float trailTimer =
            0f;


        float duration =
            Mathf.Max(
                0.05f,
                flyDuration
            );


        while (timer <
               duration)
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
                trailTimer >=
                Mathf.Max(
                    0.005f,
                    trailSpawnInterval
                ))
            {
                trailTimer =
                    0f;


                CreateTrailGhost(
                    canvas,
                    position,
                    trailGlowSize,
                    trailStartAlpha,
                    trailStartScale,
                    trailEndScale
                );


                if (useInnerTrail)
                {
                    CreateTrailGhost(
                        canvas,
                        position,
                        innerTrailSize,
                        innerTrailAlpha,
                        innerTrailStartScale,
                        innerTrailEndScale
                    );
                }
            }


            UpdateTrailGhosts();


            yield return null;
        }


        flyRootRect.anchoredPosition =
            targetLocal;


        if (glowRect != null &&
            glowImage != null)
        {
            float flashTimer =
                0f;


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


                float punch =
                    Mathf.Sin(
                        t *
                        Mathf.PI
                    );


                float scale =
                    Mathf.Lerp(
                        1f,
                        arrivalFlashScale,
                        punch
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


        PutClubIntoSlot(
            targetSlot,
            itemSprite
        );


        if (debugLogs)
        {
            Debug.Log(
                "[CLUB PICKUP] Club added to inventory.",
                this
            );
        }


        Destroy(
            flyRoot
        );


        Destroy(
            gameObject
        );
    }


    // ============================================================
    // CREATE TRAIL
    // ============================================================

    private void CreateTrailGhost(
        Canvas canvas,
        Vector2 anchoredPosition,
        Vector2 size,
        float alpha,
        float startScale,
        float endScale
    )
    {
        GameObject ghostObject =
            new GameObject(
                "Club_Trail",
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
            new Vector2(
                0.5f,
                0.5f
            );


        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.anchoredPosition =
            anchoredPosition;


        rect.sizeDelta =
            size;


        rect.localScale =
            Vector3.one *
            startScale;


        Color color =
            flyGlowColor;


        color.a =
            alpha;


        image.sprite =
            flyGlowSprite;


        image.color =
            color;


        image.raycastTarget =
            false;


        TrailGhost ghost =
            new TrailGhost
            {
                gameObject =
                    ghostObject,

                rect =
                    rect,

                image =
                    image,

                age =
                    0f,

                baseColor =
                    color,

                startScale =
                    startScale,

                endScale =
                    endScale
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
        for (int i =
                 trailGhosts.Count - 1;
             i >= 0;
             i--)
        {
            TrailGhost ghost =
                trailGhosts[i];


            if (ghost == null ||
                ghost.gameObject == null)
            {
                trailGhosts.RemoveAt(
                    i
                );

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
                    ghost.baseColor.a,
                    0f,
                    t
                );


            ghost.image.color =
                color;


            float scale =
                Mathf.Lerp(
                    ghost.startScale,
                    ghost.endScale,
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


                trailGhosts.RemoveAt(
                    i
                );
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
    // PUT CLUB INTO INVENTORY
    // ============================================================

    private void PutClubIntoSlot(
        GameObject targetSlot,
        Sprite itemSprite
    )
    {
        if (inventoryUI == null ||
            targetSlot == null ||
            itemSprite == null)
        {
            return;
        }


        bool placed =
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
                inventoryGlowRotationOffset,

                inventoryGlowPulseSpeed,
                inventoryGlowScaleAmount,
                inventoryGlowMinAlpha,
                inventoryGlowMaxAlpha
            );


        if (!placed)
        {
            Debug.LogWarning(
                "[CLUB PICKUP] Failed to put club into slot.",
                this
            );

            return;
        }


        /*
         * Новая дубинка получает
         * 100% прочности.
         *
         * Передаём полный набор
         * индивидуальных настроек
         * большого WeaponButton.
         */
        inventoryUI.ConfigureWeaponSlot(
            targetSlot,
            itemSprite,
            weaponId,

            weaponButtonIconSize,
            weaponButtonIconRotation,
            weaponButtonIconOffset,

            1f,

            weaponButtonIconScale,

            weaponButtonGlowScale,
            weaponButtonGlowSizeScale,
            weaponButtonGlowOffset,
            weaponButtonGlowRotationOffset
        );


        if (debugLogs)
        {
            Debug.Log(
                "[CLUB PICKUP] Slot configured as CLUB: " +
                targetSlot.name,
                this
            );
        }
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        pickupDistance =
            Mathf.Max(
                0.05f,
                pickupDistance
            );


        inventoryIconSize.x =
            Mathf.Max(
                1f,
                inventoryIconSize.x
            );


        inventoryIconSize.y =
            Mathf.Max(
                1f,
                inventoryIconSize.y
            );


        inventoryIconScale =
            Mathf.Max(
                0.1f,
                inventoryIconScale
            );


        weaponButtonIconSize.x =
            Mathf.Max(
                1f,
                weaponButtonIconSize.x
            );


        weaponButtonIconSize.y =
            Mathf.Max(
                1f,
                weaponButtonIconSize.y
            );


        weaponButtonIconScale =
            Mathf.Max(
                0.1f,
                weaponButtonIconScale
            );


        weaponButtonGlowScale =
            Mathf.Clamp(
                weaponButtonGlowScale,
                0.1f,
                3f
            );


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


        pickupVolume =
            Mathf.Clamp01(
                pickupVolume
            );


        trailStartAlpha =
            Mathf.Clamp01(
                trailStartAlpha
            );


        innerTrailAlpha =
            Mathf.Clamp01(
                innerTrailAlpha
            );


        inventoryGlowMinAlpha =
            Mathf.Clamp01(
                inventoryGlowMinAlpha
            );


        inventoryGlowMaxAlpha =
            Mathf.Clamp01(
                inventoryGlowMaxAlpha
            );


        if (string.IsNullOrWhiteSpace(
                weaponId))
        {
            weaponId =
                "Club";
        }
    }


    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        ClearTrailGhosts();
    }
}