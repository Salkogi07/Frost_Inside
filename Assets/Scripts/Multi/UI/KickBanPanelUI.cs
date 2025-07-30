// KickBanPanel.cs
// 역할: 킥/밴 확인 패널의 UI와 로직을 관리합니다.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KickBanPanelUI : MonoBehaviour
{
    public static KickBanPanelUI instance;

    [SerializeField] private GameObject panelObject;
    [SerializeField] private TMP_Text targetPlayerNameText;
    [SerializeField] private Toggle banToggle;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private ulong targetClientId;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        panelObject.SetActive(false);
    }

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    /// <summary>
    /// 패널을 열고 대상 플레이어 정보를 설정합니다.
    /// </summary>
    public void OpenPanel(ulong clientId, string playerName)
    {
        targetClientId = clientId;
        targetPlayerNameText.text = $"- Target: {playerName} -";
        
        // 기본값으로 킥을 선택
        banToggle.isOn = false;
        
        panelObject.SetActive(true);
    }

    private void OnConfirm()
    {
        bool shouldBan = banToggle.isOn;
        
        Debug.Log($"Requesting to kick Client ID: {targetClientId}, Ban: {shouldBan}");
        NetworkTransmission.instance.RequestKickPlayerServerRpc(targetClientId, shouldBan);
        
        panelObject.SetActive(false);
    }

    private void OnCancel()
    {
        panelObject.SetActive(false);
    }

    private void OnDestroy()
    {
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
    }
}