#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    private ItemDatabase itemDatabase;
    private SerializedProperty itemDataListProperty;

    // 각 ItemType별로 폴드아웃(접기/펴기) 상태를 저장할 딕셔너리
    private Dictionary<ItemType, bool> foldoutStates = new Dictionary<ItemType, bool>();
    // 인스펙터에서 리스트에 추가하기 위해 임시로 담아둘 ItemData 변수
    private ItemData itemToAdd;

    // 아이템의 인덱스를 키로, 에러 메시지를 값으로 저장하는 딕셔너리
    private Dictionary<int, string> itemErrors;
    // 경고 아이콘을 캐싱하여 GUI 성능을 최적화합니다.
    private GUIContent warningIcon;

    private void OnEnable()
    {
        itemDatabase = (ItemDatabase)target;
        itemDataListProperty = serializedObject.FindProperty("itemDataList");

        // Enum에 정의된 모든 ItemType에 대해 foldout 상태를 초기화합니다.
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            if (!foldoutStates.ContainsKey(type))
            {
                foldoutStates.Add(type, true); // 기본적으로 펼쳐진 상태로 시작
            }
        }

        // Unity 내장 경고 아이콘을 로드하고 캐싱합니다.
        warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml", "이 아이템에 문제가 있습니다.");
    }

    // 인스펙터가 그려질 때마다 호출됩니다.
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 매번 그릴 때마다 에러를 새로 검사합니다.
        CheckForErrors();

        var groupedItems = new Dictionary<ItemType, List<int>>();
        var nullItemIndices = new List<int>();

        for (int i = 0; i < itemDatabase.itemDataList.Count; i++)
        {
            ItemData item = itemDatabase.itemDataList[i];
            
            if (item == null)
            {
                nullItemIndices.Add(i);
                continue;
            }

            if (!groupedItems.ContainsKey(item.itemType))
            {
                groupedItems[item.itemType] = new List<int>();
            }
            groupedItems[item.itemType].Add(i);
        }

        EditorGUILayout.LabelField("아이템 목록 (타입별 분류)", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            if (groupedItems.ContainsKey(type) && groupedItems[type].Count > 0)
            {
                foldoutStates[type] = EditorGUILayout.Foldout(foldoutStates[type], $"{type} ({groupedItems[type].Count}개)", true);

                if (foldoutStates[type])
                {
                    EditorGUI.indentLevel++;
                    foreach (int index in groupedItems[type])
                    {
                        DrawItemSlot(index);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        if (nullItemIndices.Count > 0)
        {
            EditorGUILayout.LabelField($"비어있는 슬롯 ({nullItemIndices.Count}개)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (int index in nullItemIndices)
            {
                DrawItemSlot(index);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(20);

        EditorGUILayout.LabelField("새 아이템 추가", EditorStyles.boldLabel);
        itemToAdd = (ItemData)EditorGUILayout.ObjectField("추가할 아이템", itemToAdd, typeof(ItemData), false);
        GUI.enabled = (itemToAdd != null);

        if (GUILayout.Button("리스트에 새 아이템 추가"))
        {
            itemDataListProperty.arraySize++;
            SerializedProperty newItemProperty = itemDataListProperty.GetArrayElementAtIndex(itemDataListProperty.arraySize - 1);
            newItemProperty.objectReferenceValue = itemToAdd;
            itemToAdd = null;
            GUI.changed = true;
        }

        GUI.enabled = true;
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// itemDataList를 순회하며 ID가 -1이거나 중복되는 아이템을 찾아 itemErrors 딕셔너리에 기록합니다.
    /// </summary>
    private void CheckForErrors()
    {
        itemErrors = new Dictionary<int, string>();
        var idUsageMap = new Dictionary<int, List<int>>();

        for (int i = 0; i < itemDatabase.itemDataList.Count; i++)
        {
            ItemData item = itemDatabase.itemDataList[i];
            if (item == null) continue;

            // 1. ID가 -1인 경우 에러 추가
            if (item.itemId == -1)
            {
                itemErrors[i] = "아이템 ID가 -1로 설정되었습니다. 유효한 ID를 지정해주세요.";
            }

            // 2. ID 사용처 기록 (중복 검사를 위해)
            if (!idUsageMap.ContainsKey(item.itemId))
            {
                idUsageMap[item.itemId] = new List<int>();
            }
            idUsageMap[item.itemId].Add(i);
        }

        // 3. 기록된 ID 사용처를 기반으로 중복 에러 추가
        foreach (var pair in idUsageMap)
        {
            if (pair.Value.Count > 1) // 한 ID를 2개 이상의 아이템이 사용하면 중복
            {
                foreach (int index in pair.Value)
                {
                    // 기존에 -1 에러가 있었다면, 줄바꿈으로 에러 메시지를 추가합니다.
                    if (itemErrors.ContainsKey(index))
                    {
                        itemErrors[index] += $"\nID '{pair.Key}'가 다른 아이템과 중복됩니다.";
                    }
                    else
                    {
                        itemErrors[index] = $"ID '{pair.Key}'가 다른 아이템과 중복됩니다.";
                    }
                }
            }
        }
    }
    
    // 아이템 슬롯 하나를 그리는 함수
    private void DrawItemSlot(int index)
    {
        SerializedProperty itemProperty = itemDataListProperty.GetArrayElementAtIndex(index);
        ItemData itemData = itemProperty.objectReferenceValue as ItemData;
        
        GUIContent label;
        if (itemData != null)
        {
            string itemName = string.IsNullOrEmpty(itemData.itemName) ? "(이름 없음)" : itemData.itemName;
            label = new GUIContent($"[{itemData.itemId}] {itemName}");
        }
        else
        {
            label = new GUIContent($"Element {index} (None)");
        }
        
        // 이 아이템에 에러가 있는지 확인합니다.
        if (itemErrors.ContainsKey(index))
        {
            // 에러가 있다면, 라벨의 이미지와 툴팁을 설정합니다.
            label.image = warningIcon.image;
            label.tooltip = itemErrors[index];
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(itemProperty, label);
        
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f, 1f);
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            if (EditorUtility.DisplayDialog("아이템 삭제 확인", 
                $"'{ (itemData != null ? itemData.name : "이 슬롯") }'을(를) 리스트에서 정말 삭제하시겠습니까?", 
                "삭제", "취소"))
            {
                // DeleteArrayElementAtIndex는 인덱스를 변경시키므로 조심해야 합니다.
                // 하지만 여기서는 즉시 GUI를 다시 그리므로 문제가 없습니다.
                itemDataListProperty.DeleteArrayElementAtIndex(index);
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }
}
#endif