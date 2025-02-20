using UnityEngine;
using UnityEngine.Tilemaps;

public class Ladder : MonoBehaviour
{
    Tilemap ladderTilemap;

    private void Awake()
    {
        ladderTilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<Player_Move>().IsLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<Player_Move>().IsLadder = false;
            collision.GetComponent<Player_Move>().IsClimbingAllowed = false;
        }
    }
}
