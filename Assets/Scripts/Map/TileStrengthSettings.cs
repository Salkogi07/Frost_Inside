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
        [Tooltip("채광 시간")]
        public float defense = 1f;
        [Tooltip("체크 해제 시 이 타일은 채광할 수 없습니다")]
        public bool canMine = true;
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
    
    public float GetDefense(TileBase tile)
    {
        if (tile != null && defenseLookup.TryGetValue(tile, out var d))
            return d;
        return -1f;
    }
    
    public bool GetIsMineable(TileBase tile)
    {
        if (tile == null) return false;
        return !unmineableLookup.Contains(tile);
    }
}
