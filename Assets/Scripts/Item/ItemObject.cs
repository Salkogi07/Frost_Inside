using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody2D))]
public class ItemObject : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Network Sync")]
    [SerializeField] private float positionUpdateThreshold = 0.1f; // 이 거리 이상 움직여야 위치 전송
    [SerializeField] private float lerpDuration = 0.1f; // 클라이언트에서의 보간 시간

    // --- 아이템 정보 동기화 변수 ---
    private readonly NetworkVariable<Inventory_Item> networkItem = new NetworkVariable<Inventory_Item>(
        Inventory_Item.Empty,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // --- 위치 동기화 변수 (추가된 부분) ---
    private readonly NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 클라이언트 보간용 변수
    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    
    // 서버 최적화용 변수
    private Vector2 _lastSentPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        // 아이템 정보 변경 이벤트 구독
        networkItem.OnValueChanged += OnNetworkItemChanged;
        
        // 서버/클라이언트에 따라 역할 분리
        if (IsServer)
        {
            // 서버는 현재 위치를 마지막 전송 위치로 초기화
            _lastSentPosition = transform.position;
        }
        else // IsClient
        {
            // 클라이언트는 물리 시뮬레이션을 비활성화
            rb.isKinematic = true;
            
            // 위치 변경 이벤트 구독
            _networkPosition.OnValueChanged += OnPositionChanged;
        }
        
        // 스폰 시 첫 시각적 업데이트
        UpdateVisuals(networkItem.Value);
    }

    public override void OnNetworkDespawn()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        networkItem.OnValueChanged -= OnNetworkItemChanged;
        
        if (!IsServer)
        {
            _networkPosition.OnValueChanged -= OnPositionChanged;
        }
    }
    
    // 서버가 물리 업데이트 주기에 맞춰 위치 변화를 감지하고 전송
    private void FixedUpdate()
    {
        if (!IsServer) return;

        // 마지막으로 보낸 위치와 현재 위치의 거리가 threshold보다 크면 위치 업데이트
        if (Vector2.Distance(transform.position, _lastSentPosition) > positionUpdateThreshold)
        {
            _networkPosition.Value = transform.position;
            _lastSentPosition = transform.position;
        }
    }

    // 클라이언트는 매 프레임 보간 처리
    private void Update()
    {
        if (IsServer) return;
        
        // 보간 진행
        if (_lerpTime < lerpDuration)
        {
            _lerpTime += Time.deltaTime;
            transform.position = Vector2.Lerp(_lerpStartPos, _lerpTargetPos, _lerpTime / lerpDuration);
        }
    }

    // (클라이언트 전용) 서버로부터 새로운 위치 값을 받았을 때 호출됨
    private void OnPositionChanged(Vector2 previousValue, Vector2 newValue)
    {
        // 새로운 보간 시작
        _lerpStartPos = transform.position;
        _lerpTargetPos = newValue;
        _lerpTime = 0;
    }

    private void OnNetworkItemChanged(Inventory_Item previousValue, Inventory_Item newValue)
    {
        UpdateVisuals(newValue);
    }

    private void UpdateVisuals(Inventory_Item item)
    {
        if (ItemDatabase.Instance == null) return;

        if (!item.IsEmpty())
        {
            ItemData itemData = ItemDatabase.Instance.GetItemData(item.itemId);
            if (itemData != null)
            {
                spriteRenderer.sprite = itemData.icon;
                gameObject.name = "Item - " + itemData.itemName;
            }
            else
            {
                spriteRenderer.sprite = null;
                gameObject.name = "Item - Unknown";
            }
        }
        else
        {
            spriteRenderer.sprite = null;
            gameObject.name = "Item - Empty";
        }
    }
    
    [ClientRpc]
    public void SetupItemClientRpc(Inventory_Item itemData, Vector2 velocity)
    {
        if (!IsServer) return;
        networkItem.Value = itemData;
        
        // 물리적 힘을 가해 아이템을 날림
        rb.AddForce(velocity, ForceMode2D.Impulse);
    }

    public Inventory_Item GetItemData() => networkItem.Value;
}