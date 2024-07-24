using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotAttackState : BotBaseState
{
    public BotAttackState(BotStateMachine currentContext, Hero hero, PlayerData playerData, Rigidbody2D rb2D, BotStateFactory botStateFactory)
    : base(currentContext, hero, playerData, rb2D, botStateFactory)
    {

    }
    public override void EnterState(BotStateMachine.BotState tagState)
    {
        //Debug.Log($"0. EnterState Attack");
        _playerData.IsAttacking = true;
        Ctx.SubState = tagState;
        _rb2D.gravityScale = 1.8f;
        Ctx.IsParachuting = false;
        HandleAttack();
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void CheckSwitchStates()
    {
        //Debug.Log($"1. CheckSwitchStates Attack {_hero.name} {_playerData.name}");
        if (!_playerData.IsAttacking && !_playerData.IsTakingDamage)
        {
            if (Ctx.DirectionMove == 0)
            {
                SwitchState(Factory.Idle(), BotStateMachine.BotState.idle);
            }
            else
            {
                //Debug.Log($"2. CheckSwitchStates Attack {Ctx.DirectionMove}");
                SwitchState(Factory.Walk(), BotStateMachine.BotState.walk);
            }
        }
        else
        {
            if (_playerData.IsTakingDamage)
            {
                SwitchState(Factory.TakingDamage(), BotStateMachine.BotState.takingdamge);
            }

            //FlipX right
            if (Ctx.DirectionMove > 0)
                _playerData.FlipX = false;
            //FlipX left
            else if (Ctx.DirectionMove < 0)
                _playerData.FlipX = true;
        }
    }
    public override void InitializeSubState() { }

    private void HandleAttack()
    {
        //Debug.Log("1. HandleAttack");
        Ctx.IsAttackPressed = false;
        if (_playerData.WeaponType == WeaponType.SWORD)
            SwordControl();
        else if (_playerData.WeaponType == WeaponType.CUTTINGSPEAR)
            CuttingSpearControl();
        else if (_playerData.WeaponType == WeaponType.LIGHTSABER)
            LightSaberControl();
        else if (_playerData.WeaponType == WeaponType.CHAINSAW)
            ChainSawControl();
        else if (_playerData.WeaponType == WeaponType.BAMBOO)
            BambooControl();
        else if (_playerData.WeaponType == WeaponType.TRIDENT)
            TridentControl();
        else
            _hero.PlayAnimation($"atk1_" + _playerData.WeaponType.ToString().ToLower(), false, 1);
    }

    private void TridentControl()
    {
        _hero.PlayAnimation($"atk1_" + _playerData.WeaponType.ToString().ToLower(), false, 1);
        _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 100f);
    }

    private void BambooControl()
    {
        _hero.PlayAnimation($"atk1_" + _playerData.WeaponType.ToString().ToLower(), false, 1);
        _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 200f);
    }

    private void ChainSawControl()
    {
        Ctx.AttackDelayThreshold = 2f;
        if (_playerData.NumAttack == 0 || (Time.time - Ctx.LastAttackTime <= Ctx.AttackDelayThreshold))
            _playerData.NumAttack++;
        else
            _playerData.NumAttack = 1;
        Ctx.LastAttackTime = Time.time;

        _hero.PlayAnimation($"atk{_playerData.NumAttack}_" + _playerData.WeaponType.ToString().ToLower(), false, 1);

        if (_playerData.NumAttack == 3)
        {
            _rb2D.velocity = Vector2.zero;
            _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 150f);
            _playerData.NumAttack = 0;
        }
    }

    private void LightSaberControl()
    {
        Ctx.AttackDelayThreshold = 1.5f;
        if (_playerData.NumAttack == 0 || (Time.time - Ctx.LastAttackTime <= Ctx.AttackDelayThreshold))
            _playerData.NumAttack++;
        else
            _playerData.NumAttack = 1;
        Ctx.LastAttackTime = Time.time;

        _hero.PlayAnimation($"atk{_playerData.NumAttack}_" + _playerData.WeaponType.ToString().ToLower(), false, 1);

        if (_playerData.NumAttack == 2)
        {
            _rb2D.velocity = Vector2.zero;
            _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 150f);
        }
        else if (_playerData.NumAttack == 3)
        {
            _rb2D.velocity = Vector2.zero;
            _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 300f);
            _playerData.NumAttack = 0;
        }
    }

    private void CuttingSpearControl()
    {
        Ctx.AttackDelayThreshold = 2f;
        if (_playerData.NumAttack == 0 || (Time.time - Ctx.LastAttackTime <= Ctx.AttackDelayThreshold))
            _playerData.NumAttack++;
        else
            _playerData.NumAttack = 1;
        Ctx.LastAttackTime = Time.time;

        _hero.PlayAnimation($"atk{_playerData.NumAttack}_" + _playerData.WeaponType.ToString().ToLower(), false, 1);

        if (_playerData.NumAttack == 2)
        {
            _rb2D.velocity = Vector2.zero;
            _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 210f);
        }
        else if (_playerData.NumAttack == 3)
        {
            _rb2D.velocity = Vector2.zero;
            _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 350f);
            _playerData.NumAttack = 0;
        }
    }

    private void SwordControl()
    {
        if (_playerData.NumAttack == 0 || (Time.time - Ctx.LastAttackTime <= Ctx.AttackDelayThreshold))
            _playerData.NumAttack++;
        else
            _playerData.NumAttack = 1;

        Ctx.LastAttackTime = Time.time;
        HandleAttackSword(_playerData.NumAttack);
    }

    private void HandleAttackSword(int numAttack)
    {
        _hero.PlayAnimation($"atk{numAttack}_" + _playerData.WeaponType.ToString().ToLower(), false, 1);

        if (numAttack == 3)
        {
            _rb2D.velocity = Vector2.zero;
            if (!_playerData.IsJumping)
                _rb2D.AddForce((_playerData.FlipX ? Vector3.left : Vector3.right) * 200f);
            else
                _rb2D.AddForce((_playerData.FlipX ? new Vector3(-1, 0.25f, 0) : new Vector3(1, 0.25f, 0)) * 400f);

            _playerData.NumAttack = 0;
        }
    }
}
