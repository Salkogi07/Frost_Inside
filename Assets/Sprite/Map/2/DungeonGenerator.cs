using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private GameObject[] roomPrefabs;  // �پ��� ũ��� ������ �� ������
    [SerializeField] private int minSmallRooms = 3;     // �ּ� ���� �� ����
    [SerializeField] private int maxSmallRooms = 6;     // �ִ� ���� �� ����
    [SerializeField] private int roomSpacing = 2;       // �� ������ �ּ� �Ÿ� (�� �۰� ����)
    [SerializeField] private Vector2Int dungeonBounds = new Vector2Int(80, 80); // ���� ũ�� ���� (Ȯ��)

    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap groundTilemap;      // �ٴ� Ÿ�ϸ�
    [SerializeField] private Tilemap corridorTilemap;    // ��� Ÿ�ϸ�
    [SerializeField] private Tile corridorTile;          // ��� Ÿ��
    [SerializeField] private Tile groundTile;            // �ٴ� Ÿ��
    [SerializeField] private int corridorWidth = 2;      // ��� �ʺ�

    [Header("Optional Features")]
    [SerializeField] private GameObject[] obstacles;     // �� ���� ��ֹ� ������ (������)
    [SerializeField] private GameObject[] decorations;   // �� ���� ��� ������ (������)

    private List<RoomData> placedRooms = new List<RoomData>();

    [System.Serializable]
    private class RoomData
    {
        public GameObject roomObject;
        public Vector2Int center;
        public Vector2Int size;
        public BoundsInt bounds;

        public RoomData(GameObject room, Vector2Int position)
        {
            roomObject = room;

            if (room != null)
            {
                Tilemap roomTilemap = room.GetComponentInChildren<Tilemap>();
                if (roomTilemap != null)
                {
                    size = new Vector2Int(
                        roomTilemap.cellBounds.size.x,
                        roomTilemap.cellBounds.size.y
                    );
                }
                else
                {
                    size = new Vector2Int(10, 10); // �⺻��
                    Debug.LogWarning("Room prefab does not have a Tilemap component.");
                }
            }
            else
            {
                size = new Vector2Int(10, 10); // �⺻��
            }

            center = position;
            bounds = new BoundsInt(
                position.x - size.x / 2,
                position.y - size.y / 2,
                0,
                size.x,
                size.y,
                1
            );
        }
    }

    void Start()
    {
        if (corridorTile == null || corridorTilemap == null || groundTile == null || groundTilemap == null)
        {
            Debug.LogError("One or more tilemap or tile settings are missing in the Inspector.");
            return;
        }
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        if (roomPrefabs.Length == 0)
        {
            Debug.LogError("No room prefabs assigned.");
            return;
        }
        GameObject largeRoomPrefab = roomPrefabs[0];
        Vector2Int largeRoomPosition = new Vector2Int(0, 0);
        PlaceRoom(largeRoomPrefab, largeRoomPosition, true);

        int smallRoomCount = Random.Range(minSmallRooms, maxSmallRooms + 1);
        int placedSmallRooms = 0;
        int maxTries = 200; // ���� ���� ����

        while (placedSmallRooms < smallRoomCount && maxTries > 0)
        {
            if (TryPlaceSmallRoom())
            {
                placedSmallRooms++;
            }
            maxTries--;
        }

        if (maxTries <= 0)
        {
            Debug.LogWarning("Failed to place all small rooms after maximum attempts.");
        }

        ConnectRooms();

        Debug.Log($"Dungeon generated with {placedRooms.Count} rooms.");
    }

    void PlaceRoom(GameObject roomPrefab, Vector2Int position, bool isLargeRoom = false)
    {
        if (!IsWithinDungeonBounds(position, roomPrefab))
        {
            Debug.LogWarning($"Room placement outside dungeon bounds at {position}, skipping.");
            return;
        }

        GameObject roomInstance = Instantiate(roomPrefab, new Vector3(position.x - 1, position.y, 0), Quaternion.identity);
        RoomData roomData = new RoomData(roomInstance, position);
        placedRooms.Add(roomData);

        FillRoomWithGroundTiles(roomData.bounds);

        if (isLargeRoom && (obstacles.Length > 0 || decorations.Length > 0))
        {
            AddRoomFeatures(roomData.bounds);
        }
    }

    bool IsWithinDungeonBounds(Vector2Int position, GameObject roomPrefab)
    {
        if (roomPrefab == null)
        {
            Debug.LogError("Room prefab is null.");
            return false;
        }

        Tilemap roomTilemap = roomPrefab.GetComponentInChildren<Tilemap>();
        if (roomTilemap == null)
        {
            Debug.LogError("Room prefab does not have a Tilemap component.");
            return false;
        }

        Vector2Int roomSize = new Vector2Int(
            roomTilemap.cellBounds.size.x,
            roomTilemap.cellBounds.size.y
        );

        int minX = position.x - roomSize.x / 2;
        int maxX = position.x + roomSize.x / 2;
        int minY = position.y - roomSize.y / 2;
        int maxY = position.y + roomSize.y / 2;

        return minX >= -dungeonBounds.x / 2 && maxX <= dungeonBounds.x / 2 &&
               minY >= -dungeonBounds.y / 2 && maxY <= dungeonBounds.y / 2;
    }

    bool TryPlaceSmallRoom()
    {
        int maxAttempts = 50;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            GameObject smallRoomPrefab = roomPrefabs[Random.Range(1, roomPrefabs.Length)];
            // �� ���������̰� ���� �������� ��ġ ����
            Vector2Int position = new Vector2Int(
                Random.Range(-dungeonBounds.x / 2 + 10, dungeonBounds.x / 2 - 10),
                Random.Range(-dungeonBounds.y / 2 + 10, dungeonBounds.y / 2 - 10)
            );

            // ������ ��ġ�� ���� �߰��� ��Ģ�� ����
            position.x += Random.Range(-5, 6); // ��5�� ������ ������
            position.y += Random.Range(-5, 6);

            RoomData newRoomData = new RoomData(smallRoomPrefab, position);

            if (!IsOverlappingWithExistingRooms(newRoomData.bounds) && IsWithinDungeonBounds(position, smallRoomPrefab))
            {
                PlaceRoom(smallRoomPrefab, position);
                Debug.Log($"Successfully placed small room at {position}");
                return true;
            }
            attempts++;
        }

        //Debug.LogWarning($"Failed to place a small room at position {position} after max attempts.");
        return false;
    }

    bool IsOverlappingWithExistingRooms(BoundsInt newBounds)
    {
        foreach (var room in placedRooms)
        {
            BoundsInt existingBounds = room.bounds;
            bool xOverlap = newBounds.xMin < existingBounds.xMax + roomSpacing && newBounds.xMax > existingBounds.xMin - roomSpacing;
            bool yOverlap = newBounds.yMin < existingBounds.yMax + roomSpacing && newBounds.yMax > existingBounds.yMin - roomSpacing;
            if (xOverlap && yOverlap)
            {
                return true;
            }
        }
        return false;
    }

    void ConnectRooms()
    {
        if (placedRooms.Count <= 1) return;

        RoomData largeRoom = placedRooms[0];

        for (int i = 1; i < placedRooms.Count; i++)
        {
            Vector2Int start = GetClosestEdgePoint(largeRoom.bounds, placedRooms[i].center);
            Vector2Int end = placedRooms[i].center; // ���� ���� �߽����� ��� ����
            Debug.Log($"Connecting rooms: Start {start}, End {end}");
            CreateCorridor(start, end);
        }
    }

    Vector2Int GetClosestEdgePoint(BoundsInt bounds, Vector2Int point)
    {
        Vector2Int closestPoint = new Vector2Int(
            Mathf.Clamp(point.x, bounds.xMin, bounds.xMax - 1),
            Mathf.Clamp(point.y, bounds.yMin, bounds.yMax - 1)
        );

        if (closestPoint.x == bounds.xMin || closestPoint.x == bounds.xMax - 1)
        {
            closestPoint.y = Mathf.Clamp(point.y, bounds.yMin + corridorWidth, bounds.yMax - 1 - corridorWidth);
        }

        if (closestPoint.y == bounds.yMin || closestPoint.y == bounds.yMax - 1)
        {
            closestPoint.x = Mathf.Clamp(point.x, bounds.xMin + corridorWidth, bounds.xMax - 1 - corridorWidth);
        }

        return closestPoint;
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;
        while (current != end)
        {
            if (current.x != end.x)
            {
                current.x += (end.x > current.x) ? 1 : -1;
            }
            else if (current.y != end.y)
            {
                current.y += (end.y > current.y) ? 1 : -1;
            }
            FillCorridor(current);
        }
    }

    void FillCorridor(Vector2Int position)
    {
        for (int x = -corridorWidth / 2; x <= corridorWidth / 2; x++)
        {
            for (int y = -corridorWidth / 2; y <= corridorWidth / 2; y++)
            {
                Vector3Int tilePosition = new Vector3Int(position.x + x, position.y + y, 0);
                corridorTilemap.SetTile(tilePosition, corridorTile);
            }
        }
    }

    void FillRoomWithGroundTiles(BoundsInt bounds)
    {
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
            }
        }
    }

    void AddRoomFeatures(BoundsInt bounds)
    {
        // ��ֹ� �� ��� �߰� ���� (�ʿ� �� ����)
    }
}