using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    [System.Serializable]
    public class KeyMapping
    {
        public string actionName;
        public KeyCode key;
    }

    public List<KeyMapping> keyMappings = new List<KeyMapping>();
}
