
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PrisonRevealSound : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField]
    private PrisonDarknessMask darknessMask;

    [SerializeField]
    private RawImage darknessImage;

    [SerializeField]
    private AudioSource audioSource;

    [Header("REVEAL SOUND")]
    [SerializeField]
    private AudioClip revealSound;

    [SerializeField, Range(0f, 1f)]
    private float volume = 0.8f;

    [Header("DOOR REVEAL - TUTORIAL 22")]
    [SerializeField] private bool enableDoorRevealSound = true;
    [SerializeField] private AudioClip doorRevealSound;
    [SerializeField, Range(0f, 1f)] private float doorRevealVolume = 0.8f;
    [SerializeField, Min(0f)] private float doorRevealSoundDelay;

    [Header("TRIGGER")]
    [SerializeField, Range(0.5f, 1f)]
    private float fadeStartThreshold = 0.999f;

    [Header("DEBUG")]
    [SerializeField]
    private bool debugLogs = false;

    private bool soundPlayed;
    private bool doorRevealTriggered;

    private void Awake()
    {
        if (darknessMask == null)
            darknessMask = GetComponent<PrisonDarknessMask>();

        if (darknessImage == null)
            darknessImage = GetComponent<RawImage>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }

    public void PlayDoorReveal()
    {
        if (doorRevealTriggered)
            return;

        doorRevealTriggered = true;
        if (!isActiveAndEnabled || !enableDoorRevealSound || audioSource == null || doorRevealSound == null)
            return;

        StartCoroutine(PlayDoorRevealAfterDelay());
    }

    private IEnumerator PlayDoorRevealAfterDelay()
    {
        if (doorRevealSoundDelay > 0f || PauseManager.IsMenuPaused)
            yield return PauseManager.WaitForTutorialSeconds(doorRevealSoundDelay);

        if (isActiveAndEnabled && enableDoorRevealSound)
            audioSource.PlayOneShot(doorRevealSound, doorRevealVolume);
    }

    private void Update()
    {
        if (soundPlayed)
            return;

        if (darknessMask == null ||
            darknessImage == null ||
            audioSource == null ||
            revealSound == null)
        {
            return;
        }

        // Do not replay the sound when loading
        // a save where the prison door is already broken.

        if (darknessMask.IsFullLevelRevealComplete)
            return;

        // The full reveal has not yet been triggered.
        if (!darknessMask.IsFullLevelRevealStarted)
            return;

        // Wait until the darkness actually starts fading.
        if (!darknessImage.enabled)
            return;

        if (darknessImage.color.a >= fadeStartThreshold)
            return;

        soundPlayed = true;

        audioSource.PlayOneShot(revealSound, volume);

        if (debugLogs)
        {
            Debug.Log(
                "PrisonRevealSound: Full level reveal sound played.",
                this
            );
        }
    }
}
