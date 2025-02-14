using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SettingPage
{
    None,
    Menu,
    Key,
    Vidio,
    Sound
}

public class SettingManager : MonoBehaviour
{
    [System.Serializable]
    public class KeyMapping
    {
        public string actionName;
        public KeyCode key;
    }

    public List<KeyMapping> keyMappings = new List<KeyMapping>();

    [Header("Setting Panel")]
    public GameObject MenuPanel;
    public GameObject KeyPanel;

    public SettingPage currentPage = SettingPage.None;
    private bool isPause = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPage == SettingPage.None)
            {
                Enter_MenuPage();
            }
            else if (currentPage == SettingPage.Menu)
            {
                Enter_NonePage();
            }
            else if (currentPage == SettingPage.Key)
            {
                Enter_MenuPage();
            }
        }
    }

    public void Enter_NonePage()
    {
        currentPage = SettingPage.None;
        MenuPanel.SetActive(false);
        KeyPanel.SetActive(false);
        isPause = false;
        OffPause();
    }

    public void Enter_MenuPage()
    {
        currentPage = SettingPage.Menu;
        MenuPanel.SetActive(true);
        KeyPanel.SetActive(false);
        isPause = true;
        OnPause();
    }

    public void Enter_KeyPage()
    {
        currentPage = SettingPage.Key;
        MenuPanel.SetActive(true);
        KeyPanel.SetActive(true);
        isPause = true;
        OnPause();
    }

    public void OnPause()
    {
        Time.timeScale = 0;
    }

    public void OffPause()
    {
        Time.timeScale = 1;
    }
}
