using UnityEngine;

public class WorldItemBob : MonoBehaviour
{
    // ============================================================
    // BOB MOVEMENT
    // ============================================================

    [Header("BOB MOVEMENT")]

    [Tooltip(
        "Амплитуда движения по X и Y. " +
        "Для обычного движения только вверх-вниз X оставляем 0."
    )]
    [SerializeField]
    private Vector2 bobAmount =
        new Vector2(
            0f,
            0.025f
        );

    [Tooltip(
        "Сколько секунд занимает полный цикл: " +
        "вверх -> вниз -> обратно."
    )]
    [SerializeField, Min(0.1f)]
    private float cycleDuration = 3.5f;


    // ============================================================
    // BASE POSITION
    // ============================================================

    [Header("BASE POSITION OFFSET")]

    [Tooltip(
        "Дополнительное постоянное смещение предмета. " +
        "Можно отдельно поднять/опустить его по Y " +
        "или сдвинуть по X."
    )]
    [SerializeField]
    private Vector2 baseOffset =
        Vector2.zero;


    // ============================================================
    // START PHASE
    // ============================================================

    [Header("START")]

    [Tooltip(
        "Если включено, движение начинается " +
        "из центральной позиции."
    )]
    [SerializeField]
    private bool startFromCenter = true;


    // ============================================================
    // STATIONARY OBJECT
    // ============================================================

    [Header("KEEP OBJECT STATIONARY")]

    [Tooltip(
        "Объект, который должен оставаться на месте, " +
        "пока предмет двигается. " +
        "Для дубинки сюда ставим GlowAnchor."
    )]
    [SerializeField]
    private Transform stationaryObject;


    // ============================================================
    // GLOW SCALE WITH BOB
    // ============================================================

    [Header("GLOW SCALE WITH BOB")]

    [Tooltip(
        "Если включено, размер подсветки меняется " +
        "в зависимости от высоты предмета."
    )]
    [SerializeField]
    private bool scaleGlowWithBob = true;


    [Tooltip(
        "Размер подсветки, когда дубинка находится внизу."
    )]
    [SerializeField, Min(0f)]
    private float glowScaleAtBottom = 0.85f;


    [Tooltip(
        "Размер подсветки, когда дубинка находится наверху."
    )]
    [SerializeField, Min(0f)]
    private float glowScaleAtTop = 1.15f;


    [Header("GLOW SCALE RESPONSE")]

    [Tooltip(
        "Как расширяется подсветка, когда дубинка поднимается. " +
        "1 = обычное плавное расширение. " +
        "Чем больше значение, тем дольше подсветка остаётся маленькой."
    )]
    [SerializeField, Min(0.1f)]
    private float glowExpandPower = 1f;


    [Tooltip(
        "Как быстро подсветка сужается, когда дубинка начинает опускаться. " +
        "Чем больше значение, тем быстрее она визуально сужается."
    )]
    [SerializeField, Min(0.1f)]
    private float glowShrinkPower = 3f;


    // ============================================================
    // STATE
    // ============================================================

    private Vector3 startLocalPosition;

    private Vector3 stationaryWorldPosition;

    private Vector3 stationaryOriginalLocalScale;

    private float startTime;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        RememberPositions();
    }


    // ============================================================
    // ON ENABLE
    // ============================================================

    private void OnEnable()
    {
        RememberPositions();

        startTime =
            Time.time;
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        float safeDuration =
            Mathf.Max(
                0.1f,
                cycleDuration
            );


        float elapsed =
            Time.time -
            startTime;


        float phase =
            startFromCenter
                ? 0f
                : -Mathf.PI * 0.5f;


        float angle =
            (
                elapsed /
                safeDuration
            ) *
            Mathf.PI *
            2f +
            phase;


        float wave =
            Mathf.Sin(
                angle
            );


        // --------------------------------------------------------
        // ITEM MOVEMENT
        // --------------------------------------------------------

        Vector3 targetPosition =
            startLocalPosition;


        targetPosition.x +=
            baseOffset.x +
            wave *
            bobAmount.x;


        targetPosition.y +=
            baseOffset.y +
            wave *
            bobAmount.y;


        transform.localPosition =
            targetPosition;


        // --------------------------------------------------------
        // KEEP GLOW / OTHER OBJECT FIXED
        // --------------------------------------------------------

        if (stationaryObject != null)
        {
            stationaryObject.position =
                stationaryWorldPosition;
        }


        // --------------------------------------------------------
        // GLOW SCALE
        // --------------------------------------------------------

        if (stationaryObject != null &&
            scaleGlowWithBob)
        {
            /*
             * wave:
             *
             * -1 = дубинка внизу
             *  0 = середина
             * +1 = дубинка наверху
             */

            float height01 =
                Mathf.InverseLerp(
                    -1f,
                    1f,
                    wave
                );


            /*
             * Cos показывает направление движения:
             *
             * > 0 = дубинка поднимается
             * < 0 = дубинка опускается
             */
            bool movingUp =
                Mathf.Cos(
                    angle
                ) >= 0f;


            float adjustedHeight;


            if (movingUp)
            {
                /*
                 * Подъём:
                 * обычное плавное расширение.
                 */
                adjustedHeight =
                    Mathf.Pow(
                        height01,
                        glowExpandPower
                    );
            }
            else
            {
                /*
                 * Опускание:
                 * при значении больше 1
                 * подсветка начинает быстрее сужаться.
                 */
                adjustedHeight =
                    Mathf.Pow(
                        height01,
                        glowShrinkPower
                    );
            }


            float glowScale =
                Mathf.Lerp(
                    glowScaleAtBottom,
                    glowScaleAtTop,
                    adjustedHeight
                );


            stationaryObject.localScale =
                new Vector3(
                    stationaryOriginalLocalScale.x *
                    glowScale,

                    stationaryOriginalLocalScale.y *
                    glowScale,

                    stationaryOriginalLocalScale.z
                );
        }
    }


    // ============================================================
    // REMEMBER POSITIONS
    // ============================================================

    private void RememberPositions()
    {
        startLocalPosition =
            transform.localPosition;


        if (stationaryObject != null)
        {
            stationaryWorldPosition =
                stationaryObject.position;


            stationaryOriginalLocalScale =
                stationaryObject.localScale;
        }
    }


    // ============================================================
    // ON DISABLE
    // ============================================================

    private void OnDisable()
    {
        transform.localPosition =
            startLocalPosition;


        if (stationaryObject != null)
        {
            stationaryObject.position =
                stationaryWorldPosition;


            stationaryObject.localScale =
                stationaryOriginalLocalScale;
        }
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        cycleDuration =
            Mathf.Max(
                0.1f,
                cycleDuration
            );


        glowScaleAtBottom =
            Mathf.Max(
                0f,
                glowScaleAtBottom
            );


        glowScaleAtTop =
            Mathf.Max(
                0f,
                glowScaleAtTop
            );


        glowExpandPower =
            Mathf.Max(
                0.1f,
                glowExpandPower
            );


        glowShrinkPower =
            Mathf.Max(
                0.1f,
                glowShrinkPower
            );
    }
}