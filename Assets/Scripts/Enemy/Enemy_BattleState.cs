using Scripts.Enemy;
using UnityEngine;

public class Enemy_BattleState : EnemyState
{
    
    private Transform player;
    private float lastTimeWasInBattle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Enemy_BattleState(Enemy enemy, Enemy_StateMachine enemyStateMachine, string animBoolName) : base(enemy, enemyStateMachine, animBoolName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null)
        {
            player = enemy.PlayerDetection().transform;
        }

        if (shouldRetreat())
        {
            rigidbody.linearVelocity = new Vector2(enemy.retreatVelocity.x*-DirectionToPlayer(),enemy.retreatVelocity.y);
            enemy.HandleFlip(DirectionToPlayer());
        }
        
    }

    public override void Update()
    {
        base.Update();
        if (enemy.PlayerDetection() == true)
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            enemyStateMachine.ChangeState(enemy.IdleState);
        }
        if (WithinAttackRange() && enemy.PlayerDetection())
        {
            enemyStateMachine.ChangeState(enemy.AttackState);
        }
        else
        {
            enemy.SetVelocity(enemy.battleMoveSpeed * DirectionToPlayer(), rigidbody.linearVelocity.y);
        }
    }
    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() =>Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;
    private bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;
    private bool shouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;
        
    
    
    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }
        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
