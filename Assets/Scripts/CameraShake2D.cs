using System.Collections;
using UnityEngine;

public class CameraShake2D : MonoBehaviour
{
    public static CameraShake2D Instance;

    [Header("Default Shake")]
    [SerializeField] private float defaultDuration = 0.12f;
    [SerializeField] private float defaultMagnitude = 0.12f;

    private Vector3 _startLocalPos;
    private Coroutine _routine;

    private void Awake()
    {
        Instance = this;
        _startLocalPos = transform.localPosition;
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        transform.localPosition = _startLocalPos;
    }

    public void Shake(float duration, float magnitude)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    public void ShakeDefault()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float t = 0f;

        while (t < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = _startLocalPos + new Vector3(x, y, 0f);

            t += Time.unscaledDeltaTime; // чтобы тряска работала даже при замедлении времени
            yield return null;
        }

        transform.localPosition = _startLocalPos;
        _routine = null;
    }
}