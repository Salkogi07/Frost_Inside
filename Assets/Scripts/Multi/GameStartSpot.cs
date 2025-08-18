// GameStartSpot.cs (수정된 버전)
// 역할: 호스트가 게임 시작을 트리거할 수 있는 상호작용 지점을 담당합니다.

using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameStartSpot : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("게임을 시작하기 위해 키를 누르고 있어야 하는 시간(초)")]
    [SerializeField] private float requiredHoldTime = 2.0f;

    [Header("References")]
    [Tooltip("플레이어에게 보여줄 상호작용 안내 오브젝트 (예: 'Press E to Start')")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Tooltip("(선택 사항) 누르는 시간을 시각적으로 보여줄 UI Image. Fill Method가 Radial 360으로 설정되어야 합니다.")]
    [SerializeField] private Image fillImage;

    // --- 내부 변수 ---
    private bool isHostInRange = false;
    private float currentHoldTime = 0f;
    private bool isHolding = false;
    
    private void Awake()
    {
        // 시작 시 안내 문구 및 UI 초기화
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        if (fillImage != null)
        {
            fillImage.gameObject.SetActive(false);
            fillImage.fillAmount = 0;
        }
    }

    private void Update()
    {
        // 로컬 플레이어가 호스트이고 범위 내에 있을 때만 로직을 처리합니다.
        if (!isHostInRange)
        {
            return;
        }

        KeyCode interactionKey = KeyManager.instance.GetKeyCodeByName("Interaction");

        // 키를 누르기 시작했을 때
        if (Input.GetKeyDown(interactionKey))
        {
            // 모든 플레이어가 준비되었는지 먼저 확인합니다.
            if (!PlayerDataManager.instance.AreAllPlayersReady())
            {
                ChatManager.instance?.AddMessage("Not all players are ready.", MessageType.PersonalSystem);
                return; // 준비되지 않았으면 홀드를 시작하지 않습니다.
            }

            isHolding = true;
            if (fillImage != null)
            {
                fillImage.gameObject.SetActive(true);
            }
        }

        // 키를 떼었을 때
        if (Input.GetKeyUp(interactionKey))
        {
            ResetHoldState();
        }

        // 키를 누르고 있는 동안
        if (isHolding)
        {
            currentHoldTime += Time.deltaTime;

            if (fillImage != null)
            {
                fillImage.fillAmount = currentHoldTime / requiredHoldTime;
            }

            // 필요한 시간을 모두 채웠다면
            if (currentHoldTime >= requiredHoldTime)
            {
                TriggerGameStart();
                ResetHoldState();
            }
        }
    }

    private void TriggerGameStart()
    {
        // 다시 한번 확인: 내가 호스트이고 모든 플레이어가 준비되었는가?
        if (NetworkManager.Singleton.IsHost && PlayerDataManager.instance.AreAllPlayersReady())
        {
            Debug.Log("Host is starting the game...");
            NetworkTransmission.instance.RequestStartGameServerRpc();
        }
    }

    private void ResetHoldState()
    {
        isHolding = false;
        currentHoldTime = 0f;
        if (fillImage != null)
        {
            fillImage.fillAmount = 0;
            fillImage.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 플레이어가 로컬 플레이어(IsOwner)이며, 현재 게임 세션이 호스트(IsHost)일 경우에만 상호작용을 활성화합니다.
        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsOwner && NetworkManager.Singleton.IsHost)
        {
            isHostInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 나간 플레이어가 로컬 플레이어(IsOwner)이며, 현재 게임 세션이 호스트(IsHost)일 경우 상호작용을 비활성화합니다.
        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsOwner && NetworkManager.Singleton.IsHost)
        {
            isHostInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            // 범위를 벗어나면 강제로 리셋
            ResetHoldState();
        }
    }
}