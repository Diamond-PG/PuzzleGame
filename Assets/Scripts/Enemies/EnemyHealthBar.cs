using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new(0f, 0.8f);
    [SerializeField, Min(0.01f)] private float width = 0.72f;
    [SerializeField, Min(0.01f)] private float height = 0.095f;
    [SerializeField] private Color fillColor = new(0.95f, 0.38f, 0.22f, 1f);
    [SerializeField] private Color backgroundColor = new(0.16f, 0.075f, 0.06f, 1f);
    [SerializeField, Min(0.01f)] private float fillSpeed = 2.5f;
    [SerializeField, Min(0f), Tooltip("Seconds after a hit. Zero keeps the bar visible until death.")]
    private float visibleSeconds;

    private static Sprite rectangle;
    private Transform bar;
    private Transform fill;
    private float displayed = 1f;
    private float target = 1f;
    private float hideAt;
    private float facing;

    public void ShowDamage(float remainingFraction, SpriteRenderer enemySprite)
    {
        target = Mathf.Clamp01(remainingFraction);
        if (target <= 0f)
        {
            if (bar != null)
                bar.gameObject.SetActive(false);
            return;
        }

        if (!isActiveAndEnabled)
            return;

        if (bar == null)
            CreateBar(enemySprite);

        hideAt = Time.time + visibleSeconds;
        UpdateFacing();
        bar.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (bar == null || !bar.gameObject.activeSelf)
            return;

        if (visibleSeconds > 0f && Time.time >= hideAt)
        {
            SetFill(target);
            bar.gameObject.SetActive(false);
            return;
        }

        UpdateFacing();
        if (displayed != target)
            SetFill(Mathf.MoveTowards(displayed, target, fillSpeed * Time.deltaTime));
    }

    private void OnDisable()
    {
        if (bar == null)
            return;

        SetFill(target);
        bar.gameObject.SetActive(false);
    }

    private void CreateBar(SpriteRenderer enemySprite)
    {
        if (rectangle == null)
        {
            Texture2D texture = Texture2D.whiteTexture;
            rectangle = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.width);
            rectangle.name = "Enemy health rectangle";
            rectangle.hideFlags = HideFlags.HideAndDontSave;
        }

        bar = new GameObject("Enemy Health Bar").transform;
        bar.SetParent(transform, false);
        int sortingLayer = enemySprite != null ? enemySprite.sortingLayerID : 0;
        int sortingOrder = enemySprite != null ? enemySprite.sortingOrder + 1 : 1;

        Transform background = AddStrip("Background", backgroundColor, sortingLayer, sortingOrder);
        background.localScale = new Vector3(width, height, 1f);
        fill = AddStrip("Fill", fillColor, sortingLayer, sortingOrder + 1);
        SetFill(displayed);
    }

    private Transform AddStrip(string stripName, Color color, int sortingLayer, int sortingOrder)
    {
        var strip = new GameObject(stripName);
        strip.layer = gameObject.layer;
        strip.transform.SetParent(bar, false);
        var spriteRenderer = strip.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = rectangle;
        spriteRenderer.color = color;
        spriteRenderer.sortingLayerID = sortingLayer;
        spriteRenderer.sortingOrder = sortingOrder;
        return strip.transform;
    }

    private void UpdateFacing()
    {
        float newFacing = transform.TransformVector(Vector3.right).x < 0f ? -1f : 1f;
        if (newFacing == facing)
            return;

        facing = newFacing;
        bar.localPosition = new Vector3(offset.x * facing, offset.y, 0f);
        bar.localScale = new Vector3(facing, 1f, 1f);
    }

    private void SetFill(float fraction)
    {
        displayed = fraction;
        float inset = Mathf.Min(height, width) * 0.15f;
        float innerWidth = width - 2f * inset;
        fill.localScale = new Vector3(innerWidth * fraction, height - 2f * inset, 1f);
        fill.localPosition = new Vector3(innerWidth * (fraction - 1f) * 0.5f, 0f, 0f);
    }
}
