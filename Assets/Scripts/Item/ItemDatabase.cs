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
        // 최종적으로 사용할 아이템 딕셔너리
        itemDataDictionary = new Dictionary<int, ItemData>();
        // 중복 검사를 위해 ID별로 아이템을 임시 저장할 딕셔너리
        var itemsByIdForCheck = new Dictionary<int, List<ItemData>>();

        // 1. 모든 아이템 리스트를 한 번만 순회하며 기본 검사를 수행합니다.
        foreach (var item in itemDataList)
        {
            if (item == null)
            {
                continue;
            }

            // 아이템 ID가 -1인 경우 경고 메시지 출력
            if (item.itemId == -1)
            {
                Debug.LogError($"[ItemDatabase] '{item.name}' 아이템의 ID가 -1입니다. ID를 수정해주세요.");
                continue;
            }

            //중복 검사를 위해 아이템을 ID를 키로 하여 임시 딕셔너리에 추가합니다.
            if (!itemsByIdForCheck.ContainsKey(item.itemId))
            {
                // 이 ID가 처음 발견된 경우, 새로운 리스트를 생성해줍니다.
                itemsByIdForCheck[item.itemId] = new List<ItemData>();
            }
            // 해당 ID의 리스트에 현재 아이템을 추가합니다.
            itemsByIdForCheck[item.itemId].Add(item);
        }

        // 4. 임시 딕셔너리를 순회하며 중복 여부를 최종 확인하고, 유효한 아이템만 최종 딕셔너리에 추가합니다.
        foreach (var pair in itemsByIdForCheck)
        {
            int id = pair.Key;
            List<ItemData> itemList = pair.Value;

            // 5. 한 ID에 여러 아이템이 연결되어 있다면 중복이므로 오류를 출력합니다.
            if (itemList.Count > 1)
            {
                string conflictingItemNames = string.Join(", ", itemList.Select(i => $"'{i.name}'"));
                Debug.LogError($"[ItemDatabase] 아이템 ID 중복: ID '{id}'가 다음 아이템들에서 중복으로 사용되었습니다: {conflictingItemNames}");
            }
            else
            {
                // 6. 중복이 아닌 유효한 아이템만 최종 딕셔너리에 추가합니다.
                itemDataDictionary.Add(id, itemList[0]);
            }
        }

        Debug.Log($"ItemDatabase initialized with {itemDataDictionary.Count} valid items.");
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
        return itemDataList.Where(item => item != null && item.isSpawnable && !item.isOre).ToList();
    }
    
    public List<ItemData> GetOreDataList()
    {
        return itemDataList.Where(item => item != null && item.isOre).ToList();
    }
}