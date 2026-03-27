using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(Image))]
public class UICover : MonoBehaviour
{
    RectTransform rt;
    RectTransform parent;
    Image img;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        parent = rt.parent as RectTransform;
        Apply();
    }

    void OnRectTransformDimensionsChange() => Apply();

    void Apply()
    {
        if (parent == null || img == null || img.sprite == null) return;

        // Делаем якоря по центру (так sizeDelta работает предсказуемо)
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        float pw = parent.rect.width;
        float ph = parent.rect.height;

        float iw = img.sprite.rect.width;
        float ih = img.sprite.rect.height;

        float screen = pw / ph;
        float image = iw / ih;

        // COVER: увеличиваем так, чтобы закрыть экран полностью
        float targetW, targetH;
        if (screen > image)
        {
            targetW = pw;
            targetH = pw / image;
        }
        else
        {
            targetH = ph;
            targetW = ph * image;
        }

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetW);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);
        rt.localScale = Vector3.one;
    }
}