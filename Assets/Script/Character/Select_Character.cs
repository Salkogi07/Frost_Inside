using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Select_Character : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Character_Data character;
    private Image characterImage; // 캐릭터 이미지 저장
    private static Color defaultColor = Color.white; // 기본 색상
    private Vector3 defaultScale; // 기본 크기
    private bool isSelected = false; // 현재 선택 여부

    private void Awake()
    {
        characterImage = GetComponent<Image>();
        defaultColor = characterImage.color; // 초기 색상 저장
        defaultScale = transform.localScale; // 초기 크기 저장
    }

    private void OnValidate()
    {
        if (character == null)
            return;

        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length > 1 && images[1] != null)
        {
            images[1].sprite = character.selectImage;
        }

        gameObject.name = "Character - " + character.name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 오버 시 기본 크기의 1.1배로 확대
        transform.localScale = defaultScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 영역을 벗어나면 항상 기본 크기로 복귀
        transform.localScale = defaultScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Character_Manager.instance == null) return;

        // 이전 선택된 캐릭터의 선택 상태 초기화
        if (Character_Manager.instance.selectedCharacter != null)
        {
            Character_Manager.instance.selectedCharacter.ResetSelection();
        }

        // 현재 캐릭터 선택 (색상은 녹색으로 변경)
        isSelected = true;
        characterImage.color = Color.green;
        Character_Manager.instance.currentCharacter = character;
        Character_Manager.instance.selectedCharacter = this;

        // 클릭 시 크기 조정 코드는 제거하여, 마우스가 영역을 벗어나면 OnPointerExit에서 기본 크기로 복귀하게 함
    }

    public void ResetSelection()
    {
        isSelected = false;
        characterImage.color = defaultColor;
        transform.localScale = defaultScale; // 기본 크기로 복귀
    }
}
