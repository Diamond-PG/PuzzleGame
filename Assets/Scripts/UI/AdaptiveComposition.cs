using System;
using UnityEngine;

namespace Bakvand.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
    public sealed class AdaptiveComposition : MonoBehaviour
    {
        public enum ScaleMode { Contain, Cover, FitWidth }

        [Serializable]
        private sealed class Layer
        {
            public RectTransform target;
            public ScaleMode scaleMode;
            public bool useSafeArea = true;
            [Tooltip("Optional uniform increase for Contain layers, reached at approximately 22:9 for a 16:9 reference."), Range(0f, 0.2f)]
            public float wideScaleBoost;
            [Tooltip("Alignment at the full wide-screen scale boost.")]
            public Vector2 wideAlignment = new Vector2(0.5f, 0.5f);
            [Tooltip("Optional native layout size. Leave both dimensions at zero to use the composition reference.")]
            public Vector2 layoutSize;
            [Tooltip("Area available to this layer, normalized from the lower-left of its viewport.")]
            public Rect viewportRegion = new Rect(0f, 0f, 1f, 1f);
            [Tooltip("Minimum gap above the viewport bottom, as a fraction of viewport width.")]
            [Range(0f, 0.5f)] public float bottomClearanceByWidth;
            [Tooltip("For Cover, the artwork region to retain as far as the available aspect ratio permits.")]
            public Rect protectedArtwork = new Rect(0f, 0f, 1f, 1f);
            [Tooltip("For FitWidth, move this fraction of the excess height below the viewport."), Range(0f, 0.5f)]
            public float oversizeBottomCrop;
            [Tooltip("Extra distance to lower a FitWidth layer, expressed as a fraction of the viewport width."), Min(0f)]
            public float bottomBleedByWidth;
            [Tooltip("Maximum fraction of the artwork height that may be hidden below the viewport."), Range(0f, 1f)]
            public float maxBottomCrop = 1f;
            [Tooltip("Optional maximum fraction of the available width, by viewport aspect ratio.")]
            public AnimationCurve maxWidthByAspect;
            [Tooltip("0 aligns left/bottom, 1 aligns right/top.")]
            public Vector2 alignment = new Vector2(0.5f, 0.5f);
        }

        [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField, Min(0f)] private float safePadding = 16f;
        [SerializeField] private Layer[] layers = Array.Empty<Layer>();

        private RectTransform viewport;
        private Canvas rootCanvas;
        private Rect previousViewport;
        private Rect previousSafeArea;
        private bool hasLayout;

        private void OnEnable()
        {
            viewport = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            rootCanvas = canvas != null ? canvas.rootCanvas : null;
            hasLayout = false;
            Refresh();
        }

        private void OnValidate() => hasLayout = false;

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            Rect safeArea = viewport.rect;
            if (rootCanvas != null && rootCanvas.renderMode != RenderMode.WorldSpace)
            {
                Camera camera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null : rootCanvas.worldCamera;
                Rect screenSafeArea = Screen.safeArea;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        viewport, screenSafeArea.min, camera, out Vector2 min) &&
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        viewport, screenSafeArea.max, camera, out Vector2 max))
                {
                    safeArea = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
                }
            }

            if (!hasLayout || viewport.rect != previousViewport || safeArea != previousSafeArea)
                ApplyLayout(safeArea);
        }

        /// <summary>Applies an available area expressed in the viewport's local coordinates.</summary>
        public void ApplyLayout(Rect localSafeArea)
        {
            Rect bounds = ((RectTransform)transform).rect;
            if (bounds.width <= 0f || bounds.height <= 0f)
                return;

            Vector2 reference = Vector2.Max(referenceResolution, Vector2.one);
            Rect safe = Rect.MinMaxRect(
                Mathf.Max(bounds.xMin, localSafeArea.xMin), Mathf.Max(bounds.yMin, localSafeArea.yMin),
                Mathf.Min(bounds.xMax, localSafeArea.xMax), Mathf.Min(bounds.yMax, localSafeArea.yMax));
            if (safe.width <= 0f || safe.height <= 0f)
                safe = bounds;

            float padding = Mathf.Min(safePadding * Mathf.Min(safe.width / reference.x, safe.height / reference.y),
                Mathf.Min(safe.width, safe.height) * 0.49f);
            safe = Rect.MinMaxRect(safe.xMin + padding, safe.yMin + padding, safe.xMax - padding, safe.yMax - padding);

            foreach (Layer layer in layers)
            {
                if (layer.target == null)
                    continue;

                Rect viewportArea = layer.useSafeArea ? safe : bounds;
                Rect region = Normalize(layer.viewportRegion);
                if (layer.maxWidthByAspect != null && layer.maxWidthByAspect.length > 0)
                {
                    float width = Mathf.Clamp(layer.maxWidthByAspect.Evaluate(bounds.width / bounds.height),
                        0.01f, region.width);
                    region.x += (region.width - width) * 0.5f;
                    region.width = width;
                }
                Rect area = Rect.MinMaxRect(
                    viewportArea.xMin + viewportArea.width * region.xMin,
                    viewportArea.yMin + viewportArea.height * region.yMin,
                    viewportArea.xMin + viewportArea.width * region.xMax,
                    viewportArea.yMin + viewportArea.height * region.yMax);
                area.yMin = Mathf.Clamp(
                    Mathf.Max(area.yMin, bounds.yMin + bounds.width * layer.bottomClearanceByWidth),
                    viewportArea.yMin, area.yMax - 0.01f);

                Vector2 fitSize = layer.layoutSize.x > 0f && layer.layoutSize.y > 0f
                    ? layer.layoutSize : reference;
                float widthRatio = area.width / fitSize.x;
                float heightRatio = area.height / fitSize.y;
                float scale = layer.scaleMode switch
                {
                    ScaleMode.Cover => Mathf.Max(widthRatio, heightRatio),
                    ScaleMode.FitWidth => widthRatio,
                    _ => Mathf.Min(widthRatio, heightRatio)
                };

                Vector2 requestedAlignment = layer.alignment;
                if (layer.scaleMode == ScaleMode.Contain && layer.wideScaleBoost > 0f)
                {
                    float wideBlend = Mathf.Clamp01((widthRatio / heightRatio - 1f) / 0.375f);
                    scale = Mathf.Min(widthRatio, scale * (1f + layer.wideScaleBoost * wideBlend));
                    requestedAlignment = Vector2.Lerp(layer.alignment, layer.wideAlignment, wideBlend);
                }

                Vector2 alignment = new Vector2(Mathf.Clamp01(requestedAlignment.x), Mathf.Clamp01(requestedAlignment.y));
                if (layer.scaleMode == ScaleMode.Cover)
                {
                    Rect priority = Normalize(layer.protectedArtwork);
                    alignment.x = PreserveArtwork(alignment.x, widthRatio / scale, priority.xMin, priority.xMax);
                    alignment.y = PreserveArtwork(alignment.y, heightRatio / scale, priority.yMin, priority.yMax);
                }
                Vector2 slack = area.size - fitSize * scale;
                if (layer.scaleMode == ScaleMode.FitWidth && slack.y < 0f)
                    alignment.y = Mathf.Clamp01(alignment.y + layer.oversizeBottomCrop);
                Vector2 center = area.center + Vector2.Scale(slack, alignment - Vector2.one * 0.5f);
                if (layer.scaleMode == ScaleMode.FitWidth && layer.bottomBleedByWidth > 0f)
                {
                    float existingCrop = Mathf.Max(0f, bounds.yMin - (center.y - fitSize.y * scale * 0.5f));
                    float crop = Mathf.Min(existingCrop + bounds.width * layer.bottomBleedByWidth,
                        fitSize.y * scale * layer.maxBottomCrop);
                    center.y += existingCrop - crop;
                }
                layer.target.sizeDelta = fitSize;
                layer.target.anchoredPosition = center - bounds.center;
                layer.target.localScale = new Vector3(scale, scale, 1f);
            }

            previousViewport = bounds;
            previousSafeArea = localSafeArea;
            hasLayout = true;
        }

        private static Rect Normalize(Rect region)
        {
            float left = Mathf.Clamp01(region.xMin);
            float bottom = Mathf.Clamp01(region.yMin);
            float right = Mathf.Clamp01(region.xMax);
            float top = Mathf.Clamp01(region.yMax);
            return right > left && top > bottom
                ? Rect.MinMaxRect(left, bottom, right, top)
                : new Rect(0f, 0f, 1f, 1f);
        }

        private static float PreserveArtwork(float alignment, float visible, float start, float end)
        {
            float range = 1f - Mathf.Clamp01(visible);
            if (range <= 0.0001f)
                return alignment;

            // If the preferred region cannot fit, retain as much of it as the crop permits.
            float first = Mathf.Min(start, end - visible);
            float last = Mathf.Max(start, end - visible);
            return Mathf.Clamp(Mathf.Clamp(alignment * range, first, last) / range, 0f, 1f);
        }
    }
}
