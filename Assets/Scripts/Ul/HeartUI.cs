using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Heart Images (ВАЖНО: слева направо!)")]
    [SerializeField] private Image[] heartImages;

    [Header("Blink Heart")]
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkInterval = 0.25f;

    private int shownHp;
    private Coroutine blinkRoutine;
    private bool isBlinking;

    private void Awake()
    {
        // если не назначили в инспекторе — пробуем собрать автоматически из детей (слева направо по иерархии)
        if (heartImages == null || heartImages.Length == 0)
            heartImages = GetComponentsInChildren<Image>(true);

        // по умолчанию показываем все (пока PlayerHealth не скажет иначе)
        shownHp = heartImages != null ? heartImages.Length : 0;
        Draw(shownHp);
    }

    // вызывается из PlayerHealth при старте и при изменениях
    public void SetHearts(int hp)
    {
        if (isBlinking) return;

        shownHp = Mathf.Clamp(hp, 0, heartImages.Length);
        Draw(shownHp);
    }

    // вызывается из PlayerHealth при уроне (lostIndex = prev - 1)
    public void BlinkAndHide(int lostIndex)
    {
        if (heartImages == null || heartImages.Length == 0) return;
        if (lostIndex < 0 || lostIndex >= heartImages.Length) return;

        // стопаем предыдущую анимацию, чтобы не путалось
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkAndHideRoutine(lostIndex));
    }

    private IEnumerator BlinkAndHideRoutine(int index)
    {
        isBlinking = true;

        Image img = heartImages[index];
        if (img == null)
        {
            isBlinking = false;
            yield break;
        }

        // фиксируем UI в текущем состоянии (чтобы "теряемое" сердце точно было видно)
        Draw(shownHp);

        // делаем видимым на старте
        SetAlpha(img, 1f);

        for (int i = 0; i < blinkCount; i++)
        {
            SetAlpha(img, 0f);
            yield return new WaitForSeconds(blinkInterval);

            SetAlpha(img, 1f);
            yield return new WaitForSeconds(blinkInterval);
        }

        // финально скрываем
        SetAlpha(img, 0f);

        isBlinking = false;
        blinkRoutine = null;
    }

    private void Draw(int hp)
    {
        if (heartImages == null) return;

        hp = Mathf.Clamp(hp, 0, heartImages.Length);

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            // i < hp => видно, иначе прозрачное (место не двигается)
            SetAlpha(heartImages[i], (i < hp) ? 1f : 0f);
        }
    }

    private void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}