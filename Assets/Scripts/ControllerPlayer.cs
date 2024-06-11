using UnityEngine;
using DG.Tweening;

public class ControllerPlayer : MonoBehaviour
{
    [SerializeField] private int _directionMove = 0;
    [SerializeField] private bool _jumpPressed = false;
    [SerializeField] private float _moveSpeed = 17f;
    [SerializeField] private float _jumpForce = 43f;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private Rigidbody2D _rd2d;
    [SerializeField] private GameObject _indicatorGo;
    //[SerializeField] private int _countPoint = 0;
    [SerializeField] private GameManager _gameManager;

    public int DirectionMove { get => _directionMove; set => _directionMove = value; }
    public bool JumpPressed { get => _jumpPressed; set => _jumpPressed = value; }

    void Start()
    {
        _indicatorGo.SetActive(true);
        _indicatorGo.transform.DOLocalMoveY(_indicatorGo.transform.localPosition.y + 0.4f, 0.25f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
    }

    void FixedUpdate()
    {
        if (_rd2d.velocity.y < 0)
            gameObject.layer = LayerMask.NameToLayer("Player");

        Controler();
    }

    private void Controler()
    {
        Vector2 velocity = _rd2d.velocity;
        if (_directionMove == -1)
        {
            velocity.x = -_moveSpeed;
            //transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (_directionMove == 1)
        {
            velocity.x = _moveSpeed;
            //transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
            velocity.x = 0;

        //For jump
        if (_jumpPressed && _isGrounded)
        {
            gameObject.layer = LayerMask.NameToLayer("Jumping");
            velocity.y = _jumpForce;
            _isGrounded = false;
        }

        _rd2d.velocity = velocity;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }

    public void AddReward()
    {
        _gameManager.UpdatePoint(1);
    }

}
