using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private ItemData item;

    public void DropItem()
    {
        GameObject newDrop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

        Vector2 randomVelocity = new Vector2(Random.Range(-5,5), Random.Range(15,20));

        newDrop.GetComponent<ItemObject>().SetupItem(item, randomVelocity);
    }
}
