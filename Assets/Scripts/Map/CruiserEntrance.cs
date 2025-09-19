using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections;

public class CruiserEntrance : NetworkBehaviour
{
    [Tooltip("플레이어가 텔레포트될 위치")]
    public Transform targetPos;
    
    private CinemachinePositionComposer _cinemachineComposer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 트리거에 들어온 오브젝트에서 NetworkObject 컴포넌트를 찾습니다.
        if (!other.TryGetComponent<NetworkObject>(out var networkObject))
            return;

        // 이 코드를 실행하는 클라이언트가 텔레포트할 플레이어의 주인(Owner)인지 확인합니다.
        // 이렇게 함으로써 오직 로컬 플레이어만 서버에 텔레포트 요청을 보낼 수 있습니다.
        if (networkObject.IsOwner)
        {
            // 서버에 텔레포트를 요청합니다.
            RequestTeleportServerRpc();
            
            // 카메라는 클라이언트 측에서 즉시 처리하여 부드러운 화면 전환을 만듭니다.
            StartCoroutine(TeleportCameraEffect());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTeleportServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // PlayerDataManager에서 요청한 클라이언트의 정보를 가져옵니다.
        PlayerInfo playerInfo = PlayerDataManager.instance.GetPlayerInfo(clientId);

        // 플레이어 정보가 없거나, 이미 사망한 상태라면 텔레포트를 실행하지 않고 함수를 종료합니다.
        if (playerInfo == null || playerInfo.IsDead)
        {
            Debug.Log($"[Server] Teleport request denied for client {clientId}. Reason: Player is dead or not found.");
            return;
        }

        // 서버에 존재하는 플레이어 오브젝트를 찾습니다.
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            // 서버에서 플레이어의 위치를 강제로 변경합니다. 이 변경사항은 모든 클라이언트에게 자동으로 동기화됩니다.
            client.PlayerObject.GetComponent<Player>().Teleport(targetPos.position);
            Debug.Log($"[Server] Teleported client {clientId} to {targetPos.position}");
        }
    }
    
    private IEnumerator TeleportCameraEffect()
    {
        // 씬에서 시네머신 카메라를 한 번만 찾아 캐싱해둡니다.
        if (_cinemachineComposer == null)
        {
            GameObject camObj = GameObject.FindGameObjectWithTag("CinemachineCamera");
            if (camObj != null)
                _cinemachineComposer = camObj.GetComponent<CinemachinePositionComposer>();
        }

        if (_cinemachineComposer != null)
        {
            // 카메라의 Damping(지연 효과)을 꺼서 즉시 플레이어를 따라가게 합니다.
            _cinemachineComposer.Damping = Vector3.zero;
            
            // 텔레포트가 반영될 시간을 잠시 기다립니다.
            yield return new WaitForSeconds(0.1f);
            
            // Damping을 원래 값으로 복원합니다.
            _cinemachineComposer.Damping = Vector3.one;
        }
    }
}