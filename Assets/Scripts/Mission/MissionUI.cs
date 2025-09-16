using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class MissionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Transform missionListContent;
    [SerializeField] private GameObject missionEmailPrefab;

    [Header("Detail Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text detailSummary;
    
    [SerializeField] private Transform objectivesContent;
    [SerializeField] private GameObject missionObjectivePrefab;
    
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;

    [Header("References")]
    [SerializeField] private MissionComputer missionComputer;

    private Mission selectedMission;
    private List<GameObject> missionEntries = new List<GameObject>();
    private List<GameObject> missionObjectiveEntries = new List<GameObject>();

    private void Start()
    {
        uiPanel.SetActive(false);
        detailPanel.SetActive(false);
        acceptButton.onClick.AddListener(OnAcceptButtonClicked);
        cancelButton.onClick.AddListener(OnCancelOrCloseButtonClicked);
    }

    private void OnEnable()
    {
        MissionManager.instance.OnAvailableMissionsUpdated += PopulateMissionList;
        MissionManager.instance.OnMissionSelected += OnMissionSelectedByHost;
        MissionManager.instance.OnMissionAcceptedStatusChanged += OnMissionAcceptedStatusChanged;
    }

    private void OnDisable()
    {
        if (MissionManager.instance != null)
        {
            MissionManager.instance.OnAvailableMissionsUpdated -= PopulateMissionList;
            MissionManager.instance.OnMissionSelected -= OnMissionSelectedByHost;
            MissionManager.instance.OnMissionAcceptedStatusChanged -= OnMissionAcceptedStatusChanged;
        }
    }

    public void OpenUI()
    {
        uiPanel.SetActive(true);
        MissionManager.instance.SetPanelState(true);
        
        PopulateMissionList(MissionManager.instance.GetAvailableMissions());
        OnMissionSelectedByHost(MissionManager.instance.CurrentMission);
    }

    public void CloseUI()
    {
        uiPanel.SetActive(false);
        MissionManager.instance.SetPanelState(false);
        
        missionComputer.RequestExitComputerServerRpc();
    }
    
    private void OnCancelOrCloseButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost && selectedMission != null && !MissionManager.instance.IsMissionAccepted)
        {
            MissionManager.instance.DeselectMissionServerRpc();
        }
    }

    private void PopulateMissionList(List<Mission> missions)
    {
        foreach (var entry in missionEntries)
        {
            Destroy(entry);
        }
        missionEntries.Clear();

        foreach (var mission in missions)
        {
            GameObject entryObj = Instantiate(missionEmailPrefab, missionListContent);
            MissionEmailEntry emailEntry = entryObj.GetComponent<MissionEmailEntry>();
            emailEntry.Setup(mission, this);
            missionEntries.Add(entryObj);
        }
    }

    /// <summary>
    /// [수정됨] 미션 상세 정보 UI를 표시하고 버튼 활성화 로직을 명확하게 처리합니다.
    /// </summary>
    public void ShowMissionDetails(Mission mission)
    {
        selectedMission = mission;

        // mission이 null이면 (선택된 미션이 없거나 취소된 경우) 상세 패널을 숨깁니다.
        if (mission == null)
        {
            detailPanel.SetActive(false);
            return;
        }
        
        // 선택된 미션이 있으므로 상세 패널을 보여줍니다.
        detailPanel.SetActive(true);
        detailSummary.text = mission.MissionSummary;
        
        foreach (var entry in missionObjectiveEntries)
        {
            Destroy(entry);
        }
        missionObjectiveEntries.Clear();

        foreach (string detail in mission.MissionDetails)
        {
            GameObject entryObj = Instantiate(missionObjectivePrefab, objectivesContent);
            entryObj.GetComponent<MissionObjectiveEntry>().Setup(detail);
            missionObjectiveEntries.Add(entryObj);
        }

        // [수정된 로직]
        // 버튼 활성화 조건: 방장(Host)이고, 아직 미션이 최종 수락되지 않았어야 함.
        bool shouldShowButtons = NetworkManager.Singleton.IsHost && !MissionManager.instance.IsMissionAccepted;
        
        // 이 조건에 따라 수락 및 취소 버튼의 활성화 상태를 결정합니다.
        // 미션이 선택되었을 때만 이 함수가 호출되므로, 두 버튼 모두 이 조건만 만족하면 보이게 됩니다.
        acceptButton.gameObject.SetActive(shouldShowButtons);
        cancelButton.gameObject.SetActive(shouldShowButtons);
    }
    
    private void OnMissionSelectedByHost(Mission mission)
    {
        ShowMissionDetails(mission);
        foreach (var entryObj in missionEntries)
        {
            entryObj.GetComponent<MissionEmailEntry>().SetHighlight(mission != null && entryObj.GetComponent<MissionEmailEntry>().Mission.MissionId == mission.MissionId);
        }
    }

    private void OnAcceptButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost && selectedMission != null)
        {
            MissionManager.instance.AcceptMissionServerRpc();
        }
    }

    private void OnMissionAcceptedStatusChanged(bool isAccepted)
    {
        if (isAccepted)
        {
            acceptButton.gameObject.SetActive(false);
            // 미션이 수락되면 취소 버튼도 비활성화합니다.
            cancelButton.gameObject.SetActive(false);
            ChatManager.instance?.AddMessage($"Mission '{MissionManager.instance.CurrentMission.MissionTitle}' has been locked in.", MessageType.GlobalSystem);
        }
    }
}