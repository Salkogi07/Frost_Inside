using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MakeRandomMap : MonoBehaviour
{
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private int maxRooms = 5;
    [SerializeField] private SpreadTilemap spreadTilemap;
    [SerializeField] private GameObject player;

    // ���� �õ带 ���� ����: Inspector���� ���� ������ ���� ����
    [SerializeField] private int seed;
    // true�̸� ���� ������ �õ�(��: TickCount)�� ����Ͽ� ���� �ʱ�ȭ
    [SerializeField] private bool useRandomSeed = true;

    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();

    private void Start()
    {
        // ���� �õ� ����: useRandomSeed�� true��� �ý��� �õ带 ���
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
        // �ʱ�ȭ
        spreadTilemap.ClearAllTiles();
        floorTiles.Clear();
        wallTiles.Clear();

        // ù ���� (0,0)�� ����
        Vector2Int startPos = Vector2Int.zero;
        // ���� ���� �׻� roomPrefabs[0] ��� (���ϴ� ��� Random.Range�� ���� ����)
        PlaceRoom(roomPrefabs[0], startPos);

        // ���� maxRooms-1 ���� ���� �߰��� ����
        for (int i = 1; i < maxRooms; i++)
        {
            // ���� �������� ���, ���� �� �ִ� ��ġ�� ã�´�
            GameObject nextRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            if (TryFindPlacementForRoom(nextRoomPrefab, out Vector2Int foundOffset))
            {
                // ��ġ�� �ʴ� ��ġ�� ã�Ҵٸ� ���� ��ġ
                PlaceRoom(nextRoomPrefab, foundOffset);
            }
            else
            {
                // �� �̻� ���� �� �ִ� ���� ���ٸ� �ߴ�
                break;
            }
        }

        // Ÿ�ϸ� ����
        spreadTilemap.SpreadFloorTilemap(floorTiles);
        spreadTilemap.SpreadWallTilemap(wallTiles);

        // �÷��̾� ��ġ ���� (�������� ������ �� ��ġ�� �� ���� �ְ�, �׳� (0,0)���� �� ���� ����)
        player.transform.position = Vector2.zero;
    }

    /// <summary>
    /// �� ��(RoomPrefab)�� ������ ���� ������(offset)�� ������ ��ġ(floorTiles, wallTiles�� ����)
    /// </summary>
    private void PlaceRoom(GameObject roomPrefab, Vector2Int offset)
    {
        // roomPrefab ������ FloorTilemap / WallTilemap ã�Ƽ� ���� ����
        Tilemap floorTilemap, wallTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap);

        // �ٴ� Ÿ�� ����
        CopyTilemap(floorTilemap, offset, floorTiles);
        // �� Ÿ�� ����
        CopyTilemap(wallTilemap, offset, wallTiles);
    }

    /// <summary>
    /// �� ��(RoomPrefab)�� ���� ���� ��Ȯ�� 1ĭ ���̷� �´�� ��ġ�� �� �ִ� �������� ã�´�.
    /// ã���� true�� �����ϰ�, foundOffset�� �� �������� ��´�.
    /// </summary>
    private bool TryFindPlacementForRoom(GameObject roomPrefab, out Vector2Int foundOffset)
    {
        foundOffset = Vector2Int.zero;

        // 1) �� �������� WallTilemap(���� ��ǥ��)�� ���Ѵ�
        Tilemap floorTilemap, wallTilemap;
        GetTilemaps(roomPrefab, out floorTilemap, out wallTilemap);

        // �� ���� �� Ÿ��(���� ��ǥ) ���
        List<Vector2Int> newRoomLocalWallTiles = GetLocalWallPositions(wallTilemap);
        if (newRoomLocalWallTiles.Count == 0)
            return false;

        // 2) ���� ���� ���� �� Ÿ�� ���
        List<Vector2Int> existingWallList = new List<Vector2Int>(wallTiles);
        if (existingWallList.Count == 0)
        {
            // ���� ���� �ϳ��� ���� ���¶��, (0,0)�� �׳� ���δٰ� ����
            // (���� ù ���� ��ġ�� �ڶ�� wallTiles�� ���� ��������, ������ġ�� ��)
            existingWallList.Add(Vector2Int.zero);
        }

        existingWallList.Shuffle();
        newRoomLocalWallTiles.Shuffle();

        // 3) ���� �� Ÿ�� / �� �� �� Ÿ�� / ����(�����¿�)�� �����Ͽ�
        //    "������ �� �ٵ���" �������� ����ϰ�, ��ġ ���� ����(��ħ ����)�� Ȯ��
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int existingWall in existingWallList)
        {
            foreach (Vector2Int localWallPos in newRoomLocalWallTiles)
            {
                foreach (Vector2Int dir in directions)
                {
                    // ���� ��(existingWall)�� �� ��(localWallPos)�� ��Ȯ�� 1ĭ ���̷� �ٵ��� ������ ���
                    Vector2Int offset = (existingWall + dir) - localWallPos;

                    // �� offset���� �� ���� ��ġ���� ��, ���� �ٴ�/���� ��ġ�� �ʴ��� �˻�
                    if (!OverlapsExistingTiles(floorTilemap, offset, floorTiles) &&
                        !OverlapsExistingTiles(wallTilemap, offset, wallTiles))
                    {
                        foundOffset = offset;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// tilemap ���� ��� "�� Ÿ��" (HasTile�� ��ġ)�� ���� ��ǥ(Vector2Int)�� ��ȯ
    /// </summary>
    private List<Vector2Int> GetLocalWallPositions(Tilemap wallTilemap)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        BoundsInt bounds = wallTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                result.Add((Vector2Int)pos);
            }
        }
        return result;
    }

    /// <summary>
    /// �� Ÿ�ϸ�( floor or wall )�� offset��ŭ �����̵����� ��,
    /// ������ targetSet( floorTiles or wallTiles )�� ��ġ���� �˻�
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
                    // �̹� �����ϴ� Ÿ�ϰ� ��ħ
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// tilemap�� ��� Ÿ���� offset��ŭ ���� �̵��Ͽ� targetSet�� ����
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
    /// roomPrefab���� FloorTilemap, WallTilemap�� ã�Ƽ� out �Ķ���ͷ� ��ȯ
    /// (������ ������Ʈ ������ �°� ���� �ʿ�)
    /// </summary>
    private void GetTilemaps(GameObject roomPrefab, out Tilemap floorTilemap, out Tilemap wallTilemap)
    {
        // ����: roomPrefab > (index 1) Child > FloorTilemap, WallTilemap
        Transform[] gridTransforms = roomPrefab.transform.GetComponentsInChildren<Transform>();

        // ���� ������ ���� �޶��� �� ������, ��Ȳ�� �°� �����ϼ���.
        Transform tilemapParent = gridTransforms[1].transform;
        floorTilemap = tilemapParent.Find("FloorTilemap").GetComponent<Tilemap>();
        wallTilemap = tilemapParent.Find("WallTilemap").GetComponent<Tilemap>();
    }
}

/// <summary>
/// ����Ʈ ���ÿ� Ȯ�� �޼���
/// </summary>
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
