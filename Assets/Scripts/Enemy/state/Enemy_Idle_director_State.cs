using UnityEngine;

public class Enemy_Idle_director_State : EnemyState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Enemy_Idle_director_State(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
    {
    }

    
}
