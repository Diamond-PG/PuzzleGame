using UnityEngine;

public class DropTimer : MonoBehaviour
{
    public Animator animator;

    [SerializeField] private PuddlePulseTrigger puddle;

    [Header("Timing")]
    public float delayMin = 5.5f;
    public float delayMax = 8f;

    [Header("Animation")]
    public string dropAnimationName = "Drop_Fall";
    public float dropDuration = 1f;

    private float timer;
    private bool isPlaying;
    private bool pulseDone;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
            animator.enabled = false;

        isPlaying = false;
        pulseDone = false;
        SetNewTime();
    }

    void Update()
    {
        if (isPlaying) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartDrop();
        }
    }

    void StartDrop()
    {
        if (animator == null) return;

        isPlaying = true;
        pulseDone = false;

        animator.enabled = true;
        animator.Play(dropAnimationName, 0, 0f);

        Invoke(nameof(PlayPuddlePulse), dropDuration);
        Invoke(nameof(StopDrop), dropDuration);
    }

    void PlayPuddlePulse()
    {
        if (pulseDone) return;

        pulseDone = true;

        if (puddle != null)
            puddle.PlayPulse();
    }

    void StopDrop()
    {
        if (animator != null)
            animator.enabled = false;

        isPlaying = false;
        SetNewTime();
    }

    void SetNewTime()
    {
        timer = Random.Range(delayMin, delayMax);
    }
}