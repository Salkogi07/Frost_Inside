using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class TopTileLightPlacer : MonoBehaviour
{
    [Tooltip("빛을 감지할 타일맵 배열")]
    public Tilemap[] tilemaps;

    [Tooltip("생성된 마스크 텍스처를 적용할 2D 라이트")]
    public Light2D lightToApplyMask;

    [Tooltip("빛이 아래 및 옆으로 몇 칸까지 스며들지 결정합니다.")]
    [Range(1, 10)]
    public int falloffDepth = 4;

    void Start()
    {
        if (tilemaps == null || tilemaps.Length == 0 || lightToApplyMask == null)
        {
            Debug.LogError("필수 컴포넌트(Tilemap 배열 또는 Light2D)가 설정되지 않았습니다.");
            return;
        }

        // 게임 시작 시 라이트 마스크를 생성합니다.
        GenerateFalloffLightMask();
    }

    /// <summary>
    /// 외부 윤곽선을 기준으로 빛 번짐 효과를 생성하고 Light2D에 적용합니다.
    /// </summary>
    [ContextMenu("Generate Light Mask Now")] // 인스펙터에서 수동으로 실행할 수 있도록 메뉴 추가
    public void GenerateFalloffLightMask()
    {
        // 1. 모든 타일맵을 포함하는 전체 경계를 계산합니다.
        BoundsInt totalBounds = CalculateTotalBounds();
        if (totalBounds.size.x == 0 || totalBounds.size.y == 0) return;

        // Flood Fill을 위한 경계를 1칸 확장합니다.
        BoundsInt expandedBounds = totalBounds;
        expandedBounds.xMin--; expandedBounds.yMin--;
        expandedBounds.xMax++; expandedBounds.yMax++;
        
        // 2. Flood Fill을 사용하여 외부에서 접근 가능한 공기 영역을 찾습니다.
        HashSet<Vector3Int> reachableAir = FindReachableAir(expandedBounds);

        int width = totalBounds.size.x;
        int height = totalBounds.size.y;

        // 3. BFS를 위한 자료구조를 초기화합니다.
        float[,] brightnessMap = new float[width, height];
        int[,] distanceMap = new int[width, height]; // 빛으로부터의 거리를 저장
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // distanceMap을 방문하지 않은 상태(-1)로 초기화합니다.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                distanceMap[x, y] = -1;
            }
        }
        
        // 4. 빛이 시작될 지점(외부 공기이면서 타일과 인접한 곳)을 찾아 큐에 추가합니다.
        foreach(Vector3Int airPos in reachableAir)
        {
            // 실제 타일맵 경계 내에 있는 공기만 대상으로 합니다.
            if (!totalBounds.Contains(airPos)) continue;

            // 상하좌우 인접한 곳에 타일이 있는지 확인
            if (HasTileAtPosition(airPos + Vector3Int.up) ||
                HasTileAtPosition(airPos + Vector3Int.down) ||
                HasTileAtPosition(airPos + Vector3Int.left) ||
                HasTileAtPosition(airPos + Vector3Int.right))
            {
                // 셀 좌표를 2D 배열 인덱스로 변환
                int x = airPos.x - totalBounds.xMin;
                int y = airPos.y - totalBounds.yMin;

                if (distanceMap[x, y] == -1) // 아직 큐에 추가되지 않았다면
                {
                    distanceMap[x, y] = 0; // 빛의 시작점이므로 거리는 0
                    brightnessMap[x, y] = 1.0f;
                    queue.Enqueue(new Vector2Int(x, y));
                }
            }
        }

        // 5. BFS를 사용하여 빛을 전파시킵니다.
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDist = distanceMap[current.x, current.y];

            if (currentDist >= falloffDepth) continue; // 빛이 더 이상 퍼지지 않음

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                // 텍스처 범위 내에 있고, 방문하지 않았으며, 타일이 없는 공간이라면
                Vector3Int nextCellPos = new Vector3Int(totalBounds.xMin + nx, totalBounds.yMin + ny, 0);
                if (nx >= 0 && nx < width && ny >= 0 && ny < height && distanceMap[nx, ny] == -1 && !HasTileAtPosition(nextCellPos))
                {
                    int newDist = currentDist + 1;
                    distanceMap[nx, ny] = newDist;
                    brightnessMap[nx, ny] = 1.0f - (newDist / (float)falloffDepth);
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        // 6. 밝기 맵을 기반으로 라이트 마스크 텍스처를 생성합니다.
        Texture2D lightMaskTexture = new Texture2D(width, height, TextureFormat.Alpha8, false);
        Color[] pixelColors = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 알파 값만 사용하므로 new Color(0,0,0, brightness) 형태
                pixelColors[y * width + x] = new Color(0, 0, 0, brightnessMap[x, y]);
            }
        }
        lightMaskTexture.SetPixels(pixelColors);
        lightMaskTexture.filterMode = FilterMode.Point; // 픽셀 아트 스타일과 어울리도록 설정
        lightMaskTexture.Apply();

        // 7. 텍스처로 스프라이트를 생성하고 Light2D에 적용합니다.
        Sprite lightCookieSprite = Sprite.Create(
            lightMaskTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.0f, 0.0f), // 피벗을 좌측 하단으로 설정
            1.0f, // 1 타일 = 1 유닛일 경우 1로 설정 (셀 크기에 따라 조절 필요)
            0,
            SpriteMeshType.FullRect
        );
        lightCookieSprite.name = "Generated_Light_Cookie";
        
        // Light2D 컴포넌트의 위치와 스케일을 타일맵 경계에 맞춥니다.
        lightToApplyMask.transform.position = tilemaps[0].CellToWorld(totalBounds.min);
        lightToApplyMask.transform.localScale = new Vector3(tilemaps[0].cellSize.x, tilemaps[0].cellSize.y, 1);
        
        lightToApplyMask.lightType = Light2D.LightType.Sprite;
        lightToApplyMask.lightCookieSprite = lightCookieSprite;
    }

    #region Helper Functions

    private BoundsInt CalculateTotalBounds()
    {
        if (tilemaps == null || tilemaps.Length == 0) return new BoundsInt();
        
        var firstMap = tilemaps[0];
        BoundsInt totalBounds = firstMap.cellBounds;
        
        for (int i = 1; i < tilemaps.Length; i++)
        {
            totalBounds.xMin = Mathf.Min(totalBounds.xMin, tilemaps[i].cellBounds.xMin);
            totalBounds.yMin = Mathf.Min(totalBounds.yMin, tilemaps[i].cellBounds.yMin);
            totalBounds.xMax = Mathf.Max(totalBounds.xMax, tilemaps[i].cellBounds.xMax);
            totalBounds.yMax = Mathf.Max(totalBounds.yMax, tilemaps[i].cellBounds.yMax);
        }
        return totalBounds;
    }

    private HashSet<Vector3Int> FindReachableAir(BoundsInt bounds)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Vector3Int startPos = new Vector3Int(bounds.xMin, bounds.yMax - 1, 0);

        queue.Enqueue(startPos);
        visited.Add(startPos);

        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                Vector3Int next = new Vector3Int(current.x + dx[i], current.y + dy[i], 0);
                if (bounds.Contains(next) && !visited.Contains(next) && !HasTileAtPosition(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return visited;
    }

    private bool HasTileAtPosition(Vector3Int cellPos)
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap != null && tilemap.HasTile(cellPos))
            {
                return true;
            }
        }
        return false;
    }

    #endregion
}