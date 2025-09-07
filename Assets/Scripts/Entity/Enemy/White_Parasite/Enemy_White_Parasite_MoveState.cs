
    using UnityEngine;

    public class Enemy_White_Parasite_MoveState : Enemy_White_Parasite_State
    {
        public Enemy_White_Parasite_MoveState(Enemy_White_Parasite whiteParasite, Enemy_White_Parasite_StateMachine enemyStateMachine, string animBoolName) : base(whiteParasite, enemyStateMachine, animBoolName)
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
        
        if (whiteParasite.JumpState._jumpData.IsJumpDetected )
        {
            if (whiteParasite.CanPerformLeap())
            {
                
                whiteParasite.JumpState.StateName = "Move_director";
                            enemyStateMachine.ChangeState(whiteParasite.JumpState);
            }
            
        }
        if (!whiteParasite.JumpState._jumpData.isJumping && (whiteParasite.IsWallDetected || !whiteParasite.IsGroundDetected))
        {
            enemyStateMachine.ChangeState(whiteParasite.IdleState);
        }
        if (whiteParasite.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(whiteParasite.BattleState);
        }
        }

        public override void FiexedUpdate()
        {
            base.FiexedUpdate();

            whiteParasite.SetVelocity(whiteParasite.MoveSpeed * whiteParasite.FacingDirection, rb.linearVelocity.y);
        }


    
}
