using System;
using UnityEngine;

public class TestDamage : MonoBehaviour
{
    private void Start()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (var collider in colliders)
        {
            if (collider.tag == "Player")
            {
                collider.GetComponent<Entity_Health>().TakeDamage(50, transform);
                Destroy(gameObject,.5f);
            }
        }
    }
}
