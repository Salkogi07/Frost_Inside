
using UnityEngine;

public class Enemy_Skeleton_AttackState : EnemyState
{
    public Enemy_Skeleton_AttackState(Enemy enemy, Enemy_StateMachine stateMachine, string animBoolName) : base(enemy,
        stateMachine, animBoolName)
    {
        
    }

    public override void Update()
    {
        base.Update();
        
        if(triggerCalled)
        {
            enemyStateMachine.ChangeState(enemy.ChaseDirector);
            // 끝없는 추격
        }
        
        
            
    }
}
