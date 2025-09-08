using Unity.Netcode;
using UnityEngine;

public class Enemy_snow_wolf_JumpState : Enemy_snow_wolf_State
{
    public EnemyJumpData _jumpData;
    public string StateName;
    private Vector2 originalGroundCheckLocalPos;


    public Enemy_snow_wolf_JumpState(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName, EnemyJumpData jumpData) : base(SnowWolf, enemyStateMachine, animBoolName)
    {
        _jumpData = jumpData;
        
    }

    public override void Enter()
    {   
        base.Enter();
        originalGroundCheckLocalPos = SnowWolf.groundCheck.localPosition;
        SnowWolf.groundCheck.localPosition = new Vector2(0f, originalGroundCheckLocalPos.y);

        // 수평 속도와 수직 속도를 함께 적용하여 '도약' 구현
        float horizontalVelocity = SnowWolf.battleMoveSpeed * SnowWolf.FacingDirection;
        SnowWolf.SetVelocity(horizontalVelocity, _jumpData.jumpForce);
        _jumpData.isJumping = true;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (_jumpData.isJumping && SnowWolf.IsGroundDetected)
        {
            _jumpData.isJumping = false;
            SnowWolf.groundCheck.localPosition = originalGroundCheckLocalPos;
            ChangeStates(StateName);
        }
    }

    private void ChangeStates(string stateName)
    {
        if (stateName == "Chase_director")
        {
            enemyStateMachine.ChangeState(SnowWolf.BattleState);
        }
        if (stateName == "Move_director")
        {
            enemyStateMachine.ChangeState(SnowWolf.MoveState);
        }
    }
}