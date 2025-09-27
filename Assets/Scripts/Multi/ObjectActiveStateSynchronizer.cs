using Unity.Netcode;
using UnityEngine;

public class ObjectActiveStateSynchronizer : NetworkBehaviour
{
    public GameObject target;
    
    private readonly NetworkVariable<bool> _networkIsActive = new NetworkVariable<bool>(
        true, // 기본값은 활성화 상태로 시작
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        _networkIsActive.OnValueChanged += OnActiveStateChanged;
        
        OnActiveStateChanged(false, _networkIsActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        _networkIsActive.OnValueChanged -= OnActiveStateChanged;
    }
    
    private void OnActiveStateChanged(bool previousValue, bool newValue)
    {
        // 현재 게임 오브젝트의 활성화 상태가 네트워크로 받은 새로운 상태와 다를 경우에만 적용합니다.
        if (target.activeSelf != newValue)
        {
            target.SetActive(newValue);
        }
    }
    
    public void SetActiveState(bool isActive)
    {
        if (!IsServer) return;

        _networkIsActive.Value = isActive;
    }
}