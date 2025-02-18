using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private GameObject[] roomPrefabs;  // �� ������ (ū ��, ���� ��)
    [SerializeField] private int minSmallRooms = 3;     // �ּ� ���� �� ����
    [SerializeField] private int maxSmallRooms = 6;     // �ִ� ���� �� ����

    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap groundTilemap;      // �ٴ� Ÿ�ϸ�
    [SerializeField] private Tilemap corridorTilemap;    // ��� Ÿ�ϸ�
    [SerializeField] private Tile corridorTile;          // ��� Ÿ��
    [SerializeField] private Tile groundTile;            // �ٴ� Ÿ��
    [SerializeField] private int corridorWidth = 2;      // ��� �ʺ�

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
                size = new Vector2Int(
                    roomTilemap.cellBounds.size.x,
                    roomTilemap.cellBounds.size.y
                );
            }
            else
            {
                size = new Vector2Int(10, 10);
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
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // ū �� ��ġ
        GameObject largeRoomPrefab = roomPrefabs[0]; // ū ���� ù ��°�� ����
        Vector2Int largeRoomPosition = new Vector2Int(0, 0); // ū ���� ��ġ�� �߽ɿ� ��ġ
        PlaceRoom(largeRoomPrefab, largeRoomPosition);

        // ���� �� ��ġ
        int smallRoomCount = Random.Range(minSmallRooms, maxSmallRooms + 1);
        for (int i = 0; i < smallRoomCount; i++)
        {
            TryPlaceSmallRoom();
        }

        // ����� �����ϴ� ���簢�� ��� ����
        ConnectRooms();
    }

    void PlaceRoom(GameObject roomPrefab, Vector2Int position)
    {
        GameObject roomInstance = Instantiate(roomPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        RoomData roomData = new RoomData(roomInstance, position);
        placedRooms.Add(roomData);
        roomInstance.transform.position = new Vector3(position.x, position.y, 0);

        FillRoomWithGroundTiles(roomData.bounds);
    }

    void TryPlaceSmallRoom()
    {
        int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            GameObject smallRoomPrefab = roomPrefabs[Random.Range(1, roomPrefabs.Length)]; // ���� ���� ù ��° ����
            Vector2Int position = new Vector2Int(
                Random.Range(-10, 10),  // X��ǥ ����
                Random.Range(-10, 10)   // Y��ǥ ����
            );

            RoomData newRoomData = new RoomData(smallRoomPrefab, position);

            // ���� ���� ���� ��ġ�� ������ ��ġ
            if (!IsOverlappingWithExistingRooms(newRoomData.bounds))
            {
                PlaceRoom(smallRoomPrefab, position);
                break;
            }

            attempts++;
        }
    }

    bool IsOverlappingWithExistingRooms(BoundsInt newBounds)
    {
        foreach (var room in placedRooms)
        {
            // ���� ���� ��踦 ���, ���ο� ��� ��
            BoundsInt existingBounds = room.bounds;

            // X��� Y�� ������ ��ġ���� Ȯ��
            bool xOverlap = newBounds.xMin < existingBounds.xMax && newBounds.xMax > existingBounds.xMin;
            bool yOverlap = newBounds.yMin < existingBounds.yMax && newBounds.yMax > existingBounds.yMin;

            // X��� Y�� ��� ��ġ�� true ��ȯ
            if (xOverlap && yOverlap)
            {
                return true;
            }
        }
        return false;
    }


    void ConnectRooms()
    {
        for (int i = 0; i < placedRooms.Count - 1; i++)
        {
            CreateCorridor(placedRooms[i].center, placedRooms[i + 1].center);
        }
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        // ���簢�� ��� �����
        while (current.x != end.x || current.y != end.y)
        {
            if (current.x != end.x)
            {
                current.x += (end.x > current.x) ? 1 : -1;
            }
            else if (current.y != end.y)
            {
                current.y += (end.y > current.y) ? 1 : -1;
            }

            // ��� Ÿ�� ��ġ
            FillCorridor(current);
        }
    }

    void FillCorridor(Vector2Int position)
    {
        for (int w = 0; w < corridorWidth; w++)
        {
            Vector3Int tilePosition = new Vector3Int(position.x - w, position.y, 0);
            corridorTilemap.SetTile(tilePosition, corridorTile);
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
}
