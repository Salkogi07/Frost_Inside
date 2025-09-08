using UnityEngine;

public class Enemy_snow_wolf_GroggyState : Enemy_snow_wolf_State
{
    private float Groggying;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Enemy_snow_wolf_GroggyState(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName, float groggying) : base(SnowWolf, enemyStateMachine, animBoolName)
    {
        Groggying = groggying;
    }

    public override void Enter()
    {
        base.Enter();
         
    }
    

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if(Groggying <= 0)
        {
            enemyStateMachine.ChangeState(SnowWolf.IdleState);
        }
        else
        {
            GrooggingTime();
        }
    }
    private void GrooggingTime() =>  Groggying  -= Time.deltaTime;
}
