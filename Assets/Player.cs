using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;

public class Player : Agent
{
    [SerializeField] private float _moveSpeed = 20f;
    [SerializeField] private float _jumpForce = 30f;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private Rigidbody2D _rd2d;
    [SerializeField] private TextMeshProUGUI _txtmpReward;
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private RayPerceptionSensorComponent2D _raySensorObstacle;
    [SerializeField] private RayPerceptionSensorComponent2D _raySensorFruit;

    void FixedUpdate()
    {
        if (_rd2d.velocity.y < 0)
            gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public override void OnEpisodeBegin()
    {
        _rd2d.velocity = Vector2.zero;
        _rd2d.angularVelocity = 0f;
        transform.localPosition = new Vector3(0, 4, transform.localPosition.z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(_rd2d.velocity.x);
        sensor.AddObservation(_isGrounded);

        //sensor.AddObservation(_gameManager.Fruits[0].transform.localPosition.x);
        //sensor.AddObservation(_gameManager.Fruits[0].transform.localPosition.y);

        var rayPerceptionInput = _raySensorObstacle.GetRayPerceptionInput();
        var rayPerceptionOutput = RayPerceptionSensor.Perceive(rayPerceptionInput);
        // Thêm quan sát vào sensor và vẽ tia cảm biến
        foreach (var rayOutput in rayPerceptionOutput.RayOutputs)
        {
            // Thêm khoảng cách chuẩn hóa
            sensor.AddObservation(rayOutput.HitFraction);

            // Thêm chỉ số tag của đối tượng
            sensor.AddObservation(rayOutput.HitTagIndex);
        }


        rayPerceptionInput = _raySensorFruit.GetRayPerceptionInput();
        rayPerceptionOutput = RayPerceptionSensor.Perceive(rayPerceptionInput);
        bool detectFruit = false;
        float hitFraction = 0;
        int hitTagIndex = 0;
        foreach (var rayOutput in rayPerceptionOutput.RayOutputs)
        {
            if (rayOutput.HitTaggedObject)
            {
                hitFraction = rayOutput.HitFraction;
                hitTagIndex = rayOutput.HitTagIndex;
                detectFruit = true;
                break;
            }
        }
        sensor.AddObservation(detectFruit);
        sensor.AddObservation(hitFraction);
        sensor.AddObservation(hitTagIndex);
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
        }

        Vector2 velocity = _rd2d.velocity;
        int action = act[0];
        int jumpAction = act[1];
        //Debug.Log($"MoveAgent {action} {jumpAction}");
        if (action == 1)
        {
            velocity.x = -_moveSpeed;  // Di chuyển sang trái
        }
        else if (action == 2)
        {
            velocity.x = _moveSpeed;  // Di chuyển sang phải
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
        if (GetCumulativeReward() < 0.8f)
        {
            AddReward(value);
            _txtmpReward.text = $"{GetCumulativeReward()}";
        }
        else
        {
            SetReward(1f);
            EndEpisode();
        }
    }

}
