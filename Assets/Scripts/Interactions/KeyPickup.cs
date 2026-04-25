using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class KeyPickup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject keyIconUI;

    [Header("Optional Visuals")]
    [SerializeField] private GameObject keyGlowObject;
    [SerializeField] private MonoBehaviour keyPulseScript;

    [Header("Player Tag")]
    [SerializeField] private string playerTag = "Player";

    [Header("Pickup Settings")]
    [SerializeField] private float pickupDistance = 1.2f;

    [Header("Fly To UI Animation")]
    [SerializeField] private bool animateToUI = true;
    [SerializeField] private float flyDuration = 0.35f;
    [SerializeField] private float flyArcHeight = 0.6f;
    [SerializeField] private float endScaleMultiplier = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public static bool HasKey { get; private set; }

    private bool pickedUp;
    private bool playerIsNearby;
    private Collider2D nearbyPlayerCollider;
    private Collider2D keyCollider;
    private Camera mainCamera;

    private void Awake()
    {
        keyCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        // При загрузке сцены считаем, что ключ ещё не взят
        HasKey = false;

        if (keyIconUI != null)
            keyIconUI.SetActive(false);

        if (debugLogs)
            Debug.Log($"[KEY] Awake. keyIconUI={(keyIconUI != null ? "OK" : "NULL")}", this);
    }

    private void Update()
    {
        if (pickedUp)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (!playerIsNearby)
            return;

        // Клик мышкой в редакторе
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            TryPickupByPointer(screenPos);
        }

        // Тап на телефоне
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            TryPickupByPointer(screenPos);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
            return;

        bool isPlayer =
            other.CompareTag(playerTag) ||
            (other.transform.root != null && other.transform.root.CompareTag(playerTag));

        if (!isPlayer)
            return;

        playerIsNearby = true;
        nearbyPlayerCollider = other;

        if (debugLogs)
            Debug.Log("[KEY] Player is NEAR the key. Waiting for click/tap.", this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (pickedUp)
            return;

        bool isPlayer =
            other.CompareTag(playerTag) ||
            (other.transform.root != null && other.transform.root.CompareTag(playerTag));

        if (!isPlayer)
            return;

        playerIsNearby = false;

        if (nearbyPlayerCollider == other)
            nearbyPlayerCollider = null;

        if (debugLogs)
            Debug.Log("[KEY] Player moved away from key.", this);
    }

    private void TryPickupByPointer(Vector2 screenPos)
    {
        if (mainCamera == null)
            return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2 point2D = new Vector2(worldPos.x, worldPos.y);

        Collider2D hit = Physics2D.OverlapPoint(point2D);

        if (debugLogs)
        {
            string hitName = hit != null ? hit.name : "NULL";
            Debug.Log($"[KEY] Pointer click/tap. Hit={hitName}", this);
        }

        if (hit == null)
            return;

        if (hit != keyCollider)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj == null)
        {
            Debug.LogWarning("[KEY] Player with required tag not found.", this);
            return;
        }

        float distance = Vector2.Distance(playerObj.transform.position, transform.position);

        if (debugLogs)
            Debug.Log($"[KEY] Clicked on key. Distance to player = {distance:F2}", this);

        if (distance > pickupDistance)
        {
            if (debugLogs)
                Debug.Log("[KEY] Too far from key. Pickup denied.", this);

            return;
        }

        PickupKey();
    }

    private void PickupKey()
    {
        if (pickedUp)
            return;

        pickedUp = true;
        HasKey = true;

        if (debugLogs)
            Debug.Log("[KEY] Key picked up successfully.", this);

        if (keyCollider != null)
            keyCollider.enabled = false;

        // Отключаем glow
        if (keyGlowObject != null)
            keyGlowObject.SetActive(false);

        // Отключаем пульс ключа
        if (keyPulseScript != null)
            keyPulseScript.enabled = false;

        if (animateToUI && keyIconUI != null)
            StartCoroutine(AnimateKeyToUI());
        else
            FinishPickupInstant();
    }

    private IEnumerator AnimateKeyToUI()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 startWorldPos = transform.position;
        Vector3 startScale = transform.localScale;

        Vector3 targetWorldPos = startWorldPos;

        RectTransform keyIconRect = keyIconUI.GetComponent<RectTransform>();
        Canvas canvas = keyIconUI.GetComponentInParent<Canvas>();

        if (mainCamera != null && keyIconRect != null && canvas != null)
        {
            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                keyIconRect.position
            );

            screenPoint.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            targetWorldPos = mainCamera.ScreenToWorldPoint(screenPoint);
            targetWorldPos.z = transform.position.z;
        }

        float time = 0f;
        Vector3 endScale = startScale * endScaleMultiplier;

        while (time < flyDuration)
        {
            float t = time / flyDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = Vector3.Lerp(startWorldPos, targetWorldPos, smoothT);
            pos.y += Mathf.Sin(smoothT * Mathf.PI) * flyArcHeight;

            transform.position = pos;
            transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetWorldPos;
        transform.localScale = endScale;

        FinishPickupInstant();
    }

    private void FinishPickupInstant()
    {
        if (keyIconUI != null)
        {
            keyIconUI.SetActive(true);

            if (debugLogs)
                Debug.Log("[KEY] KeyIcon UI enabled.", this);
        }
        else
        {
            Debug.LogWarning("[KEY] keyIconUI is NULL. Assign KeyIcon in Inspector.", this);
        }

        gameObject.SetActive(false);
    }

    public static bool PlayerHasKey()
    {
        return HasKey;
    }

    public static void ConsumeKey()
    {
        HasKey = false;
    }
}