using Stats;
using UnityEngine;

public class Ghost_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f, 1f);
    public LayerMask groundLayer; // Ground
    float stay;
    public float Maxstay = 1f;

    private Monster_stat stat;

    public Player_Condition PlayerCondition;
    
    void Start()
    {
        stat = GetComponent<Monster_stat>();

        Boxsize = new Vector2(stat.range[0], stat.range[1]);
    }
    
    void Update()
    {

        Collider2D hit = Physics2D.OverlapBox(transform.position, Boxsize, 0f, groundLayer);

        if (hit != null)
        {
            stay += Time.deltaTime;
            if (stay >= Maxstay)
            {
                PlayerCondition.ChangeTemperature(-1.25f);
                stay = 0f;
            }
        }
        else
        {
            stay = 0f;
             
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Boxsize);
    }
}
