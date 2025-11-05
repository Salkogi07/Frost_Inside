using UnityEngine;

public class Enemy_Spine_GroggyState : Enemy_Spine_State
{
    private float Groggying;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Enemy_Spine_GroggyState(Enemy_Spine enemySpine, Enemy_Spine_StateMachine enemyStateMachine, string animBoolName) : base(enemySpine, enemyStateMachine, animBoolName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
         Groggying = enemySpine.stats.Groggy;
    }
    

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if(Groggying <= 0)
        {
            enemyStateMachine.ChangeState(enemySpine.IdleState);
        }
        else
        {
            GrooggingTime();
        }
    }
    private void GrooggingTime() =>  Groggying  -= Time.deltaTime;
}
