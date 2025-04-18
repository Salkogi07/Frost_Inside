using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileMiningSettings", menuName = "Mining/TileMiningSettings")]
public class TileMiningSetting : ScriptableObject
{
    [System.Serializable]
    public class TileData
    {
        public TileBase tile;          // 채굴 대상 타일 에셋
        [Tooltip("이 타일의 방어력 (높을수록 느리게 깎임)")]
        public float defense;          // 방어력 수치
    }

    public List<TileData> tileConfigs = new List<TileData>();

    // 런타임용 딕셔너리
    private Dictionary<TileBase, float> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<TileBase, float>();
        foreach (var data in tileConfigs)
            if (data.tile != null && !lookup.ContainsKey(data.tile))
                lookup.Add(data.tile, data.defense);
    }

    // 해당 타일의 방어력 반환 (없으면 0)
    public float GetDefense(TileBase tile) =>
        lookup.TryGetValue(tile, out var d) ? d : 0f;
}
