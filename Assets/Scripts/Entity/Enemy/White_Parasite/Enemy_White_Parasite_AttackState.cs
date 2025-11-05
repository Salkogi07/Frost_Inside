
using UnityEngine;

public class Enemy_White_Parasite_AttackState : Enemy_White_Parasite_State
{
    public Enemy_White_Parasite_AttackState(Enemy_White_Parasite whiteParasite, Enemy_White_Parasite_StateMachine enemyStateMachine, string animBoolName) : base(whiteParasite, enemyStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        if(triggerCalled)
        {
            enemyStateMachine.ChangeState(whiteParasite.BattleState);
            // 끝없는 추격
        }
        
        // int calculatedStamina = whiteParasite.stats.damage.GetValue();
        // PlayerCondition.ChangeTemperature(calculatedStamina);
        // enemyStateMachine.ChangeState(whiteParasite.DeadState);
    }
}
