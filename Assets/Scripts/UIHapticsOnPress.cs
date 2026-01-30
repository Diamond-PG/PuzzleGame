using UnityEngine;
using UnityEngine.EventSystems;

public class UIHapticsOnPress : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private bool enable = true;
    [SerializeField] private bool mobileOnly = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enable) return;
        if (mobileOnly && Application.isEditor) return;

        MicroHaptics.TinyClick();
    }
}