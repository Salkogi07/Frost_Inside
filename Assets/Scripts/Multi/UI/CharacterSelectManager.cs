using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    [SerializeField] private CharacterSelectButtonUI[] characterButtons;

    private void Start()
    {
        foreach (var buttonUI in characterButtons)
        {
            Button button = buttonUI.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnCharacterSelected(buttonUI.CharacterId));
            }
        }
        
        // 예시: UpdateSelectionUI(PlayerDataManager.instance.MyInfo.SelectedCharacterId);
    }
    
    private void OnCharacterSelected(int characterId)
    {
        if (PlayerDataManager.instance.MyInfo.SelectedCharacterId == characterId)
            return;
        
        ChatManager.instance.AddMessage($"Character {characterId} has been selected.", MessageType.PersonalSystem);
        NetworkTransmission.instance.SetMyCharacterServerRpc(characterId);

        // 모든 버튼의 UI 상태를 업데이트
        UpdateSelectionUI(characterId);
    }

    // 모든 버튼의 UI를 업데이트하는 메서드
    private void UpdateSelectionUI(int selectedCharacterId)
    {
        foreach (var buttonUI in characterButtons)
        {
            if (buttonUI.CharacterId == selectedCharacterId)
            {
                // 선택된 ID와 버튼의 ID가 같다면 "선택" 상태로 변경
                buttonUI.SetSelected();
            }
            else
            {
                // 다르다면 "선택 해제" 상태로 변경
                buttonUI.SetDeselected();
            }
        }
    }
}