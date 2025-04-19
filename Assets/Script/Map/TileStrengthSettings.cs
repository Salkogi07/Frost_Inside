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
        [Tooltip("값이 클수록 채굴 시간이 더 오래 걸립니다.")]
        public float defense = 1f;
        [Tooltip("체크 해제 시 이 타일은 채굴할 수 없습니다.")]
        public bool canMine = true;         // 추가된 필드
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
    /// 해당 타일의 방어력 반환 (없으면 기본 1f)
    /// </summary>
    public float GetDefense(TileBase tile)
    {
        if (tile != null && defenseLookup.TryGetValue(tile, out var d))
            return d;
        return 1f;
    }

    /// <summary>
    /// 해당 타일이 채굴 가능한지 여부 반환
    /// </summary>
    public bool IsMineable(TileBase tile)
    {
        if (tile == null) return false;
        return !unmineableLookup.Contains(tile);
    }
}
