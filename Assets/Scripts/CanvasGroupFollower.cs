using UnityEngine;

public class CanvasGroupFollower : MonoBehaviour
{
    [SerializeField] private CanvasGroup source; // откуда берём альфу
    [SerializeField] private CanvasGroup target; // куда применяем альфу

    private void Awake()
    {
        if (target == null)
            target = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (source == null || target == null)
            return;

        target.alpha = source.alpha;
    }
}