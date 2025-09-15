// Enemy.cs

using Unity.Netcode;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Network Sync")]
    private NetworkVariable<Vector2> _networkPosition = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> _networkIsFacingRight = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Network Optimization")]
    [SerializeField] private float positionUpdateThreshold = 0.1f;
    [SerializeField] private float lerpDuration = 0.1f;
    private Vector2 _lastSentPosition;
    private bool _lastSentIsFacingRight;

    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    
    // 풀에서 처음 소환되었는지 확인하는 플래그
    private bool _justSpawned = true;

    [Header("Facing Direction")]
    protected bool _isFacingRight = false;
    public int FacingDirection { get; protected set; } = -1;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            // 서버가 아닌 클라이언트에서는 물리 시뮬레이션을 비활성화합니다.
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    protected virtual void Update()
    {
        if (IsServer)
        {
            // 서버: 위치와 방향 변경 시 네트워크 변수 업데이트
            UpdateNetworkVariablesIfNeeded();
        }
        else
        {
            //Debug.Log("Enemy Snyc : Client");
            // 클라이언트: 서버로부터 받은 데이터로 위치와 방향 보간
            HandleClientInterpolation();
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
    
    private void HandleClientInterpolation()
    {
        // 방금 스폰되었다면, Lerp 없이 즉시 위치를 설정합니다.
        if (_justSpawned)
        {
            transform.position = _networkPosition.Value;
            _justSpawned = false;
            //Debug.Log("Client : justSpawned");
        }
        
        // 네트워크 위치 값이 변경되면 새로운 보간 시작
        if (_lerpTargetPos != _networkPosition.Value)
        {
            _lerpStartPos = transform.position;
            _lerpTargetPos = _networkPosition.Value;
            _lerpTime = 0;
            //Debug.Log("Clinet : lerp Start");
        }
        
        //Debug.LogFormat("_networkPosition : {0}, _networkIsFacingRight : {1}", _networkPosition.Value, _networkIsFacingRight.Value);
        //Debug.LogFormat("_lerpStartPos: {0}, position : {1}, _lerpTargetPos : {2} ", _lerpStartPos, transform.position, _lerpTargetPos);

        // 보간 진행
        if (_lerpTime < lerpDuration)
        {
            _lerpTime += Time.deltaTime;
            transform.position = Vector2.Lerp(_lerpStartPos, _lerpTargetPos, _lerpTime / lerpDuration);
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

    /// <summary>
    /// NetworkEnemyPool에서 몬스터를 가져올 때 호출되는 초기화 함수입니다.
    /// </summary>
    public virtual void InitializeForPool(Vector3 initialPosition)
    {
        // 몬스터 상태 초기화 (체력, AI 상태 등)
        gameObject.SetActive(true);
        _justSpawned = true;
        
        // 서버에서만 위치를 설정하고 즉시 네트워크 변수에 반영합니다.
        if (IsServer)
        {
            transform.position = initialPosition;
            transform.rotation = Quaternion.identity; // 회전 값 초기화
            _isFacingRight = false; // 방향 상태 초기화
            FacingDirection = -1; // 방향 정수 초기화

            _networkPosition.Value = initialPosition;
            _networkIsFacingRight.Value = _isFacingRight; // 초기화된 방향을 네트워크에 동기화
            
            _lastSentPosition = initialPosition;
            _lastSentIsFacingRight = _isFacingRight; // 마지막 전송 상태도 초기화

            // 물리 상태 초기화
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }
    
    /// <summary>
    /// Non-Owner 클라이언트를 위한 시각적 Flip
    /// </summary>
    private void FlipVisualsOnly()
    {
        _isFacingRight = _networkIsFacingRight.Value;
        FacingDirection = _isFacingRight ? 1 : -1;

        // 현재 방향에 맞게 로컬 스케일 조정 (초기 방향이 왼쪽(-1) 기준이라고 가정)
        transform.rotation = _isFacingRight ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }
    
    /// <summary>
    /// 몬스터가 죽거나 비활성화될 때 오브젝트 풀로 반환합니다.
    /// </summary>
    public void ReturnToPool()
    {
        if (!IsServer) return;

        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && NetworkEnemyPool.Instance != null)
        {
            NetworkEnemyPool.Instance.ReturnObjectToPool(networkObject);
        }
        else
        {
            // 풀을 사용하지 않는 경우 그냥 파괴
            Destroy(gameObject);
        }
    }
}