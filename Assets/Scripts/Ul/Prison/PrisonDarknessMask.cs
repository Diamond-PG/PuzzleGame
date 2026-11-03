using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class PrisonDarknessMask : MonoBehaviour
{
    // ============================================================
    // 01 - HERO VISIBLE AREA
    // ============================================================

    [Header("VISIBLE AREA")]
    [SerializeField, Range(0f, 1f)]
    private float centerX = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float centerY = 0.5f;

    [SerializeField, Range(0.01f, 1f)]
    private float visibleWidth = 0.35f;

    [SerializeField, Range(0.01f, 1f)]
    private float visibleHeight = 0.45f;

    // ============================================================
    // 02 - EDGE AND DARKNESS
    // ============================================================

    [Header("EDGE SETTINGS")]
    [SerializeField, Range(0f, 0.3f)]
    private float edgeSoftness = 0.035f;

    [Header("DARKNESS SETTINGS")]
    [SerializeField, Range(0f, 1f)]
    private float darkness = 1f;

    [SerializeField]
    private bool showMaskOnStart = true;

    // ============================================================
    // 03 - MOVEMENT BUTTONS
    // ============================================================

    [Header("MOVEMENT BUTTONS")]
    [SerializeField]
    private bool showMovementWindow = false;

    [Header("LEFT ARROW")]
    [SerializeField, Range(0f, 1f)]
    private float leftArrowX = 0.08f;

    [SerializeField, Range(0f, 1f)]
    private float leftArrowY = 0.12f;

    [SerializeField, Range(0.005f, 0.3f)]
    private float leftArrowRadius = 0.045f;

    [Header("RIGHT ARROW")]
    [SerializeField, Range(0f, 1f)]
    private float rightArrowX = 0.19f;

    [SerializeField, Range(0f, 1f)]
    private float rightArrowY = 0.12f;

    [SerializeField, Range(0.005f, 0.3f)]
    private float rightArrowRadius = 0.045f;

    // ============================================================
    // 04 - JUMP BUTTON
    // ============================================================

    [Header("JUMP BUTTON")]
    [SerializeField]
    private bool showJumpWindow = false;

    [SerializeField, Range(0f, 1f)]
    private float jumpCenterX = 0.87f;

    [SerializeField, Range(0f, 1f)]
    private float jumpCenterY = 0.15f;

    [SerializeField, Range(0.005f, 0.3f)]
    private float jumpRadius = 0.055f;

    // ============================================================
    // 05 - KICK BUTTON
    // ============================================================

    [Header("KICK BUTTON")]
    [SerializeField]
    private bool showKickWindow = false;

    [SerializeField, Range(0f, 1f)]
    private float kickCenterX = 0.73f;

    [SerializeField, Range(0f, 1f)]
    private float kickCenterY = 0.13f;

    [SerializeField, Range(0.005f, 0.3f)]
    private float kickRadius = 0.05f;

    // ============================================================
    // 06 - AUTOMATIC TUTORIAL
    // ============================================================

    [Header("AUTOMATIC TUTORIAL")]
    [SerializeField]
    private bool automaticTutorialWindows = true;

    // ============================================================
    // 07 - DOOR REVEAL
    // ============================================================

    [Header("DOOR REVEAL")]
    [SerializeField]
    private PrisonBreakDoor prisonBreakDoor;

    [SerializeField]
    private TutorialTextController tutorialController;

    [SerializeField]
    private Camera worldCamera;

    [SerializeField, Min(0.1f)]
    private float doorRevealDuration = 2f;

    [Header("AUTOMATIC DOOR BOUNDS")]
    [SerializeField, Range(0f, 0.25f)]
    private float doorRevealPaddingX = 0.025f;

    [SerializeField, Range(0f, 0.25f)]
    private float doorRevealPaddingY = 0.025f;

    [SerializeField, Range(0.001f, 0.15f)]
    private float doorRevealEdgeSoftness = 0.035f;

    // ============================================================
    // 08 - DOOR WINDOW POSITION AND SIZE
    // ============================================================

    [Header("DOOR WINDOW - POSITION")]
    [SerializeField, Range(-0.5f, 0.5f)]
    private float doorRevealOffsetX = 0f;

    [SerializeField, Range(-0.5f, 0.5f)]
    private float doorRevealOffsetY = 0f;

    [Header("DOOR WINDOW - SIZE")]
    [SerializeField, Range(0.01f, 1f)]
    private float doorRevealWidth = 0.20f;

    [SerializeField, Range(0.01f, 1f)]
    private float doorRevealHeight = 0.45f;

    [SerializeField]
    private bool matchHeroWindowHeight = true;

    [SerializeField]
    private bool useManualDoorWindowSize = true;

    // ============================================================
    // 09 - PERFORMANCE
    // ============================================================

    [Header("PERFORMANCE")]
    [SerializeField, Range(64, 512)]
    private int maskResolution = 256;

    [SerializeField, Range(5, 60)]
    private int doorRevealFPS = 20;

    // ============================================================
    // 10 - AUTOMATIC BUTTON ALIGNMENT
    // ============================================================

    [Header("AUTOMATIC BUTTON ALIGNMENT")]
    [SerializeField]
    private bool followUIButtons = true;

    [Header("UI BUTTON REFERENCES")]
    [SerializeField]
    private RectTransform leftArrowButton;

    [SerializeField]
    private RectTransform rightArrowButton;

    [SerializeField]
    private RectTransform jumpButton;

    [SerializeField]
    private RectTransform kickButton;

    // ============================================================
    // 11 - BUTTON WINDOW OFFSETS
    // ============================================================

    [Header("LEFT ARROW WINDOW OFFSET")]
    [SerializeField]
    private Vector2 leftArrowFineOffset = Vector2.zero;

    [Header("RIGHT ARROW WINDOW OFFSET")]
    [SerializeField]
    private Vector2 rightArrowFineOffset = Vector2.zero;

    [Header("JUMP WINDOW OFFSET")]
    [SerializeField]
    private Vector2 jumpFineOffset = Vector2.zero;

    [Header("KICK WINDOW OFFSET")]
    [SerializeField]
    private Vector2 kickFineOffset = Vector2.zero;

    // ============================================================
    // 12 - AUTO-FIT BUTTON WINDOW SIZE
    // ============================================================

    [Header("AUTO-FIT BUTTON WINDOW SIZE")]
    [SerializeField]
    private bool autoFitWindowSize = true;

    [SerializeField, Range(0.8f, 1.6f)]
    private float windowSizeMultiplier = 1.12f;

    [SerializeField, Range(0f, 0.4f)]
    private float circleFeatherRatio = 0.12f;

    // ============================================================
    // 13 - INDIVIDUAL WINDOW SCALE
    // ============================================================

    [Header("INDIVIDUAL WINDOW SCALE")]
    [SerializeField, Range(0.5f, 1.8f)]
    private float leftWindowScale = 1f;

    [SerializeField, Range(0.5f, 1.8f)]
    private float rightWindowScale = 1f;

    [SerializeField, Range(0.5f, 1.8f)]
    private float jumpWindowScale = 1f;

    [SerializeField, Range(0.5f, 1.8f)]
    private float kickWindowScale = 1f;

    // ============================================================
    // 14 - EXACT BUTTON GRAPHICS
    // ============================================================

    [Header("OPTIONAL EXACT BUTTON GRAPHICS")]
    [SerializeField]
    private RectTransform leftArrowGraphic;

    [SerializeField]
    private RectTransform rightArrowGraphic;

    [SerializeField]
    private RectTransform jumpGraphic;

    [SerializeField]
    private RectTransform kickGraphic;

    // ============================================================
    // 15 - VISIBLE SPRITE PIXELS
    // ============================================================

    [Header("VISIBLE SPRITE PIXELS")]

    [Tooltip(
        "Find visible sprite pixels, ignoring transparent margins."
    )]
    [SerializeField]
    private bool useVisibleSpritePixels = true;

    [SerializeField, Range(0.01f, 0.5f)]
    private float visiblePixelAlphaThreshold = 0.08f;

    // ============================================================
    // 16 - ALIGNMENT DEBUG
    // ============================================================

    [Header("ALIGNMENT CHECK (TEMPORARY)")]

    [Tooltip(
        "Shows red center crosses and circle outlines."
    )]
    [SerializeField]
    private bool showWindowCenterMarkers = false;

    // ============================================================
    // 17 - FULL LEVEL REVEAL
    // ============================================================

    [Header("FULL LEVEL REVEAL AFTER DOOR BREAK")]

    [Tooltip(
        "Automatically open the whole level when the door is broken."
    )]
    [SerializeField]
    private bool automaticFullLevelReveal = true;

    [Tooltip(
        "Delay after the door breaks. Allows Yahoo celebration to finish."
    )]
    [SerializeField, Min(0f)]
    private float fullLevelRevealDelay = 0.9f;

    [Tooltip(
        "Duration of the complete darkness fade-out."
    )]
    [SerializeField, Min(0.01f)]
    private float fullLevelRevealDuration = 2f;

    // ============================================================
    // 17A - AUTOSAVE FULL LEVEL REVEAL
    // ============================================================

    [Header("AUTOSAVE - FULL LEVEL REVEAL")]

    [Tooltip(
        "If the prison door was destroyed in a previous session, " +
        "open the entire level immediately when loading the scene. " +
        "The normal first-time reveal animation is unchanged."
    )]
    [SerializeField]
    private bool restoreFullRevealFromAutosave = true;

    // ============================================================
    // 18 - KEEPER FADE SYNCHRONIZATION
    // ============================================================

    [Header("KEEPER FADE SYNCHRONIZATION")]

    [Tooltip(
        "Start hiding the vertical Keeper frame together " +
        "with the full level reveal."
    )]
    [SerializeField]
    private bool fadeKeeperWithLevel = true;

    [Tooltip(
        "Existing TutorialKeeperFade component."
    )]
    [SerializeField]
    private TutorialKeeperFade tutorialKeeperFade;

    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private RawImage maskImage;
    private RectTransform maskRect;
    private Canvas maskCanvas;

    private Texture2D maskTexture;

    private Color32[] basePixels;
    private Color32[] workingPixels;

    private int textureWidth;
    private int textureHeight;

    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    private Vector2 lastMaskSize;

    private bool doorRevealStarted;
    private bool doorRevealCompleted;

    private float doorRevealProgress;
    private Coroutine doorRevealRoutine;

    private Rect doorScreenRect;

    // Full level transition state.

    private bool fullLevelRevealStarted;
    private bool fullLevelRevealCompleted;

    private float fullLevelMaskOpacity = 1f;

    private Coroutine fullLevelRevealRoutine;

    private struct CircleWindow
    {
        public float x;
        public float y;
        public float radius;
        public bool autoSized;
    }

    private struct SpriteVisibleBounds
    {
        public Rect normalizedRect;
        public bool valid;
    }

    private CircleWindow leftWindow;
    private CircleWindow rightWindow;
    private CircleWindow jumpWindow;
    private CircleWindow kickWindow;

    private RectTransform cachedLeftGraphic;
    private RectTransform cachedRightGraphic;
    private RectTransform cachedJumpGraphic;
    private RectTransform cachedKickGraphic;

    private readonly Dictionary<Sprite, SpriteVisibleBounds>
        visibleSpriteCache =
        new Dictionary<Sprite, SpriteVisibleBounds>();

    private readonly Vector3[] corners = new Vector3[4];

    private bool warnedUnreadableTexture;

    private const float PositionTolerance = 0.0002f;

    private static readonly Rect FullUV =
        new Rect(0f, 0f, 1f, 1f);

    public bool IsDoorRevealComplete =>
        doorRevealCompleted;

    public bool IsFullLevelRevealStarted =>
        fullLevelRevealStarted;

    public bool IsFullLevelRevealComplete =>
        fullLevelRevealCompleted;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        maskImage = GetComponent<RawImage>();
        maskRect = GetComponent<RectTransform>();
        maskCanvas = GetComponentInParent<Canvas>();

        maskImage.raycastTarget = false;
        maskImage.uvRect = FullUV;

        fullLevelMaskOpacity = 1f;

        if (automaticTutorialWindows)
        {
            showMovementWindow = false;
            showJumpWindow = false;
            showKickWindow = false;
        }

        // The saved door state belongs to the previous play session.
        // We must not replay the black mask or the fade after loading.
        // Restore in Awake so other components can read the completed
        // reveal state as soon as their Start/Update methods run.
        if (restoreFullRevealFromAutosave &&
            TutorialProgressManager.DoorBroken)
        {
            RestoreFullRevealImmediatelyFromAutosave();
        }
        else if (showMaskOnStart)
        {
            RefreshMask();
        }
        else
        {
            maskImage.enabled = false;
        }
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (prisonBreakDoor == null)
        {
            prisonBreakDoor =
                FindFirstObjectByType<PrisonBreakDoor>();
        }

        if (tutorialController == null)
        {
            tutorialController =
                FindFirstObjectByType<TutorialTextController>();
        }

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (fadeKeeperWithLevel &&
            tutorialKeeperFade == null)
        {
            tutorialKeeperFade =
                FindFirstObjectByType<TutorialKeeperFade>(
                    FindObjectsInactive.Include
                );
        }

        if (followUIButtons)
            FindButtonReferences();

        CacheButtonGraphics();

        Canvas.ForceUpdateCanvases();

        // Never re-create the prison darkness for a completed save.
        if (showMaskOnStart && !fullLevelRevealCompleted)
            RefreshMask();
    }

    // ============================================================
    // 18A - RESTORE FULL REVEAL AFTER LOADING A BROKEN DOOR
    // ============================================================

    private void RestoreFullRevealImmediatelyFromAutosave()
    {
        // The door's own script restores its broken sprite and collider.
        // This component restores only the level visibility and the
        // completion flags watched by the LEFT Keeper tutorial.
        // Do not start the celebration, door-reveal or fade coroutines.
        doorRevealStarted = true;
        doorRevealCompleted = true;
        doorRevealProgress = 1f;

        fullLevelRevealStarted = true;
        fullLevelRevealCompleted = true;
        fullLevelMaskOpacity = 0f;

        if (maskImage == null)
            maskImage = GetComponent<RawImage>();

        if (maskImage != null)
        {
            ApplyMaskOpacity();
            maskImage.enabled = false;
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        // The actual broken door has priority.
        // The complete level reveal can start only once.

        if (automaticFullLevelReveal &&
            !fullLevelRevealStarted &&
            !fullLevelRevealCompleted &&
            prisonBreakDoor != null &&
            prisonBreakDoor.IsBroken)
        {
            BeginFullLevelReveal();
            return;
        }

        // Preserve the original door tutorial reveal.

        if (!doorRevealStarted &&
            !fullLevelRevealStarted &&
            !fullLevelRevealCompleted &&
            tutorialController != null &&
            tutorialController.ShouldShowTutorial() &&
            tutorialController.GetCurrentPage() >= 22 &&
            tutorialController.IsLocalizationReady())
        {
            BeginDoorReveal(prisonBreakDoor);
        }
    }

    // ============================================================
    // LATE UPDATE
    // ============================================================

    private void LateUpdate()
    {
        if (maskImage == null ||
            !maskImage.enabled ||
            fullLevelRevealCompleted)
        {
            return;
        }

        bool sizeChanged =
            Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight ||
            (maskRect.rect.size - lastMaskSize)
                .sqrMagnitude > 0.01f;

        bool windowsChanged = UpdateComputedWindows();

        if (sizeChanged || windowsChanged)
            RefreshMask();
    }

    // ============================================================
    // FIND BUTTON REFERENCES
    // ============================================================

    private void FindButtonReferences()
    {
        if (maskCanvas == null)
            return;

        RectTransform[] rects =
            maskCanvas.GetComponentsInChildren<RectTransform>(
                true
            );

        foreach (RectTransform rect in rects)
        {
            if (leftArrowButton == null &&
                rect.name == "BtnLeft_Hitbox")
            {
                leftArrowButton = rect;
            }

            if (rightArrowButton == null &&
                rect.name == "BtnRight_Hitbox")
            {
                rightArrowButton = rect;
            }

            if (jumpButton == null &&
                rect.name == "JumpButton")
            {
                jumpButton = rect;
            }

            if (kickButton == null &&
                rect.name == "ActionButton_Leg")
            {
                kickButton = rect;
            }
        }
    }

    // ============================================================
    // CACHE BUTTON GRAPHICS
    // ============================================================

    private void CacheButtonGraphics()
    {
        cachedLeftGraphic =
            ResolveGraphic(
                leftArrowButton,
                leftArrowGraphic
            );

        cachedRightGraphic =
            ResolveGraphic(
                rightArrowButton,
                rightArrowGraphic
            );

        cachedJumpGraphic =
            ResolveGraphic(
                jumpButton,
                jumpGraphic
            );

        cachedKickGraphic =
            ResolveGraphic(
                kickButton,
                kickGraphic
            );
    }

    // ============================================================
    // RESOLVE BUTTON GRAPHIC
    // ============================================================

    private RectTransform ResolveGraphic(
        RectTransform hitbox,
        RectTransform exactGraphic)
    {
        if (exactGraphic != null)
            return exactGraphic;

        if (hitbox == null)
            return null;

        Image[] images =
            hitbox.GetComponentsInChildren<Image>(true);

        RectTransform best = null;
        float bestArea = -1f;

        foreach (Image img in images)
        {
            if (img.sprite == null ||
                img.color.a <= 0.05f)
            {
                continue;
            }

            RectTransform rect = img.rectTransform;

            float area = Mathf.Abs(
                rect.rect.width *
                rect.rect.height *
                rect.lossyScale.x *
                rect.lossyScale.y
            );

            if (area > bestArea)
            {
                bestArea = area;
                best = rect;
            }
        }

        return best != null ? best : hitbox;
    }

    // ============================================================
    // GET VISIBLE SPRITE PIXEL BOUNDS
    // ============================================================

    private SpriteVisibleBounds GetVisibleSpriteBounds(
        Sprite sprite)
    {
        SpriteVisibleBounds fallback =
            new SpriteVisibleBounds
            {
                normalizedRect = FullUV,
                valid = false
            };

        if (sprite == null || !useVisibleSpritePixels)
            return fallback;

        if (visibleSpriteCache.TryGetValue(
            sprite,
            out SpriteVisibleBounds cached))
        {
            return cached;
        }

        SpriteVisibleBounds result = fallback;

        try
        {
            Texture2D texture = sprite.texture;

            if (texture == null || !texture.isReadable)
            {
                if (!warnedUnreadableTexture)
                {
                    Debug.LogWarning(
                        "PrisonDarknessMask: Button sprite " +
                        "is not Read/Write Enabled. " +
                        "Using full graphic rectangle.",
                        this
                    );

                    warnedUnreadableTexture = true;
                }
            }
            else
            {
                Rect atlasRect = sprite.textureRect;

                Vector2 trimOffset =
                    sprite.textureRectOffset;

                int ix =
                    Mathf.RoundToInt(atlasRect.x);

                int iy =
                    Mathf.RoundToInt(atlasRect.y);

                int iw =
                    Mathf.RoundToInt(atlasRect.width);

                int ih =
                    Mathf.RoundToInt(atlasRect.height);

                if (iw > 0 &&
                    ih > 0 &&
                    ix >= 0 &&
                    iy >= 0 &&
                    ix + iw <= texture.width &&
                    iy + ih <= texture.height &&
                    sprite.rect.width > 0f &&
                    sprite.rect.height > 0f)
                {
                    Color[] colors =
                        texture.GetPixels(
                            ix,
                            iy,
                            iw,
                            ih
                        );

                    int minX = iw;
                    int minY = ih;

                    int maxX = -1;
                    int maxY = -1;

                    for (int y = 0; y < ih; y++)
                    {
                        int start = y * iw;

                        for (int x = 0; x < iw; x++)
                        {
                            if (colors[start + x].a <
                                visiblePixelAlphaThreshold)
                            {
                                continue;
                            }

                            minX = Mathf.Min(minX, x);
                            minY = Mathf.Min(minY, y);

                            maxX = Mathf.Max(maxX, x);
                            maxY = Mathf.Max(maxY, y);
                        }
                    }

                    if (maxX >= minX &&
                        maxY >= minY)
                    {
                        float left = Mathf.Clamp01(
                            (trimOffset.x + minX) /
                            sprite.rect.width
                        );

                        float right = Mathf.Clamp01(
                            (trimOffset.x + maxX + 1f) /
                            sprite.rect.width
                        );

                        float bottom = Mathf.Clamp01(
                            (trimOffset.y + minY) /
                            sprite.rect.height
                        );

                        float top = Mathf.Clamp01(
                            (trimOffset.y + maxY + 1f) /
                            sprite.rect.height
                        );

                        if (right > left &&
                            top > bottom)
                        {
                            result.normalizedRect =
                                Rect.MinMaxRect(
                                    left,
                                    bottom,
                                    right,
                                    top
                                );

                            result.valid = true;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Fallback for unsupported sprite packing.
        }

        visibleSpriteCache[sprite] = result;

        return result;
    }

    // ============================================================
    // REAL VISIBLE UI GRAPHIC RECTANGLE
    // ============================================================

    private Rect GetVisualLocalRect(
        RectTransform visual)
    {
        Rect original = visual.rect;

        Image image = visual.GetComponent<Image>();

        if (image == null ||
            image.type != Image.Type.Simple)
        {
            return original;
        }

        Sprite sprite =
            image.overrideSprite != null
                ? image.overrideSprite
                : image.sprite;

        if (sprite == null)
            return original;

        Rect drawn = image.GetPixelAdjustedRect();

        if (image.preserveAspect &&
            drawn.height > 0.001f &&
            sprite.rect.width > 0f &&
            sprite.rect.height > 0f)
        {
            float spriteAspect =
                sprite.rect.width /
                sprite.rect.height;

            float areaAspect =
                drawn.width /
                drawn.height;

            if (spriteAspect > areaAspect)
            {
                float height =
                    drawn.width / spriteAspect;

                drawn.y +=
                    (drawn.height - height) * 0.5f;

                drawn.height = height;
            }
            else
            {
                float width =
                    drawn.height * spriteAspect;

                drawn.x +=
                    (drawn.width - width) * 0.5f;

                drawn.width = width;
            }
        }

        SpriteVisibleBounds opaque =
            GetVisibleSpriteBounds(sprite);

        if (!opaque.valid)
            return drawn;

        Rect normalized = opaque.normalizedRect;

        return Rect.MinMaxRect(
            drawn.xMin +
                drawn.width * normalized.xMin,

            drawn.yMin +
                drawn.height * normalized.yMin,

            drawn.xMin +
                drawn.width * normalized.xMax,

            drawn.yMin +
                drawn.height * normalized.yMax
        );
    }

    // ============================================================
    // GET EXACT BOUNDS RELATIVE TO MASK
    // ============================================================

    private bool GetMaskBounds(
        RectTransform visual,
        out Rect bounds)
    {
        bounds = new Rect();

        if (visual == null ||
            maskRect == null)
        {
            return false;
        }

        Rect maskArea = maskRect.rect;

        if (maskArea.width <= 0.001f ||
            maskArea.height <= 0.001f)
        {
            return false;
        }

        Rect r = GetVisualLocalRect(visual);

        corners[0] = visual.TransformPoint(
            new Vector3(r.xMin, r.yMin, 0f)
        );

        corners[1] = visual.TransformPoint(
            new Vector3(r.xMin, r.yMax, 0f)
        );

        corners[2] = visual.TransformPoint(
            new Vector3(r.xMax, r.yMax, 0f)
        );

        corners[3] = visual.TransformPoint(
            new Vector3(r.xMax, r.yMin, 0f)
        );

        Canvas sourceCanvas =
            visual.GetComponentInParent<Canvas>();

        bool sameCanvas =
            maskCanvas != null &&
            sourceCanvas != null &&
            maskCanvas.rootCanvas ==
                sourceCanvas.rootCanvas;

        Camera sourceCamera = null;
        Camera targetCamera = null;

        if (!sameCanvas)
        {
            if (sourceCanvas != null &&
                sourceCanvas.renderMode !=
                    RenderMode.ScreenSpaceOverlay)
            {
                sourceCamera =
                    sourceCanvas.worldCamera;

                if (sourceCamera == null)
                    sourceCamera = Camera.main;
            }

            if (maskCanvas != null &&
                maskCanvas.renderMode !=
                    RenderMode.ScreenSpaceOverlay)
            {
                targetCamera =
                    maskCanvas.worldCamera;

                if (targetCamera == null)
                    targetCamera = Camera.main;
            }
        }

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;

        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < 4; i++)
        {
            Vector2 local;

            if (sameCanvas)
            {
                Vector3 point =
                    maskRect.InverseTransformPoint(
                        corners[i]
                    );

                local = new Vector2(
                    point.x,
                    point.y
                );
            }
            else
            {
                Vector2 screen =
                    RectTransformUtility
                        .WorldToScreenPoint(
                            sourceCamera,
                            corners[i]
                        );

                if (!RectTransformUtility
                    .ScreenPointToLocalPointInRectangle(
                        maskRect,
                        screen,
                        targetCamera,
                        out local))
                {
                    return false;
                }
            }

            minX = Mathf.Min(minX, local.x);
            minY = Mathf.Min(minY, local.y);

            maxX = Mathf.Max(maxX, local.x);
            maxY = Mathf.Max(maxY, local.y);
        }

        bounds = Rect.MinMaxRect(
            minX,
            minY,
            maxX,
            maxY
        );

        return true;
    }

    // ============================================================
    // CREATE BUTTON WINDOW
    // ============================================================

    private CircleWindow MakeWindow(
        RectTransform button,
        RectTransform graphic,
        Vector2 fineOffset,
        float manualX,
        float manualY,
        float manualRadius,
        float individualScale)
    {
        CircleWindow result = new CircleWindow
        {
            x = manualX,
            y = manualY,
            radius = manualRadius,
            autoSized = false
        };

        if (!followUIButtons ||
            button == null ||
            maskRect == null)
        {
            return result;
        }

        RectTransform visual =
            graphic != null ? graphic : button;

        if (!GetMaskBounds(
            visual,
            out Rect bounds))
        {
            return result;
        }

        Rect area = maskRect.rect;

        result.x = Mathf.Clamp01(
            (bounds.center.x - area.xMin) /
            area.width +
            fineOffset.x
        );

        result.y = Mathf.Clamp01(
            (bounds.center.y - area.yMin) /
            area.height +
            fineOffset.y
        );

        if (autoFitWindowSize)
        {
            float radiusInLocalUnits =
                0.5f * Mathf.Max(
                    bounds.width,
                    bounds.height
                );

            result.radius = Mathf.Max(
                0.001f,
                radiusInLocalUnits *
                windowSizeMultiplier *
                individualScale /
                area.height
            );

            result.autoSized = true;
        }

        return result;
    }

    // ============================================================
    // CHECK WINDOW CHANGES
    // ============================================================

    private static bool Different(
        CircleWindow a,
        CircleWindow b)
    {
        return
            Mathf.Abs(a.x - b.x) > PositionTolerance ||
            Mathf.Abs(a.y - b.y) > PositionTolerance ||
            Mathf.Abs(a.radius - b.radius) >
                PositionTolerance ||
            a.autoSized != b.autoSized;
    }

    // ============================================================
    // UPDATE BUTTON WINDOWS
    // ============================================================

    private bool UpdateComputedWindows()
    {
        CircleWindow l = MakeWindow(
            leftArrowButton,
            cachedLeftGraphic,
            leftArrowFineOffset,
            leftArrowX,
            leftArrowY,
            leftArrowRadius,
            leftWindowScale
        );

        CircleWindow r = MakeWindow(
            rightArrowButton,
            cachedRightGraphic,
            rightArrowFineOffset,
            rightArrowX,
            rightArrowY,
            rightArrowRadius,
            rightWindowScale
        );

        CircleWindow j = MakeWindow(
            jumpButton,
            cachedJumpGraphic,
            jumpFineOffset,
            jumpCenterX,
            jumpCenterY,
            jumpRadius,
            jumpWindowScale
        );

        CircleWindow k = MakeWindow(
            kickButton,
            cachedKickGraphic,
            kickFineOffset,
            kickCenterX,
            kickCenterY,
            kickRadius,
            kickWindowScale
        );

        bool changed =
            Different(l, leftWindow) ||
            Different(r, rightWindow) ||
            Different(j, jumpWindow) ||
            Different(k, kickWindow);

        leftWindow = l;
        rightWindow = r;
        jumpWindow = j;
        kickWindow = k;

        return changed;
    }

    // ============================================================
    // BEGIN DOOR REVEAL
    // ============================================================

    public void BeginDoorReveal(PrisonBreakDoor door)
    {
        if (door != null)
            prisonBreakDoor = door;

        if (doorRevealStarted ||
            doorRevealCompleted ||
            fullLevelRevealStarted ||
            fullLevelRevealCompleted)
        {
            return;
        }

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (prisonBreakDoor == null ||
            worldCamera == null ||
            !UpdateDoorScreenRect())
        {
            Debug.LogWarning(
                "PrisonDarknessMask: Door, camera " +
                "or door sprite is missing.",
                this
            );

            return;
        }

        doorRevealStarted = true;
        doorRevealCompleted = false;
        doorRevealProgress = 0f;

        if (doorRevealRoutine != null)
            StopCoroutine(doorRevealRoutine);

        doorRevealRoutine =
            StartCoroutine(RevealDoorRoutine());
    }

    // ============================================================
    // DOOR REVEAL ANIMATION
    // ============================================================

    private IEnumerator RevealDoorRoutine()
    {
        float elapsed = 0f;

        float interval =
            1f / Mathf.Max(5, doorRevealFPS);

        float nextUpdate = 0f;

        while (elapsed < doorRevealDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= nextUpdate)
            {
                float t = Mathf.Clamp01(
                    elapsed / doorRevealDuration
                );

                doorRevealProgress =
                    Mathf.SmoothStep(0f, 1f, t);

                RenderDoorReveal();

                nextUpdate = elapsed + interval;
            }

            yield return null;
        }

        doorRevealProgress = 1f;

        RenderDoorReveal();

        doorRevealCompleted = true;
        doorRevealRoutine = null;
    }

    // ============================================================
    // DOOR SCREEN RECTANGLE
    // ============================================================

    private bool UpdateDoorScreenRect()
    {
        if (prisonBreakDoor == null ||
            worldCamera == null)
        {
            return false;
        }

        SpriteRenderer renderer =
            prisonBreakDoor
                .GetComponentInChildren<SpriteRenderer>();

        if (renderer == null)
            return false;

        Bounds bounds = renderer.bounds;

        Vector3[] worldPoints =
        {
            new Vector3(
                bounds.min.x,
                bounds.min.y,
                bounds.center.z
            ),

            new Vector3(
                bounds.min.x,
                bounds.max.y,
                bounds.center.z
            ),

            new Vector3(
                bounds.max.x,
                bounds.min.y,
                bounds.center.z
            ),

            new Vector3(
                bounds.max.x,
                bounds.max.y,
                bounds.center.z
            )
        };

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;

        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        foreach (Vector3 point in worldPoints)
        {
            Vector3 v =
                worldCamera.WorldToViewportPoint(point);

            if (v.z <= 0f)
                return false;

            minX = Mathf.Min(minX, v.x);
            minY = Mathf.Min(minY, v.y);

            maxX = Mathf.Max(maxX, v.x);
            maxY = Mathf.Max(maxY, v.y);
        }

        float width = useManualDoorWindowSize
            ? doorRevealWidth
            : maxX - minX +
              2f * doorRevealPaddingX;

        float height = matchHeroWindowHeight
            ? visibleHeight
            : (useManualDoorWindowSize
                ? doorRevealHeight
                : maxY - minY +
                  2f * doorRevealPaddingY);

        width = Mathf.Max(0.01f, width);
        height = Mathf.Max(0.01f, height);

        float cx =
            (minX + maxX) * 0.5f +
            doorRevealOffsetX;

        float cy =
            (minY + maxY) * 0.5f +
            doorRevealOffsetY;

        doorScreenRect = new Rect(
            cx - width * 0.5f,
            cy - height * 0.5f,
            width,
            height
        );

        return true;
    }

    // ============================================================
    // 19 - BEGIN FULL LEVEL REVEAL
    // ============================================================

    public void BeginFullLevelReveal()
    {
        if (fullLevelRevealStarted ||
            fullLevelRevealCompleted)
        {
            return;
        }

        fullLevelRevealStarted = true;

        if (fullLevelRevealRoutine != null)
        {
            StopCoroutine(fullLevelRevealRoutine);
        }

        fullLevelRevealRoutine =
            StartCoroutine(FullLevelRevealRoutine());
    }

    // ============================================================
    // 20 - FULL LEVEL REVEAL ANIMATION
    // ============================================================

    private IEnumerator FullLevelRevealRoutine()
    {
        // Wait for the hero's celebration.
        // The delay is adjustable in Inspector.

        if (fullLevelRevealDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                fullLevelRevealDelay
            );
        }

        // Start hiding the vertical Keeper presentation.

        if (fadeKeeperWithLevel)
        {
            if (tutorialKeeperFade == null)
            {
                tutorialKeeperFade =
                    FindFirstObjectByType<TutorialKeeperFade>(
                        FindObjectsInactive.Include
                    );
            }

            if (tutorialKeeperFade != null)
            {
                tutorialKeeperFade.HideSmoothly();
            }
        }

        // Fade the entire darkness overlay without
        // recalculating the texture on every frame.

        float startOpacity =
            fullLevelMaskOpacity;

        float elapsed = 0f;

        float duration = Mathf.Max(
            0.01f,
            fullLevelRevealDuration
        );

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            fullLevelMaskOpacity = Mathf.Lerp(
                startOpacity,
                0f,
                smoothProgress
            );

            ApplyMaskOpacity();

            yield return null;
        }

        fullLevelMaskOpacity = 0f;
        ApplyMaskOpacity();

        fullLevelRevealCompleted = true;

        // Do not waste rendering work after the
        // entire level has become visible.

        if (maskImage != null)
            maskImage.enabled = false;

        fullLevelRevealRoutine = null;
    }

    // ============================================================
    // 21 - APPLY MASK OPACITY
    // ============================================================

    private void ApplyMaskOpacity()
    {
        if (maskImage == null)
            return;

        // RGB stays white to preserve the texture's
        // original black color and alpha values.
        //
        // Only the final opacity is animated.

        maskImage.color = new Color(
            1f,
            1f,
            1f,
            Mathf.Clamp01(fullLevelMaskOpacity)
        );
    }

    // ============================================================
    // ENSURE TEXTURE
    // ============================================================

    private void EnsureTexture()
    {
        int resolution =
            Mathf.Clamp(maskResolution, 64, 512);

        if (maskTexture != null &&
            textureWidth == resolution &&
            textureHeight == resolution)
        {
            return;
        }

        if (maskTexture != null)
            Destroy(maskTexture);

        textureWidth = resolution;
        textureHeight = resolution;

        maskTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        maskTexture.wrapMode =
            TextureWrapMode.Clamp;

        maskTexture.filterMode =
            FilterMode.Bilinear;

        basePixels =
            new Color32[resolution * resolution];

        workingPixels =
            new Color32[resolution * resolution];

        maskImage.texture = maskTexture;

        maskImage.uvRect = FullUV;

        ApplyMaskOpacity();
    }

    // ============================================================
    // REFRESH MASK
    // ============================================================

    public void RefreshMask()
    {
        // Never recreate the darkness after
        // the level has been fully revealed.

        if (fullLevelRevealCompleted)
            return;

        if (maskImage == null)
            maskImage = GetComponent<RawImage>();

        if (maskRect == null)
            maskRect = GetComponent<RectTransform>();

        if (maskCanvas == null)
            maskCanvas = GetComponentInParent<Canvas>();

        EnsureTexture();
        UpdateComputedWindows();

        float width =
            Mathf.Max(1f, maskRect.rect.width);

        float height =
            Mathf.Max(1f, maskRect.rect.height);

        float aspect = width / height;

        lastMaskSize = maskRect.rect.size;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        for (int y = 0; y < textureHeight; y++)
        {
            float ny =
                (float)y / (textureHeight - 1);

            for (int x = 0; x < textureWidth; x++)
            {
                float nx =
                    (float)x / (textureWidth - 1);

                float alpha =
                    RectangleAlpha(nx, ny);

                if (showMovementWindow)
                {
                    alpha = Mathf.Min(
                        alpha,
                        CircleAlpha(
                            nx,
                            ny,
                            leftWindow,
                            aspect
                        )
                    );

                    alpha = Mathf.Min(
                        alpha,
                        CircleAlpha(
                            nx,
                            ny,
                            rightWindow,
                            aspect
                        )
                    );
                }

                if (showJumpWindow)
                {
                    alpha = Mathf.Min(
                        alpha,
                        CircleAlpha(
                            nx,
                            ny,
                            jumpWindow,
                            aspect
                        )
                    );
                }

                if (showKickWindow)
                {
                    alpha = Mathf.Min(
                        alpha,
                        CircleAlpha(
                            nx,
                            ny,
                            kickWindow,
                            aspect
                        )
                    );
                }

                basePixels[
                    y * textureWidth + x
                ] = new Color32(
                    0,
                    0,
                    0,
                    (byte)Mathf.RoundToInt(
                        Mathf.Clamp01(alpha) * 255f
                    )
                );
            }
        }

        if (doorRevealStarted)
        {
            UpdateDoorScreenRect();
            RenderDoorReveal();
        }
        else
        {
            Array.Copy(
                basePixels,
                workingPixels,
                basePixels.Length
            );

            DrawDebugMarkers();
            UploadPixels();
        }
    }

    // ============================================================
    // HERO RECTANGLE
    // ============================================================

    private float RectangleAlpha(
        float nx,
        float ny)
    {
        float dx =
            Mathf.Abs(nx - centerX) -
            visibleWidth * 0.5f;

        float dy =
            Mathf.Abs(ny - centerY) -
            visibleHeight * 0.5f;

        return SoftAlpha(
            Mathf.Max(dx, dy),
            edgeSoftness
        );
    }

    // ============================================================
    // BUTTON CIRCLE
    // ============================================================

    private float CircleAlpha(
        float nx,
        float ny,
        CircleWindow circle,
        float aspect)
    {
        float dx =
            (nx - circle.x) * aspect;

        float dy =
            ny - circle.y;

        float distance =
            Mathf.Sqrt(dx * dx + dy * dy) -
            circle.radius;

        float softness = circle.autoSized
            ? circle.radius * circleFeatherRatio
            : edgeSoftness;

        return SoftAlpha(
            distance,
            softness
        );
    }

    // ============================================================
    // SOFT ALPHA
    // ============================================================

    private float SoftAlpha(
        float outside,
        float softness)
    {
        if (softness < 0.0001f)
        {
            return outside > 0f
                ? darkness
                : 0f;
        }

        return Mathf.SmoothStep(
            0f,
            darkness,
            Mathf.Clamp01(outside / softness)
        );
    }

    // ============================================================
    // RENDER DOOR REVEAL
    // ============================================================

    private void RenderDoorReveal()
    {
        if (maskTexture == null ||
            basePixels == null ||
            workingPixels == null)
        {
            return;
        }

        Array.Copy(
            basePixels,
            workingPixels,
            basePixels.Length
        );

        float softness = Mathf.Max(
            0.001f,
            doorRevealEdgeSoftness
        );

        float minX = doorScreenRect.xMin;
        float maxX = doorScreenRect.xMax;

        float minY = doorScreenRect.yMin;
        float maxY = doorScreenRect.yMax;

        int startX = Mathf.Clamp(
            Mathf.FloorToInt(
                (minX - softness) *
                (textureWidth - 1)
            ),
            0,
            textureWidth - 1
        );

        int endX = Mathf.Clamp(
            Mathf.CeilToInt(
                (maxX + softness) *
                (textureWidth - 1)
            ),
            0,
            textureWidth - 1
        );

        int startY = Mathf.Clamp(
            Mathf.FloorToInt(
                (minY - softness) *
                (textureHeight - 1)
            ),
            0,
            textureHeight - 1
        );

        int endY = Mathf.Clamp(
            Mathf.CeilToInt(
                (maxY + softness) *
                (textureHeight - 1)
            ),
            0,
            textureHeight - 1
        );

        for (int y = startY; y <= endY; y++)
        {
            float ny =
                (float)y / (textureHeight - 1);

            for (int x = startX; x <= endX; x++)
            {
                float nx =
                    (float)x / (textureWidth - 1);

                float dx = Mathf.Max(
                    minX - nx,
                    nx - maxX
                );

                float dy = Mathf.Max(
                    minY - ny,
                    ny - maxY
                );

                float outside =
                    Mathf.Max(dx, dy);

                float opening =
                    1f - Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.Clamp01(
                            outside / softness
                        )
                    );

                float targetAlpha =
                    darkness *
                    (1f - opening * doorRevealProgress);

                int index =
                    y * textureWidth + x;

                byte value =
                    (byte)Mathf.RoundToInt(
                        Mathf.Clamp01(targetAlpha) *
                        255f
                    );

                if (value < workingPixels[index].a)
                {
                    workingPixels[index] =
                        new Color32(
                            0,
                            0,
                            0,
                            value
                        );
                }
            }
        }

        DrawDebugMarkers();
        UploadPixels();
    }

    // ============================================================
    // DEBUG: SHOW EXACT WINDOW CENTERS
    // ============================================================

    private void DrawDebugMarkers()
    {
        if (!showWindowCenterMarkers)
            return;

        if (showMovementWindow)
        {
            DrawMarker(leftWindow);
            DrawMarker(rightWindow);
        }

        if (showJumpWindow)
            DrawMarker(jumpWindow);

        if (showKickWindow)
            DrawMarker(kickWindow);
    }

    private void DrawMarker(
        CircleWindow window)
    {
        int px = Mathf.RoundToInt(
            window.x * (textureWidth - 1)
        );

        int py = Mathf.RoundToInt(
            window.y * (textureHeight - 1)
        );

        for (int d = -2; d <= 2; d++)
        {
            SetMarkerPixel(px + d, py);
            SetMarkerPixel(px, py + d);
        }

        int radius = Mathf.RoundToInt(
            window.radius * (textureHeight - 1)
        );

        for (int step = 0; step < 96; step++)
        {
            float angle =
                step * Mathf.PI * 2f / 96f;

            int ox = Mathf.RoundToInt(
                Mathf.Cos(angle) * radius
            );

            int oy = Mathf.RoundToInt(
                Mathf.Sin(angle) * radius
            );

            SetMarkerPixel(
                px + ox,
                py + oy
            );
        }
    }

    private void SetMarkerPixel(
        int x,
        int y)
    {
        if (x < 0 ||
            y < 0 ||
            x >= textureWidth ||
            y >= textureHeight)
        {
            return;
        }

        workingPixels[
            y * textureWidth + x
        ] = new Color32(
            255,
            30,
            30,
            255
        );
    }

    // ============================================================
    // UPLOAD TEXTURE
    // ============================================================

    private void UploadPixels()
    {
        if (maskTexture == null ||
            maskImage == null ||
            fullLevelRevealCompleted)
        {
            return;
        }

        maskTexture.SetPixels32(workingPixels);
        maskTexture.Apply(false, false);

        maskImage.texture = maskTexture;
        maskImage.uvRect = FullUV;

        // IMPORTANT:
        // Preserve the current fade opacity.
        // Never reset it to Color.white during transition.

        ApplyMaskOpacity();

        maskImage.enabled = true;
    }

    // ============================================================
    // PUBLIC MASK CONTROLS
    // ============================================================

    public void ShowMask()
    {
        if (fullLevelRevealCompleted)
            return;

        RefreshMask();
    }

    public void HideMask()
    {
        if (maskImage == null)
            maskImage = GetComponent<RawImage>();

        maskImage.enabled = false;
    }

    public void ShowMovementWindow()
    {
        showMovementWindow = true;
        RefreshMask();
    }

    public void HideMovementWindow()
    {
        showMovementWindow = false;
        RefreshMask();
    }

    public void ShowJumpWindow()
    {
        showJumpWindow = true;
        RefreshMask();
    }

    public void HideJumpWindow()
    {
        showJumpWindow = false;
        RefreshMask();
    }

    public void ShowKickWindow()
    {
        showKickWindow = true;
        RefreshMask();
    }

    public void HideKickWindow()
    {
        showKickWindow = false;
        RefreshMask();
    }

    public void HideAllButtonWindows()
    {
        showMovementWindow = false;
        showJumpWindow = false;
        showKickWindow = false;

        RefreshMask();
    }

    // ============================================================
    // AUTOMATIC TUTORIAL STAGES
    // ============================================================

    public void SetTutorialStage(int stage)
    {
        if (!automaticTutorialWindows)
            return;

        bool movement = stage >= 1;
        bool jump = stage >= 2;
        bool kick = stage >= 3;

        if (showMovementWindow == movement &&
            showJumpWindow == jump &&
            showKickWindow == kick)
        {
            return;
        }

        showMovementWindow = movement;
        showJumpWindow = jump;
        showKickWindow = kick;

        RefreshMask();
    }

    public void UnlockMovementWindows()
    {
        if (!showMovementWindow)
        {
            showMovementWindow = true;
            RefreshMask();
        }
    }

    public void UnlockJumpWindow()
    {
        if (!showJumpWindow)
        {
            showJumpWindow = true;
            RefreshMask();
        }
    }

    public void UnlockKickWindow()
    {
        if (!showKickWindow)
        {
            showKickWindow = true;
            RefreshMask();
        }
    }

    // ============================================================
    // HERO WINDOW POSITION
    // ============================================================

    public void SetHeroWindowCenter(
        float x,
        float y)
    {
        x = Mathf.Clamp01(x);
        y = Mathf.Clamp01(y);

        if (Mathf.Abs(centerX - x) <=
                PositionTolerance &&
            Mathf.Abs(centerY - y) <=
                PositionTolerance)
        {
            return;
        }

        centerX = x;
        centerY = y;

        RefreshMask();
    }

    // ============================================================
    // INSPECTOR UPDATE
    // ============================================================

    private void OnValidate()
    {
        fullLevelRevealDelay =
            Mathf.Max(0f, fullLevelRevealDelay);

        fullLevelRevealDuration =
            Mathf.Max(0.01f, fullLevelRevealDuration);

        if (Application.isPlaying &&
            maskImage != null &&
            maskImage.enabled)
        {
            visibleSpriteCache.Clear();

            CacheButtonGraphics();

            RefreshMask();
        }
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        visibleSpriteCache.Clear();

        if (maskTexture != null)
        {
            Destroy(maskTexture);
            maskTexture = null;
        }
    }
}
