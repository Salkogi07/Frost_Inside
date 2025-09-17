// MissionManager.cs
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class MissionManager : NetworkBehaviour
{
    public static MissionManager instance;

    [Header("Mission Database")]
    [SerializeField]
    private List<Mission> allMissions = new List<Mission>();

    // --- 네트워크 동기화 변수 ---
    private NetworkList<int> availableMissionIds; // 이용 가능한 미션 ID 목록
    private NetworkVariable<int> selectedMissionId = new NetworkVariable<int>(-1); // 선택된 미션 ID
    private NetworkVariable<bool> isMissionAccepted = new NetworkVariable<bool>(false); // 미션 수락 여부

    // --- 평판 (게임 오버 조건) ---
    private NetworkVariable<int> teamReputation = new NetworkVariable<int>(3); // 팀 평판

    // --- 이벤트 ---
    public event System.Action<List<Mission>> OnAvailableMissionsUpdated; // 이용 가능한 미션 목록 업데이트 시 호출
    public event System.Action<Mission> OnMissionSelected; // 미션 선택 시 호출
    public event System.Action<bool> OnMissionAcceptedStatusChanged; // 미션 수락 상태 변경 시 호출
    public event System.Action<int> OnReputationChanged; // 평판 변경 시 호출

    // --- 프로퍼티 ---
    public Mission CurrentMission => GetMissionById(selectedMissionId.Value); // 현재 선택된 미션
    public bool IsMissionAccepted => isMissionAccepted.Value; // 미션 수락 여부
    public int TeamReputation => teamReputation.Value; // 팀 평판
    public bool IsMissionPanelOpen { get; private set; } // 미션 패널이 열려있는지 여부

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            availableMissionIds = new NetworkList<int>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateRandomMissions(); // 서버인 경우, 무작위 미션 생성
        }

        // 네트워크 변수 값 변경 시 이벤트 구독
        availableMissionIds.OnListChanged += OnAvailableMissionsChanged;
        selectedMissionId.OnValueChanged += OnSelectedMissionIdChanged;
        isMissionAccepted.OnValueChanged += OnAcceptedStatusChanged;
        teamReputation.OnValueChanged += OnReputationValueChanged;

        // 씬 전환 후 UI가 현재 상태를 올바르게 반영하도록 초기 이벤트 호출
        if (availableMissionIds.Count > 0)
        {
            InvokeAvailableMissionsUpdate();
        }
        if (selectedMissionId.Value != -1)
        {
            OnMissionSelected?.Invoke(GetMissionById(selectedMissionId.Value));
        }
        OnMissionAcceptedStatusChanged?.Invoke(isMissionAccepted.Value);
        OnReputationChanged?.Invoke(teamReputation.Value);
    }

    public override void OnNetworkDespawn()
    {
        // 네트워크 변수 이벤트 구독 해제
        availableMissionIds.OnListChanged -= OnAvailableMissionsChanged;
        selectedMissionId.OnValueChanged -= OnSelectedMissionIdChanged;
        isMissionAccepted.OnValueChanged -= OnAcceptedStatusChanged;
        teamReputation.OnValueChanged -= OnReputationValueChanged;
    }

    private void OnAvailableMissionsChanged(NetworkListEvent<int> changeEvent)
    {
        InvokeAvailableMissionsUpdate();
    }

    // 이용 가능한 미션 목록 업데이트 이벤트를 호출하는 함수
    private void InvokeAvailableMissionsUpdate()
    {
        List<Mission> availableMissions = new List<Mission>();
        foreach (int id in availableMissionIds)
        {
            Mission mission = GetMissionById(id);
            if (mission != null)
            {
                availableMissions.Add(mission);
            }
        }
        OnAvailableMissionsUpdated?.Invoke(availableMissions);
    }

    private void OnSelectedMissionIdChanged(int previousValue, int newValue)
    {
        OnMissionSelected?.Invoke(GetMissionById(newValue));
    }

    private void OnAcceptedStatusChanged(bool previousValue, bool newValue)
    {
        OnMissionAcceptedStatusChanged?.Invoke(newValue);
    }

    private void OnReputationValueChanged(int previousValue, int newValue)
    {
        OnReputationChanged?.Invoke(newValue);
        if (newValue <= 0)
        {
            // 평판이 0 이하일 경우 게임 오버 로직 실행
            Debug.Log("팀 평판이 0이 되어 게임 오버입니다.");
            // 예시: GameManager.instance.GameOver();
        }
    }

    public void SetPanelState(bool isOpen)
    {
        IsMissionPanelOpen = isOpen;
    }

    /// <summary>
    /// (서버 전용) 로비에서 게임이 시작될 때 2~3개의 무작위 미션을 생성합니다.
    /// </summary>
    public void GenerateRandomMissions()
    {
        if (!IsServer) return;

        availableMissionIds.Clear();
        isMissionAccepted.Value = false;
        selectedMissionId.Value = -1;

        if (allMissions.Count == 0) return;

        List<Mission> shuffledMissions = allMissions.OrderBy(x => System.Guid.NewGuid()).ToList();
        int count = Mathf.Min(shuffledMissions.Count, Random.Range(2, 4));

        for (int i = 0; i < count; i++)
        {
            availableMissionIds.Add(shuffledMissions[i].MissionId);
        }
    }

    public Mission GetMissionById(int id)
    {
        return allMissions.FirstOrDefault(m => m.MissionId == id);
    }

    public List<Mission> GetAvailableMissions()
    {
        List<Mission> missions = new List<Mission>();
        foreach (var id in availableMissionIds)
        {
            var mission = GetMissionById(id);
            if(mission != null) missions.Add(mission);
        }
        return missions;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectMissionServerRpc(int missionId)
    {
        // 미션이 이미 수락된 상태에서는 선택을 변경할 수 없습니다.
        if (isMissionAccepted.Value) return;

        if (availableMissionIds.Contains(missionId))
        {
            selectedMissionId.Value = missionId;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AcceptMissionServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) return; // 호스트만 실행 가능
        if (selectedMissionId.Value != -1)
        {
            isMissionAccepted.Value = true;
            Debug.Log($"호스트가 미션 '{GetMissionById(selectedMissionId.Value).MissionTitle}'을(를) 수락했습니다.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CancelMissionAcceptanceServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) return; // 호스트만 실행 가능
        if (isMissionAccepted.Value)
        {
            isMissionAccepted.Value = false;
            Debug.Log($"호스트가 미션 '{CurrentMission.MissionTitle}'의 수락을 취소했습니다.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DecreaseReputationServerRpc()
    {
        if (teamReputation.Value > 0)
        {
            teamReputation.Value--;
        }
    }
}