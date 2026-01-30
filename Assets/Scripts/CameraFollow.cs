using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;      // Player
    [SerializeField] float smoothTime = 0.15f;
    [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);

    Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;

        // если у тебя 2D, обычно Z должен оставаться -10
        targetPos.z = offset.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}