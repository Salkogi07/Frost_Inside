using UnityEngine;


    public class Enemy_White_Parasite_IdleState : Enemy_White_Parasite_State
    {
        public Enemy_White_Parasite_IdleState(Enemy_White_Parasite whiteParasite, Enemy_White_Parasite_StateMachine enemyStateMachine, string animBoolName) : base(whiteParasite, enemyStateMachine, animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            stateTimer = whiteParasite.IdleTime;
        }

        public override void Update()
        {
            base.Update();

        if (whiteParasite.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(whiteParasite.BattleState);
        }

        if (stateTimer < 0)
        {
                if (whiteParasite.IsGroundDetected == false || whiteParasite.IsWallDetected)
                {
                    whiteParasite.Flip();
                }
                enemyStateMachine.ChangeState(whiteParasite.MoveState);
        }
                
            
        }
        
    }
