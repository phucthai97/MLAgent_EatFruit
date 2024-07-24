using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

/// <summary>
/// Author: PhucThai
/// </summary>
public class BotStateMachine : Agent
{
    public enum BotState
    {
        grounded,
        jump,
        idle,
        walk,
        attack,
        roll,
        takingdamge,
        specialattack
    }
    [SerializeField] private int _teamId = 0;

    public BotState superState;
    [SerializeField] private BotState _subState;
    [SerializeField] private Vector3 _bodyVelocity;
    // state variables
    BotStateFactory _states;
    BotBaseState _currentState;

    [Header("[MOVE]")]
    [SerializeField] private int _directionMove;

    [Space(7)]
    [Header("[JUMP]")]
    private bool _isJumpPressed;
    [SerializeField] private float _jumpForce = 10.7f;

    //Parachute
    private bool _parachutePressed = false;
    [SerializeField] private bool _isParachuting = false;
    [SerializeField] private float _lastParachuteTime;
    [SerializeField] private LayerMask _raycastLayer;
    [SerializeField] private float _grounDistance;

    //Jump up to interact with the ground
    [SerializeField] private bool _isGrounded;
    private Transform _groundCheck;

    //Jump up to interact with the rocks
    [SerializeField] private bool _isRocked;

    [Space(7)]
    [Header("[ATTACK]")]

    private bool _isAttackPressed;
    [SerializeField] private float _lastAttackTime;
    private float _attackDelayThreshold = 2.25f;

    [Space(7)]
    [Header("[ROLL]")]
    [SerializeField] private int _maxNumRolls = 2;
    [SerializeField] private int _countRoll = 0;
    [SerializeField] private float _lastRollTime;
    [SerializeField] float _rollDelayThreshold = 2f;
    [SerializeField] private bool _isRecovery = false;
    private bool _isRollPressed;

    [Space(7)]
    [Header("[REFERENCES]")]
    [SerializeField] private bool _isController;
    [SerializeField] private Rigidbody2D _rb2D;
    [SerializeField] private Hero _hero;
    private PlayerControlUI _playerControlUI;

    [Space(7)]
    [Header("[ML AGENTS]")]
    [SerializeField] private float _weaponsUse = 0;
    private TrainingManager _trainingManager;
    //[SerializeField] private BufferSensorComponent m_BufferSensor;

    /// <summary>
    /// Properties
    /// </summary>
    /// 
    public BotBaseState CurrentState { get => _currentState; set => _currentState = value; }
    public bool IsJumpPressed { get => _isJumpPressed; set => _isJumpPressed = value; }
    public float JumpForce { get => _jumpForce; }
    public bool IsGrounded { get => _isGrounded; set => _isGrounded = value; }
    public bool IsRocked { get => _isRocked; set => _isRocked = value; }
    public Transform GroundCheck { get => _groundCheck; set => _groundCheck = value; }
    public int DirectionMove
    {
        get => _directionMove;
        set
        {
            _directionMove = value;
            //Debug.Log($"Set directionMove with {value}");
        }
    }
    public bool IsAttackPressed { get => _isAttackPressed; set => _isAttackPressed = value; }
    public bool IsRollPressed { get => _isRollPressed; set => _isRollPressed = value; }
    public Vector3 BodyVelocity { get => _bodyVelocity; }
    public float AttackDelayThreshold { get => _attackDelayThreshold; set => _attackDelayThreshold = value; }
    public float LastAttackTime { get => _lastAttackTime; set => _lastAttackTime = value; }
    public Hero Hero { get => _hero; }
    public BotState SubState { get => _subState; set => _subState = value; }
    public int MaxNumRolls { get => _maxNumRolls; set => _maxNumRolls = value; }
    public float LastRollTime { get => _lastRollTime; set => _lastRollTime = value; }
    public float RollDelayThreshold { get => _rollDelayThreshold; }
    public int CountRoll { get => _countRoll; set => _countRoll = value; }
    public bool IsRecovery { get => _isRecovery; set => _isRecovery = value; }
    public bool ParachutePressed { get => _parachutePressed; set => _parachutePressed = value; }
    public bool IsParachuting { get => _isParachuting; set => _isParachuting = value; }
    public float GrounDistance { get => _grounDistance; set => _grounDistance = value; }
    public PlayerControlUI PlayerControlUI { get => _playerControlUI; }
    public bool IsController { get => _isController; set => _isController = value; }
    public TrainingManager TrainingManager { get => _trainingManager; set => _trainingManager = value; }

    // Start is called before the first frame update
    void Start()
    {
        if (!_hero.IsTraining)
            Initial();
    }

    public void Initial()
    {
        _rb2D = GetComponent<Rigidbody2D>();

        _groundCheck = transform.Find("groundCheck");
        _raycastLayer = LayerMask.GetMask("Ground");

        // setup state
        _states = new BotStateFactory(this, _hero, _hero.PlayerData, _rb2D);
        _currentState = _states.Grounded();
        _currentState.EnterState(BotState.grounded);

        //For traning ML Agent
        _teamId = GetComponent<BehaviorParameters>().TeamId;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_hero.PlayerData.IsKill)
            return;

        _bodyVelocity = _rb2D.velocity;
        _currentState.UpdateStates();
    }

    void OnMove(InputValue value)
    {
        if (_isController)
            DirectionMove = (short)value.Get<Vector2>().x;
    }

    void OnJumpUI()
    {
        //print($"OnJump UI");
        if (_isController)
        {
            if (!_hero.PlayerData.IsJumping && _bodyVelocity.y == 0)
                _isJumpPressed = true;
            else if (_isParachuting && (Time.time - _lastParachuteTime > 0.1f))
            {
                //Debug.Log($"Time.time - _lastParachuteTime {Time.time - _lastParachuteTime}");
                _isParachuting = false;
                _hero.ResetRigidBodyHero();
            }
            else if (_bodyVelocity.y != 0 && _grounDistance > 4.2f && (Time.time - _lastParachuteTime > 0.1f))
                _parachutePressed = true;

            _lastParachuteTime = Time.time;
        }
    }

    void OnAttackUI()
    {
        //print($"OnAttack UI");
        if (_isController)
        {
            if (!_hero.PlayerData.IsAttacking)
                _isAttackPressed = true;
        }
    }

    void OnRollUI()
    {
        //print($"OnRoll UI");
        if (_isController)
        {
            if (!_hero.PlayerData.IsRolling && !_hero.PlayerData.IsAttacking && !(_bodyVelocity.y < 0) && !_isRecovery)
                _isRollPressed = true;
        }
    }

    #region CHANGE WEAPON BY NUMBER KEYCAP
    public void OnWeapon1()
    {
        PreSetWeapon(0);
    }

    public void OnWeapon2()
    {
        PreSetWeapon(1);
    }

    public void OnWeapon3()
    {
        PreSetWeapon(2);
    }

    public void OnWeapon4()
    {
        PreSetWeapon(3);
    }

    private void PreSetWeapon(int index)
    {
        if (Hero.PlayerData.EquipWeaponList.Count > index)
        {
            //_hero.PlayerData.NumAttack = 0;
            if (_hero.PlayerData.UpdateOrReloadCurrentWeapon(index))
            {
                // level reference
                var levelWeapon = GlobalInfo.instance.userUpgradeStatData.Single(t => t.Key == $"weapon{_hero.GetIndexWeapon() + 1}").Value;
                //gameObject.GetPhotonView().RPC("SetWeapon", PhotonTargets.All, _hero.GetIndexWeapon(), levelWeapon);
                _hero.SetWeapon(_hero.GetIndexWeapon(), levelWeapon);
            }
        }
        else
            Debug.Log($"You clicked at slot {index + 1} but WeaponList only has {Hero.PlayerData.EquipWeaponList.Count} weapons");
    }
    
    #endregion

    public void RecoveryStamina()
    {
        _isRecovery = true;
        StartCoroutine(CoroutineRecovery());
    }

    IEnumerator CoroutineRecovery()
    {
        yield return new WaitForSecondsRealtime(3.8f);
        _isRecovery = false;
    }

    /// <summary>
    /// Calculate the distance from the character to the ground
    /// </summary>
    public void CheckGroundDistance()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, _raycastLayer);
        if (hit.collider != null)
        {
            //Debug.DrawRay(transform.position, Vector2.down * hit.distance, Color.red);
            _grounDistance = hit.distance;
        }
    }

    #region FOR TRAINING AGENT
    public override void OnEpisodeBegin()
    {
        // //Get random pos
        // if (_hero.IsTraining)
        // {
        //     if (Random.value > 0.3f)
        //     {
        //         Vector2 pos = new Vector3(Random.Range(-17f, 18) + _trainingManager.TransParentGame.position.x
        //                                , Random.Range(-2f, 9) + _trainingManager.TransParentGame.position.y
        //                                , 0);
        //         transform.position = pos;
        //     }
        //     else
        //     {
        //         List<float> list = new List<float>() { -1.7f, 1.7f };
        //         Vector2 pos = _trainingManager.SupplyDropPositions[_trainingManager.GetRandomPositionAndRemove()].position; ;
        //         transform.position = new Vector2(pos.x + list[Random.Range(0, list.Count)], pos.y + 2.7f);
        //     }
        // }

        //Vector2 pos = new Vector2(0 + _trainingManager.TransParentGame.position.x
        //                                , -2 + _trainingManager.TransParentGame.position.y);
        //transform.position = pos;

        //_rb2D.velocity = Vector2.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(new Vector2(transform.localPosition.x, transform.localPosition.y));
        sensor.AddObservation(new Vector2(_rb2D.velocity.x, _rb2D.velocity.y));
        sensor.AddObservation(_hero.PlayerData.IsJumping ? 1.0f : 0.0f);
        sensor.AddObservation(_isGrounded ? 1.0f : 0.0f);
        sensor.AddObservation(_hero.PlayerData.IsRolling ? 1.0f : 0.0f);
        sensor.AddObservation((float)CountRoll);
        sensor.AddObservation(_hero.PlayerData.IsAttacking ? 1.0f : 0.0f);
        sensor.AddObservation(_hero.PlayerData.IsTakingDamage ? 1.0f : 0.0f);
        sensor.AddObservation(_weaponsUse);
        //sensor.AddObservation(_hero.PlayerData.CurrentHealth);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log($"localPosition {new Vector2(transform.localPosition.x, transform.localPosition.y)} - {new Vector2(_rb2D.velocity.x, _rb2D.velocity.y)}");

        // Move the agent using the action.
        MoveAgent(actions.DiscreteActions);
    }

    private void MoveAgent(ActionSegment<int> act)
    {
        if (_hero.IsTraining)
        {
            AddReward(-0.0013f);
            if (GetCumulativeReward() < -0.99)
            {
                //_trainingManager.UpdateScoreText(-1, _hero.NumBOT);
                //transform.localPosition = new Vector3(1, 0, 0);

                // if (_trainingManager.SuppliesList.Count > 0)
                // {
                //     foreach (SupplyTrainingAgent child in _trainingManager.SuppliesList)
                //     {
                //         if (child != null)
                //             Destroy(child.gameObject);
                //     }
                // }
                // _trainingManager.CurrentItemCount = 0;
                // _trainingManager.CreateSupplies();

                SetRewardForAgent(-1);
            }
        }

        if (_isController)
        {
            if (act[0] == 1)
                DirectionMove = -1;
            else if (act[0] == 2)
                DirectionMove = 1;
            else
                DirectionMove = 0;
        }


        if (act[1] == 1)
            OnJumpUI();

        if (act[2] == 1)
            OnRollUI();

        if (act[3] == 1)
            OnAttackUI();

        if (act[4] != 0)
        {
            PreSetWeapon(act[4] - 1);
            _weaponsUse = act[4] - 1;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        //Move
        if (Input.GetKey(KeyCode.A))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.D))
            discreteActionsOut[0] = 2;
        else
            discreteActionsOut[0] = 0;

        //Jump
        if (Input.GetKey(KeyCode.J))
            discreteActionsOut[1] = 1;
        else
            discreteActionsOut[1] = 0;

        //Roll
        if (Input.GetKey(KeyCode.L))
            discreteActionsOut[2] = 1;
        else
            discreteActionsOut[2] = 0;

        //Attack
        if (Input.GetKey(KeyCode.K))
            discreteActionsOut[3] = 1;
        else
            discreteActionsOut[3] = 0;

        if (Input.GetKey(KeyCode.Alpha1))
            discreteActionsOut[4] = 1;
        else if (Input.GetKey(KeyCode.Alpha2))
            discreteActionsOut[4] = 2;
        else
            discreteActionsOut[4] = 0;
    }

    public void AddRewardForAgent(float value)
    {
        AddReward(value);
        //Debug.Log($"AddReward: {value}");
        //_trainingManager.UpdateScoreText(value, _hero.NumBOT);
    }

    public void SetRewardForAgent(float value)
    {
        SetReward(value);
        _trainingManager.UpdateScoreText(value, _teamId);
        //CurriculumManager.Instance.Calculate(value);
        //CurriculumManager.Instance.ResetSupplies();
        EndEpisode();
    }

    public void SetRandomPos()
    {
        _trainingManager.SetRandomPos(gameObject);
    }

    #endregion
}