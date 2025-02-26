using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MakeRandomMap : MonoBehaviour
{
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    [SerializeField] private SpreadTilemap spreadTilemap;
    [SerializeField] private GameObject player;

    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed = true;

    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();

    private void Start()
    {
        if (useRandomSeed)
        {
            seed = System.Environment.TickCount;
        }
        Random.InitState(seed);
        Debug.Log("Using seed: " + seed);

        GenerateMap();
    }

    public void GenerateMap()
    {
        spreadTilemap.ClearAllTiles();
        floorTiles.Clear();
        wallTiles.Clear();
        corridorTiles.Clear();

        // ù ���� (0,0)�� ��ġ
        Vector2Int startPos = Vector2Int.zero;
        PlaceRoom(roomPrefabs[0], startPos);

        // ���� maxRooms-1���� �� ����
        for (int i = 1; i < maxRooms; i++)
        {
            GameObject nextRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoomPrefab, out Vector2Int foundOffset))
            {
                PlaceRoom(nextRoomPrefab, foundOffset);
            }
            else
            {
                // �� �̻� ��ġ �Ұ��ϸ� �ߴ�
                break;
            }
        }

        // ���� Ÿ�ϸʿ� Floor, Wall, Corridor ��ġ
        spreadTilemap.SpreadFloorTilemap(floorTiles);
        spreadTilemap.SpreadWallTilemap(wallTiles);
        spreadTilemap.SpreadCorridorTilemap(corridorTiles);

        // �÷��̾� ��ġ ����
        player.transform.position = Vector2.zero;
    }

    /// <summary>
    /// ��(������) Ÿ�ϸ��� ������ ��ġ�� ����
    /// </summary>
    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        Tilemap floorTilemap, wallTilemap, corridorTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap, out corridorTilemap);

        CopyTilemap(floorTilemap, offset, floorTiles);
        CopyTilemap(wallTilemap, offset, wallTiles);
        CopyTilemap(corridorTilemap, offset, corridorTiles);
    }

    /// <summary>
    /// �� ���� ���� �ʿ� ���� �� �ִ� �������� ã�� ����:
    ///  - ���� ����(up/down)�� ��� 1�� �̻� �����̸� OK
    ///  - ���� ����(left/right)�� ��� 2�� �̻� �����̾�� OK
    ///  - ������ ����Ǹ� ���� ������ �� Ÿ���� ����
    /// </summary>
    private bool TryFindPlacementForRoom(GameObject roomPrefab, out Vector2Int foundOffset)
    {
        foundOffset = Vector2Int.zero;

        // �� ��(������)�� Tilemap�� ��������
        Tilemap floorTilemap, wallTilemap, corridorTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap, out corridorTilemap);

        // �� �� ��� Ÿ�� (���� ��ǥ)
        List<Vector2Int> newRoomLocalCorridorTiles = GetLocalCorridorPositions(corridorTilemap);
        if (newRoomLocalCorridorTiles.Count == 0)
            return false;

        // ���� ��� Ÿ�� ���
        List<Vector2Int> existingCorridorList = new List<Vector2Int>(corridorTiles);
        if (existingCorridorList.Count == 0)
        {
            // ���� ��ΰ� ���ٸ� (0,0) ���� ��ζ�� ����
            existingCorridorList.Add(Vector2Int.zero);
        }

        // ������ ��ȸ(����)
        existingCorridorList.Shuffle();
        newRoomLocalCorridorTiles.Shuffle();

        // �����¿� ����
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int existingCorridor in existingCorridorList)
        {
            foreach (Vector2Int localCorridorPos in newRoomLocalCorridorTiles)
            {
                foreach (Vector2Int dir in directions)
                {
                    // ����: 1�� ����, ����: 2�� ����
                    int requiredConnections = (dir.y != 0) ? 1 : 2;

                    // ��ġ ������ ���
                    Vector2Int offset = (existingCorridor + dir) - localCorridorPos;

                    // Floor/Wall ��ħ �˻�
                    if (OverlapsExistingTiles(floorTilemap, offset, floorTiles)) continue;
                    if (OverlapsExistingTiles(wallTilemap, offset, wallTiles)) continue;

                    // ��� ���� �˻�
                    int connectedCount = CountCorridorConnections(newRoomLocalCorridorTiles, offset, existingCorridorList);
                    if (connectedCount >= requiredConnections)
                    {
                        // ���� ����!
                        foundOffset = offset;

                        // ���� ������ �� Ÿ�� �����ϱ�
                        // -> ���� ��� Ÿ��(existingCorridor)�� �� ��� Ÿ���� �ٴ� ���� = (existingCorridor + dir)
                        //    ���� �� �ڸ��� �� Ÿ���� �־��ٸ� ���ش�.
                        Vector2Int connectingPos = existingCorridor + dir;
                        if (wallTiles.Contains(connectingPos))
                        {
                            wallTiles.Remove(connectingPos);
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// �� �� corridor Ÿ�ϵ��� offset��ŭ �Ű��� ��,
    /// ���� corridor Ÿ�ϵ�� �����¿�� ������(= �����) Ÿ�� ������ ����.
    /// </summary>
    private int CountCorridorConnections(
        List<Vector2Int> newRoomLocalCorridorTiles,
        Vector2Int offset,
        List<Vector2Int> existingCorridorList)
    {
        int count = 0;
        HashSet<Vector2Int> existingCorridorSet = new HashSet<Vector2Int>(existingCorridorList);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var localPos in newRoomLocalCorridorTiles)
        {
            Vector2Int worldPos = localPos + offset;
            foreach (var dir in directions)
            {
                Vector2Int neighbor = worldPos + dir;
                if (existingCorridorSet.Contains(neighbor))
                {
                    count++;
                    // �� Ÿ���� ���� �������� ����Ǿ '1��'���θ� ������ break
                    break;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// corridorTilemap ���� ��� Ÿ�� ��ǥ�� ����Ʈ�� ��ȯ
    /// </summary>
    private List<Vector2Int> GetLocalCorridorPositions(Tilemap corridorTilemap)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        BoundsInt bounds = corridorTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (corridorTilemap.HasTile(pos))
            {
                result.Add((Vector2Int)pos);
            }
        }
        return result;
    }

    /// <summary>
    /// sourceTilemap�� offset��ŭ �Ű��� ��, targetSet�� ��ġ���� �˻�
    /// </summary>
    private bool OverlapsExistingTiles(Tilemap sourceTilemap, Vector2Int offset, HashSet<Vector2Int> targetSet)
    {
        BoundsInt bounds = sourceTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (sourceTilemap.HasTile(pos))
            {
                Vector2Int adjustedPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);
                if (targetSet.Contains(adjustedPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// sourceTilemap �� ��� Ÿ���� offset��ŭ ���� �̵��Ͽ� targetSet�� ����
    /// </summary>
    private void CopyTilemap(Tilemap sourceTilemap, Vector2Int offset, HashSet<Vector2Int> targetSet)
    {
        BoundsInt bounds = sourceTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (sourceTilemap.HasTile(pos))
            {
                Vector2Int adjustedPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);
                targetSet.Add(adjustedPos);
            }
        }
    }

    /// <summary>
    /// roomPrefab���� FloorTilemap, WallTilemap, CorridorTilemap ã��
    /// (������Ʈ ������ ���� ���� �ʿ�)
    /// </summary>
    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTilemap, out Tilemap wallTilemap, out Tilemap corridorTilemap)
    {
        Transform[] gridTransforms = roomPrefab.transform.GetComponentsInChildren<Transform>();
        // ���÷� 1�� �ε��� �ڽ��� Ÿ�ϸ� �θ��� ����
        Transform tilemapParent = gridTransforms[1].transform;

        floorTilemap = tilemapParent.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTilemap = tilemapParent.Find("WallTilemap").GetComponent<Tilemap>();
        corridorTilemap = tilemapParent.Find("CorridorTilemap").GetComponent<Tilemap>();
    }
}


public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}