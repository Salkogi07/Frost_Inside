using System;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody2D))]
public class ItemObject : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private readonly NetworkVariable<Inventory_Item> networkItem = new NetworkVariable<Inventory_Item>(
        Inventory_Item.Empty, // 기본값을 빈 아이템으로 설정
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        networkItem.OnValueChanged += OnNetworkItemChanged;
        UpdateVisuals(networkItem.Value);
    }

    public override void OnNetworkDespawn()
    {
        networkItem.OnValueChanged -= OnNetworkItemChanged;
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
            ItemData itemData = ItemDatabase.Instance.GetItemData(item.itemId); // int ID로 조회
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
    
    [ServerRpc(RequireOwnership = false)]
    public void SetupItemServerRpc(Inventory_Item itemData, Vector2 velocity)
    {
        if (!IsServer) return;
        networkItem.Value = itemData;
        rb.linearVelocity = velocity;
    }
}