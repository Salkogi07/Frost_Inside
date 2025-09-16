using UnityEngine;
using System;
using System.Collections.Generic;

public enum MissionObjectiveType
{
    Collect,    // 아이템 수집
    Eliminate   // 몬스터 제거
}

[Serializable]
public struct ItemRequirement
{
    public int requiredItemId;
    public int amount;
}

[Serializable]
public struct EnemyRequirement
{
    public int TargetMonsterId;
    public int EliminationTargetCount;
}

[CreateAssetMenu(fileName = "New Mission", menuName = "Missions/Mission")]
public class Mission : ScriptableObject
{
    [Tooltip("네트워크 동기화를 위한 고유 ID")]
    public int MissionId;
    public string MissionTitle;
    [TextArea(3, 5)]
    [Tooltip("미션 목록에 표시될 간단한 요약 내용")]
    public string MissionSummary;
    [TextArea(5, 10)]
    [Tooltip("미션 상세 정보 창에 표시될 세부 목표들")]
    public string[] MissionDetails;
    
    [Header("Mission Objective")]
    public MissionObjectiveType ObjectiveType;

    [Header("For 'Collect' Type")]
    [Tooltip("목표 타입이 Collect일 경우, 필요한 아이템과 수량 목록")]
    public List<ItemRequirement> CollectionRequirements;

    [Header("For 'Eliminate' Type")]
    [Tooltip("목표 타입이 Eliminate일 경우, 제거해야 할 몬스터의 이름 또는 ID")]
    public List<EnemyRequirement> EliminationRequirements;
    
}