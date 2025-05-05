using Unity.Cinemachine;
using UnityEngine;

public class CammeraSetting : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cam;

    void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform != null)
        {
            cam.Follow = playerTransform;
            cam.LookAt = playerTransform;
        }
        else
        {
            Debug.LogWarning("Player를 찾을 수 없습니다. 태그 확인 필요.");
        }
    }
}
