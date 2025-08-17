using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CharacterChangeSpot : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("이 지점에서 변경될 캐릭터의 ID (PlayerSpawner의 프리팹 배열 인덱스와 일치)")]
    [SerializeField] private int targetCharacterId = 0;
    
    [Tooltip("캐릭터를 변경하기 위해 키를 누르고 있어야 하는 시간(초)")]
    [SerializeField] private float requiredHoldTime = 1.5f;

    [Header("References")]
    [Tooltip("플레이어에게 보여줄 상호작용 안내 오브젝트 (예: 'Press E' 텍스트 또는 아이콘)")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Tooltip("(선택 사항) 누르는 시간을 시각적으로 보여줄 UI Image. Fill Method가 Radial 360으로 설정되어야 합니다.")]
    [SerializeField] private Image fillImage;

    // --- 내부 변수 ---
    private bool isLocalPlayerInRange = false;
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
        // 로컬 플레이어가 범위 내에 있을 때만 상호작용 로직을 처리
        if (!isLocalPlayerInRange)
        {
            return;
        }

        KeyCode interactionKey = KeyManager.instance.GetKeyCodeByName("Interaction");

        // 키를 누르기 시작했을 때
        if (Input.GetKeyDown(interactionKey))
        {
            isHolding = true;
            if (fillImage != null)
            {
                fillImage.gameObject.SetActive(true);
            }
        }

        // 키를 떼었을 때
        if (Input.GetKeyUp(interactionKey))
        {
            // 누르고 있던 상태를 리셋
            ResetHoldState();
        }

        // 키를 누르고 있는 동안
        if (isHolding)
        {
            currentHoldTime += Time.deltaTime;

            // 시각적 피드백 업데이트
            if (fillImage != null)
            {
                fillImage.fillAmount = currentHoldTime / requiredHoldTime;
            }

            // 필요한 시간을 모두 채웠다면
            if (currentHoldTime >= requiredHoldTime)
            {
                TriggerCharacterChange();
                // 성공적으로 변경했으므로 상태 리셋
                ResetHoldState();
            }
        }
    }

    private void TriggerCharacterChange()
    {
        var myInfo = PlayerDataManager.instance.MyInfo;

        // 이미 해당 캐릭터를 선택한 상태가 아니라면 서버에 변경 요청
        if (myInfo != null && myInfo.SelectedCharacterId != targetCharacterId)
        {
            NetworkTransmission.instance.RequestCharacterChangeServerRpc(targetCharacterId);
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
        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsOwner)
        {
            isLocalPlayerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<NetworkObject>(out var networkObject) && networkObject.IsOwner)
        {
            isLocalPlayerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            // 범위를 벗어나면 키를 누르고 있던 상태를 강제로 리셋
            ResetHoldState();
        }
    }
}