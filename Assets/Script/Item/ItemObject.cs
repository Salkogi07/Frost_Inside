using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] public ItemData itemData;

    private void OnValidate()
    {
        if (itemData != null)
            return;

        GetComponent<SpriteRenderer>().sprite = itemData.icon;
        gameObject.name = "Item object - " + itemData.name;
    }
}
