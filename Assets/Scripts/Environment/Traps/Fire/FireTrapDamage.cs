using UnityEngine;

public class FireTrapDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (playerHealth.IsDead)
            return;

        if (playerHealth.IsInvulnerable)
            return;

        playerHealth.TakeDamage(damage);
    }
}