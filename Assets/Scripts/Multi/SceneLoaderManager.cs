// --- START OF FILE SceneLoaderManager.cs ---

using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 시작 시 초기 씬을 로드하는 부트스트래퍼 클래스입니다.
/// </summary>
public class SceneLoaderManager : MonoBehaviour
{
    private void Start()
    {
        // [개선] NetworkManager가 초기화된 후 메인 메뉴 씬을 로드하도록 대기합니다.
        // 이는 의존성 문제를 방지하는 좋은 패턴입니다.
        StartCoroutine(LoadMainSceneAfterNetworkManagerInit());
    }

    private IEnumerator LoadMainSceneAfterNetworkManagerInit()
    {
        // NetworkManager.Singleton이 null이 아닐 때까지 대기합니다.
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        
        // 메인 메뉴 씬을 로드합니다.
        SceneManager.LoadScene("MainMenu");
    }
}