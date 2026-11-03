
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class KeeperFinalFarewellSound : MonoBehaviour
{
    [Header("FAREWELL - TUTORIAL 54")]
    [SerializeField]
    private string completedDialogueSaveKey =
        "TutorialPuzzleFinale_Completed";

    [Header("FAREWELL AUDIO")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip farewellSound;

    [SerializeField, Range(0f, 1f)]
    private float farewellVolume = 0.8f;

    [SerializeField, Range(0.1f, 3f)]
    private float farewellPitch = 1f;

    [SerializeField, Min(0f)]
    private float soundDelay = 0f;

    [Header("DEBUG")]
    [SerializeField]
    private bool debugLogs = false;

    private bool wasCompleted;
    private bool soundTriggered;
    private Coroutine soundRoutine;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // A dialogue completed in an earlier playthrough
        // must not trigger its sound on scene loading.
        wasCompleted = IsDialogueCompleted();
    }

    private void Update()
    {
        if (soundTriggered)
            return;

        bool completedNow = IsDialogueCompleted();

        if (!wasCompleted && completedNow)
        {
            soundTriggered = true;

            soundRoutine = StartCoroutine(
                PlayFarewellSoundRoutine()
            );
        }

        wasCompleted = completedNow;
    }

    private bool IsDialogueCompleted()
    {
        return PlayerPrefs.GetInt(
            completedDialogueSaveKey, 0
        ) == 1;
    }

    private IEnumerator PlayFarewellSoundRoutine()
    {
        if (soundDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                soundDelay
            );
        }

        if (audioSource != null && farewellSound != null)
        {
            audioSource.pitch = farewellPitch;

            audioSource.PlayOneShot(
                farewellSound,
                farewellVolume
            );

            if (debugLogs)
            {
                Debug.Log(
                    "[KEEPER FAREWELL] Sound played.",
                    this
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[KEEPER FAREWELL] " +
                "AudioSource or Farewell Sound is missing.",
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
        farewellVolume =
            Mathf.Clamp01(farewellVolume);

        farewellPitch =
            Mathf.Clamp(farewellPitch, 0.1f, 3f);

        soundDelay =
            Mathf.Max(0f, soundDelay);
    }
}
