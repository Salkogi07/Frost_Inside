
using UnityEngine;

public class Enemy_GroundedState : Enemy_Grounded_director_State
{
    public Enemy_GroundedState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
    {
        
    }

    public override void Update()
    {
        base.Update();

        if (enemy.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(enemy.BattleDirector);
        }
    }
}
