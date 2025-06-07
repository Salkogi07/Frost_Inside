using UnityEngine;

public class Robot_Cannon : MonoBehaviour
{
    [Header("총알")] 
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] public int bullet = 50;
    [SerializeField] public float cooltime = 0.1f;
    
    public Transform player;

    private float time;

    private Vector3 direction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // player = GameObject.FindGameObjectWithTag("Player").transform;
        Cannon();
    }

    // Update is called once per frame
    void Update()
    {

        if (cooltime <= time && bullet > 0)
        {
            
                           
                                
            Cannon();
                    time = 0f;            
                            
        }

        else
        {
            time += Time.deltaTime;
        }


    }


    public void Cannon()
    {   
        bullet--; 
        direction = (player.position - transform.position).normalized;
        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bulletPrefab.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        
        
        
    }
    
    
}
