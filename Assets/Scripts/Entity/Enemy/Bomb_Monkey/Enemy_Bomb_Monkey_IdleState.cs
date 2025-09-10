using UnityEngine;


public class Enemy_Bomb_Monkey_IdleState : Enemy_Bomb_Monkey_State
{
    public Enemy_Bomb_Monkey_IdleState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine,
        string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        bombMonkey.BattleState.moveSpeedBoost = 0f;
        stateTimer = bombMonkey.IdleTime;
    }

    public override void Update()
    {
        base.Update();

        if (bombMonkey.PlayerDetection() == true)
        {
            enemyStateMachine.ChangeState(bombMonkey.BattleState);
        }

        if (stateTimer < 0)
        {
            if (bombMonkey.IsGroundDetected == false || bombMonkey.IsWallDetected)
            {
                bombMonkey.Flip();
            }

            enemyStateMachine.ChangeState(bombMonkey.MoveState);
        }
    }
}