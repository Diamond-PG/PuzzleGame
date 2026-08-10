using UnityEngine;

public class FireSoundZone : MonoBehaviour
{
    [SerializeField] private TimedFireTrap timedFireTrap;

    private void Awake()
    {
        if (timedFireTrap == null)
        {
            timedFireTrap =
                GetComponentInParent<TimedFireTrap>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (timedFireTrap != null)
        {
            timedFireTrap.SetPlayerNear(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (timedFireTrap != null)
        {
            timedFireTrap.SetPlayerNear(false);
        }
    }
}