// ResultsScreenUIManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResultsScreenUIManager : MonoBehaviour
{
    public static ResultsScreenUIManager instance;

    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform playerResultsContainer; // 플레이어 결과 항목이 생성될 부모 Transform
    [SerializeField] private GameObject playerResultPrefab; // 이름과 상태 텍스트를 가진 UI 프리팹
    
    [Header("Mission Result")]
    [SerializeField] private TMP_Text missionResultText;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failureColor = Color.red;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        resultsPanel.SetActive(false);
    }
    
    private void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    /// <summary>
    /// 결산 정보를 받아와 화면에 표시합니다.
    /// </summary>
    public void Show(PlayerResultInfo[] results)
    {
        // 이전 결과 삭제
        foreach (Transform child in playerResultsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 새로운 결과 채우기
        foreach (var result in results)
        {
            GameObject resultObj = Instantiate(playerResultPrefab, playerResultsContainer);
            var texts = resultObj.GetComponentsInChildren<TMP_Text>(); // 프리팹 구조에 맞게 수정 필요
            texts[0].text = result.SteamName.ToString();
            texts[1].text = result.Status.ToString();
        }
        
        UpdateMissionResultUI();
        
        nextButton.interactable = true;
        resultsPanel.SetActive(true);
    }
    
    private void UpdateMissionResultUI()
    {
        if (missionResultText == null) return;

        MissionOutcome outcome = MissionManager.instance.FinalMissionOutcome.Value;

        switch (outcome)
        {
            case MissionOutcome.Success:
                missionResultText.gameObject.SetActive(true);
                missionResultText.text = "MISSION SUCCESSFUL";
                missionResultText.color = successColor;
                break;
            case MissionOutcome.Failure:
                missionResultText.gameObject.SetActive(true);
                missionResultText.text = "MISSION FAILED";
                missionResultText.color = failureColor;
                break;
            case MissionOutcome.None:
            default:
                missionResultText.gameObject.SetActive(false);
                break;
        }
    }

    private void OnNextButtonClicked()
    {
        nextButton.interactable = false; // 중복 클릭 방지
        ChatManager.instance?.AddMessage("Waiting for other players...", MessageType.PersonalSystem);
        NetworkTransmission.instance.NotifyReadyForNextSceneServerRpc();
    }
}