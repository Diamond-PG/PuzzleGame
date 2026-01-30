using System.Collections;
using UnityEngine;

public class PanelBoomEffect : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Boom Settings")]
    [SerializeField] private float startScale = 0.8f;    // начальный размер (меньше 1)
    [SerializeField] private float boomScale = 1.2f;     // основной "взрыв" (> 1)
    [SerializeField] private float boomDuration = 0.18f; // длительность фазы взрыва

    [Header("Overshoot (отскок)")]
    [SerializeField] private float overshootScale = 0.97f;    // чуть меньше 1
    [SerializeField] private float overshootDuration = 0.10f; // длительность отскока

    [Header("Timing")]
    [SerializeField] private float delay = 0f; // задержка перед эффектом (для текста можем сделать > 0)

    [Header("Sound (опционально)")]
    [SerializeField] private AudioSource audioSource;

    private Coroutine boomRoutine;
    private bool boomPlayed;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        boomPlayed = false;

        // если CanvasGroup нет — запускаем сразу
        if (canvasGroup == null)
        {
            StartBoom();
        }
    }

    private void Update()
    {
        if (canvasGroup == null) return;

        // ждём, пока панель реально начинает появляться (alpha > 0)
        if (!boomPlayed && canvasGroup.alpha > 0.01f)
        {
            boomPlayed = true;
            StartBoom();
        }
    }

    private void StartBoom()
    {
        if (boomRoutine != null)
            StopCoroutine(boomRoutine);

        boomRoutine = StartCoroutine(Boom());

        // звук по желанию
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private IEnumerator Boom()
    {
        if (target == null)
            yield break;

        // задержка перед эффектом (для текста можно делать delay > 0)
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        Vector3 normalScale = Vector3.one;
        Vector3 s0 = normalScale * startScale;       // начальный размер (меньше 1)
        Vector3 s1 = normalScale * boomScale;        // "взрыв" (больше 1)
        Vector3 s2 = normalScale * overshootScale;   // отскок (чуть меньше 1)

        float t = 0f;

        // Фаза 1: маленькая → большая (взрыв)
        while (t < boomDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / boomDuration;
            target.localScale = Vector3.Lerp(s0, s1, k);
            yield return null;
        }

        t = 0f;

        // Фаза 2: большая → чуть меньше нормы (отскок)
        if (overshootDuration > 0f)
        {
            while (t < overshootDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / overshootDuration;
                target.localScale = Vector3.Lerp(s1, s2, k);
                yield return null;
            }
        }

        t = 0f;

        // Фаза 3: чуть меньше нормы → нормальный размер
        while (t < overshootDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / overshootDuration;
            target.localScale = Vector3.Lerp(s2, normalScale, k);
            yield return null;
        }

        target.localScale = normalScale;
    }
}