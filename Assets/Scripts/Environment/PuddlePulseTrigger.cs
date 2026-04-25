using UnityEngine;

public class PuddlePulseTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string pulseAnimationName = "Puddle_Pulse";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayPulse()
    {
        if (animator == null) return;

        animator.Play(pulseAnimationName, 0, 0f);
    }
}