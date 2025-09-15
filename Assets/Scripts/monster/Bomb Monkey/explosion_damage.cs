using UnityEngine;
using System.Collections;
using Stats;
using Unity.Netcode;

public class explosion_damage : NetworkBehaviour
{
    public int damage;
    
    public override void OnNetworkSpawn()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                if(collider.TryGetComponent<Entity_Health>(out Entity_Health health))
                {
                    health.TakeDamage(damage, gameObject.transform);
                }
            }
        }
        
        Invoke(nameof(DestroySelf), 0.5f);
    }
    
    private void DestroySelf()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
    }
}
