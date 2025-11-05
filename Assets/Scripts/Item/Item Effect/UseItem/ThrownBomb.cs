using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody2D))]
public class ThrownBomb : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField] private float explosionDelay = 3f; // 폭발까지의 시간
    [SerializeField] private GameObject explosionPrefab; // 폭발 시 생성될 이펙트 프리팹 (TestDamage.cs 포함)

    [Header("네트워크 동기화")]
    [SerializeField] private float lerpDuration = 0.1f;
    [SerializeField] private float positionUpdateThreshold = 0.1f;

    private Rigidbody2D rb;

    // 네트워크를 통해 위치를 동기화하기 위한 변수
    private readonly NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 클라이언트에서의 부드러운 움직임을 위한 변수
    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    private Vector2 _lastSentPosition;
    
    private int bombDamage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        // 클라이언트에서는 물리 시뮬레이션을 직접 하지 않고 서버의 위치를 따라갑니다.
        if (IsClient)
        {
            rb.isKinematic = true;
            _networkPosition.OnValueChanged += OnPositionChanged;
        }

        // 스폰 시 초기 위치를 강제로 설정하여 부드러운 Lerp 시작을 준비합니다.
        transform.position = _networkPosition.Value;
        _lerpStartPos = transform.position;
        _lerpTargetPos = transform.position;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _networkPosition.OnValueChanged -= OnPositionChanged;
        }
    }

    private void FixedUpdate()
    {
        // 서버에서만 물리적 위치를 계산하고, 위치가 일정 이상 변경되면 클라이언트에 전송합니다.
        if (!IsServer || !gameObject.activeInHierarchy) return;

        if (Vector2.Distance(transform.position, _lastSentPosition) > positionUpdateThreshold)
        {
            _networkPosition.Value = transform.position;
            _lastSentPosition = transform.position;
        }
    }

    private void Update()
    {
        // 클라이언트에서는 서버로부터 받은 위치까지 부드럽게 이동(Lerp)합니다.
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

    private void OnPositionChanged(Vector2 previousValue, Vector2 newValue)
    {
        _lerpStartPos = transform.position;
        _lerpTargetPos = newValue;
        _lerpTime = 0;
    }

    /// <summary>
    /// (서버 전용) 폭탄을 던지고 폭발 타이머를 시작합니다.
    /// </summary>
    public void Launch(Vector2 direction, float force)
    {
        if (!IsServer) return;

        _lastSentPosition = transform.position;
        _networkPosition.Value = transform.position;

        rb.isKinematic = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        StartCoroutine(ExplosionTimer());
    }

    private IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    private void Explode()
    {
        if (!IsServer) return;

        if (explosionPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosionInstance.GetComponent<NetworkObject>().Spawn();
            explosionInstance.GetComponent<Explosion_Effect>().SetDamageClientRpc(bombDamage);
        }

        // 폭탄 오브젝트를 '사용 아이템 풀'에 반환합니다.
        ResetForPool();
        NetworkUseItemPool.Instance.ReturnObjectToPool(NetworkObject);
    }

    private void ResetForPool()
    {
        if (!IsServer) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    
    /// <summary>
    /// (서버 전용) 이 폭탄이 가할 데미지를 설정합니다.
    /// </summary>
    public void SetDamage(int damage)
    {
        if (!IsServer) return;
        this.bombDamage = damage;
    }
}