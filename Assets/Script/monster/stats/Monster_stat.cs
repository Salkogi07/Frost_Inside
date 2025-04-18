using UnityEngine;

public class Monster_stat : MonoBehaviour
{

    [Header("¿œπ› Ω∫≈»")]
    [SerializeField] public float Health;
    [SerializeField] public float MaxHealth;
    [SerializeField] public float Damage;
    [SerializeField] public float Armor;
    [SerializeField] public float Groggy = 0f;
    [SerializeField] public float speed = 1f;

    [Header("s")]
    //[SerializeField] public Vector2 range = new Vector2(1f, 1f);
    [SerializeField] public float[] range = { 1f, 1f };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
