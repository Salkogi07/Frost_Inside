using Unity.Netcode;
using UnityEngine;

// 모든 적의 부모 클래스. 네트워크 동기화와 기본적인 로직을 담당합니다.
// abstract 키워드를 사용하여 이 클래스 자체를 직접 씬에 배치할 수 없도록 합니다.
public abstract class Enemy : Entity
{
    [Header("Network Sync")]
    // [서버] -> [클라이언트] 단방향 동기화
    private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> _networkIsFacingRight = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Movement")]
    public bool IsFacingRight { get; protected set; } = true;
    public int FacingDirection { get; protected set; } = 1;

    [Header("Network Optimization")]
    [SerializeField] private float positionUpdateThreshold = 0.1f; // 이 거리 이상 움직여야 위치 전송
    [SerializeField] private float lerpDuration = 0.1f; // 클라이언트 측에서 위치 보간에 걸리는 시간

    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    
    // 풀에서 처음 생성되었을 때 순간이동(Lerp 없음)을 위한 플래그
    private bool _teleportOnNextUpdate = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 서버가 아닌 클라이언트에서는 물리 시뮬레이션을 비활성화합니다.
        // 클라이언트는 서버가 보내주는 위치 값으로만 움직여야 합니다.
        if (!IsServer && rb != null)
        {
            rb.isKinematic = true;
        }
        
        // 네트워크 변수 값이 변경될 때마다 클라이언트에서 호출될 함수를 등록합니다.
        _networkPosition.OnValueChanged += OnPositionChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (_networkPosition != null)
        {
            _networkPosition.OnValueChanged -= OnPositionChanged;
        }
    }
    
    protected virtual void Update()
    {
        // 서버는 AI 로직과 물리적 움직임을 처리하고, 그 결과를 네트워크 변수에 씁니다.
        if (IsServer)
        {
            // (자식 클래스에서 StateMachine.UpdateActiveState() 등을 호출)
            UpdateNetworkVariablesIfNeeded();
        }
        // 클라이언트는 서버로부터 받은 데이터로 부드럽게 위치와 방향을 보간합니다.
        else
        {
            // 스폰 직후 순간이동 처리
            if (_teleportOnNextUpdate)
            {
                transform.position = _networkPosition.Value;
                _teleportOnNextUpdate = false;
            }
            else
            {
                 // 위치 보간 (Lerp)
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

            // 방향 동기화 (시각적 처리만)
            if (IsFacingRight != _networkIsFacingRight.Value)
            {
                FlipVisualsOnly();
            }
        }
    }

    /// <summary>
    /// NetworkEnemyPool에서 호출되는 초기화 함수입니다.
    /// 몬스터를 활성화하고 시작 위치를 설정합니다.
    /// </summary>
    /// <param name="initialPosition">몬스터가 스폰될 월드 위치</param>
    public virtual void InitializeForPool(Vector3 initialPosition)
    {
        if (!IsServer) return;

        // 클라이언트가 다음 업데이트에서 순간이동하도록 플래그 설정
        _teleportOnNextUpdate = true;
        
        // 서버 측 위치 즉시 설정
        transform.position = initialPosition;
        
        // 네트워크 변수 업데이트 (클라이언트들에게 전송)
        _networkPosition.Value = initialPosition;
        
        // (필요 시) 체력, 상태 등 초기화 로직 추가
    }
    
    // 서버에서만 호출되어야 하는 속도 설정 함수
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (!IsServer || isknocked) return;

        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        CheckAndFlip(xVelocity);
    }
    
    // 서버에서만 호출되어야 하는 방향 전환 함수
    public void Flip()
    {
        if (!IsServer) return;

        IsFacingRight = !IsFacingRight;
        FacingDirection *= -1;
        transform.Rotate(0, 180, 0);
        
        // 방향 변경을 클라이언트에 알림
        _networkIsFacingRight.Value = IsFacingRight;
    }
    
    // 자식 클래스에서 반드시 구현해야 하는 죽음 처리 로직
    public abstract void Die();
    
    // 서버에서 위치나 방향이 크게 변경되었을 때만 네트워크 변수를 업데이트하여 최적화합니다.
    private void UpdateNetworkVariablesIfNeeded()
    {
        if (Vector2.Distance(transform.position, _networkPosition.Value) > positionUpdateThreshold)
        {
            _networkPosition.Value = transform.position;
        }

        if (IsFacingRight != _networkIsFacingRight.Value)
        {
            _networkIsFacingRight.Value = IsFacingRight;
        }
    }
    
    // 클라이언트에서 위치 값이 변경되었을 때 호출됩니다.
    private void OnPositionChanged(Vector2 previousValue, Vector2 newValue)
    {
        if (IsServer) return;

        // 새로운 보간 시작
        _lerpStartPos = transform.position;
        _lerpTargetPos = newValue;
        _lerpTime = 0;
    }

    // 클라이언트에서 방향 전환 시 시각적 요소만 변경 (물리적 변경 없음)
    private void FlipVisualsOnly()
    {
        IsFacingRight = _networkIsFacingRight.Value;
        FacingDirection = IsFacingRight ? 1 : -1;
        transform.rotation = Quaternion.Euler(0, IsFacingRight ? 0 : 180, 0);
    }

    private void CheckAndFlip(float xDirection)
    {
        if (xDirection > 0 && !IsFacingRight)
            Flip();
        else if (xDirection < 0 && IsFacingRight)
            Flip();
    }
}