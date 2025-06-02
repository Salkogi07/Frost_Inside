using UnityEngine;

public class Monkey_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f,1f);
    public LayerMask groundLayer;
    public GameObject prf;

    private Monster_stat stat;
    private Bomb_Monkey_Director bomb_Monkey_Director;
      
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        bomb_Monkey_Director = GetComponent<Bomb_Monkey_Director>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        Boxsize = new Vector2(stat.range[0], stat.range[1]);
    }
    
    void Update()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize,0f);
        foreach(Collider2D collider in hit)
        {
            if(collider.tag == "Player" )
            {
                bomb_Monkey_Director.Pattern = Bomb_Monkey_Director.pattern.Attack;
                Instantiate(prf, transform.position, Quaternion.identity);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Boxsize);
    }
}
