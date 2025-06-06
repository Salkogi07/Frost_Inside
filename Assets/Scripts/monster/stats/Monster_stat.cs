using UnityEngine;

public class Monster_stat : MonoBehaviour
{
    
     public float Health;
     public Stat MaxHealth;
     public Stat Damage;
     public float Attack_speed;
     public Stat Armor;
     public Stat Groggy;
     public float speed;

    [Header("s")]
    //[SerializeField] public Vector2 range = new Vector2(1f, 1f);
    [SerializeField] public float[] range = { 1f, 1f };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = MaxHealth.GetValue();
    }

    // Update is called once per frame
    void Update()
    {
        if(Health <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void damage(float damage)
    {
         Health -= damage;
    }
}
