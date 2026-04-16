using UnityEngine;

public class DoorUnlock : MonoBehaviour
{
    [Header("Door Parts")]
    [SerializeField] private GameObject doorVisual;
    [SerializeField] private Collider2D doorCollider;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float interactDistance = 0.8f;

    [Header("Key UI")]
    [SerializeField] private GameObject keyIconUI;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool isOpened;

    private void Awake()
    {
        if (doorVisual == null)
            doorVisual = gameObject;

        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();
    }

    public void TryOpenDoorWithKey()
    {
        if (isOpened)
            return;

        if (!KeyPickup.PlayerHasKey())
        {
            if (debugLogs)
                Debug.Log("[DOOR] No key. Door stays closed.", this);

            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj == null)
        {
            if (debugLogs)
                Debug.LogWarning("[DOOR] Player not found.", this);

            return;
        }

        float distance = Vector2.Distance(playerObj.transform.position, transform.position);

        if (debugLogs)
            Debug.Log($"[DOOR] TryOpenDoorWithKey. Distance = {distance:F2}", this);

        if (distance > interactDistance)
        {
            if (debugLogs)
                Debug.Log("[DOOR] Player too far.", this);

            return;
        }

        OpenDoor();
    }

    private void OpenDoor()
    {
        if (isOpened)
            return;

        isOpened = true;

        if (debugLogs)
            Debug.Log("[DOOR] Door opened. Key consumed.", this);

        // Отключаем коллайдер двери
        if (doorCollider != null)
            doorCollider.enabled = false;

        // Прячем визуал двери
        if (doorVisual != null)
            doorVisual.SetActive(false);

        // Убираем ключ из UI
        if (keyIconUI != null)
            keyIconUI.SetActive(false);

        // Сбрасываем ключ, чтобы нельзя было использовать снова
        KeyPickup.ConsumeKey();
    }
}