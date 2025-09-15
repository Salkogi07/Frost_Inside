using UnityEngine;

public class Enemy_laboratory_robot_GroggyState : Enemy_laboratory_robot_State
{
    private float Groggying;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Enemy_laboratory_robot_GroggyState(Enemy_laboratory_robot laboratoryRobot, Enemy_laboratory_robot_StateMachine enemyStateMachine, string animBoolName) : base(laboratoryRobot, enemyStateMachine, animBoolName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
         Groggying = laboratoryRobot.stats.Groggy;
    }
    

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if(Groggying <= 0)
        {
            enemyStateMachine.ChangeState(laboratoryRobot.IdleState);
        }
        else
        {
            GrooggingTime();
        }
    }
    private void GrooggingTime() =>  Groggying  -= Time.deltaTime;
}
