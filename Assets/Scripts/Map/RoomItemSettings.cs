using UnityEngine;

public class RoomItemSettings : MonoBehaviour
{
    [Header("아이템 드랍 설정")]
    [Tooltip("아이템 생성 될 수 있는 최대 드랍 개수")]
    public int maxDropCount = 3;

    [Header("몬스터 프리팹 설정")]
    [Tooltip("한 몬스터가 소환할 수 있는 몬스터 프리팹 리스트")]
    public GameObject[] monsterPrefabs;

}