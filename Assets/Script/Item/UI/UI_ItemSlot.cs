using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image itemImage;

    public InventoryItem item;

    
    public void UpdateSlot(InventoryItem _newItem)
    {
        item = _newItem;

        itemImage.color = Color.white;

        if(item != null)
        {
            itemImage.sprite = item.data.icon;
        }
    }

    public void CleanUpSlot()
    {
        item = null;

        itemImage.sprite = null;
        itemImage.color = Color.clear;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {

        if (Input.GetKey(KeyManager.instance.GetKeyCodeByName("Throw Item")))
        {
            PlayerManager.instance.item_drop.GenerateThrow(item.data);
            return;
        }

        if(item.data.itemType == ItemType.Equipment)
            Inventory.instance.EquipItem(item.data);
    }
}
