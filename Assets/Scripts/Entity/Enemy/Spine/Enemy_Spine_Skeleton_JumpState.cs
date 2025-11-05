using Unity.Netcode;
using UnityEngine;

public class Enemy_Spine_JumpState : Enemy_Spine_State
{
    public EnemyJumpData _jumpData;
    public string StateName;
    private Vector2 originalGroundCheckLocalPos;
    public Enemy_Spine_JumpState(Enemy_Spine enemySpine,Enemy_Spine_StateMachine enemyStateMachine, string animBoolName,EnemyJumpData JumpData) : base(enemySpine, enemyStateMachine, animBoolName)
    {
        _jumpData =  JumpData;
    }

    public override void Enter()
    {   
        base.Enter();
        originalGroundCheckLocalPos = enemySpine.groundCheck.localPosition;
        enemySpine.groundCheck.localPosition = new Vector2(0f, originalGroundCheckLocalPos.y);

        // 수평 속도와 수직 속도를 함께 적용하여 '도약' 구현
        float horizontalVelocity = enemySpine.battleMoveSpeed * enemySpine.FacingDirection;
        enemySpine.SetVelocity(horizontalVelocity, _jumpData.jumpForce);
        _jumpData.isJumping = true;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (_jumpData.isJumping && enemySpine.IsGroundDetected)
        {
            _jumpData.isJumping = false;
            enemySpine.groundCheck.localPosition = originalGroundCheckLocalPos;
            ChangeStates(StateName);
        }
    }

    private void ChangeStates(string stateName)
    {
        if (stateName == "Chase_director")
        {
            enemyStateMachine.ChangeState(enemySpine.BattleState);
        }
        if (stateName == "Move_director")
        {
            enemyStateMachine.ChangeState(enemySpine.MoveState);
        }
    }
}