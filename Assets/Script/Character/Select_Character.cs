using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Select_Character : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Character_Data character;

    private void OnValidate()
    {
        if (character == null)
            return;

        Image image = GetComponentInChildren<Image>();
        if (image != null)
        {
            image.sprite = character.selectImage;
        }

        gameObject.name = "Character - " + character.name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Character_Manager.instance != null)
        {
            Character_Manager.instance.currentCharacter = character;
        }
    }
}
