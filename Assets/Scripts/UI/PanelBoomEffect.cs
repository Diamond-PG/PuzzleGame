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
    [SerializeField] private bool pauseWithMenu;

    private Coroutine boomRoutine;
    private bool boomPlayed;
    private Vector3 restingScale = Vector3.one;
    private float multiplier = 1f;

    internal float PeakScale => Mathf.Max(1f, startScale, boomScale, overshootScale);

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
            if (pauseWithMenu)
                yield return PauseManager.WaitForTutorialSeconds(delay);
            else
                yield return new WaitForSecondsRealtime(delay);
        }

        float t = 0f;

        // Фаза 1: маленькая → большая (взрыв)
        while (t < boomDuration)
        {
            t += DeltaTime;
            float k = t / boomDuration;
            SetMultiplier(Mathf.Lerp(startScale, boomScale, k));
            yield return null;
        }

        t = 0f;

        // Фаза 2: большая → чуть меньше нормы (отскок)
        if (overshootDuration > 0f)
        {
            while (t < overshootDuration)
            {
                t += DeltaTime;
                float k = t / overshootDuration;
                SetMultiplier(Mathf.Lerp(boomScale, overshootScale, k));
                yield return null;
            }
        }

        t = 0f;

        // Фаза 3: чуть меньше нормы → нормальный размер
        while (t < overshootDuration)
        {
            t += DeltaTime;
            float k = t / overshootDuration;
            SetMultiplier(Mathf.Lerp(overshootScale, 1f, k));
            yield return null;
        }

        SetMultiplier(1f);
    }

    private float DeltaTime => pauseWithMenu ? PauseManager.TutorialDeltaTime : Time.unscaledDeltaTime;

    internal void SetRestingScale(Vector3 scale)
    {
        restingScale = scale;
        SetMultiplier(multiplier);
    }

    private void SetMultiplier(float value)
    {
        multiplier = value;
        target.localScale = restingScale * value;
    }
}
