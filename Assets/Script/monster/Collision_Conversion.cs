using UnityEngine;

public class Collision_Conversion : MonoBehaviour
{
    public Vector2 Boxsize = new Vector2(1f, 0.5f);
    public LayerMask groundLayer; // Ground 레이어를 설정
    public bool Collision = true;
    public bool IsCollision = false;



    private Monster_stat stat;
    private Bomb_Monkey_Director bomb_Monkey_Director;

    //public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<Monster_stat>();
        bomb_Monkey_Director = GetComponent<Bomb_Monkey_Director>();
        // Boxsize = new Vector2(stat.range.x, stat.range.y);
        
    }

    // Update is called once per frame
    void Update()
    {
        //Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize, 0f);





    }
    public void Collision_conversion()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, Boxsize, 0f, groundLayer);
        foreach (Collider2D collider in hit)
        {
            if (collider.tag == "Ground" || collider.tag == "Mining_Tile")
            {
                Debug.Log("연결됬노");
                if(Collision)
                {
                    Collision = false;
                    IsCollision = true;
                }

            }
            //Debug.Log("연결됬노");
        }
        Collision = true;
        //if (hit != null)
        //{
        //    Debug.Log("연결됬노");
        //}
        }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.6f, 0.0f, 0.9f);
        Gizmos.DrawWireCube(transform.position, Boxsize); // pos를 중심으로 OverlapBox 위치와 크기를 시각적으로 확인
    }

}
