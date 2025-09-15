
using UnityEngine;

public class Enemy_laboratory_robot_AttackState : Enemy_laboratory_robot_State
{
    public Enemy_laboratory_robot_AttackState(Enemy_laboratory_robot laboratoryRobot, Enemy_laboratory_robot_StateMachine stateMachine, string animBoolName) : base(laboratoryRobot,
        stateMachine, animBoolName)
    {
        
    }

    public override void Update()
    {
        base.Update();
        
        if(triggerCalled)
        {
            enemyStateMachine.ChangeState(laboratoryRobot.BattleState);
            // 끝없는 추격
        }
        
        
            
    }
}
