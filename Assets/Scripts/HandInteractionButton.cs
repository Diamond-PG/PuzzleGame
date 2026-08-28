using UnityEngine;
using UnityEngine.UI;

public interface IHandInteractable
{
    bool CanHandInteract { get; }
    void HandInteract();
}

public class HandInteractionButton : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("INTERACTION DISTANCE")]
    [Tooltip("Расстояние от игрока, на котором загорается кнопка руки.")]
    [SerializeField] private float interactionDistance = 1.0f;

    [Header("DETECTION")]
    [Tooltip("Какие слои проверять. Пока можно оставить Everything.")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("BUTTON")]
    [SerializeField] private Button handButton;

    [Header("DEBUG")]
    [SerializeField] private bool debugLogs = false;

    private IHandInteractable currentInteractable;
    private MonoBehaviour currentInteractableBehaviour;

    private void Awake()
    {
        if (handButton == null)
            handButton = GetComponent<Button>();

        FindPlayer();

        if (handButton != null)
        {
            handButton.onClick.RemoveListener(UseHand);
            handButton.onClick.AddListener(UseHand);

            // В начале рука неактивна.
            handButton.interactable = false;
        }
        else
        {
            Debug.LogError(
                "[HAND BUTTON] Button component not found!",
                this
            );
        }
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        FindNearestInteractable();
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void FindNearestInteractable()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                player.position,
                interactionDistance,
                interactableLayers
            );

        IHandInteractable bestInteractable = null;
        MonoBehaviour bestBehaviour = null;

        float bestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            MonoBehaviour[] behaviours =
                hit.GetComponentsInParent<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                if (behaviour is not IHandInteractable interactable)
                    continue;

                if (!interactable.CanHandInteract)
                    continue;

                float distance =
                    Vector2.Distance(
                        player.position,
                        behaviour.transform.position
                    );

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestInteractable = interactable;
                    bestBehaviour = behaviour;
                }
            }
        }

        bool changed =
            bestBehaviour != currentInteractableBehaviour;

        currentInteractable = bestInteractable;
        currentInteractableBehaviour = bestBehaviour;

        if (handButton != null)
        {
            // Это автоматически переключает:
            // false = тусклая рука
            // true  = яркая рука
            handButton.interactable =
                currentInteractable != null;
        }

        if (debugLogs && changed)
        {
            if (currentInteractableBehaviour != null)
            {
                Debug.Log(
                    "[HAND BUTTON] Interaction available: " +
                    currentInteractableBehaviour.name,
                    this
                );
            }
            else
            {
                Debug.Log(
                    "[HAND BUTTON] No interaction nearby.",
                    this
                );
            }
        }
    }

    private void UseHand()
    {
        if (currentInteractable == null)
            return;

        if (!currentInteractable.CanHandInteract)
            return;

        if (debugLogs &&
            currentInteractableBehaviour != null)
        {
            Debug.Log(
                "[HAND BUTTON] Interact with: " +
                currentInteractableBehaviour.name,
                this
            );
        }

        currentInteractable.HandInteract();

        // Сразу перепроверяем состояние после взаимодействия.
        FindNearestInteractable();
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Gizmos.DrawWireSphere(
            player.position,
            interactionDistance
        );
    }

    private void OnDestroy()
    {
        if (handButton != null)
            handButton.onClick.RemoveListener(UseHand);
    }
}