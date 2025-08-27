using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    
    // public Enemy_IdleState IdleState;
    // public Enemy_MoveState  MoveState;
    // public Enemy_ChaseState   ChaseState;
    // public Enemy_AttackState AttackState;
    // public Enemy_DeadState  DeadState;
    
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
        States = new Dictionary<System.Type, EnemyState>();
    }


    protected virtual void Start()
    {
        
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
        IsWallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * FacingDirection,
                                         wallCheckDistance, whatIsWall)
                                     && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * FacingDirection,
                                         wallCheckDistance, whatIsWall);
        
        
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
}