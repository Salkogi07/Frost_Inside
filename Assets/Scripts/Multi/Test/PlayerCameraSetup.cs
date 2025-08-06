// --- START OF NEW FILE PlayerCameraSetup.cs ---

using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine;

// 이 스크립트는 플레이어 프리팹에 추가되어야 합니다.
public class PlayerCameraSetup : NetworkBehaviour
{
    private CinemachineCamera _camera;

    public override void OnNetworkSpawn()
    {
        // 이 스크립트는 모든 클라이언트에서 실행되지만,
        // 아래 코드는 오직 이 오브젝트의 "소유자" 클라이언트에서만 실행됩니다.
        if (IsOwner)
        {
            // 씬에 있는 Cinemachine 카메라를 찾습니다.
            // 태그를 사용하는 것이 간단하고 효율적입니다.
            GameObject camObj = GameObject.FindGameObjectWithTag("CinemachineCamera");
            if (camObj != null)
            {
                _camera = camObj.GetComponent<CinemachineCamera>();
                if (_camera != null)
                {
                    // 찾은 카메라가 자신의 캐릭터를 따라다니도록 설정합니다.
                    _camera.Follow = transform;
                    _camera.LookAt = transform;
                    Debug.Log("Cinemachine camera target set for local player.");
                }
                else
                {
                    Debug.LogError("Object with tag 'CinemachineCamera' does not have a CinemachineCamera component.");
                }
            }
            else
            {
                Debug.LogError("Could not find GameObject with tag 'CinemachineCamera'. Make sure your scene camera has this tag.");
            }
        }
    }
}