
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public class PlayerPuzzleWow : MonoBehaviour
{
    // ============================================================
    // 01 - REFERENCES
    // ============================================================

    [Header("01 - REFERENCES")]

    [SerializeField]
    private PuzzlePiecePickup firstPuzzlePiece;

    [SerializeField]
    private PlayerVisual playerVisual;

    [SerializeField]
    private SpriteRenderer playerSpriteRenderer;


    // ============================================================
    // 02 - SURPRISED PLAYER
    // ============================================================

    [Header("02 - SURPRISED PLAYER")]

    [Tooltip("Surprised player without a weapon.")]
    [SerializeField]
    private Sprite surprisedNoWeaponSprite;

    [Tooltip("Surprised player holding the club.")]
    [SerializeField]
    private Sprite surprisedClubSprite;

    [Tooltip("Delay after the first puzzle piece is picked up.")]
    [SerializeField, Min(0f)]
    private float surpriseDelay = 2.15f;

    [Tooltip("Duration of the surprised pose.")]
    [SerializeField, Min(0.01f)]
    private float surpriseDuration = 0.85f;


    // ============================================================
    // 03 - WOW SOUND
    // ============================================================

    [Header("03 - WOW SOUND")]

    [SerializeField]
    private AudioSource wowAudioSource;

    [SerializeField]
    private AudioClip wowSound;

    [SerializeField, Range(0f, 1f)]
    private float wowVolume = 0.8f;

    [SerializeField, Range(0.1f, 3f)]
    private float wowPitch = 1f;

    [SerializeField, Min(0f)]
    private float wowSoundDelay = 0f;


    // ============================================================
    // 04 - DEBUG
    // ============================================================

    [Header("04 - DEBUG")]

    [SerializeField]
    private bool debugLogs = false;


    // ============================================================
    // PRIVATE STATE
    // ============================================================

    private bool previousPickedUp;
    private bool reactionStarted;
    private bool showingSurprise;

    private Sprite activeSurpriseSprite;

    private Coroutine reactionRoutine;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (playerVisual == null)
        {
            playerVisual =
                GetComponent<PlayerVisual>();
        }

        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (wowAudioSource == null)
        {
            wowAudioSource =
                GetComponent<AudioSource>();
        }
    }


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (firstPuzzlePiece != null)
        {
            previousPickedUp =
                firstPuzzlePiece.IsPickedUp;
        }
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (reactionStarted ||
            firstPuzzlePiece == null)
        {
            return;
        }

        bool pickedUpNow =
            firstPuzzlePiece.IsPickedUp;

        if (pickedUpNow && !previousPickedUp)
        {
            reactionStarted = true;

            reactionRoutine =
                StartCoroutine(
                    PlaySurpriseRoutine()
                );
        }

        previousPickedUp = pickedUpNow;
    }


    // ============================================================
    // LATE UPDATE - SURPRISE OVERRIDE
    // ============================================================

    private void LateUpdate()
    {
        if (!showingSurprise)
        {
            return;
        }

        if (playerSpriteRenderer == null ||
            activeSurpriseSprite == null)
        {
            return;
        }

        // PlayerVisual handles normal animations.
        // We override only the brief puzzle reaction.
        playerSpriteRenderer.sprite =
            activeSurpriseSprite;
    }


    // ============================================================
    // SURPRISE ROUTINE
    // ============================================================

    private IEnumerator PlaySurpriseRoutine()
    {
        if (surpriseDelay > 0f)
        {
            yield return new WaitForSeconds(
                surpriseDelay
            );
        }

        // Choose the correct sprite according to
        // the weapon currently equipped.
        bool clubEquipped =
            playerVisual != null &&
            playerVisual.IsClubEquipped;

        activeSurpriseSprite =
            clubEquipped
                ? surprisedClubSprite
                : surprisedNoWeaponSprite;

        // If the matching sprite has not been
        // assigned, use the available alternative.
        if (activeSurpriseSprite == null)
        {
            activeSurpriseSprite =
                clubEquipped
                    ? surprisedNoWeaponSprite
                    : surprisedClubSprite;
        }

        if (activeSurpriseSprite != null)
        {
            showingSurprise = true;

            playerSpriteRenderer.sprite =
                activeSurpriseSprite;
        }
        else if (debugLogs)
        {
            Debug.LogWarning(
                "[PUZZLE WOW] No surprise sprite assigned.",
                this
            );
        }

        if (debugLogs)
        {
            Debug.Log(
                "[PUZZLE WOW] Surprise started. " +
                "Club equipped: " + clubEquipped,
                this
            );
        }

        // Play WOW independently from the sprite.
        if (wowSoundDelay > 0f)
        {
            yield return new WaitForSeconds(
                wowSoundDelay
            );
        }

        if (wowAudioSource != null &&
            wowSound != null)
        {
            wowAudioSource.pitch =
                wowPitch;

            wowAudioSource.PlayOneShot(
                wowSound,
                wowVolume
            );
        }
        else if (debugLogs)
        {
            Debug.LogWarning(
                "[PUZZLE WOW] AudioSource or WOW clip missing.",
                this
            );
        }

        // Total pose duration is measured from
        // the beginning of the reaction.
        float remainingDuration =
            Mathf.Max(
                0f,
                surpriseDuration - wowSoundDelay
            );

        if (remainingDuration > 0f)
        {
            yield return new WaitForSeconds(
                remainingDuration
            );
        }

        showingSurprise = false;

        activeSurpriseSprite = null;

        reactionRoutine = null;

        if (debugLogs)
        {
            Debug.Log(
                "[PUZZLE WOW] Surprise finished.",
                this
            );
        }
    }


    // ============================================================
    // DISABLE
    // ============================================================

    private void OnDisable()
    {
        if (reactionRoutine != null)
        {
            StopCoroutine(
                reactionRoutine
            );

            reactionRoutine = null;
        }

        showingSurprise = false;

        activeSurpriseSprite = null;
    }


    // ============================================================
    // VALIDATION
    // ============================================================

    private void OnValidate()
    {
        surpriseDelay =
            Mathf.Max(
                0f,
                surpriseDelay
            );

        surpriseDuration =
            Mathf.Max(
                0.01f,
                surpriseDuration
            );

        wowVolume =
            Mathf.Clamp01(
                wowVolume
            );

        wowPitch =
            Mathf.Clamp(
                wowPitch,
                0.1f,
                3f
            );

        wowSoundDelay =
            Mathf.Max(
                0f,
                wowSoundDelay
            );
    }
}
