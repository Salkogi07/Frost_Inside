
using UnityEngine;

public class Enemy_Spine_AttackState : Enemy_Spine_State
{
    public Enemy_Spine_AttackState(Enemy_Spine enemySpine, Enemy_Spine_StateMachine stateMachine, string animBoolName) : base(enemySpine,
        stateMachine, animBoolName)
    {
        
    }

    public override void Update()
    {
        base.Update();
        
        if(triggerCalled)
        {
            enemyStateMachine.ChangeState(enemySpine.BattleState);
            // 끝없는 추격
        }
        
        
            
    }
}
