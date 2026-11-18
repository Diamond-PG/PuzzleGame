using UnityEngine;
using UnityEngine.EventSystems;

public class JumpButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private PlayerJump playerJump;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("UP BUTTON PRESSED");

        if (playerJump != null)
        {
            playerJump.Jump();
        }
        else
        {
            Debug.LogError("PlayerJump не подключён в JumpButton!");
        }
    }
}