using System.Collections;
using UnityEngine;

public class DoorUnlock : MonoBehaviour
{
    [Header("Door Parts")]
    [SerializeField] private Transform doorLeft;
    [SerializeField] private Transform doorRight;
    [SerializeField] private Transform lockBar;

    [Header("Door Passage Collider")]
    [Tooltip(
        "Коллайдер самой картинки ворот. " +
        "Он переводится в Trigger, чтобы игрок и Guard " +
        "могли свободно ходить перед воротами."
    )]
    [SerializeField] private Collider2D doorCollider;

    [Header("Level Exit")]
    [Tooltip(
        "Trigger выхода в туннель / завершения уровня. " +
        "Пока ворота закрыты, он будет выключен. " +
        "После открытия включится."
    )]
    [SerializeField] private Collider2D levelExitTrigger;

    [Header("Guard Requirement")]
    [Tooltip(
        "Страж, которого обязательно нужно уничтожить " +
        "перед открытием ворот."
    )]
    [SerializeField] private GuardEnemy requiredGuard;

    [Tooltip(
        "Если включено, дверь нельзя открыть, пока Guard жив."
    )]
    [SerializeField] private bool requireGuardDefeated = true;

    [Header("Sorting")]
    [SerializeField] private int doorOrderInLayer = 1;
    [SerializeField] private int lockOrderInLayer = 2;
    [SerializeField] private int flashOrderInLayer = 3;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip(
        "Насколько близко игрок должен стоять к воротам, " +
        "чтобы ключ сработал."
    )]
    [SerializeField] private float interactDistance = 1f;

    [Header("Key UI")]
    [SerializeField] private GameObject keyIconUI;

    [Header("Open Sound")]
    [SerializeField] private AudioSource unlockAudioSource;
    [SerializeField] private bool playUnlockSound = true;

    [Header("Unlock Haptics")]
    [SerializeField] private bool useUnlockHaptics = true;

    [Header("Open Animation")]
    [SerializeField] private float openDuration = 0.7f;
    [SerializeField] private float leftOpenXOffset = -0.45f;
    [SerializeField] private float rightOpenXOffset = 0.45f;
    [SerializeField] private float openedScaleX = 0.55f;
    [SerializeField] private float openedScaleY = 1f;

    [Header("Lock Fall")]
    [SerializeField] private float lockFallDistance = 0.55f;
    [SerializeField] private float lockFallDuration = 0.35f;

    [Header("Lock Flash")]
    [SerializeField] private ParticleSystem lockFlash;
    [SerializeField] private bool playFlashBeforeLockFalls = true;

    [Header("Visual")]
    [SerializeField] private float openedDarkness = 0.65f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool isOpened;
    private bool openingStarted;

    public bool IsOpened => isOpened;

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

        /*
         * ВАЖНО.
         *
         * Сами ворота визуально находятся
         * ЗА игроком и Guard.
         *
         * Поэтому их Collider не должен быть
         * физической стеной в горизонтальном коридоре.
         *
         * Но Collider мы не удаляем.
         * Просто переводим его в Trigger.
         */
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
            doorCollider.enabled = true;
        }

        /*
         * Выход в тоннель нельзя активировать,
         * пока дверь закрыта.
         */
        if (levelExitTrigger != null)
        {
            levelExitTrigger.enabled = false;
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

    private void Start()
    {
        FixSortingOrder();
    }

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
            return;

        SpriteRenderer sr =
            target.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sortingOrder =
                order;
        }
    }

    public void TryOpenDoorWithKey()
    {
        if (isOpened ||
            openingStarted)
        {
            return;
        }

        /*
         * 1. Сначала обязательно проверяем Guard.
         */
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
                        "[DOOR] Guard is still alive. " +
                        "Door stays closed.",
                        this
                    );
                }

                return;
            }

            /*
             * Если requireGuardDefeated включён,
             * а Guard вообще не назначен и не найден,
             * дверь тоже не открываем.
             *
             * Так мы не получим случайного обхода
             * условия из-за неправильной ссылки.
             */
            if (requiredGuard == null)
            {
                if (debugLogs)
                {
                    Debug.LogWarning(
                        "[DOOR] Required Guard not found. " +
                        "Door cannot open.",
                        this
                    );
                }

                return;
            }
        }

        /*
         * 2. Проверяем ключ.
         */
        if (!KeyPickup.PlayerHasKey())
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[DOOR] No key. " +
                    "Door stays closed.",
                    this
                );
            }

            return;
        }

        /*
         * 3. Ищем игрока.
         */
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

        /*
         * 4. Проверяем расстояние.
         */
        float distance =
            Vector2.Distance(
                playerObj.transform.position,
                transform.position
            );

        if (debugLogs)
        {
            Debug.Log(
                $"[DOOR] TryOpenDoorWithKey. " +
                $"Distance = {distance:F2}",
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

            return;
        }

        /*
         * Все условия выполнены:
         *
         * Guard мёртв.
         * Ключ есть.
         * Игрок рядом.
         */
        StartCoroutine(
            OpenDoorRoutine()
        );
    }

    private IEnumerator OpenDoorRoutine()
    {
        if (openingStarted ||
            isOpened)
        {
            yield break;
        }

        openingStarted = true;

        FixSortingOrder();

        if (debugLogs)
        {
            Debug.Log(
                "[DOOR] Door opening started.",
                this
            );
        }

        /*
         * Иконку ключа убираем.
         */
        if (keyIconUI != null)
        {
            keyIconUI.SetActive(false);
        }

        /*
         * Ключ расходуется только тогда,
         * когда дверь реально начала открываться.
         *
         * Если Guard жив или игрок далеко,
         * ключ НЕ тратится.
         */
        KeyPickup.ConsumeKey();

        if (useUnlockHaptics)
        {
            MicroHaptics.TinyClick();
        }

        PlayUnlockSound();

        /*
         * Вспышка перед падением замка.
         */
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

        /*
         * Падение центрального замка.
         */
        if (lockBar != null)
        {
            yield return StartCoroutine(
                FallLockRoutine()
            );
        }

        /*
         * Либо вспышка после замка,
         * если так выставлено в Inspector.
         */
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

        /*
         * Раздвигаем створки.
         */
        yield return StartCoroutine(
            OpenWingsRoutine()
        );

        /*
         * Сам Collider картинки двери
         * больше вообще не нужен.
         *
         * До открытия он был Trigger,
         * поэтому не мешал ходить.
         */
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        /*
         * ТЕПЕРЬ открываем настоящий выход
         * в туннель / LevelExitTrigger.
         */
        if (levelExitTrigger != null)
        {
            levelExitTrigger.enabled = true;
        }

        isOpened = true;
        openingStarted = false;

        if (debugLogs)
        {
            Debug.Log(
                "[DOOR] Door opened. " +
                "Level exit enabled.",
                this
            );
        }
    }

    private void PlayUnlockSound()
    {
        if (!playUnlockSound)
            return;

        if (unlockAudioSource == null)
            return;

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

        float timer = 0f;

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

        if (lockBar != null)
        {
            lockBar.gameObject.SetActive(
                false
            );
        }
    }

    private IEnumerator OpenWingsRoutine()
    {
        if (doorLeft == null ||
            doorRight == null)
        {
            Debug.LogWarning(
                "[DOOR] Door parts " +
                "are not assigned.",
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

        float timer = 0f;

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
}