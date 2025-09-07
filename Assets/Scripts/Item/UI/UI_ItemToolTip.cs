using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemToolTip : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemCoinText;
    [SerializeField] private TextMeshProUGUI itemWeightText;
    [SerializeField] private TextMeshProUGUI itemExplanation;
    
    private void Awake()
    {
        HideToolTip();
    }
    
    public void ShowToolTip(Inventory_Item item)
    {
        if (item.Data.itemId == -1)
            return;
        
        // 텍스트 업데이트
        itemImage.color = Color.white;
        itemImage.type = Image.Type.Simple;
        itemImage.preserveAspect = true;
        itemImage.sprite = item.Data.icon;
        itemNameText.text = item.Data.itemName;
        itemTypeText.text = item.Data.itemType.ToString();
        itemCoinText.text = item.price.ToString();
        itemWeightText.text = item.Data.itemWeight.ToString();
        itemExplanation.text = item.Data.explanation;
    }

    public void HideToolTip()
    {
        itemImage.color = new Color(1, 1, 1, 0);
        itemImage.sprite = null;
        itemNameText.text = "None";
        itemTypeText.text = "None";
        itemCoinText.text = "None";
        itemWeightText.text = "None";
        itemExplanation.text = "None";
    }
}
