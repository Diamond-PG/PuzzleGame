using UnityEngine;

public class DropTimer : MonoBehaviour
{
    public Animator animator;

    [SerializeField] private PuddlePulseTrigger puddle;

    public float delayMin = 5.5f;
    public float delayMax = 8f;

    private float timer;
    private bool isPlaying;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        animator.enabled = false;
        isPlaying = false;
        SetNewTime();
    }

    void Update()
    {
        if (!isPlaying)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                StartDrop();
            }
        }
    }

    void StartDrop()
    {
        isPlaying = true;
        animator.enabled = true;
        animator.Play("Drop_Fall", 0, 0f);

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(StopDrop), clipLength);
    }

    void StopDrop()
    {
        animator.enabled = false;
        isPlaying = false;
        SetNewTime();
    }

    void SetNewTime()
    {
        timer = Random.Range(delayMin, delayMax);
    }

    public void OnDropHit()
    {
        if (puddle != null)
        {
            puddle.PlayPulse();
        }
    }
}