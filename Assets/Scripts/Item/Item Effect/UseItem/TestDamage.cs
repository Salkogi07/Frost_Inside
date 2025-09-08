using System;
using UnityEngine;

public class TestDamage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<Entity_Health>().TakeDamage(1000, transform);
    }
}
