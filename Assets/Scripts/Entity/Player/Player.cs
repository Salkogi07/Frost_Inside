using FMOD.Studio;
using UnityEngine;
using Unity.Netcode;

public class Player : Entity
{
    public ParticleSystem Dust { get; private set; }
    public Player_Stats Stats { get; private set; }
    public Player_Condition Condition { get; private set; }
    public Player_TileMining TileMining { get; private set; }
    public Laser Laser { get; private set; }
    
    public Player_IdleState IdleState { get; private set; }
    public Player_WalkState WalkState { get; private set; }
    public Player_RunState RunState { get; private set; }
    public Player_JumpState JumpState { get; private set; }
    public Player_FallState FallState { get; private set; }
    public Player_MiningState MiningState { get; private set; }

    private Player_StateMachine _playerStateMachine;
    [SerializeField] private GameObject playerObject;

    [Header("Movement details")] 
    public float CurrentSpeed { get; private set; }
    public float JumpForce;

    [Range(0, 1)] public float inAirMoveMultiplier = .7f;
    private bool _isFacingRight = false;
    public int FacingDirection { get; private set; } = -1;

    private KeyCode _lastKey = KeyCode.None;
    public float MoveInput { get; private set; } = 0f;

    [Header("Collision detection")] [SerializeField]
    private Transform groundCheck;

    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    [SerializeField] private LayerMask whatIsGround;
    public bool IsGroundDetected { get; private set; }
    
    [Header("Mining")]
    public bool CanMine = true;
    
    private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> _networkIsFacingRight = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    private float _lerpDuration = 0.05f;
    
    [Header("Network Optimization")]
    [SerializeField] private float positionUpdateThreshold = 0.05f; // 이 거리 이상 움직여야 위치 전송
    private Vector2 _lastSentPosition;
    private bool _lastSentIsFacingRight;

    [SerializeField] private bool IsTest = false;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Owner가 아닌 클라이언트에서는 물리 시뮬레이션을 비활성화하여
        // 네트워크로부터 받은 위치로만 움직이게 함
        if (!IsOwner && rb != null)
        {
            rb.isKinematic = true;
        }
        else if (IsOwner)
        {
            _lastSentPosition = transform.position;
            _lastSentIsFacingRight = _isFacingRight;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        Stats = GetComponent<Player_Stats>();
        Condition = GetComponent<Player_Condition>();
        TileMining = GetComponent<Player_TileMining>();
        Laser = GetComponentInChildren<Laser>();
        
        Dust = GetComponentInChildren<ParticleSystem>();

        _playerStateMachine = new Player_StateMachine();

        IdleState = new Player_IdleState(this, _playerStateMachine, "idle");
        WalkState = new Player_WalkState(this, _playerStateMachine, "walk");
        RunState = new Player_RunState(this, _playerStateMachine, "run");
        JumpState = new Player_JumpState(this, _playerStateMachine, "jumpFall");
        FallState = new Player_FallState(this, _playerStateMachine, "jumpFall");
        MiningState = new Player_MiningState(this, _playerStateMachine, "mining");
    }

    private void Start()
    {
        _playerStateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (ChatManager.instance.IsChatting || InventoryManager.Instance.isInvenOpen)
        {
            _playerStateMachine.ChangeState(IdleState);
            return;
        }
        
        if (IsTest)
        {
            ProcessKeyboardInput();
            _playerStateMachine.UpdateActiveState();
            return;
        }
        
        if (IsOwner)
        {
            ProcessKeyboardInput();
            _playerStateMachine.UpdateActiveState();

            UpdateNetworkVariablesIfNeeded();
        }
        else
        {
            // 네트워크 위치 값이 변경되면 새로운 보간 시작
            if (_lerpTargetPos != _networkPosition.Value)
            {
                _lerpStartPos = transform.position;
                _lerpTargetPos = _networkPosition.Value;
                _lerpTime = 0;
            }

            // 보간 진행
            if (_lerpTime < _lerpDuration)
            {
                _lerpTime += Time.deltaTime;
                transform.position = Vector2.Lerp(_lerpStartPos, _lerpTargetPos, _lerpTime / _lerpDuration);
            }
            else
            {
                transform.position = _lerpTargetPos; // 보간이 끝나면 목표 위치로 정확히 이동
            }

            // 바라보는 방향 동기화
            if (_isFacingRight != _networkIsFacingRight.Value)
            {
                FlipVisualsOnly();
            }
        }
    }

    private void UpdateNetworkVariablesIfNeeded()
    {
        // 위치 동기화: 마지막으로 보낸 위치와 현재 위치의 거리가 threshold보다 클 때만 전송
        if (Vector2.Distance(_lastSentPosition, transform.position) > positionUpdateThreshold)
        {
            _networkPosition.Value = transform.position;
            _lastSentPosition = transform.position;
        }

        // 방향 동기화: 마지막으로 보낸 방향과 현재 방향이 다를 때만 전송
        if (_lastSentIsFacingRight != _isFacingRight)
        {
            _networkIsFacingRight.Value = _isFacingRight;
            _lastSentIsFacingRight = _isFacingRight;
        }
    }
    
    private void FixedUpdate()
    {
        HandleCollisionDetection();

        _playerStateMachine.FiexedUpdateActiveState();
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
        if (isknocked)
            return;
        
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
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
            PlayDustEffectServerRpc();

        Vector3 currentScale = playerObject.transform.localScale;
        currentScale.x *= -1;
        playerObject.transform.localScale = currentScale;
        _isFacingRight = !_isFacingRight;
        FacingDirection *= -1;
    }
    
    // 클라이언트가 서버에게 파티클 재생을 요청하는 RPC입니다.
    [ServerRpc]
    private void PlayDustEffectServerRpc()
    {
        // 서버가 모든 클라이언트에게 파티클 재생을 명령합니다.
        PlayDustEffectClientRpc();
    }

    // 서버의 명령을 받아 모든 클라이언트에서 실행되는 RPC입니다.
    [ClientRpc]
    private void PlayDustEffectClientRpc()
    {
        // 모든 클라이언트에서 Dust 파티클을 재생합니다.
        if (Dust != null)
        {
            Dust.Play();
        }
    }
    
    // Non-Owner 클라이언트를 위한 시각적 Flip (파티클 재생 없음)
    private void FlipVisualsOnly()
    {
        _isFacingRight = _networkIsFacingRight.Value;
        FacingDirection = _isFacingRight ? 1 : -1;

        Vector3 currentScale = playerObject.transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * FacingDirection * -1; // 초기 방향에 맞게 조정
        playerObject.transform.localScale = currentScale;
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
    }
}