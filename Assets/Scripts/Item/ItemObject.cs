using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public Inventory_Item item;

    private void setupVisuals()
    {
        if (item == null)
            return;

        GetComponent<SpriteRenderer>().sprite = item.data.icon;
        gameObject.name = "Item object - " + item.data.name;
    }

    public void SetupItem(Inventory_Item _itemData, Vector2 _velocity)
    {
        item = _itemData;
        rb.linearVelocity = _velocity;

        setupVisuals();
    }
}
