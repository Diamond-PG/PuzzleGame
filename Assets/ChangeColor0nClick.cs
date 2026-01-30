using UnityEngine;

public class ChangeColorOnClick : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // При запуске игры ставим случайный цвет
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = RandomColor();
        }
    }

    private void Update()
    {
        // ЛКМ нажата в этот кадр?
        if (Input.GetMouseButtonDown(0))
        {
            // Переводим позицию мыши в координаты мира
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point2D = new Vector2(worldPos.x, worldPos.y);

            // Пускаем луч в точку клика
            RaycastHit2D hit = Physics2D.Raycast(point2D, Vector2.zero);

            // Если мы попали в коллайдер ЭТОГО объекта
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                _spriteRenderer.color = RandomColor();
            }
        }
    }

    private Color RandomColor()
    {
        return new Color(
            Random.value,
            Random.value,
            Random.value
        );
    }
}