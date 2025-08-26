using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyGamePlayerSpawner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // 씬 이벤트 및 플레이어 추가 이벤트를 구독하여 스폰 타이밍을 감지
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerAdded += HandlePlayerAdded;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        
        if (NetworkManager.Singleton?.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
        }
        if (PlayerDataManager.instance != null)
        {
            PlayerDataManager.instance.OnPlayerAdded -= HandlePlayerAdded;
        }
    }

    // 조건 1: 플레이어가 로비 씬 로딩을 완료했을 때
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete && sceneEvent.SceneName == "LobbyGame")
        {
            Debug.Log($"[Lobby Spawner] 클라이언트 {sceneEvent.ClientId}가 로비 씬에 진입. 스폰을 요청합니다.");
            PlayerSpawnController.instance.SpawnPlayerForClient(sceneEvent.ClientId);
        }
    }

    // 조건 2: 다른 플레이어들이 이미 로비에 있을 때, 새로운 플레이어가 접속했을 때
    private void HandlePlayerAdded(PlayerInfo newPlayer)
    {
        if (SceneManager.GetActiveScene().name == "LobbyGame")
        {
            Debug.Log($"[Lobby Spawner] 새로운 플레이어 {newPlayer.SteamName}가 로비에 참여. 스폰을 요청합니다.");
            PlayerSpawnController.instance.SpawnPlayerForClient(newPlayer.ClientId);
        }
    }
}