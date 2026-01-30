using System.Collections;           // добавили это
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private AudioSource audioSource;
public Animator playButtonAnimator;
public Animator quitButtonAnimator;
    [Header("Button Sounds")]
    public AudioClip playClip;
    public AudioClip quitClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayGame()
    {
       if (playButtonAnimator != null)
    {
        playButtonAnimator.SetTrigger("Pressed");
    }
  // запускаем корутину: сначала звук, потом загрузка сцены
        StartCoroutine(PlayAndLoad());
    }

    private IEnumerator PlayAndLoad()
    {
        if (audioSource != null && playClip != null)
        {
            audioSource.PlayOneShot(playClip);
        }

        // подожди 0.25 секунды, чтобы звук был слышен
        yield return new WaitForSeconds(0.25f);

        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
{
    if (quitButtonAnimator != null)
        quitButtonAnimator.SetTrigger("Pressed");

    StartCoroutine(QuitRoutine());
}

private IEnumerator QuitRoutine()
{
    // 1) играем щелчок
    if (audioSource != null && quitClip != null)
    {
        audioSource.PlayOneShot(quitClip);

        // ждём сколько длится звук (реальное время, не зависит от Time.timeScale)
        yield return new WaitForSecondsRealtime(quitClip.length);
    }
    else
    {
        // запасной вариант, если клип/аудио не назначены
        yield return new WaitForSecondsRealtime(0.25f);
    }

    // 2) выходим
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
}
}