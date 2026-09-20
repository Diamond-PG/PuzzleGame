using System.Collections;
using UnityEngine;

public class DoorUnlock : MonoBehaviour
{
    // ============================================================
    // DOOR PARTS
    // ============================================================

    [Header("Door Parts")]

    [SerializeField]
    private Transform doorLeft;

    [SerializeField]
    private Transform doorRight;

    [SerializeField]
    private Transform lockBar;


    // ============================================================
    // DOOR PASSAGE COLLIDER
    // ============================================================

    [Header("Door Passage Collider")]

    [Tooltip(
        "Коллайдер самой картинки ворот. " +
        "Он переводится в Trigger, чтобы игрок и Guard " +
        "могли свободно ходить перед воротами."
    )]
    [SerializeField]
    private Collider2D doorCollider;


    // ============================================================
    // LEVEL EXIT
    // ============================================================

    [Header("Level Exit")]

    [Tooltip(
        "Trigger выхода в туннель / завершения уровня. " +
        "Пока ворота закрыты, он выключен. " +
        "После открытия включается."
    )]
    [SerializeField]
    private Collider2D levelExitTrigger;


    // ============================================================
    // GUARD REQUIREMENT
    // ============================================================

    [Header("Guard Requirement")]

    [Tooltip(
        "Страж, которого обязательно нужно уничтожить " +
        "перед открытием ворот."
    )]
    [SerializeField]
    private GuardEnemy requiredGuard;

    [Tooltip(
        "Если включено, дверь нельзя открыть, пока Guard жив."
    )]
    [SerializeField]
    private bool requireGuardDefeated = true;


    // ============================================================
    // SORTING
    // ============================================================

    [Header("Sorting")]

    [SerializeField]
    private int doorOrderInLayer = 1;

    [SerializeField]
    private int lockOrderInLayer = 2;

    [SerializeField]
    private int flashOrderInLayer = 3;


    // ============================================================
    // PLAYER
    // ============================================================

    [Header("Player")]

    [SerializeField]
    private string playerTag = "Player";

    [Tooltip(
        "Насколько близко игрок должен стоять к воротам, " +
        "чтобы ключ сработал."
    )]
    [SerializeField]
    private float interactDistance = 1f;


    // ============================================================
    // OPEN SOUND
    // ============================================================

    [Header("Open Sound")]

    [SerializeField]
    private AudioSource unlockAudioSource;

    [SerializeField]
    private bool playUnlockSound = true;


    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("Unlock Haptics")]

    [SerializeField]
    private bool useUnlockHaptics = true;


    // ============================================================
    // OPEN ANIMATION
    // ============================================================

    [Header("Open Animation")]

    [SerializeField]
    private float openDuration = 0.7f;

    [SerializeField]
    private float leftOpenXOffset = -0.45f;

    [SerializeField]
    private float rightOpenXOffset = 0.45f;

    [SerializeField]
    private float openedScaleX = 0.55f;

    [SerializeField]
    private float openedScaleY = 1f;


    // ============================================================
    // LOCK FALL
    // ============================================================

    [Header("Lock Fall")]

    [SerializeField]
    private float lockFallDistance = 0.55f;

    [SerializeField]
    private float lockFallDuration = 0.35f;


    // ============================================================
    // LOCK FLASH
    // ============================================================

    [Header("Lock Flash")]

    [SerializeField]
    private ParticleSystem lockFlash;

    [SerializeField]
    private bool playFlashBeforeLockFalls = true;


    // ============================================================
    // VISUAL
    // ============================================================

    [Header("Visual")]

    [SerializeField]
    private float openedDarkness = 0.65f;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]

    [SerializeField]
    private bool debugLogs = true;


    // ============================================================
    // STATE
    // ============================================================

    private bool isOpened;

    private bool openingStarted;


    // ============================================================
    // PUBLIC
    // ============================================================

    public bool IsOpened =>
        isOpened;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (doorCollider == null)
        {
            doorCollider =
                GetComponent<Collider2D>();
        }

        if (unlockAudioSource == null)
        {
            unlockAudioSource =
                GetComponent<AudioSource>();
        }

        if (requiredGuard == null)
        {
            requiredGuard =
                Object.FindFirstObjectByType<GuardEnemy>();
        }

        if (doorCollider != null)
        {
            doorCollider.isTrigger =
                true;

            doorCollider.enabled =
                true;
        }

        if (levelExitTrigger != null)
        {
            levelExitTrigger.enabled =
                false;
        }

        FixSortingOrder();

        if (lockFlash != null)
        {
            lockFlash.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }
    }


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        FixSortingOrder();
    }


    // ============================================================
    // SORTING
    // ============================================================

    private void FixSortingOrder()
    {
        SetOrder(
            doorLeft,
            doorOrderInLayer
        );

        SetOrder(
            doorRight,
            doorOrderInLayer
        );

        SetOrder(
            lockBar,
            lockOrderInLayer
        );

        if (lockFlash != null)
        {
            ParticleSystemRenderer psRenderer =
                lockFlash.GetComponent<
                    ParticleSystemRenderer
                >();

            if (psRenderer != null)
            {
                psRenderer.sortingOrder =
                    flashOrderInLayer;
            }
        }
    }


    private void SetOrder(
        Transform target,
        int order
    )
    {
        if (target == null)
        {
            return;
        }

        SpriteRenderer sr =
            target.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sortingOrder =
                order;
        }
    }


    // ============================================================
    // TRY OPEN WITH KEY
    // ============================================================

    public void TryOpenDoorWithKey()
    {
        if (isOpened ||
            openingStarted)
        {
            return;
        }


        // --------------------------------------------------------
        // GUARD
        // --------------------------------------------------------

        if (requireGuardDefeated)
        {
            if (requiredGuard == null)
            {
                requiredGuard =
                    Object.FindFirstObjectByType<
                        GuardEnemy
                    >();
            }

            if (requiredGuard != null &&
                !requiredGuard.IsDead)
            {
                if (debugLogs)
                {
                    Debug.Log(
                        "[DOOR] Guard is still alive.",
                        this
                    );
                }

                return;
            }

            if (requiredGuard == null)
            {
                if (debugLogs)
                {
                    Debug.LogWarning(
                        "[DOOR] Required Guard not found.",
                        this
                    );
                }

                return;
            }
        }


        // --------------------------------------------------------
        // KEY
        // --------------------------------------------------------

        if (!KeyPickup.PlayerHasKey())
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[DOOR] Player has no key.",
                    this
                );
            }

            return;
        }


        // --------------------------------------------------------
        // PLAYER
        // --------------------------------------------------------

        GameObject playerObj =
            GameObject.FindGameObjectWithTag(
                playerTag
            );

        if (playerObj == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning(
                    "[DOOR] Player not found.",
                    this
                );
            }

            return;
        }


        // --------------------------------------------------------
        // DISTANCE
        // --------------------------------------------------------

        float distance =
            Vector2.Distance(
                playerObj.transform.position,
                transform.position
            );

        if (debugLogs)
        {
            Debug.Log(
                "[DOOR] Key pressed. Distance = " +
                distance.ToString("F2"),
                this
            );
        }

        if (distance >
            interactDistance)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[DOOR] Player too far.",
                    this
                );
            }

            /*
             * Очень важно:
             * ключ НЕ исчезает.
             */
            return;
        }


        // --------------------------------------------------------
        // SUCCESS
        // --------------------------------------------------------

        StartCoroutine(
            OpenDoorRoutine()
        );
    }


    // ============================================================
    // OPEN ROUTINE
    // ============================================================

    private IEnumerator OpenDoorRoutine()
    {
        if (openingStarted ||
            isOpened)
        {
            yield break;
        }

        openingStarted =
            true;

        FixSortingOrder();


        /*
         * Ключ расходуется ТОЛЬКО здесь.
         *
         * KeyPickup сам знает,
         * в какой динамической ячейке
         * сейчас лежит настоящий ключ.
         */
        KeyPickup.ConsumeKey();


        if (useUnlockHaptics)
        {
            MicroHaptics.TinyClick();
        }

        PlayUnlockSound();


        // --------------------------------------------------------
        // FLASH BEFORE LOCK
        // --------------------------------------------------------

        if (lockFlash != null &&
            playFlashBeforeLockFalls)
        {
            lockFlash.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );

            lockFlash.Play();
        }


        // --------------------------------------------------------
        // LOCK FALL
        // --------------------------------------------------------

        if (lockBar != null)
        {
            yield return StartCoroutine(
                FallLockRoutine()
            );
        }


        // --------------------------------------------------------
        // FLASH AFTER LOCK
        // --------------------------------------------------------

        if (lockFlash != null &&
            !playFlashBeforeLockFalls)
        {
            lockFlash.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );

            lockFlash.Play();
        }


        // --------------------------------------------------------
        // OPEN WINGS
        // --------------------------------------------------------

        yield return StartCoroutine(
            OpenWingsRoutine()
        );


        // --------------------------------------------------------
        // COLLIDERS
        // --------------------------------------------------------

        if (doorCollider != null)
        {
            doorCollider.enabled =
                false;
        }

        if (levelExitTrigger != null)
        {
            levelExitTrigger.enabled =
                true;
        }


        isOpened =
            true;

        openingStarted =
            false;

        if (debugLogs)
        {
            Debug.Log(
                "[DOOR] Door opened.",
                this
            );
        }
    }


    // ============================================================
    // SOUND
    // ============================================================

    private void PlayUnlockSound()
    {
        if (!playUnlockSound)
        {
            return;
        }

        if (unlockAudioSource == null)
        {
            return;
        }

        if (unlockAudioSource.clip != null)
        {
            unlockAudioSource.PlayOneShot(
                unlockAudioSource.clip
            );
        }
        else
        {
            unlockAudioSource.Play();
        }
    }


    // ============================================================
    // LOCK FALL
    // ============================================================

    private IEnumerator FallLockRoutine()
    {
        Vector3 startPos =
            lockBar.localPosition;

        Vector3 endPos =
            startPos +
            new Vector3(
                0f,
                -lockFallDistance,
                0f
            );

        SpriteRenderer lockRenderer =
            lockBar.GetComponent<SpriteRenderer>();

        Color startColor =
            lockRenderer != null
                ? lockRenderer.color
                : Color.white;

        float safeDuration =
            Mathf.Max(
                0.01f,
                lockFallDuration
            );

        float timer =
            0f;

        while (timer <
               safeDuration)
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    safeDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            lockBar.localPosition =
                Vector3.Lerp(
                    startPos,
                    endPos,
                    t
                );

            if (lockRenderer != null)
            {
                Color c =
                    startColor;

                c.a =
                    Mathf.Lerp(
                        startColor.a,
                        0f,
                        t
                    );

                lockRenderer.color =
                    c;
            }

            yield return null;
        }

        lockBar.localPosition =
            endPos;

        lockBar.gameObject.SetActive(
            false
        );
    }


    // ============================================================
    // OPEN WINGS
    // ============================================================

    private IEnumerator OpenWingsRoutine()
    {
        if (doorLeft == null ||
            doorRight == null)
        {
            Debug.LogWarning(
                "[DOOR] Door parts are not assigned.",
                this
            );

            yield break;
        }

        FixSortingOrder();

        Vector3 leftStartPos =
            doorLeft.localPosition;

        Vector3 rightStartPos =
            doorRight.localPosition;


        Vector3 leftEndPos =
            leftStartPos +
            new Vector3(
                leftOpenXOffset,
                0f,
                0f
            );

        Vector3 rightEndPos =
            rightStartPos +
            new Vector3(
                rightOpenXOffset,
                0f,
                0f
            );


        Vector3 leftStartScale =
            doorLeft.localScale;

        Vector3 rightStartScale =
            doorRight.localScale;


        Vector3 leftEndScale =
            new Vector3(
                openedScaleX,
                openedScaleY,
                leftStartScale.z
            );

        Vector3 rightEndScale =
            new Vector3(
                openedScaleX,
                openedScaleY,
                rightStartScale.z
            );


        SpriteRenderer leftRenderer =
            doorLeft.GetComponent<SpriteRenderer>();

        SpriteRenderer rightRenderer =
            doorRight.GetComponent<SpriteRenderer>();


        Color leftStartColor =
            leftRenderer != null
                ? leftRenderer.color
                : Color.white;

        Color rightStartColor =
            rightRenderer != null
                ? rightRenderer.color
                : Color.white;


        Color leftEndColor =
            new Color(
                openedDarkness,
                openedDarkness,
                openedDarkness,
                leftStartColor.a
            );

        Color rightEndColor =
            new Color(
                openedDarkness,
                openedDarkness,
                openedDarkness,
                rightStartColor.a
            );


        float safeDuration =
            Mathf.Max(
                0.01f,
                openDuration
            );

        float timer =
            0f;

        while (timer <
               safeDuration)
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    safeDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            doorLeft.localPosition =
                Vector3.Lerp(
                    leftStartPos,
                    leftEndPos,
                    t
                );

            doorRight.localPosition =
                Vector3.Lerp(
                    rightStartPos,
                    rightEndPos,
                    t
                );


            doorLeft.localScale =
                Vector3.Lerp(
                    leftStartScale,
                    leftEndScale,
                    t
                );

            doorRight.localScale =
                Vector3.Lerp(
                    rightStartScale,
                    rightEndScale,
                    t
                );


            if (leftRenderer != null)
            {
                leftRenderer.color =
                    Color.Lerp(
                        leftStartColor,
                        leftEndColor,
                        t
                    );
            }

            if (rightRenderer != null)
            {
                rightRenderer.color =
                    Color.Lerp(
                        rightStartColor,
                        rightEndColor,
                        t
                    );
            }

            yield return null;
        }


        doorLeft.localPosition =
            leftEndPos;

        doorRight.localPosition =
            rightEndPos;

        doorLeft.localScale =
            leftEndScale;

        doorRight.localScale =
            rightEndScale;


        if (leftRenderer != null)
        {
            leftRenderer.color =
                leftEndColor;
        }

        if (rightRenderer != null)
        {
            rightRenderer.color =
                rightEndColor;
        }

        FixSortingOrder();
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        interactDistance =
            Mathf.Max(
                0f,
                interactDistance
            );

        openDuration =
            Mathf.Max(
                0.01f,
                openDuration
            );

        lockFallDuration =
            Mathf.Max(
                0.01f,
                lockFallDuration
            );

        lockFallDistance =
            Mathf.Max(
                0f,
                lockFallDistance
            );

        openedDarkness =
            Mathf.Clamp01(
                openedDarkness
            );
    }
}