
using UnityEngine;

public class Enemy_snow_wolf_BattleState : Enemy_snow_wolf_State
{
    
    private Transform player;
    private float lastTimeWasInBattle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Enemy_snow_wolf_BattleState(Enemy_snow_wolf SnowWolf, Enemy_snow_wolf_StateMachine enemyStateMachine, string animBoolName, Transform player, float lastTimeWasInBattle) : base(SnowWolf, enemyStateMachine, animBoolName)
    {
        // this.player = player;
        // this.lastTimeWasInBattle = lastTimeWasInBattle;
    }

    public override void Enter()
    {
        
        base.Enter();
        UpdateBattleTimer();
        
        if (player == null)
        {
            player = SnowWolf.GetPlayerReference();
        }
        // player ??= enemy.GetPlayerReference();
        if (shouldRetreat())
        {
            rb.linearVelocity = new Vector2(SnowWolf.retreatVelocity.x*-DirectionToPlayer(),SnowWolf.retreatVelocity.y);
            SnowWolf.HandleFlip(DirectionToPlayer());
        }
        
    }

    public override void Update()
    {
        base.Update();
        
        if (SnowWolf.PlayerDetection() == true)
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            enemyStateMachine.ChangeState(SnowWolf.IdleState);
        }
        if (WithinAttackRange() && SnowWolf.PlayerDetection())
        {
            enemyStateMachine.ChangeState(SnowWolf.AttackState);
        }

        if( SnowWolf.JumpState._jumpData.IsJumpDetected||player.transform.position.y > SnowWolf.transform.position.y+1f)
        {
            
            SnowWolf.JumpState.StateName = "Chase_director";
            enemyStateMachine.ChangeState(SnowWolf.JumpState);
            
        }
        
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        SnowWolf.SetVelocity(SnowWolf.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() =>Time.time > lastTimeWasInBattle + SnowWolf.battleTimeDuration;
    private bool WithinAttackRange() => DistanceToPlayer() < SnowWolf.attackDistance;
    private bool shouldRetreat() => DistanceToPlayer() < SnowWolf.minRetreatDistance;
        
    
    
    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        return Mathf.Abs(player.position.x - SnowWolf.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }
        return player.position.x > SnowWolf.transform.position.x ? 1 : -1;
    }
}
