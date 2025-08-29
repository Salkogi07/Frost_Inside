using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    /*[SerializeField] private GameObject dropPrefab;

    protected void DropItem(Inventory_Item _itemData)
    {
        GameObject newDrop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

        Vector2 randomVelocity = new Vector2(Random.Range(-5,5), Random.Range(15,20));

        newDrop.GetComponent<ItemObject>().SetupItem(_itemData, randomVelocity);
    }

    protected void ThrowItem(Inventory_Item _itemData)
    {
        GameObject newDrop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

        Vector2 randomVelocity = new Vector2(3 * transform.localScale.x * -1, 8);

        newDrop.GetComponent<ItemObject>().SetupItem(_itemData, randomVelocity);
    }*/
}
