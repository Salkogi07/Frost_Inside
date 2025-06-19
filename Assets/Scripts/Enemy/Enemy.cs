using Script.Plyayer_22;
using UnityEngine;

namespace Scripts.Enemy
{
    public class Enemy : MonoBehaviour
    {
        public Animator Anim { get; private set; }
        public Rigidbody2D Rigidbody { get; private set; }
        
        public Enemy_StateMachine EnemyStateMachine;
        
        public Enemy_IdleState IdleState { get; private set; }
        public Enemy_MoveState MoveState { get; private set; }
         public Enemy_AttackState AttackState { get; private set; }

        
        [Header("Movement details")]
        public float IdleTime = 2;
        public float MoveSpeed = 1.4f;
        
        [Range(0, 10)]
        public float moveAnimSpeedMultiplier = 1;
        
        private bool _isFacingRight = false;
        public int FacingDirection { get; private set; } = -1;

        [Header("Collision detection")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private float groundCheckDistance;
        
        public bool IsGroundDetected { get; private set; }
        public bool IsWallDetected { get; private set; }
        [SerializeField] private Transform primaryWallCheck;
        [SerializeField] private Transform secondaryWallCheck;
        [SerializeField] private LayerMask whatIsWall;
        [SerializeField] private float wallCheckDistance;


        protected virtual void Awake()
        {
            Anim = GetComponentInChildren<Animator>();
            Rigidbody = GetComponent<Rigidbody2D>();

            EnemyStateMachine = new Enemy_StateMachine();

            IdleState = new Enemy_IdleState(this, EnemyStateMachine, "idle");
            MoveState = new Enemy_MoveState(this, EnemyStateMachine, "move");
        }
        

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            EnemyStateMachine.UpdateActiveState();
        }

        protected virtual void FixedUpdate()
        {
            HandleCollisionDetection();
            EnemyStateMachine.FiexedUpdateActiveState();
        }

        public void CallAnimationTrigger()
        {
            // EnemyStateMachine.currentState.CallAnimationTrigger();
        }
        
        
        
        public void SetVelocity(float xVelocity, float yVelocity)
        {
            Rigidbody.linearVelocity = new Vector2(xVelocity, yVelocity);
            HandleFlip(xVelocity);
        }

        
        private void HandleFlip(float xVelcoity)
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

        private void HandleCollisionDetection()
        {
            IsGroundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
            IsWallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsWall) 
                             && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsWall);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));
            Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * FacingDirection, 0));
            Gizmos.DrawLine(secondaryWallCheck.position, secondaryWallCheck.position + new Vector3(wallCheckDistance * FacingDirection, 0));
        }
    }
}