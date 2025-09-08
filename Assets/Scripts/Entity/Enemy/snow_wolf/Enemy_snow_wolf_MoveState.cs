
    using UnityEngine;

    public class Enemy_snow_wolf_MoveState : Enemy_snow_wolf_State
    {
        public Enemy_snow_wolf_MoveState(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName) : base(SnowWolf, enemyStateMachine, animBoolName)
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
        
        if (SnowWolf.JumpState._jumpData.IsJumpDetected )
        {
            if (SnowWolf.CanPerformLeap())
            {
                
                SnowWolf.JumpState.StateName = "Move_director";
                            enemyStateMachine.ChangeState(SnowWolf.JumpState);
            }
            
        }
        if (!SnowWolf.JumpState._jumpData.isJumping && (SnowWolf.IsWallDetected || !SnowWolf.IsGroundDetected))
        {
            enemyStateMachine.ChangeState(SnowWolf.IdleState);
        }
        if (SnowWolf.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(SnowWolf.BattleState);
        }
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

            SnowWolf.SetVelocity(SnowWolf.MoveSpeed * SnowWolf.FacingDirection, rb.linearVelocity.y);
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
