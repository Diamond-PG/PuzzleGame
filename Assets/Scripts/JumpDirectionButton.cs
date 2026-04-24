using UnityEngine;
using UnityEngine.EventSystems;

public class JumpDirectionButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private PlayerJump playerJump;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("UP BUTTON CLICKED");

        if (playerJump == null)
        {
            Debug.LogError("PlayerJump НЕ подключён!");
            return;
        }

        playerJump.Jump();
    }
}