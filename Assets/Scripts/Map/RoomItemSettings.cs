using UnityEngine;

public class RoomItemSettings : MonoBehaviour
{
    [Header("������ ��� ���� ����")]
    [Tooltip("�� �濡�� �ִ� �� ���� �������� �������")]
    public int maxDropCount = 3;

    [Header("���� ���� ����")]
    [Tooltip("�� �濡�� ���� ������ ���� Prefab ����Ʈ")]
    public GameObject[] monsterPrefabs;
}