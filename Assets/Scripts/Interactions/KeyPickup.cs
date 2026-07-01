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

    [Header("Pickup Sound")]
    [SerializeField] private AudioSource pickupAudioSource;
    [SerializeField] private bool playPickupSound = true;

    [Header("Pickup Haptics")]
    [SerializeField] private bool usePickupHaptics = true;

    [Header("Fly To UI Animation")]
    [SerializeField] private bool animateToUI = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public static bool HasKey { get; private set; }

    private bool pickedUp;
    private bool playerIsNearby;
    private Collider2D nearbyPlayerCollider;
    private Collider2D keyCollider;
    private Camera mainCamera;
    private SpriteRenderer[] spriteRenderers;
    private PickupFlyEffect flyEffect;

    private void Awake()
    {
        keyCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        flyEffect = GetComponent<PickupFlyEffect>();

        if (pickupAudioSource == null)
            pickupAudioSource = GetComponent<AudioSource>();

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

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryPickupByPointer(Mouse.current.position.ReadValue());

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            TryPickupByPointer(Touchscreen.current.primaryTouch.position.ReadValue());
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

        if (usePickupHaptics)
            MicroHaptics.TinyClick();

        if (playPickupSound && pickupAudioSource != null && pickupAudioSource.clip != null)
            pickupAudioSource.PlayOneShot(pickupAudioSource.clip);

        if (keyGlowObject != null)
            keyGlowObject.SetActive(false);

        if (keyPulseScript != null)
            keyPulseScript.enabled = false;

        if (animateToUI && keyIconUI != null && flyEffect != null)
            StartCoroutine(flyEffect.FlyToUI(keyIconUI, FinishPickupInstant));
        else
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

        HideWorldKeyVisuals();

        float waitTime = 0f;

        if (playPickupSound && pickupAudioSource != null && pickupAudioSource.clip != null)
            waitTime = pickupAudioSource.clip.length;

        StartCoroutine(DisableAfterSound(waitTime));
    }

    private void HideWorldKeyVisuals()
    {
        if (spriteRenderers == null)
            return;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
                sr.enabled = false;
        }
    }

    private IEnumerator DisableAfterSound(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

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