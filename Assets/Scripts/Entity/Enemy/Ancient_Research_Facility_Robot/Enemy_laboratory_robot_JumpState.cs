using Unity.Netcode;
using UnityEngine;

public class Enemy_laboratory_robot_JumpState : Enemy_laboratory_robot_State
{
    public EnemyJumpData _jumpData;
    public string StateName;
    private Vector2 originalGroundCheckLocalPos;

    public Enemy_laboratory_robot_JumpState(Enemy_laboratory_robot laboratoryRobot, Enemy_laboratory_robot_StateMachine enemyStateMachine, string animBoolName, EnemyJumpData jumpData) : base(laboratoryRobot, enemyStateMachine, animBoolName)
    {
        _jumpData = jumpData;
       
    }

   
    public override void Enter()
    {   
        base.Enter();
        originalGroundCheckLocalPos = laboratoryRobot.groundCheck.localPosition;
        laboratoryRobot.groundCheck.localPosition = new Vector2(0f, originalGroundCheckLocalPos.y);

        // 수평 속도와 수직 속도를 함께 적용하여 '도약' 구현
        float horizontalVelocity = laboratoryRobot.battleMoveSpeed * laboratoryRobot.FacingDirection;
        laboratoryRobot.SetVelocity(horizontalVelocity, _jumpData.jumpForce);
        _jumpData.isJumping = true;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (_jumpData.isJumping && laboratoryRobot.IsGroundDetected)
        {
            _jumpData.isJumping = false;
            laboratoryRobot.groundCheck.localPosition = originalGroundCheckLocalPos;
            ChangeStates(StateName);
        }
    }

    private void ChangeStates(string stateName)
    {
        if (stateName == "Chase_director")
        {
            enemyStateMachine.ChangeState(laboratoryRobot.BattleState);
        }
        if (stateName == "Move_director")
        {
            enemyStateMachine.ChangeState(laboratoryRobot.MoveState);
        }
    }
}