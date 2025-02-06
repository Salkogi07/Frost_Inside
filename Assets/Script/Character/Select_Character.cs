using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Select_Character : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Character_Data character;
    private Image characterImage; // ĳ���� �̹��� ����
    private static Color defaultColor = Color.white; // �⺻ ����
    private Vector3 defaultScale; // �⺻ ũ��
    private bool isSelected = false; // ���� ���� ����

    private void Awake()
    {
        characterImage = GetComponent<Image>();
        defaultColor = characterImage.color; // �ʱ� ���� ����
        defaultScale = transform.localScale; // �ʱ� ũ�� ����
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
        if (Character_Manager.instance == null) return;

        // ���� ���õ� ĳ������ ���� ���� �ʱ�ȭ
        if (Character_Manager.instance.selectedCharacter != null)
        {
            Character_Manager.instance.selectedCharacter.ResetSelection();
        }

        // ���� ĳ���� ���� (������ ������� ����)
        isSelected = true;
        characterImage.color = Color.green;
        Character_Manager.instance.currentCharacter = character;
        Character_Manager.instance.selectedCharacter = this;

        // Ŭ�� �� ũ�� ���� �ڵ�� �����Ͽ�, ���콺�� ������ ����� OnPointerExit���� �⺻ ũ��� �����ϰ� ��
    }

    public void ResetSelection()
    {
        isSelected = false;
        characterImage.color = defaultColor;
        transform.localScale = defaultScale; // �⺻ ũ��� ����
    }
}
