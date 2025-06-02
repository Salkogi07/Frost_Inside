using UnityEngine;

public class Collision_Conversion : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f, 0.5f);
    public LayerMask groundLayer; // Ground
    public bool Collision = true;
    public bool IsCollision = false;

    private Monster_stat stat;
    private Bomb_Monkey_Director bomb_Monkey_Director;
    
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        bomb_Monkey_Director = GetComponent<Bomb_Monkey_Director>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
    }
    
    public void Collision_conversion()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize, 0f, groundLayer);
        foreach (Collider2D collider in hit)
        {
            if (collider.tag == "Ground" || collider.tag == "Mining_Tile")
            {
                Debug.Log("ㄴㄴㅇㄴ");
                if(Collision)
                {
                    Collision = false;
                    IsCollision = true;
                }
            }
        }
        Collision = true;
        //if (hit != null)
        //{
        //    Debug.Log("");
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.6f, 0.0f, 0.9f);
        Gizmos.DrawWireCube(transform.position, Boxsize);
    }
}
