using Unity.Netcode;
using UnityEngine;

public class Enemy_JumpState : EnemyState
{
    private Entity _entity;
    public EnemyJumpData _jumpData;
    public string StateName;

    public Enemy_JumpState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName,EnemyJumpData JumpData) : base(enemy, enemyStateMachine, animBoolName)
    {
        _jumpData =  JumpData;
    }

    public override void Enter()
    {   
        base.Enter();
        // enemy.SetVelocity(rigidbody.linearVelocity.x,_jumpData.jumpForce);
        enemy.SetVelocity(rigidbody.linearVelocity.x,_jumpData.jumpForce);
        _jumpData.isJumping = true;
        
        
    }
    
    public override void Update()
    {
        base.Update();
        
        
        
        if (_jumpData.isJumping && enemy.IsGroundDetected)
        {
            _jumpData.isJumping = false;
            ChangeStates(StateName);
        }
    }

    private void ChangeStates(string stateName)
    {
        
        if (stateName == "Chase_director")
        {
            Debug.Log("ddddd");
            enemyStateMachine.ChangeState(enemy.ChaseDirector);
        }
        if (stateName == "Move_director")
        {
            enemyStateMachine.ChangeState(enemy.MoveDirector);
        }
    }
    
    
}