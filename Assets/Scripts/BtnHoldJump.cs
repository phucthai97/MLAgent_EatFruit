using UnityEngine;
using UnityEngine.EventSystems;

public class BtnHolJump : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private ControllerPlayer _controllerPlayer;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_controllerPlayer.JumpPressed)
            _controllerPlayer.JumpPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_controllerPlayer.JumpPressed)
            _controllerPlayer.JumpPressed = false;
    }
}
