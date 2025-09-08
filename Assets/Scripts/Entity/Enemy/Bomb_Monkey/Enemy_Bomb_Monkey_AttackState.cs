using UnityEngine;
using System.Collections.Generic;

public class Enemy_Bomb_Monkey_AttackState : Enemy_Bomb_Monkey_State
{
    public Enemy_Bomb_Monkey_AttackState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine, string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        GameObject boomEffect = Object.Instantiate(bombMonkey.prf, bombMonkey.transform.position, Quaternion.identity);
        boomEffect.GetComponent<explosion_damage>().damage = bombMonkey.stats.damage.GetValue();
        Debug.Log(boomEffect.GetComponent<explosion_damage>().damage);
        
        enemyStateMachine.ChangeState(bombMonkey.DeadState);
    }
}
