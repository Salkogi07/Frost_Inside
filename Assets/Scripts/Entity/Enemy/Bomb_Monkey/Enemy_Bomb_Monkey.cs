﻿using System.Collections.Generic;
using Stats;
using UnityEngine;

public class Enemy_Bomb_Monkey : Entity
{
    public Enemy_Bomb_Monkey_IdleState IdleState;
    public Enemy_Bomb_Monkey_MoveState MoveState;
    public Enemy_Bomb_Monkey_BattleState BattleState;
    public Enemy_Bomb_Monkey_JumpState JumpState;
    public Enemy_Bomb_Monkey_AttackState AttackState;
    public Enemy_Bomb_Monkey_DeadState DeadState;


    private BoxCollider2D coll;
    public Enemy_Stats stats;

    protected Enemy_Bomb_Monkey_StateMachine EnemyStateMachine;


    private bool _isFacingRight = false;
    public int FacingDirection { get; private set; } = -1;

    [Header("Collision detection [Ground]")] [SerializeField]
    public LayerMask whatIsGround;

    [SerializeField] public Transform groundCheck;
    [SerializeField] private float groundCheckDistance;

    [Header("Collision detection [Wall]")] [SerializeField]
    public LayerMask whatIsWall;

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

    [Header("Boob detection")] [SerializeField]
    private LayerMask whatIsBob;

    [SerializeField] public GameObject prf;
    [SerializeField] private Transform boobTargetChack;
    [SerializeField] private float targetcheckRadius;
    [Header("Movement details")] public float IdleTime;
    public float MoveSpeed = 1.4f;

    [Range(0, 10)] public float moveAnimSpeedMultiplier = 1;
    [Header("bouns speed Movement")] public float speedIncreaseRate = 0.65f; // 초당 속도 증가량
    public float maxSpeedBoost = 3f;

    public Transform player { get; private set; }

    public void TryEnterBattleState(Transform player)
    {
        if (AttackState == null)
        {
            return;
        }

        if (EnemyStateMachine.currentState == BattleState)
        {
            return;
        }

        if (EnemyStateMachine.currentState == AttackState)
        {
            return;
        }

        this.player = player;
        EnemyStateMachine.ChangeState(BattleState);
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

    public void PerformAttack()
    {
        GetDetectedColliders();
        // entityHealth?.TakeDamage(damage,transform);
        if (BattleState.Explosive)
        {
            foreach (var target in GetDetectedColliders())
            {
                EnemyStateMachine.ChangeState(AttackState);
            }
        }
    }

    private Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(boobTargetChack.position, targetcheckRadius, whatIsBob);
    }

    protected virtual void Awake()
    {
        base.Awake();

        EnemyStateMachine = new Enemy_Bomb_Monkey_StateMachine();
        coll = GetComponent<BoxCollider2D>();
        stats = GetComponent<Enemy_Stats>();

        AttackState = new Enemy_Bomb_Monkey_AttackState(this, EnemyStateMachine, "attack");
        IdleState = new Enemy_Bomb_Monkey_IdleState(this, EnemyStateMachine, "idle");
        MoveState = new Enemy_Bomb_Monkey_MoveState(this, EnemyStateMachine, "move");
        BattleState = new Enemy_Bomb_Monkey_BattleState(this, EnemyStateMachine, "chase");
        // GroundedState = new Enemy_GroundedState(this, EnemyStateMachine,null);
        JumpState = new Enemy_Bomb_Monkey_JumpState(this, EnemyStateMachine, "jump", jumpData);
        DeadState = new Enemy_Bomb_Monkey_DeadState(this, EnemyStateMachine, "dead");
    }
    
    protected virtual void Start()
    {
        EnemyStateMachine.Initialize(IdleState);
    }

    public void Deading()
    {
        Debug.Log("d");
        EnemyStateMachine.ChangeState(DeadState);
    }

    protected virtual void Update()
    {
        HandleCollisionDetection();
        PerformAttack();
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
        JumpState._jumpData.IsJumpDetected = Physics2D.Raycast(JumpState._jumpData.primaryJumpCheck.position,
            Vector2.right * FacingDirection, JumpState._jumpData.jumpCheckDistance, JumpState._jumpData.whatIsJump);
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


        if (JumpState != null && JumpState._jumpData != null && JumpState._jumpData.primaryJumpCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(JumpState._jumpData.primaryJumpCheck.position,
                new Vector3(JumpState._jumpData.primaryJumpCheck.position.x + (FacingDirection * JumpState._jumpData.jumpCheckDistance), JumpState._jumpData.primaryJumpCheck.position.y));
        }

        if (boobTargetChack != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(boobTargetChack.position, targetcheckRadius);
        }
    }
    
    public void CallAnimationTrigger()
    {
        EnemyStateMachine.currentState.CallAnimationTrigger();
    }

    public bool CanPerformLeap()
    {
        float jumpForce = JumpState._jumpData.jumpForce;
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        // 최대 높이 계산 (v^2 / (2g))
        float maxJumpHeight = (jumpForce * jumpForce) / (2f * gravity);

        // 시작 위치 (적이 바라보는 방향 앞쪽)
        Vector2 startPoint = new Vector2(
            transform.position.x + (JumpState._jumpData.jumpCheckDistance * FacingDirection),
            transform.position.y
        );

        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        // 계산된 최대 점프 높이까지 검사
        for (float yOffset = 0; yOffset <= maxJumpHeight; yOffset += 0.05f)
        {
            // 현재 높이에서 체크 지점
            Vector2 checkPoint = new Vector2(startPoint.x, startPoint.y + yOffset);
            // Debug.Log(checkPoint.y);
            // 현재 높이 기준 앞으로 벽/장애물 있는지 확인
            Collider2D hit = Physics2D.OverlapBox(checkPoint, boxSize, 0f, whatIsWall);

            if (hit == null)
            {
                // 박스 공간이 비어 있음 → 점프 가능
                return true;
            }
        }
        
        return false;
    }
}