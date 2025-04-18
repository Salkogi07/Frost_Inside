using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileMiningSettings", menuName = "Mining/TileMiningSettings")]
public class TileMiningSetting : ScriptableObject
{
    [System.Serializable]
    public class TileData
    {
        public TileBase tile;          // ä�� ��� Ÿ�� ����
        [Tooltip("�� Ÿ���� ���� (�������� ������ ����)")]
        public float defense;          // ���� ��ġ
    }

    public List<TileData> tileConfigs = new List<TileData>();

    // ��Ÿ�ӿ� ��ųʸ�
    private Dictionary<TileBase, float> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<TileBase, float>();
        foreach (var data in tileConfigs)
            if (data.tile != null && !lookup.ContainsKey(data.tile))
                lookup.Add(data.tile, data.defense);
    }

    // �ش� Ÿ���� ���� ��ȯ (������ 0)
    public float GetDefense(TileBase tile) =>
        lookup.TryGetValue(tile, out var d) ? d : 0f;
}
