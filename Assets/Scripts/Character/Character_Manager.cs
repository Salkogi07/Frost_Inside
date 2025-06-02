using UnityEngine;

public class Character_Manager : MonoBehaviour
{
    public static Character_Manager instance;

    public Character_Data currentCharacter;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
