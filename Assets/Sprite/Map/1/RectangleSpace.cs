using UnityEngine;

public class RectangleSpace : MonoBehaviour
{
    public Vector2Int leftDown;
    public int width;
    public int height;

    public RectangleSpace(Vector2Int leftDown, int  width, int height)
    {
        this.leftDown = leftDown;
        this.width = width;
        this.height = height;
    }

    public Vector2Int Center()
    {
        return new Vector2Int(((leftDown.x * 2) + width - 1) / 2, ((leftDown.y * 2) + height - 1) / 2);
    }
}
