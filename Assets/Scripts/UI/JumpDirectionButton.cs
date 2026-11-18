using UnityEngine;
using UnityEngine.EventSystems;

public class JumpDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private PlayerJump playerJump;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("UP BUTTON CLICKED");

        if (ClimbHook.PlayerIsOnHook)
        {
            ClimbHook.SetClimbVerticalInput(1f);
            return;
        }

        if (playerJump == null)
        {
            Debug.LogError("PlayerJump НЕ подключён!");
            return;
        }

        playerJump.Jump();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ClimbHook.SetClimbVerticalInput(0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ClimbHook.SetClimbVerticalInput(0f);
    }
}