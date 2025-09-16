// MissionComputer.cs

using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MissionComputer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private MissionUI missionUI; // 미션 UI 스크립트 참조

    private NetworkVariable<bool> isBeingUsed = new NetworkVariable<bool>(false);
    private HashSet<ulong> playersInRange = new HashSet<ulong>();

    private void Awake()
    {
        interactionPrompt.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        isBeingUsed.OnValueChanged += OnUsageStateChanged;
    }

    private void Update()
    {
        if (playersInRange.Contains(NetworkManager.Singleton.LocalClientId))
        {
            if (Input.GetKeyDown(KeyManager.instance.GetKeyCodeByName("Interaction")))
            {
                if (isBeingUsed.Value)
                {
                    ChatManager.instance?.AddMessage("Another user is currently using the terminal.", MessageType.PersonalSystem);
                }
                else
                {
                    RequestUseComputerServerRpc();
                }
            }
        }
    }

    private void OnUsageStateChanged(bool previousValue, bool newValue)
    {
        // UI 상태 업데이트 (예: 컴퓨터 화면에 "사용 중" 표시)
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUseComputerServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!isBeingUsed.Value)
        {
            isBeingUsed.Value = true;
            OpenMissionUIClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } } });
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestExitComputerServerRpc()
    {
        isBeingUsed.Value = false;
    }

    [ClientRpc]
    private void OpenMissionUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        missionUI.OpenUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsOwner)
        {
            playersInRange.Add(networkObject.OwnerClientId);
            interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsOwner)
        {
            playersInRange.Remove(networkObject.OwnerClientId);
            interactionPrompt.SetActive(false);
        }
    }
}