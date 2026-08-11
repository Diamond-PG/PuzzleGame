using UnityEngine;

public class CageSwing : MonoBehaviour
{
    [Header("Horizontal Swing")]

    [Tooltip("Насколько клетка двигается влево-вправо.")]
    [SerializeField] private float horizontalDistance = 0.08f;

    [Tooltip("Скорость покачивания.")]
    [SerializeField] private float swingSpeed = 0.65f;

    [Header("Optional Rotation")]

    [Tooltip("Очень лёгкий наклон. Поставь 0, если не нужен.")]
    [SerializeField] private float maxAngle = 0f;

    [Header("Motion Shape")]

    [SerializeField] private float phaseOffset = 0f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        float swing =
            Mathf.Sin(
                Time.time * swingSpeed +
                phaseOffset
            );

        float moveX =
            swing * horizontalDistance;

        float angle =
            swing * maxAngle;

        transform.localPosition =
            startPosition +
            new Vector3(
                moveX,
                0f,
                0f
            );

        transform.localRotation =
            startRotation *
            Quaternion.Euler(
                0f,
                0f,
                angle
            );
    }

    private void OnDisable()
    {
        transform.localPosition =
            startPosition;

        transform.localRotation =
            startRotation;
    }
}