using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private GameObject[] roomPrefabs;  // 방 프리팹 (큰 방, 작은 방)
    [SerializeField] private int minSmallRooms = 3;     // 최소 작은 방 개수
    [SerializeField] private int maxSmallRooms = 6;     // 최대 작은 방 개수

    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap groundTilemap;      // 바닥 타일맵
    [SerializeField] private Tilemap corridorTilemap;    // 통로 타일맵
    [SerializeField] private Tile corridorTile;          // 통로 타일
    [SerializeField] private Tile groundTile;            // 바닥 타일
    [SerializeField] private int corridorWidth = 2;      // 통로 너비

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
        // 큰 방 배치
        GameObject largeRoomPrefab = roomPrefabs[0]; // 큰 방은 첫 번째로 설정
        Vector2Int largeRoomPosition = new Vector2Int(0, 0); // 큰 방의 위치는 중심에 배치
        PlaceRoom(largeRoomPrefab, largeRoomPosition);

        // 작은 방 배치
        int smallRoomCount = Random.Range(minSmallRooms, maxSmallRooms + 1);
        for (int i = 0; i < smallRoomCount; i++)
        {
            TryPlaceSmallRoom();
        }

        // 방들을 연결하는 직사각형 통로 생성
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
            GameObject smallRoomPrefab = roomPrefabs[Random.Range(1, roomPrefabs.Length)]; // 작은 방은 첫 번째 제외
            Vector2Int position = new Vector2Int(
                Random.Range(-10, 10),  // X좌표 랜덤
                Random.Range(-10, 10)   // Y좌표 랜덤
            );

            RoomData newRoomData = new RoomData(smallRoomPrefab, position);

            // 방이 기존 방들과 겹치지 않으면 배치
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
            // 기존 방의 경계를 얻고, 새로운 방과 비교
            BoundsInt existingBounds = room.bounds;

            // X축과 Y축 범위가 겹치는지 확인
            bool xOverlap = newBounds.xMin < existingBounds.xMax && newBounds.xMax > existingBounds.xMin;
            bool yOverlap = newBounds.yMin < existingBounds.yMax && newBounds.yMax > existingBounds.yMin;

            // X축과 Y축 모두 겹치면 true 반환
            if (xOverlap && yOverlap)
            {
                return true;
            }
        }
        return false;
    }


    void ConnectRooms()
    {
        if (placedRooms.Count == 0) return;

        RoomData largeRoom = placedRooms[0]; // 첫 번째 방이 큰 방

        for (int i = 1; i < placedRooms.Count; i++) // 작은 방들과 연결
        {
            CreateCorridor(largeRoom.center, placedRooms[i].center);
        }
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        // 직사각형 통로 만들기
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

            // 통로 타일 배치
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
