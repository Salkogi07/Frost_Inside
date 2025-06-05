using UnityEngine;

public class White_Parasite_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f,1f);
    public LayerMask groundLayer;


    private Monster_stat stat;
    private White_Parasite_Director white_Parasite_Director;

    void Start()
    {
        stat = GetComponent<Monster_stat>();
        white_Parasite_Director = GetComponent<White_Parasite_Director>();
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
                white_Parasite_Director.Pattern = White_Parasite_Director.pattern.Attack;
                //인벤토리에 들어가 티타임
                //도와주시오
                Debug.Log("티타임");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Boxsize);
    }
}
