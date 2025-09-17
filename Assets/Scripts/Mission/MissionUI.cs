using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class MissionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject uiPanel; // 미션 UI 전체 패널
    [SerializeField] private Transform missionListContent; // 미션 이메일 목록이 표시될 부모 트랜스폼
    [SerializeField] private GameObject missionEmailPrefab; // 개별 미션 이메일 프리팹
    [SerializeField] private TMP_Text mailCountText; // 메일 개수를 표시할 텍스트

    [Header("Detail Panel")]
    [SerializeField] private GameObject detailPanel; // 미션 상세 정보 패널
    [SerializeField] private TMP_Text detailSummary; // 미션 요약 텍스트

    [SerializeField] private Transform objectivesContent; // 미션 목표 목록이 표시될 부모 트랜스폼
    [SerializeField] private GameObject missionObjectivePrefab; // 개별 미션 목표 프리팹

    [SerializeField] private Button acceptButton; // 미션 수락 버튼
    [SerializeField] private Button cancelAcceptanceButton; // 미션 수락 취소 버튼

    [Header("References")]
    [SerializeField] private MissionComputer missionComputer; // 상호작용한 미션 컴퓨터

    private Mission selectedMission; // 현재 선택된 미션
    private List<GameObject> missionEntries = new List<GameObject>(); // 생성된 미션 이메일 UI 목록
    private List<GameObject> missionObjectiveEntries = new List<GameObject>(); // 생성된 미션 목표 UI 목록

    private void Start()
    {
        uiPanel.SetActive(false); // 시작 시 UI 패널 비활성화
        detailPanel.SetActive(false); // 시작 시 상세 정보 패널 비활성화
        acceptButton.onClick.AddListener(OnAcceptButtonClicked); // 수락 버튼 리스너 추가
        cancelAcceptanceButton.onClick.AddListener(OnCancelAcceptanceButtonClicked); // 수락 취소 버튼 리스너 추가
    }

    private void OnEnable()
    {
        // MissionManager의 이벤트에 구독
        MissionManager.instance.OnAvailableMissionsUpdated += PopulateMissionList; // 이용 가능한 미션 목록 업데이트 시
        MissionManager.instance.OnMissionSelected += OnMissionSelectedByHost; // 호스트가 미션을 선택했을 때
        MissionManager.instance.OnMissionAcceptedStatusChanged += OnMissionAcceptedStatusChanged; // 미션 수락 상태가 변경되었을 때
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화 시 MissionManager 이벤트 구독 해제
        if (MissionManager.instance != null)
        {
            MissionManager.instance.OnAvailableMissionsUpdated -= PopulateMissionList;
            MissionManager.instance.OnMissionSelected -= OnMissionSelectedByHost;
            MissionManager.instance.OnMissionAcceptedStatusChanged -= OnMissionAcceptedStatusChanged;
        }
    }

    // 미션 UI 열기
    public void OpenUI()
    {
        uiPanel.SetActive(true); // UI 패널 활성화
        MissionManager.instance.SetPanelState(true); // MissionManager에 패널이 열렸음을 알림

        // 현재 이용 가능한 미션 목록으로 UI를 채우고, 호스트가 선택한 미션을 표시
        PopulateMissionList(MissionManager.instance.GetAvailableMissions());
        OnMissionSelectedByHost(MissionManager.instance.CurrentMission);
    }

    // 미션 UI 닫기
    public void CloseUI()
    {
        uiPanel.SetActive(false); // UI 패널 비활성화
        MissionManager.instance.SetPanelState(false); // MissionManager에 패널이 닫혔음을 알림

        missionComputer.RequestExitComputerServerRpc(); // 서버에 컴퓨터 사용 종료를 요청
    }

    // 전달받은 미션 목록으로 UI를 채움
    private void PopulateMissionList(List<Mission> missions)
    {
        if (mailCountText != null)
        {
            mailCountText.text = $"{missions.Count}";
        }
        
        // 기존에 생성된 미션 목록 UI 제거
        foreach (var entry in missionEntries)
        {
            Destroy(entry);
        }
        missionEntries.Clear();

        // 새로운 미션 목록으로 UI 생성
        foreach (var mission in missions)
        {
            GameObject entryObj = Instantiate(missionEmailPrefab, missionListContent);
            MissionEmailEntry emailEntry = entryObj.GetComponent<MissionEmailEntry>();
            emailEntry.Setup(mission, this);
            missionEntries.Add(entryObj);
        }
    }

    // 특정 미션의 상세 정보를 UI에 표시
    public void ShowMissionDetails(Mission mission)
    {
        selectedMission = mission;

        // mission이 null이면 (선택 해제 등) 상세 정보 패널을 숨김
        if (mission == null)
        {
            detailPanel.SetActive(false);
            return;
        }

        // 선택된 미션의 정보로 상세 정보 패널을 채우고 활성화
        detailPanel.SetActive(true);
        detailSummary.text = mission.MissionSummary;

        // 기존에 생성된 목표 목록 UI 제거
        foreach (var entry in missionObjectiveEntries)
        {
            Destroy(entry);
        }
        missionObjectiveEntries.Clear();

        // 새로운 목표 목록으로 UI 생성
        foreach (string detail in mission.MissionDetails)
        {
            GameObject entryObj = Instantiate(missionObjectivePrefab, objectivesContent);
            entryObj.GetComponent<MissionObjectiveEntry>().Setup(detail);
            missionObjectiveEntries.Add(entryObj);
        }

        // 버튼 상태 업데이트
        UpdateMissionDetailButtons();
    }
    
    // 미션 상세 정보 패널의 버튼들 (수락, 취소) 상태를 업데이트
    private void UpdateMissionDetailButtons()
    {
        // 기본적으로 모든 버튼을 숨김
        acceptButton.gameObject.SetActive(false);
        cancelAcceptanceButton.gameObject.SetActive(false);
        
        // 버튼은 호스트에게만 보임
        if (!NetworkManager.Singleton.IsHost) return;
        
        bool isMissionAccepted = MissionManager.instance.IsMissionAccepted;

        if (isMissionAccepted)
        {
            // 미션이 수락된 상태일 경우, 현재 보고 있는 미션이 수락된 미션인지 확인
            if (selectedMission != null && selectedMission.MissionId == MissionManager.instance.CurrentMission.MissionId)
            {
                // 맞다면, "수락 취소" 버튼을 표시
                cancelAcceptanceButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // 수락된 미션이 없고, 현재 선택된 미션이 있을 경우
            if (selectedMission != null)
            {
                // "수락" 버튼을 표시
                acceptButton.gameObject.SetActive(true);
            }
        }
    }

    // 호스트가 미션을 선택했을 때 호출되는 콜백 함수
    private void OnMissionSelectedByHost(Mission mission)
    {
        ShowMissionDetails(mission); // 선택된 미션의 상세 정보를 표시
        // 모든 미션 이메일 UI를 순회하며 선택된 미션에 하이라이트 처리
        foreach (var entryObj in missionEntries)
        {
            var emailEntry = entryObj.GetComponent<MissionEmailEntry>();
            bool isSelected = mission != null && emailEntry.Mission.MissionId == mission.MissionId;
            emailEntry.SetHighlight(isSelected);
        }
    }

    // 수락 버튼 클릭 시 호출
    private void OnAcceptButtonClicked()
    {
        // 호스트이고, 선택된 미션이 있을 경우에만 작동
        if (NetworkManager.Singleton.IsHost && selectedMission != null)
        {
            MissionManager.instance.AcceptMissionServerRpc(); // 서버에 미션 수락을 요청
        }
    }
    
    // 수락 취소 버튼 클릭 시 호출
    private void OnCancelAcceptanceButtonClicked()
    {
        // 호스트이고, 수락된 미션이 있을 경우에만 작동
        if (NetworkManager.Singleton.IsHost && MissionManager.instance.IsMissionAccepted)
        {
            MissionManager.instance.CancelMissionAcceptanceServerRpc(); // 서버에 미션 수락 취소를 요청
        }
    }

    // 미션 수락 상태가 변경되었을 때 호출되는 콜백 함수
    private void OnMissionAcceptedStatusChanged(bool isAccepted)
    {
        UpdateMissionDetailButtons(); // 버튼 상태 업데이트
        
        if (isAccepted)
        {
            // 미션이 수락되었을 때 채팅 메시지 출력
            ChatManager.instance?.AddMessage($"미션 '{MissionManager.instance.CurrentMission.MissionTitle}'이(가) 확정되었습니다.", MessageType.GlobalSystem);
        }
        else
        {
            // 미션 수락이 취소되었을 때 채팅 메시지 출력
            ChatManager.instance?.AddMessage($"미션 '{MissionManager.instance.CurrentMission.MissionTitle}'이(가) 취소되었습니다.", MessageType.GlobalSystem);
        }
    }
}