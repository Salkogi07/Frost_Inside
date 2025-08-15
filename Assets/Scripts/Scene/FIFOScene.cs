using UnityEngine;

public class FIFOScene : MonoBehaviour
{
    public void OnclickButton()
    {
        TestLoadingManager.instance.LoadScene("Start_FIFO");
    }
}
