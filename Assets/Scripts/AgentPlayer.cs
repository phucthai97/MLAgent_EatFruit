using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;

public class AgentPlayer : Agent
{
    [SerializeField] private float _moveSpeed = 17f;
    [SerializeField] private float _jumpForce = 43f;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private Rigidbody2D _rd2d;
    //[SerializeField] private TextMeshProUGUI _txtmpCountWin;
    //[SerializeField] private int _countWin = 0;
    //[SerializeField] private int _countLose = 0;
    [SerializeField] private RayPerceptionSensorComponent2D _raySensorFruit;
    [SerializeField] GameManager _gameManager;

    void Start()
    {
        transform.localPosition = new Vector3(0, 4, transform.localPosition.z);
        _rd2d.velocity = Vector2.zero;
        _rd2d.angularVelocity = 0f;
    }

    void FixedUpdate()
    {
        if (_rd2d.velocity.y < 0)
            gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public override void OnEpisodeBegin()
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 localPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
        Vector2 velocity = new Vector2(_rd2d.velocity.x, _rd2d.velocity.y);
        sensor.AddObservation(localPos);
        sensor.AddObservation(velocity);
        //sensor.AddObservation(transform.localPosition.x);
        //sensor.AddObservation(transform.localPosition.y);
        //sensor.AddObservation(_rd2d.velocity.x);
        //sensor.AddObservation(_rd2d.velocity.y);
        sensor.AddObservation(_isGrounded);

        RayPerceptionInput rayPerceptionInput = _raySensorFruit.GetRayPerceptionInput();
        RayPerceptionOutput rayPerceptionOutput = RayPerceptionSensor.Perceive(rayPerceptionInput);
        bool detectFruit = false;
        Vector2 pos = new Vector2();
        foreach (var rayOutput in rayPerceptionOutput.RayOutputs)
        {
            if (rayOutput.HitTaggedObject)
            {
                detectFruit = true;
                pos = new Vector2(rayOutput.HitGameObject.transform.localPosition.x
                                , rayOutput.HitGameObject.transform.localPosition.y);
                break;
            }
        }
        sensor.AddObservation(detectFruit);
        sensor.AddObservation(pos);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move the agent using the action.
        MoveAgent(actions.DiscreteActions);
    }

    private void MoveAgent(ActionSegment<int> act)
    {
        AddReward(-0.0005f);
        //Debug.Log($"CumulativeReward: {GetCumulativeReward()}");
        if (GetCumulativeReward() < -0.99)
        {
            SetReward(-1);
            EndEpisode();
            // _countLose--;
            // _txtmpCountWin.text = $"{_countWin} / {_countLose}";
        }

        Vector2 velocity = _rd2d.velocity;
        int action = act[0];
        int jumpAction = act[1];
        //Debug.Log($"MoveAgent {action} {jumpAction}");
        if (action == 1)
        {
            velocity.x = -_moveSpeed;  // Di chuyển sang trái
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (action == 2)
        {
            velocity.x = _moveSpeed;  // Di chuyển sang phải
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            velocity.x = 0;  // Dừng lại
        }

        // Nhảy
        if (jumpAction == 1 && _isGrounded)
        {
            gameObject.layer = LayerMask.NameToLayer("Jumping");
            //_rd2d.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
            velocity.y = _jumpForce;
            _isGrounded = false;
        }

        _rd2d.velocity = velocity;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //Debug.Log($"Heuristic 0");
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 2;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[1] = 1;
        }
        else
        {
            discreteActionsOut[1] = 0;
        }
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

    public void HandleReWard(float value)
    {
        // if (GetCumulativeReward() < 0.8f)
        // {
        //     AddReward(value);
        //     //Debug.Log($"{GetCumulativeReward()}");
        // }
        // else
        // {
        SetReward(1f);
        // _countWin++;
        // _txtmpCountWin.text = $"{_countWin} / {_countLose}";
        _gameManager.UpdatePoint(-1);
        EndEpisode();
        //}
    }
}
