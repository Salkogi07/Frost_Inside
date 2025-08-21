using UnityEngine;

public class Skeleton_IdleState : EnemyState
{
    private Enemy_Skeleton  Skeleton;
    public Skeleton_IdleState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
    {
        Skeleton = enemy as Enemy_Skeleton;
    }

    public override void Enter()
    {
        base.Enter();
            
        stateTimer = enemy.IdleTime;
    }

    public override void Update()
    {
        base.Update();
        //     
        // if(stateTimer < 0)
        //     enemyStateMachine.ChangeState(Skeleton.MoveState);
    }


}
