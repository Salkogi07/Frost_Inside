using UnityEngine;


    public class Enemy_Skeleton_IdleState : EnemyState
    {
        
        public Enemy_Skeleton_IdleState(Enemy_StateMachine enemyStateMachine, string animBoolName, Enemy_Skeleton enemySkeleton) : base(null, enemyStateMachine, animBoolName)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            
            stateTimer = enemy.IdleTime;
        }

        public override void Update()
        {
            base.Update();

        if (enemy.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(enemy.ChaseDirector);
        }

        if (stateTimer < 0)
            {
                if (enemy.IsGroundDetected == false || enemy.IsWallDetected)
                {
                    enemy.Flip();
                }
                enemyStateMachine.ChangeState(enemy.MoveDirector);
            }
                
            
        }
        
    }
