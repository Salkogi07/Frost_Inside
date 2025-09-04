
using UnityEngine;

public class Enemy_Bomb_Monkey_AttackState : Enemy_Bomb_Monkey_State
{
    public Enemy_Bomb_Monkey_AttackState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine, string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        
        if(triggerCalled)
        {
            enemyStateMachine.ChangeState(bombMonkey.BattleState);
            // 끝없는 추격
        }
        
        
            
    }
}
