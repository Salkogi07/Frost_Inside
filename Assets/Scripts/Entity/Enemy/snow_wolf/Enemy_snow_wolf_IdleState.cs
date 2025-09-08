using UnityEngine;


    public class Enemy_snow_wolf_IdleState : Enemy_snow_wolf_State
    {
        public Enemy_snow_wolf_IdleState(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName) : base(SnowWolf, enemyStateMachine, animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            stateTimer = SnowWolf.IdleTime;
        }

        public override void Update()
        {
            base.Update();

        if (SnowWolf.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(SnowWolf.BattleState);
        }

        if (stateTimer < 0)
        {
                if (SnowWolf.IsGroundDetected == false || SnowWolf.IsWallDetected)
                {
                    SnowWolf.Flip();
                }
                enemyStateMachine.ChangeState(SnowWolf.MoveState);
        }
                
            
        }
        
    }
