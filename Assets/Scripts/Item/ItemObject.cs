// ItemObject.cs (수정된 최종 버전)

using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody2D))]
public class ItemObject : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Network Sync")] [SerializeField]
    private float positionUpdateThreshold = 0.1f;

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
        else
        {
            // Lerp가 끝나면 타겟 위치에 정확히 고정
            transform.position = _lerpTargetPos;
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

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(velocity, ForceMode2D.Impulse);

        StartVisualsClientRpc(transform.position);
    }

    /// <summary>
    /// [ClientRpc] 모든 클라이언트에서 아이템의 시작 위치를 강제로 설정하여
    /// 순간이동 문제를 해결하고 Lerp를 부드럽게 시작하도록 준비합니다.
    /// </summary>
    [ClientRpc]
    private void StartVisualsClientRpc(Vector3 startPosition)
    {
        // 서버는 물리 시뮬레이션을 직접 하므로 이 로직이 필요 없습니다.
        if (IsServer) return;

        // 아이템의 위치를 서버가 알려준 정확한 시작점으로 즉시 이동시킵니다.
        transform.position = startPosition;

        // Lerp 변수들도 이 시작점으로 초기화하여, 다음 NetworkVariable 업데이트를
        // 부드럽게 이어받을 수 있도록 준비합니다.
        _lerpStartPos = startPosition;
        _lerpTargetPos = startPosition;
        _lerpTime = 0f;
    }

    public Inventory_Item GetItemData() => networkItem.Value;

    public void ResetForPool()
    {
        if (!IsServer) return;

        networkItem.Value = Inventory_Item.Empty;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}