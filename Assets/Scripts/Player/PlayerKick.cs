using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerVisual playerVisual;

    [Header("Kick Timing")]
    [SerializeField] private float kickDuration = 0.20f;
    [SerializeField] private float kickCooldown = 0.25f;

    [Header("Movement")]
    [SerializeField] private bool stopHorizontalMovement = true;
    [SerializeField] private bool lockMovementDuringKick = true;

    [Header("Kick Voice")]
    [SerializeField] private AudioSource kickAudioSource;
    [SerializeField] private AudioClip kickVoiceSound;

    [Range(0f, 1f)]
    [SerializeField] private float kickVoiceVolume = 1f;

    [Tooltip("Через сколько секунд после начала удара прозвучит голос.")]
    [SerializeField] private float kickVoiceDelay = 0f;

    [Header("Temporary PC Test")]
    [SerializeField] private bool enableKeyboardTest = true;

    private bool isKicking;
    private bool facingRight = true;

    private float nextKickTime;

    private Coroutine kickRoutine;
    private Coroutine voiceRoutine;

    public bool IsKicking => isKicking;
    public bool FacingRight => facingRight;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerVisual == null)
            playerVisual = GetComponent<PlayerVisual>();

        if (kickAudioSource == null)
        {
            AudioSource[] sources =
                GetComponents<AudioSource>();

            if (sources.Length > 0)
            {
                kickAudioSource =
                    sources[sources.Length - 1];
            }
        }
    }

    private void Update()
    {
        if (!isKicking)
            UpdateFacingDirection();

        if (enableKeyboardTest)
            HandleKeyboardTest();
    }

    private void HandleKeyboardTest()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            KickRight();

        if (Keyboard.current.qKey.wasPressedThisFrame)
            KickLeft();
    }

    private void UpdateFacingDirection()
    {
        if (rb == null)
            return;

        float horizontalSpeed =
            rb.linearVelocity.x;

        if (horizontalSpeed > 0.05f)
            facingRight = true;
        else if (horizontalSpeed < -0.05f)
            facingRight = false;
    }

    public bool Kick()
    {
        return StartKick(facingRight);
    }

    public bool KickRight()
    {
        return StartKick(true);
    }

    public bool KickLeft()
    {
        return StartKick(false);
    }

    public bool KickToward(Vector3 worldPosition)
    {
        bool kickToRight =
            worldPosition.x >=
            transform.position.x;

        return StartKick(kickToRight);
    }

    private bool StartKick(bool kickToRight)
    {
        if (isKicking)
            return false;

        if (Time.time < nextKickTime)
            return false;

        if (playerVisual == null)
            return false;

        facingRight = kickToRight;

        kickRoutine =
            StartCoroutine(
                KickRoutine(kickToRight)
            );

        return true;
    }

    private IEnumerator KickRoutine(bool kickToRight)
    {
        isKicking = true;

        nextKickTime =
            Time.time +
            kickCooldown;

        if (stopHorizontalMovement &&
            rb != null)
        {
            Vector2 velocity =
                rb.linearVelocity;

            velocity.x = 0f;

            rb.linearVelocity =
                velocity;
        }

        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled = false;
        }

        if (kickToRight)
            playerVisual.PlayKickRight();
        else
            playerVisual.PlayKickLeft();

        PlayKickVoice();

        yield return new WaitForSeconds(
            kickDuration
        );

        playerVisual.EndKick();

        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled = true;
        }

        isKicking = false;
        kickRoutine = null;
    }

    private void PlayKickVoice()
    {
        if (kickAudioSource == null)
            return;

        if (kickVoiceSound == null)
            return;

        if (voiceRoutine != null)
        {
            StopCoroutine(voiceRoutine);
            voiceRoutine = null;
        }

        voiceRoutine =
            StartCoroutine(
                KickVoiceRoutine()
            );
    }

    private IEnumerator KickVoiceRoutine()
    {
        if (kickVoiceDelay > 0f)
        {
            yield return new WaitForSeconds(
                kickVoiceDelay
            );
        }

        if (kickAudioSource != null &&
            kickVoiceSound != null)
        {
            kickAudioSource.PlayOneShot(
                kickVoiceSound,
                kickVoiceVolume
            );
        }

        voiceRoutine = null;
    }

    private void OnDisable()
    {
        if (kickRoutine != null)
        {
            StopCoroutine(kickRoutine);
            kickRoutine = null;
        }

        if (voiceRoutine != null)
        {
            StopCoroutine(voiceRoutine);
            voiceRoutine = null;
        }

        if (playerVisual != null)
            playerVisual.EndKick();

        if (lockMovementDuringKick &&
            playerController != null)
        {
            playerController.enabled = true;
        }

        isKicking = false;
    }
}