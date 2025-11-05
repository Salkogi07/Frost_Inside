using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestStoreCount : MonoBehaviour
{
    public int count;

    public TextMeshProUGUI text;

    public void Set()
    {
        count = 1;
        text.text = count.ToString();
    }

    public void CountAdd()
    {
        Debug.Log("CountAdd");
        if (count >= 4)
        {
            count = 4;
            return;
        }
        
        count++;
        text.text = count.ToString();
    }
    
    public void CountRemove()
    {
        Debug.Log("CountRemove");
        if (count <= 1)
        {
            count = 1;
            return;
        }
        count--;
        text.text = count.ToString();
    }
}
