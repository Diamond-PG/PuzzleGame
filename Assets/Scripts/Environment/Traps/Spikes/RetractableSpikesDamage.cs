using UnityEngine;

public class RetractableSpikesDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(
        Collider2D other
    )
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(
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

        if (playerHealth.IsInvulnerable)
            return;

        playerHealth.TakeDamage(
            damage
        );
    }
}