
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GalleryLevelsBackgroundAdapter : MonoBehaviour
{
    [Header("Background Images")]
    [SerializeField] private Image nearBackground;
    [SerializeField] private Image farBackground;

    [Header("Screen Adaptation")]
    [SerializeField, Range(1f, 2.5f)]
    private float aspectRatioThreshold = 1.7f;

    [Header("Display")]
    [SerializeField] private bool useNearOnWideScreens = true;

    private RectTransform container;
    private float previousWidth = -1f;
    private float previousHeight = -1f;

    private void OnEnable()
    {
        container = GetComponent<RectTransform>();
        RefreshBackground();
    }

    private void OnRectTransformDimensionsChange()
    {
        RefreshBackground();
    }

    private void Update()
    {
        if (container == null)
            container = GetComponent<RectTransform>();

        if (container == null)
            return;

        Vector2 size = container.rect.size;

        if (!Mathf.Approximately(size.x, previousWidth) ||
            !Mathf.Approximately(size.y, previousHeight))
        {
            RefreshBackground();
        }
    }

    private void OnValidate()
    {
        container = GetComponent<RectTransform>();
        RefreshBackground();
    }

    private void RefreshBackground()
    {
        if (container == null)
            container = GetComponent<RectTransform>();

        if (container == null)
            return;

        float width = container.rect.width;
        float height = container.rect.height;

        if (width <= 0f || height <= 0f)
            return;

        previousWidth = width;
        previousHeight = height;

        float aspectRatio = width / height;
        bool wideScreen = aspectRatio >= aspectRatioThreshold;

        bool showNear = useNearOnWideScreens
            ? wideScreen
            : !wideScreen;

        SetBackground(nearBackground, showNear);
        SetBackground(farBackground, !showNear);
    }

    private void SetBackground(Image image, bool visible)
    {
        if (image == null)
            return;

        image.enabled = visible;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }
}
