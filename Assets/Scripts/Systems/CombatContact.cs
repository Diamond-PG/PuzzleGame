using UnityEngine;

internal readonly struct CombatContact
{
    public readonly Vector3 Position;
    public readonly float Direction;

    private CombatContact(Vector3 position, float direction)
    {
        Position = position;
        Direction = direction < 0f ? -1f : 1f;
    }

    public static CombatContact FromHitbox(Collider2D target, Vector2 strikeCenter, float direction)
    {
        Bounds bounds = target.bounds;
        float side = direction < 0f ? bounds.max.x + 0.05f : bounds.min.x - 0.05f;
        float margin = bounds.extents.y * 0.15f;
        float height = Mathf.Clamp(strikeCenter.y, bounds.min.y + margin, bounds.max.y - margin);
        Vector2 surface = target.ClosestPoint(new Vector2(side, height));
        return new CombatContact(new Vector3(surface.x, surface.y, target.transform.position.z), direction);
    }

    public static CombatContact Between(Transform attacker, Component target, Collider2D targetCollider)
    {
        Vector3 targetPosition = target.transform.position;
        Vector2 origin = attacker != null ? (Vector2)attacker.position : (Vector2)targetPosition + Vector2.left;
        float direction = targetPosition.x - origin.x;
        return targetCollider != null && targetCollider.enabled
            ? FromHitbox(targetCollider, origin, direction)
            : new CombatContact(targetPosition, direction);
    }
}
