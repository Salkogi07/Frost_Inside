
using UnityEngine;

public class Enemy_Bomb_Monkey_BattleState : Enemy_Bomb_Monkey_State
{
    
    private Transform player;
    private float lastTimeWasInBattle;
    public bool Explosive =false;

    public Enemy_Bomb_Monkey_BattleState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine, string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
    }

   




    public override void Enter()
    {
        
        base.Enter();
        UpdateBattleTimer();
        
        if (player == null)
        {
            player = bombMonkey.GetPlayerReference();
        }
        // player ??= enemy.GetPlayerReference();
        if (shouldRetreat())
        {
            rb.linearVelocity = new Vector2(bombMonkey.retreatVelocity.x*-DirectionToPlayer(), bombMonkey.retreatVelocity.y);
            bombMonkey.HandleFlip(DirectionToPlayer());
        }
        
    }

    public override void Update()
    {
        base.Update();

        if (bombMonkey.PlayerDetection() == true)
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            enemyStateMachine.ChangeState(bombMonkey.IdleState);
        }

        if (bombMonkey.PlayerDetection())
        {
            Explosive = true;
        }
        else
        {
            Explosive = false;
        }

    
        if(bombMonkey.JumpState._jumpData.IsJumpDetected||player.transform.position.y > bombMonkey.transform.position.y+1f)
        {

            bombMonkey.JumpState.StateName = "Chase_director";
            enemyStateMachine.ChangeState(bombMonkey.JumpState);
            
        }
        
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        bombMonkey.SetVelocity(bombMonkey.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() =>Time.time > lastTimeWasInBattle + bombMonkey.battleTimeDuration;
    
    private bool shouldRetreat() => DistanceToPlayer() < bombMonkey.minRetreatDistance;
        
    
    
    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        return Mathf.Abs(player.position.x - bombMonkey.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }
        return player.position.x > bombMonkey.transform.position.x ? 1 : -1;
    }
}
