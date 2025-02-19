using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    [System.Serializable]
    public class KeyBinding
    {
        public string keyName;
        public KeyCode defaultKey;
        public TextMeshProUGUI keyText; // 키 변경 시 적용할 UI 텍스트
    }

    public List<KeyBinding> keyBindings = new List<KeyBinding>();

    private Dictionary<string, KeyCode> currentKeys = new Dictionary<string, KeyCode>();
    private string waitingForKey = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        LoadKeys();
        UpdateUI();
    }

    public void StartRebinding(string keyName)
    {
        waitingForKey = keyName;
        Debug.Log("키 변경 대기: " + keyName);
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(waitingForKey))
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    SetKey(waitingForKey, key);
                    waitingForKey = null;
                    break;
                }
            }
        }
    }

    public void SetKey(string keyName, KeyCode newKey)
    {
        if (currentKeys.ContainsKey(keyName))
        {
            currentKeys[keyName] = newKey;
            PlayerPrefs.SetString("Key_" + keyName, newKey.ToString());
            UpdateUI();
        }
    }

    public void ResetKey(string keyName)
    {
        foreach (var binding in keyBindings)
        {
            if (binding.keyName == keyName)
            {
                SetKey(keyName, binding.defaultKey);
                break;
            }
        }
    }

    public void ResetAllKeys()
    {
        foreach (var binding in keyBindings)
        {
            SetKey(binding.keyName, binding.defaultKey);
        }
    }

    private void LoadKeys()
    {
        foreach (var binding in keyBindings)
        {
            if (PlayerPrefs.HasKey("Key_" + binding.keyName))
            {
                currentKeys[binding.keyName] = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_" + binding.keyName));
            }
            else
            {
                currentKeys[binding.keyName] = binding.defaultKey;
            }
        }
    }

    private void UpdateUI()
    {
        foreach (var binding in keyBindings)
        {
            if (binding.keyText != null)
            {
                binding.keyText.text = currentKeys[binding.keyName].ToString();
            }
        }
    }
}
