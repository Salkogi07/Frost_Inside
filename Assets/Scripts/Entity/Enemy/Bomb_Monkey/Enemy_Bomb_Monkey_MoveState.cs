
    using UnityEngine;

public class Enemy_Bomb_Monkey_MoveState : Enemy_Bomb_Monkey_State
{
    public Enemy_Bomb_Monkey_MoveState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine, string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
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
        
        if (bombMonkey.JumpState._jumpData.IsJumpDetected )
        {
            if (bombMonkey.CanPerformLeap())
            {

                bombMonkey.JumpState.StateName = "Move_director";
                            enemyStateMachine.ChangeState(bombMonkey.JumpState);
            }
            
        }
        if (!bombMonkey.JumpState._jumpData.isJumping && (bombMonkey.IsWallDetected || !bombMonkey.IsGroundDetected))
        {
            enemyStateMachine.ChangeState(bombMonkey.IdleState);
        }
        if (bombMonkey.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(bombMonkey.BattleState);
        }
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

        bombMonkey.SetVelocity(bombMonkey.MoveSpeed * bombMonkey.FacingDirection, rb.linearVelocity.y);
        }


    
}
