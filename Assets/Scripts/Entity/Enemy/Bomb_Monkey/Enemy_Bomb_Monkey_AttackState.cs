using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class Enemy_Bomb_Monkey_AttackState : Enemy_Bomb_Monkey_State
{
    public Enemy_Bomb_Monkey_AttackState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine, string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (!bombMonkey.IsServer)
        {
            enemyStateMachine.ChangeState(bombMonkey.DeadState);
            return;
        }
        
        
        GameObject boomEffect = Object.Instantiate(bombMonkey.prf, bombMonkey.transform.position, Quaternion.identity);

        boomEffect.GetComponent<NetworkObject>().Spawn();
        int calculatedDamage = bombMonkey.stats.damage.GetValue();
        boomEffect.GetComponent<Explosion_damage>().SetDamageRpc(calculatedDamage);
        Debug.Log(boomEffect.GetComponent<Explosion_damage>().damage);
        
        enemyStateMachine.ChangeState(bombMonkey.DeadState);
    }
}
