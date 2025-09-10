using UnityEngine;
using Unity.Netcode;

public class GamePlayerSpawner : MonoBehaviour
{
    private void Awake()
    {
        GameManager.instance.gamePlayerSpawner = this;
    }
    
    /// <summary>
    /// (서버 전용) 모든 플레이어를 게임 씬에 스폰하도록 요청합니다.
    /// </summary>
    public void SpawnAllPlayersForGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log("[Game Spawner] 모든 플레이어 스폰 명령을 수신. Spawn Controller에게 전달합니다.");
        foreach (var playerInfo in PlayerDataManager.instance.GetAllPlayers())
        {
            PlayerSpawnController.instance.SpawnPlayerForClient(playerInfo.ClientId);
            
            PlayerDataManager.instance.AddPlayerSpawnedClient(playerInfo.ClientId);
        }
    }
}