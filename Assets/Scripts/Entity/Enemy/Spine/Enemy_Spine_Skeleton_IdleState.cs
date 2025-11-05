using UnityEngine;


    public class Enemy_Spine_IdleState : Enemy_Spine_State
    {
        
        public Enemy_Spine_IdleState(Enemy_Spine enemySpine,Enemy_Spine_StateMachine enemyStateMachine, string animBoolName) : base(enemySpine, enemyStateMachine, animBoolName)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            
            stateTimer = enemySpine.IdleTime;
        }

        public override void Update()
        {
            base.Update();

        if (enemySpine.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(enemySpine.BattleState);
        }

        if (stateTimer < 0)
        {
                if (enemySpine.IsGroundDetected == false || enemySpine.IsWallDetected)
                {
                    enemySpine.Flip();
                }
                enemyStateMachine.ChangeState(enemySpine.MoveState);
        }
                
            
        }
        
    }
