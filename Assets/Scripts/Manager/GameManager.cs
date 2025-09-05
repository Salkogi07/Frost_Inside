using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject playerPrefab;
    
    public GamePlayerSpawner gamePlayerSpawner;
    public MakeRandomMap makeRandomMap;
    public ItemSpawner itemSpawner;
    public MonsterSpawner monsterSpawner;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}