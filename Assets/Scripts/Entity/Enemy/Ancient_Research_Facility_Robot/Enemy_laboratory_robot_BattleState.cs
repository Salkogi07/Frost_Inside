
using UnityEngine;

public class Enemy_laboratory_robot_BattleState : Enemy_laboratory_robot_State
{
    
    private Transform player;
    private float lastTimeWasInBattle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Enemy_laboratory_robot_BattleState(Enemy_laboratory_robot laboratoryRobot, Enemy_laboratory_robot_StateMachine enemyStateMachine, string animBoolName) : base(laboratoryRobot, enemyStateMachine, animBoolName)
    {
        
    }

    public override void Enter()
    {
        
        base.Enter();
        UpdateBattleTimer();
        
        if (player == null)
        {
            player = laboratoryRobot.GetPlayerReference();
        }
        // player ??= enemy.GetPlayerReference();
        if (shouldRetreat())
        {
            rb.linearVelocity = new Vector2(laboratoryRobot.retreatVelocity.x*-DirectionToPlayer(),laboratoryRobot.retreatVelocity.y);
            laboratoryRobot.HandleFlip(DirectionToPlayer());
        }
        
    }

    public override void Update()
    {
        base.Update();
        
        if (laboratoryRobot.PlayerDetection() == true)
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            enemyStateMachine.ChangeState(laboratoryRobot.IdleState);
        }
        if (WithinAttackRange() && laboratoryRobot.PlayerDetection())
        {
            enemyStateMachine.ChangeState(laboratoryRobot.AttackState);
        }

        if( laboratoryRobot.JumpState._jumpData.IsJumpDetected||player.transform.position.y > laboratoryRobot.transform.position.y+1f)
        {
            
            laboratoryRobot.JumpState.StateName = "Chase_director";
            enemyStateMachine.ChangeState(laboratoryRobot.JumpState);
            
        }
        
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        laboratoryRobot.SetVelocity(laboratoryRobot.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() =>Time.time > lastTimeWasInBattle + laboratoryRobot.battleTimeDuration;
    private bool WithinAttackRange() => DistanceToPlayer() < laboratoryRobot.attackDistance;
    private bool shouldRetreat() => DistanceToPlayer() < laboratoryRobot.minRetreatDistance;
        
    
    
    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        return Mathf.Abs(player.position.x - laboratoryRobot.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }
        return player.position.x > laboratoryRobot.transform.position.x ? 1 : -1;
    }
}
