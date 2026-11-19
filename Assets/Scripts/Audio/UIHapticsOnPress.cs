using UnityEngine;
using UnityEngine.EventSystems;

public class UIHapticsOnPress : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private bool enable = true;
    [SerializeField] private bool mobileOnly = true;
    [SerializeField] private bool playOnPointerDown = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playOnPointerDown)
            PlayHaptic();
    }

    public void PlayHaptic()
    {
        if (!isActiveAndEnabled || !enable) return;
        if (mobileOnly && Application.isEditor) return;

        MicroHaptics.TinyClick();
    }
}