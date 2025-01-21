using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemImage;

    public InventoryItem item;

    
    public void UpdateSlot(InventoryItem _newItem)
    {
        item = _newItem;

        if(item != null)
        {
            itemImage.sprite = item.data.icon;
        }
    }
}
