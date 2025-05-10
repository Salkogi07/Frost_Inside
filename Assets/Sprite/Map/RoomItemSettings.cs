using UnityEngine;

public class RoomItemSettings : MonoBehaviour
{
    [Header("아이템 드롭 개수 설정")]
    [Tooltip("이 방에서 최소 몇 개의 아이템을 드롭할지")]
    public int minDropCount = 2;

    [Tooltip("이 방에서 최대 몇 개의 아이템을 드롭할지")]
    public int maxDropCount = 3;
}
