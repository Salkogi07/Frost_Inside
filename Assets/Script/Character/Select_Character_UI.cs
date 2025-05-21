using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Select_Character_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Character_Data character;
    [SerializeField] private Color defaultColor = Color.white;

    private Image characterImage;
    private Vector3 defaultScale;

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

        Character_Manager.instance.currentCharacter = character;
        GetComponentInParent<Character_UI>().UpdateCharacter_SelectUI();
    }

    public void ResetSelection()
    {
        characterImage.color = defaultColor;
        transform.localScale = defaultScale;
    }

    public void UpdateUI()
    {
        if (Character_Manager.instance.currentCharacter == character)
            characterImage.color = Color.green;
        else
            ResetSelection();
    }
}
