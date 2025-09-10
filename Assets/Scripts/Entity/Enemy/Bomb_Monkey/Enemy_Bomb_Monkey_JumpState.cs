using Unity.Netcode;
using UnityEngine;

public class Enemy_Bomb_Monkey_JumpState : Enemy_Bomb_Monkey_State
{
    public EnemyJumpData _jumpData;
    public string StateName;
    private Vector2 originalGroundCheckLocalPos;

    public Enemy_Bomb_Monkey_JumpState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine,
        string animBoolName, EnemyJumpData JumpData) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
        _jumpData = JumpData;
    }


    public override void Enter()
    {
        base.Enter();
        originalGroundCheckLocalPos = bombMonkey.groundCheck.localPosition;
        bombMonkey.groundCheck.localPosition = new Vector2(0f, originalGroundCheckLocalPos.y);

        // 수평 속도와 수직 속도를 함께 적용하여 '도약' 구현
        float horizontalVelocity = bombMonkey.battleMoveSpeed * bombMonkey.FacingDirection;
        bombMonkey.SetVelocity(horizontalVelocity, _jumpData.jumpForce);
        _jumpData.isJumping = true;
    }

    public override void Update()
    {
        base.Update();

        if (_jumpData.isJumping && bombMonkey.IsGroundDetected)
        {
            _jumpData.isJumping = false;
            bombMonkey.groundCheck.localPosition = originalGroundCheckLocalPos;
            ChangeStates(StateName);
        }
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        bombMonkey.BattleState.acceleration();
    }


    private void ChangeStates(string stateName)
    {
        if (stateName == "Chase_director")
        {
            enemyStateMachine.ChangeState(bombMonkey.BattleState);
        }

        if (stateName == "Move_director")
        {
            enemyStateMachine.ChangeState(bombMonkey.MoveState);
        }
    }
}