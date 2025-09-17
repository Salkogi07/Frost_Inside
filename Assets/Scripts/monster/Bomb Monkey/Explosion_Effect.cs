using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Explosion_Effect : NetworkBehaviour
{
    public int damage;

    [ClientRpc]
    public void SetDamageClientRpc(int damageValue)
    {
        damage = damageValue;
        Debug.Log($"[Client] Explosion damage set to: {damage}");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Entity_Health>(out var entityHealth))
        {
            entityHealth.TakeDamage(damage, transform);
        }
        
        Debug.Log($"[Server] Dealt {damage} damage to {other.name}");
        
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
