using UnityEngine;

public class RetractableSpikesTrigger : MonoBehaviour
{
    [SerializeField] private RetractableSpikes spikes;

    private void Awake()
    {
        if (spikes == null)
        {
            spikes =
                GetComponentInParent<
                    RetractableSpikes
                >();
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<
                PlayerHealth
            >();

        if (playerHealth == null)
            return;

        if (playerHealth.IsDead)
            return;

        if (spikes != null)
            spikes.PlayerEnteredTrigger();
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<
                PlayerHealth
            >();

        if (playerHealth == null)
            return;

        if (spikes != null)
            spikes.PlayerExitedTrigger();
    }
}