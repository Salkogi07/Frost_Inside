using UnityEngine;


    public class Enemy_laboratory_robot_IdleState : Enemy_laboratory_robot_State
    {
        public Enemy_laboratory_robot_IdleState(Enemy_laboratory_robot laboratoryRobot, Enemy_laboratory_robot_StateMachine enemyStateMachine, string animBoolName) : base(laboratoryRobot, enemyStateMachine, animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            stateTimer = laboratoryRobot.IdleTime;
        }

        public override void Update()
        {
            base.Update();

        if (laboratoryRobot.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(laboratoryRobot.BattleState);
        }

        if (stateTimer < 0)
        {
                if (laboratoryRobot.IsGroundDetected == false || laboratoryRobot.IsWallDetected)
                {
                    laboratoryRobot.Flip();
                }
                enemyStateMachine.ChangeState(laboratoryRobot.MoveState);
        }
                
            
        }
        
    }
