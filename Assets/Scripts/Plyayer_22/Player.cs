using UnityEngine;
using System.Collections;

namespace Script.Plyayer_22
{
    public class Player : MonoBehaviour
    {
        public ParticleSystem Dust { get; private set; }
        public Animator Anim { get; private set; }
        public Rigidbody2D Rigidbody2D { get; private set; }
        
        private StateMachine _stateMachine;

        public Player_IdleState IdleState { get; private set; }
        public Player_WalkState WalkState { get; private set; }
        public Player_RunState RunState { get; private set; }
        public Player_JumpState JumpState { get; private set; }
        public Player_FallState FallState { get; private set; }

        
        [Header("Movement details")]
        public float CurrentSpeed { get; private set; }
        public float WalkSpeed; // 나중에 스탯처리
        public float RunSpeed; // 나중에 스탯처리
        public float JumpForce;

        [Range(0,1)]
        public float inAirMoveMultiplier = .7f;
        private bool _isFacingRight = false;
        
        private KeyCode _lastKey = KeyCode.None;
        public float MoveInput { get; private set; } = 0f;

        [Header("Collision detection")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.1f);
        [SerializeField] private LayerMask whatIsGround;

        [SerializeField] private float groundCheckDistance;
        public bool IsGroundDetected { get; private set; }


        private void Awake()
        {
            Anim = GetComponentInChildren<Animator>();
            Dust = GetComponentInChildren<ParticleSystem>();
            Rigidbody2D = GetComponent<Rigidbody2D>();

            _stateMachine = new StateMachine();

            IdleState = new Player_IdleState(this, _stateMachine, "idle");
            WalkState = new Player_WalkState(this, _stateMachine, "walk");
            RunState = new Player_RunState(this, _stateMachine, "run");
            JumpState = new Player_JumpState(this, _stateMachine, "jumpFall");
            FallState = new Player_FallState(this, _stateMachine, "jumpFall");
        }
        

        private void Start()
        {
            _stateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            ProcessKeyboardInput();
            
            _stateMachine.UpdateActiveState();
        }

        private void FixedUpdate()
        {
            HandleCollisionDetection();
            _stateMachine.FiexedUpdateActiveState();
        }

        private void ProcessKeyboardInput()
        {
            KeyCode leftKey = KeyManager.instance.GetKeyCodeByName("Move Left");
            KeyCode rightKey = KeyManager.instance.GetKeyCodeByName("Move Right");

            // 마지막으로 누른 키 저장
            if (Input.GetKeyDown(leftKey)) _lastKey = leftKey;
            if (Input.GetKeyDown(rightKey)) _lastKey = rightKey;

            // 현재 눌려 있는 키 확인
            bool isLeftHeld = Input.GetKey(leftKey);
            bool isRightHeld = Input.GetKey(rightKey);

            MoveInput = 0;

            if (isLeftHeld && isRightHeld)
            {
                // 둘 다 눌린 경우는 마지막 누른 키 우선
                if (_lastKey == leftKey)
                    MoveInput = -1;
                else if (_lastKey == rightKey)
                    MoveInput = 1;
            }
            else if (isLeftHeld)
            {
                MoveInput = -1;
            }
            else if (isRightHeld)
            {
                MoveInput = 1;
            }
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
            if (IsGroundDetected)
                Dust.Play();
            
            transform.Rotate(0, 180, 0);
            _isFacingRight = !_isFacingRight;
        }

        private void HandleCollisionDetection()
        {
            IsGroundDetected = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, whatIsGround);
        }

        public void SetMoveSpeed(float speed)
        {
            CurrentSpeed = speed;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

            //Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        }
    }
}
