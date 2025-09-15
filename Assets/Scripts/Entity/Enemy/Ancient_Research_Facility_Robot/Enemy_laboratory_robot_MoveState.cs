
    using UnityEngine;

    public class Enemy_laboratory_robot_MoveState : Enemy_laboratory_robot_State
    {
        public Enemy_laboratory_robot_MoveState(Enemy_laboratory_robot laboratoryRobot, Enemy_laboratory_robot_StateMachine enemyStateMachine, string animBoolName) : base(laboratoryRobot, enemyStateMachine, animBoolName)
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
        
        if (laboratoryRobot.JumpState._jumpData.IsJumpDetected )
        {
            if (laboratoryRobot.CanPerformLeap())
            {
                
                laboratoryRobot.JumpState.StateName = "Move_director";
                            enemyStateMachine.ChangeState(laboratoryRobot.JumpState);
            }
            
        }
        if (!laboratoryRobot.JumpState._jumpData.isJumping && (laboratoryRobot.IsWallDetected || !laboratoryRobot.IsGroundDetected))
        {
            enemyStateMachine.ChangeState(laboratoryRobot.IdleState);
        }
        if (laboratoryRobot.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(laboratoryRobot.BattleState);
        }
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

            laboratoryRobot.SetVelocity(laboratoryRobot.MoveSpeed * laboratoryRobot.FacingDirection, rb.linearVelocity.y);
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
