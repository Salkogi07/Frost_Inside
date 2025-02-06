using UnityEngine;

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
    public Select_Character selectedCharacter; // 선택된 캐릭터 저장
}
