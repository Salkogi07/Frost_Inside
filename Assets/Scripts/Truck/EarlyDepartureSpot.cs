// EarlyDepartureSpot.cs
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class EarlyDepartureSpot : MonoBehaviour
{
    [SerializeField] private float requiredHoldTime = 3.0f;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Image fillImage;

    private bool isPlayerInRange = false;
    private float currentHoldTime = 0f;
    private bool isHolding = false;

    private void Awake()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (fillImage != null)
        {
            fillImage.gameObject.SetActive(false);
            fillImage.fillAmount = 0;
        }
    }

    private void Update()
    {
        if (!isPlayerInRange) return;
        
        // `KeyManager`가 있다면 사용하고, 없다면 KeyCode.E 등으로 직접 지정
        KeyCode interactionKey = KeyManager.instance.GetKeyCodeByName("Interaction");

        if (Input.GetKeyDown(interactionKey))
        {
            isHolding = true;
            if (fillImage != null) fillImage.gameObject.SetActive(true);
        }

        if (Input.GetKeyUp(interactionKey))
        {
            ResetHoldState();
        }

        if (isHolding)
        {
            currentHoldTime += Time.deltaTime;
            if (fillImage != null) fillImage.fillAmount = currentHoldTime / requiredHoldTime;

            if (currentHoldTime >= requiredHoldTime)
            {
                TriggerEarlyDeparture();
                ResetHoldState();
            }
        }
    }
    
    private void TriggerEarlyDeparture()
    {
        // 서버에게 출발 시퀀스를 시작하도록 요청합니다.
        RequestDepartureServerRpc();
    }
    
    // 클라이언트가 서버에게 출발을 요청하기 위해 이 함수를 호출합니다.
    // RequireOwnership = false로 설정하여 이 오브젝트의 소유자가 아니더라도 RPC를 보낼 수 있습니다.
    [ServerRpc(RequireOwnership = false)]
    private void RequestDepartureServerRpc()
    {
        Debug.Log("Server received a request for early departure.");
        // 서버에서만 CruiserController의 출발 시퀀스를 실행합니다.
        CruiserController.instance?.StartDepartureSequence();
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

    // 호스트인 로컬 플레이어만 상호작용 가능
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsOwner)
        {
            isPlayerInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsOwner)
        {
            isPlayerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            ResetHoldState();
        }
    }
}