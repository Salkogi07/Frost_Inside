/*using Unity.Netcode;
using UnityEngine;

public class PlayerGameMangerSetup : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // 이 스크립트는 모든 클라이언트에서 실행되지만,
        // 아래 코드는 오직 이 오브젝트의 "소유자" 클라이언트에서만 실행됩니다.
        if (IsOwner)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.playerPrefab = gameObject;
            }   
        }
    }
}*/