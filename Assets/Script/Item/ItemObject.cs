using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public ItemData itemData;
    [SerializeField] public Vector2 velocity;

    private void OnValidate()
    {
        if (itemData != null)
            return;

        GetComponent<SpriteRenderer>().sprite = itemData.icon;
        gameObject.name = "Item object - " + itemData.name;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)){
            rb.linearVelocity = velocity;
        }
    }
}
