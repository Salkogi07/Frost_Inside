using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InGameMissionUI : MonoBehaviour
{
    [Header("UI References")] 
    [SerializeField] private GameObject missionPanel; // 미션 UI 전체 패널
    
    [SerializeField] private Transform objectivesContainer; // 목표 항목들이 추가될 부모 객체
    [SerializeField] private GameObject objectivePrefab; // 목표 항목 UI 프리팹

    private List<GameObject> objectiveEntries = new List<GameObject>();

    private void Start()
    {
        UpdateMissionDisplay(MissionManager.instance.CurrentMission);
    }

    private void UpdateMissionDisplay(Mission mission)
    {
        if (mission == null)
        {
            missionPanel.SetActive(false);
            return;
        }

        // 기존 목표 UI 삭제
        foreach (var entry in objectiveEntries)
        {
            Destroy(entry);
        }

        objectiveEntries.Clear();

        // 새로운 목표 UI 생성
        // MissionDetails 배열을 사용하여 목표를 표시합니다.
        // 실시간 진행 상황 표시는 MissionProgressManager와 연동하여 구현할 수 있습니다.
        foreach (string detail in mission.MissionDetails)
        {
            GameObject entryObj = Instantiate(objectivePrefab, objectivesContainer);
            // 여기서는 MissionObjectiveEntry 프리팹을 재사용한다고 가정합니다.
            entryObj.GetComponent<MissionObjectiveEntry>().Setup(detail);
            objectiveEntries.Add(entryObj);
        }
    }
}