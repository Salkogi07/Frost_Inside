
using UnityEngine;

public class Enemy_Bomb_Monkey_BattleState : Enemy_Bomb_Monkey_State
{
    
    private Transform player;
    private float lastTimeWasInBattle;
    private float moveSpeedBoost = 0f;
    private int lastDirectionToPlayer = 0;
    
    
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
        distanceX();
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

    
       

        if (bombMonkey.JumpState._jumpData.IsJumpDetected || distanceX())
        {
            bombMonkey.JumpState.StateName = "Chase_director";
            enemyStateMachine.ChangeState(bombMonkey.JumpState);
        }
        
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        int currentDirection = DirectionToPlayer();

        // 방향이 바뀌었는지 확인
        if (currentDirection != lastDirectionToPlayer)
        {
            moveSpeedBoost = 0f; // 방향이 바뀌면 속도 증가 초기화
            lastDirectionToPlayer = currentDirection;
        }
        else
        {
            // 계속 같은 방향이면 속도 점점 증가
            moveSpeedBoost += bombMonkey.speedIncreaseRate * Time.fixedDeltaTime;
            moveSpeedBoost = Mathf.Clamp(moveSpeedBoost, 0f, bombMonkey.maxSpeedBoost); // 최대 제한
        }

        float finalSpeed = bombMonkey.battleMoveSpeed + moveSpeedBoost;
        
        bombMonkey.SetVelocity(bombMonkey.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() =>Time.time > lastTimeWasInBattle + bombMonkey.battleTimeDuration;
    private bool distanceX()
    {
        float distanceX = Mathf.Abs(player.transform.position.x - bombMonkey.transform.position.x);
        if(distanceX >= 3f)
            return true;
        return false;
    }  
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
