using System.Collections.Generic;
using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private List<ItemData> itemList; // 여러 개의 아이템 데이터를 받을 리스트

    private void Start()
    {
        DropMultipleItems();
    }

    public void DropMultipleItems()
    {
        float offsetX = 2f; // 2칸 간격

        for (int i = 0; i < itemList.Count; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(i * offsetX, 0, 0);
            GameObject newDrop = Instantiate(dropPrefab, spawnPosition, Quaternion.identity);

            Vector2 randomVelocity = new Vector2(Random.Range(-5, 5), Random.Range(15, 20));

            newDrop.GetComponent<ItemObject>().SetupItem(itemList[i], randomVelocity);
        }
    }
}
