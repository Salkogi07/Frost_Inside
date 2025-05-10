using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemCoinText;
    [SerializeField] private TextMeshProUGUI itemExplanation;

    private RectTransform background;
    private Vector2 originalSize; // 배경의 원래 크기 저장

    // 최소 너비와 패딩 값 설정
    public float minWidth = 200f;
    public float horizontalPadding = 20f; // 좌우 패딩 합계

    // 슬롯 위치에서 얼마만큼 떨어져서 표시할지 설정 (예: 오른쪽으로 50픽셀)
    public Vector2 tooltipOffset = new Vector2(50f, 0f);

    // 슬롯의 위치(slotPosition)를 받아 그 위치에 오프셋을 적용하여 툴팁을 표시
    public void ShowToolTip(InventoryItem item, Vector3 slotPosition)
    {
        if (item == null)
            return;

        // 텍스트 업데이트
        itemNameText.text = item?.data.itemName;
        itemTypeText.text = item?.data.itemType.ToString();
        itemExplanation.text = item?.data.explanation;

        background = GetComponent<RectTransform>();
        originalSize = background.sizeDelta; // 시작 시 배경의 크기를 저장

        // 텍스트의 preferredWidth 갱신을 위해 레이아웃 강제 재계산
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemNameText.rectTransform);

        // 아이템 이름의 길이에 따라 목표 너비 계산 (10자 이상일 경우 preferredWidth 기준)
        float targetWidth = minWidth;
        if (itemNameText.text.Length > 10)
        {
            targetWidth = Mathf.Max(minWidth, itemNameText.preferredWidth + horizontalPadding);
        }

        // 배경 너비만 변경 (높이는 원래 크기를 유지)
        background.sizeDelta = new Vector2(targetWidth, originalSize.y);

        // 슬롯 위치에 오프셋 적용 (3D 좌표이므로 z는 0으로 고정)
        transform.position = slotPosition + new Vector3(tooltipOffset.x, tooltipOffset.y, 0f);

        gameObject.SetActive(true);
    }

    public void HideToolTip()
    {
        // 툴팁을 숨길 때 배경 크기를 원래 상태로 복원
        background.sizeDelta = originalSize;
        gameObject.SetActive(false);
    }
}
