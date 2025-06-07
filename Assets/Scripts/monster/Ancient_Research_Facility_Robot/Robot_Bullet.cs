using Stats;
using UnityEngine;

public class Robot_Bullet : MonoBehaviour
{
    public float Destroy_Time;
    public float speed; 
    public int damage = 2; //임시 방편
    private bool hasHit = false; // 데미지를 줬는지 체크
    
    
    private Monster_StatGroup monsterStatGroup;
    public Vector2 Boxsize = new Vector2(1f, 1f);
    public LayerMask groundLayer; // Ground
    private Transform player;
    private Rigidbody2D rb;
    Vector3 direction;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject,Destroy_Time );
        player = GameObject.FindGameObjectWithTag("Player").transform;
         direction = (player.position - transform.position).normalized;
         // speed += Random.Range();
         float _Y_random = Random.Range(-0.65f, 0.65f);
         transform.position += new Vector3(0f, _Y_random, 0f);
    }

    void Update()
    {
        
        rb.linearVelocity = direction * speed;

        if (!hasHit)
        {
            Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize, 0f);
            foreach (Collider2D collider in hit)
            {
                if (collider.tag == "Player")
                {

                    collider.GetComponent<Player_Condition>().TakeDamage(damage);
                    hasHit = true; // 더 이상 데미지를 주지 않도록
                    break;
                }

            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Boxsize);
    }
}
