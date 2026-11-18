using UnityEngine;
using UnityEngine.EventSystems;

public class HoldDirectionButton :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    ICancelHandler
{
    // ============================================================
    // PUBLIC STATE
    // ============================================================

    public bool IsHeld { get; private set; }

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]

    [SerializeField]
    private bool debugClimbInput = false;

    // ============================================================
    // PRIVATE
    // ============================================================

    private int activePointerId =
        int.MinValue;

    private MobileInput mobileInput;

    /*
     *  1 = UP
     * -1 = DOWN
     *  0 = LEFT / RIGHT
     *
     * Определяется автоматически.
     */
    private float climbVerticalValue;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        FindMobileInput();
        ResolveDirection();
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        /*
         * Повторяем после Start,
         * чтобы MobileInput точно успел
         * появиться в сцене.
         */
        FindMobileInput();
        ResolveDirection();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (!IsHeld)
        {
            return;
        }

        /*
         * На случай, если ссылка появилась
         * чуть позже.
         */
        if (mobileInput == null)
        {
            FindMobileInput();
            ResolveDirection();
        }

        /*
         * Только UP / DOWN напрямую
         * передают команду ClimbHook.
         */
        if (Mathf.Abs(climbVerticalValue) >
            0.01f)
        {
            ClimbHook.SetClimbVerticalInput(
                climbVerticalValue
            );
        }
    }

    // ============================================================
    // FIND MOBILE INPUT
    // ============================================================

    private void FindMobileInput()
    {
        if (mobileInput != null)
        {
            return;
        }

        mobileInput =
            FindFirstObjectByType<MobileInput>();
    }

    // ============================================================
    // RESOLVE BUTTON DIRECTION
    // ============================================================

    private void ResolveDirection()
    {
        climbVerticalValue =
            0f;

        if (mobileInput == null)
        {
            return;
        }

        /*
         * Никаких чисел руками в Inspector.
         *
         * Скрипт просто смотрит,
         * в какое поле MobileInput
         * назначена эта конкретная кнопка.
         */

        if (mobileInput.up == this)
        {
            climbVerticalValue =
                1f;

            return;
        }

        if (mobileInput.down == this)
        {
            climbVerticalValue =
                -1f;

            return;
        }

        /*
         * LEFT / RIGHT остаются 0.
         */
        climbVerticalValue =
            0f;
    }

    // ============================================================
    // POINTER DOWN
    // ============================================================

    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        if (eventData == null)
        {
            return;
        }

        activePointerId =
            eventData.pointerId;

        IsHeld =
            true;

        FindMobileInput();
        ResolveDirection();

        if (Mathf.Abs(climbVerticalValue) >
            0.01f)
        {
            /*
             * Передаём команду немедленно,
             * не ждём Update.
             */
            ClimbHook.SetClimbVerticalInput(
                climbVerticalValue
            );

            if (debugClimbInput)
            {
                Debug.Log(
                    $"[CLIMB BUTTON] {name} -> " +
                    $"{climbVerticalValue}",
                    this
                );
            }
        }
    }

    // ============================================================
    // POINTER UP
    // ============================================================

    public void OnPointerUp(
        PointerEventData eventData
    )
    {
        if (eventData == null)
        {
            ReleaseButton();
            return;
        }

        if (activePointerId ==
            eventData.pointerId)
        {
            ReleaseButton();
        }
    }

    // ============================================================
    // POINTER EXIT
    // ============================================================

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        if (eventData == null ||
            activePointerId ==
            eventData.pointerId)
        {
            ReleaseButton();
        }
    }

    // ============================================================
    // DRAG
    // ============================================================

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        /*
         * Удержание продолжается.
         */
    }

    public void OnDrag(
        PointerEventData eventData
    )
    {
        /*
         * Ничего делать не нужно.
         */
    }

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        if (eventData == null ||
            activePointerId ==
            eventData.pointerId)
        {
            ReleaseButton();
        }
    }

    // ============================================================
    // CANCEL
    // ============================================================

    public void OnCancel(
        BaseEventData eventData
    )
    {
        ReleaseButton();
    }

    // ============================================================
    // RELEASE
    // ============================================================

    private void ReleaseButton()
    {
        bool wasVerticalButton =
            Mathf.Abs(climbVerticalValue) >
            0.01f;

        IsHeld =
            false;

        activePointerId =
            int.MinValue;

        if (!wasVerticalButton)
        {
            return;
        }

        /*
         * Если одновременно удерживается
         * другая вертикальная кнопка,
         * сохраняем её значение.
         *
         * Иначе отправляем 0.
         */
        float remainingVertical =
            0f;

        if (mobileInput != null)
        {
            if (mobileInput.up != null &&
                mobileInput.up.IsHeld)
            {
                remainingVertical +=
                    1f;
            }

            if (mobileInput.down != null &&
                mobileInput.down.IsHeld)
            {
                remainingVertical -=
                    1f;
            }
        }

        ClimbHook.SetClimbVerticalInput(
            Mathf.Clamp(
                remainingVertical,
                -1f,
                1f
            )
        );

        if (debugClimbInput)
        {
            Debug.Log(
                $"[CLIMB BUTTON] {name} RELEASE -> " +
                $"{remainingVertical}",
                this
            );
        }
    }

    // ============================================================
    // SAFETY
    // ============================================================

    private void OnDisable()
    {
        ReleaseButton();
    }

    private void OnApplicationFocus(
        bool hasFocus
    )
    {
        if (!hasFocus)
        {
            ReleaseButton();
        }
    }

    private void OnApplicationPause(
        bool pauseStatus
    )
    {
        if (pauseStatus)
        {
            ReleaseButton();
        }
    }
}