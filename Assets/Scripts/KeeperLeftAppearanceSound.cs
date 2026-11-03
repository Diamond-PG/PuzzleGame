
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class KeeperLeftAppearanceSound : MonoBehaviour
{
    [Header("LEFT KEEPER REFERENCES")]
    [SerializeField]
    private TutorialLeftFrameReveal leftFrameReveal;

    [SerializeField]
    private CanvasGroup leftFrameCanvasGroup;

    [Header("APPEARANCE SOUND")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip appearanceSound;

    [SerializeField, Range(0f, 1f)]
    private float appearanceVolume = 0.8f;

    [SerializeField, Range(0.1f, 3f)]
    private float appearancePitch = 1f;

    [SerializeField, Min(0f)]
    private float soundDelay = 0f;

    [Header("TRIGGER")]
    [Tooltip("The sound starts when the left frame begins fading in.")]
    [SerializeField, Range(0f, 1f)]
    private float triggerAlpha = 0.02f;

    [Header("DEBUG")]
    [SerializeField]
    private bool debugLogs = false;

    private bool soundTriggered;
    private Coroutine soundRoutine;

    private void Awake()
    {
        if (leftFrameReveal == null)
        {
            leftFrameReveal =
                GetComponent<TutorialLeftFrameReveal>();
        }

        if (leftFrameCanvasGroup == null)
        {
            leftFrameCanvasGroup =
                GetComponent<CanvasGroup>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (soundTriggered || soundRoutine != null)
            return;

        if (leftFrameReveal == null ||
            leftFrameCanvasGroup == null)
            return;

        if (!leftFrameReveal.IsAppearanceStarted)
            return;

        if (leftFrameReveal.IsAppearanceFinished)
            return;

        if (leftFrameCanvasGroup.alpha < triggerAlpha)
            return;

        soundTriggered = true;

        soundRoutine = StartCoroutine(
            PlayAppearanceSound()
        );
    }

    private IEnumerator PlayAppearanceSound()
    {
        if (soundDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                soundDelay
            );
        }

        if (audioSource != null && appearanceSound != null)
        {
            audioSource.pitch = appearancePitch;

            audioSource.PlayOneShot(
                appearanceSound,
                appearanceVolume
            );

            if (debugLogs)
            {
                Debug.Log(
                    "[KEEPER LEFT] Appearance sound played.",
                    this
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[KEEPER LEFT] Missing AudioSource or AudioClip.",
                this
            );
        }

        soundRoutine = null;
    }

    private void OnDisable()
    {
        if (soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
            soundRoutine = null;
        }
    }

    private void OnValidate()
    {
        appearanceVolume =
            Mathf.Clamp01(appearanceVolume);

        appearancePitch =
            Mathf.Clamp(appearancePitch, 0.1f, 3f);

        soundDelay =
            Mathf.Max(0f, soundDelay);

        triggerAlpha =
            Mathf.Clamp01(triggerAlpha);
    }
}
