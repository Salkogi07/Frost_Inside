using UnityEngine;
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

    // 클라이언트가 서버에게 타일 채굴을 요청
    [ServerRpc(RequireOwnership = false)]
    public void RequestMineTileServerRpc(Vector3Int cellPosition, TileMapType tileMapType)
    {
        // 모든 클라이언트에게 타일 제거를 명령
        RemoveTileClientRpc(cellPosition, tileMapType);
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