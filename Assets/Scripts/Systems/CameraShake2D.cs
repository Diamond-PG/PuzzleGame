using System.Collections;
using UnityEngine;

public class CameraShake2D : MonoBehaviour
{
    public static CameraShake2D Instance;

    [Header("Default Shake")]
    [SerializeField] private float defaultDuration = 0.12f;
    [SerializeField] private float defaultMagnitude = 0.12f;

    private Vector3 _startLocalPos;
    private Vector2 _legacyOffset;
    private Vector2 _combatOffset;
    private Coroutine _routine;

    private void Awake()
    {
        Instance = this;
        _startLocalPos = transform.localPosition;
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        _legacyOffset = Vector2.zero;
        _combatOffset = Vector2.zero;
        transform.localPosition = _startLocalPos;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    internal void SetCombatOffset(Vector2 offset)
    {
        if (!isActiveAndEnabled)
            return;

        _combatOffset = offset;
        ApplyOffset();
    }

    private void ApplyOffset() => transform.localPosition = _startLocalPos + (Vector3)(_legacyOffset + _combatOffset);

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

            _legacyOffset = new Vector2(x, y);
            ApplyOffset();

            t += Time.unscaledDeltaTime; // чтобы тряска работала даже при замедлении времени
            yield return null;
        }

        _legacyOffset = Vector2.zero;
        ApplyOffset();
        _routine = null;
    }
}
