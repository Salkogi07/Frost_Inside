using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Player_Stats player;
    public Player_ItemDrop item_drop;
    
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player_Stats>();
        item_drop = player.gameObject.GetComponent<Player_ItemDrop>();
    }
}