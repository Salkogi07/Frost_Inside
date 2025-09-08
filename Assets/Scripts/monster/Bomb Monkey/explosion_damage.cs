using UnityEngine;
using System.Collections;
using Stats;

public class explosion_damage : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f, 1f);
    public LayerMask groundLayer; // Ground

    public Enemy_Stats enemyStats;
    private Enemy_Bomb_Monkey _bombMonkey;
    

    private bool hasExploded = false;
    private bool closing = false;
    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        _bombMonkey = GetComponent<Enemy_Bomb_Monkey>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        //Boxsize = new Vector2(stat.range[0], stat.range[1]);
        StartCoroutine(Bombing());
    }


    
    // Update is called once per frame
    void Update()
    {
        if (hasExploded) return;
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize, 0f);
        if (!closing)
        {
            foreach (Collider2D collider in hit)
            {
                if (collider.tag == "Player")
                {
                    hasExploded = true;
                    collider.GetComponent<Entity_Health>().TakeDamage(enemyStats.damage.GetValue(), transform);
                    Debug.Log(enemyStats.damage.GetValue());
                }

            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Boxsize);
    }
    IEnumerator Bombing()
    {
        yield return new WaitForSeconds(0.1f);
        closing = true;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
    
}
