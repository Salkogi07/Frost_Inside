
    using UnityEngine;

    public class Enemy_Spine_MoveState : Enemy_Spine_State
    {
        
        public Enemy_Spine_MoveState(Enemy_Spine enemySpine, Enemy_Spine_StateMachine enemyStateMachine, string animBoolName) : base(
            enemySpine,enemyStateMachine, animBoolName)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            // if (enemySkeleton.IsGroundDetected == false || enemySkeleton.IsWallDetected)
            // {
            //     enemySkeleton.Flip();
            // }
        }

        public override void Update()
        {
        base.Update();
        
        if (enemySpine.JumpState._jumpData.IsJumpDetected )
        {
            if (enemySpine.CanPerformLeap())
            {
                
                enemySpine.JumpState.StateName = "Move_director";
                            enemyStateMachine.ChangeState(enemySpine.JumpState);
            }
            
        }
        if (!enemySpine.JumpState._jumpData.isJumping && (enemySpine.IsWallDetected || !enemySpine.IsGroundDetected))
        {
            enemyStateMachine.ChangeState(enemySpine.IdleState);
        }
        if (enemySpine.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(enemySpine.BattleState);
        }
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

            enemySpine.SetVelocity(enemySpine.MoveSpeed * enemySpine.FacingDirection, rb.linearVelocity.y);
        }


    // public bool MeasureWallHeight()
    // {
    //     
    //     Vector2 startpoint = new Vector2(enemy.transform.position.x + (enemy.wallCheckDistance * enemy.FacingDirection), enemy.transform.position.y);
    //        
    //     while (enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce <= startpoint.y)
    //     {
    //         startpoint = new Vector2(startpoint.x, startpoint.y + 0.01f);
    //         RaycastHit2D hit = Physics2D.Raycast(startpoint, Vector2.up, enemy.GetState<Enemy_JumpState>()._jumpData.jumpForce, enemy.whatIsWall);
    //         return true;
    //     }
    //     return false;
    // }
}
