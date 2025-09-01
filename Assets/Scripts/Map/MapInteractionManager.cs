﻿using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

public class MapInteractionManager : NetworkBehaviour
{
    public static MapInteractionManager Instance;
    
    public SpreadTilemap spreadTilemap;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMineTileServerRpc(Vector3Int cellPosition, TileMapType tileMapType)
    {
        // 서버에서만 실행되는 로직
        if (!IsServer) return;

        // 1. 요청된 기본 타일(예: 벽)을 모든 클라이언트에서 제거하도록 명령합니다.
        RemoveTileClientRpc(cellPosition, tileMapType);
        
        GameManager gameManager = GameManager.instance;

        // 2. 채굴된 위치에 Ore 정보가 있는지 확인합니다.
        Vector2Int posKey = (Vector2Int)cellPosition;
        if (gameManager.makeRandomMap != null && gameManager.makeRandomMap.oreTileDict.TryGetValue(posKey, out OreSetting oreInfo))
        {
            // 3. Ore 정보를 찾았다면, 해당 Ore에 설정된 아이템을 스폰합니다.
            //    타일의 월드 좌표를 계산합니다. (중앙으로 보정)
            Vector3 worldPosition = spreadTilemap.OreTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0f);

            // ItemSpawner를 사용해 아이템을 스폰합니다.
            if (gameManager.itemSpawner != null)
            {
                gameManager.itemSpawner.SpawnSingleItem(oreInfo.dropItem, worldPosition);
            }

            // 4. 모든 클라이언트의 Ore 타일맵에서 광석 타일을 제거하도록 명령합니다.
            RemoveTileClientRpc(cellPosition, TileMapType.Ore);

            // 5. 서버의 데이터에서 Ore 정보를 제거하여 중복 생성을 방지합니다.
            gameManager.makeRandomMap.oreTileDict.Remove(posKey);
        }
    }

    // 서버가 모든 클라이언트에게 타일 제거를 명령
    [ClientRpc]
    private void RemoveTileClientRpc(Vector3Int cellPosition, TileMapType tileMapType)
    {
        // 모든 클라이언트의 타일맵에서 해당 타일을 제거
        Tilemap tilemap = spreadTilemap.GetTileMap(tileMapType);
        
        tilemap.SetTile(cellPosition, null);
    }
}