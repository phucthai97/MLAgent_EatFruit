using UnityEngine;
using UnityEngine.EventSystems;

public class BtnHoldMove : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int _valueBtn;
    [SerializeField] private ControllerPlayer _controllerPlayer;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_controllerPlayer.DirectionMove == 0)
            _controllerPlayer.DirectionMove = _valueBtn;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_controllerPlayer.DirectionMove == _valueBtn)
            _controllerPlayer.DirectionMove = 0;
    }
}
