// --- START OF FILE CharacterSelectButtonUI.cs ---

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private int characterId;
    
    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.cyan;

    private Image characterImage;
    private Vector3 defaultScale;
    
    public int CharacterId => characterId;

    private void Awake()
    {
        characterImage = GetComponent<Image>();
        defaultScale = transform.localScale;
        SetDeselected();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = defaultScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = defaultScale;
    }
    
    public void SetDeselected()
    {
        characterImage.color = defaultColor;
        transform.localScale = defaultScale;
    }
    
    public void SetSelected()
    {
        characterImage.color = selectedColor;
    }
}