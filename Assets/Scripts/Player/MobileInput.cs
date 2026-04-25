using UnityEngine;

public class MobileInput : MonoBehaviour
{
    [Header("Direction Buttons")]
    public HoldDirectionButton left;
    public HoldDirectionButton right;
    public HoldDirectionButton up;
    public HoldDirectionButton down;

    public float Horizontal
    {
        get
        {
            float value = 0f;
            if (right != null && right.IsHeld) value += 1f;
            if (left != null && left.IsHeld) value -= 1f;
            return value;
        }
    }

    public float Vertical
    {
        get
        {
            float value = 0f;
            if (up != null && up.IsHeld) value += 1f;
            if (down != null && down.IsHeld) value -= 1f;
            return value;
        }
    }
}