using UnityEngine;
using UnityEngine.EventSystems;

public class HoldDirectionButton :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    ICancelHandler
{
    public bool IsHeld { get; private set; }

    /*
     * Запоминаем конкретный палец, который нажал кнопку.
     * Это важно для телефона, где одновременно могут быть
     * несколько касаний разных кнопок.
     */
    private int activePointerId = int.MinValue;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            return;

        activePointerId = eventData.pointerId;
        IsHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData == null)
        {
            ReleaseButton();
            return;
        }

        if (activePointerId == eventData.pointerId)
            ReleaseButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        /*
         * Если палец уехал за пределы кнопки,
         * прекращаем считать её зажатой.
         */
        if (eventData == null ||
            activePointerId == eventData.pointerId)
        {
            ReleaseButton();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        /*
         * Начало небольшого движения пальца не должно
         * само по себе включать другую кнопку.
         * Текущее удержание пока сохраняем.
         */
    }

    public void OnDrag(PointerEventData eventData)
    {
        /*
         * Само перемещение пальца обрабатывается
         * системой событий Unity.
         *
         * Если палец выйдет за границу кнопки,
         * сработает OnPointerExit и состояние сбросится.
         */
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        /*
         * На некоторых телефонах завершение перетаскивания
         * может прийти вместо обычного PointerUp.
         */
        if (eventData == null ||
            activePointerId == eventData.pointerId)
        {
            ReleaseButton();
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        /*
         * Сброс при отмене события системой.
         */
        ReleaseButton();
    }

    private void OnDisable()
    {
        ReleaseButton();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ReleaseButton();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            ReleaseButton();
    }

    private void ReleaseButton()
    {
        IsHeld = false;
        activePointerId = int.MinValue;
    }
}