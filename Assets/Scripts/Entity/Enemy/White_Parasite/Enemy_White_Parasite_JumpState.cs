using Unity.Netcode;
using UnityEngine;

public class Enemy_White_Parasite_JumpState : Enemy_White_Parasite_State
{
    public EnemyJumpData _jumpData;
    public string StateName;
    private Vector2 originalGroundCheckLocalPos;

    public Enemy_White_Parasite_JumpState(Enemy_White_Parasite whiteParasite, Enemy_White_Parasite_StateMachine enemyStateMachine, string animBoolName, EnemyJumpData jumpData) : base(whiteParasite, enemyStateMachine, animBoolName)
    {
        _jumpData = jumpData;
        
    }

    
    public override void Enter()
    {   
        base.Enter();
        originalGroundCheckLocalPos = whiteParasite.groundCheck.localPosition;
        whiteParasite.groundCheck.localPosition = new Vector2(0f, originalGroundCheckLocalPos.y);

        // 수평 속도와 수직 속도를 함께 적용하여 '도약' 구현
        float horizontalVelocity = whiteParasite.battleMoveSpeed * whiteParasite.FacingDirection;
        whiteParasite.SetVelocity(horizontalVelocity, _jumpData.jumpForce);
        _jumpData.isJumping = true;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (_jumpData.isJumping && whiteParasite.IsGroundDetected)
        {
            _jumpData.isJumping = false;
            whiteParasite.groundCheck.localPosition = originalGroundCheckLocalPos;
            ChangeStates(StateName);
        }
    }

    private void ChangeStates(string stateName)
    {
        if (stateName == "Chase_director")
        {
            enemyStateMachine.ChangeState(whiteParasite.BattleState);
        }
        if (stateName == "Move_director")
        {
            enemyStateMachine.ChangeState(whiteParasite.MoveState);
        }
    }
}