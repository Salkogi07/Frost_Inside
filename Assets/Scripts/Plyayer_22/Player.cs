using UnityEngine;

namespace Script.Plyayer_22
{
    public class Player : MonoBehaviour
    {
        public Animator Anim { get; private set; }
        public Rigidbody2D Rigidbody2D { get; private set; }
        
        public PlayerInputSet Input { get; private set; }
        private StateMachine _stateMachine;

        public Player_IdleState IdleState { get; private set; }
        public Player_MoveState MoveState { get; private set; }
        public Player_JumpState JumpState { get; private set; }
        public Player_FallState FallState { get; private set; }


        [Header("Movement details")]
        public float moveSpeed;
        public float jumpForce;

        [Range(0,1)]
        public float inAirMoveMultiplier = .7f;
        private bool _isFacingRight = false;
        public Vector2 MoveInput { get; private set; }

        [Header("Collision detection")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.1f);
        [SerializeField] private LayerMask whatIsGround;

        [SerializeField] private float groundCheckDistance;
        public bool IsGroundDetected { get; private set; }


        private void Awake()
        {
            Anim = GetComponentInChildren<Animator>();
            Rigidbody2D = GetComponent<Rigidbody2D>();

            _stateMachine = new StateMachine();
            Input = new PlayerInputSet();

            IdleState = new Player_IdleState(this, _stateMachine, "idle");
            MoveState = new Player_MoveState(this, _stateMachine, "move");
            JumpState = new Player_JumpState(this, _stateMachine, "jumpFall");
            FallState = new Player_FallState(this, _stateMachine, "jumpFall");
        }

        private void OnEnable()
        {
            Input.Enable();

            Input.Player.Movement.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            Input.Player.Movement.canceled += ctx => MoveInput = Vector2.zero;
        }

        private void OnDisable()
        {
            Input.Disable();
        }

        private void Start()
        {
            _stateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            _stateMachine.UpdateActiveState();
        }

        private void FixedUpdate()
        {
            HandleCollisionDetection();
            _stateMachine.FiexedUpdateActiveState();
        }

        public void SetVelocity(float xVelocity, float yVelocity)
        {
            Rigidbody2D.linearVelocity = new Vector2(xVelocity, yVelocity);
            HandleFlip(xVelocity);
        }

        private void HandleFlip(float xVelcoity)
        {
            if (xVelcoity > 0 && !_isFacingRight)
                Flip();
            else if (xVelcoity < 0 && _isFacingRight)
                Flip();
        }

        private void Flip()
        {
            transform.Rotate(0, 180, 0);
            _isFacingRight = !_isFacingRight;
        }

        private void HandleCollisionDetection()
        {
            IsGroundDetected = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, whatIsGround);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

            //Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        }
    }
}
