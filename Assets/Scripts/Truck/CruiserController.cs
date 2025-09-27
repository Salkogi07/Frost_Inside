// CruiserController.cs

using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CruiserController : NetworkBehaviour
{
    public static CruiserController instance;
    
    [SerializeField] private Transform crewPos;
    [SerializeField] private SnowCrusier_Open Anim;
    
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float travelDistance = 200f; // 이 거리만큼 이동하면 결산 화면 표시

    [Header("Departure Check")]
    [Tooltip("출발 시 플레이어 존재 여부를 확인할 영역의 중심 위치입니다.")]
    [SerializeField] private Transform checkZoneCenter;
    [Tooltip("플레이어 존재 여부를 확인할 영역의 크기입니다.")]
    [SerializeField] private Vector2 checkZoneSize = new Vector2(10, 5);
    [Tooltip("플레이어 오브젝트에 할당된 레이어입니다.")]
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Mission Check")]
    [Tooltip("미션 아이템을 확인할 영역의 중심 위치입니다.")]
    [SerializeField] private Transform itemCheckZoneCenter;
    [Tooltip("미션 아이템을 확인할 영역의 크기입니다.")]
    [SerializeField] private Vector2 itemCheckZoneSize = new Vector2(10, 5);
    [Tooltip("아이템 오브젝트에 할당된 레이어입니다.")]
    [SerializeField] private LayerMask itemLayer;
    
    [Header("References")]
    [Tooltip("플레이어가 출입을 감지하는 트리거 콜라이더")]
    [SerializeField] private Collider2D[] allEntrances;
    
    // --- 네트워크 동기화 변수 ---
    private NetworkVariable<Vector2> _networkPosition = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<CruiserState> _networkState = new(CruiserState.Docked, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public CruiserState CurrentState => _networkState.Value;
    
    // --- 클라이언트 보간용 변수 ---
    private Vector2 _lerpStartPos;
    private Vector2 _lerpTargetPos;
    private float _lerpTime;
    private readonly float lerpDuration = 0.1f;

    // --- 서버 전용 변수 ---
    private Vector2 _startPosition;
    private readonly HashSet<ulong> playersInside = new HashSet<ulong>();

    public enum CruiserState { Docked, Moving, Finished }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _startPosition = transform.position;
            _networkPosition.Value = _startPosition;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            ServerUpdate();
        }
        else
        {
            ClientUpdate();
        }
    }

    private void ServerUpdate()
    {
        // 움직이는 상태일 때만 위치 업데이트
        if (_networkState.Value == CruiserState.Moving)
        {
            Anim.Move();
            transform.position += Vector3.right * moveSpeed * Time.deltaTime; // 예시: 오른쪽으로 이동
            _networkPosition.Value = transform.position;

            // 목표 이동 거리에 도달하면 게임 종료
            if (Vector2.Distance(_startPosition, transform.position) >= travelDistance)
            {
                _networkState.Value = CruiserState.Finished;
                NetworkTransmission.instance.EndGameAndShowResultsServerRpc();
            }
        }
    }

    private void ClientUpdate()
    {
        // Enemy.cs와 동일한 클라이언트 위치 보간 로직
        if (_lerpTargetPos != _networkPosition.Value)
        {
            _lerpStartPos = transform.position;
            _lerpTargetPos = _networkPosition.Value;
            _lerpTime = 0;
        }

        if (_lerpTime < lerpDuration)
        {
            _lerpTime += Time.deltaTime;
            transform.position = Vector2.Lerp(_lerpStartPos, _lerpTargetPos, _lerpTime / lerpDuration);
        }
        else
        {
            transform.position = _lerpTargetPos;
        }
    }

    /// <summary>
    /// (서버 전용) 크루저 출발 시퀀스를 시작합니다.
    /// </summary>
    public void StartDepartureSequence()
    {
        if (!IsServer || _networkState.Value != CruiserState.Docked) return;

        Debug.Log("[Server] Cruiser departure sequence initiated!");
        
        // --- START: OverlapBox를 이용한 플레이어 확인 로직 (요청에 따라 수정됨) ---

        // 1. 기존 '내부 플레이어' 목록을 초기화합니다.
        playersInside.Clear();

        // 2. 지정된 영역(checkZone) 내의 모든 플레이어 콜라이더를 감지합니다.
        if (checkZoneCenter == null)
        {
            Debug.LogError("[Server] 'Check Zone Center'가 할당되지 않았습니다! 플레이어 확인을 스킵합니다.");
        }
        else
        {
            Collider2D[] playersInZone = Physics2D.OverlapBoxAll(checkZoneCenter.position, checkZoneSize, 0f, playerLayer);
            Debug.Log($"[Server] OverlapBox found {playersInZone.Length} player colliders in the zone.");

            foreach (var playerCollider in playersInZone)
            {
                if (playerCollider.TryGetComponent<NetworkObject>(out var networkObject))
                {
                    playersInside.Add(networkObject.OwnerClientId);
                    Debug.Log($"[Server] Player {networkObject.OwnerClientId} is inside the cruiser at departure.");
                }
            }
        }
        
        CheckMissionItems();
        
        // --- END: OverlapBox 로직 ---

        // 3. 최종 플레이어 상태 목록 생성
        var results = new List<PlayerResultInfo>();
        var allPlayers = PlayerDataManager.instance.GetAllPlayers();

        foreach (var player in allPlayers)
        {
            PlayerStatus status;
        
            // 1순위: 사망 여부 확인
            if (player.IsDead)
            {
                status = PlayerStatus.Deceased;
            }
            // 2순위: 생존 여부 (크루저 내부에 있는지) 확인
            else if (playersInside.Contains(player.ClientId))
            {
                status = PlayerStatus.Survived;
            }
            // 3순위: 그 외 (살아있지만 밖에 있는 경우)
            else
            {
                status = PlayerStatus.Missing;
            }
        
            results.Add(new PlayerResultInfo { ClientId = player.ClientId, SteamName = player.SteamName, Status = status });
        }

        // 4. NetworkTransmission에 최종 결과 전달
        NetworkTransmission.instance.SetFinalPlayerResults(results.ToArray());
        
        // 5. 내부에 있는 플레이어들의 ID 목록을 만듭니다.
        ulong[] playersInsideIds = new ulong[playersInside.Count];
        playersInside.CopyTo(playersInsideIds);
        Debug.Log($"[Server] {playersInsideIds.Length} players inside the cruiser.");

        // 6. 모든 클라이언트에게 출발 절차를 시작하도록 알립니다. (입구 잠금, 카메라 전환 등)
        DepartingClientRpc(playersInsideIds);

        // 7. 상태를 '이동 중'으로 변경하여 움직임 시작
        _networkState.Value = CruiserState.Moving;
    }
    
    /// <summary>
    /// (서버 전용) 크루저 내부의 아이템을 확인하여 미션 성공/실패 여부를 결정합니다.
    /// </summary>
    private void CheckMissionItems()
    {
        Mission currentMission = MissionManager.instance.CurrentMission;

        // 수집 미션이 아니거나, 수락된 미션이 없으면 검사를 중단합니다.
        if (currentMission == null || currentMission.ObjectiveType != MissionObjectiveType.Collect)
        {
            MissionManager.instance.SetFinalMissionOutcomeServerRpc(MissionOutcome.None);
            return;
        }

        // 1. 크루저 내부의 모든 아이템을 수집합니다.
        var itemsInCruiser = new Dictionary<int, int>();
        if (itemCheckZoneCenter != null)
        {
            Collider2D[] itemsInZone = Physics2D.OverlapBoxAll(itemCheckZoneCenter.position, itemCheckZoneSize, 0f, itemLayer);
            Debug.Log($"[Server] OverlapBox found {itemsInZone.Length} item colliders in the zone.");
            
            foreach (var itemCollider in itemsInZone)
            {
                if (itemCollider.TryGetComponent<ItemObject>(out var itemObject))
                {
                    Inventory_Item itemData = itemObject.GetItemData();
                    if (!itemData.IsEmpty())
                    {
                        if (itemsInCruiser.ContainsKey(itemData.itemId))
                        {
                            itemsInCruiser[itemData.itemId]++;
                        }
                        else
                        {
                            itemsInCruiser[itemData.itemId] = 1;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("[Server] 'Item Check Zone Center'가 할당되지 않았습니다! 아이템 확인을 스킵합니다.");
        }

        // 2. 미션 요구사항과 비교합니다.
        bool isSuccess = true;
        foreach (var requirement in currentMission.CollectionRequirements)
        {
            // 요구 아이템이 없거나, 수량이 부족하면 실패 처리
            if (!itemsInCruiser.ContainsKey(requirement.requiredItemId) || itemsInCruiser[requirement.requiredItemId] < requirement.amount)
            {
                isSuccess = false;
                Debug.Log($"[Server] Mission Failed: Item ID {requirement.requiredItemId} required {requirement.amount}, but found {(itemsInCruiser.ContainsKey(requirement.requiredItemId) ? itemsInCruiser[requirement.requiredItemId] : 0)}.");
                break;
            }
        }

        // 3. 최종 결과를 설정합니다.
        if (isSuccess)
        {
            MissionManager.instance.SetFinalMissionOutcomeServerRpc(MissionOutcome.Success);
        }
        else
        {
            MissionManager.instance.SetFinalMissionOutcomeServerRpc(MissionOutcome.Failure);
            // 실패 시 평판 감소
            MissionManager.instance.DecreaseReputationServerRpc();
        }
    }
    
    [ClientRpc]
    private void DepartingClientRpc(ulong[] playersInsideIds)
    {
        Anim.Close();
        
        // 1. 모든 입구를 잠급니다. (1단계에서 수정 완료)
        if (allEntrances != null)
        {
            foreach (var entrance in allEntrances)
            {
                entrance.enabled = false;
            }
        }
        ChatManager.instance?.AddMessage("The cruiser has departed! The entrance is now closed.", MessageType.GlobalSystem);

        // 2. 내가 크루저 안에 있는지 확인합니다.
        bool amIInside = false;
        foreach (ulong id in playersInsideIds)
        {
            if (id == NetworkManager.Singleton.LocalClientId)
            {
                amIInside = true;
                break;
            }
        }

        Debug.Log($"[Client] DepartingClientRpc received. My ClientId is {NetworkManager.Singleton.LocalClientId}. Am I inside? {amIInside}");

        // 3. 만약 내가 안에 있다면, 카메라 전환을 실행합니다.
        if (amIInside)
        {
            Debug.Log("[Client] I am inside. Attempting to switch camera...");

            var camObj = GameObject.FindGameObjectWithTag("CinemachineCamera");
            if (camObj == null)
            {
                // 이 로그가 출력된다면 태그가 잘못되었거나 오브젝트가 비활성화된 것입니다.
                Debug.LogError("[Client] FATAL: Could not find object with tag 'CinemachineCamera'!");
                return;
            }
        
            Debug.Log($"[Client] Found camera object: {camObj.name}");

            StartCoroutine(FollowCamera(camObj));
            
            Debug.Log("[Client] SUCCESS: Camera target switched to Cruiser.");
        }
    }
    
    private IEnumerator FollowCamera(GameObject camObj)
    {
        // 씬에서 시네머신 카메라를 한 번만 찾아 캐싱해둡니다.
        if (camObj != null)
        {
            CinemachinePositionComposer _cinemachineComposer = camObj.GetComponent<CinemachinePositionComposer>();
            CinemachineCamera vcam = camObj.GetComponent<CinemachineCamera>();
            if (_cinemachineComposer != null)
            {
                // 카메라의 Damping(지연 효과)을 꺼서 즉시 플레이어를 따라가게 합니다.
                _cinemachineComposer.Damping = Vector3.zero;
                vcam.Follow = crewPos;
                
                // 텔레포트가 반영될 시간을 잠시 기다립니다.
                yield return new WaitForSeconds(0.1f);
                
                // Damping을 원래 값으로 복원합니다.
                _cinemachineComposer.Damping = Vector3.one;
            }
        }
    }
    
    // --- 이동 중인 크루저와 충돌한 개체 처리 (서버에서만 실행) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer || _networkState.Value != CruiserState.Moving) return;

        // 충돌한 객체가 'Entity' 컴포넌트를 가지고 있는지 확인
        if (collision.gameObject.TryGetComponent<Entity>(out var entity))
        {
            // TODO: 여기에 데미지를 주는 코드를 삽입하세요.
            // 예를 들어, entity.TakeDamage(9999); 또는 entity.Die(); 와 같은 메소드를 호출합니다.
            Debug.Log($"[Server] Cruiser collided with and killed {entity.name}");
        }
    }

    // --- 기즈모(Gizmo)를 사용하여 에디터에서 확인 영역을 시각적으로 표시 ---
    private void OnDrawGizmosSelected()
    {
        // 플레이어 확인 영역 Gizmo
        if (checkZoneCenter != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawCube(checkZoneCenter.position, checkZoneSize);
        }
        
        // 아이템 확인 영역 Gizmo
        if (itemCheckZoneCenter != null)
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f); // 파란색
            Gizmos.DrawCube(itemCheckZoneCenter.position, itemCheckZoneSize);
        }
    }
}