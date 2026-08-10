using UnityEngine;

public class FireSpriteAnimator : MonoBehaviour
{
    [Header("Fire Renderers")]
    [SerializeField] private SpriteRenderer[] fireRenderers;

    [Header("Fire Frames")]
    [SerializeField] private Sprite[] fireSprites;

    [Header("Animation")]
    [SerializeField] private float framesPerSecond = 10f;

    [Tooltip("Если включено, разные языки огня начинают анимацию с разных кадров.")]
    [SerializeField] private bool randomStartFrame = true;

    private int[] currentFrames;
    private float[] timers;
    private float frameDuration;

    private void Awake()
    {
        frameDuration = 1f / Mathf.Max(1f, framesPerSecond);

        int count = fireRenderers != null
            ? fireRenderers.Length
            : 0;

        currentFrames = new int[count];
        timers = new float[count];

        if (fireSprites == null || fireSprites.Length == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            if (randomStartFrame)
            {
                currentFrames[i] =
                    Random.Range(0, fireSprites.Length);

                timers[i] =
                    Random.Range(0f, frameDuration);
            }
            else
            {
                currentFrames[i] = 0;
                timers[i] = 0f;
            }

            ApplySprite(i);
        }
    }

    private void Update()
    {
        if (fireRenderers == null ||
            fireSprites == null ||
            fireSprites.Length == 0)
        {
            return;
        }

        for (int i = 0; i < fireRenderers.Length; i++)
        {
            if (fireRenderers[i] == null)
                continue;

            if (!fireRenderers[i].gameObject.activeInHierarchy)
                continue;

            timers[i] += Time.deltaTime;

            if (timers[i] < frameDuration)
                continue;

            timers[i] -= frameDuration;

            currentFrames[i]++;

            if (currentFrames[i] >= fireSprites.Length)
                currentFrames[i] = 0;

            ApplySprite(i);
        }
    }

    private void ApplySprite(int index)
    {
        if (index < 0 ||
            index >= fireRenderers.Length)
        {
            return;
        }

        if (fireRenderers[index] == null)
            return;

        fireRenderers[index].sprite =
            fireSprites[currentFrames[index]];
    }
}