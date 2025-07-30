using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SetUpLoader : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadMainSceneAfterNetworkManagerInit());
    }

    private IEnumerator LoadMainSceneAfterNetworkManagerInit()
    {
        // NetworkManager.Singleton이 null이 아닐 때까지 대기합니다.
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        
        // 메인 메뉴 씬을 로드합니다.
        SceneManager.LoadScene("Menu");
    }
}
