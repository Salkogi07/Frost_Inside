// LoadingManager.cs
// 역할: 씬 전환 시 애니메이션, 최소 로딩 시간, 네트워크 동기화 등 여러 조건을 সমন্ব합하여 관리합니다.

using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    [Header("Settings")]
    public float minLoadTime = 2.0f; // 최소 로딩 시간

    [Header("Objects")]
    [SerializeField] private GameObject loadingScreenObj;
    [SerializeField] private GameObject backGroundObj;
    [SerializeField] private Animator animator;

    private bool isLoading = false;

    // 클라이언트가 서버로부터 초기 데이터를 받았는지 확인하는 플래그
    public bool IsInitialDataReceived { get; set; } = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 씬을 로드합니다.
    /// </summary>
    /// <param name="sceneName">로드할 씬의 이름</param>
    /// <param name="activationCondition">씬을 활성화하기 위해 충족해야 하는 추가 조건. null이면 즉시 활성화됩니다.</param>
    public void LoadScene(string sceneName, Func<bool> activationCondition)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneCoroutine(sceneName, activationCondition));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, Func<bool> activationCondition)
    {
        isLoading = true;
        IsInitialDataReceived = false; // 새로운 로딩을 위해 플래그 초기화

        // 1. 페이드 아웃 애니메이션 시작 및 대기
        loadingScreenObj.SetActive(true);
        backGroundObj.SetActive(true);
        animator.Play("Shader_Out");
        yield return new WaitForSeconds(GetAnimationLength("Shader_Out")); // 애니메이션 길이만큼 대기

        // 2. 비동기 씬 로딩 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;

        // 3. 모든 조건이 충족될 때까지 대기
        while (true)
        {
            timer += Time.deltaTime;

            bool sceneReady = operation.progress >= 0.9f;
            bool minTimeElapsed = timer >= minLoadTime;
            bool customConditionMet = activationCondition?.Invoke() ?? true;

            if (sceneReady && minTimeElapsed && customConditionMet)
            {
                break; // 모든 조건 충족, 루프 탈출
            }

            yield return null;
        }

        // 4. 씬 활성화 및 페이드 인 애니메이션
        operation.allowSceneActivation = true;

        // 씬이 완전히 로드될 때까지 한 프레임 대기
        yield return new WaitForEndOfFrame();

        animator.Play("Shader_In");
        yield return new WaitForSeconds(GetAnimationLength("Shader_In"));

        loadingScreenObj.SetActive(false);
        isLoading = false;
    }

    // 애니메이터에서 특정 클립의 길이를 찾아 반환하는 헬퍼 함수
    private float GetAnimationLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 1f; // 기본값

        return animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == clipName)?.length ?? 1f;
    }
}