using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    [Header("Fire Visuals")]
    [SerializeField] private Transform[] fireVisuals;

    [Header("Flicker Speed")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 8f;

    [Header("Vertical Flicker")]
    [SerializeField] private float verticalAmount = 0.12f;

    [Header("Horizontal Flicker")]
    [SerializeField] private float horizontalAmount = 0.05f;

    [Header("Small Sway")]
    [SerializeField] private float swayAmount = 2.5f;

    private Vector3[] originalScales;
    private Quaternion[] originalRotations;
    private float[] phases;
    private float[] speeds;

    private void Awake()
    {
        if (fireVisuals == null ||
            fireVisuals.Length == 0)
        {
            return;
        }

        originalScales =
            new Vector3[fireVisuals.Length];

        originalRotations =
            new Quaternion[fireVisuals.Length];

        phases =
            new float[fireVisuals.Length];

        speeds =
            new float[fireVisuals.Length];

        for (int i = 0; i < fireVisuals.Length; i++)
        {
            if (fireVisuals[i] == null)
                continue;

            originalScales[i] =
                fireVisuals[i].localScale;

            originalRotations[i] =
                fireVisuals[i].localRotation;

            phases[i] =
                Random.Range(0f, Mathf.PI * 2f);

            speeds[i] =
                Random.Range(minSpeed, maxSpeed);
        }
    }

    private void Update()
    {
        if (fireVisuals == null)
            return;

        for (int i = 0; i < fireVisuals.Length; i++)
        {
            Transform fire = fireVisuals[i];

            if (fire == null)
                continue;

            float wave =
                Mathf.Sin(
                    Time.time * speeds[i] +
                    phases[i]
                );

            float secondWave =
                Mathf.Sin(
                    Time.time *
                    speeds[i] *
                    1.37f +
                    phases[i] * 1.7f
                );

            Vector3 baseScale =
                originalScales[i];

            float scaleX =
                1f +
                secondWave *
                horizontalAmount;

            float scaleY =
                1f +
                wave *
                verticalAmount;

            fire.localScale =
                new Vector3(
                    baseScale.x * scaleX,
                    baseScale.y * scaleY,
                    baseScale.z
                );

            float sway =
                secondWave *
                swayAmount;

            fire.localRotation =
                originalRotations[i] *
                Quaternion.Euler(
                    0f,
                    0f,
                    sway
                );
        }
    }

    private void OnDisable()
    {
        RestoreOriginalValues();
    }

    private void RestoreOriginalValues()
    {
        if (fireVisuals == null ||
            originalScales == null)
        {
            return;
        }

        for (int i = 0; i < fireVisuals.Length; i++)
        {
            if (fireVisuals[i] == null)
                continue;

            fireVisuals[i].localScale =
                originalScales[i];

            fireVisuals[i].localRotation =
                originalRotations[i];
        }
    }
}