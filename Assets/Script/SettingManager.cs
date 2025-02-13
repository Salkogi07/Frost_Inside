using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SettingManager : MonoBehaviour
{
    // Inspector에서 각 액션과 키를 설정할 수 있도록 직렬화합니다.
    [System.Serializable]
    public class KeyMapping
    {
        public string actionName;      // 액션 이름 (예: "Jump", "Shoot")
        public KeyCode key;            // 해당 액션에 할당된 키
        public UnityEvent onAction;    // 키 입력 시 실행할 이벤트
    }

    // Inspector에서 여러 키 매핑을 리스트 형태로 관리합니다.
    public List<KeyMapping> keyMappings = new List<KeyMapping>();

    // 빠른 조회를 위해 KeyCode와 UnityEvent를 매핑하는 Dictionary
    private Dictionary<KeyCode, UnityEvent> keyEventDictionary;

    void Awake()
    {
        keyEventDictionary = new Dictionary<KeyCode, UnityEvent>();

        // 리스트에 설정된 키 매핑을 Dictionary에 등록
        foreach (KeyMapping mapping in keyMappings)
        {
            if (!keyEventDictionary.ContainsKey(mapping.key))
            {
                keyEventDictionary.Add(mapping.key, mapping.onAction);
            }
            else
            {
                Debug.LogWarning("이미 등록된 키입니다: " + mapping.key);
            }
        }
    }

    void Update()
    {
        // Dictionary에 등록된 모든 키를 검사하여 입력이 들어오면 해당 이벤트 실행
        foreach (KeyValuePair<KeyCode, UnityEvent> keyPair in keyEventDictionary)
        {
            if (Input.GetKeyDown(keyPair.Key))
            {
                keyPair.Value.Invoke();
            }
        }
    }

    // 동적으로 키를 재설정할 수 있는 메서드 (예: 옵션 메뉴에서 사용)
    public void RebindKey(string actionName, KeyCode newKey)
    {
        // 리스트에서 해당 액션을 찾은 후 리바인딩 수행
        for (int i = 0; i < keyMappings.Count; i++)
        {
            if (keyMappings[i].actionName == actionName)
            {
                // 기존 키와 연관된 Dictionary 항목 삭제
                if (keyEventDictionary.ContainsKey(keyMappings[i].key))
                {
                    keyEventDictionary.Remove(keyMappings[i].key);
                }

                // 새 키 설정
                KeyMapping mapping = keyMappings[i];
                mapping.key = newKey;

                // 새 키가 이미 등록되어 있지 않은지 확인 후 추가
                if (!keyEventDictionary.ContainsKey(newKey))
                {
                    keyEventDictionary.Add(newKey, mapping.onAction);
                }
                else
                {
                    Debug.LogWarning("새 키가 이미 다른 액션에 할당되어 있습니다: " + newKey);
                }
                break;
            }
        }
    }
}
