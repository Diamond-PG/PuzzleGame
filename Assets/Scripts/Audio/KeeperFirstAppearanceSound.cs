
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class KeeperFirstAppearanceSound : MonoBehaviour
{
    [Header("KEEPER REFERENCES")]
    [SerializeField]
    private TutorialTextController tutorialController;

    [SerializeField]
    private GameObject keeperVisual;

    [SerializeField]
    private GameObject tutorialTextObject;

    [Header("FIRST APPEARANCE")]
    [Tooltip("First page of the original Keeper tutorial.")]
    [SerializeField, Min(0)]
    private int firstTutorialPage = 0;

    [Header("APPEARANCE AUDIO")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip appearanceSound;

    [SerializeField, Range(0f, 1f)]
    private float appearanceVolume = 0.8f;

    [SerializeField, Min(0f)]
    private float appearanceDelay = 0f;

    [Header("DEBUG")]
    [SerializeField]
    private bool debugLogs = false;

    private bool played;
    private Coroutine playRoutine;

    private void Update()
    {
        if (played || playRoutine != null)
            return;

        if (!IsFirstAppearanceReady())
            return;

        playRoutine = StartCoroutine(
            PlayFirstAppearanceRoutine()
        );
    }

    private bool IsFirstAppearanceReady()
    {
        if (tutorialController == null)
            return false;

        if (!tutorialController.ShouldShowTutorial())
            return false;

        if (!tutorialController.IsLocalizationReady())
            return false;

        if (tutorialController.GetCurrentPage() !=
            firstTutorialPage)
            return false;

        if (keeperVisual == null ||
            !keeperVisual.activeInHierarchy)
            return false;

        if (tutorialTextObject == null ||
            !tutorialTextObject.activeInHierarchy)
            return false;

        return true;
    }

    private IEnumerator PlayFirstAppearanceRoutine()
    {
        if (appearanceDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                appearanceDelay
            );
        }

        if (IsFirstAppearanceReady() &&
            audioSource != null &&
            appearanceSound != null)
        {
            audioSource.PlayOneShot(
                appearanceSound,
                appearanceVolume
            );

            played = true;

            if (debugLogs)
            {
                Debug.Log(
                    "[KEEPER] First appearance sound played.",
                    this
                );
            }
        }

        playRoutine = null;
    }

    private void OnDisable()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }

    private void OnValidate()
    {
        appearanceVolume =
            Mathf.Clamp01(appearanceVolume);

        appearanceDelay =
            Mathf.Max(0f, appearanceDelay);
    }
}
