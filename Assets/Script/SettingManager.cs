using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SettingManager : MonoBehaviour
{
    // Inspector���� �� �׼ǰ� Ű�� ������ �� �ֵ��� ����ȭ�մϴ�.
    [System.Serializable]
    public class KeyMapping
    {
        public string actionName;      // �׼� �̸� (��: "Jump", "Shoot")
        public KeyCode key;            // �ش� �׼ǿ� �Ҵ�� Ű
        public UnityEvent onAction;    // Ű �Է� �� ������ �̺�Ʈ
    }

    // Inspector���� ���� Ű ������ ����Ʈ ���·� �����մϴ�.
    public List<KeyMapping> keyMappings = new List<KeyMapping>();

    // ���� ��ȸ�� ���� KeyCode�� UnityEvent�� �����ϴ� Dictionary
    private Dictionary<KeyCode, UnityEvent> keyEventDictionary;

    void Awake()
    {
        keyEventDictionary = new Dictionary<KeyCode, UnityEvent>();

        // ����Ʈ�� ������ Ű ������ Dictionary�� ���
        foreach (KeyMapping mapping in keyMappings)
        {
            if (!keyEventDictionary.ContainsKey(mapping.key))
            {
                keyEventDictionary.Add(mapping.key, mapping.onAction);
            }
            else
            {
                Debug.LogWarning("�̹� ��ϵ� Ű�Դϴ�: " + mapping.key);
            }
        }
    }

    void Update()
    {
        // Dictionary�� ��ϵ� ��� Ű�� �˻��Ͽ� �Է��� ������ �ش� �̺�Ʈ ����
        foreach (KeyValuePair<KeyCode, UnityEvent> keyPair in keyEventDictionary)
        {
            if (Input.GetKeyDown(keyPair.Key))
            {
                keyPair.Value.Invoke();
            }
        }
    }

    // �������� Ű�� �缳���� �� �ִ� �޼��� (��: �ɼ� �޴����� ���)
    public void RebindKey(string actionName, KeyCode newKey)
    {
        // ����Ʈ���� �ش� �׼��� ã�� �� �����ε� ����
        for (int i = 0; i < keyMappings.Count; i++)
        {
            if (keyMappings[i].actionName == actionName)
            {
                // ���� Ű�� ������ Dictionary �׸� ����
                if (keyEventDictionary.ContainsKey(keyMappings[i].key))
                {
                    keyEventDictionary.Remove(keyMappings[i].key);
                }

                // �� Ű ����
                KeyMapping mapping = keyMappings[i];
                mapping.key = newKey;

                // �� Ű�� �̹� ��ϵǾ� ���� ������ Ȯ�� �� �߰�
                if (!keyEventDictionary.ContainsKey(newKey))
                {
                    keyEventDictionary.Add(newKey, mapping.onAction);
                }
                else
                {
                    Debug.LogWarning("�� Ű�� �̹� �ٸ� �׼ǿ� �Ҵ�Ǿ� �ֽ��ϴ�: " + newKey);
                }
                break;
            }
        }
    }
}
