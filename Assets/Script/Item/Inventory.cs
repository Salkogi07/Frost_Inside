using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<ItemData> inventory = new List<ItemData>();

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(instance);
    }

    public void AddItem(ItemData _item)
    {
        inventory.Add(_item);
    }

}
