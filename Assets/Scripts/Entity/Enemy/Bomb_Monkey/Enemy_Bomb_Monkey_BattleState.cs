using UnityEngine;

public class Enemy_Bomb_Monkey_BattleState : Enemy_Bomb_Monkey_State
{
    private Transform player;
    private float lastTimeWasInBattle;
    public float moveSpeedBoost = 0f;
    private int lastDirectionToPlayer = 0;

    public bool Explosive = false;

    public Enemy_Bomb_Monkey_BattleState(Enemy_Bomb_Monkey bombMonkey, Enemy_Bomb_Monkey_StateMachine enemyStateMachine,
        string animBoolName) : base(bombMonkey, enemyStateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 이 상태의 모든 로직은 서버에서만 실행되어야 합니다.
        if (!bombMonkey.IsServer) return;

        player = bombMonkey.GetPlayerReference();
        
        // 플레이어 참조를 얻지 못했다면 즉시 Idle 상태로 돌아갑니다.
        if (player == null)
        {
            enemyStateMachine.ChangeState(bombMonkey.IdleState);
            return;
        }
        
        // 후퇴 로직을 SetVelocity를 통해 처리하여 방향 전환도 함께 처리합니다.
        if (shouldRetreat())
        {
            bombMonkey.SetVelocity(bombMonkey.retreatVelocity.x * -DirectionToPlayer(), bombMonkey.retreatVelocity.y);
        }
    }

    public override void Update()
    {
        base.Update();

        // AI 로직은 서버에서만 실행합니다.
        if (!bombMonkey.IsServer) return;

        // 플레이어가 비활성화되었거나 사라졌는지 확인합니다.
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            enemyStateMachine.ChangeState(bombMonkey.IdleState);
            return;
        }

        // PlayerDetection()은 RaycastHit2D를 반환하므로, collider가 있는지 확인해야 합니다.
        if (bombMonkey.PlayerDetection().collider != null)
        {
            UpdateBattleTimer();
            Explosive = true;
        }
        else
        {
             Explosive = false;
             
             if (BattleTimeIsOver())
             {
                 enemyStateMachine.ChangeState(bombMonkey.IdleState);
             }
        }
    }

    public override void FiexedUpdate()
    {
        base.FiexedUpdate();
        
        // 물리 및 이동 로직은 서버에서만 실행합니다.
        if (!bombMonkey.IsServer) return;
        
        // 플레이어 참조가 유효한지 다시 확인합니다.
        if (player == null) return;
        
        acceleration();
        
        if (bombMonkey.JumpState._jumpData.IsJumpDetected || distanceX())
        {
            bombMonkey.JumpState.StateName = "Chase_director";
            enemyStateMachine.ChangeState(bombMonkey.JumpState);
        }
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;
    
    public bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + bombMonkey.battleTimeDuration;

    private bool distanceX()
    {
        // player가 null일 수 있는 경우를 대비합니다.
        if (player == null) return false;
        
        float distanceX = Mathf.Abs(player.transform.position.x - bombMonkey.transform.position.x);
        return distanceX >= 3f;
    }

    private bool shouldRetreat()
    {
        // player가 null일 수 있는 경우를 대비합니다.
        if (player == null) return false;
        
        return DistanceToPlayer() < bombMonkey.minRetreatDistance;
    }

    public void acceleration()
    {
        // player가 null이면 가속 로직을 실행하지 않습니다.
        if (player == null)
        {
            bombMonkey.SetVelocity(0, rb.linearVelocity.y); // 멈추도록 처리
            return;
        }

        int currentDirection = DirectionToPlayer();

        // 방향이 바뀌었는지 확인
        if (currentDirection != lastDirectionToPlayer)
        {
            moveSpeedBoost = 0f; // 방향이 바뀌면 속도 증가 초기화
            lastDirectionToPlayer = currentDirection;
        }
        else
        {
            if (moveSpeedBoost < bombMonkey.maxSpeedBoost)
            {
                moveSpeedBoost += bombMonkey.speedIncreaseRate * Time.fixedDeltaTime;
                moveSpeedBoost = Mathf.Min(moveSpeedBoost, bombMonkey.maxSpeedBoost);
            }
        }

        float finalSpeed = bombMonkey.battleMoveSpeed + moveSpeedBoost;

        // 부모 클래스의 SetVelocity를 호출하여 속도와 방향을 한 번에 제어합니다.
        bombMonkey.SetVelocity(finalSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }

    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }

        return Vector2.Distance(player.position, bombMonkey.transform.position);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0; // 플레이어가 없으면 방향도 없습니다.
        }

        return player.position.x > bombMonkey.transform.position.x ? 1 : -1;
    }
}
