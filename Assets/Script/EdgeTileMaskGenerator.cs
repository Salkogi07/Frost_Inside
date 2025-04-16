using UnityEngine;
using UnityEngine.Tilemaps;

public class EdgeTileMaskGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Texture2D maskTexture;
    public int textureWidth = 256;
    public int textureHeight = 256;

    void Start()
    {
        GenerateEdgeMask();
    }

    void GenerateEdgeMask()
    {
        maskTexture = new Texture2D(textureWidth, textureHeight);
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos))
                {
                    // 주변 4방향 검사
                    bool isEdge = false;
                    Vector3Int[] directions = {
                        new Vector3Int(1, 0, 0),
                        new Vector3Int(-1, 0, 0),
                        new Vector3Int(0, 1, 0),
                        new Vector3Int(0, -1, 0)
                    };

                    foreach (var dir in directions)
                    {
                        if (!tilemap.HasTile(pos + dir))
                        {
                            isEdge = true;
                            break;
                        }
                    }

                    if (isEdge)
                    {
                        int texX = x - bounds.xMin;
                        int texY = y - bounds.yMin;
                        maskTexture.SetPixel(texX, texY, Color.white);
                    }
                }
            }
        }

        maskTexture.Apply();
        // 생성된 마스크 텍스처를 저장하거나 Shader에 전달
    }
}