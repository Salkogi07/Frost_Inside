using UnityEngine;


public class Enemy_Bomb_Monkey_DeadState : Enemy_Bomb_Monkey_State
{
    private Collider2D col;

    public Enemy_Bomb_Monkey_DeadState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine,
        string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
        col = bombMonkey.GetComponent<Collider2D>();
    }


    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entered");

        if (bombMonkey.IsServer)
        {
            enemyStateMachine.SwitchOffStateMachine();
            bombMonkey.ReturnToPool();
        }
    }
}