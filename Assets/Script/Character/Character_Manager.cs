using UnityEngine;
using UnityEngine.SceneManagement;

public class Character_Manager : MonoBehaviour
{
    public static Character_Manager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) return;
        DontDestroyOnLoad(instance);
    }

    public Character_Data currentCharacter;
    public Select_Character selectedCharacter; // ���õ� ĳ���� ����

    public void LoadingScenes()
    {
        SceneManager.LoadScene("LoadingScene");
    }
}
