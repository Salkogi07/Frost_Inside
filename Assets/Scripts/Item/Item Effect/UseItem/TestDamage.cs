using System;
using Unity.Netcode;
using UnityEngine;

public class TestDamage : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                if(collider.TryGetComponent<Entity_Health>(out Entity_Health health))
                {
                    health.TakeDamage(50, gameObject.transform);
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
