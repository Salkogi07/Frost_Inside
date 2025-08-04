
using UnityEngine;

public class Enemy_AttackState : EnemyState
{
    public Enemy_AttackState(Enemy enemy, Enemy_StateMachine stateMachine, string animBoolName) : base(enemy,
        stateMachine, animBoolName)
    {
        
    }

    public override void Update()
    {
        base.Update();
        
        if(triggerCalled)
        {
            enemyStateMachine.ChangeState(enemy.BattleState);
            // 끝없는 추격
        }
        
        
            
    }
}
