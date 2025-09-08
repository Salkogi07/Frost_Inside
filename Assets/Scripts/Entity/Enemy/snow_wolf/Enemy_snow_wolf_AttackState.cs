
using UnityEngine;

public class Enemy_snow_wolf_AttackState : Enemy_snow_wolf_State
{
    public Enemy_snow_wolf_AttackState(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName) : base(SnowWolf, enemyStateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        
        if(triggerCalled)
        {
            enemyStateMachine.ChangeState(SnowWolf.BattleState);
            // 끝없는 추격
        }
        
        
            
    }
}
