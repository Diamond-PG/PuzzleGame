using UnityEngine;
using UnityEngine.EventSystems;

public class HoldDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public bool IsHeld { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsHeld = false;
    }

    // Чтобы удержание не "залипало", если палец уехал/сорвался
    public void OnDrag(PointerEventData eventData) { }

    private void OnDisable()
    {
        IsHeld = false;
    }
}