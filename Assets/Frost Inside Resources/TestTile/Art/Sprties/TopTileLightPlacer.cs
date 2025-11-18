using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TopTileLightPlacer : MonoBehaviour
{
    [Tooltip("검사할 타일맵 배열")]
    public Tilemap[] tilemaps;

    // 최종적으로 찾아낸 윤곽선 타일의 위치를 저장할 HashSet
    private HashSet<Vector3Int> contourTiles = new HashSet<Vector3Int>();

    [ContextMenu("외부 윤곽선 타일 계산 및 기즈모 표시")]
    public void GenerateFalloffLightMask()
    {
        if (tilemaps == null || tilemaps.Length == 0)
        {
            Debug.LogError("타일맵이 설정되지 않았습니다. 작업을 중단합니다.");
            return;
        }

        // 1. 모든 타일맵을 포함하는 전체 경계를 계산합니다.
        BoundsInt totalBounds = CalculateTotalBounds();
        // 계산 범위를 약간 확장하여 경계 밖에서 시작할 수 있도록 합니다.
        totalBounds.xMin--;
        totalBounds.yMin--;
        totalBounds.xMax++;
        totalBounds.yMax++;

        // 2. Flood Fill을 사용하여 외부 공기 영역을 찾습니다.
        HashSet<Vector3Int> reachableAir = FindReachableAir(totalBounds);

        // 새로운 계산을 위해 이전 데이터를 초기화합니다.
        contourTiles.Clear();

        // 3. 모든 타일을 순회하며 '외부 공기'와 인접한 타일을 찾습니다.
        for (int x = totalBounds.xMin; x < totalBounds.xMax; x++)
        {
            for (int y = totalBounds.yMin; y < totalBounds.yMax; y++)
            {
                Vector3Int currentPos = new Vector3Int(x, y, 0);

                // 현재 위치에 타일이 있는 경우에만 검사
                if (HasTileAtPosition(currentPos))
                {
                    Vector3Int topPos = currentPos + Vector3Int.up;
                    Vector3Int leftPos = currentPos + Vector3Int.left;
                    Vector3Int rightPos = currentPos + Vector3Int.right;

                    // 이웃(위, 왼쪽, 오른쪽) 중 하나라도 '외부 공기'에 포함된다면 윤곽선 타일입니다.
                    if (reachableAir.Contains(topPos) || 
                        reachableAir.Contains(leftPos) || 
                        reachableAir.Contains(rightPos))
                    {
                        contourTiles.Add(currentPos);
                    }
                }
            }
        }

        Debug.Log($"총 {contourTiles.Count}개의 외부 윤곽선 타일을 찾았습니다. Scene 뷰에서 기즈모를 확인하세요.");
    }

    /// <summary>
    /// Flood Fill (BFS)을 사용하여 맵 외부에서 접근 가능한 모든 '공기' 셀을 찾습니다.
    /// </summary>
    private HashSet<Vector3Int> FindReachableAir(BoundsInt bounds)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        // 시작점을 경계의 왼쪽 상단으로 설정
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

                // 경계 내에 있고, 아직 방문하지 않았으며, 타일이 없는 '공기' 셀인 경우
                if (bounds.Contains(next) && !visited.Contains(next) && !HasTileAtPosition(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return visited;
    }

    private BoundsInt CalculateTotalBounds()
    {
        if (tilemaps == null || tilemaps.Length == 0) return new BoundsInt();
        
        BoundsInt totalBounds = tilemaps[0].cellBounds;
        for (int i = 1; i < tilemaps.Length; i++)
        {
            totalBounds.xMin = Mathf.Min(totalBounds.xMin, tilemaps[i].cellBounds.xMin);
            totalBounds.yMin = Mathf.Min(totalBounds.yMin, tilemaps[i].cellBounds.yMin);
            totalBounds.xMax = Mathf.Max(totalBounds.xMax, tilemaps[i].cellBounds.xMax);
            totalBounds.yMax = Mathf.Max(totalBounds.yMax, tilemaps[i].cellBounds.yMax);
        }
        return totalBounds;
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

    private void OnDrawGizmos()
    {
        if (contourTiles == null || contourTiles.Count == 0 || tilemaps == null || tilemaps.Length == 0) return;

        Gizmos.color = Color.yellow;
        Tilemap referenceTilemap = tilemaps[0];

        foreach (Vector3Int cellPosition in contourTiles)
        {
            Vector3 worldPosition = referenceTilemap.GetCellCenterWorld(cellPosition);
            // 기즈모를 좀 더 잘 보이게 하기 위해 큐브 대신 작은 점으로 그립니다.
            Gizmos.DrawCube(worldPosition, referenceTilemap.cellSize * 0.5f);
        }
    }
}