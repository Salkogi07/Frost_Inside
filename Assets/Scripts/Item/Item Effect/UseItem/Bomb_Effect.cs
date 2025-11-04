using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "BombEffect", menuName = "Data/UseItem Effect/Bomb Effect")]
public class Bomb_Effect : UseItem_Effect
{
    [Tooltip("NetworkUseItemPool에 등록된 '던져지는 폭탄' 프리팹")]
    public GameObject thrownBombPrefab;
    
    [Header("폭탄 설정")]
    [Tooltip("폭탄이 가하는 데미지")]
    public int damage = 50;

    [Tooltip("폭탄을 던지는 힘")]
    public float throwForce = 10f;

    public override void ExecuteEffect(GameObject playerObject)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        if (NetworkUseItemPool.Instance == null)
        {
            Debug.LogError("NetworkUseItemPool 인스턴스가 씬에 없습니다!");
            return;
        }

        // 1. 새로운 사용 아이템 풀에서 thrownBombPrefab에 해당하는 오브젝트를 가져옵니다.
        NetworkObject bombNetworkObject = NetworkUseItemPool.Instance.GetObject(thrownBombPrefab, playerObject.transform.position, Quaternion.identity);
        
        if (bombNetworkObject == null)
        {
            Debug.LogError("풀에서 폭탄 오브젝트를 가져오는 데 실패했습니다. Prefab이 NetworkUseItemPool에 등록되었는지 확인하세요.");
            return;
        }

        if (bombNetworkObject.TryGetComponent<ThrownBomb>(out ThrownBomb bomb))
        {
            Vector2 throwDirection = new Vector2(playerObject.GetComponent<Player>().FacingDirection, 0.5f);
            
            bomb.SetDamage(damage); 
            bomb.Launch(throwDirection, throwForce);
        }
        else
        {
            Debug.LogError("풀에서 가져온 오브젝트에 ThrownBomb 스크립트가 없습니다.");
            NetworkUseItemPool.Instance.ReturnObjectToPool(bombNetworkObject); // 즉시 반환
        }
    }
}