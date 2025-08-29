using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class Enemy_Skeleton : Entity
{

    public Enemy_Skeleton_IdleState IdleState { get; private set; }
    public Enemy_Skeleton_MoveState MoveState { get; private set; }
    public Enemy_Skeleton_BattleState BattleState { get; private set; }
    public Enemy_JumpState JumpState { get; private set; }
    public Enemy_AttackState AttackState { get; private set; }
    public Enemy_DeadState DeadState { get; private set; }

    public Enemy Enemy;
    Enemy_Skeleton _skeleton;

    private BoxCollider2D coll;
    
    protected Enemy_StateMachine EnemyStateMachine;
    protected Dictionary<System.Type, EnemyState> States;
    
    private bool _isFacingRight = false;
    public int FacingDirection { get; private set; } = -1;

    [Header("Collision detection [Ground]")] 
    [SerializeField] public LayerMask whatIsGround;
    [SerializeField] public Transform groundCheck;
    [SerializeField] private float groundCheckDistance;
    
    [Header("Collision detection [Wall]")] 
    [SerializeField] public LayerMask whatIsWall;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform secondaryWallCheck;
    [SerializeField] public float wallCheckDistance;

    [Header("Jump Config")] public EnemyJumpData jumpData;

    public bool IsGroundDetected { get; private set; }
    public bool IsWallDetected { get; private set; }

    [Header("Battle details")] public float battleMoveSpeed = 3;
    public float attackDistance = 2;
    public float battleTimeDuration = 5;
    public float minRetreatDistance = 1;

    public Vector2 retreatVelocity;

    [Header("Player detection")] [SerializeField]
    private Transform playerCheck;

    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private float playerCheckDistance = 10;

    [Header("Movement details")] public float IdleTime = 2;
    public float MoveSpeed = 1.4f;

    [Range(0, 10)] public float moveAnimSpeedMultiplier = 1;

    
    
    public Transform player { get; private set; }

    public Idle_director IdleDirector;
    public Move_director MoveDirector;
    public Chase_director ChaseDirector;
    // public Grounded_Idirector  GroundedDirector;
    public Life_director LifeDirector;
    

    public T GetState<T>() where T : EnemyState
    {
        if (States.TryGetValue(typeof(T), out var state))
            return state as T;

        return null; // 없는 상태라면 null
    }
    
    public void TryEnterBattleState(Transform player)
    {
        if (GetState<Enemy_AttackState>() == null)
        {
             return;
        }
        if (EnemyStateMachine.currentState == ChaseDirector)
        {
            return;
        }
        if(EnemyStateMachine.currentState == GetState<Enemy_AttackState>())
        {
            return;
        }
        this.player =  player;
        EnemyStateMachine.ChangeState(ChaseDirector);
    }

    public Transform GetPlayerReference()
    {
        if (player == null)
        {
            player = PlayerDetection().transform;
        }

        return player;
    }

    public RaycastHit2D PlayerDetection()
    {
        RaycastHit2D hit = Physics2D.Raycast(playerCheck.position, Vector2.right * FacingDirection, playerCheckDistance,
            whatIsPlayer | whatIsGround);

        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return default;
        }

        return hit;
    }
    
    protected virtual void Awake()
    {
        base.Awake();
        
        EnemyStateMachine = new Enemy_StateMachine();
        coll = GetComponent<BoxCollider2D>();
        States = new Dictionary<System.Type, EnemyState>();

        // AttackState = new Enemy_AttackState(EnemyStateMachine, "attack",_skeleton);
        IdleState = new Enemy_Skeleton_IdleState(EnemyStateMachine, "idle",_skeleton);
        MoveState = new Enemy_Skeleton_MoveState(EnemyStateMachine, "move",_skeleton);
        BattleState = new Enemy_Skeleton_BattleState(EnemyStateMachine, "battle",_skeleton);
        // GroundedState = new Enemy_GroundedState(this, EnemyStateMachine,null);
        // JumpState = new Enemy_JumpState(EnemyStateMachine, "jump",_skeleton, jumpData);
        //
        // DeadState = new Enemy_DeadState(EnemyStateMachine, "dead",_skeleton);

       
    }


    protected virtual void Start()
    {
        EnemyStateMachine.Initialize(IdleDirector);
    }

    protected virtual void Update()
    {
        HandleCollisionDetection();
        EnemyStateMachine.UpdateActiveState();
    }

    protected virtual void FixedUpdate()
    {
        EnemyStateMachine.FiexedUpdateActiveState();
    }
    
    
    public void HandleFlip(float xVelcoity)
    {
        if (xVelcoity > 0 && !_isFacingRight)
            Flip();
        else if (xVelcoity < 0 && _isFacingRight)
            Flip();
    }
    
    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        _isFacingRight = !_isFacingRight;
        FacingDirection *= -1;
    }
    
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isknocked)
            return;

        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }
    
    private void HandleCollisionDetection()
    {
        IsGroundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        IsWallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * FacingDirection,wallCheckDistance, whatIsWall)
                                     && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsWall);
    }
    
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position,
            primaryWallCheck.position + new Vector3(wallCheckDistance * FacingDirection, 0));
        Gizmos.DrawLine(secondaryWallCheck.position,
            secondaryWallCheck.position + new Vector3(wallCheckDistance * FacingDirection, 0));
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCheck.position,
            new Vector3(playerCheck.position.x + (FacingDirection * playerCheckDistance), playerCheck.position.y));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerCheck.position,
            new Vector3(playerCheck.position.x + (FacingDirection * attackDistance), playerCheck.position.y));

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(playerCheck.position,
            new Vector3(playerCheck.position.x + (FacingDirection * minRetreatDistance), playerCheck.position.y));
    }
    
    public void CallAnimationTrigger()
    {
        EnemyStateMachine.currentState.CallAnimationTrigger();
    }

    public bool CanPerformLeap()
    {
        var jumpState = GetState<Enemy_JumpState>();
        if (jumpState == null)
            return false;

        // BoxCast 파라미터 설정
        Vector2 boxSize = coll.size;
        float jumpHeight = jumpState._jumpData.jumpForce * 0.5f; // 대략적인 점프 높이 예측
        float leapDistance = jumpState._jumpData.jumpVelocity * 0.5f; // 대략적인 도약 거리 예측
        Vector2 castOrigin = (Vector2)transform.position + new Vector2(0, boxSize.y / 2);

        // 1. 머리 위 공간 확인 (수직 BoxCast)
        RaycastHit2D ceilingHit = Physics2D.BoxCast(castOrigin, boxSize, 0f, Vector2.up, jumpHeight, whatIsWall);
        if (ceilingHit.collider != null)
        {
            // Debug.Log("천장이 막혀 점프 불가!");
            return false;
        }

        // 2. 전방 착지 공간 확인 (대각선 BoxCast)
        Vector2 leapDirection = new Vector2(FacingDirection, 1).normalized;
        RaycastHit2D forwardHit = Physics2D.BoxCast(castOrigin, boxSize, 0f, leapDirection, leapDistance, whatIsWall);
        if (forwardHit.collider != null)
        {
            // Debug.Log("점프 경로에 장애물이 있어 점프 불가!");
            return false;
        }

        // 모든 테스트 통과: 점프 가능
        return true;
    }
}