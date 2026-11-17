
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
            playButtonAnimator.SetTrigger("Pressed");

        StartCoroutine(
            LoadGameRoutine(true, playClip)
        );
    }

    // =========================================
    // CONTINUE
    // =========================================

    public void ContinueGame()
    {
        if (isTransitioning)
            return;

        if (!GameInstallationSave.CanContinue())
        {
            Debug.LogWarning(
                "[MainMenu] Continue blocked: " +
                "no valid save for this installation."
            );

            return;
        }

        isTransitioning = true;

        if (continueButtonAnimator != null)
            continueButtonAnimator.SetTrigger("Pressed");

        AudioClip sound = continueClip != null
            ? continueClip
            : playClip;

        StartCoroutine(
            LoadGameRoutine(false, sound)
        );
    }

    // =========================================
    // LOAD GAME
    // =========================================

    private IEnumerator LoadGameRoutine(
        bool startNewGame,
        AudioClip sound)
    {
        if (audioSource != null && sound != null)
            audioSource.PlayOneShot(sound);

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
            TutorialProgressManager.ResetAllProgress();

            ResetPuzzleFinaleForNewGame();

            // Register a new playthrough
            // for the current installation.
            GameInstallationSave.MarkNewGameStarted();

            Debug.Log(
                "[MainMenu] Starting new game."
            );
        }
        else
        {
            Debug.Log(
                "[MainMenu] Continuing saved game."
            );
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(firstLevelScene);
    }

    private static void ResetPuzzleFinaleForNewGame()
    {
        PlayerPrefs.DeleteKey(PuzzleFinaleKeyV1);
        PlayerPrefs.DeleteKey(PuzzleFinaleKeyV2);

        PlayerPrefs.Save();

        Debug.Log(
            "[MainMenu] Puzzle finale reset."
        );
    }

    // =========================================
    // QUIT
    // =========================================

    public void QuitGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        if (quitButtonAnimator != null)
            quitButtonAnimator.SetTrigger("Pressed");

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
