using UnityEngine;
using UnityEngine.EventSystems;

public class HoverThrowSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public bool isPointerOver = false;

    public void OnPointerEnter(PointerEventData eventData)
        => isPointerOver = true;  // ���콺�� ���� ���� �������� ��

    public void OnPointerExit(PointerEventData eventData)
        => isPointerOver = false; // ������ ����� ��
}
