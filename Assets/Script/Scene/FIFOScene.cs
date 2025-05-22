using UnityEngine;

public class FIFOScene : MonoBehaviour
{
    public void OnclickButton()
    {
        LoadingManager.instance.LoadScene("Start_FIFO");
    }
}
