
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Button Animators")]
    public Animator playButtonAnimator;
    public Animator continueButtonAnimator;
    public Animator quitButtonAnimator;

    [Header("Button Sounds")]
    public AudioClip playClip;
    public AudioClip continueClip;
    public AudioClip quitClip;

    [Header("Game Settings")]
    [SerializeField]
    private string firstLevelScene = "Level1";

    [SerializeField, Min(0f)]
    private float playLoadDelay = 0.25f;

    // Separate save flags for the final Keeper dialogue.
    private const string PuzzleFinaleKeyV1 =
        "TutorialPuzzleFinale_Completed";

    private const string PuzzleFinaleKeyV2 =
        "TutorialPuzzleFinale_Completed_V2";

    private AudioSource audioSource;
    private bool isTransitioning;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // =========================================
    // NEW GAME
    // =========================================

    // Keep existing Inspector connection.
    public void PlayGame()
    {
        NewGame();
    }

    public void NewGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        if (playButtonAnimator != null)
        {
            playButtonAnimator.SetTrigger("Pressed");
        }

        StartCoroutine(
            LoadGameRoutine(true, playClip)
        );
    }

    // =========================================
    // CONTINUE GAME
    // =========================================

    public void ContinueGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        if (continueButtonAnimator != null)
        {
            continueButtonAnimator.SetTrigger("Pressed");
        }

        AudioClip sound = continueClip != null
            ? continueClip
            : playClip;

        StartCoroutine(
            LoadGameRoutine(false, sound)
        );
    }

    // =========================================
    // LOAD LEVEL
    // =========================================

    private IEnumerator LoadGameRoutine(
        bool startNewGame,
        AudioClip sound)
    {
        if (audioSource != null && sound != null)
        {
            audioSource.PlayOneShot(sound);
        }

        yield return new WaitForSecondsRealtime(
            playLoadDelay
        );

        if (!Application.CanStreamedLevelBeLoaded(
            firstLevelScene))
        {
            Debug.LogError(
                "[MainMenu] Scene not found: " +
                firstLevelScene
            );

            isTransitioning = false;
            yield break;
        }

        if (startNewGame)
        {
            // Reset the existing tutorial progress.
            TutorialProgressManager.ResetAllProgress();

            // Also reset the final Keeper dialogue.
            ResetPuzzleFinaleForNewGame();

            Debug.Log(
                "[MainMenu] New Game: tutorial and " +
                "puzzle finale progress reset."
            );
        }
        else
        {
            // Preserve all existing tutorial progress
            // and final Keeper dialogue completion.
            Debug.Log(
                "[MainMenu] Continue Game: " +
                "saved progress preserved."
            );
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(firstLevelScene);
    }

    // =========================================
    // RESET FINAL KEEPER DIALOGUE
    // =========================================

    private static void ResetPuzzleFinaleForNewGame()
    {
        bool wasCompletedV1 =
            PlayerPrefs.GetInt(PuzzleFinaleKeyV1, 0) == 1;

        bool wasCompletedV2 =
            PlayerPrefs.GetInt(PuzzleFinaleKeyV2, 0) == 1;

        PlayerPrefs.DeleteKey(PuzzleFinaleKeyV1);
        PlayerPrefs.DeleteKey(PuzzleFinaleKeyV2);

        PlayerPrefs.Save();

        Debug.Log(
            "[MainMenu] Puzzle finale reset. " +
            "Previous completion flags: " +
            "V1=" + wasCompletedV1 +
            ", V2=" + wasCompletedV2
        );
    }

    // =========================================
    // QUIT GAME
    // =========================================

    public void QuitGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        if (quitButtonAnimator != null)
        {
            quitButtonAnimator.SetTrigger("Pressed");
        }

        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        if (audioSource != null && quitClip != null)
        {
            audioSource.PlayOneShot(quitClip);

            yield return new WaitForSecondsRealtime(
                quitClip.length
            );
        }
        else
        {
            yield return new WaitForSecondsRealtime(
                0.25f
            );
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
