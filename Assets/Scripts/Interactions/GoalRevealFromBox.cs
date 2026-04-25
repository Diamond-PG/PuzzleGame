using System.Collections;
using UnityEngine;

public class GoalRevealFromBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer goalSpriteRenderer;
    [SerializeField] private Collider2D goalCollider;
    [SerializeField] private Transform goalTransform;
    [SerializeField] private Transform popPoint;
    [SerializeField] private Transform landPoint;

    [Header("Player Side")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool mirrorByPlayerSide = true;

    [Header("Pop Animation")]
    [SerializeField] private float popDuration = 0.30f;
    [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Land Animation")]
    [SerializeField] private bool useLandPoint = true;
    [SerializeField] private float landDuration = 0.22f;
    [SerializeField] private AnimationCurve landCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Scale Animation")]
    [SerializeField] private bool animateScale = true;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.04f, 0.04f, 1f);
    [SerializeField] private Vector3 shownScale = new Vector3(0.08f, 0.08f, 1f);

    [Header("Optional Debug")]
    [SerializeField] private bool debugLogs = false;

    private Vector3 startLocalPosition;
    private bool revealed;

    private void Awake()
    {
        if (goalTransform == null)
            goalTransform = transform;

        startLocalPosition = goalTransform.localPosition;
        HideGoalImmediate();
    }

    public void HideGoalImmediate()
    {
        revealed = false;

        if (goalSpriteRenderer != null)
            goalSpriteRenderer.enabled = false;

        if (goalCollider != null)
            goalCollider.enabled = false;

        if (goalTransform != null)
        {
            goalTransform.localPosition = startLocalPosition;

            if (animateScale)
                goalTransform.localScale = hiddenScale;
        }
    }

    public void RevealGoal()
    {
        if (revealed)
            return;

        revealed = true;
        StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        if (goalSpriteRenderer != null)
            goalSpriteRenderer.enabled = true;

        if (goalTransform == null)
            yield break;

        Vector3 startPos = startLocalPosition;
        Vector3 popPos = GetResolvedPopPosition();
        Vector3 finalLandPos = GetResolvedLandPosition();

        Vector3 startScale = animateScale ? hiddenScale : goalTransform.localScale;
        Vector3 endScale = animateScale ? shownScale : goalTransform.localScale;

        float time = 0f;

        // Этап 1: вылет к GoalPopPoint
        while (time < popDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / Mathf.Max(0.0001f, popDuration));
            float eased = popCurve.Evaluate(t);

            goalTransform.localPosition = Vector3.Lerp(startPos, popPos, eased);

            if (animateScale)
                goalTransform.localScale = Vector3.Lerp(startScale, endScale, eased);

            yield return null;
        }

        goalTransform.localPosition = popPos;

        if (animateScale)
            goalTransform.localScale = endScale;

        // Этап 2: падение к GoalLandPoint
        if (useLandPoint && landPoint != null)
        {
            float landTime = 0f;
            Vector3 landStartPos = popPos;

            while (landTime < landDuration)
            {
                landTime += Time.deltaTime;
                float t = Mathf.Clamp01(landTime / Mathf.Max(0.0001f, landDuration));
                float eased = landCurve.Evaluate(t);

                goalTransform.localPosition = Vector3.Lerp(landStartPos, finalLandPos, eased);
                yield return null;
            }

            goalTransform.localPosition = finalLandPos;
        }

        if (goalCollider != null)
            goalCollider.enabled = true;

        if (debugLogs)
            Debug.Log("[GoalRevealFromBox] Goal reveal finished.", this);
    }

    private Vector3 GetResolvedPopPosition()
    {
        Vector3 result = popPoint != null ? popPoint.localPosition : startLocalPosition;

        if (mirrorByPlayerSide && IsPlayerOnRight())
            result.x = -result.x;

        return result;
    }

    private Vector3 GetResolvedLandPosition()
    {
        Vector3 result = landPoint != null ? landPoint.localPosition : startLocalPosition;

        if (mirrorByPlayerSide && IsPlayerOnRight())
            result.x = -result.x;

        return result;
    }

    private bool IsPlayerOnRight()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            if (debugLogs)
                Debug.LogWarning("[GoalRevealFromBox] Player not found by tag: " + playerTag, this);

            return false;
        }

        return player.transform.position.x > transform.position.x;
    }
}