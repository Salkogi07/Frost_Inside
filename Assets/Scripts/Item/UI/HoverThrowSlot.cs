using UnityEngine;
using UnityEngine.EventSystems;

public class HoverThrowSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public bool isPointerOver = false;

    public void OnPointerEnter(PointerEventData eventData)
        => isPointerOver = true;  // 마우스가 슬롯 위로 진입했을 때

    public void OnPointerExit(PointerEventData eventData)
        => isPointerOver = false; // 슬롯을 벗어났을 때
}
