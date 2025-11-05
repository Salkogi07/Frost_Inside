
using UnityEngine;

public class Enemy_White_Parasite_BattleState : Enemy_White_Parasite_State
{
    
    private Transform player;
    private float lastTimeWasInBattle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool Explosive = false;

    public Enemy_White_Parasite_BattleState(Enemy_White_Parasite whiteParasite, Enemy_White_Parasite_StateMachine enemyStateMachine, string animBoolName) : base(whiteParasite, enemyStateMachine, animBoolName)
    {
       
    }

    public override void Enter()
    {
        
        base.Enter();
        UpdateBattleTimer();
        
        if (player == null)
        {
            player = whiteParasite.GetPlayerReference();
        }
        // player ??= enemy.GetPlayerReference();
        if (shouldRetreat())
        {
            rb.linearVelocity = new Vector2(whiteParasite.retreatVelocity.x*-DirectionToPlayer(),whiteParasite.retreatVelocity.y);
            whiteParasite.HandleFlip(DirectionToPlayer());
        }
        
    }

    public override void Update()
    {
        base.Update();
        
        if (whiteParasite.PlayerDetection() == true)
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            enemyStateMachine.ChangeState(whiteParasite.IdleState);
        }
        if (WithinAttackRange() && whiteParasite.PlayerDetection())
        {
            enemyStateMachine.ChangeState(whiteParasite.AttackState);
        }

        if( whiteParasite.JumpState._jumpData.IsJumpDetected||player.transform.position.y > whiteParasite.transform.position.y+1f)
        {
            
            whiteParasite.JumpState.StateName = "Chase_director";
            enemyStateMachine.ChangeState(whiteParasite.JumpState);
            
        }
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            enemyStateMachine.ChangeState(whiteParasite.IdleState);
            return;
        }

        // PlayerDetection()은 RaycastHit2D를 반환하므로, collider가 있는지 확인해야 합니다.
        if (whiteParasite.PlayerDetection().collider != null)
        {
            UpdateBattleTimer();
            Explosive = true;
        }
        else
        {
            Explosive = false;
             
            if (BattleTimeIsOver())
            {
                enemyStateMachine.ChangeState(whiteParasite.IdleState);
            }
        }
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        whiteParasite.SetVelocity(whiteParasite.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() =>Time.time > lastTimeWasInBattle + whiteParasite.battleTimeDuration;
    private bool WithinAttackRange() => DistanceToPlayer() < whiteParasite.attackDistance;
    private bool shouldRetreat() => DistanceToPlayer() < whiteParasite.minRetreatDistance;
        
    
    
    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        return Mathf.Abs(player.position.x - whiteParasite.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }
        return player.position.x > whiteParasite.transform.position.x ? 1 : -1;
    }
}
