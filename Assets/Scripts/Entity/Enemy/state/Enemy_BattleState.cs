
using UnityEngine;

public class Enemy_BattleState : Chase_director
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
        UpdateBattleTimer();
        
        if (player == null)
        {
            player = enemy.GetPlayerReference();
        }
        // player ??= enemy.GetPlayerReference();
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
            enemyStateMachine.ChangeState(enemy.IdleDirector);
        }
        if (WithinAttackRange() && enemy.PlayerDetection()&& enemy.GetState<Enemy_AttackState>() != null)
        {
            enemyStateMachine.ChangeState(enemy.GetState<Enemy_AttackState>());
        }

        if(player.transform.position.y > enemy.transform.position.y+1f && enemy.GetState<Enemy_JumpState>()  != null)
        {
            Debug.Log("Player "+player.transform.position.y +"enemy "+enemy.transform.position.y );
            enemy.GetState<Enemy_JumpState>().StateName = "Chase_director";
            enemyStateMachine.ChangeState(enemy.GetState<Enemy_JumpState>());
            
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
