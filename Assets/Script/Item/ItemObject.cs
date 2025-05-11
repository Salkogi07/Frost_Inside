using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public InventoryItem item;

    private void setupVisuals()
    {
        if (item == null)
            return;

        GetComponent<SpriteRenderer>().sprite = item.data.icon;
        gameObject.name = "Item object - " + item.data.name;
    }

    public void SetupItem(InventoryItem _itemData, Vector2 _velocity)
    {
        item = _itemData;
        rb.linearVelocity = _velocity;

        setupVisuals();
    }
}
