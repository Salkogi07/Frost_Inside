using UnityEngine;

public class Monster_stat : MonoBehaviour
{
    [Header("stat")]
     // public Stat MaxHealth;
     // public float Health;
     public Monster_StatGroup MonsterGroup;
     

    [Header("s")]
    //[SerializeField] public Vector2 range = new Vector2(1f, 1f);
    [SerializeField] public float[] range = { 1f, 1f };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Health = MaxHealth.GetValue();
    }

    // Update is called once per frame
    void Update()
    {
        // if(Health <= 0f)
        // {
        //     Destroy(gameObject);
        // }
    }

    // public void damage(float damage)
    // {
    //      Health -= damage;
    // }
}
