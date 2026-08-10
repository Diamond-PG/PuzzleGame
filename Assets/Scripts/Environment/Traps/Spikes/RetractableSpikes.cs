using System.Collections;
using UnityEngine;

public class RetractableSpikes : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Transform spikesVisual;
    [SerializeField] private GameObject damageZone;

    [Header("Spike Positions")]
    [SerializeField] private float hiddenY = -1.60f;
    [SerializeField] private float openY = 0.58f;

    [Header("Timing")]
    [Tooltip("Небольшая задержка после того, как игрок активировал ловушку.")]
    [SerializeField] private float warningDelay = 0.03f;

    [Tooltip("Время быстрого выхода шипов.")]
    [SerializeField] private float extendDuration = 0.14f;

    [Tooltip("Сколько шипы остаются полностью открытыми.")]
    [SerializeField] private float openDuration = 0.85f;

    [Tooltip("Время ухода шипов обратно.")]
    [SerializeField] private float retractDuration = 0.22f;

    [Tooltip("Через сколько ловушка сможет сработать снова.")]
    [SerializeField] private float cooldown = 0.35f;

    [Header("Movement")]
    [Tooltip("Добавляет более резкий эффект выстрела.")]
    [SerializeField] private bool useEaseOut = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Звук выдвижения шипов.")]
    [SerializeField] private AudioClip extendSound;

    [Tooltip("Звук втягивания шипов.")]
    [SerializeField] private AudioClip retractSound;

    [Range(0f, 1f)]
    [SerializeField] private float extendVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float retractVolume = 0.8f;

    private bool isRunning;
    private bool playerInsideTrigger;

    private Vector3 hiddenPosition;
    private Vector3 openPosition;

    private Coroutine trapRoutine;

    private void Awake()
    {
        if (spikesVisual == null)
        {
            Transform found = transform.Find("SpikesVisual");

            if (found != null)
                spikesVisual = found;
        }

        if (damageZone == null)
        {
            Transform found = transform.Find("DamageZone");

            if (found != null)
                damageZone = found.gameObject;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (spikesVisual != null)
        {
            hiddenPosition = spikesVisual.localPosition;
            hiddenPosition.y = hiddenY;

            openPosition = spikesVisual.localPosition;
            openPosition.y = openY;

            spikesVisual.localPosition = hiddenPosition;
        }

        if (damageZone != null)
        {
            damageZone.SetActive(false);
        }
    }

    public void PlayerEnteredTrigger()
    {
        playerInsideTrigger = true;

        if (isRunning)
            return;

        trapRoutine = StartCoroutine(TrapSequence());
    }

    public void PlayerExitedTrigger()
    {
        playerInsideTrigger = false;
    }

    private IEnumerator TrapSequence()
    {
        isRunning = true;

        if (warningDelay > 0f)
        {
            yield return new WaitForSeconds(warningDelay);
        }

        PlayExtendSound();

        yield return MoveSpikes(
            hiddenPosition,
            openPosition,
            extendDuration,
            true
        );

        if (damageZone != null)
        {
            damageZone.SetActive(true);
        }

        if (openDuration > 0f)
        {
            yield return new WaitForSeconds(openDuration);
        }

        if (damageZone != null)
        {
            damageZone.SetActive(false);
        }

        PlayRetractSound();

        yield return MoveSpikes(
            openPosition,
            hiddenPosition,
            retractDuration,
            false
        );

        if (cooldown > 0f)
        {
            yield return new WaitForSeconds(cooldown);
        }

        isRunning = false;
        trapRoutine = null;

        if (playerInsideTrigger)
        {
            trapRoutine = StartCoroutine(TrapSequence());
        }
    }

    private IEnumerator MoveSpikes(
        Vector3 from,
        Vector3 to,
        float duration,
        bool extending
    )
    {
        if (spikesVisual == null)
            yield break;

        if (duration <= 0f)
        {
            spikesVisual.localPosition = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            float movementT;

            if (useEaseOut && extending)
            {
                movementT =
                    1f -
                    Mathf.Pow(
                        1f - t,
                        3f
                    );
            }
            else
            {
                movementT =
                    t * t *
                    (3f - 2f * t);
            }

            spikesVisual.localPosition =
                Vector3.LerpUnclamped(
                    from,
                    to,
                    movementT
                );

            yield return null;
        }

        spikesVisual.localPosition = to;
    }

    private void PlayExtendSound()
    {
        if (audioSource == null)
            return;

        if (extendSound == null)
            return;

        audioSource.PlayOneShot(
            extendSound,
            extendVolume
        );
    }

    private void PlayRetractSound()
    {
        if (audioSource == null)
            return;

        if (retractSound == null)
            return;

        audioSource.PlayOneShot(
            retractSound,
            retractVolume
        );
    }

    private void OnDisable()
    {
        if (trapRoutine != null)
        {
            StopCoroutine(trapRoutine);
            trapRoutine = null;
        }

        isRunning = false;
        playerInsideTrigger = false;

        if (damageZone != null)
        {
            damageZone.SetActive(false);
        }

        if (spikesVisual != null)
        {
            spikesVisual.localPosition =
                hiddenPosition;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}