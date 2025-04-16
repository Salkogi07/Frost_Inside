using UnityEngine;

public class Mimic_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f,1f);
    public LayerMask groundLayer; // Ground 레이어를 설정
    public GameObject pos; // OverlapBox의 중심 위치를 지정할 변수




    float Coll;
    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(pos.transform.position, Boxsize,0f);
        foreach(Collider2D collider in hit)
        {
            if(collider.tag == "Player" && Coll >= 3f)
            {Coll = 0f;
                collider.GetComponent<Player_Stats>().TakeDamage(10);
            }
        }
        
            
            
       
            if(Coll < 3f) Coll += Time.deltaTime;
       }
    
    //public void hits()
    //{
       
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos.transform.position, Boxsize); // pos를 중심으로 OverlapBox 위치와 크기를 시각적으로 확인
    }
}
