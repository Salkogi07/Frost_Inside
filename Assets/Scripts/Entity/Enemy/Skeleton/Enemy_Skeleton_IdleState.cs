using UnityEngine;


    public class Enemy_Skeleton_IdleState : Enemy_Skeleton_State
    {
        
        public Enemy_Skeleton_IdleState(Enemy_Skeleton_StateMachine enemyStateMachine, string animBoolName, Enemy_Skeleton enemySkeleton) : base(null, enemyStateMachine, animBoolName)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            
            stateTimer = enemySkeleton.IdleTime;
        }

        public override void Update()
        {
            base.Update();

        if (enemySkeleton.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(enemySkeleton.BattleState);
        }

        if (stateTimer < 0)
        {
                if (enemySkeleton.IsGroundDetected == false || enemySkeleton.IsWallDetected)
                {
                    enemySkeleton.Flip();
                }
                enemyStateMachine.ChangeState(enemySkeleton.MoveState);
        }
                
            
        }
        
    }
