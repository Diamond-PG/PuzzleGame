using UnityEngine;

public class UIClickSfx : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickClip;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    // Длина клика (чтобы PauseManager мог подождать перед сменой сцены)
    public float ClickLength => clickClip != null ? clickClip.length : 0f;

    public void PlayClick()
    {
        if (!audioSource || !clickClip) return;

        // PlayOneShot не обрезает звук при частых нажатиях (в пределах одного AudioSource)
        audioSource.PlayOneShot(clickClip, volume);
    }
}