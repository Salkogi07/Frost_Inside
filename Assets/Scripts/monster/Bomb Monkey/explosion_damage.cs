using UnityEngine;
using System.Collections;
using Stats;
using Unity.Netcode;

public class Explosion_damage : NetworkBehaviour
{
    public int damage;

    [ClientRpc]
    public void SetDamageClientRpc(int damageValue)
    {
        damage = damageValue;
        // 클라이언트에서 데미지가 잘 설정되었는지 확인하기 위한 로그
        Debug.Log($"[Client] Explosion damage set to: {damage}");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Entity_Health>(out var entityHealth))
        {
            entityHealth.TakeDamage(damage, transform);
        }

        // 서버에서 데미지 로그 출력
        Debug.Log($"[Server] Dealt {damage} damage to {other.name}");

        // 0.5초 후 네트워크 오브젝트를 파괴합니다.
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        // 서버에서 Despawn을 호출하면 모든 클라이언트에서 오브젝트가 사라집니다.
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
