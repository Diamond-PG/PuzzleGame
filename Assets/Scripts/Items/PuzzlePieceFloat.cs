using UnityEngine;

public class PuzzlePieceFloat : MonoBehaviour
{
    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("REFERENCES")]

    [Tooltip(
        "Collider пазла. Пока он выключен во время вылета из ящика, " +
        "покачивание не работает. После приземления Collider включается, " +
        "и тогда начинается анимация."
    )]
    [SerializeField]
    private Collider2D pickupCollider;


    // ============================================================
    // FLOAT MOVEMENT
    // ============================================================

    [Header("FLOAT MOVEMENT")]

    [Tooltip(
        "Насколько пазл двигается влево-вправо."
    )]
    [SerializeField, Min(0f)]
    private float horizontalAmount = 0.035f;

    [Tooltip(
        "Насколько пазл двигается вверх-вниз."
    )]
    [SerializeField, Min(0f)]
    private float verticalAmount = 0.045f;

    [Tooltip(
        "Скорость общего покачивания."
    )]
    [SerializeField, Min(0.01f)]
    private float movementSpeed = 1.25f;


    // ============================================================
    // ROTATION
    // ============================================================

    [Header("ROTATION")]

    [Tooltip(
        "Небольшой наклон пазла влево-вправо."
    )]
    [SerializeField]
    private bool useRotation = true;

    [Tooltip(
        "Максимальный угол наклона в градусах."
    )]
    [SerializeField, Range(0f, 20f)]
    private float rotationAmount = 2.5f;

    [Tooltip(
        "Скорость наклона."
    )]
    [SerializeField, Min(0.01f)]
    private float rotationSpeed = 0.9f;


    // ============================================================
    // MOVEMENT SHAPE
    // ============================================================

    [Header("MOVEMENT SHAPE")]

    [Tooltip(
        "Разница фаз между движением по X и Y. " +
        "Благодаря этому пазл слегка плавает, " +
        "а не двигается строго по диагонали."
    )]
    [SerializeField]
    private float verticalPhaseOffset = 1.15f;


    // ============================================================
    // STATE
    // ============================================================

    private Vector3 baseWorldPosition;

    private Quaternion baseWorldRotation;

    private Transform rememberedParent;

    private bool floatStarted;

    private float animationTime;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (pickupCollider == null)
        {
            pickupCollider =
                GetComponent<Collider2D>();
        }

        rememberedParent =
            transform.parent;

        baseWorldPosition =
            transform.position;

        baseWorldRotation =
            transform.rotation;
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        /*
         * Пока GoalRevealFromBox прячет пазл
         * или проигрывает его вылет из ящика,
         * Collider выключен.
         *
         * В этот момент этот скрипт вообще
         * не вмешивается в движение.
         */
        if (pickupCollider == null ||
            !pickupCollider.enabled)
        {
            floatStarted =
                false;

            return;
        }


        // ========================================================
        // PARENT CHANGED
        // ========================================================

        /*
         * BreakableHardBox после вылета
         * отсоединяет пазл от ящика:
         *
         * transform.SetParent(null, true)
         *
         * Поэтому обязательно отслеживаем
         * смену родителя и заново запоминаем
         * мировую позицию.
         *
         * Это не даёт пазлу телепортироваться
         * после уничтожения ящика.
         */
        if (transform.parent !=
            rememberedParent)
        {
            rememberedParent =
                transform.parent;

            baseWorldPosition =
                transform.position;

            baseWorldRotation =
                transform.rotation;

            animationTime =
                0f;

            floatStarted =
                true;
        }


        // ========================================================
        // FIRST FRAME AFTER LANDING
        // ========================================================

        if (!floatStarted)
        {
            floatStarted =
                true;

            animationTime =
                0f;

            rememberedParent =
                transform.parent;

            baseWorldPosition =
                transform.position;

            baseWorldRotation =
                transform.rotation;
        }


        // ========================================================
        // TIME
        // ========================================================

        animationTime +=
            Time.deltaTime;


        // ========================================================
        // POSITION
        // ========================================================

        float horizontalWave =
            Mathf.Sin(
                animationTime *
                movementSpeed
            );

        float verticalWave =
            Mathf.Sin(
                animationTime *
                movementSpeed *
                1.17f +
                verticalPhaseOffset
            );

        Vector3 targetPosition =
            baseWorldPosition;

        targetPosition.x +=
            horizontalWave *
            horizontalAmount;

        targetPosition.y +=
            verticalWave *
            verticalAmount;

        /*
         * ВАЖНО:
         *
         * Используем WORLD POSITION,
         * а не localPosition.
         *
         * Поэтому смена родителя после
         * разрушения ящика больше
         * не отправляет пазл вниз.
         */
        transform.position =
            targetPosition;


        // ========================================================
        // ROTATION
        // ========================================================

        if (useRotation)
        {
            float rotationWave =
                Mathf.Sin(
                    animationTime *
                    rotationSpeed
                );

            float zRotation =
                rotationWave *
                rotationAmount;

            transform.rotation =
                baseWorldRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    zRotation
                );
        }
    }


    // ============================================================
    // DISABLE
    // ============================================================

    private void OnDisable()
    {
        floatStarted =
            false;

        animationTime =
            0f;
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        horizontalAmount =
            Mathf.Max(
                0f,
                horizontalAmount
            );

        verticalAmount =
            Mathf.Max(
                0f,
                verticalAmount
            );

        movementSpeed =
            Mathf.Max(
                0.01f,
                movementSpeed
            );

        rotationAmount =
            Mathf.Max(
                0f,
                rotationAmount
            );

        rotationSpeed =
            Mathf.Max(
                0.01f,
                rotationSpeed
            );
    }
}