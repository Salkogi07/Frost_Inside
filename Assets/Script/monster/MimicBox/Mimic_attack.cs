using UnityEngine;

public class Mimic_attack : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f,1f);
    public LayerMask groundLayer; // Ground 레이어를 설정
    public GameObject pos; // OverlapBox의 중심 위치를 지정할 변수



    private Monster_stat stat;
    private MimicBoxDirector boxDirector;
    float Coll;
    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        boxDirector = GetComponent<MimicBoxDirector>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        Boxsize = new Vector2(stat.range[0], stat.range[1]);
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(pos.transform.position, Boxsize,0f);
        foreach(Collider2D collider in hit)
        {
            if(collider.tag == "Player" && Coll <= 0f)
            {
                Coll = stat.Attack_speed;
                boxDirector.Pattern = MimicBoxDirector.pattern.Attack;
                collider.GetComponent<Player_Stats>().TakeDamage(stat.Damage);
                Debug.Log(Coll);
            }
        }
        
            
            
       
            if(Coll > 0f) Coll -= Time.deltaTime;
       }

    //public void hits()
    //{
    //    Collider2D[] hit = Physics2D.OverlapBoxAll(pos.transform.position, Boxsize, 0f);
    //    foreach (Collider2D collider in hit)
    //    {
    //        if (collider.tag == "Player" && Coll <= 0f)
    //        {
                
                
    //        }
    //    }
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos.transform.position, Boxsize); // pos를 중심으로 OverlapBox 위치와 크기를 시각적으로 확인
    }
}
