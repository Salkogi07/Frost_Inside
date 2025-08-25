using Unity.Netcode;
using UnityEngine;

public class Enemy_JumpState : EnemyState
{
    private Entity _entity;
    private EnemyJumpData _jumpData;
    public string StateName;

    public Enemy_JumpState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName,EnemyJumpData JumpData) : base(enemy, enemyStateMachine, animBoolName)
    {
        _jumpData =  JumpData;
    }

    public override void Enter()
    {   
        base.Enter();
        enemy.SetVelocity(rigidbody.linearVelocity.x,_jumpData.jumpForce);
       
        _jumpData.isJumping = true;
        
        
    }
    
    public override void Update()
    {
        base.Update();
        
        if (rigidbody.linearVelocity.y < 0)
                    ChangeStates(StateName);
        
        if (_jumpData.isJumping && enemy.IsGroundDetected)
        {
            _jumpData.isJumping = false;
        }
    }

    public void ChangeStates(string stateName)
    {
        if (stateName == "Chase_director")
        {
            enemyStateMachine.ChangeState(enemy.ChaseDirector);
        }
        if (stateName == "Move_director")
        {
            enemyStateMachine.ChangeState(enemy.MoveDirector);
        }
    }
    // public bool MeasureWallHeight()
    // {
    //     
    //     Vector2 startpoint = new Vector2(enemy.EnemyCheck().x + (enemy.wallCheckDistance * enemy.FacingDirection), enemy.EnemyCheck().y);
    //        
    //     while (enemy.jumpForce <= startpoint.y)
    //     {
    //         startpoint = new Vector2(startpoint.x, startpoint.y + 0.01f);
    //         RaycastHit2D hit = Physics2D.Raycast(startpoint, Vector2.up, enemy.jumpForce, enemy.whatIsWall);
    //         return true;
    //     }
    //     return false;
    // }
    // public void jumping(float xVelocity, float yVelocity)
    // {
    //     
    //     Debug.Log("너의 곁에 있으면 나는 행복해  "+rigidbody);
    //     enemy.JumPing = true;
    //     enemy.SetVelocity(xVelocity, yVelocity,enemy.JumPing);
    //     
    //     enemy.jumpingTime = enemy.jumpCoolTime;
    // }
}