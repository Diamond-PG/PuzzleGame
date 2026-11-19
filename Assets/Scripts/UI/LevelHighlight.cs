using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RawImage))]
public sealed class LevelHighlight : MonoBehaviour
{
    private static readonly int ShaderTime = Shader.PropertyToID("_ROLPLevelHighlightTime");
    private static readonly int ContourWidth = Shader.PropertyToID("_LineWidth");
    private static readonly int GlowSize = Shader.PropertyToID("_GlowSize");

    private RawImage graphic;
    private Material lastRenderMaterial;
    private float lastContourWidth;
    private float lastGlowSize;

    // Matches the padding of the distance-field texture, relative to the sampled frame.
    [SerializeField] private Vector2 contourPadding = new Vector2(24f / 384f, 24f / 367f);

    internal void Show(RectTransform frame)
    {
        if (frame == null)
        {
            gameObject.SetActive(false);
            return;
        }

        var rect = (RectTransform)transform;
        if (rect.parent != frame)
        {
            rect.SetParent(frame, false);
            rect.SetAsFirstSibling();
            rect.anchorMin = -contourPadding;
            rect.anchorMax = Vector2.one + contourPadding;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        graphic = GetComponent<RawImage>();
        lastRenderMaterial = null;
    }

    private void LateUpdate()
    {
        Shader.SetGlobalFloat(ShaderTime, Time.unscaledTime);

        var renderer = graphic.canvasRenderer;
        if (renderer.materialCount == 0)
            return;

        var source = graphic.material;
        var rendered = renderer.GetMaterial(0);
        if (rendered == null || rendered == source)
        {
            lastRenderMaterial = null;
            return;
        }

        // UI Mask can render with a cached stencil copy of the editable material.
        float width = source.GetFloat(ContourWidth);
        float glow = source.GetFloat(GlowSize);
        if (rendered == lastRenderMaterial && width == lastContourWidth && glow == lastGlowSize)
            return;

        rendered.SetFloat(ContourWidth, width);
        rendered.SetFloat(GlowSize, glow);
        lastRenderMaterial = rendered;
        lastContourWidth = width;
        lastGlowSize = glow;
    }
}
