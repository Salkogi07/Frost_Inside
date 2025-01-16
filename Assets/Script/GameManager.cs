using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public int player_HP = 0;
    private int player_maxHP = 100;
    private bool isDead;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
                Destroy(this.gameObject);
        }
    }
}
