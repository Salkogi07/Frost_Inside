using Unity.Netcode;
using UnityEngine;

public abstract class Enemy : Entity
{
    [Header("Network Sync")]
    [SerializeField] private float positionUpdateThreshold = 0.05f;
    [SerializeField] private float lerpDuration = 0.05f;
    [Tooltip("몬스터가 처음 스폰될 때 바라볼 방향 (1: 오른쪽, -1: 왼쪽)")]
    [SerializeField] private int initialFacingDirection = -1; // 원본 코드에 맞춰 왼쪽을 기본값으로 설정

    // --- 네트워크 변수들 ---
    private readonly NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>();
    private readonly NetworkVariable<int> _networkFacingDirection = new NetworkVariable<int>();
    // 애니메이션 동기화를 위해 X축 속도를 네트워크로 전송합니다.
    private readonly NetworkVariable<float> _networkVelocityX = new NetworkVariable<float>();
    
    // --- 클라이언트 측 Lerp 관련 변수들 ---
    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    private Vector2 _lastSentPosition;

    // --- 서버 측 상태 변수 ---
    private bool _isFacingRight = true;
    public int FacingDirection { get; protected set; } = 1;

    // 클라이언트가 애니메이터에 사용할 수 있도록 네트워크로 동기화된 X축 속도를 노출합니다.
    public float NetworkVelocityX => _networkVelocityX.Value;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _lastSentPosition = transform.position;
            // 인스펙터에서 설정한 초기 방향을 적용합니다.
            FacingDirection = initialFacingDirection;
            _isFacingRight = initialFacingDirection == 1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * FacingDirection, transform.localScale.y, transform.localScale.z);
            _networkFacingDirection.Value = FacingDirection;
        }
        else
        {
            if (rb != null) rb.isKinematic = true;
            _networkPosition.OnValueChanged += OnPositionChanged;
            _networkFacingDirection.OnValueChanged += OnFacingDirectionChanged;
            // 초기 방향을 즉시 적용합니다.
            HandleVisualFacingDirection(_networkFacingDirection.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            _networkPosition.OnValueChanged -= OnPositionChanged;
            _networkFacingDirection.OnValueChanged -= OnFacingDirectionChanged;
        }
    }

    protected virtual void Update()
    {
        if (IsServer) return;

        if (_lerpTime < lerpDuration)
        {
            _lerpTime += Time.deltaTime;
            transform.position = Vector2.Lerp(_lerpStartPos, _lerpTargetPos, _lerpTime / lerpDuration);
        }
        else
        {
            transform.position = _lerpTargetPos;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!IsServer || !gameObject.activeInHierarchy) return;

        if (Vector2.Distance(transform.position, _lastSentPosition) > positionUpdateThreshold)
        {
            _networkPosition.Value = transform.position;
            _lastSentPosition = transform.position;
        }
        // 서버의 Rigidbody 속도를 네트워크 변수에 계속 업데이트하여 클라이언트에 전송합니다.
        _networkVelocityX.Value = rb.linearVelocity.x;
    }

    protected abstract void ResetEnemyState();

    public void InitializeForPool(Vector3 startPosition)
    {
        if (!IsServer) return;

        transform.position = startPosition;
        _lastSentPosition = startPosition;
        _networkPosition.Value = startPosition;

        var health = GetComponent<Enemy_Health>();
        if (health != null) health.ResetHealth();
        
        // 초기 방향을 다시 설정합니다.
        FacingDirection = initialFacingDirection;
        _isFacingRight = initialFacingDirection == 1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * FacingDirection, transform.localScale.y, transform.localScale.z);
        _networkFacingDirection.Value = FacingDirection;

        ResetEnemyState();
        SyncInitialPositionClientRpc(startPosition);
    }

    [ClientRpc]
    private void SyncInitialPositionClientRpc(Vector3 startPosition)
    {
        if (IsServer) return;
        transform.position = startPosition;
        _lerpStartPos = startPosition;
        _lerpTargetPos = startPosition;
        _lerpTime = lerpDuration;
    }

    private void OnPositionChanged(Vector2 previousValue, Vector2 newValue)
    {
        _lerpStartPos = transform.position;
        _lerpTargetPos = newValue;
        _lerpTime = 0;
    }

    private void OnFacingDirectionChanged(int previousValue, int newValue)
    {
        HandleVisualFacingDirection(newValue);
    }

    /// <summary>
    /// (서버 전용) 몬스터의 속도를 설정하고, 움직이는 방향으로 몸을 돌립니다.
    /// </summary>
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (!IsServer || isknocked) return;
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }
    
    /// <summary>
    /// (서버 전용) 후퇴와 같이, 특정 방향을 바라보면서 다른 방향으로 움직여야 할 때 사용합니다.
    /// </summary>
    public void SetVelocityAndFaceDirection(Vector2 velocity, int directionToFace)
    {
        if (!IsServer || isknocked) return;
        rb.linearVelocity = velocity;
        // HandleFlip 로직을 사용하되, 실제 속도가 아닌 '바라볼 방향'을 기준으로 합니다.
        if (directionToFace > 0 && !_isFacingRight) Flip();
        else if (directionToFace < 0 && _isFacingRight) Flip();
    }

    private void HandleFlip(float xVelocity)
    {
        if (xVelocity > 0 && !_isFacingRight) Flip();
        else if (xVelocity < 0 && _isFacingRight) Flip();
    }

    /// <summary>
    /// (서버 전용) 방향을 전환하고 네트워크에 동기화합니다.
    /// </summary>
    public void Flip()
    {
        if (!IsServer) return;
        _isFacingRight = !_isFacingRight;
        FacingDirection *= -1;
        _networkFacingDirection.Value = FacingDirection;
        // 서버에서도 시각적 방향을 업데이트합니다.
        HandleVisualFacingDirection(FacingDirection);
    }

    /// <summary>
    /// (서버와 클라이언트 모두 사용) 전달받은 방향 값으로 실제 오브젝트의 스케일을 조절해 뒤집습니다.
    /// </summary>
    private void HandleVisualFacingDirection(int newDirection)
    {
        // transform.Rotate 대신 localScale을 직접 조작하여 더 안정적으로 방향을 전환합니다.
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * newDirection, transform.localScale.y, transform.localScale.z);
    }
}