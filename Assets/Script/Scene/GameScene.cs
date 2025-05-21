using UnityEngine;

public class GameScene : MonoBehaviour
{
    public void OnclickButton()
    {
        LoadingManager.instance.LoadScene("GameScene");
    }
}
