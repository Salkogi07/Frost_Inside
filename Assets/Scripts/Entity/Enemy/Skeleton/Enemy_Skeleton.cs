﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class Enemy_Skeleton : Entity
{

    public Enemy_Skeleton_IdleState IdleState;
    public Enemy_Skeleton_MoveState MoveState;
    public Enemy_Skeleton_BattleState BattleState;
    public Enemy_Skeleton_JumpState JumpState;
    public Enemy_Skeleton_AttackState AttackState;
    public Enemy_Skeleton_DeadState DeadState;


    private BoxCollider2D coll;

    protected Enemy_Skeleton_StateMachine EnemyStateMachine;


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

    [Header("Movement details")] public float IdleTime = 2;
    public float MoveSpeed = 1.4f;

    [Range(0, 10)] public float moveAnimSpeedMultiplier = 1;



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

    protected virtual void Awake()
    {
        base.Awake();

        EnemyStateMachine = new Enemy_Skeleton_StateMachine();
        coll = GetComponent<BoxCollider2D>();


        AttackState = new Enemy_Skeleton_AttackState(this, EnemyStateMachine, "attack");
        IdleState = new Enemy_Skeleton_IdleState(this, EnemyStateMachine, "idle");
        MoveState = new Enemy_Skeleton_MoveState(this, EnemyStateMachine, "move");
        BattleState = new Enemy_Skeleton_BattleState(this, EnemyStateMachine, "battle");
        // GroundedState = new Enemy_GroundedState(this, EnemyStateMachine,null);
        JumpState = new Enemy_Skeleton_JumpState(this, EnemyStateMachine, "jump", jumpData);
        //
        // DeadState = new Enemy_DeadState(EnemyStateMachine, "dead",_skeleton);


    }


    protected virtual void Start()
    {
        EnemyStateMachine.Initialize(IdleState);
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

    public bool CanPerformLeap()
    {
        float jumpForce = JumpState._jumpData.jumpForce;
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        // 최대 높이 계산 (v^2 / (2g))
        float maxJumpHeight = (jumpForce * jumpForce) / (2f * gravity);

        // 시작 위치 (적이 바라보는 방향 앞쪽)
        Vector2 startPoint = new Vector2(
            transform.position.x + (wallCheckDistance * FacingDirection),
            transform.position.y
        );

        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        // 계산된 최대 점프 높이까지 검사
        for (float yOffset = 0; yOffset <= maxJumpHeight; yOffset += 0.05f)
        {
            // 현재 높이에서 체크 지점
            Vector2 checkPoint = new Vector2(startPoint.x, startPoint.y + yOffset);

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
    





