using System.Collections;
using UnityEngine;

public class ConstantFireSound : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Постоянный звук горения огня.")]
    [SerializeField] private AudioClip fireLoopSound;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.6f;

    [Header("Loop")]
    [Tooltip("За сколько секунд до конца звук запускается заново. Убирает паузу между повторами.")]
    [SerializeField] private float overlapTime = 0.08f;

    private bool playerNear;
    private Coroutine loopRoutine;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = volume;
        }
    }

    public void SetPlayerNear(bool isNear)
    {
        playerNear = isNear;

        if (playerNear)
        {
            StartFireSound();
        }
        else
        {
            StopFireSound();
        }
    }

    private void StartFireSound()
    {
        if (audioSource == null)
            return;

        if (fireLoopSound == null)
            return;

        if (loopRoutine != null)
            return;

        loopRoutine = StartCoroutine(FireLoopRoutine());
    }

    private IEnumerator FireLoopRoutine()
    {
        while (playerNear)
        {
            audioSource.clip = fireLoopSound;
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.Play();

            float waitTime =
                Mathf.Max(
                    0.05f,
                    fireLoopSound.length - overlapTime
                );

            yield return new WaitForSeconds(waitTime);
        }

        loopRoutine = null;
    }

    private void StopFireSound()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void OnDisable()
    {
        playerNear = false;
        StopFireSound();
    }
}