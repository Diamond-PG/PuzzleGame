using UnityEngine;

public class ConstantFireSoundZone : MonoBehaviour
{
    [SerializeField] private ConstantFireSound constantFireSound;

    private void Awake()
    {
        if (constantFireSound == null)
        {
            constantFireSound =
                GetComponentInParent<ConstantFireSound>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (playerHealth.IsDead)
            return;

        if (constantFireSound != null)
        {
            constantFireSound.SetPlayerNear(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (constantFireSound != null)
        {
            constantFireSound.SetPlayerNear(false);
        }
    }
}