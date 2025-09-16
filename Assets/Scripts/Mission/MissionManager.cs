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

    // 네트워크 동기화 변수
    private NetworkList<int> availableMissionIds;
    private NetworkVariable<int> selectedMissionId = new NetworkVariable<int>(-1);
    private NetworkVariable<bool> isMissionAccepted = new NetworkVariable<bool>(false);
    
    // 평판 (게임오버 체력)
    private NetworkVariable<int> teamReputation = new NetworkVariable<int>(3);

    // 이벤트
    public event System.Action<List<Mission>> OnAvailableMissionsUpdated;
    public event System.Action<Mission> OnMissionSelected;
    public event System.Action<bool> OnMissionAcceptedStatusChanged;
    public event System.Action<int> OnReputationChanged;

    public Mission CurrentMission => GetMissionById(selectedMissionId.Value);
    public bool IsMissionAccepted => isMissionAccepted.Value;
    public int TeamReputation => teamReputation.Value;
    
    public bool IsMissionPanelOpen { get; private set; }

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
        GenerateRandomMissions();
        
        availableMissionIds.OnListChanged += OnAvailableMissionsChanged;
        selectedMissionId.OnValueChanged += OnSelectedMissionIdChanged;
        isMissionAccepted.OnValueChanged += OnAcceptedStatusChanged;
        teamReputation.OnValueChanged += OnReputationValueChanged;

        // 씬 전환 후에도 UI가 최신 상태를 반영할 수 있도록 초기 이벤트 호출
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
        availableMissionIds.OnListChanged -= OnAvailableMissionsChanged;
        selectedMissionId.OnValueChanged -= OnSelectedMissionIdChanged;
        isMissionAccepted.OnValueChanged -= OnAcceptedStatusChanged;
        teamReputation.OnValueChanged -= OnReputationValueChanged;
    }

    private void OnAvailableMissionsChanged(NetworkListEvent<int> changeEvent)
    {
        InvokeAvailableMissionsUpdate();
    }

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
            // 평판이 0 이하면 게임오버 로직 실행
            Debug.Log("Team reputation is 0. Game Over.");
            // ex: GameManager.instance.GameOver();
        }
    }
    
    public void SetPanelState(bool isOpen)
    {
        IsMissionPanelOpen = isOpen;
    }

    /// <summary>
    /// (서버 전용) 로비에서 게임 시작 시 2~3개의 미션을 랜덤으로 생성합니다.
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
        // 이미 수락된 미션은 변경 불가
        if (isMissionAccepted.Value) return;

        bool isValidId = false;
        foreach (int id in availableMissionIds)
        {
            if (id == missionId)
            {
                isValidId = true;
                break;
            }
        }
        if (isValidId)
        {
            selectedMissionId.Value = missionId;
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void DeselectMissionServerRpc()
    {
        if (!isMissionAccepted.Value)
        {
            selectedMissionId.Value = -1;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AcceptMissionServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) return; // 방장만 실행 가능
        if (selectedMissionId.Value != -1)
        {
            isMissionAccepted.Value = true;
            Debug.Log($"Mission '{GetMissionById(selectedMissionId.Value).MissionTitle}' has been accepted by the host.");
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