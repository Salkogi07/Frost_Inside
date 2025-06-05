using UnityEngine;

namespace Scripts.Enemy
{
    public class Enemy_IdleState : EnemyState
    {
        public Enemy_IdleState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
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
            
            if(stateTimer < 0)
                enemyStateMachine.ChangeState(enemy.MoveState);
        }
    }
}