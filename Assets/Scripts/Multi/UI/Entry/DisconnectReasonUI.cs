using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DisconnectReasonUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text disconnectReasonText;
    [SerializeField] private GameObject disconnectReasonPanel;
    [SerializeField] private Button disconnectReasonButton;

    void Start()
    {
        disconnectReasonButton.onClick.AddListener(() => disconnectReasonPanel.gameObject.SetActive(false));
        if (disconnectReasonText == null)
        {
            Debug.LogWarning("Disconnect Reason Text가 할당되지 않았습니다.");
            return;
        }
        
        if (!string.IsNullOrEmpty(GameNetworkManager.DisconnectReason))
        {
            disconnectReasonPanel.gameObject.SetActive(true);
            disconnectReasonText.text = GameNetworkManager.DisconnectReason;
            
            GameNetworkManager.SetDisconnectReason("");
        }
        else
        {
            disconnectReasonPanel.gameObject.SetActive(false);
        }
    }
}