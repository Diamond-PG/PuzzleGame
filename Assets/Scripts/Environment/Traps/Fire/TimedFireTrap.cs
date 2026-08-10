using System.Collections;
using UnityEngine;

public class TimedFireTrap : MonoBehaviour
{
    [Header("Fire Visuals")]
    [SerializeField] private GameObject[] fireVisuals;

    [Header("Damage")]
    [SerializeField] private GameObject damageZone;

    [Header("Timing")]
    [SerializeField] private float fireOnDuration = 2.5f;
    [SerializeField] private float fireOffDuration = 2.5f;
    [SerializeField] private float startDelay = 0f;

    [Header("Start State")]
    [SerializeField] private bool startWithFireOn = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Короткий звук включения огня.")]
    [SerializeField] private AudioClip fireStartSound;

    [Tooltip("Короткий звук выключения огня.")]
    [SerializeField] private AudioClip fireStopSound;

    [Range(0f, 1f)]
    [SerializeField] private float startVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float stopVolume = 0.8f;

    private Coroutine fireRoutine;

    private bool fireIsOn;
    private bool playerNear;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        fireRoutine = StartCoroutine(FireCycle());
    }

    private void OnDisable()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }

        fireIsOn = false;
        playerNear = false;

        SetVisualState(false);

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator FireCycle()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        fireIsOn = startWithFireOn;

        while (true)
        {
            if (fireIsOn)
            {
                TurnFireOn();

                yield return new WaitForSeconds(
                    Mathf.Max(0.05f, fireOnDuration)
                );
            }
            else
            {
                TurnFireOff();

                yield return new WaitForSeconds(
                    Mathf.Max(0.05f, fireOffDuration)
                );
            }

            fireIsOn = !fireIsOn;
        }
    }

    private void TurnFireOn()
    {
        SetVisualState(true);

        if (playerNear)
        {
            PlayStartSound();
        }
    }

    private void TurnFireOff()
    {
        SetVisualState(false);

        if (playerNear)
        {
            PlayStopSound();
        }
    }

    private void SetVisualState(bool isOn)
    {
        if (fireVisuals != null)
        {
            for (int i = 0; i < fireVisuals.Length; i++)
            {
                if (fireVisuals[i] != null)
                {
                    fireVisuals[i].SetActive(isOn);
                }
            }
        }

        if (damageZone != null)
        {
            damageZone.SetActive(isOn);
        }
    }

    public void SetPlayerNear(bool isNear)
    {
        playerNear = isNear;

        if (!playerNear)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            return;
        }

        // Если игрок подошёл, когда огонь уже горит,
        // мы не ждём следующего цикла — даём ему услышать огонь.
        if (fireIsOn)
        {
            PlayStartSound();
        }
    }

    private void PlayStartSound()
    {
        if (audioSource == null)
            return;

        if (fireStartSound == null)
            return;

        audioSource.PlayOneShot(
            fireStartSound,
            startVolume
        );
    }

    private void PlayStopSound()
    {
        if (audioSource == null)
            return;

        if (fireStopSound == null)
            return;

        audioSource.PlayOneShot(
            fireStopSound,
            stopVolume
        );
    }
}