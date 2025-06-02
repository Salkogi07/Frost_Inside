using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

[CreateAssetMenu(fileName = "TileStrengthSettings", menuName = "Mining/TileStrengthSettings")]
public class TileStrengthSettings : ScriptableObject
{
    [System.Serializable]
    public class TileStrength
    {
        public TileBase tile;
        [Tooltip("���� Ŭ���� ä�� �ð��� �� ���� �ɸ��ϴ�.")]
        public float defense = 1f;
        [Tooltip("üũ ���� �� �� Ÿ���� ä���� �� �����ϴ�.")]
        public bool canMine = true;         // �߰��� �ʵ�
    }

    public List<TileStrength> strengths = new List<TileStrength>();

    private Dictionary<TileBase, float> defenseLookup;
    private HashSet<TileBase> unmineableLookup;

    private void OnEnable()
    {
        defenseLookup = new Dictionary<TileBase, float>();
        unmineableLookup = new HashSet<TileBase>();

        foreach (var entry in strengths)
        {
            if (entry.tile == null) continue;
            defenseLookup[entry.tile] = entry.defense;
            if (!entry.canMine)
                unmineableLookup.Add(entry.tile);
        }
    }

    /// <summary>
    /// �ش� Ÿ���� ���� ��ȯ (������ �⺻ 1f)
    /// </summary>
    public float GetDefense(TileBase tile)
    {
        if (tile != null && defenseLookup.TryGetValue(tile, out var d))
            return d;
        return 1f;
    }

    /// <summary>
    /// �ش� Ÿ���� ä�� �������� ���� ��ȯ
    /// </summary>
    public bool IsMineable(TileBase tile)
    {
        if (tile == null) return false;
        return !unmineableLookup.Contains(tile);
    }
}
