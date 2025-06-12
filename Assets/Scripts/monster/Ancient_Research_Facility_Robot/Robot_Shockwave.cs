using UnityEngine;
using System.Collections;
using Stats;

public class Robot_Shockwave : MonoBehaviour
{
    public Vector2 initialBoxSize = new Vector2(0.1f, 0.1f); // 시작 크기
    public Vector2 maxBoxSize = new Vector2(3f, 3f); // 최대 크기
    public float expandSpeed = 5f; // 커지는 속도
    public LayerMask groundLayer; // Ground

    private Vector2 boxSize;
    private Monster_stat stat;
    private MimicBoxDirector boxDirector;
    public int damage = 30;

    
    public Animator animator;
    private bool hasExploded = false;
    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        boxDirector = GetComponent<MimicBoxDirector>();
        boxSize = initialBoxSize;
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        //Boxsize = new Vector2(stat.range[0], stat.range[1]);
        animator.Play("Boom_tester");
    }

    // Update is called once per frame
    void Update()
    {
        if (boxSize.x < maxBoxSize.x || boxSize.y < maxBoxSize.y)
        {
            boxSize = Vector2.MoveTowards(boxSize, maxBoxSize, expandSpeed * Time.deltaTime);
        }
        else
        {
            Destroy(gameObject);    
        }
        
        if (hasExploded) return;
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f);
        foreach (Collider2D collider in hit)
        {
            if (collider.tag == "Player")
            {
                hasExploded = true;
                collider.GetComponent<Player_Condition>().TakeDamage(damage);
            }
            // if (collider.tag == "Player")
            // {
            //     Debug.Log("sdsds");
            //
            // }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
    
}
