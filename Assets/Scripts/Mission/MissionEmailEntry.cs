// MissionEmailEntry.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class MissionEmailEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text missionTitleText;
    [SerializeField] private Image highlightImage; // 선택 시 하이라이트 될 이미지

    public Mission Mission { get; private set; }
    private MissionUI missionUI;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
        SetHighlight(false);
    }

    public void Setup(Mission mission, MissionUI ui)
    {
        this.Mission = mission;
        this.missionUI = ui;
        missionTitleText.text = mission.MissionTitle;
    }

    private void OnClicked()
    {
        // 모든 클라이언트는 상세보기 가능
        missionUI.ShowMissionDetails(Mission);
        
        // 방장만 미션 선택 요청 가능
        if (NetworkManager.Singleton.IsHost)
        {
            MissionManager.instance.SelectMissionServerRpc(Mission.MissionId);
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = isHighlighted;
        }
    }
}