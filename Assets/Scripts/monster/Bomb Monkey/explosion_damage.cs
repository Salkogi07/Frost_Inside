using UnityEngine;
using System.Collections;
using Stats;

public class explosion_damage : MonoBehaviour
{
    public int damage;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<Entity_Health>().TakeDamage(damage, transform);
        Destroy(gameObject,.5f);
    }
}
