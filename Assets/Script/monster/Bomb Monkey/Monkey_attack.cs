using UnityEngine;

public class Monkey_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f,1f);
    public LayerMask groundLayer; // Ground 레이어를 설정
    public GameObject prf;
    


    private Monster_stat stat;
    private Bomb_Monkey_Director bomb_Monkey_Director;
    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        bomb_Monkey_Director = GetComponent<Bomb_Monkey_Director>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        Boxsize = new Vector2(stat.range[0], stat.range[1]);
    }

    // Update is called once per frame
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
        Gizmos.DrawWireCube(transform.position, Boxsize); // pos를 중심으로 OverlapBox 위치와 크기를 시각적으로 확인
    }
    
}
