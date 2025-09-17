using UnityEngine;
using System.Collections;
using Stats;
using Unity.Netcode;

public class Explosion_damage : NetworkBehaviour
{
    public int damage;

    [ClientRpc]
    public void SetDamageRpc(int damageValue)
    {
        damage = damageValue;
        // Ŭ���̾�Ʈ���� �������� �� �����Ǿ����� Ȯ���ϱ� ���� �α�
        Debug.Log($"[Client] Explosion damage set to: {damage}");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // ������ ó���� ���������� ����ǵ��� �����մϴ�.
        if (!IsServer)
        {
            return;
        }

        // ������ Entity_Health ������Ʈ�� ������ �ִ��� Ȯ���ϰ� �������� �ݴϴ�.
        if (other.TryGetComponent<Entity_Health>(out var entityHealth))
        {
            entityHealth.TakeDamage(damage, transform);
        }

        // �������� ������ �α� ���
        Debug.Log($"[Server] Dealt {damage} damage to {other.name}");

        // 0.5�� �� ��Ʈ��ũ ������Ʈ�� �ı��մϴ�.
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        // �������� Despawn�� ȣ���ϸ� ��� Ŭ���̾�Ʈ���� ������Ʈ�� ������ϴ�.
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
