
using UnityEngine;

public class Enemy_Spine_BattleState : Enemy_Spine_State
{
    
    private Transform player;
    private float lastTimeWasInBattle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Enemy_Spine_BattleState(Enemy_Spine enemySpine,Enemy_Spine_StateMachine enemyStateMachine, string animBoolName) : base(enemySpine, enemyStateMachine, animBoolName)
    {
    }
    
    public override void Enter()
    {
        
        base.Enter();
        UpdateBattleTimer();
        
        if (player == null)
        {
            player = enemySpine.GetPlayerReference();
        }
        // player ??= enemy.GetPlayerReference();
        if (shouldRetreat())
        {
            rb.linearVelocity = new Vector2(enemySpine.retreatVelocity.x*-DirectionToPlayer(),enemySpine.retreatVelocity.y);
            enemySpine.HandleFlip(DirectionToPlayer());
        }
        
    }

    public override void Update()
    {
        base.Update();
        
        if (enemySpine.PlayerDetection() == true)
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            enemyStateMachine.ChangeState(enemySpine.IdleState);
        }
        if (WithinAttackRange() && enemySpine.PlayerDetection())
        {
            enemyStateMachine.ChangeState(enemySpine.AttackState);
        }

        if( enemySpine.JumpState._jumpData.IsJumpDetected||player.transform.position.y > enemySpine.transform.position.y+1f)
        {
            
            enemySpine.JumpState.StateName = "Chase_director";
            enemyStateMachine.ChangeState(enemySpine.JumpState);
            
        }
        
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        enemySpine.SetVelocity(enemySpine.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() =>Time.time > lastTimeWasInBattle + enemySpine.battleTimeDuration;
    private bool WithinAttackRange() => DistanceToPlayer() < enemySpine.attackDistance;
    private bool shouldRetreat() => DistanceToPlayer() < enemySpine.minRetreatDistance;
        
    
    
    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        return Mathf.Abs(player.position.x - enemySpine.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }
        return player.position.x > enemySpine.transform.position.x ? 1 : -1;
    }
}
