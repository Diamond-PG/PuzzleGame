using UnityEngine;
using System.Collections;

public class QuitButtonController : MonoBehaviour
{
    private Animator animator;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnQuitButtonClicked()
    {
        StartCoroutine(QuitSequence());
    }

    private IEnumerator QuitSequence()
    {
        // 1️⃣ Анимация
        if (animator != null)
        {
            animator.SetTrigger("QuitPressed");
        }

        // 2️⃣ Звук
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 3️⃣ Ждём, пока звук проиграется
        yield return new WaitForSeconds(0.3f);

        // 4️⃣ Выход
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}