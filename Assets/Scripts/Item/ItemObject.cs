// ItemObject.cs (수정된 버전)

using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody2D))]
public class ItemObject : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Network Sync")]
    [SerializeField] private float positionUpdateThreshold = 0.1f;
    [SerializeField] private float lerpDuration = 0.1f;

    private readonly NetworkVariable<Inventory_Item> networkItem = new NetworkVariable<Inventory_Item>(
        Inventory_Item.Empty,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    
    private Vector2 _lastSentPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        networkItem.OnValueChanged += OnNetworkItemChanged;
        
        if (IsServer)
        {
            _lastSentPosition = transform.position;
        }
        else 
        {
            rb.isKinematic = true;
            _networkPosition.OnValueChanged += OnPositionChanged;
        }
        
        UpdateVisuals(networkItem.Value);
    }

    public override void OnNetworkDespawn()
    {
        networkItem.OnValueChanged -= OnNetworkItemChanged;
        
        if (!IsServer)
        {
            _networkPosition.OnValueChanged -= OnPositionChanged;
        }
    }
    
    private void FixedUpdate()
    {
        if (!IsServer || !gameObject.activeInHierarchy) return;

        if (Vector2.Distance(transform.position, _lastSentPosition) > positionUpdateThreshold)
        {
            _networkPosition.Value = transform.position;
            _lastSentPosition = transform.position;
        }
    }

    private void Update()
    {
        if (IsServer) return;
        
        if (_lerpTime < lerpDuration)
        {
            _lerpTime += Time.deltaTime;
            transform.position = Vector2.Lerp(_lerpStartPos, _lerpTargetPos, _lerpTime / lerpDuration);
        }
    }

    private void OnPositionChanged(Vector2 previousValue, Vector2 newValue)
    {
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
            gameObject.name = "Item - Empty (Pooled)";
        }
    }
    
    /// <summary>
    /// (서버 전용) 풀에서 꺼낸 아이템을 설정하고 발사합니다.
    /// </summary>
    public void SetupAndLaunch(Inventory_Item itemData, Vector2 velocity)
    {
        if (!IsServer) return;

        networkItem.Value = itemData;
        
        // 물리적 힘을 가해 아이템을 날림
        rb.linearVelocity = Vector2.zero; // 이전 속도 초기화
        rb.AddForce(velocity, ForceMode2D.Impulse);

        // 클라이언트에게도 동일한 효과를 주기 위해 ClientRpc 호출
        LaunchClientRpc(velocity);
    }

    [ClientRpc]
    private void LaunchClientRpc(Vector2 velocity)
    {
        // 클라이언트에서는 물리 시뮬레이션이 없으므로 시각적인 효과만 필요하다면 여기에 구현
        // 현재 구조에서는 위치 동기화로 충분하므로 비워둡니다.
    }

    public Inventory_Item GetItemData() => networkItem.Value;

    /// <summary>
    /// (서버 전용) 풀에 반환되기 전 오브젝트를 리셋합니다.
    /// </summary>
    public void ResetForPool()
    {
        if (!IsServer) return;
        
        networkItem.Value = Inventory_Item.Empty;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}