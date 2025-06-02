using UnityEngine;
using System.Collections;

public class explosion_damage : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f, 1f);
    public LayerMask groundLayer; // Ground

    private Monster_stat stat;
    private MimicBoxDirector boxDirector;
    public int damage = 30;

    private bool hasExploded = false;
    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        boxDirector = GetComponent<MimicBoxDirector>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        //Boxsize = new Vector2(stat.range[0], stat.range[1]);
        StartCoroutine(Bombing());
    }

    // Update is called once per frame
    void Update()
    {
        if (hasExploded) return;
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize, 0f);
        foreach (Collider2D collider in hit)
        {
            if (collider.tag == "Player")
            {
                hasExploded = true;
                collider.GetComponent<Player_Stats>().TakeDamage(damage);
            }
            if (collider.tag == "Player")
            {
                Debug.Log("sdsds");

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
        Destroy(gameObject);
    }
}
