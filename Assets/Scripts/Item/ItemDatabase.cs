using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    public List<ItemData> itemDataList;
    private Dictionary<int, ItemData> itemDataDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
    }

    private void InitializeDatabase()
    {
        itemDataDictionary = itemDataList.Where(item => item != null && item.itemId != -1)
            .ToDictionary(item => item.itemId);
        Debug.Log($"ItemDatabase initialized with {itemDataDictionary.Count} items.");
    }

    public ItemData GetItemData(int id)
    {
        if (id == -1) return null;

        if (itemDataDictionary.TryGetValue(id, out ItemData data))
        {
            return data;
        }
        Debug.LogWarning($"[ItemDatabase] ItemData with ID '{id}' not found.");
        return null;
    }

    /// <summary>
    /// 스폰 가능한(isSpawnable == true) 아이템 데이터 리스트를 반환합니다.
    /// </summary>
    /// <returns>스폰 가능한 아이템 리스트</returns>
    public List<ItemData> GetSpawnableItems()
    {
        return itemDataList.Where(item => item != null && item.isSpawnable).ToList();
    }
}