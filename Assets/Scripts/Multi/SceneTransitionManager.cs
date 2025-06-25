// --- START OF FILE SceneTransitionManager.cs ---

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 부드럽게 처리하는 싱글턴 클래스입니다. 로딩 화면을 표시할 수 있습니다.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI")]
    [SerializeField]
    private GameObject loadingCanvas; // 로딩 중 표시할 캔버스

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 지정된 이름의 씬으로 전환을 시작합니다.
    /// </summary>
    /// <param name="sceneName">로드할 씬의 이름</param>
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 로딩 캔버스 활성화
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(true);
        }

        // 비동기 씬 로드
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 로드가 완료될 때까지 대기
        while (!operation.isDone)
        {
            // 여기서 로딩 바(progress bar) 등을 업데이트 할 수 있습니다.
            // float progress = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }

        // 로드 완료 후 로딩 캔버스 비활성화
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
        }
    }
}