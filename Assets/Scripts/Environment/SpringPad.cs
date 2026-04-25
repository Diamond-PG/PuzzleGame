using System.Collections;
using UnityEngine;

public class SpringPad : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float launchForce = 12f;
    [SerializeField] private float minImpactSpeedToActivate = 0.2f;

    [Header("Trigger Rules")]
    [SerializeField] private float cooldown = 0.25f;
    [SerializeField] private float playerAboveTolerance = 0.08f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer springRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private float pressedTime = 0.08f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Collider2D springCollider;
    private bool canActivate = true;

    private void Awake()
    {
        springCollider = GetComponent<Collider2D>();

        if (springRenderer == null)
            springRenderer = GetComponent<SpriteRenderer>();

        if (springRenderer != null && normalSprite != null)
            springRenderer.sprite = normalSprite;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryActivate(collision);
    }

    private void TryActivate(Collision2D collision)
    {
        if (!canActivate)
            return;

        if (!collision.collider.CompareTag("Player"))
            return;

        Rigidbody2D playerRb = collision.collider.GetComponent<Rigidbody2D>();

        if (playerRb == null || springCollider == null)
            return;

        float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);

        if (impactSpeed < minImpactSpeedToActivate)
        {
            if (debugLogs)
                Debug.Log("SpringPad: удар слишком слабый", this);

            return;
        }

        float playerBottomY = collision.collider.bounds.min.y;
        float springTopY = springCollider.bounds.max.y;

        if (playerBottomY < springTopY - playerAboveTolerance)
        {
            if (debugLogs)
                Debug.Log("SpringPad: игрок не сверху пружины", this);

            return;
        }

        Activate(playerRb);
    }

    private void Activate(Rigidbody2D playerRb)
    {
        canActivate = false;

        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.AddForce(Vector2.up * launchForce, ForceMode2D.Impulse);

        if (debugLogs)
            Debug.Log("SpringPad: ПРУЖИНА СРАБОТАЛА!", this);

        StartCoroutine(SpringVisualRoutine());
        Invoke(nameof(ResetCooldown), cooldown);
    }

    private IEnumerator SpringVisualRoutine()
    {
        if (springRenderer != null && pressedSprite != null)
            springRenderer.sprite = pressedSprite;

        yield return new WaitForSeconds(pressedTime);

        if (springRenderer != null && normalSprite != null)
            springRenderer.sprite = normalSprite;
    }

    private void ResetCooldown()
    {
        canActivate = true;
    }
}