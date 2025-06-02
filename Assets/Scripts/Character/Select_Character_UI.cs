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
        characterImage.color = defaultColor;
        defaultScale = transform.localScale;
    }

    private void OnValidate()
    {
        if (character == null)
            return;
        
        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length > 1 && images[1] != null)
        {
            images[1].sprite = character.selectSprite;
        }

        gameObject.name = "Character - " + character.name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ���콺 ���� �� �⺻ ũ���� 1.1��� Ȯ��
        transform.localScale = defaultScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ���콺�� ������ ����� �׻� �⺻ ũ��� ����
        transform.localScale = defaultScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Character_Manager manager = Character_Manager.instance;

        if (manager == null) return;

        manager.currentCharacter = character;
        GetComponentInParent<Character_UI>().UpdateCharacter_SelectUI();
    }

    public void ResetSelection()
    {
        characterImage.color = defaultColor;
        transform.localScale = defaultScale;
    }

    public void UpdateUI()
    {
        Character_Manager manager = Character_Manager.instance;

        if (manager.currentCharacter == character)
        {
            characterImage.color = Color.green;
            GetComponentInParent<Character_UI>().Update_illustration(character.illustrationSprite);
        }
        else
            ResetSelection();
    }
}
