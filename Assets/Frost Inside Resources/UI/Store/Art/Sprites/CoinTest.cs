using TMPro;
using UnityEngine;

public class CoinTest : MonoBehaviour
{
    private int coin = 500;
    public TextMeshProUGUI text;
    
    private void Start()
    {
        text.text = coin.ToString();
    }
    
    public void Buy(int price)
    {
        coin -= price;
        text.text = coin.ToString();
    }
}
