// MissionProgressManager.cs
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class MissionProgressManager : NetworkBehaviour
{
    public static MissionProgressManager instance;

    // key: 아이템/몬스터 ID, value: 수량
    private Dictionary<int, int> itemProgress = new Dictionary<int, int>();
    private Dictionary<int, int> enemyProgress = new Dictionary<int, int>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// (서버 전용) 새로운 미션을 위해 모든 진행 상황을 초기화합니다.
    /// </summary>
    public void ResetProgress()
    {
        if (!IsServer) return;
        itemProgress.Clear();
        enemyProgress.Clear();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportItemCollectedServerRpc(int itemId, int amount)
    {
        if (!itemProgress.ContainsKey(itemId))
        {
            itemProgress[itemId] = 0;
        }
        itemProgress[itemId] += amount;
        Debug.Log($"Server: Item {itemId} collected. Total: {itemProgress[itemId]}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportEnemyEliminatedServerRpc(int enemyId)
    {
        if (!enemyProgress.ContainsKey(enemyId))
        {
            enemyProgress[enemyId] = 0;
        }
        enemyProgress[enemyId]++;
        Debug.Log($"Server: Enemy {enemyId} eliminated. Total: {enemyProgress[enemyId]}");
    }

    /// <summary>
    /// (서버 전용) 현재 미션의 성공 여부를 체크합니다.
    /// </summary>
    public void CheckMissionSuccess()
    {
        if (!IsServer) return;

        Mission currentMission = MissionManager.instance.CurrentMission;
        if (currentMission == null)
        {
            Debug.LogWarning("Cannot check mission success: No mission is active.");
            return;
        }

        bool isSuccess = true;
        if (currentMission.ObjectiveType == MissionObjectiveType.Collect)
        {
            foreach (var requirement in currentMission.CollectionRequirements)
            {
                if (!itemProgress.ContainsKey(requirement.requiredItemId) || itemProgress[requirement.requiredItemId] < requirement.amount)
                {
                    isSuccess = false;
                    break;
                }
            }
        }
        else if (currentMission.ObjectiveType == MissionObjectiveType.Eliminate)
        {
            foreach (var requirement in currentMission.EliminationRequirements)
            {
                if (!enemyProgress.ContainsKey(requirement.TargetMonsterId) || enemyProgress[requirement.TargetMonsterId] < requirement.EliminationTargetCount)
                {
                    isSuccess = false;
                    break;
                }
            }
        }

        if (isSuccess)
        {
            Debug.Log($"Mission '{currentMission.MissionTitle}' Succeeded!");
            // 성공 보상 로직
        }
        else
        {
            Debug.Log($"Mission '{currentMission.MissionTitle}' Failed!");
            // 실패 페널티: 평판 감소
            MissionManager.instance.DecreaseReputationServerRpc();
        }

        // 다음 미션을 위해 진행상황 초기화
        ResetProgress();
    }
}