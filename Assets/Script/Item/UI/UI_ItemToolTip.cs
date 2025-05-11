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
    private Vector2 originalSize; // ����� ���� ũ�� ����

    // �ּ� �ʺ�� �е� �� ����
    public float minWidth = 200f;
    public float horizontalPadding = 20f; // �¿� �е� �հ�

    // ���� ��ġ���� �󸶸�ŭ �������� ǥ������ ���� (��: ���������� 50�ȼ�)
    public Vector2 tooltipOffset = new Vector2(50f, 0f);

    // ������ ��ġ(slotPosition)�� �޾� �� ��ġ�� �������� �����Ͽ� ������ ǥ��
    public void ShowToolTip(InventoryItem item, Vector3 slotPosition)
    {
        if (item == null)
            return;

        // �ؽ�Ʈ ������Ʈ
        itemNameText.text = item?.data.itemName;
        itemTypeText.text = item?.data.itemType.ToString();
        itemCoinText.text = item.price.ToString();
        itemExplanation.text = item?.data.explanation;

        background = GetComponent<RectTransform>();
        originalSize = background.sizeDelta; // ���� �� ����� ũ�⸦ ����

        // �ؽ�Ʈ�� preferredWidth ������ ���� ���̾ƿ� ���� ����
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemNameText.rectTransform);

        // ������ �̸��� ���̿� ���� ��ǥ �ʺ� ��� (10�� �̻��� ��� preferredWidth ����)
        float targetWidth = minWidth;
        if (itemNameText.text.Length > 10)
        {
            targetWidth = Mathf.Max(minWidth, itemNameText.preferredWidth + horizontalPadding);
        }

        // ��� �ʺ� ���� (���̴� ���� ũ�⸦ ����)
        background.sizeDelta = new Vector2(targetWidth, originalSize.y);

        // ���� ��ġ�� ������ ���� (3D ��ǥ�̹Ƿ� z�� 0���� ����)
        transform.position = slotPosition + new Vector3(tooltipOffset.x, tooltipOffset.y, 0f);

        gameObject.SetActive(true);
    }

    public void HideToolTip()
    {
        // ������ ���� �� ��� ũ�⸦ ���� ���·� ����
        background.sizeDelta = originalSize;
        gameObject.SetActive(false);
    }
}
